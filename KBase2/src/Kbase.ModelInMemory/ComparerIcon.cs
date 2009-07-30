using System;
using System.Collections.Generic;
using System.Text;
using Kbase.Model;

namespace Kbase.ModelInMemory
{
    class ComprarerIcon : Comparer<SnippetInMemory>
    {
        public override int Compare(SnippetInMemory x, SnippetInMemory y)
        {
            return x.Icon.CompareTo(y.Icon);
        }
    }
}
