namespace AkkaFractal.Core
{
    public class RenderTile
    {
        public RenderTile(int x, int y, int height, int width, int imageHeight, int imageWidth)
        {
            X = x;
            Y = y;
            Height = height;
            Width = width;

            ImageHeight = imageHeight;
            ImageWidth = imageWidth;
        }


        public int X { get; }
        public int Y { get; }
        public int Height { get; }
        public int Width { get; }

        public int ImageHeight { get; }
        public int ImageWidth { get; }
    }
}