using System;
using System.Collections.Generic;
using System.Text;

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
namespace Kbase.Model.Search
{

    /// <summary>
    /// We use these strict values so that we can do searches and not worry about whether it's text or title or just text
    /// see the matches methods in the snippet if you want to see where this works.
    /// NOTE: Use the SearchCriterionConverter to get these back and forth to strings!
    /// </summary>
    public enum SearchTypeTextTitle
    {
        Title = 1,
        Text = 2,
        Text_or_Title = Title | Text,
        Id = 4,
        Icon = 8,
        Created = 16,
        Modified = 32
    }


    public enum SearchTypeIsContains
    { // before on after for dates, of course
        Contains,
        Is,
        Before,
        After
    }

    public enum SearchTypeWhere
    {
        Own = 1,
        Parent = 2,
        Ancestor = 4,
        Ancestor_or_Own = Ancestor | Own,
        Parent_or_Own = Parent | Own
    }

    public enum SearchTypeConcat
    {
        And,
        Or,
        Not,
        None
    }


}
