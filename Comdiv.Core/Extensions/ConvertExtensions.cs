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
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;
using Comdiv.Application;
using Comdiv.Persistence;

namespace Comdiv.Extensions{
    ///<summary>
    ///</summary>
    [DebuggerStepThrough]
    public static class ConvertExtensions{
        private static readonly string[] dateformats = new[]{
                                                                "dd.MM.yyyy HH:mm", "dd.MM.yyyy", "yyyy-MM-dd HH:mm",
                                                                "yyyy-MM-dd", "yyyyMMddHHmm", "yyyyMMddHHmmss"
                                                            };

        public static string toStr(this object x){
            return x == null ? String.Empty : x.ToString();
        }

        public static bool isId(this object x){
            if(x==null) return false;
            if(x.toStr().noContent()) return false;
            if(x.Equals(0)) return false;
            return true;
        }

        public static bool between<TValue>(this TValue testValue, TValue minValue, TValue maxValue)
           where TValue : struct
        {
            decimal it = testValue.toDecimal();
            decimal min = minValue.toDecimal();
            decimal max = maxValue.toDecimal();
            return it >= min && it <= max;
        }

        public static DateTime toDate(this object x){
            bool resolved;
            return toDate(x, out resolved);
        }

        public static DateTime toDate(this object x, out bool resolved){
            var res = DateExtensions.Begin;
            resolved = true;
            if (null == x){
                return res;
            }
            if (x is DateTime){
                return (DateTime) x;
            }
            if (x is string) {
            	var s = (string) x;
				
                res = ((string) x).parseDateTime(out resolved);
				if(!resolved) {
					if (s.ToLower().Contains("cегодня"))
					{
						res = DateTime.Today;
						resolved = true;
					}
					else if (s.ToLower().Contains("вчера"))
					{
						res = DateTime.Today.AddDays(-1); 
						resolved = true;
					}
				}
                return res;
            }
            if (x is ValueType){
                return DateExtensions.Begin.AddDays(x.toInt());
            }
            return res;
        }

        public static bool toBool(this object x){
            if (null == x){
                return false;
            }
            if (x is bool){
                return (bool) x;
            }
            if (x is string){
                return ((string) x).ToUpper().isIn("TRUE", "1", "T", "И", "ДА", "Y", "YES", "ИСТИНА");
            }
            return Convert.ToBoolean(x);
        }

        public static T to<T>(this object x, string system = null)
        {
            return to<T>(x, false,system);
        }

        public static T to<T>(this object x, bool safe, string system = null)
        {
            return to(x, safe, default(T),system);
        }

        public static T to<T>(this object x, bool safe, T def, string system = null)
        {
            return (T) x.to(typeof (T), safe, def,system);
        }

        public static object to(this object x, Type type, string system = null)
        {
            return to(x, type, false,system);
        }

        public static object to(this object x, Type type, bool safe, string system = null)
        {
            return to(x, type, safe, null,system);
        }

        public static object to(this object x, Type type, bool safe, object def,string system=null) {
            system = system.hasContent() ? system : "Default";
            if (null == x){
                if (def != null){
                    return def;
                }
                return type.IsValueType ? type.create() : null;
            }
            if (type.IsAssignableFrom(x.GetType())){
                return x;
            }
            if (x is XElement){
                return to((XElement) x, type, null);
            }
            if(type.IsEnum) {
                if(x is int) {
                    var name = Enum.GetName(type, x);
                    return Enum.Parse(type, name);
                }else if(x is string) {
                    return Enum.Parse(type, x as string,true);
                }
            }
            try{
                if (type == typeof (string)){
                    return x.toStr();
                }
                if (type == typeof (int)){
                    return x.toInt();
                }
                if (type == typeof (bool)){
                    return x.toBool();
                }
                if (type == typeof (decimal)){
                    return x.toDecimal();
                }
                if (type == typeof (DateTime)){
                    return x.toDate();
                }
                if (type.IsEnum){
                    if (x is ValueType){
                        var val = x.toInt();
                        return Enum.ToObject(type, val);
                    }
                    return Enum.Parse(type, x.toStr());
                }
                if( ((x is int) || (x is string)) && (type.IsInterface || type.IsClass) ){
                    return myapp.storage.Get(type,system,true).Load(type,x);
                }

                return Convert.ChangeType(x, type);
            }
            catch (Exception ex){
                if (safe){
                    return def;
                }
                else{
                    throw new FormatException("Cannot convert {0} of type {1} to {2}"._format("x", x.GetType(), type),
                                              ex);
                }
            }
        }

        public static T to<T>(this XElement element){
            return to<T>(element, null);
        }

        public static T to<T>(this XElement element, string attributeName){
            return (T) element.to(typeof (T), attributeName);
        }

        public static object to(this XElement element, Type t, string attributeName){
            if (attributeName.no()){
                return element.Value.to(t);
            }
            return element.Attribute(attributeName).Value.to(t);
        }

        private static DateTime parseDateTime(this string date, out bool resolved){
            resolved = false;
            if (date.noContent()){
                return DateExtensions.Begin;
            }
            date = date.Trim();


            var d = DateTime.Now;
            if (date.ToLower() == "сегодня") {
                resolved = true;
                return DateTime.Today;
            }
            if (date == "*"){
                resolved = true;
                return d;
            }
            var res = DateTime.TryParse(date, CultureInfo.GetCultureInfo("Ru-ru").DateTimeFormat,
                                        DateTimeStyles.AllowWhiteSpaces,
                                        out d);
            if (!res){
                res = DateTime.TryParseExact(date, dateformats, CultureInfo.InvariantCulture,
                                             DateTimeStyles.AllowWhiteSpaces, out d);
            }

            resolved = res;
            if (d.Year == 8000) {
                d = new DateTime(DateTime.Today.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);
            }
            if (d.Year < (DateExtensions.Begin.Year-200)) d = DateExtensions.Begin;
            if (d > DateExtensions.End) d = DateExtensions.End;
            return d;
        }
        public static int toIntSafe(this object x)
        {
            try
            {
                return toInt(x);
            }
            catch
            {
                return 0;
            }
        }
        public static int toInt(this object x){
            if (null == x){
                return 0;
            }
            if (Equals(String.Empty, x)){
                return 0;
            }
            if (x is int){
                return (int) x;
            }
            if (x is decimal || x is double || x is Single){
                return Convert.ToInt32( x);
            }
            return Convert.ToInt32(x.toDecimal(true));
        }

        public static decimal toDecimal(this object x){
            if (x is DBNull) return 0;
            return toDecimal(x, false);
        }

        public static decimal toDecimal(this object x, bool safe){
            try{
                if (null == x){
                    return 0;
                }
                if (Equals(String.Empty, x)){
                    return 0;
                }
                if (x is decimal){
                    return (decimal) x;
                }
                if (x is string){
                    if("-"==x || "--" == x || ""==x || "error"==x) {
                        return 0;
                    }
                    string x_ = ((string) x).replace("\\s+", String.Empty).Replace(",", ".");
                    try{
                        return Decimal.Parse(x_, NumberFormatInfo.InvariantInfo);
                    }
                    catch (FormatException){
                        throw new Exception("format of '" + x_ + "' cannot be parsed as decimal");
                    }
                }
                return Convert.ToDecimal(x);
            }
            catch{
                if (safe){
                    return 0;
                }
                throw;
            }
        }
    }
}