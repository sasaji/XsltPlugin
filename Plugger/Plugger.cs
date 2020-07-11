using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace MyTest
{
    public class Plugger
    {
        public void Plugin()
        {
            XmlDocument ruleXml = new XmlDocument();
            string xslText = @"<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"">";
            xslText += @"<xsl:template match=""yukari"">";
            xslText += @"<yukari xmlns:yukari1=""urn:yukari1"" xmlns:yukari2=""urn:yukari2"">";
            xslText += @"<test1><xsl:value-of select=""yukari1:SayGoodbye()""/></test1>";
            xslText += @"<test2><xsl:value-of select=""yukari2:SayHello()""/></test2>";
            xslText += @"</yukari>";
            xslText += @"</xsl:template>";
            xslText += @"</xsl:stylesheet>";
            ruleXml.LoadXml(xslText);
            XmlDocument sourceXml = new XmlDocument();
            string xmlText = @"<yukari></yukari>";
            sourceXml.LoadXml(xmlText);

            XslCompiledTransform xslt = new XslCompiledTransform();
            XsltSettings settings = new XsltSettings(false, true);
            settings.EnableDocumentFunction = true;
            xslt.Load(ruleXml, settings, new XmlUrlResolver());
            Console.WriteLine(ruleXml.OuterXml);

            XsltArgumentList args = new XsltArgumentList();
            args.AddExtensionObject("urn:yukari1", this);

            FileStream fs = new FileStream(@"..\..\..\PluginModule\bin\Debug\PluginModule.dll", FileMode.Open);
            byte[] rawAssembly = new byte[(int)fs.Length];
            fs.Read(rawAssembly, 0, rawAssembly.Length);
            fs.Close();
            System.Reflection.Assembly assembly = AppDomain.CurrentDomain.Load(rawAssembly);
            Type t = assembly.GetType("MyTest.ThePlugin");
            object trans = (object)Activator.CreateInstance(t);
            args.AddExtensionObject("urn:yukari2", trans);

            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            xslt.Transform(sourceXml, args, writer);
            byte[] chars = stream.ToArray();
            string result = Encoding.UTF8.GetString(chars);
            XmlDocument targetXml = new XmlDocument();
            targetXml.InnerXml = result;
            Console.WriteLine(targetXml.OuterXml);
        }

        public string SayGoodbye()
        {
            return "GOODBYE !!";
        }
    }
}
