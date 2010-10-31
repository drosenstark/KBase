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
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using Kbase2Api;

namespace Kbase.LibraryWrap
{
    public class PlugInManager
    {
        static Kbase2Api.PlugInUtil util;

        public static void Init()
        {
            PlugInManager.util = new PlugInUtilImpl();
        }

        public static List<MenuItem> getPluginItems()
        {
            Form owner = Universe.Instance.mainForm;
            TextBoxBase textPane = Universe.Instance.detailPane;

            List<MenuItem> retVal = new List<MenuItem>();
            if (util.ErrorHandler == null)
                throw new Exception("Trying to get plugins but I have no error handler.");
            // get the dlls
            string[] dlls = getDllsFromAppBase();
            foreach (string dllName in dlls)
            {
                try
                {
                    util.ErrorHandler.ShowErrorSilent("loading from " + dllName);
                    Assembly ass = Assembly.LoadFrom(dllName);
                    Type[] types = ass.GetTypes();
                    foreach (Type type in types)
                    {
                        if (type.IsSubclassOf(typeof(TextPanePlugin)))
                        {
                            util.ErrorHandler.ShowErrorSilent("Instantiating " + type.FullName);
                            //ClassType = Ass.GetType(ClassName, True)
                            TextPanePlugin plugin = ass.CreateInstance(type.FullName) as TextPanePlugin;
                            plugin.Init(owner, textPane, util);
                            retVal.AddRange(plugin.getMenuItemsForToolsMenu());
                        }
                    }
                }
                catch (Exception e2)
                {
                    MainFrm.MainForm.ShowError(e2, "Error loading DLL " + dllName);
                }

            }


            return retVal; 
        }

        public static string[] getDllsFromAppBase()
        {
            List<string> retVal = new List<string>();
            // figure out the plugins directory
            string pluginsDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "/plugins/";
            DirectoryInfo directory = new DirectoryInfo(pluginsDir);
            if (!directory.Exists)
            {
                util.ErrorHandler.ShowErrorSilent("Plugin directory " + directory.FullName + " does not exist, no plugins will load.");
                return new string[0];
            }
            foreach (FileInfo file in directory.GetFiles("*.dll"))
            {
                retVal.Add(file.FullName);
            }

            return retVal.ToArray();

        }

    }
}
