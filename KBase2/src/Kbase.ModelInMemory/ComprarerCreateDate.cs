using System;
using System.Collections.Generic;
using System.Text;
using Kbase.Model;

namespace Kbase.ModelInMemory 
{
    class ComprarerModifiedDate : Comparer<SnippetInMemory>
    {
        public override int Compare(SnippetInMemory x, SnippetInMemory y)
        {
            return x.Modified.CompareTo(y.Modified);
        }
    }
}
