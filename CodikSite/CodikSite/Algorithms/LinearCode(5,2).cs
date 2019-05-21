using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodikSite.Algorithms
{
    public class Code
    {
        public string[,] H;
        public string[,] G;
        private SortedSet<string> vectors;
        public SortedSet<string>[] A;
        public string[] S;
        public Code(string matrix)
        {                                
            FullVectors();            
            FormingH(matrix);
            FormingG(matrix);
            FormingA();
            FormingS();
        }
        public string FormingPrint ()
        {
            var ansver = new StringBuilder();
            ansver.Append("H:\n");
            for (int i = 0;i<2;i++)
            {
                for(int j = 0;j<5;j++)
                {
                    ansver.Append(H[j, i]);
                }
                ansver.Append("\n");
            }
            ansver.Append("G:\n");
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    ansver.Append(G[j, i]);
                }
                ansver.Append("\n");
            }
            for (int i = 0;i<8;i++)
            {
                ansver.Append("A["+i+"] = { ");
                foreach (var s in A[i])
                {
                    ansver.Append(s + " ");
                }
                ansver.Append("}\nS[" + i + "] = " + S[i] + "\n");
            }
            return ansver.ToString();
        }

        public string Coding(string enter)
        {
            var enters = enter.Split(' ');
            var vector = new StringBuilder();
            for (int i = 0;i<5;i++)
            {
                int sum = 0;
                for(int j = 0;j<2;j++)
                {
                    if (enters[j]=="1" && enters[j] == H[i, j])
                        sum++;
                }
                vector.Append(sum % 2);
            }
            return vector.ToString();
        }
        public string RandomExp()
        {
            Random random = new Random();
            int i = random.Next(0, 4);
            var vector = new StringBuilder("00000");
            vector[i] = '1';
            return vector.ToString();
        }
        public string SearchExp(string enter)
        {
            var ansver = new StringBuilder("S = ");
            var vector = new StringBuilder();
            for (int l = 0; l < 3; l++)
            {
                int sum = 0;
                for (int k = 0; k < 5; k++)
                {
                    if (G[k, l] == "1" && G[k, l] == enter[k].ToString())
                        sum++;
                }
                vector.Append(sum % 2);
            }
            ansver.Append(vector);
            for (int i = 0; i < 8; i++)
            {
                if (vector.ToString() == S[i])
                {
                    foreach (var v in A[i])
                    {
                        if (v.IndexOf('1') == v.LastIndexOf('1'))
                        {
                            ansver.Append("\nExp = " + A[i].ElementAt(0));
                            break;
                        }
                        
                    }
                    break;
                    
                }
            }
            return ansver.ToString();
        }
        private void FormingH(string st)
        {
            H = new string[5, 2];
            var SplitSt = st.Split(new char[] { ' ','\n' }, StringSplitOptions.RemoveEmptyEntries);
            int k = 0;
            for (int i = 0; i < 2; i++)
                for (int j = 2; j < 5; j++)
                {
                    H[j, i] = SplitSt[k];
                    k++;
                }
            H[0, 0] = "1";
            H[1, 0] = "0";
            H[1, 1] = "1";
            H[0, 1] = "0";
        }
        private void FormingG (string st)
        {
            G = new string[5, 3];
            var SplitSt = st.Split(new char[] { ' ','\n' }, StringSplitOptions.RemoveEmptyEntries);
            int k = 0;
            for (int i = 0;i<2;i++)
            {
                for(int j = 0;j<3;j++)
                {
                    G[i, j] = SplitSt[k];
                    k++;
                }
            }
            G[2, 0] = "1";
            G[3, 0] = "0";
            G[4, 0] = "0";
            G[3, 1] = "1";
            G[2, 1] = "0";            
            G[4, 1] = "0";
            G[4, 2] = "1";
            G[2, 2] = "0";
            G[3, 2] = "0";
           

        }
        private void FullVectors()
        {
            vectors = new SortedSet<string>();
            for (int i = 0; i < 32; i++)
            {
                int num = i;
                var vector = new StringBuilder();
                for (int k = 16; k > 0; k = k / 2)
                {
                    if (num - k >= 0)

                    {
                        vector.Append("1");
                        num -= k;
                    }
                    else
                        vector.Append("0");
                }
                vectors.Add(vector.ToString());
            }
        } 
        private void FormingA ()
        {
            A = new SortedSet<string>[8];
            for (int i = 0; i < 8; i++)
                A[i] = new SortedSet<string>();
            var vector = new StringBuilder();
            for (int i = 0; i < 2; i++)
            {                
                for (int j = 0; j < 5; j++)
                {
                    vector.Append(H[j, i]);
                }
                A[0].Add(vector.ToString());
                vectors.Remove(vector.ToString());
                vector = new StringBuilder();
            }
            A[0].Add(AddingVectors(A[0].ElementAt(0), A[0].ElementAt(1)));
            A[0].Add("00000");
            vectors.Remove("00000");
            
            for (int i = 1;i<8;i++)
            {
                var b = vectors.First();
                vectors.Remove(b);
                for (int j = 0;j<4;j++)
                {
                    A[i].Add(AddingVectors(b, A[0].ElementAt(j)));
                }
            }
        }
        public string AddingVectors (string vector1, string vector2)
        {
            var rezult = new StringBuilder();
            for (int i = 0;i<5;i++)
            {
                if (vector1[i] != vector2[i])
                    rezult.Append("1");
                else
                    rezult.Append("0");
            }
            vectors.Remove(rezult.ToString());
            return rezult.ToString();
        }
        private void FormingS ()
        {
            S = new string[8];
            for (int i = 0;i<8;i++)
            {
                var vector = new StringBuilder();
                string u = A[i].ElementAt(0);
                for (int l = 0; l<3;l++)
                {
                    int sum = 0;                    
                    for (int k = 0;k<5;k++)
                    {
                        if (G[k, l] == "1" && G[k, l] == u[k].ToString())
                            sum++;
                    }
                    vector.Append(sum % 2);
                }
                S[i] = vector.ToString();
            }
        }
    }
}
