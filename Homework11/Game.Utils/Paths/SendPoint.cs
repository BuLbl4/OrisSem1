using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Utils.Paths
{
    public class SendPoint
    {
        public SendPoint() { }
        public SendPoint(Point point, string? color)
        {
            Point = point;
            Color = color;
        }

        public Point Point { get; set; }
        public string? Color { get; set; }
    }
}
