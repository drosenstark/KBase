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
using Kbase.Model;
using System.Windows.Forms;
using Kbase.MainFrm;
using Kbase.LibraryWrap;

namespace Kbase
{

    public class Startup
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] parameters)
        {
            try
            {
                Settings forceLoad = Universe.Instance.Settings;
                Logger.Init();


                Startup.parameters = parameters;

                MainForm form = new MainForm();

                if (!Universe.emergencyExit)
                {
                    Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
                    Logger.Log("Starting v. 0.0.1b");
                    Application.Run(form);
                }
                // Logger will be shut down in the exit events
            }
            catch (Exception ex)
            {
                MainForm.ShowError(new FatalErrorException("Error on startup.", ex));
            }
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MainForm.ShowErrorSilent(e.Exception);
        }

        public static string[] parameters = new string[0];

        public static void Init3()
        {

            try
            {

                Universe.Instance.InitializeModelGateway();
                if (parameters != null && parameters.Length > 0)
                {
                    string path = parameters[0];
                    Universe.Instance.Restore(path, false);
                }
                
                
                
                // if it is A client server version, then we have to force show the children
                // and grandchildren (otherwise we never load anything)
                if (Universe.Instance.Settings.ModelGatewayClassname.IndexOf("ModelClient") != -1)
                {
                    Snippet top = Universe.Instance.ModelGateway.TopLevelSnippet;
                    top.UI.ShowChildren();
                    top.UI.ShowGrandChildren();
                }
                if (Universe.Instance.Settings.AutoRefreshInSeconds > 0)
                    Universe.Instance.ModelGateway.StartRunawayThreads();
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }



        }
    
    
    }

}
