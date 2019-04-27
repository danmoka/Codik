using System.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodikSite.Algorithms
{
    /// <summary>
    /// Arithmetic coding
    /// </summary>
    public class ArithmeticCoding : ITextEncodingAlgorithm
    {
        private Dictionary<char, int> dictionary;
        private List<char> letters;
        private List<double> frequencies;
        private double Left, Right;
        private StringBuilder output;

        public ArithmeticCoding()
        {
            dictionary = new Dictionary<char, int>();
            letters = new List<char>();
            frequencies = new List<double>();
            Left = 0;
            Right = 1;
        }
        public string Encode(string Enter, out double compressionRatio)
        {
            Left = 0;
            Right = 1;
            output = new StringBuilder();
            if (dictionary.Count == 0)
                CreateDictionary(Enter);
            int count = Enter.Length;
            foreach (char letter in Enter)
            {
                int index = letters.IndexOf(letter);
                double range = Right - Left;
                Right = Left + range * frequencies[index];
                if (index != 0)
                {
                    Left += range * frequencies[index - 1];
                }
                if ((int)Math.Round(Left * 100, 0) == (int)Math.Round(Right * 100, 0))
                {
                    ChangeBorders(ref output);
                }
            }
            var mid = new StringBuilder();
            mid.Append((Left + (Right - Left) / 2));
            mid.Remove(0, 2);
            output.Append(mid);
            output.Append(",");
            output.Append(Enter.Length);
            output.Append(",");
            output.Append(GetFrequencyDictionary());

            compressionRatio = 1.1; // это коэф сжатия

            return output.ToString();
        }
        public string Decode(string Enter)
        {
            SetFrequencyDictionary(Enter);
            var str = Enter.Split(',');
            int Count = int.Parse(str[1]);
            output = new StringBuilder();
            var entd = new StringBuilder("0,");
            entd.Append(str[0]);
            double enter = Convert.ToDouble(entd.ToString());
            Left = 0;
            Right = 1;
            for (int i = 0; i < Count; i++)
            {
                int index = 0;


                index = BinarySearch_Rec(enter, 0, frequencies.Count - 1, Right - Left);
                output.Append(letters[index]);

                if ((int)Math.Round(Left * 100, 0) == (int)Math.Round(Right * 100, 0))
                {
                    ChangeBordersDecode(ref entd);
                    enter = Convert.ToDouble(entd.ToString());
                }

            }

            return output.ToString();

        }
        private int BinarySearch_Rec(double key, int left, int right, double range)
        {
            int mid = left + (right - left) / 2;
            double d = Left + (double)(range * frequencies[mid]);
            if (left == right)
            {
                Right = Left + range * frequencies[mid];
                if (mid != 0)
                {
                    Left += range * frequencies[mid - 1];
                }
                return mid;
            }
            else if (d > key)
                return BinarySearch_Rec(key, left, mid, range);
            else
                return BinarySearch_Rec(key, mid + 1, right, range);
        }
        public string GetFrequencyDictionary()
        {
            StringBuilder output = new StringBuilder();
            int count = letters.Count;

            output.Append('{');
            foreach (var ch in dictionary)
            {
                output.Append('[');
                output.Append(ch.Key);
                output.Append('-');
                output.Append(ch.Value);
                output.Append(']');

            }
            output.Append('}');
            return output.ToString();
        }
        public void SetFrequencyDictionary(string Dictionary)
        {
            letters = new List<char>();
            frequencies = new List<double>();
            dictionary = new Dictionary<char, int>();
            int n = 0;

            var codedTextPattern = @"\{(\[(.|\n)\-\d+\])+\}\z";
            var matchAllCodes = Regex.Match(Dictionary, codedTextPattern);
            if (matchAllCodes.Success)
            {
                var singleCodePattern = @"\[(.|\n)\-\d+\]";

                foreach (Match matchCode in Regex.Matches(matchAllCodes.Value, singleCodePattern))
                {
                    var code = int.Parse(matchCode.Value.Substring(3, matchCode.Value.Length - 4));
                    n += code;
                    dictionary.Add(matchCode.Value[1], code);
                }
            }
            else
            {
                throw new ArgumentException();
            }


            //string mask = @"[(.|\n)\-\d+]";
            //var matches = Regex.Matches(Dictionary, mask);
            //foreach (Match match in matches)
            //{
            //    int freq;

            //    string[] value = match.Value.Split(':');
            //    char letter;
            //    if (value[0] == "")
            //    {
            //        letter = ':';
            //        freq = Convert.ToInt32(value[2]);
            //    }
            //    else
            //    {
            //        letter = (value[0][0]);

            //        freq = Convert.ToInt32(value[1]);
            //    }
            //    n += freq;
            //    dictionary.Add(letter, freq);
            //}

            CreateLine(n);
        }
        private void CreateDictionary(string Enter)
        {
            int n = Enter.Length;

            for (int i = 0; i < n; i++)
            {
                if (dictionary.ContainsKey(Enter[i]))
                {
                    dictionary[Enter[i]]++;
                }
                else
                {
                    dictionary.Add((Enter[i]), 1);
                }
            }
            CreateLine(n);
        }
        private void CreateLine(int n)
        {
            var sorteddic = dictionary.OrderByDescending(s => s.Value);
            double border = 0;
            foreach (var pair in sorteddic)
            {
                letters.Add(pair.Key);
                border += (double)pair.Value / n;
                frequencies.Add(border);
            }
        }
        private void ChangeBorders(ref StringBuilder ou)
        {
            var cl = new StringBuilder();
            cl.Append(Left);
            var cr = new StringBuilder();
            cr.Append(Right);

            int n = Math.Min(cl.Length, cr.Length);
            for (int i = 2; i < n; i++)
            {
                if (cl[i] == cr[i])
                {
                    ou.Append(cl[i]);
                }
                else
                {
                    cl.Remove(2, i - 2);
                    cr.Remove(2, i - 2);
                    break;
                }
            }
            Left = Convert.ToDouble(cl.ToString());
            Right = Convert.ToDouble(cr.ToString());

        }
        private void ChangeBordersDecode(ref StringBuilder stringBuilder)
        {
            var cl = new StringBuilder();
            cl.Append(Left);
            var cr = new StringBuilder();
            cr.Append(Right);
            int n = Math.Min(cl.Length, cr.Length);
            for (int i = 2; i < n; i++)
            {
                if (cl[i] != cr[i])
                {
                    cl.Remove(2, i - 2);
                    cr.Remove(2, i - 2);
                    stringBuilder.Remove(2, i - 2);
                    break;
                }
            }
            Left = Convert.ToDouble(cl.ToString());
            Right = Convert.ToDouble(cr.ToString());
        }
    }
}