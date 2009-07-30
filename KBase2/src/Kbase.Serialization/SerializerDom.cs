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
using System.Collections;
using System.Xml;
using System.Diagnostics;
using Kbase.Model.Search;
using Kbase.Model;
using Kbase.LibraryWrap;

namespace Kbase.Serialization
{
	/// <summary>
	/// Yeah, why not do automatic .NET serialization! Because it breaks with larger
    /// files. And then it doesn't work on .NET Mobile.
	/// </summary>
	public class SerializerDom : Serializer
	{
		public SerializerDom()
		{
		}

        // don't be scared by the absolute numbers for the pieces of the document
        // they can be changed, they ONLY refer to the structure that we
        // use in the DEFAULT_DOC.
        // On reading the XML document, we use element names
        public override XmlDocument GetXml(SerializableUniverse serializablePiece)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(DEFAULT_DOC); // READ IN THE TEXT BELOW WHICH IS THE 
            // STRUCTURE OF THE DOCUMENT
            XmlElement kbaseNode = (XmlElement)doc.FirstChild;

            XmlElement versionNode = (XmlElement)kbaseNode.FirstChild;
            XmlText text = doc.CreateTextNode(serialize(serializablePiece.Version));
            versionNode.AppendChild(text);

            XmlElement propertiesNode = (XmlElement)kbaseNode.ChildNodes[1];
            foreach (int id in serializablePiece.propertyIds)
            {
                makeChildXmlAndAppend(doc, propertiesNode, id);
            }

            XmlElement propertySetsNode = (XmlElement)kbaseNode.ChildNodes[2];
            foreach (int id in serializablePiece.propertySetIds)
            {
                makeChildXmlAndAppend(doc, propertySetsNode, id);
            }

            if (serializablePiece.UnknownPieceToReserialize != null) {
                XmlNodeList unknowns = serializablePiece.UnknownPieceToReserialize as XmlNodeList;
                foreach (XmlNode child in unknowns) {
                    XmlNode childClone = doc.ImportNode(child, true);
                    kbaseNode.AppendChild(childClone);
                }
            }

            XmlElement topLevelIdsNode = (XmlElement)kbaseNode.ChildNodes[3];
            foreach (int id in serializablePiece.topLevelIds)
            {
                makeChildXmlAndAppend(doc, topLevelIdsNode, id);
            }

            XmlElement snippetsNode = (XmlElement)kbaseNode.ChildNodes[4];
            foreach (SerializableSnippet snippet in serializablePiece.snippets.Values)
            {
                makeSnippetXmlAndAppend(doc, snippetsNode, snippet);
            }


            Encryption encryption = Universe.Instance.encryption;
            if (encryption.On)
            {
                XmlEncryption.EncryptElement(doc.FirstChild, encryption.Password, true);
            }

            return doc;

        }



		private XmlElement makeSnippetXmlAndAppend(XmlDocument doc, XmlElement parent, 
			SerializableSnippet snippet) 
		{
			XmlElement snippetNode = doc.CreateElement("Snippet");
			makeTextAndAppend(doc,snippetNode,"Text",snippet.Text);
			makeTextAndAppend(doc,snippetNode,"Title",snippet.Title);
			makeTextAndAppend(doc,snippetNode,"Created",serialize(snippet.Created));
            makeTextAndAppend(doc, snippetNode, "Modified", serialize(snippet.Modified));
            makeTextAndAppend(doc, snippetNode, "Id", Convert.ToString(snippet.Id));
			makeTextAndAppend(doc,snippetNode,"Color",snippet.Color);
			makeTextAndAppend(doc,snippetNode,"Icon",Convert.ToString(snippet.Icon));
			foreach (int child in snippet.children) {
				makeChildXmlAndAppend(doc,snippetNode,child);
			}

            foreach (SerializableCriterion criterion in snippet.Criteria)
            {
                makeCriterionXmlAndAppend(doc, snippetNode, criterion);
            }

			parent.AppendChild(snippetNode);
			return snippetNode;
		}


		private void makeChildXmlAndAppend(XmlDocument doc, XmlElement parent, 
			int id) 
		{
			makeTextAndAppend(doc,parent,"Child",Convert.ToString(id));
		}


        private void makeCriterionXmlAndAppend(XmlDocument doc, XmlElement parent, SerializableCriterion criterion)
        {
            string text = criterion.GetAsText();
            if (text == null)
                return;
            makeTextAndAppend(doc, parent, "Criterion", text);
        }


		private void makeTextAndAppend(XmlDocument doc, XmlElement parent, string name, string value) 
		{
			XmlText text = doc.CreateTextNode(value);
			XmlElement child = doc.CreateElement(name);
			child.AppendChild(text);
			parent.AppendChild(child);
		}

