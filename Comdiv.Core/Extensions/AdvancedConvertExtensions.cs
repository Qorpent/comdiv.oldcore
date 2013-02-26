// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;

namespace Comdiv.Extensions{
    public static class AdvancedConvertExtensions{
        public static int DefaultRegexTimeout = 2000;
        public static object safeSync = new object();
        public const string lSlash = "\\";
        public const string rSlash = "/";
        public static string toZip(this string text)
        {
            // Converts to byte array 
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            var ms = new MemoryStream();
            int length = bytes.Length; // the length of the text
            using (var zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                // Writes compressed bytes to the MemoryStream ojbect from the bytes
                zip.Write(bytes, 0, length);
            }
            var compressed = new byte[ms.Length];
            // Reads a block of bytes from the current stream and current position and writes the data to buffer (compressed)
            ms.Position = 0;
            ms.Read(compressed, 0, compressed.Length);

            // Saves the length of the compressed text
            byte[] buffer = new byte[compressed.Length + 4];
            Buffer.BlockCopy(compressed, 0, buffer, 4, compressed.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(length), 0, buffer, 0, 4);

            // Returns base64 string
            return Convert.ToBase64String(buffer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compressed"></param>
        /// <exception cref="System.OverflowException"/>
        /// <returns></returns>
        public static string unZip(this string compressed)
        {

            // Converts the specified String (compressed), which encodes binary data as base 64 digits, to an equivalent 8-bit unsigned integer array.
            byte[] bytes = Convert.FromBase64String(compressed);
            using (var ms = new MemoryStream())
            {
                // Gets the number of bytes of the results, decompressed stiring
                var decompressedlength = BitConverter.ToInt32(bytes, 0); // 4 bytes only
                ms.Write(bytes, 4, bytes.Length - 4);
                byte[] buffer = new byte[decompressedlength];
                // Reads decompressed bytes to the buffer 
                ms.Position = 0;
                using (var zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }
                return Encoding.UTF8.GetString(buffer);
            }
        }

        const string entityPrefix = "&#";
        private const string entitySuffix = ";";
        /// <summary>
        ///  одирует переданную строку в виде последовательности символов, где все 
        /// символы заменены сущност€ми Utf8 &amp;#CODE;
        /// </summary>
        public static string toHtmlUtf(this string str)
        {
            if (String.IsNullOrEmpty(str)) return "";
            var result = new StringBuilder();
            foreach (var c in str)
            {
				
//                var bytes = Encoding.UTF8.GetBytes(c.ToString());
//				Console.WriteLine( Encoding.UTF8.GetBytes(str).concat( ","));
//				int up =  bytes[0];
//				if(bytes.Length>1){
//					up = (bytes[1] << 8) + bytes[0];
//				}
//								
                result.Append(entityPrefix);
                result.Append((int)c);
                result.Append(entitySuffix);
            }
            return result.ToString();
        }

        /// <summary>
        /// ѕреобразует HTML в читабельный текст
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string toText(this string value,bool preserveimages=false)
        {
            if (value == null) return String.Empty;
            var result = value
                .replace(@"<(br|p|table|tr|li)[^>]*?>", Environment.NewLine)
                .replace(@"</td\s*>", "\t")
                .replace(@"<hr[^>]*?>", "\r\n----------\r\n")
                .replace(@"&([lr]a)?(quo)t?;", "\"")
                .replace(@"&apos;", "'")
                .replace(@"&ndash;", "-")
                .replace(@"&amp;", "&")
                .replace(@"&nbsp;", " ");
            if(preserveimages) {
                result = result.replace(@"(?ix)<img[\s\S]+?>", m =>
                                                                   {
                                                                       var src =
                                                                           m.Value.find(@"src=['""]([\s\S]+?)['""\s]", 1);
                                                                       return "---IMG:" + src + "--";
                                                                   });
            }
            result =result
                .replace(@"<[^>]*>",String.Empty)
                .replace(@"[\r\n]\s+", Environment.NewLine)
                .replace(@"\x20+", " ")
                .Trim()
                ;
            return result;
        }

        public static string toSqlDateString(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }

        public static string toSqlDateString(this string date)
        {
            return toSqlDateString(date.toDate());
        }

        public static string toXmlDateString(this DateTime date)
        {
            return date.ToString("yyyy-MM-ddThh:mm:ss");
        }

        public static string toXmlDateString(this string date)
        {
            return toXmlDateString(date.toDate());
        }

        public static string toInternetDateString(this DateTime date)
        {
            return date.ToString("R", CultureInfo.GetCultureInfo("En-us"));
        }

        public static string cleanHtml(this string value, bool preserveimages=false)
        {
            return value.toText(preserveimages).toHtml();
        }

        public static string toSqlString(this string value)
        {
            if (null == value) return String.Empty;
            return value.Replace("'", "''");
        }

        public const string LocalDomainName = "(local)";

        /// <summary>
        /// Ѕерет строку с полным именем пользовател€ и возвращает нормализованное им€ с учетом или без учета домена
        /// </summary>
        /// <param name="user"></param>
        /// <param name="useDomain"></param>
        /// <returns></returns>
        public static string toUserName(this IPrincipal user, bool useDomain)
        {
            return user.Identity.Name.toUserName(useDomain);
        }

        public static string toUserName(this string userName)
        {
            return toUserName(userName, true);
        }

        public static string toUserName(this string userName, bool useDomain)
        {
            if (userName.noContent()) return "";
            if (useDomain) return userName;
            if (userName.Contains(lSlash) || userName.Contains(rSlash)) return userName.Split('\\', '/')[1];
            return userName;
        }

        public static string toDomain(this string userName)
        {
            if (userName.noContent()) return "";
            if (userName.Contains(lSlash) || userName.Contains(rSlash))
                return userName.Split('\\', '/')[0];
            return LocalDomainName;
        }

        public static string toHtml(this string str)
        {
            return str
                .replace("&", "&amp;")
                .replace("<", "&lt;")
                .replace(">", "&gt;")
                .replace(@"[\r\n]{1,2}", m => "<br/>\r\n")
                .replace(@"\t", "<span>&nbsp;&nbsp;&nbsp;&nbsp;</span>")
                .replace(@"--IMG:([\s\S]+?)(--)", "<img src='$1' />")
                .replace(@"-{5,}","<hr/>\r\n")
                
                ;
        }
    }
}