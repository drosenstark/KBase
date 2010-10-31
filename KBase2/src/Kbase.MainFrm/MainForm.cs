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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;
using Kbase.SnippetTreeView;
using Kbase.Model;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using Kbase.LibraryWrap;
using Kbase.DetailPanel;

namespace Kbase.MainFrm
{
    /// <summary>
    /// Summary description for KBase.
    /// </summary>
    public class MainForm : Form
    {
        private StatusBarPanel statusBarBigPanel = new StatusBarPanel();
        public static string DialogCaption = "Confusionists KBase";
        public Splitter splitter = new Splitter();
        public Splitter splitter2 = new Splitter();
        public Splitter splitter3 = new Splitter();



        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;


        public MainForm()
        {
            Universe.Instance.mainForm = this;
            Init();
        }

        public void Init()
        {
            if (Universe.emergencyExit)
                return;
            //
            // Required for Windows Form Designer support
            //
            Init2();
            //if (Universe.Instance.Settings.testing)
            //{
            //    new TestForm().Show();
            //}

            Icon = Kbase.Icon.IconList.GetIconFromAssembly("Kbase.Support.kbase.ico");
            Universe.Instance.Settings.RestoreWindows();
            //Init3();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void Init2()
        {
            Universe.Instance.detailPane = new Kbase.DetailPanel.DetailPane();
            Universe.Instance.snippetPane = new SnippetPane();


            // 
            // detailPane
            // 
            Universe.Instance.detailPane.AcceptsTab = true;
            Universe.Instance.detailPane.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            Universe.Instance.detailPane.Location = new System.Drawing.Point(136, 96);
            Universe.Instance.detailPane.Name = "detailPane";
            Universe.Instance.detailPane.Size = new System.Drawing.Size(120, 120);
            Universe.Instance.detailPane.TabIndex = 3;
            // 
            // snippetPane
            // 
            Universe.Instance.snippetPane.ImageIndex = -1;
            Universe.Instance.snippetPane.Location = new System.Drawing.Point(16, 72);
            Universe.Instance.snippetPane.Name = "snippetPane";
            Universe.Instance.snippetPane.Size = new System.Drawing.Size(175, 88);
            Universe.Instance.snippetPane.TabIndex = 1;

            Panel snippetPanePanel = new Panel();
            Image forward = Kbase.Icon.IconList.GetImageFromAssembly("Kbase.Support.right.ico", false);
            Image back = Kbase.Icon.IconList.GetImageFromAssembly("Kbase.Support.left.ico", false);
            Image refresh = Kbase.Icon.IconList.GetImageFromAssembly("Kbase.Support.refresh.ico", false);
            Kbase.MultipleSelectionTreeView.NavigationToolBar bar = new Kbase.MultipleSelectionTreeView.NavigationToolBar(Universe.Instance.snippetPane, back, forward);

            bar.Dock = DockStyle.Top;
            snippetPanePanel.Controls.Add(Universe.Instance.snippetPane);
            snippetPanePanel.Controls.Add(new Splitter());
            snippetPanePanel.Controls.Add(bar);
            Universe.Instance.snippetPane.Top = bar.Height;

            LoadMenus();

            // 
            // splitter
            // 
            splitter.Location = new System.Drawing.Point(0, 0);
            splitter.Name = "splitter";
            splitter.TabStop = false;

            splitter2.Location = new System.Drawing.Point(0, 0);
            splitter2.Name = "splitter2";
            splitter2.TabStop = false;

            splitter3.Location = new System.Drawing.Point(0, 0);
            splitter3.Name = "splitter2";
            splitter3.TabStop = false;

            // 
            // statusBar
            // 
            StatusBar statusBar = new StatusBar();
            statusBar.Location = new System.Drawing.Point(0, 244);
            statusBar.Name = "statusBar";
            statusBar.Panels.AddRange(new StatusBarPanel[] { this.statusBarBigPanel });
            statusBar.ShowPanels = true;
            statusBar.Size = new System.Drawing.Size(292, 22);

            // 
            // statusBarBigPanel
            // 
            statusBarBigPanel.Text = "TheKBase (www.thekbase.com)";
            statusBarBigPanel.Width = 1000;

            // propertiesPane
            Universe.Instance.snippetDetailPane = new Kbase.Properties.SnippetDetailPane();
            Universe.Instance.snippetDetailPane.TabIndex = 2;


            // set up the subpanel
            Panel panel = new Panel();
            Universe.Instance.detailPane.Dock = DockStyle.Fill;
            splitter3.Dock = DockStyle.Top;
            Universe.Instance.snippetDetailPane.Dock = DockStyle.Top;
            panel.Controls.Add(Universe.Instance.detailPane);
            panel.Controls.Add(splitter3);
            panel.Controls.Add(Universe.Instance.snippetDetailPane);
            panel.Dock = DockStyle.Fill;

            Panel biggerOuterRightPanel = new Panel();
            searchForm.Dock = DockStyle.Top;
            splitter2.Dock = DockStyle.Top;
            biggerOuterRightPanel.Controls.Add(panel);
            biggerOuterRightPanel.Controls.Add(splitter2);
            biggerOuterRightPanel.Controls.Add(searchForm);
            biggerOuterRightPanel.Dock = DockStyle.Fill;


            Universe.Instance.snippetPane.Dock = DockStyle.Fill;
            snippetPanePanel.Dock = DockStyle.Left;
            splitter.Dock = DockStyle.Left;
            statusBar.Dock = DockStyle.Bottom;

            this.Controls.Add(biggerOuterRightPanel);
            this.Controls.Add(splitter);
            this.Controls.Add(snippetPanePanel);
            this.Controls.Add(statusBar);

            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(950, 680);
            this.Name = "KBase";
            AnnounceEditing();
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            this.Deactivate += new EventHandler(OnDeactivate);
            this.Activated += new EventHandler(OnActivate);
        }


        MenuItem fileMenu;
        public MenuItem autoSaveMenu;

        void LoadMenus()
        {
            MainMenu mainMenu1 = new MainMenu();
            this.Menu = mainMenu1;

            fileMenu = new MenuItem("&File");
            mainMenu1.MenuItems.Add(fileMenu);

            MenuItem item;

            item = new MenuItem("&New");
            item.Click += new System.EventHandler(this.ClickNew);
            fileMenu.MenuItems.Add(item);

            item = new MenuItem("&Open");
            item.Click += new System.EventHandler(this.ClickOpen);
            fileMenu.MenuItems.Add(item);

            item = new MenuItem("Merge &Into Current");
            item.Click += new System.EventHandler(this.ClickMerge);
            fileMenu.MenuItems.Add(item);

            item = new MenuItem("&Save");
            item.Click += new System.EventHandler(this.ClickSave);
            fileMenu.MenuItems.Add(item);

            item = new MenuItem("Save &As");
            item.Click += new System.EventHandler(this.ClickSaveAs);
            fileMenu.MenuItems.Add(item);

            item = new MenuItem("Add/Remove &Password Encryption");
            item.Click += new System.EventHandler(this.ClickAddPassword);
            fileMenu.MenuItems.Add(item);

            item = new MenuItem("-");
            menuItemPutRecentFilesAfter = item;
            fileMenu.MenuItems.Add(item);
            fileMenu.MenuItems.Add(new MenuItem("-"));

            autoSaveMenu = new MenuItem("&Autosave");
            fileMenu.MenuItems.Add(autoSaveMenu);
            autoSaveMenu.Click += new System.EventHandler(this.ClickAutoSaveToggle);

            item = new MenuItem("E&xit");
            item.Click += new System.EventHandler(this.ClickExit);
            fileMenu.MenuItems.Add(item);

            // setup EDIT menu
            MenuItem editMenu = new MenuItem("T&ext Pane");
            mainMenu1.MenuItems.Add(editMenu);


            item = new MenuItem("&Undo");
            item.Click += new System.EventHandler(Universe.Instance.detailPane.ClickUndo);
            editMenu.MenuItems.Add(item);

            item = new MenuItem("&Redo");
            item.Click += new System.EventHandler(Universe.Instance.detailPane.ClickRedo);
            editMenu.MenuItems.Add(item);

            editMenu.MenuItems.Add(new MenuItem("-"));

            item = new MenuItem("Cu&t");
            item.Click += new System.EventHandler(Universe.Instance.detailPane.ClickCut);
            editMenu.MenuItems.Add(item);

            item = new MenuItem("&Copy");
            item.Click += new System.EventHandler(Universe.Instance.detailPane.ClickCopy);
            editMenu.MenuItems.Add(item);

            item = new MenuItem("&Paste");
            item.Click += new System.EventHandler(Universe.Instance.detailPane.ClickPaste);
            editMenu.MenuItems.Add(item);

            editMenu.MenuItems.Add(new MenuItem("-"));

            item = new MenuItem("Copy S&pecial (as Text)");
            item.Click += new System.EventHandler(Universe.Instance.detailPane.ClickCopyPlainText);
            editMenu.MenuItems.Add(item);

            item = new MenuItem("Paste &Special (As Text)");
            item.Click += new System.EventHandler(Universe.Instance.detailPane.ClickPastePlainText);
            editMenu.MenuItems.Add(item);

            editMenu.MenuItems.Add(new MenuItem("-"));

            item = new MenuItem("&Find");
            item.Click += new System.EventHandler(Universe.Instance.detailPane.ClickFind);
            editMenu.MenuItems.Add(item);

            editMenu.MenuItems.Add(new MenuItem("-"));

            item = new MenuItem("Insert Link to &File");
            item.Click += new System.EventHandler(Universe.Instance.detailPane.ClickInsertHyperlink);
            editMenu.MenuItems.Add(item);

            item = new MenuItem("Insert Link to &Snippet");
            item.Click += new System.EventHandler(Universe.Instance.detailPane.ClickInsertSnippetlink);
            editMenu.MenuItems.Add(item);

            item = new MenuItem("Insert &Date");
            item.Click += new System.EventHandler(Universe.Instance.detailPane.ClickInsertDate);
            editMenu.MenuItems.Add(item);


            // VIEW MENU
            MenuItem viewMenu = new MenuItem("&View");
            mainMenu1.MenuItems.Add(viewMenu);

            item = new MenuItem("Zoom &In");
            item.Click += new System.EventHandler(Universe.Instance.detailPane.ClickZoomIn);
            viewMenu.MenuItems.Add(item);

            item = new MenuItem("Zoom &Out");
            item.Click += new System.EventHandler(Universe.Instance.detailPane.ClickZoomOut);
            viewMenu.MenuItems.Add(item);

            item = new MenuItem("Show &Tracing Information");
            item.Click += new System.EventHandler(ClickShowTraceInformation);
            if (Universe.Instance.Settings.tracing)
                viewMenu.MenuItems.Add(item);


            // TOOLS MENU
            MenuItem toolsMenu = new MenuItem("&Tools");
            mainMenu1.MenuItems.Add(toolsMenu);

            item = new MenuItem("&Export using XSLT");
            item.Click += new System.EventHandler(this.ClickExport);
            toolsMenu.MenuItems.Add(item);

            PlugInManager.Init();
            List<MenuItem> plugInItems = PlugInManager.getPluginItems();
            foreach (MenuItem plugInItem in plugInItems)
            {
                toolsMenu.MenuItems.Add(plugInItem);
            }

            MenuItem helpMenu = new MenuItem("&Help");
            mainMenu1.MenuItems.Add(helpMenu);

            item = new MenuItem("Want to &Support TheKBase Development?");
            item.Click += new System.EventHandler(this.ClickDonate);
            helpMenu.MenuItems.Add(item);

            item = new MenuItem("&About");
            item.Click += new System.EventHandler(this.ClickAbout);
            helpMenu.MenuItems.Add(item);

        }




        /// <summary>
        /// 
        /// </summary>
        /// <returns>returns false if the action should be interrupted</returns>
        private bool InsistOnSave()
        {
            // if we haven't loaded yet, return.
            if (Universe.Instance.detailPane == null || !Universe.Instance.isModelGatewayInitialized)
                return true;

            if (ExternalSnippet.Watchers.Count > 0)
            {
                string message = "You are editing " + (ExternalSnippet.Watchers.Count) + " snippets externally. If they have been saved, press OK.";
                DialogResult result = MessageBox.Show(message, DialogCaption, MessageBoxButtons.OKCancel);
                if (result == DialogResult.Cancel)
                    return false;
                else
                    ExternalSnippet.DropWatchers(); // no problem if they cancel later, we've shutdown the watchers
            }

            Universe.Instance.detailPane.Save();
            if (Universe.Instance.ModelGateway.Dirty)
            {
                string message = "Do you want to save the changes to " + PathName + "?";
                DialogResult result = MessageBox.Show(message, DialogCaption, MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Cancel)
                    return false;
                else if (result == DialogResult.Yes)
                {
                    return ClickSave(); // returns success, must be inverted
                }
            }
            return true;
        }

        bool isInitialized = false;
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (!isInitialized)
            {
                isInitialized = true;
                Startup.Init3();
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            try
            {
                // if it's an emergency exit we do not even try to save
                if (!Universe.Instance.IsEmergencyExitInvoked)
                {
                    e.Cancel = !InsistOnSave();
                    if (e.Cancel)
                        return;
                }
                Universe.Instance.Settings.SaveSilent();

                Universe.Instance.Exit();
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
            Universe universe = Universe.Instance;
            try
            {
                base.OnKeyDown(e);
                if (e.KeyCode == Keys.S && e.Control)
                    ClickSave();
                else if (e.KeyCode == Keys.Left && e.Alt)
                    Universe.Instance.snippetPane.NavigateBackward();
                else if (e.KeyCode == Keys.Right && e.Alt)
                    Universe.Instance.snippetPane.NavigateForward();
                else if (e.Control && e.KeyCode == Keys.F)
                    Universe.Instance.detailPane.ClickFind(null, null);
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            try
            {
                base.OnKeyPress(e);
                // suppress the beeps
                e.Handled = true;

            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }
        }


        public void OnKeyDownPublic(KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        private void PrepareFileDialog(FileDialog fileDialog)
        {
            fileDialog.Filter = "TheKBase files (*.kbase, *.kbase.xml, *.xml)|*.kbase;*.kbase.xml;*.xml|All files (*.*)|*.*";
            fileDialog.FilterIndex = 0;
            fileDialog.RestoreDirectory = true;
        }






        private void ClickNew(object sender, System.EventArgs e)
        {
            Universe universe = Universe.Instance;

            try
            {
                // if the user is hitting new, didn't need to save or
                // did save but there were no nodes in there
                if (Universe.Instance.Path == null && Universe.Instance.ModelGateway.TopLevelSnippet.Children.Count == 0)
                {
                    ShowMessage("You are already in an empty KBase. \nRight click in the left pane to create a snippet.");
                }
                else
                {
                    bool insistResult = InsistOnSave();
                    if (insistResult)
                        Universe.Instance.Reset();
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }

        public void ClickOpenRecentFile(object o, EventArgs i)
        {
            try
            {
                // jump out if save didn't go or user cancelled.
                if (!InsistOnSave())
                    return;
                MenuItem fileItem = o as MenuItem;
                FileInfo tag = fileItem.Tag as FileInfo;
                Universe.Instance.Restore(tag.FullName, false);
            }
            catch (Exception e2)
            {
                // this error is fatal because if we can't find the file on load
                // we don't load. But here we unfatalize it.
                if (e2 is FatalErrorException)
                    e2 = new Exception("File could not be loaded.", e2);
                MainForm.ShowError(e2);
            }
        }

        private void ClickOpen(object sender, System.EventArgs e)
        {
            Universe universe = Universe.Instance;
            try
            {
                // jump out if save didn't go or user cancelled.
                if (!InsistOnSave())
                    return;
                OpenFileDialog fileDialog = new OpenFileDialog();
                PrepareFileDialog(fileDialog);
                DialogResult result = fileDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string path = fileDialog.FileName;
                    if (path != null)
                    {
                        Universe.Instance.Restore(path, false);
                    }
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }
        }




        private void ClickMerge(object sender, System.EventArgs e)
        {
            Universe universe = Universe.Instance;

            try
            {
                // jump out if save didn't go or user cancelled.
                if (!InsistOnSave())
                    return;
                OpenFileDialog fileDialog = new OpenFileDialog();
                PrepareFileDialog(fileDialog);
                DialogResult result = fileDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string path = fileDialog.FileName;
                    if (path != null)
                    {
                        Universe.Instance.Restore(path, true);
                    }
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }


        public Search.SearchForm searchForm = new Search.SearchForm();

        private void ClickExport(object sender, System.EventArgs e)
        {
            try
            {
                OpenFileDialog fileDialogXsl = new OpenFileDialog();
                fileDialogXsl.Title = "XSL File";
                fileDialogXsl.Filter = "XSL Files (*.xsl)|*.xsl|All files (*.*)|*.*";
                fileDialogXsl.FilterIndex = 1;
                fileDialogXsl.RestoreDirectory = true;
                fileDialogXsl.InitialDirectory = Application.StartupPath;
                string oldDir = Environment.CurrentDirectory;
                DialogResult result = fileDialogXsl.ShowDialog();
                if (result != DialogResult.OK)
                    return;
                string xsl = fileDialogXsl.FileName;
                if (xsl == null)
                    return;
                SaveFileDialog fileDialog2 = new SaveFileDialog();
                fileDialog2.Title = "Output File";
                fileDialog2.Filter = "HTML Files (*.html)|*.html|All files (*.*)|*.*";
                fileDialog2.FilterIndex = 1;
                fileDialog2.RestoreDirectory = true;
                result = fileDialog2.ShowDialog();
                if (result != DialogResult.OK)
                    return;

                string outputPath = fileDialog2.FileName;
                if (outputPath == null)
                    return;
                Universe.Instance.Export(xsl, outputPath);
                DialogResult launch = MessageBox.Show("Saved sucessfully to " + outputPath + ". Open it?", DialogCaption, MessageBoxButtons.YesNo);
                if (launch == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("\"" + outputPath + "\"");
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }


        private void ClickDonate(object sender, System.EventArgs e)
        {
            try
            {
                string message = "Donate via PayPal to help support TheKBase development. You choose the amount to donate.\n" +
                    "Click OK to go to PayPal now.";
                DialogResult result = MessageBox.Show(message, DialogCaption, MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    string donateHere = "https://www.paypal.com/cgi-bin/webscr?business=paypal@confusionists.com&cmd=_xclick&item_name=Donation to support software development at Confusionists, Inc.";
                    System.Diagnostics.Process.Start(donateHere);
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }

        private void ClickAbout(object sender, System.EventArgs e)
        {
            try
            {
                new AboutBox().ShowDialog(this);
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }

        private void ClickShowTraceInformation(object sender, System.EventArgs e)
        {
            try
            {
                Universe.Instance.ModelGateway.PrintInfo();
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }

        public void ForceAutosaveOff() {
            if (autoSaveMenu.Checked)
                ClickAutoSaveToggle(null, null);
        }

        public void ClickAutoSaveToggle(object sender, System.EventArgs e)
        {
            try
            {
                autoSaveMenu.Checked = !autoSaveMenu.Checked;
                if (autoSaveMenu.Checked)
                {
                    Universe.Instance.startAutoSave();
                }
                else
                {
                    Universe.Instance.stopAutoSave();
                }
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }

        private void ClickExit(object sender, System.EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }

        private void ClickSave(object sender, System.EventArgs e)
        {
            try
            {
                ClickSave();
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if save went okay</returns>
        private bool ClickSave()
        {
            if (Universe.Instance.Path == null)
                return ClickSaveAs();
            else
                return Universe.Instance.Save();
        }


        private void ClickSaveAs(object sender, System.EventArgs e)
        {
            try
            {
                ClickSaveAs();
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }

        bool ClickSaveAs()
        {
            bool success = false;
            SaveFileDialog fileDialog = new SaveFileDialog();
            PrepareFileDialog(fileDialog);
            DialogResult result = fileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                string path = fileDialog.FileName;
                if (path != null)
                {
                    Universe.Instance.Path = path;
                    success = Universe.Instance.Save();
                    AnnounceEditing();
                }
            }
            return success;
        }

        private void ClickAddPassword(object sender, System.EventArgs e)
        {
            try
            {
                Universe.Instance.encryption.SolicitPassword();
            }
            catch (Exception e2)
            {
                MainForm.ShowError(e2);
            }

        }


        MenuItem menuItemPutRecentFilesAfter = null;

        public void AddFileToOpenMenu(System.IO.FileInfo file)
        {
            MenuItem duplicate = null;
            foreach (MenuItem idem in recentFiles)
            {
                if (((FileInfo)idem.Tag).FullName.Equals(file.FullName))
                    duplicate = idem;
            }
            if (duplicate != null)
            {
                fileMenu.MenuItems.Remove(duplicate);
                recentFiles.Remove(duplicate);
            }


            MenuItem item = new MenuItem(file.Name);
            item.Click += new System.EventHandler(this.ClickOpenRecentFile);


            fileMenu.MenuItems.Add(menuItemPutRecentFilesAfter.Index + 1, item);
            item.Tag = file;
            if (recentFiles.Count > 3)
            {
                MenuItem removeThis = recentFiles[2];
                recentFiles.Remove(removeThis);
                fileMenu.MenuItems.Remove(removeThis);
            }
            recentFiles.Insert(0, item);
        }

        List<MenuItem> recentFiles = new List<MenuItem>();

        public string[] RecentFiles
        {
            get
            {
                string[] retVal = new string[recentFiles.Count];
                int i2 = recentFiles.Count;
                for (int i = 0; i < recentFiles.Count; i++)
                {
                    retVal[--i2] = (recentFiles[i].Tag as FileInfo).FullName;
                }
                return retVal;
            }
            set
            {
                if (value == null)
                    return; // settings may be empty
                foreach (string fullname in value)
                {
                    FileInfo info = new FileInfo(fullname);
                    AddFileToOpenMenu(info);
                }
            }
        }

        private const string NO_PATH = "UNTITLED";
        private string PathName
        {
            get
            {
                if (Universe.Instance.Path == null)
                    return NO_PATH;
                else
                    return Universe.Instance.Path;
            }

        }


        public void AnnounceEditing(Snippet snippet)
        {
            if (snippet != null)
                Universe.Instance.SnippetBeingViewedTitle = snippet.Title;
            AnnounceEditing();
        }


        public void SetStatus(string text)
        {
            statusBarBigPanel.Text = text + " (" + System.DateTime.Now + ")";
        }

        public void AnnounceEditing()
        {
            string snippetTitle = Universe.Instance.detailPane.GetSnippetName();
            int lastSlash = PathName.LastIndexOf("\\");
            string path;
            if (lastSlash > -1)
                path = PathName.Remove(0, lastSlash + 1);
            else
                path = PathName;
            Text = path + " - TheKBase - " + snippetTitle;
        }


        public static void ShowErrorSilent(Exception ex)
        {
            if (ex == null)
                return;
            else
            {
                if (Logger.IsInitialized)
                    Logger.Log(ex);
            }
        }

        public static void ShowError(Exception ex)
        {
            ShowError(ex, ex.Message);
        }


        public static void ShowError(string text)
        {
            ShowError(null, text);
        }

        private static bool messageBoxShowing = false;

        public static void ShowError(Exception ex, string text)
        {
            string header;
            if (ex is FatalErrorException)
                header = "Fatal Error";
            else
                header = "Error";

            if (text == null || text.Length == 0)
                text = header;

            if (!messageBoxShowing)
            {
                messageBoxShowing = true;
                MessageBox.Show("An error has ocurred (" + text + "). Please see the log file for error details.", header);
                messageBoxShowing = false;
            }

            if (ex != null)
                ShowErrorSilent(ex);

            if (ex is FatalErrorException)
            {
                if (Logger.IsInitialized)
                    Logger.Log("MainForm.ShowError: FatalException thrown, MainForm proceeding to emergency exit.");
                Universe.Instance.EmergencyExit();
            }
        }

        static string GetMessages(Exception ex)
        {
            string message = ex.Message; // +"\n" + ex.StackTrace;
            if (ex.InnerException != null)
            {
                message += "\n" + GetMessages(ex.InnerException);
            }
            return message;
        }


        public void ShowMessage(string text)
        {
            MessageBox.Show(text, DialogCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #region Code to handle blocking of the UI if the file is password encrypted
        Timer lockForPasswordTimer = null;

        private void startLockTimer()
        {
            if (lockForPasswordTimer == null)
            {
                lockForPasswordTimer = new Timer();
                lockForPasswordTimer.Tick += new EventHandler(lockTimerHandler);
            }
            lockForPasswordTimer.Interval = 300000;
            lockForPasswordTimer.Start();
        }

        private void stopLockTimer()
        {
            if (lockForPasswordTimer != null)
                lockForPasswordTimer.Stop();
        }

        void OnDeactivate(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                return;
            // we always start even if the file is not encrypted
            // though we could have done it the other way around
            startLockTimer();
        }

        void OnActivate(object sender, EventArgs e)
        {
            stopLockTimer();
        }

        void lockTimerHandler(object sender, EventArgs e)
        {
            if (lockForPasswordTimer != null)
                lockForPasswordTimer.Stop();
            if (Universe.Instance.encryption.On)
            {
                WindowState = FormWindowState.Minimized;
            }
        }

        bool solicitPass()
        {
            bool passOkay = true;
            if (Universe.Instance.encryption.On)
                passOkay = Universe.Instance.encryption.SolicitPasswordOnUIBlock();
            return passOkay;
        }

        bool shouldBeMinimized = false;

        protected override void OnResize(EventArgs e)
        {
            if (!shouldBeMinimized)
            {
                if (WindowState == FormWindowState.Minimized)
                    shouldBeMinimized = true;
                base.OnResize(e);
            }
            else
            {
                if (WindowState != FormWindowState.Minimized)
                {
                    if (solicitPass())
                    {
                        base.OnResize(e);
                        stopLockTimer();
                        shouldBeMinimized = false;
                    }
                    else
                        WindowState = FormWindowState.Minimized;
                }
            }
        }
        #endregion

    }
}
