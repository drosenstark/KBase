using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ConfusionUtilities;
using System.Timers;
using Kbase.Model;
using System.Text.RegularExpressions;

namespace Kbase.DetailPanel
{
    public class ExternalSnippet
    {

        private FileInfo info;
        private DateTime lastWriteTime;
        Timer timer;
        Snippet snippet;

        public ExternalSnippet(Snippet snippet)
        {
            this.snippet = snippet;
            string filename = MakeValidFileName(snippet.Id + "_" + snippet.Title);
            info = new FileInfo(Util.getFilenameInAppDir("/temp/" + filename + ".txt")); // TODO: we have to clean this to avoid kbase viruses!
        } 

        public void StartWatching() {
            using (StreamWriter writer = new StreamWriter(info.FullName)) {
                writer.Write(snippet.TextReadOnce);
                writer.Close();
                lastWriteTime = info.LastWriteTime;
                Util.ExecuteCommand(info.FullName);
                timer = new Timer();
                timer.Interval = 100;
                timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                timer.Start();
            }
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            info.Refresh();
            if (info.LastWriteTime.CompareTo(lastWriteTime) != 0)
            {
                using (StreamReader reader = new StreamReader(info.FullName)) {
                    snippet.Text = reader.ReadToEnd();
                    lastWriteTime = info.LastWriteTime;
                }
            }
        }

        public void ShutDown()
        {
            timer.Stop();
            using (StreamWriter writer = new StreamWriter(info.FullName))
            {
                writer.Write("THIS FILE IS NO LONGER BEING WATCHED");
                writer.Close();
                this.snippet = null;
                info = null;
                timer = null;
            }
        }

        private static string MakeValidFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars())+" ");
            string invalidReStr = string.Format(@"[{0}]", invalidChars);
            return Regex.Replace(name, invalidReStr, "_");
        }

    }



}
