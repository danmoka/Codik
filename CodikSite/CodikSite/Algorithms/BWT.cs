using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodikSite.Algorithms
{
    /// <summary>
    /// Burrows–Wheeler transform
    /// </summary>
    //source https://neerc.ifmo.ru/wiki/index.php?title=Преобразование_Барроуза-Уилера
    public class BWT : ITextEncodingAlgorithm
    {
        public string Encode(string sourceText, out double compressionRatio)
        {
            // при изменении нужно изменить и в регулярном выражении в Decode
            int textBlockSize = 8192;
            var answer = new StringBuilder(sourceText.Length);
            int currentIndex = 0;

            while (currentIndex != sourceText.Length)
            {
                int length = Math.Min(sourceText.Length - currentIndex, textBlockSize);
                answer.Append('(');
                answer.Append(EncodeTextBlock(sourceText.Substring(currentIndex, length)));
                answer.Append(')');
                currentIndex += length;
            }

            compressionRatio = (double)sourceText.Length / answer.Length;

            return answer.ToString();
        }

        public string Decode(string codedText)
        {
            if (Regex.IsMatch(codedText, @"\A(\((.|\n){8192},\d+\))*(\((.|\n){1,8191},\d+\)){0,1}\z"))
            {
                var singleCodePattern = @"(\((.|\n){8192},\d+\))|(\((.|\n){1,8191},\d+\)\z)";
                var answer = new StringBuilder(codedText.Length);

                foreach (Match match in Regex.Matches(codedText, singleCodePattern))
                {
                    answer.Append(DecodeTextBlock(match.Value.Substring(1, match.Value.Length - 2)));
                }

                return answer.ToString();
            }
            else
            {
                throw new ArgumentException();
            }
        }

        //Перестановка
        private struct Permutation : IComparable<Permutation>
        {
            private readonly char[] _charArray;
            public bool Original { get; set; }

            public Permutation(int length, bool original)
            {
                this._charArray = new char[length];
                this.Original = original;
            }

            public char this[int index]
            {
                get
                {
                    return _charArray[index];
                }
                set
                {
                    _charArray[index] = value;
                }
            }

            public int CompareTo(Permutation other)
            {
                for (int i = 0; i < this._charArray.Length; i++)
                {
                    var comparisonResult = this._charArray[i].CompareTo(other._charArray[i]);
                    if (comparisonResult != 0)
                    {
                        return comparisonResult;
                    }
                }
                return 0;
            }
        }

        private string EncodeTextBlock(string textBlock)
        {
            var matrix = new Permutation[textBlock.Length];
            FillMatrix(matrix, textBlock);
            matrix[0].Original = true;
            Array.Sort(matrix);
            var answer = new StringBuilder(textBlock.Length + 5);
            int index = 0;
            var lastColumn = textBlock.Length - 1;

            for (int i = 0; i < textBlock.Length; i++)
            {
                answer.Append(matrix[i][lastColumn]);
                if (matrix[i].Original)
                {
                    index = i;
                }
            }

            answer.Append(',');
            answer.Append(index);
            return answer.ToString();
        }

        private void FillMatrix(Permutation[] permutations, string sourceText)
        {
            int taskCount = 1;

            try
            {
                taskCount = Environment.ProcessorCount; //логические процессоры
                //foreach (var item in new System.Management.ManagementObjectSearcher("Select NumberOfCores from Win32_Processor").Get())
                //{
                //    coreCount += int.Parse(item["NumberOfCores"].ToString());
                //}
            }
            catch
            {
                taskCount = 4;
            }

            var tasks = new List<Task>(taskCount);
            int lengthOnTask = Math.DivRem(permutations.Length, taskCount, out int remainder);
            int currentIndex = 0;

            for (int i = 0; currentIndex < permutations.Length; i++)
            {
                var index = currentIndex;
                var currentLength = lengthOnTask;
                if (i < remainder)
                {
                    currentLength++;
                }
                tasks.Add(Task.Run(() => FillPartOfMatrix(sourceText, permutations, index, currentLength)));
                currentIndex += currentLength;
            }

            Task.WaitAll(tasks.ToArray());

            //local function
            void FillPartOfMatrix(string text, Permutation[] matrix, int startIndex, int length)
            {
                var endIndex = startIndex + length;

                for (int i = startIndex; i < endIndex; i++)
                {
                    matrix[i] = new Permutation(text.Length, false);

                    for (int j = 0; j < text.Length; j++)
                    {
                        matrix[i][j] = text[(i + j) % text.Length];
                    }

                }

            }

        }

        private struct State
        {
            public char Symbol { get; private set; } //Символ в текущей позиции
            public int Count { get; private set; } //Количество появлений этого же символа до текущей позиции

            public State(char symbol, int count)
            {
                this.Symbol = symbol;
                this.Count = count;
            }
        }

        //Преобразование Барроуза-Уилера, Викиконспекты, Обратное преобразование, линейный алгоритм
        private string DecodeTextBlock(string codedTextBlock)
        {
            int separatorPosition = codedTextBlock.LastIndexOf(',');
            //получили номер строки в отсортированной матрице
            int index = int.Parse(codedTextBlock.Substring(separatorPosition + 1));
            var states = new State[separatorPosition]; //массив состояний для преобразованной строки
            var frequencies = new Dictionary<char, int>(); 
            
            for (int i = 0; i < separatorPosition; i++)
            {
                var symbol = codedTextBlock[i];
                if (frequencies.TryGetValue(symbol, out int value))
                {
                    frequencies[symbol]++;
                }
                else
                {
                    frequencies.Add(symbol, 1);
                }
                states[i] = new State(symbol, value);
            }

            /*в lessCount хранится количество символов в преобразованной строке,
             * которые лексикографически меньше, чем текущий */
            var lessCount = frequencies;
            int sum = 0;

            foreach (var symbol in from pair in frequencies orderby pair.Key select pair.Key)
            {
                var value = lessCount[symbol];
                lessCount[symbol] = sum;
                sum += value;
            }

            frequencies = null;            
            var characters = new char[separatorPosition];

            while (separatorPosition > 0)
            {
                var state = states[index];
                characters[--separatorPosition] = state.Symbol;
                index = state.Count + lessCount[state.Symbol];
            }

            return new string(characters);
        }

    }
}