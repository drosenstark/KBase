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
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Kbase.Model;

namespace Kbase.DetailPanel
{
	/// <summary>
	/// The conversion engine for plain text to Rtf and vice versa for the detail pane.
    /// Kbase files store only plain text and bullet with - and do indents with tab
	/// </summary>
	public class RtfConverter : RichTextBox
	{

		const char LINE_BREAK = '\n';
		internal const int LINE_BREAK_LENGTH = 1;
		protected const string BULLET_TEXT = "- ";

		// IndentPerTab must be divisible by spaces per tab!
		public const int IndentPerTab = 20;
		public const int SpacesPerTab = 5;
		public const int SpacesPerTabPerIndentPerTab = 4;


		/// <summary>
		/// NOTE:
		/// this assumes that "this" is a new one and doesn't conflict with 
		/// other rtfConverters (always create a new).
		/// </summary>
		/// <param name="snippet"></param>
		/// <param name="rtf"></param>
		public static string GetTextFromRtf(string rtf) 
		{
			try 
			{
				string retVal;
				Cursor.Current = Cursors.WaitCursor;
				RtfConverter converter = new RtfConverter();
				converter.Rtf = rtf;
				retVal = converter.Serialize(0,converter.Text.Length);
				return retVal;
			} 
			finally 
			{
				Cursor.Current = Cursors.Default;
			}
		}

		public static string Serialize(string rtf, int start, int length) 
		{
			try 
			{
				Cursor.Current = Cursors.WaitCursor;
				RtfConverter converter = new RtfConverter();
				converter.Rtf = rtf;
				return converter.Serialize(start, length);
			} 
			finally 
			{
				Cursor.Current = Cursors.Default;
			}
		}

		
		// this is obviously not ready for big strings which will be spooled to 
		// disk, for instance.  All in memory.
		// Client must save the SelectionStart and SelectionLength and restore them after use.
		private string Serialize(int start, int length) 
		{
			if (Text.Length == 0)
				return "";
			StringBuilder sBuilder = new StringBuilder();

			int firstLine = GetLine(start);
			int lastLine = GetLine(start + length);
			bool indentFirstLine = IsLineBeginning(start);
			int charactersEaten = 0;
			for (int i=firstLine;i<=lastLine; i++) 
			{
				Select(i);
				if (i > firstLine || indentFirstLine) 
				{
					sBuilder.Append(GetIndentText(SelectionIndent, SelectionBullet));
				}
				string textToAppend;
				int startLine = 0;
				// the first line is partial and must be measured
				if (i == firstLine) 
				{
					int charactersInPreviousLines = 0;
					for (int c=0; c<i; c++) 
					{
						charactersInPreviousLines += Lines[c].Length + LINE_BREAK_LENGTH;
					}
					startLine = start - charactersInPreviousLines;
				}
				int lengthLine;
				// like the last line, in fact
				if (i == lastLine) 
					lengthLine  = length - charactersEaten;					
				else
					lengthLine = Lines[i].Length - startLine;

				textToAppend = Lines[i].Substring(startLine,lengthLine);					
				charactersEaten += textToAppend.Length + LINE_BREAK_LENGTH;
				sBuilder.Append(textToAppend);
				if (i < lastLine) 
					sBuilder.Append(LINE_BREAK);
			}
			return sBuilder.ToString();
		}

		public static string FormatClipboard(System.Drawing.Font font) 
		{
			RtfConverter converter = new RtfConverter();
			converter.Font = font;
			converter.PasteAndFormat();
			Clipboard.SetDataObject(new DataObject(DataFormats.Rtf, converter.Rtf));
			return converter.Rtf;
		} 
		
		public static string LoadAndCopyToClipboard(string text, System.Drawing.Font font) 
		{
			RtfConverter converter = new RtfConverter();
			converter.Font = font;
			converter.Load(text);
			Clipboard.SetDataObject(new DataObject(DataFormats.Rtf, converter.Rtf));
			return converter.Rtf;
		} 

		public static string Load(string text, System.Drawing.Font font) 
		{
			RtfConverter converter = new RtfConverter();
			converter.Font = font;
			return converter.Load(text);
		} 
		
		void PasteAndFormat() 
		{
			Paste();
			Select(0, Text.Length + 1);
			SelectionFont = this.Font;
			SelectionColor = System.Drawing.Color.Black;
			SelectionAlignment = HorizontalAlignment.Left;
			SelectionCharOffset = 0;
			SelectionRightIndent = 0;
			RightMargin = 0;
		}
		
		string Load(string text) 
		{
			try 
			{
				Cursor.Current = Cursors.WaitCursor;
				int startingLine = 0;
				SelectionIndent = 0;
				SelectionBullet = false;
				SelectionHangingIndent = 0;
				if (text.Length == 0)
					return "";
				Line[] lines = null;
				string[] rawTextLines = text.Split(LINE_BREAK);
				lines = new Line[rawTextLines.Length];
				StringBuilder textBuilder = new StringBuilder();
				for (int i=0; i<lines.Length; i++) 
				{
					Line line = lines[i] = new Line(rawTextLines[i]);
					line.RemoveTrailing("\r");
					textBuilder.Append(line.GetTextClean());
					if (i < lines.Length - 1) 
						textBuilder.Append(LINE_BREAK);
				}
				Text = textBuilder.ToString();

				// This is separate because the Text additions would wipe out all 
				// formatting (string is immutable).
				for (int i=0; i<lines.Length; i++) 
				{
					Select(i+startingLine);
					SelectionIndent = lines[i].GetIndent();
					if (lines[i].IsBulleted()) 
					{
						SelectionBullet = true;
						SelectionHangingIndent = IndentPerTab;					}
						
				}
				return Rtf;
			}
			finally 
			{
				Cursor.Current = Cursors.Default;
			}
		}