		public override SerializableUniverse Restore(string path)
		{
			XmlReader reader = null;
			try 
			{
				SerializableUniverse retVal = new SerializableUniverse();
				XmlDocument doc = new XmlDocument();
				reader = new XmlTextReader(path);
				doc.Load(reader);

				XmlElement kbaseNode = (XmlElement)doc.GetElementsByTagName("Kbase")[0];

                if (kbaseNode.FirstChild.Name.Equals(XmlEncryption.ENCRYPTED_NODE_NAME)) {
                    string password = null;
                    bool success = false;
                    XmlNode encryptedNode = kbaseNode.FirstChild;
                    while (!success)
                    {
                        password = Universe.Instance.encryption.SolicitPasswordOnLoad(password);
                        if (password == null)
                            throw new Exception("User cancelled.");
                        try
                        {
                            XmlEncryption.DecryptElement(encryptedNode, password);
                            Universe.Instance.encryption.Password = password;
                            success = true;
                        }
                        catch (Exception ex) {
                            Logger.Log("Failed decryption " + ex.Message);
                        }
                    }
                }

				// version node
				XmlElement versionNode = (XmlElement)kbaseNode.GetElementsByTagName("Version")[0];
				XmlText text = (XmlText)versionNode.FirstChild;
				retVal.Version = deserializeDateTime(text.Value);

                // deleting as we go
                kbaseNode.RemoveChild(versionNode);

				XmlElement topLevelIdNode = (XmlElement)kbaseNode.GetElementsByTagName("TopLevelIds")[0];
				foreach (XmlElement idXml in topLevelIdNode.ChildNodes) 
				{
					int id = HandleId(idXml);
					retVal.topLevelIds.Add(id);
				}

                // deleting as we go
                kbaseNode.RemoveChild(topLevelIdNode);

                XmlElement snippetsNode = (XmlElement)kbaseNode.GetElementsByTagName("Snippets")[0];
				foreach (XmlElement snippetXml in snippetsNode.ChildNodes) 
				{
					SerializableSnippet sSnippet = HandleSnippet(snippetXml);
					retVal.snippets.Add(sSnippet.Id,sSnippet);
				}

                // deleting as we go
                kbaseNode.RemoveChild(snippetsNode);

                XmlElement propertiesNode = (XmlElement)kbaseNode.GetElementsByTagName("Properties")[0];
                if (propertiesNode != null) // for backwards compatability
                {
                    foreach (XmlElement idXml in propertiesNode.ChildNodes)
                    {
                        int id = HandleId(idXml);
                        retVal.propertyIds.Add(id);
                    }
                    // deleting as we go
                    kbaseNode.RemoveChild(propertiesNode);
                }

                
                XmlElement propertySetNode = (XmlElement)kbaseNode.GetElementsByTagName("PropertySets")[0];
                if (propertySetNode != null) // for backwards compatability
                {
                    foreach (XmlElement idXml in propertySetNode.ChildNodes)
                    {
                        int id = HandleId(idXml);
                        retVal.propertySetIds.Add(id);
                    }
                    // deleting as we go
                    kbaseNode.RemoveChild(propertySetNode);
                }


                // now find all the other elements
                if (kbaseNode.ChildNodes.Count > 0)
                    retVal.UnknownPieceToReserialize = kbaseNode.ChildNodes;

				return retVal;
			} 
			finally 
			{
				if (reader!=null)
					reader.Close();
			}
		}

		SerializableSnippet HandleSnippet(XmlElement xmlElement) 
		{
			SerializableSnippet retVal = new SerializableSnippet();
			foreach (XmlElement child in xmlElement.ChildNodes) 
			{
				if (child.Name.Equals("Text")) 
				{
					string text = GetTextBelow(child);
					retVal.Text = text;
				}			
				else if (child.Name.Equals("Title")) 
				{
					string text = GetTextBelow(child);
					retVal.Title = text;
				}
				else if (child.Name.Equals("Created")) 
				{
					string text = GetTextBelow(child);
					retVal.Created = deserializeDateTime(text);
				}
                else if (child.Name.Equals("Modified"))
                {
                    string text = GetTextBelow(child);
                    retVal.Modified = deserializeDateTime(text);
                }
                else if (child.Name.Equals("Color")) 
				{
					string text = GetTextBelow(child);
					retVal.Color = text;
				}			
				else if (child.Name.Equals("Icon")) 
				{
					string text = GetTextBelow(child);
					retVal.Icon = text;				
				}			
				else if (child.Name.Equals("Id"))
				{
					string text = GetTextBelow(child);
					retVal.Id = Convert.ToInt32(text);				
				}			
				else if (child.Name.Equals("Child"))
				{
					string text = GetTextBelow(child);
					retVal.children.Add(Convert.ToInt32(text));				
				}
                else if (child.Name.Equals("Criterion"))
                {
                    string text = GetTextBelow(child);
                    SerializableCriterion criterion = new SerializableCriterion(text);
                    retVal.Criteria.Add(criterion);
                }			
			
			}
				
			return retVal;
		}


		int HandleId(XmlElement xmlElement) 
		{
			Debug.Assert(xmlElement.Name == "Child");
			string id = GetTextBelow(xmlElement);
			return (Convert.ToInt32(id));				
		}
		
		
		
		static string GetTextBelow(XmlElement element) 
		{
			XmlText textNode = (XmlText)element.FirstChild;
			if (textNode == null)
				return "";
			else
				return textNode.Value;
		}

		
		static string dateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffffffzzz";
		private static String serialize(DateTime dateTime) 
		{
			return dateTime.ToString(dateTimeFormat);
		}

		private static DateTime deserializeDateTime(string dateTime) 
		{
			return Convert.ToDateTime(dateTime);
		}


        static string DEFAULT_DOC = @"<Kbase><Version></Version><Properties></Properties><PropertySets></PropertySets><TopLevelIds></TopLevelIds><Snippets></Snippets></Kbase>";	


	}
}
