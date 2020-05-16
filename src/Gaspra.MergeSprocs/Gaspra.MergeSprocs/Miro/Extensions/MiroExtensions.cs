using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Gaspra.MergeSprocs.Miro.Extensions
{
    public class MiroExtensions
    {
        public static List<PointF> GetCircularPoints(double radius, PointF center, double angleInterval)
        {
            List<PointF> points = new List<PointF>();

            for (double interval = angleInterval; interval < 2 * Math.PI; interval += angleInterval)
            {
                double X = center.X + (radius * Math.Cos(interval));
                double Y = center.Y + (radius * Math.Sin(interval));

                points.Add(new PointF((float)X, (float)Y));
            }

            return points;
        }
    }
}
