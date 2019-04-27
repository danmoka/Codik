﻿using System;
using System.Text;
using System.Text.RegularExpressions;

namespace CodikSite.Algorithms
{
    /// <summary>
    /// Run-Length Encoding
    /// </summary>
    public class RLE : ITextEncodingAlgorithm
    {        
        public string Encode(string sourceText, out double compressionRatio)
        {
            var answer = new StringBuilder();
            int blockСounter, charCounter, maxCharCounter;
            blockСounter = charCounter = maxCharCounter = 1;

            for (int i = 1; i < sourceText.Length; i++)
            {
                if (sourceText[i - 1] == sourceText[i]) charCounter++;
                else
                {
                    answer.Append(string.Format("({0},{1})", sourceText[i - 1], charCounter));
                    if (charCounter > maxCharCounter) maxCharCounter = charCounter;
                    blockСounter++;
                    charCounter = 1;
                }
            }

            answer.Append(string.Format("({0},{1})", sourceText[sourceText.Length - 1], charCounter));

            double charSize = BitHacks.GetHighestBitPosition(char.MaxValue);
            compressionRatio = (sourceText.Length * charSize) /
                (blockСounter * (BitHacks.GetHighestBitPosition(maxCharCounter) + charSize));

            return answer.ToString();

        }

        public string Decode(string codedText)
        {
            var codedTextPattern = @"\A(\((.|\n),\d+\))+\z";
            if (Regex.Match(codedText, codedTextPattern).Success)
            {
                var singleCodePattern = @"\((.|\n),\d+\)";
                var answer = new StringBuilder(codedText.Length);

                foreach (Match match in Regex.Matches(codedText, singleCodePattern))
                {
                    int count = int.Parse(match.Value.Substring(3, match.Value.Length - 4));
                    answer.Append(match.Value[1], count);
                }

                return answer.ToString();
            }
            else throw new ArgumentException();
        }
    }
}