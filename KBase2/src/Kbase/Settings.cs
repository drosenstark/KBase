/*
This file is part of TheKBase Desktop
A Multi-Hierarchical  Information Manager
Copyright (C) 2004-2007 Daniel Rosenstark

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
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Xml.Serialization;
using Kbase.MainFrm;
using Kbase.LibraryWrap;


namespace Kbase
{
	/// <summary>
	/// Loads and saves window settings to the current Universe.Instance.
	/// </summary>
	public class Settings
	{
		public FormWindowState mainFormWindowState;
		public System.Drawing.Size mainFormSize;
		// this one doesn't work (???)
		public Point mainFormLocation;
		public int mainFormSplitterSplitPosition;
		public int  mainFormSplitter2SplitPosition;
		public System.Drawing.Size mainFormSnippetPaneSize;
		public System.Drawing.Size mainFormDetailPaneSize;
        public string username = "";
        public string password = "";

        public string[] recentFiles;

        [XmlIgnore()]
        public string DEFAULT_TEXT = "New Snippet";

        [XmlIgnore()]
        public int MaxRowsPerServerHit= 100;
        
        [XmlIgnore()]
        string modelGatewayClassname = "Kbase.ModelInMemory.SnippetDictionary";
        [System.Xml.Serialization.XmlIgnore()]
        public string clientServerGatewayClassname = "Kbase.ModelClientServer.ClientServerGateway";


        public bool testing = false;
        public bool tracing = false;
        public int AutoRefreshInSeconds = 10; // AutoRefresh of zero is off
        public string SearchIcon = "Book Red";
        public string DefaultIcon = "Book Blue";
        public bool DoNotAskUserToConfirmSelectionChanceInLocationPane = false;

        bool restoreWindows = false;

		public Settings()
		{
		}

        [XmlIgnore()]
        public string ModelGatewayClassname {
            get {
                return modelGatewayClassname;
            }
        }

        
        
        static string Path {
			get 
			{
				return System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "/kbase.config";
			}
		}

		public void SaveSilent() 
		{
			try 
			{
				Save();					
			} 
			catch (Exception e) {
				MainForm.ShowErrorSilent(e);
			}
		}
		
		public void Save() 
		{
			BuildSettings();
			XmlSerializer ser = new XmlSerializer(typeof(Settings));
			System.IO.TextWriter writer = null;
			try 
			{
                Logger.Log("Saving Settings to " + Settings.Path);
                DateTime marker = DateTime.Now;
                writer = new System.IO.StreamWriter(Path);
				ser.Serialize(writer, this);
                Logger.LogTimer(marker, "Saving Settings");
            } 
			finally 
			{
				if (writer != null)
					writer.Close();
			}
		}

		/// <summary>
		/// constructs the state from the various windows in the universe
		/// </summary>
		private void BuildSettings() 
		{
			Universe universe = Universe.Instance;
			mainFormWindowState = universe.mainForm.WindowState;
			// you have to unmaximize the pane or you can't save those settings
			// and it must be visible
			universe.mainForm.WindowState = FormWindowState.Normal;
			mainFormSize = universe.mainForm.Size;
			mainFormLocation = universe.mainForm.Location;
			mainFormSplitterSplitPosition = universe.mainForm.splitter.SplitPosition;
			mainFormSplitter2SplitPosition = universe.mainForm.splitter2.SplitPosition;
			mainFormSnippetPaneSize = Universe.Instance.snippetPane.Size;
            mainFormDetailPaneSize = Universe.Instance.detailPane.Size;
            recentFiles = Universe.Instance.mainForm.RecentFiles;
		}

		public static Settings RestoreSilent() 
{
			try 
			{
				return Restore();					
			} 
			catch (Exception e) 
			{
				MainForm.ShowErrorSilent(e);
                return new Settings();
			}
		}

		
		public static Settings Restore() 
		{
			if (!new System.IO.FileInfo(Path).Exists)
				return new Settings();
			System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(Settings));
			System.IO.TextReader reader = null;
			try 
			{
				reader = new System.IO.StreamReader(Path);
				Settings settings = (Settings)ser.Deserialize(reader);
                settings.restoreWindows = true;
                return settings;
			} 
			finally  
			{
				if (reader != null)
					reader.Close();
			}
					
		}

		/// <summary>
		/// configures the windows from the settings objects
		/// </summary>
		public void RestoreWindows() {
            if (!restoreWindows)
                return;
			Universe universe = Universe.Instance;
            Debug.Assert(universe.mainForm != null, "MainForm must be set in the Universe before restoring the Settings");
			universe.mainForm.SuspendLayout();
			universe.mainForm.WindowState = mainFormWindowState;
			universe.mainForm.Size = mainFormSize;
			universe.mainForm.Location = mainFormLocation;
			universe.mainForm.splitter.SplitPosition= mainFormSplitterSplitPosition;
			universe.mainForm.splitter2.SplitPosition = mainFormSplitter2SplitPosition;
            Universe.Instance.snippetPane.Size = mainFormSnippetPaneSize;
            Universe.Instance.detailPane.Size = mainFormDetailPaneSize;
            Universe.Instance.mainForm.RecentFiles = recentFiles;

			universe.mainForm.ResumeLayout();
		}
	}

}
