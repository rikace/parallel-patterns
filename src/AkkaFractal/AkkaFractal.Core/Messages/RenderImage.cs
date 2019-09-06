namespace AkkaFractal.Core
{
    public class RenderImage
    {
        public RenderImage(int height, int width)
        {
            Height = height;
            Width = width;
        }

        public int Height { get; }
        public int Width { get; }
    }
}