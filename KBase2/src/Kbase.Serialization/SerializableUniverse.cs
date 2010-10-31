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
using System.Collections;
using System.Diagnostics;
using System.Xml.Serialization;
using Kbase.SnippetTreeView;
using Kbase.Model;
using System.Xml;
using System.Collections.Generic;
using Kbase.MainFrm;
using Kbase.LibraryWrap;


namespace Kbase.Serialization
{

    /// <summary>
    /// Convert the ModelInMemory (well, any model, really) into a nicer
    /// serializable model without circular references.
    /// </summary>
    [Serializable()]
	public class SerializableUniverse
	{
        [NonSerialized()]Serializer serializer = null;
		/// <summary>
		/// To support clipboard actions in the SnippetPane to compare instances of this class.
		/// </summary>
		public DateTime Version = DateTime.Now;
        public Hashtable snippets = new Hashtable();
		public List<int> topLevelIds = new List<int>();
        [NonSerialized()]
        public List<int> propertyIds = new List<int>();
        [NonSerialized()]
        public List<int> propertySetIds = new List<int>();
        [NonSerialized()]
        public bool restoredFromOldFileFormat = false;
        [NonSerialized()]
        public object UnknownPieceToReserialize = null;

		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="theParents">Kbase.Model.Snippet snippets</param>
		public SerializableUniverse(List<Snippet> theParents) : this(theParents, Serializer.GetDefaultSerializer())
		{
		}

		// create an empty serializable universe
		public SerializableUniverse() 
		{
		}

		/// <summary>
        ///  constructor preps for serialization
		/// </summary>
		/// <param name="parents"></param>
		/// <param name="serializer"></param>
		public SerializableUniverse(List<Snippet> parents, Serializer serializer) 
		{
			this.serializer = serializer;
			foreach (Snippet snippet in parents) 
			{
                this.topLevelIds.Add(snippet.Id);
                Handle(snippet);
			}

            // now handle the other bits, which are the properties, propertySets and unknown bit
            foreach (Snippet snippet in Universe.Instance.propertiesPaneHolder.getProperties())
            {
                propertyIds.Add(snippet.Id);                
            }

            foreach (Snippet snippet in Universe.Instance.propertiesPaneHolder.getPropertySets())
            {
                propertySetIds.Add(snippet.Id);
            }


            UnknownPieceToReserialize = Universe.Instance.ModelGateway.UnknownPieceToReserialize;
		}

		/// <summary>
		/// first time around, goes to the to level snippets, otherwise it goes to normal snippets
		/// </summary>
		/// <param name="snippet"></param>
		/// <param name="storeWhere"></param>
		public void Handle(Snippet snippet) {
			if (!snippets.ContainsKey(snippet.Id)) 
			{
				SerializableSnippet sSnippet = new SerializableSnippet(snippet);
				snippets.Add(sSnippet.Id, sSnippet);
				foreach (Snippet child in snippet.Children) 
				{
					Handle(child);
				}
			}
		}

		void PlaceInHashtable(Hashtable table, ArrayList list) 
		{
			foreach (SerializableSnippet sSnip in list) 
			{
				table.Add(sSnip.Id, sSnip);			
			}
		}


		public static SerializableUniverse Restore(string path, 
			Serializer serializer, 
			Snippet where, 
			bool merge) 
		{
			Logger.Log("Restoring from " + path);

            DateTime marker = DateTime.Now;
            
            SerializableUniverse retVal = null;
			// exceptions just get thrown up
            retVal = Restore(path,serializer);

            Logger.LogTimer(marker, "Disk restore");

            retVal.Restore(where,merge);
			return retVal;
		}

        void RestoreProperties() {
            // this is necessary for kbase to kbase cutting and pasting
            // the propertyIds, if empty, comes over as null
            if (propertyIds == null)
                return;
            foreach (int id in propertyIds) {
                Snippet property = Universe.Instance.ModelGateway.FindSnippet(id);
                if (property != null)
                    Universe.Instance.propertiesPaneHolder.RegisterProperty(property);
            }
            Universe.Instance.snippetDetailPane.OnAfterRestoreProperties();
        }

        void RestorePropertySets()
        {
            // this is necessary for kbase to kbase cutting and pasting
            // the propertyIds, if empty, comes over as null
            if (propertySetIds == null)
                return;
            foreach (int id in propertySetIds)
            {
                Snippet propertySet = Universe.Instance.ModelGateway.FindSnippet(id);
                if (propertySet != null)
                    Universe.Instance.propertiesPaneHolder.AddPropertySet(propertySet);
            }
            Universe.Instance.snippetDetailPane.OnAfterRestoreProperties();
        }


		/// <summary>
		///  TODO this doesn't respect the where yet... 
		/// </summary>
		/// <param name="where"></param>
		/// <param name="merge"></param>
		public List<Snippet> Restore(Snippet where, bool merge) 
		{
            Universe.Instance.ModelGateway.SuspendEvents = true;
            List<Snippet> retVal = new List<Snippet>();
		    if (!merge) {
			    Universe.Instance.snippetPane.Reset();
			    Universe.Instance.ModelGateway.TopLevelSnippet.UI.ForceShowingChildren(false);
		    }

            DateTime marker = DateTime.Now;

            if (!merge)
                SnippetInstanceTopLevel.ReplaceTopOfTree(new ArrayList(topLevelIds.Count));

		    foreach (int id in topLevelIds) 
		    {
			    SerializableSnippet sSnippet = snippets[id] as SerializableSnippet;
			    Snippet realSnippet = sSnippet.MakeSnippetInModel(where, snippets, merge);
                retVal.Add(realSnippet);
		    }
            Universe.Instance.ModelGateway.SuspendEvents = false;

            RestoreProperties();
            RestorePropertySets();
            
            // restore the unknown piece, which basically amounts to storing it in the model gateway
            Universe.Instance.ModelGateway.UnknownPieceToReserialize = UnknownPieceToReserialize; // null is okay

		    if (!merge) {
			    Universe.Instance.ModelGateway.TopLevelSnippet.UI.ShowChildren();
			    Universe.Instance.ModelGateway.TopLevelSnippet.UI.ShowGrandChildren();
                SnippetInstanceTopLevel.ReplaceTopOfTree();
            }


            Logger.LogTimer(marker, "Model Creation");

            return retVal;
		}

		public static SerializableUniverse Restore(string path, Serializer serializer) 
		{
			SerializableUniverse retVal = serializer.Restore(path);
			return retVal;
		}


		public static SerializableUniverse Restore(string path, Snippet where, bool resetFirst) 
		{
			Serializer defaultSerializer = new SerializerDom();//Serializer.GetDefaultSerializer();
			return Restore(path,defaultSerializer,where,resetFirst);
		}

		
		public void Save(string path) 
		{
			serializer.Save(this,path);
		}

		public System.Xml.XmlDocument GetXml() 
		{
            return serializer.GetXml(this);
		}

        public void Export(string xsl, string outputFilename) {
            System.IO.TextWriter writer = null;
            try
            {
                System.Xml.Xsl.XslCompiledTransform xsltransform = new System.Xml.Xsl.XslCompiledTransform();
                xsltransform.Load(xsl);

                // get the XML Document 
                System.Xml.XPath.IXPathNavigable xmlDoc = GetXml();
                writer = new System.IO.StreamWriter(outputFilename);
                // this is the right way to do it, but it's important NOT to use an XMLWriter
                xsltransform.Transform(xmlDoc.CreateNavigator(),null, writer);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }
	}


}
