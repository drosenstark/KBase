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

        const string DIRECTORY_SUFFIX = "snippets";

        public string FileExtension = ".txt"; // someday we'll make this changeable

        public ExternalSnippet(Snippet snippet)
        {
            this.snippet = snippet;
            Init();
        }

        private void Init() {
            string kbaseId = Universe.Instance.ModelGateway.GetHashCode().ToString().Substring(0, 3);
            string filename = MakeValidFileName(kbaseId + "_" + snippet.Id + "_" + snippet.Title + FileExtension);
            DirectoryInfo dir = new DirectoryInfo(Util.getFilenameInAppDir(DIRECTORY_SUFFIX));
            if (!dir.Exists) {
                Logger.Log("About to create directory " + dir.FullName);
                dir.Create();
            }
            filename = dir.FullName + Path.DirectorySeparatorChar + filename;
            Logger.Log("--------------------------> " + dir.FullName);
            info = new FileInfo(filename); 
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
                Watchers.Add(this);
            }
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
                writer.WriteLine("-----------------------------------------");
                writer.WriteLine("TheKBase: FILE IS NO LONGER BEING WATCHED");
                writer.WriteLine("TheKBase: This filename " + info.Name + " may be overwritten.");
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
