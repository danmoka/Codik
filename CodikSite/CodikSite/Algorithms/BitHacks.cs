
namespace CodikSite.Algorithms
{
    static class BitHacks
    {
        public static uint GetRealSizeForNumber(uint number)
        {
            uint bitCount = 0;

            do
            {
                bitCount++;
            } while ((number >>= 1) > 0);

            return bitCount;
        }
    }
}