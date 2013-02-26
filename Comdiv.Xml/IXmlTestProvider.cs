using System;


namespace Comdiv.Xml{
    [Obsolete]
    /// <summary>
        /// Summary description for IXmlTestProvider.
        /// </summary>
    public interface IXmlTestProvider{
        IXmlTest GetXmlTest(params object[] advancedParameters);
        IXmlTest GetXmlTest(string data, XmlTestDepth testDepth, params object[] advancedParameters);
    }
}