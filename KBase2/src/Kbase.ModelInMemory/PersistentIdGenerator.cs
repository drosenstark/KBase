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

namespace Kbase.ModelInMemory
{
	/// <summary>
	/// Primary key generator for snippets, if you will. 
	/// </summary>
	public class PersistentIdGenerator
	{
		int id = 0;
		public PersistentIdGenerator()
		{
		}

		public int GetId() {
			System.Threading.Monitor.Enter(this);
			int retVal = ++id;			
			System.Threading.Monitor.Exit(this);
			return retVal;
		}

		public void InsistOnNewMax(int newMax) {
			// this could be problematic if one thread is insisting on a new max
			// and another one is getting a new id, but at least we cannot have problems
			// with either one individually
			System.Threading.Monitor.Enter(this);
			if (newMax > id)
				id = newMax;			
			System.Threading.Monitor.Exit(this);
		}

		public void Reset() 
		{
			// this could be problematic if one thread is insisting on a new max
			// and another one is getting a new id, but at least we cannot have problems
			// with either one individually
			System.Threading.Monitor.Enter(this);
			id = 1; // reset to 1 and not 0 because the topLevelSnippet gets 1
            // as its ID.  This is a little hack to avoid problems.
			System.Threading.Monitor.Exit(this);
		}
	
	}
}
