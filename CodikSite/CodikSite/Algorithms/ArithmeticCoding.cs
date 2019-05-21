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
                return (first - second).Numerator < 0;
            }

            public static bool operator >(RationalNumber first, RationalNumber second)
            {
                return (first - second).Numerator > 0;
            }

            public static bool operator <=(RationalNumber first, RationalNumber second)
            {
                return (first - second).Numerator <= 0;
            }

            public static bool operator >=(RationalNumber first, RationalNumber second)
            {
                return (first - second).Numerator >= 0;
            }

            public static implicit operator RationalNumber(BigInteger x)
            {
                return new RationalNumber(x, 1);
            }

            public static implicit operator RationalNumber(int x)
            {
                return new RationalNumber(x, 1);
            }

            public override string ToString()
            {
                return string.Format("{0}/{1}", this.Numerator.ToString(), this.Denominator.ToString());
            }
        }

        private struct Border
        {
            public RationalNumber Left { get; private set; }
            public RationalNumber Right { get; private set; }

            public Border(RationalNumber left, RationalNumber right)
            {
                this.Left = left;
                this.Right = right;
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

        private string ToStringFrequencyDictionary(Dictionary<char, int> frequencies)
        {
            var code = new StringBuilder();
            code.Append('{');

            foreach (var pair in frequencies)
            {
                code.Append(string.Format("[{0}-{1}]", pair.Key, pair.Value));
            }

            code.Append('}');
            return code.ToString();
        }

        private Dictionary<char, int> GetFrequencyDictionary(string codedText,
            out int dictionaryIndex, out int numberCharacters)
        {
            numberCharacters = 0;
            var frequencyDictionaryPattern = @"\{(\[(.|\n)\-\d+\])+\}\z";
            var dictionaryMatch = Regex.Match(codedText, frequencyDictionaryPattern);
            if (dictionaryMatch.Success)
            {
                dictionaryIndex = dictionaryMatch.Index;
                var keyValuePairPattern = @"\[(.|\n)\-\d+\]";
                var dictionary = new Dictionary<char, int>();

                foreach (Match keyValueMatch in Regex.Matches(dictionaryMatch.Value, keyValuePairPattern))
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

        private Dictionary<char, Border> GetBorders(Dictionary<char, int> frequencies, int numberCharacters)
        {
            var borders = new Dictionary<char, Border>(frequencies.Count);
            int sum = 0;

            foreach (var elem in from item in frequencies orderby item.Key select item)
            {
                var left = sum;
                sum += elem.Value;
                borders.Add(elem.Key, new Border(new RationalNumber(left, numberCharacters),
                    new RationalNumber(sum, numberCharacters)));
            }

            return borders;
        }

        private int GetFirstDigitAfterDot(RationalNumber number)
        {
            return (int)(10 * number.Numerator / number.Denominator);
        }

        private IEnumerable<int> DeleteSameDigit(ref RationalNumber left, ref RationalNumber right)
        {
            var someDigits = new List<int>();
            int digit = GetFirstDigitAfterDot(left);

            while (digit == GetFirstDigitAfterDot(right))
            {
                someDigits.Add(digit);
                left = left * 10 - digit;
                right = right * 10 - digit;
            }

            return someDigits;
        }

        public string Encode(string sourceText, out double compressionRatio)
        {
            var frequencies = CountEachCharacter(sourceText);
            var borders = GetBorders(frequencies, sourceText.Length);
            var frequenciesCode = ToStringFrequencyDictionary(frequencies);
            frequencies = null;
            var code = new StringBuilder();
            RationalNumber left = 0, right = 1;

            foreach (var symbol in sourceText)
            {
                var range = right - left;
                right = left + range * borders[symbol].Right;
                left = left + range * borders[symbol].Left;

                foreach (var digit in DeleteSameDigit(ref left, ref right))
                {
                    code.Append(digit);
                }

            }

            code.Append(GetFirstDigitAfterDot(left) + 1);
            compressionRatio = (double)sourceText.Length * 16 / code.Length / 4;
            code.Append(frequenciesCode);
            return code.ToString();
        }

        public string Decode(string codedText)
        {
            var sourceText = new StringBuilder();
            var borders = GetBorders(GetFrequencyDictionary(codedText, out int index,
                out int numberCharacters), numberCharacters);

            RationalNumber code = new RationalNumber(BigInteger.Parse(codedText.Substring(0, index)),
                BigInteger.Pow(10, index));

            while (sourceText.Length < numberCharacters)
            {
                foreach (var pair in borders)
                {
                    var border = pair.Value;
                    if (border.Left <= code && border.Right >= code)
                    {
                        sourceText.Append(pair.Key);
                        code = (code - border.Left) / (border.Right - border.Left);
                        break;
                    }
                }
            }

            return sourceText.ToString();
        }
    }
}