		void Select(int line) 
		{
			Select(GetStartOfLine(line),Lines[line].Length + LINE_BREAK_LENGTH);
		}


		bool IsLineBeginning(int character) 
		{
			bool lineBeginning = true;

			// the logic to check the oldSelection and make 
			// sure we can change the first line indent based on the spaces
			if (character == 0 || Text.Length == 0)
				lineBeginning = true;
			else if (!Text.Substring(character-1,1).Equals(LINE_BREAK))
				lineBeginning= false;
			return lineBeginning;
		}		

		private LineMetric[] lineInfos = null;

		LineMetric[] LineInfos 
		{
			get 
			{
				if (lineInfos == null) 
				{
					lineInfos = new LineMetric[Lines.Length];
					int whereAreWe = 0;
					for (int i=0; i<Lines.Length; i++) 
					{
						lineInfos[i].DistanceFromStartOfText = whereAreWe;
						whereAreWe += Lines[i].Length;
						lineInfos[i].End = whereAreWe;
						whereAreWe += LINE_BREAK_LENGTH;
					}
				}	
				return lineInfos;
			}
		}

		int GetStartOfLine(int line) 
		{
			if (line > Lines.Length-1)
				throw new Exception("out of range for line selection.");
			return LineInfos[line].DistanceFromStartOfText;
		}


	
		int GetLine(int cursorPosition) 
		{
			// it would be inefficient to use the LineInfos but we know that we'll
			// be using them again for something else so we just build them all.
			for (int i=0; i<LineInfos.Length; i++) 
			{
				if (cursorPosition <= LineInfos[i].End)
					return i;
			}
			// this case can actually be iffed out: if there's no text or something.
			throw new IndexOutOfRangeException("what?");;
		}

		const char SPACE = ' ';
		const char TAB = '\t';
		public string GetIndentText(int indent, bool isBulleted) 
		{
			StringBuilder sBuilder = new StringBuilder();
			int tabs = indent/IndentPerTab;
			int spaces = (indent % IndentPerTab) / SpacesPerTabPerIndentPerTab;
			for (int i = 0; i<tabs; i++) 
			{
				sBuilder.Append(TAB);
			}
			for (int i = 0; i<spaces; i++) 
			{
				sBuilder.Append(SPACE);
			}
			if (isBulleted) 
				sBuilder.Append(BULLET_TEXT);
			return sBuilder.ToString();
		}



	}

	public struct LineMetric 
	{
		public int DistanceFromStartOfText;
		public int End;
	}

	/// <summary>
	/// Line encapsulated information about a Line to help with indenting.  
	/// Note that a line in the RTF is actually a paragraph.
	/// </summary>
	public class Line 
	{
		internal static Regex regEx = new Regex("(?<"+REGEX_TABS+">[\\t]*)(?<"+REGEX_SPACES+"> *)(?<"+REGEX_BULLET+">- )?(?<"+REGEX_OTHER+">.*)");
		internal const string REGEX_SPACES = "spaces";
		internal const string REGEX_TABS = "tabs";
		internal const string REGEX_BULLET = "bullet";
		internal const string REGEX_OTHER = "other";

		private string Text;
		Match matchAllInternal = null;

		public Match MatchAll 
		{
			get 
			{
				if (matchAllInternal == null)
					matchAllInternal = regEx.Match(Text);
				return matchAllInternal;
			}			
		}

		public Line(string text) 
		{
			this.Text = text;
		}		

		public bool IsBulleted() 
		{
			bool retVal = (MatchAll.Groups[REGEX_BULLET].Length > 0);
			return retVal;
		}

		// returns the indent needed for the line
		public int GetIndent() 
		{
			int spaces = MatchAll.Groups[REGEX_SPACES].Length;
			int tabs = MatchAll.Groups[REGEX_TABS].Length;
			return GetIndent(spaces, tabs);
		}			

		private int GetIndent(int spaces, int tabs) 
		{
			int indent = (spaces / RtfConverter.SpacesPerTab) * RtfConverter.IndentPerTab;
			indent += (spaces % RtfConverter.SpacesPerTab) * (RtfConverter.SpacesPerTabPerIndentPerTab);
			indent += tabs * RtfConverter.IndentPerTab;
			return indent;
		}
	

		public int GetLength(bool includeLineBreak) 
		{
			int retVal = MatchAll.Groups[REGEX_OTHER].Length;
			if (includeLineBreak)	
				retVal += RtfConverter.LINE_BREAK_LENGTH;
			return retVal;
		}
	
		public string GetTextClean() 
		{
			string retVal = MatchAll.Groups[REGEX_OTHER].ToString();
			return retVal;
		}

		public void RemoveTrailing(string character) 
		{
			if (Text.Length > 0 && Text.Substring(Text.Length-1,1) == character)
				Text = Text.Remove(Text.Length - 1,1);
		}
	}
}
