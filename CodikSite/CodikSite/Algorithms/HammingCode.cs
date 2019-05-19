using System;
using System.Text;

namespace CodikSite.Algorithms
{
    public class Hamming
    {
        private int[,] G = new int[,] { {1,1,1,0,0,0,0 },
            {1,0,0,1,1,0,0 },{0,1,0,1,0,1,0},{1,1,0,1,0,0,1}
        };
        private int[,] H = new int[,] { {0,0,1},{0,1,0},{0,1,1},{1,0,0},{1,0,1},{1,1,0},{1,1,1}
        };
        public Hamming() { }
        public string Encode (string enter)
        {
            StringBuilder builder = new StringBuilder();
            for(int i = 0;i<7;i++)
            {
                int rez = 0;
                for (int j = 0;j<4;j++)
                {
                    if (enter[j] == '1' && G[j, i] == 1)
                        rez++;
                }
                builder.Append(rez % 2);
            }
            return builder.ToString();

        }
        public string Decode (string enter)
        {
            var entErr = ErrorGenerator(enter);
            
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                int rez = 0;
                for (int j = 0; j < 7; j++)
                {
                    if (entErr[j] == '1' && H[j, i] == 1)
                        rez++;
                }
                builder.Append(rez % 2);
            }
            int index = 0;
            if (builder[0] == '1')
                index += 4;
            if (builder[1] == '1')
                index += 2;
            if (builder[2] == '1')
                index += 1;
            return entErr+'\n'+ index.ToString();
        }
        private string ErrorGenerator(string enter)
        {
            Random random = new Random();
            int i = random.Next(0, 6);
            StringBuilder builder = new StringBuilder(enter);
            if (builder[i] == '0')
                builder[i] = '1';
            else
                builder[i] = '0';
            return builder.ToString();
        }
    }
}