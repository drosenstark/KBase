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

namespace Kbase.Model
{
	/// <summary>
	/// Summary description for IllegalMoveOrCopyException.
	/// </summary>
	public class IllegalMoveOrCopyException : Exception
	{
		public IllegalMoveOrCopyException(Snippet who, Snippet movingOrCopyingTo, string why) : base ("Cannot move or copy snippet " + 
			who + " to snippet " + movingOrCopyingTo + ". Reason: " + why + ".")
		{
		}
	}


	public class NewChildIsAncestorException : IllegalMoveOrCopyException 
	{
		static string MESSAGE = "New parent is an ancestor.";

		public NewChildIsAncestorException (Snippet who, Snippet movingOrCopyingTo) :
			base(who, movingOrCopyingTo,MESSAGE) 
		{
		
		} 
		
	}



}
