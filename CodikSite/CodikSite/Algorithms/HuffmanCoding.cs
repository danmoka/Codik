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

        public HuffmanCoding() : this(2)
        {

        }

        public HuffmanCoding(int numberSystem)
        {
            if (numberSystem < 2)
            {
                throw new ArgumentException();
            }
            if (numberSystem > 10)
            {
                throw new ArgumentException();
            }
            this._numberSystem = numberSystem;
        }

        private Dictionary<char, int> GetFrequencyDictionary(string text)
        {
            var frequencyDictionary = new Dictionary<char, int>();

            foreach (char symbol in text)
            {
                if (frequencyDictionary.ContainsKey(symbol))
                {
                    frequencyDictionary[symbol]++;
                }
                else
                {
                    frequencyDictionary.Add(symbol, 1);
                }
            }

            return frequencyDictionary;
        }

        private class Node : IComparable<Node>
        {
            public char Symbol { get; private set; }
            public int Frequency { get; private set; }
            public List<Node> Childs { get; private set; }

            public Node(char symbol, int frequency, List<Node> childs)
            {
                this.Symbol = symbol;
                this.Frequency = frequency;
                this.Childs = childs;
            }

            public int CompareTo(Node other)
            {
                var result = -this.Frequency.CompareTo(other.Frequency);
                if (result == 0)
                {
                    result = this.Symbol.CompareTo(other.Symbol);
                }
                return result;
            }
        }

        private Dictionary<char, string> CreateCodeWords(Dictionary<char, int> frequencyDictionary,
            int numberSystem)
        {
            var tree = new List<Node>(frequencyDictionary.Count);

            foreach (var pair in frequencyDictionary)
            {
                tree.Add(new Node(pair.Key, pair.Value, null));
            }

            {
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
                var nodes = new List<Node>(start);
                var sumNodes = 0;
                tree.Sort();

                while (tree.Count > 0 && nodes.Count != start)
                {
                    int last = tree.Count - 1;
                    nodes.Add(tree[last]);
                    sumNodes += tree[last].Frequency;
                    tree.RemoveAt(last);
                }
                //
                tree.Add(new Node(char.MinValue, sumNodes, nodes));
            }

            while (tree.Count > 1)
            {
                tree.Sort();
                var childs = new List<Node>(numberSystem);
                var sumFrequencies = 0;

                while (childs.Count < numberSystem)
                {
                    var lastIndex = tree.Count - 1;
                    childs.Add(tree[lastIndex]);
                    sumFrequencies += tree[lastIndex].Frequency;
                    tree.RemoveAt(lastIndex);
                }

                tree.Add(new Node(char.MinValue, sumFrequencies, childs));
            }

            return GetCodesWhenTraversing(tree[0]);
        }

        private Dictionary<char, string> GetCodesWhenTraversing(Node node)
        {
            var codes = new Dictionary<char, string>(113);
            var path = new StringBuilder(32);
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

        public string Encode(string sourceText, out double compressionRatio)
        {
            var codeWords = CreateCodeWords(GetFrequencyDictionary(sourceText), this._numberSystem);
            var code = new StringBuilder();

            foreach(var symbol in sourceText)
            {
                code.Append(codeWords[symbol]);
            }

            compressionRatio = ((double)sourceText.Length * BitHacks.GetRealSizeForNumber(char.MaxValue))
                / ((double)BitHacks.GetRealSizeForNumber((uint)Math.Min(codeWords.Count, _numberSystem) - 1)
                * code.Length);
            code.Append(codeWords.ToSrtingExtension());
            return code.ToString();
        }

        private Dictionary<string, char> GetCodeWords(string codedText, out int dictionaryIndex)
        {
            if (Regex.IsMatch(codedText, @"\A\d+\{(\[(.|\n)\-\d+\])+\}\z"))
            {
                dictionaryIndex = Regex.Match(codedText, @"\{(\[(.|\n)\-\d+\])+\}\z").Index;
                var singleCodePattern = @"\[(.|\n)\-\d+\]";
                var dictionary = new Dictionary<string, char>();

                foreach (Match charCodeWordPairMatch in Regex.Matches(codedText.Substring(dictionaryIndex),
                    singleCodePattern))
                {
                    var codeWord = charCodeWordPairMatch.Value.Substring(3,
                        charCodeWordPairMatch.Value.Length - 4);
                    dictionary.Add(codeWord, charCodeWordPairMatch.Value[1]);
                }

                return dictionary;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public string Decode(string codedText)
        {
            var codeWords = GetCodeWords(codedText, out int dictionaryIndex);
            var word = new StringBuilder();
            var sourceText = new StringBuilder();

            for (int i = 0; i < dictionaryIndex; i++)
            {
                word.Append(codedText[i]);
                if (codeWords.TryGetValue(word.ToString(), out char symbol))
                {
                    sourceText.Append(symbol);
                    word.Clear();
                }
            }

            if (word.Length != 0)
            {
                throw new ArgumentException();
            }
            return sourceText.ToString();
        }
        
    }
}