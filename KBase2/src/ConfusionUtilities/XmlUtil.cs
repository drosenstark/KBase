/*
This file is part of TheKBase Desktop
A Multi-Hierarchical  Information Manager
Copyright (C) 2004-2010 Daniel Rosenstark

TheKBase Desktop is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
("GPL") version 2 as published by the Free Software Foundation.
See the file LICENSE.TXT for the full text of the GNU GPL, or see
http://www.gnu.org/licenses/gpl.txt

For using TheKBase Desktop with software that can not be combined with 
the GNU GPL or any other queries, please contact Daniel Rosenstark 
(license@thekbase.com).
*/
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
