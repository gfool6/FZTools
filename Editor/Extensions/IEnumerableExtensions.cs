using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace FZTools
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> SplitChunk<T>(this IEnumerable<T> ie, int chunk)
        {
            return ie.Select((v, i) => (v, i)).GroupBy(t => t.i / chunk).Select(t => t.Select(tt => tt.v));
        }
    }
}