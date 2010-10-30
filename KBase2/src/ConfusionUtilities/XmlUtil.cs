using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace ConfusionUtilities
{
    public class XmlUtil
    {

        public static void transform(string xslFilename, string xmlFilename, Stream output, bool isFragment)
        {
            try
            {
                xslFilename = System.AppDomain.CurrentDomain.BaseDirectory + "/" + xslFilename;
                xmlFilename = System.AppDomain.CurrentDomain.BaseDirectory + "/" + xmlFilename;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFilename);
                XmlDocument xslDoc = new XmlDocument();
                xslDoc.Load(xslFilename);

                XslCompiledTransform xsltransform = new XslCompiledTransform();
                XmlWriter writer = null;
                if (isFragment)
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.ConformanceLevel = ConformanceLevel.Fragment;
                    writer = XmlWriter.Create(output, settings);
                }
                else {
                    writer = XmlWriter.Create(output);
                }
                xsltransform.Load(xslDoc);
                xsltransform.Transform(xmlDoc, writer);
            }
            catch (Exception ex)
            {
                // we don't know if someone else has the html headers outputted or not, so we
                // don't include them here
                string theWholeMessage = "Error occurred<br/><pre>" + ex.ToString() + "</pre>";
                byte[] bytes = UTF8Encoding.UTF8.GetBytes(theWholeMessage.ToString());
                output.Write(bytes, 0, bytes.Length);

            }
        }
 
    }
}
