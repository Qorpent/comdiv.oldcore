using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Comdiv.Extensions;


namespace Comdiv.Xml{
    [Obsolete]
    /// <summary>
        /// Summary description for XmlTestBase.
        /// </summary>
    public class XmlTestBase : IXmlTest, IXmlTestProvider{
        #region IXmlTest Members
        private static bool Is(Enum target, Enum test) {
            if (!test.GetType().Equals(target.GetType()))
                throw new ArgumentException("{0}!={1}"._format(target.GetType(), test.GetType()));
            return (target.toInt() & test.toInt()) != 0;
        }

        public bool IsXml(string data, XmlTestDepth testDepth){
            if (Is(testDepth,XmlTestDepth.FullValidation)) return TryReadAsXml(data);

            var startsWithElement = Regex.IsMatch(data, @"^(<\?xml[\s\S]+?\?>)?\s*<\w+[\s\S]+?/?>",
                                                  RegexOptions.Compiled);
            var endsWithElement = Regex.IsMatch(data, @"<\w+[\s\S]*?/?>\s*$", RegexOptions.Compiled);
            return startsWithElement && endsWithElement;
        }

        #endregion

        #region IXmlTestProvider Members

        public IXmlTest GetXmlTest(string data, XmlTestDepth testDepth, params object[] advancedParameters){
            return this;
        }

        IXmlTest IXmlTestProvider.GetXmlTest(params object[] advancedParameters){
            return this;
        }

        #endregion

        public bool TryReadAsXml(string data){
            var reader = XmlReader.Create(new StringReader(data));

            try{
                while (reader.Read()){}
            }
            catch (XmlException){
                return false;
            }

            return true;
        }

        public static bool GetIsXml(string data, XmlTestDepth depth){
            return new XmlTestBase().IsXml(data, depth);
        }
    }
}