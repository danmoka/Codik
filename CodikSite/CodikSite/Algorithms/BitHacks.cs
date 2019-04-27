
namespace CodikSite.Algorithms
{
    static class BitHacks
    {
        public static int GetHighestBitPosition(int number)
        {
            int bitPos = 1;
            int val = 2;
            while (val <= number)
            {
                val <<= 1;
                bitPos++;
            }
            return bitPos;
        }
    }
}