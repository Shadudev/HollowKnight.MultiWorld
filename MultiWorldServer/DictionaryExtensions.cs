using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiWorldServer
{
    static class DictionaryExtensions
    {
        // Kind of like python defaultdicts, I got annoyed with checking if an item existed before adding to it
        public static U GetOrCreateDefault<T, U>(this Dictionary<T, U> dict, T key) where U : new()
        {
            if (dict.ContainsKey(key)) return dict[key];
            
            dict[key] = new U();
            return dict[key];

        }
    }
}
