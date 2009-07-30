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
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using Kbase.MultipleSelectionTreeView;
using System.Reflection;
using System.IO;
using Kbase.MainFrm;
using Kbase.LibraryWrap;

namespace Kbase.Icon
{
	/// <summary>
	/// Handles ALL icon functionality, including
    /// - loading them from the disk
    /// - dealing with the indexes that we have to use in the tree
    /// Manipulates objects of .NET types for images and files
    /// and a few support class types (NamedIconSet NamedImage).
	/// </summary>
	public class IconList
	{
        static string ICON_INSTRUCTIONS = "Icons must be in pairs. Names must be 'closedExample one.ico' "+
            "and matching 'openExample one.ico'";
        static private IconList instance = null;
		public Dictionary<string, NamedIconSet> icons = new Dictionary<string,NamedIconSet>();
        public int defaultIconIndex = -1;
		public string searchIcon = null;

		static public IconList Instance 
		{
			get {
				if (instance == null)
					instance = new IconList();
				return instance;
			}		
		}


        static string Path
        {
            get
            {
                return System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "/icons";
            }
        }
        
        private string defaultIcon = null;

		public string DefaultIcon {
			get {
				if (defaultIcon == null)
					defaultIcon = GetIconName(defaultIconIndex);
				return defaultIcon;
			}
		}

		IconList()
		{
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                DirectoryInfo dirInfo = new DirectoryInfo(Path);
                LoadIcons(assembly, dirInfo);
            }
            catch (FatalErrorException fex) {
                throw fex;
            }
            catch (Exception e)
            {
                throw new FatalErrorException("Problem loading icons.", e);
            }
		}
		
		void LoadIcons(Assembly assembly, DirectoryInfo directory) 
		{

            // closed or unselected icons from assembly
			foreach (string name in assembly.GetManifestResourceNames()) {
                if (name.IndexOf("closed") != -1 && name.IndexOf(".ico") != -1)
                {
                    Image image = GetImage(assembly, name);
                    PlaceImage(false, image, name);
                }
			}

            // closed or unselected icons from files
            if (directory.Exists)
            {
                foreach (FileInfo file in directory.GetFiles())
                {
                    string name = file.Name;
                    if (name.IndexOf("closed") != -1 && name.IndexOf(".ico") != -1)
                    {
                        Image image = GetImage(file.FullName);
                        PlaceImage(false, image, name);
                    }
                }
            }

            // open or selected icons from assembly
            foreach (string name in assembly.GetManifestResourceNames())
            {
                if (name.IndexOf("open") != -1 && name.IndexOf(".ico") != -1)
                {
                    Image image = GetImage(assembly, name);
                    PlaceImage(true, image, name);
                }
            }


            // open or selected icons from files
            if (directory.Exists)
            {
                foreach (FileInfo file in directory.GetFiles())
                {
                    string name = file.Name;
                    if (name.IndexOf("open") != -1 && name.IndexOf(".ico") != -1)
                    {
                        Image image = GetImage(file.FullName);
                        PlaceImage(true, image, name);
                    }
                }
            }

            // verify the icons 
            if (icons.Count == 0)
                throw new FatalErrorException("No closed icons could be loaded, we have serious problems with the Assembly.");

            foreach (NamedIconSet set in icons.Values)
            {
                if (set.ImageUnselected == null)
                    throw new FatalErrorException("Unmatched icon " + set.FancyName + "\n" + ICON_INSTRUCTIONS);
                else if (set.ImageSelected == null) {
                    set.FillInImageSelected();
                }
            }


            // make the image lists, thereby assigning indexes to the namedicons
            MakeImageLists();

            // now that we've sorted we can look for the default and other icons
            foreach (NamedIconSet namedIconSet in icons.Values)
            {
                if (defaultIconIndex == -1 && namedIconSet.FancyName.Equals(Universe.Instance.Settings.DefaultIcon))
                    defaultIconIndex = namedIconSet.IndexUnselected;
                if (searchIcon == null && namedIconSet.FancyName.Equals(Universe.Instance.Settings.SearchIcon))
                    searchIcon = namedIconSet.FancyName;
            }

            // what do we do if we cannot find them?
            if (defaultIconIndex == -1)
                defaultIconIndex = 0;
            if (searchIcon == null)
                searchIcon = this.GetIconName(defaultIconIndex);
        }

		private void PlaceImage(bool selected, Image image, string name) 
		{
            if (!name.Contains(" "))
                throw new FatalErrorException("Icon " + name + " is illegal, it has to contain a space.\n" + ICON_INSTRUCTIONS);
            if (!selected)
            {
                NamedImage namedImage = new NamedImageUnselected(image, name);
                NamedIconSet namedSet = new NamedIconSet();
                namedSet.ImageUnselected = namedImage;
                if (icons.ContainsKey(namedImage.FancyName)) {
                    throw new FatalErrorException("Problem with icons, duplicate icon " + name + "\n" + ICON_INSTRUCTIONS);
                }
                icons.Add(namedImage.FancyName, namedSet);
            }
            else {
                NamedImage namedImage = new NamedImageSelected(image, name);
                if (!icons.ContainsKey(namedImage.FancyName)) {
                    throw new FatalErrorException("Problem with icons, unmatched icon " + name + "\n" + ICON_INSTRUCTIONS);
                }
                NamedIconSet namedSet = icons[namedImage.FancyName];
                namedSet.ImageSelected = namedImage;
            }

		}




