using System.Collections.Generic;
using System.Text;
using System;

namespace CodikSite.Algorithms
{
    static class IDictionaryExtensions
    {
        public static string ToSrtingExtension<TKey, TValue>(this IDictionary<TKey,TValue> dictionary)
        {           
            if (dictionary != null)
            {
                var representation = new StringBuilder();
                representation.Append('{');

                foreach (var pair in dictionary)
                {
                    representation.Append(string.Format("[{0}-{1}]", pair.Key, pair.Value));
                }

                representation.Append('}');
                if (representation.Length == 2)
                {
                    representation.Clear();
                }
                return representation.ToString();
            }
            else
            {
                throw new ArgumentNullException();
            }  
        }

    }
}