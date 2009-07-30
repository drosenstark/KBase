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
using System.Windows.Forms;
using Kbase.Serialization;
using System.Diagnostics;
using System.Collections.Generic;

namespace Kbase.SnippetTreeView
{
	/// <summary>
	/// Summary description for LocalClipboard.
	/// </summary>
	public class LocalClipboard : ArrayList
	{
		public bool IsMove = false;
		public DateTime Version = DateTime.Now;

		public LocalClipboard(IList nodes, bool isMove) : base(nodes)
		{
			this.IsMove = isMove;
		}

        public void CopyToWindowsClipboard()
        {
			DataObject dataObj = new DataObject();
            dataObj.SetData(GetSerializablePiece());
			Clipboard.SetDataObject(dataObj, false);

            // test before returning
            object test = GetFromWindowsClipboard();
            if (test == null)
                throw new NullReferenceException("aha!");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>null if data isn't in this format</returns>
		public SerializableUniverse GetFromWindowsClipboard() {
			IDataObject dataBlob = Clipboard.GetDataObject();
			return GetFromWindowsClipboard(dataBlob);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>null if data isn't in this format</returns>
		public static SerializableUniverse GetFromWindowsClipboard(IDataObject dataBlob) 
		{
			SerializableUniverse retVal = null;
			if (dataBlob.GetDataPresent(typeof(SerializableUniverse))) 
			{
                // someday, we might worry about this 
                // System.Runtime.InteropServices.COMException 
                // but not now
                retVal = (SerializableUniverse)dataBlob.GetData(typeof(SerializableUniverse));
			}
			return retVal;
		}



        public SerializableUniverse GetSerializablePiece() 
		{
			List<Kbase.Model.Snippet> snippets = new List<Kbase.Model.Snippet>(this.Count);
			foreach (SnippetTNode node in this) {
				snippets.Add(node.Snippet);
			}
            SerializableUniverse serializable = new SerializableUniverse(snippets);
			serializable.Version = Version;
			return serializable;
		}
	}
}