        public static Image GetImage(Assembly assembly, string name)
        {
            Image image;
            image = Image.FromStream(assembly.GetManifestResourceStream(name));
            return image;
        }

        public static Image GetImage(string filename)
        {
            Image image;
            FileStream stream = null; 
            try
            {
                stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                image = Image.FromStream(stream);
                return image;
            }
            finally {
                if (stream != null)
                    stream.Close();
            }
        }


        ImageList cachedImageList = null;
        ImageList cachedImageListNoSelected = null;

        void MakeImageLists() {
            cachedImageListNoSelected = new ImageList();
            cachedImageList = new ImageList();

            // first we load up the keys and the imagelists for the unselected icons
            foreach (NamedIconSet namedImageSet in icons.Values)
            {
                string key = namedImageSet.ImageUnselected.OriginalName;
                Image image = namedImageSet.ImageUnselected.image;
                cachedImageListNoSelected.Images.Add(key, image);
                cachedImageList.Images.Add(key, image);
                namedImageSet.IndexUnselected = cachedImageList.Images.IndexOfKey(key);
            }

            // then we load up for the selected icons, but only in one of the two imagelist collections
            foreach (NamedIconSet namedImageSet in icons.Values)
            {
                string key = namedImageSet.ImageSelected.OriginalName;
                Image image = namedImageSet.ImageSelected.image;
                cachedImageList.Images.Add(key, image);
                namedImageSet.IndexSelected = cachedImageList.Images.IndexOfKey(key);
            }
        }
        
        public ImageList GetImageList(bool includeSelected) 
		{
            if (cachedImageList == null)
            {
                MakeImageLists();
            }
            if (includeSelected)
                return cachedImageList;
            else
                return cachedImageListNoSelected;
		}		

		public int GetIconIndexUnselected(string name) {
            if (name == null || name.Length == 0)
                throw new NullReferenceException("Icon name may not be null nor blank.");
            int retVal;
            try
            {
                NamedIconSet set = icons[name];
                retVal = set.IndexUnselected;
            }
            catch (KeyNotFoundException) {
                try
                {
                    // legacy issues
                    NamedIconSet set = icons["Book " + name];
                    retVal = set.IndexUnselected;
                    System.Diagnostics.Debug.WriteLine("Successfully found icon " + name + " by prepending 'Book '.");
                }
                catch (KeyNotFoundException)
                {
                    Warn(name);
                    retVal = defaultIconIndex;
                    Logger.Log("Substituting default icon for " + name + ".");
                }
            }
			return retVal;
		}

        public int GetIconIndexSelected(string name)
        {
            if (name == null || name.Length == 0)
                throw new NullReferenceException("Icon name may not be null nor blank.");
            int retVal;
            try
            {
                NamedIconSet set = icons[name];
                retVal = set.IndexSelected;
            }
            catch (KeyNotFoundException)
            {
                try
                {
                    // legacy issues
                    NamedIconSet set = icons["Book " + name];
                    retVal = set.IndexSelected;
                    System.Diagnostics.Debug.WriteLine("Successfully found icon " + name + " by prepending 'Book '.");
                }
                catch (KeyNotFoundException)
                {
                    Warn(name);
                    retVal = defaultIconIndex;
                    Logger.Log("Substituting default icon for " + name + ".");
                }
            }
            return retVal;
        }

        bool warned = false;
        void Warn(string iconName) {
            if (!warned)
                MessageBox.Show("You are missing one or more icon files (including " + iconName + ").\nSnippets will be shown with the default icon but will remain intact.", MainForm.DialogCaption, MessageBoxButtons.OK);
            warned = true;    
        }

		public string GetIconName(int index) 
		{
            foreach (NamedIconSet set in icons.Values) {
                if (set.IndexUnselected == index)
                    return set.ImageUnselected.FancyName;
            }
            throw new FatalErrorException("Not sure how, but the user has the index for an icon that does not exist.");
		}

        public static System.Drawing.Icon GetIconFromAssembly(string name)
        {
            Stream stream = null;
            try
            {
                // prep the icon
                Assembly assembly = Assembly.GetExecutingAssembly();
                stream = assembly.GetManifestResourceStream(name);
                System.Drawing.Icon retVal = new System.Drawing.Icon(stream);
                return retVal;
            }
            finally {
                if (stream != null)
                    stream.Close();
            }
        }

        public static Image GetImageFromAssembly(string name, bool closeStream)
        {
            Stream stream = null;
            try
            {
                // prep the icon
                Assembly assembly = Assembly.GetExecutingAssembly();
                stream = assembly.GetManifestResourceStream(name);
                Image retVal = Image.FromStream(stream);
                return retVal;
            }
            finally
            {
                if (stream != null && closeStream)
                    stream.Close();
            }
        }

	}


}
