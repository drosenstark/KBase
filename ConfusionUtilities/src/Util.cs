using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace ConfusionUtilities
{
    public class Util
    {

        /// <summary>
        /// Generates a random string with the given length
        /// </summary>
        /// <param name="size">Size of the string</param>
        /// <returns>Random string</returns>
        public static string GetRandom(int size)
        {
            StringBuilder retVal = new StringBuilder();
            Random random = new Random();
            char letterOrNumber;
            for (int i = 0; i < size; i++)
            {
                int nextRandomFullRange = Convert.ToInt32(Math.Floor(36 * random.NextDouble()));
                if (nextRandomFullRange > 25)
                    letterOrNumber = Convert.ToChar(nextRandomFullRange - 26 + 48); // numbers
                else
                    letterOrNumber = Convert.ToChar(nextRandomFullRange + 97); // numbers

                retVal.Append(letterOrNumber);
            }
            return retVal.ToString();
        }

        public static string GetTimestamp()
        {
            System.DateTime date = System.DateTime.Now;
            return date.ToString("s").Replace('T', ' ');
        }

        /// <summary>
        /// Use pagename with no leading slash, like whatever.aspx. 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pagename">http://host:port/app/pagename</param>
        /// <returns></returns>
        public static string getUrlForPage(HttpRequest request, string pagename) {
            string retVal = "http://" + request.Url.Host + ":" + request.Url.Port + request.ApplicationPath + "/" + pagename;
            return retVal;
        }

        /// <summary>
        /// use filename without leading slash nor anything else
        // we would write to this base ANYWAY, but we have to specify it because
        // otherwise we get a security error
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>c:\inetpub\wherever\filename</returns>
        public static string getFilenameInAppDir(string filename) {
            string baseDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            if (!filename.StartsWith(Path.DirectorySeparatorChar.ToString()))
                filename = Path.DirectorySeparatorChar + filename;
            
            string retVal = baseDir + filename;
            return retVal;
        }

        public static void ExecuteCommand(string cmd, string paramList, string command)
        {
            ProcessStartInfo processInfo;
            paramList = paramList + " " + command;
            processInfo = new ProcessStartInfo(cmd, paramList);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            Process.Start(processInfo);
        }

        public static void ExecuteCommand(string command)
        {
            string os = Environment.OSVersion.Platform.ToString();
            if (os.StartsWith("Unix"))
                ExecuteCommand("open", "", command);
            else if (os.StartsWith("Win"))
                ExecuteCommand("cmd.exe", "/C",command);
        }

        private static bool richTextBoxBroken;
        private static bool ranRichTextBoxTest = false;

        /// <summary>
        /// This is a Mono thing, the RichTextBox is crap on Mono
        /// </summary>
        /// <returns></returns>
        public static bool IsRichTextBoxBroken()
        {
            if (!ranRichTextBoxTest)
            {
                RichTextBox rtf = new RichTextBox();
                rtf.Text = "what";
                string rtfBefore = rtf.Rtf;
                rtf.SelectAll();
                rtf.SelectionIndent = 20;
                string rtfAfter = rtf.Rtf;
                if (rtfAfter == rtfBefore)
                    richTextBoxBroken = true;
                else
                    richTextBoxBroken = false;
                ranRichTextBoxTest = true;
            }
            return richTextBoxBroken;
        }

        public static bool IsMono() {
            Type t = Type.GetType("Mono.Runtime");
            return (t != null);
        }


    }
}
