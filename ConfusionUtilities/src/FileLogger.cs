/*
This file is part of Confusion Utilities
Copyright (C) 2004-2007 Daniel Rosenstark
license@confusionists.com
*/
using System;
using System.Threading;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace ConfusionUtilities
{

    public class FileLogger : Logger
    {
        FileInfo file = null;
        internal static string BACKUP_FILENAME = "confusionists.log";
        /// <summary>
        /// Put this key in your appSettings sections of the assembly config file and
        /// the fileLogger will use it.
        /// </summary>
        public static string KEY_FOR_FILENAME = "LogFile";
        object threadMonitor = new object();

        public static string LoggerFilename = null;
        /// <summary>
        /// must be called before init
        /// </summary>
        /// <param name="name"></param>
        public static void SetFileName(string name)
        {
            //System.Configuration.ConfigurationManager.AppSettings.Set(KEY_FOR_FILENAME, name);
            // can't do this in Mono, so
            LoggerFilename = name;
        }

        public FileLogger() : base() { }

        /// <summary>
        /// Will try to open a log on the LogFile key found in the AppSettings (assemblyname.config)
        /// If that doesn't work, it will use the backup filename
        /// THEN it will try to write, and if it has problems it will throw an Exception
        /// thereby interrupting the Init process (started by the Logger.Log, usually)
        /// </summary>
        protected override void init(bool logging)
        {
            try
            {
                base.init(logging);
                if (!base.Logging)
                    return;
                string filename = System.Configuration.ConfigurationManager.AppSettings[KEY_FOR_FILENAME];
                if (filename == null || filename.Length == 0)
                {
                    filename = LoggerFilename;
                }
                if (filename == null || filename.Length == 0)
                {
                    filename = BACKUP_FILENAME;
                }
                if (filename == null || filename.Length == 0)
                    throw new NullReferenceException("FileLogger has a blank filename to write to.");
                filename = Util.getFilenameInAppDir(filename);
                Init2(filename);
            }
            catch (Exception ex) {
                ShutDown();
                throw ex;
            }
        }

        protected void Init2(string filename)
        {
            file = new FileInfo(filename);
            log("Testing Log Write", EventType.Information);
            Write(); // here we want the Write method to throw up its errors
            Debug.WriteLine("FileLogger: Sucessfully started logging to " + file.FullName);
        }

        protected override void Write()
        {
            lock (threadMonitor)
            {
                StreamWriter sw = null;
                try
                {
                    Debug.Assert(file != null);
                    if (logBuffer.Length == 0)
                        return;
                    sw = new StreamWriter(file.FullName, true);
                    string output = logBuffer.ToString();
                    Debug.WriteLine("[logging to " + file.FullName + "] " + output);
                    sw.Write(output);
                    sw.Flush();
                    sw.Close();
                    logBuffer.Remove(0, logBuffer.Length);

                }
                finally
                {
                    try
                    {
                        if (sw != null)
                            sw.Close();
                    }
                    catch (Exception ex)
                    {
                        // could not close the StringWriter, but we really have no way to handle this
                        // anymore. This should almost NEVER happen, because we did our testing when the app
                        // started. If it does, we'll never know unless we happen to be seeing the Debug
                        // messages
                        Debug.WriteLine(ex.Message);
                    }
                }

            }
        }

        public override string getInfo()
        {
            if (file == null)
                return "No log file";
            return base.getInfo() + " filename: " + file.FullName;
        }

        internal override string tail(int maxLines)
        {
            lock (threadMonitor)
            {
                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(file.FullName);
                    string theWholeFile = sr.ReadToEnd();
                    char[] separator = { '\n' };
                    string[] lines = theWholeFile.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    StringBuilder retVal = new StringBuilder();
                    int concatFrom = 0;
                    if (lines.Length > maxLines)
                        concatFrom = lines.Length - maxLines;
                    for (int i = concatFrom; i < lines.Length; i++)
                    {
                        retVal.AppendLine(lines[i]);
                    }

                    return retVal.ToString();
                }
                catch (FileNotFoundException) {
                    return "File is empty";
                }
                finally
                {
                    try
                    {
                        if (sr != null)
                            sr.Close();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }

        }

        internal override void wipe()
        {
            lock (threadMonitor)
            {
                try
                {
                    file.Delete();
                }
                catch (Exception e)
                {
                    log(e);
                }
            }
        }

    }

}
