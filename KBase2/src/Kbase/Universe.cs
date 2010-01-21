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
using System.Drawing;
using System.Collections.Generic;
using System.Timers;
using System.Windows.Forms;
using System.Diagnostics;
using Kbase.Serialization;
using Kbase.SnippetTreeView;
using Kbase.Model;
using System.Reflection;
using Kbase.MainFrm;
using Kbase.LibraryWrap;

namespace Kbase 
{
	/// <summary>
	/// The application is not MDI but just in case we want to do that in the future,
    /// we center everything in non-static fields in a Universe instance.
    /// You could have several instances in memory for an MDI application.
	/// </summary>
	internal class Universe 
	{
	

		public string SnippetBeingViewedTitle = null;
		public static Universe Instance = new Universe();
        public static bool emergencyExit = false;


		private Universe()
		{
		}

        Settings settings = null;

        public Settings Settings {
            get { 
                if (settings == null)
                    settings = Settings.RestoreSilent();
                return settings;
            }
        }

		public Kbase.DetailPanel.DetailPane detailPane;
		public MainForm mainForm;
		public SnippetPane snippetPane;
		public TestForm testForm;
        public Properties.SnippetDetailPane snippetDetailPane;
        public Properties.PropertiesPaneHolder propertiesPaneHolder;

        public Encryption encryption = new Encryption();

		Kbase.Model.ModelGateway modelGateway = null;

		public Kbase.Model.ModelGateway ModelGateway {
			get {
                if (modelGateway == null)
                {
                    InitializeModelGateway();
                }
                Debug.Assert(modelGateway != null);
				return modelGateway; 
			}
		}

        public bool isModelGatewayInitialized = false;

        public void InitializeModelGateway()
        {
            try
            {
                Type gatewayType = Type.GetType(Settings.ModelGatewayClassname);
                ConstructorInfo constructor = gatewayType.GetConstructor(System.Type.EmptyTypes);
                modelGateway =
                    (ModelGateway)constructor.Invoke(null);
                Snippet test = modelGateway.TopLevelSnippet;
                isModelGatewayInitialized = true;
            }
            catch (FatalErrorException fex) {
                throw fex;
            }
            catch (Exception e)
            {
                if (e.InnerException is FatalErrorException)
                    throw e.InnerException;
                else
                    throw new FatalErrorException("Gateway " + Settings.ModelGatewayClassname + " could not be loaded. Is the classname correct?", e);
            }
        }

		/// <summary>
		/// returns true if save suceeds, will NOT call saveas!
		/// </summary>
		/// <returns></returns>
		public bool Save() 
		{
			bool success = false;
			detailPane.Save();

			SerializableUniverse universe = new SerializableUniverse(ModelGateway.TopLevelSnippet.Children);
			try 
			{
                Logger.Log("Saving TheKBase File to " + Path);
                DateTime marker = DateTime.Now;
				universe.Save(Path);
				mainForm.SetStatus("Sucessfully saved to " + Path + "");
				success = true;
                ModelGateway.Dirty = false;
                Logger.LogTimer(marker, "Saving file");
            } 
			catch (Exception ex) 
			{
				MainForm.ShowError(ex, "Error saving");
			}
			return success;
		}


        /// <summary>
        /// </summary>
        /// <returns></returns>
        public void Export(string xsl, string outputFile)
        {
            detailPane.Save();
            SerializableUniverse universe = new SerializableUniverse(ModelGateway.TopLevelSnippet.Children);
            universe.Export(xsl, outputFile);
        }


		public string Path = null;

		public void Restore(string newPath, bool mergeIntoCurrent) 
		{
            try
            {
                if (!mergeIntoCurrent)
                    Universe.Instance.mainForm.ForceAutosaveOff();

                mainForm.UseWaitCursor = true;
                System.IO.FileInfo file = new System.IO.FileInfo(newPath);
                if (!file.Exists)
                    throw new FatalErrorException("File not found " + file.FullName);
                newPath = file.FullName;
                if (mergeIntoCurrent)
                {
                    SerializableUniverse.Restore(newPath, Universe.Instance.ModelGateway.TopLevelSnippet, true);
                    // TODO 
                    mainForm.SetStatus("Sucessfully merged from " + newPath);
                    modelGateway.Dirty = true;
                }
                else
                {
                    Reset();
                    SerializableUniverse restoredSU = Kbase.Serialization.SerializableUniverse.Restore(newPath, Universe.Instance.ModelGateway.TopLevelSnippet, false);
                    mainForm.SetStatus("Sucessfully restored from " + newPath);
                    Path = newPath;
                    // if we've restored from another file format, we must automatically be dirty
                    if (restoredSU.restoredFromOldFileFormat)
                        modelGateway.Dirty = true;
                    else
                        modelGateway.Dirty = false;
                }
                mainForm.AddFileToOpenMenu(new System.IO.FileInfo(newPath));
                mainForm.AnnounceEditing();
                snippetPane.OnAfterRestore(Universe.Instance.modelGateway.TopLevelSnippet);
            }
            catch (FatalErrorException fex)
            {
                throw fex;
            }
            catch (Exception ex)
            {
                Path = null;
                MainForm.ShowError(ex, "Problem loading");
                Reset();
            }
            finally {
                mainForm.UseWaitCursor = false;            
            
            }

		}

        

