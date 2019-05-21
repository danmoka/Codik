using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodikSite.Algorithms
{
    public class ArithmeticCoding : ITextEncodingAlgorithm
    {
        private struct RationalNumber
        {
            public BigInteger Numerator { get; private set; }
            public BigInteger Denominator { get; private set; }

            public RationalNumber(BigInteger numerator, BigInteger denominator)
            {
                if (denominator.IsZero)
                {
                    throw new ArgumentOutOfRangeException();
                }
                var gcd = BigInteger.GreatestCommonDivisor(numerator, denominator);
                this.Numerator = numerator / gcd;
                this.Denominator = denominator / gcd;

                if (Denominator < 0)
                {
                    Numerator *= -1;
                    Denominator *= -1;
                }
            }

            public static RationalNumber operator +(RationalNumber first, RationalNumber second)
            {
                return new RationalNumber(first.Numerator * second.Denominator +
                    first.Denominator * second.Numerator, first.Denominator * second.Denominator);
            }

            public static RationalNumber operator -(RationalNumber first, RationalNumber second)
            {
                return new RationalNumber(first.Numerator * second.Denominator -
                    first.Denominator * second.Numerator, first.Denominator * second.Denominator);
            }

            public static RationalNumber operator *(RationalNumber first, RationalNumber second)
            {
                return new RationalNumber(first.Numerator * second.Numerator,
                    first.Denominator * second.Denominator);
            }

            public static RationalNumber operator /(RationalNumber first, RationalNumber second)
            {
                return new RationalNumber(first.Numerator * second.Denominator,
                    first.Denominator * second.Numerator);
            }

            public static bool operator <(RationalNumber first, RationalNumber second)
            {
                return (first.Numerator * second.Denominator -
                    first.Denominator * second.Numerator) < 0;
            }

            public static bool operator >(RationalNumber first, RationalNumber second)
            {
                return (first.Numerator * second.Denominator -
                    first.Denominator * second.Numerator) > 0;
            }

            public static bool operator <=(RationalNumber first, RationalNumber second)
            {
                return (first.Numerator * second.Denominator -
                    first.Denominator * second.Numerator) <= 0;
            }

            public static bool operator >=(RationalNumber first, RationalNumber second)
            {
                return (first.Numerator * second.Denominator -
                    first.Denominator * second.Numerator) >= 0;
            }

            public static implicit operator RationalNumber(BigInteger number)
            {
                return new RationalNumber(number, 1);
            }

            public static implicit operator RationalNumber(int number)
            {
                return new RationalNumber(number, 1);
            }

            public override string ToString()
            {
                return string.Format("{0}/{1}", this.Numerator, this.Denominator);
            }
        }

        private Dictionary<char, int> CountEachCharacter(IEnumerable<char> characters)
        {
            var dictionary = new Dictionary<char, int>();

            foreach (var symbol in characters)
            {
                if (dictionary.ContainsKey(symbol))
                {
                    dictionary[symbol]++;
                }
                else
                {
                    dictionary.Add(symbol, 1);
                }
            }

            return dictionary;
        }
        
        private Dictionary<char, int> GetFrequencyDictionary(string codedText,
            out int dictionaryIndex, out int numberCharacters)
        {
            if (Regex.IsMatch(codedText, @"\A\d+\{(\[(.|\n)\-\d+\])+\}\z"))
            {
                numberCharacters = 0;
                dictionaryIndex = Regex.Match(codedText, @"\{(\[(.|\n)\-\d+\])+\}\z").Index;
                var keyValuePairPattern = @"\[(.|\n)\-\d+\]";
                var dictionary = new Dictionary<char, int>();

                foreach (Match keyValueMatch in Regex.Matches(codedText.Substring(dictionaryIndex),
                    keyValuePairPattern))
                {
                    var number = int.Parse(keyValueMatch.Value.Substring(3, keyValueMatch.Value.Length - 4));
                    numberCharacters += number;
                    dictionary.Add(keyValueMatch.Value[1], number);
                }

                return dictionary;
            }
            else
            {
                throw new ArgumentException();
            }           
        }
       
        private int GetFirstDigitAfterDot(RationalNumber number)
        {
            return (int)(10 * number.Numerator / number.Denominator);
        }

        private void DeleteSameDigits(ref RationalNumber left, ref RationalNumber right,
            StringBuilder code)
        {
            int digit = GetFirstDigitAfterDot(left);

            while (digit == GetFirstDigitAfterDot(right))
            {
                code.Append(digit);
                left = left * 10 - digit;
                right = right * 10 - digit;
                digit = GetFirstDigitAfterDot(left);
            }

        }

        public string Encode(string sourceText, out double compressionRatio)
        {
            var frequencies = CountEachCharacter(sourceText);
            var probabilities = new List<RationalNumber>(frequencies.Count + 1)
            {
                0
            };
            var characterIndexes = new Dictionary<char, int>(frequencies.Count);

            foreach (var pair in from item in frequencies orderby (int)item.Key select item)
            {
                characterIndexes.Add(pair.Key, probabilities.Count);
                probabilities.Add(new RationalNumber(pair.Value, sourceText.Length)
                    + probabilities[probabilities.Count - 1]);
            }

            var code = new StringBuilder();
            RationalNumber left = 0, right = 1;

            foreach (var symbol in sourceText)
            {
                var index = characterIndexes[symbol];
                var range = right - left;
                right = left + range * probabilities[index];
                left = left + range * probabilities[index-1];
                DeleteSameDigits(ref left, ref right, code);
            }

            code.Append(GetFirstDigitAfterDot(left) + 1);
            compressionRatio = (double)sourceText.Length * 4 / code.Length;
            code.Append(frequencies.ToSrtingExtension());
            return code.ToString();
        }

        public string Decode(string codedText)
        {
            var sourceText = new StringBuilder();
            var probabilities = new List<RationalNumber> { 0 };
            var indexCharacters = new Dictionary<int, char>();
            int dictionaryIndex, numberCharacters;

            foreach (var pair in from item in GetFrequencyDictionary(codedText,
                out dictionaryIndex, out numberCharacters) orderby (int)item.Key select item)
            {
                indexCharacters.Add(probabilities.Count, pair.Key);
                probabilities.Add(new RationalNumber(pair.Value, numberCharacters)
                    + probabilities[probabilities.Count - 1]);
            }

            RationalNumber code = new RationalNumber(BigInteger.Parse(codedText.Substring(0, dictionaryIndex)),
                BigInteger.Pow(10, dictionaryIndex));

            while (sourceText.Length < numberCharacters)
            {
                int index = probabilities.FindLastIndex(x => (x <= code)) + 1;
                sourceText.Append(indexCharacters[index]);
                code = (code - probabilities[index - 1])
                    / (probabilities[index] - probabilities[index - 1]);                     
            }

            return sourceText.ToString();
        }

    }
}