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
using ConfusionUtilities;
using System.Timers;
using Kbase.Model;
using System.Text.RegularExpressions;
using Kbase.MainFrm;

namespace Kbase.DetailPanel
{
    public class ExternalSnippet
    {

        private FileInfo info;
        private DateTime lastWriteTime;
        System.Timers.Timer timer;
        Snippet snippet;

        const string DIRECTORY_SUFFIX = "snippets";

        public string FileExtension = "txt"; // in the future could be changeable via settings file

        public ExternalSnippet(Snippet snippet)
        {
            this.snippet = snippet;
            Init();
        }

        private void Init() {
            string filename = MakeValidFileName(snippet.Title.ToLower().Trim() + "_" + snippet.Id + "." + FileExtension);
			
			DirectoryInfo dir = null;
			
			if (Universe.Instance.Settings.externalEditLocation == Settings.DEFAULT_EXTERNAL_EDIT_LOCATION) 
	            dir = new DirectoryInfo(Util.getFilenameInAppDir(DIRECTORY_SUFFIX));
			else
				dir = new DirectoryInfo(Universe.Instance.Settings.externalEditLocation);
			
            if (!dir.Exists) {
                Logger.Log("About to create directory " + dir.FullName);
                dir.Create();
            }
            filename = dir.FullName + Path.DirectorySeparatorChar + filename;
			// order the OS to open the file
            info = new FileInfo(filename); 
        }

        public void popOpen() { 
            Util.ExecuteCommand(info.FullName);
        }

        public void StartWatching() {
			bool overwriteFile = true;
			if (info.Exists) {
                string message = "External file " + info.Name + " already exists. Overwrite it?";
				overwriteFile = Universe.Instance.mainForm.ShowDialogYesNo(message);
			}
			if (overwriteFile) {
	            using (StreamWriter writer = new StreamWriter(info.FullName)) {
	                writer.Write(snippet.TextReadOnce);
	                writer.Close();
				}
			}
            lastWriteTime = info.LastWriteTime;
            popOpen();
            timer = new Timer();
            timer.Interval = 100;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Start();
            Watchers.Add(this);
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            info.Refresh();
            if (info.LastWriteTime.CompareTo(lastWriteTime) != 0)
            {
                using (StreamReader reader = new StreamReader(info.FullName)) {
                    snippet.Text = reader.ReadToEnd();
                    if (Universe.Instance.snippetDetailPane.IsEditingSnippet(snippet)) {
                        Universe.Instance.detailPane.UpdateTextFromExternal();
                    }
                    lastWriteTime = info.LastWriteTime;
                }
            }
        }

        public void ShutDown()
        {
            timer.Stop();
            using (StreamWriter writer = new StreamWriter(info.FullName, true))
            {
                writer.WriteLine();
                writer.WriteLine("-----------------------------------------");
                writer.WriteLine("TheKBase: FILE IS NO LONGER BEING WATCHED");
                writer.Close();
                this.snippet = null;
                info = null;
                timer = null;
                Watchers.Remove(this);
            }
        }

        private static string MakeValidFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars())+" ");
            string invalidReStr = string.Format(@"[{0}]", invalidChars);
            return Regex.Replace(name, invalidReStr, "_");
        }

        public static List<ExternalSnippet> Watchers = new List<ExternalSnippet>();

        public static void DropWatchers() {
            // clone to avoid concurrency modification errors
            ExternalSnippet[] watchersClone = new ExternalSnippet[Watchers.Count];
            Watchers.CopyTo(watchersClone);

            foreach (ExternalSnippet watcher in watchersClone) {
                watcher.ShutDown(); 
            }
        }

    }



}
