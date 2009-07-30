using System;
using System.Collections.Generic;
using System.Text;
using Kbase.Model;

namespace Kbase.ModelInMemory 
{
    class ComprarerCreateDate : Comparer<SnippetInMemory>
    {
        public override int Compare(SnippetInMemory x, SnippetInMemory y)
        {
            return x.Created.CompareTo(y.Created);
        }
    }
}
