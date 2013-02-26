using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Comdiv.Extensions;
using NUnit.Framework;

namespace Comdiv.Booxml.Test
{
    public class BooxmlBaseTest
    {

       
        public BooxmlParser boox;
        
        public XElement xml;
        public BooxmlGenerator boog;

        [SetUp]
        public virtual void setup(){
            Console.WriteLine();
            Console.WriteLine("----");
            this.boox = new BooxmlParser();
            this.boog = new BooxmlGenerator();
            
        }
        public XElement test(string input){
            this.xml = boox.Parse(input);
            return this.xml;
        }

        public string gen(string xml){
            return boog.Generate(XElement.Parse(xml)).Trim();
        }

        public string test_(string input){
            test(input);
            return simply();
        }
        
        public string simply(){
            XmlWriterSettings sets = new XmlWriterSettings();
            sets.Indent = false;
            sets.OmitXmlDeclaration = true;
            var sb = new StringBuilder();
            var w = XmlWriter.Create(new StringWriter(sb), sets);
            xml.WriteTo(w);
            w.Flush();
            w.Close();
            var result = sb.ToString().replace(@"(\w+=)""([^""]+)""", "$1'$2'").replace(@"\s((_line)|(_file))=[""'][\w\d]+[""']","");
            return result;
        }

    }
}
