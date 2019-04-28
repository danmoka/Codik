using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CodikSite.Algorithms
{
    /// <summary>
    /// Huffman coding
    /// </summary>
    // source http://neerc.ifmo.ru/wiki/index.php?title=Алгоритм_Хаффмана_для_n_ичной_системы_счисления
    // http://trinary.ru/kb/d0085167-8832-466b-8cba-cc2e377e84c3
    public class HuffmanCoding : ITextEncodingAlgorithm
    {
        private readonly int _numberSystem;

        public HuffmanCoding()
        {
            this._numberSystem = 2;
        }

        public HuffmanCoding(int numberSystem)
        {
            if (numberSystem < 2) throw new ArgumentException();
            if (numberSystem > 10) throw new ArgumentException();
            this._numberSystem = numberSystem;
        }

        public string Encode(string sourceText, out double compressionRatio)
        {
            var codes = CreateCodeWords(sourceText, _numberSystem);
            var builder = Coder.GetCodedMessageBuilder(sourceText, codes);

            compressionRatio = (sourceText.Length * (double)BitHacks.GetRealSizeForNumber(char.MaxValue))
                / ((double)BitHacks.GetRealSizeForNumber((uint)Math.Min(codes.Count, _numberSystem) - 1)
                * builder.Length);

            return Coder.AddCodeWords(builder, codes).ToString();
        }

        public string Decode(string codedText)
        {
            return Decoder.GetDecodedMessage(codedText);
        }

        private IReadOnlyDictionary<char, string> CreateCodeWords(string message, int numberSystem)
        {
            return CreateCodeWords(GetFrequencyDictionary(message), numberSystem);
        }

        private IReadOnlyDictionary<char, string> CreateCodeWords(IReadOnlyDictionary<char, double>
            frequencyDictionary, int numberSystem)
        {
            var tree = new List<Node>(frequencyDictionary.Count);

            foreach (var key in frequencyDictionary.Keys)
            {
                tree.Add(new Node(key, frequencyDictionary[key], null));
            }

            int m = frequencyDictionary.Count;
            if (m > numberSystem)
            {

                while ((m - numberSystem) % (numberSystem - 1) != 0)
                {
                    m++;
                }

            }
            int start = numberSystem - (m - frequencyDictionary.Count); //сколько нужно взять на первом шаге

            // первый шаг
            tree.Sort();
            var nodes = new List<Node>(start);
            var sumNodes = 0.0;
            while (tree.Count > 0 && nodes.Count != start)
            {
                int last = tree.Count - 1;
                nodes.Add(tree[last]);
                sumNodes += tree[last].Value;
                tree.RemoveAt(last);
            }
            //

            tree.Add(new Node('0', sumNodes, nodes));

            while (tree.Count > 1)
            {
                tree.Sort();
                var list = new List<Node>(numberSystem);
                var sum = 0.0;

                while (list.Count < numberSystem)
                {
                    int last = tree.Count - 1;
                    list.Add(tree[last]);
                    sum += tree[last].Value;
                    tree.RemoveAt(last);
                }

                tree.Add(new Node('0', sum, list));
            }

            return GetCodesWhenTraversing(tree[0]);
        }

        private IReadOnlyDictionary<char, double> GetFrequencyDictionary(string message)
        {
            var frequencyDict = new Dictionary<char, double>();

            foreach (char symbol in message)
            {
                if (frequencyDict.ContainsKey(symbol))
                {
                    frequencyDict[symbol]++;
                }
                else
                {
                    frequencyDict.Add(symbol, 1);
                }
            }

            return frequencyDict;
        }

        private IReadOnlyDictionary<char, string> GetCodesWhenTraversing(Node node)
        {
            Dictionary<char, string> codes = new Dictionary<char, string>(113);
            StringBuilder path = new StringBuilder(32);
            DoRecursionTraversal(node, path, codes);
            return codes;
        }

        private void DoRecursionTraversal(Node node, StringBuilder path, Dictionary<char, string> codes)
        {
            if (node.Childs != null)
            {
                for (int i = 0; i < node.Childs.Count; i++)
                {
                    path.Append((char)('0' + i));
                    DoRecursionTraversal(node.Childs[i], path, codes);
                    path.Remove(path.Length - 1, 1);
                }
            }
            else
            {
                codes.Add(node.Symbol, path.ToString());
            }
        }

        private class Node : IComparable<Node>
        {
            public char Symbol { get; private set; }
            public double Value { get; private set; }
            public IReadOnlyList<Node> Childs { get; private set; }

            public Node(char symbol, double value, List<Node> childs)
            {
                this.Symbol = symbol;
                this.Value = value;
                this.Childs = childs;
            }

            public int CompareTo(Node other)
            {
                var result = -this.Value.CompareTo(other.Value);
                if (result == 0)
                {
                    result = -this.Symbol.CompareTo(other.Symbol);
                }
                return result;
            }
        }

        private static class Coder
        {
            public static string GetCodedMessage(string message, IReadOnlyDictionary<char, string> codes,
                bool addCodes = false)
            {
                return GetCodedMessageBuilder(message, codes, addCodes).ToString();
            }

            public static StringBuilder AddCodeWords(StringBuilder codedMessageBuilder,
                IReadOnlyDictionary<char, string> codes)
            {
                codedMessageBuilder.Append('{');

                foreach (var pair in codes)
                {
                    codedMessageBuilder.Append(string.Format("[{0}-{1}]", pair.Key, pair.Value));
                }

                codedMessageBuilder.Append('}');

                return codedMessageBuilder;
            }

            public static StringBuilder GetCodedMessageBuilder(string message,
                IReadOnlyDictionary<char, string> codes, bool addCodes = false)
            {
                StringBuilder codedMessageBuilder = new StringBuilder(message.Length * 3);

                foreach (var symbol in message)
                {
                    codedMessageBuilder.Append(codes[symbol]);
                }

                if (addCodes) codedMessageBuilder = AddCodeWords(codedMessageBuilder, codes);

                return codedMessageBuilder;
            }

        }

        private static class Decoder
        {

            public static string GetDecodedMessage(string codedMessage,
                IReadOnlyDictionary<char, string> codeWords)
            {
                var words = new Dictionary<string, char>(codeWords.Count);

                foreach (var symbol in codeWords.Keys)
                {
                    words.Add(codeWords[symbol], symbol);
                }

                return GetDecodedMessage(codedMessage, words);
            }

            public static string GetDecodedMessage(string codedMessage,
                IReadOnlyDictionary<string, char> codeWords = null)
            {
                int dictionaryPosition;
                if (codeWords == null) codeWords = GetCodeWords(codedMessage, out dictionaryPosition);
                else dictionaryPosition = codedMessage.Length;
                var word = new StringBuilder(32);
                var decodedMessage = new StringBuilder();

                for (int i = 0; i < dictionaryPosition; i++)
                {
                    word.Append(codedMessage[i]);
                    if (codeWords.TryGetValue(word.ToString(), out char decoded))
                    {
                        decodedMessage.Append(decoded);
                        word.Clear();
                    }
                }

                return decodedMessage.ToString();
            }

            static IReadOnlyDictionary<string, char> GetCodeWords(string codedMessage,
                out int dictionaryIndex)
            {
                var codedTextPattern = @"\{(\[(.|\n)\-\d+\])+\}\z";
                var matchAllCodes = Regex.Match(codedMessage, codedTextPattern);
                if (matchAllCodes.Success)
                {
                    dictionaryIndex = matchAllCodes.Index;
                    var singleCodePattern = @"\[(.|\n)\-\d+\]";
                    var dictionary = new Dictionary<string, char>();

                    foreach (Match matchCode in Regex.Matches(matchAllCodes.Value, singleCodePattern))
                    {
                        var code = matchCode.Value.Substring(3, matchCode.Value.Length - 4);
                        dictionary.Add(code, matchCode.Value[1]);
                    }

                    return dictionary;
                }
                else
                {
                    throw new ArgumentException();
                }
            }

        }

    }
}