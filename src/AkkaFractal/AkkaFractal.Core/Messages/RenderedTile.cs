namespace AkkaFractal.Core
{
    public class RenderedTile
    {
        public RenderedTile(int x, int y, byte[] bytes)
        {
            X = x;
            Y = y;
            Bytes = bytes;
        }

        public int X { get; }
        public int Y { get; }
        public byte[] Bytes { get; }
    }
}