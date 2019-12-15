using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

using Grasshopper.Kernel.Data;
using Grasshopper.Kernel;


namespace residence
{
    class Building
    {
        public Point3d Point {get; set;}  // position of the residence
        public Curve Residence { get; set; }  // shape of the residence
        public Curve SpaceRegulation { get; set; } // shape of spacing regulation
        public Curve SunRegulation { get; set; } // shape of sunshine regulation
        public double Radius { get; set; }  // radius of the residence
        public double Height { get; set; } // height of the residence
        public double Regulation { get; set; } // distance of spacing regulation

        public Building(Point3d pt, double r, double h)
        {
            Point = pt;
            Radius = r;
            Height = h;
            Regulation = 13.00;
            SunRegulation = CalculateSunRegulation(pt, r, h);
            SpaceRegulation = new Circle(Point,Radius + Regulation).ToNurbsCurve();
            Residence = new Circle(Point, Radius).ToNurbsCurve();
        }

        public Building(Point3d pt, double r, double h, double regulation)
        {
            Point = pt;
            Radius = r;
            Height = h;
            Regulation = regulation;
            SunRegulation = CalculateSunRegulation(pt, r, h);
            SpaceRegulation = new Circle(Point, Radius + Regulation).ToNurbsCurve();
            Residence = new Circle(Point, Radius).ToNurbsCurve();
        }
        /// <summary>
        /// construct the sunshine regulation 
        /// </summary>
        /// <param name="pt">postion of the residence</param>
        /// <param name="r">radius of the residence</param>
        /// <param name="h">height of the residence</param>
        /// <returns>the profile of the shadow</returns>
        public Curve CalculateSunRegulation(Point3d pt, double r, double h)
    {
            /* h: the height of residence
                shadow: Length of shadow at 11:00 and 13:00
                shadowMid: Length of shadow at 12:00
                angle: Solar Azimuth
                angleCD: orientation for point C and D by cosine law */

            double shadow = h / Math.Tan((25.0 + 14.0 / 60)/180 * Math.PI);
            double shadowMid = h / Math.Tan((26.0 + 36.0 / 60) / 180 * Math.PI);
            double angle = (15.0 +12.0 /60) / 180 * Math.PI;
            double angleCD = (9.0 + 40.0 / 60) / 180 * Math.PI;
            Circle circleA = new Circle(pt, r);
            Point3d f = circleA.PointAt(Math.PI * 2 - angle);
            Point3d a = circleA.PointAt(Math.PI + angle);
            Point3d centerB = new Point3d(pt.X - shadow * Math.Sin(angle), pt.Y + shadow * Math.Cos(angle), 0);
            Point3d centerC = new Point3d(pt.X + shadow * Math.Sin(angle), pt.Y + shadow * Math.Cos(angle), 0);
            Point3d g = new Point3d(pt.X, pt.Y + shadowMid + r, 0);
            var circleB = new Circle(centerB, r);
            var circleC = new Circle(centerC, r);
            var b = circleB.PointAt(Math.PI + angle);
            var c = circleB.PointAt(Math.PI / 2 - angleCD);
            var d = circleC.PointAt(Math.PI / 2 + angleCD);
            var e = circleC.PointAt(Math.PI * 2 - angle);

            Arc arcBC = new Arc(circleB, new Interval(Math.PI + angle, Math.PI / 2 - angleCD));
            Arc arcCD = new Arc(c, g, d);
            Arc arcDE = new Arc(circleC, new Interval( - angle, Math.PI / 2 + angleCD));
            Arc arcAF = new Arc(circleA, new Interval(Math.PI + angle, Math.PI * 2 - angle));
            Line lineAB = new Line(a, b);
            Line lineEF = new Line(e, f);

            List<Curve> curves = new List<Curve>();
            curves.Add(lineAB.ToNurbsCurve());
            curves.Add(arcBC.ToNurbsCurve());
            curves.Add(arcCD.ToNurbsCurve());
            curves.Add(arcDE.ToNurbsCurve());
            curves.Add(lineEF.ToNurbsCurve());
            curves.Add(arcAF.ToNurbsCurve());

            var cc = Curve.JoinCurves(curves);
            var curve =  cc.Single();
            return curve;

        }
        /// <summary>
        /// Move the residence according to the vector
        /// </summary>
        /// <param name="vector">Direction of movement</param>
        /// <returns>Success or not</returns>
        public bool MoveBuilding(Vector3d vector)
        {
            if (Residence.Translate(vector) &&
                SpaceRegulation.Translate(vector) &&
                SunRegulation.Translate(vector))
                return true;
            else return false;
        }
    }
}
