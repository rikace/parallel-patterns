using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaFractalShared
{
    public class RenderedTile
    {
        public RenderedTile(byte[] bytes, int x, int y)
        {
            Bytes = bytes;
            X = x;
            Y = y;
        }
        public RenderedTile(bool isLastTile)
        {
            this.IsLastTile = isLastTile;
        }

        public byte[] Bytes { get; }
        public int X { get; }
        public int Y { get; }
        public bool IsLastTile { get; }
    }
}
