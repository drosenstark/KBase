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
using System.Diagnostics;
using System.IO;
using Kbase.SnippetTreeView;
using Kbase.Model;
using Kbase.MainFrm;



namespace Kbase.DetailPanel
{
	public class HyperlinkUtil
	{
		string relativeFileRoot = null;

		// file being edited is TheKBase being edited, whose directory 
		// which will
		// be the base for the relative links
		public HyperlinkUtil(string fileBeingEdited) 
		{
			if (fileBeingEdited == null)
				relativeFileRoot = "";
			else 
			{
				FileInfo file = new FileInfo(fileBeingEdited);
				relativeFileRoot = file.DirectoryName.Replace(@"\","/");			
			}


		}

		public void Open(string path) 
		{
			if (path.StartsWith("file://this#")) 
			{
				OpenSnippet(path);
				return;
			}
			// fix the backslashes, we don't accept 'em
			path = System.Web.HttpUtility.UrlDecode(path);   //path.Replace("%20"," ");
			path = path.Replace(@"\","/");

			if (path.Substring(0,schemeLength).Equals(schemeFile))
				OpenFile(path);
			else
				Launch(path);
		}

		static int schemeLength = "file://".Length;
		const string schemeFile = "file://";
		const string SNIPPET_LOCATOR = "#";

		private void OpenSnippet(string path) {
			int endOfEquals = path.IndexOf(SNIPPET_LOCATOR);
			if (endOfEquals == -1)
				return;
			endOfEquals += SNIPPET_LOCATOR.Length;
			int snippetId = Convert.ToInt16(path.Substring(endOfEquals));
			Snippet snippet = Universe.Instance.ModelGateway.FindSnippet(snippetId);			
			if (snippet != null) 
			{
				SnippetInstance instance = snippet.UI.GetAnyInstance();
				instance.SelectExclusive();
			} 
			else {
                MainForm.ShowError("Snippet could not be found.");
			}
		}

		private void OpenFile(string path) 
		{

			// first, strip off the leading protocol://
			path = path.Substring(schemeLength,path.Length - schemeLength);
			string uncCandidate = path.Substring(0,2);
			bool isUNC =  uncCandidate.Equals("//");

			if (isUNC) 
			{
				// not sure about the handling for these
				// let them go for now
			} 
			else
			{
				if (!path.Substring(1,1).Equals(":"))
					path = Derelativize(path);
			}

			FileInfo fileInfo = new FileInfo(path);

			if (fileInfo.Exists)
				Launch(fileInfo.FullName);
			else
				throw new FileNotFoundException("File " + fileInfo.FullName + " not found.");
		}

		public string Derelativize(string path) 
		{
			string retVal = null;
			// we can do all kinds of fancy stuff here, but really the point is to 
			// smash it onto the basePath
			if (relativeFileRoot.Length > 0)
				retVal = relativeFileRoot + "/" + path;
			else
				retVal = path;
			retVal = retVal.Replace("//","/");
			return retVal;
		}

        /// <summary>
        /// Relativize a path so that it uses the FileBeingEdited as a comparison
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// this could definitely be optimized or simplified, but it does work 
		public string Relativize(string path) 
		{
			// fix it
			path = path.Replace(@"\","/");
			// is there a relative path?  If not, return.
			if (relativeFileRoot.Length == 0)
				return path;
			// do they have the same drive? If not, return.
			if (!path.Substring(0,2).ToLower().Equals(relativeFileRoot.Substring(0,2).ToLower()))
				return path;
			
			// pull off the drive letters
			path = path.Substring(2,path.Length - 2);
			string compareTo = relativeFileRoot.Substring(2,relativeFileRoot.Length - 2);

			string[] piecesOfPath = path.Split("/".ToCharArray());
			string[] piecesOfCompareTo;
			if (compareTo.Equals("/")) 
			{
				// this is due to the weirdness of the split: it would normally produce TWO which is inconsistent
				piecesOfCompareTo = new string[1];
				piecesOfCompareTo[0] = "";
			} 
			else 
				piecesOfCompareTo = compareTo.Split("/".ToCharArray());
			
			System.Text.StringBuilder builder = new System.Text.StringBuilder(path.Length);

			int piecesOfPathAccountedFor = 0;
			for (int i=0; i<piecesOfCompareTo.Length; i++) 
			{
				if (i < piecesOfPath.Length) 
				{
					if (!piecesOfCompareTo[i].Equals(piecesOfPath[i]))
						builder.Append("../");
					else 
						piecesOfPathAccountedFor++;
				} 
				else
					builder.Append("../");

			}
			for (int i = piecesOfPathAccountedFor; i<piecesOfPath.Length; i++) 
			{
				builder.Append(piecesOfPath[i]);
				if (i<piecesOfPath.Length - 1)
					builder.Append("/");
			}
			return builder.ToString();
		}

			
		
		

		/// <summary>
		/// Will launch an exception up if there is a problem.
		/// </summary>
		/// <param name="path"></param>
		private void Launch(string path) 
		{
			System.Diagnostics.Process.Start(path);
		}
	}
}
