using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CodikSite.Algorithms
{
    /// <summary>
    /// LZ78
    /// </summary>
    public class LZ78 : ITextEncodingAlgorithm
    {
        public string Encode(string sourceText, out double compressionRatio)
        {
            var dictionary = new Dictionary<string, int>
            {
                { string.Empty, 0 }
            };

            var answer = new StringBuilder();
            var buffer = string.Empty;
            int blockCounter = 0;

            foreach (var symbol in sourceText)
            {
                if (dictionary.ContainsKey(buffer + symbol))
                {
                    buffer += symbol;
                }
                else
                {
                    answer.Append(string.Format("({0},{1})", dictionary[buffer], symbol));
                    blockCounter++;
                    dictionary.Add(buffer + symbol, dictionary.Count);
                    buffer = string.Empty;
                }
            }

            answer.Append(string.Format("({0},{1})", dictionary[buffer], '$'));
            blockCounter++;

            double charSize = BitHacks.GetRealSizeForNumber(char.MaxValue);
            compressionRatio = (sourceText.Length * charSize) /
                (blockCounter * (charSize + BitHacks.GetRealSizeForNumber((uint)dictionary.Count)));

            return answer.ToString();
        }

        public string Decode(string codedText)
        {
            var codedTextPattern = @"\A(\(\d+,(.|\n)\))+\z";
            if (Regex.Match(codedText, codedTextPattern).Success)
            {
                var singleCodePattern = @"\(\d+,(.|\n)\)";
                var answer = new StringBuilder(codedText.Length);
                var dictionary = new List<string>
                {
                    string.Empty
                };

                foreach (Match match in Regex.Matches(codedText, singleCodePattern))
                {
                    var position = int.Parse(match.Value.Substring(1, match.Value.Length - 4));
                    var symbol = match.Value[match.Value.Length - 2];
                    string word = string.Format("{0}{1}", dictionary[position], symbol);
                    answer.Append(word);
                    dictionary.Add(word);
                }

                answer.Remove(answer.Length - 1, 1);
                return answer.ToString();
            }
            else throw new ArgumentException();
        }
    }

}