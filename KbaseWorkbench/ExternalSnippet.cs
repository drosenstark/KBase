using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ConfusionUtilities;
using System.Timers;

namespace KbaseWorkbench
{
    class ExternalSnippet
    {

        private FileInfo info;
        private DateTime lastWriteTime;
        Timer timer;

        public ExternalSnippet(string name, string text) { 
            info = new FileInfo(name);
            StreamWriter writer = new StreamWriter(info.FullName);
            writer.WriteLine(text);
            writer.Close();
            lastWriteTime = info.LastWriteTime;
            Util.ExecuteCommand(info.FullName);
            timer = new Timer();
            timer.Interval = 100;
            timer.Elapsed +=new ElapsedEventHandler(timer_Elapsed); 
            timer.Start();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            info.Refresh();
            if (info.LastWriteTime.CompareTo(lastWriteTime) != 0)
            {
                Logger.Log("yes we have a change");
                lastWriteTime = info.LastWriteTime;
            }
        }

        public void shutDown() {
            timer.Stop();
        }
    }



}
