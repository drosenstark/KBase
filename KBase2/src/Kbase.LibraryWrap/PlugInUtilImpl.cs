using System;
using System.Collections.Generic;
using System.Text;
using Kbase2Api;
using Kbase.DetailPanel;

namespace Kbase.LibraryWrap 
{
    public class PlugInUtilImpl : PlugInUtil
    {
        public PlugInUtilImpl() {
            this.ErrorHandler = new PlugInErrorHandler();
        }

        public override string derelativizeLinkAccordingToKbaseDirectory(string path)
        {
            string retVal = path;
            // if it's a physical drive, we don't do this
            if (!path.Substring(1, 1).Equals(":"))
                retVal = new HyperlinkUtil(Universe.Instance.Path).Derelativize(path);
            return retVal;
        }
    }
}
