namespace AkkaFractal.Core
{
    public class SseFormatTile
    {
        public SseFormatTile(int x, int y, string imageBase64)
        {
            X = x;
            Y = y;
            ImageBase64 = imageBase64;
        }

        public int X { get; }
        public int Y { get; }
        public string ImageBase64 { get; }
    }
}