		public void Reset() {
			try 
			{
                Universe.Instance.mainForm.ForceAutosaveOff();
				Snippet topLevel = ModelGateway.TopLevelSnippet;
				topLevel.RemoveAllChildSnippets();
                OnAfterSelectNone();
                Universe.Instance.propertiesPaneHolder.Reset();
                detailPane.Reset();
                snippetPane.Reset(topLevel);
                Path = null;
                ModelGateway.Dirty = false;
                if (ModelGateway.IsResetable)
                    modelGateway.Reset();
                mainForm.AnnounceEditing();
                encryption.Reset();
			}
			catch (FatalErrorException fex) {
				throw fex;
			}
			catch (Exception ex) 
			{
				throw new FatalErrorException("Could not reset, exiting.", ex);
			}
		}

        SnippetInstance cachedSelectionSingle = null;

        void Cache(SnippetInstance instance)
        {
            cachedSelectionSingle = instance;
        }

        void CacheReset()
        {
            cachedSelectionSingle = null;
        }

        public void OnAfterSelectOnCachedNodeChanged(Snippet whoChanged) {
            if (cachedSelectionSingle != null && whoChanged == cachedSelectionSingle.Snippet)
                OnAfterSelect(cachedSelectionSingle);
        }

        public void OnAfterSelect(SnippetInstance instance) {
            Cache(instance);
            detailPane.Edit(instance.Snippet);
            snippetDetailPane.Edit(instance);
        }

        public void OnAfterSelect(List<Snippet> snippets) {
            CacheReset();
            detailPane.EditNone();
            snippetDetailPane.Edit(snippets);
        }

        public void OnAfterSelectNone()
        {
            CacheReset();
            snippetDetailPane.EditNone();
            detailPane.EditNone();
        }

        public static string DotNetRuntimeInfo {
            get {
                string retVal = System.Environment.Version.ToString();
                return retVal;
            }
        }

        public static string LicensingInfoAsRtf {
            get
            {
                try
                {
                    // prep the icons
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    System.IO.Stream resourceStream = assembly.GetManifestResourceStream("Kbase.Support.license.rtf");
                    System.IO.StreamReader reader = new System.IO.StreamReader(resourceStream);
                    return reader.ReadToEnd();
                }
                catch (Exception ex)
                {
                    MainForm.ShowError(ex);
                    return "Licensing Information Not Available";
                }
            }
        }

        System.Timers.Timer autoSaveTimer;
        public void startAutoSave() {
            if (autoSaveTimer == null) {
                autoSaveTimer= new System.Timers.Timer();
                autoSaveTimer.Interval = 1000;
                autoSaveTimer.Elapsed += new System.Timers.ElapsedEventHandler(autoSaveTimer_Elapsed);
            }
            autoSaveTimer.Start();
        }

        public void stopAutoSave() {
            if (autoSaveTimer != null) autoSaveTimer.Stop();
        }

        void autoSaveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                mainForm.Invoke(new ZeroArgumentEventHandler(autoSaveTimerInner));
            }
            catch (InvalidOperationException) { 
                // mainform is disposed
            }
        }

        void autoSaveTimerInner() {
            if (Universe.Instance.ModelGateway.Dirty && Universe.Instance.Path != null)
                Save();
        }


        /// <summary>
        ///  Don't call me directly, please.<br>
        ///  If you register with me, you should also register with MainForm.closing
        /// </summary>
        public Kbase.ZeroArgumentEventHandler ExitEvents = null;
        bool isEmergencyExit = false;
        public bool IsEmergencyExitInvoked {
            get {
                return isEmergencyExit;
            }
            private set {
                isEmergencyExit = value;
            }
        
        }
        public void EmergencyExit() {
            IsEmergencyExitInvoked = true;
            if (Universe.Instance.mainForm != null && Universe.Instance.mainForm.IsHandleCreated)
            {
                ZeroArgumentEventHandler handler = new ZeroArgumentEventHandler(this.EmergencyExitInvocable);
                Universe.Instance.mainForm.Invoke(handler);
            }
            else
                EmergencyExitInvocable();
        }

        public void EmergencyExitInvocable() {
            if (mainForm != null)
                mainForm.Close();
            Exit();
        }

        /// <summary>
        /// This is called by all exiters whether emergency or not
        /// </summary>
        public void Exit() {
            if (ExitEvents != null)
                ExitEvents();
            // the problem is that one of the exit events (or any other thread, for
            // that matter) can be stacking up asynchronous events in the MainForm
            // to be invoke. So the Logger can be shut down and then we get 
            // Logger is shutdown exceptions. 
            // 
            // However I think this is now solved 2007-06-16 because I put the mainform
            // close before this exit call.
            if (Logger.IsInitialized)
                Logger.ShutDown();
        }

        public void EmergencyExit(FatalErrorException fex)
        {
            MainForm.ShowError(fex);
            EmergencyExit();
        }
        
	}
}
