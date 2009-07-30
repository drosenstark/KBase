using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ConfusionUtilities
{
    /// <summary>
    /// copied the idea from http://weblogs.asp.net/rmclaws/archive/2003/10/19/32534.aspx
    /// Really we don't change the encoding, just how the encoding is displayed
    /// but it's enough for the XmlDocument.save method
    /// </summary>
    public class StringWriterUtf8 : StringWriter
    {
        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }
    }
}
