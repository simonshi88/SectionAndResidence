using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace Section
{
    class Model
    {
        public PolylineCurve Polyline; //Shape of this section
        public List<double> FloorLength; // Section length of each storey
        public List<Line> FloorLines;  // Floor line of each storey
        public List<Point3d> PointsA;  // Start points on one side of the section
        public List<Point3d> PointsB; // End points on the other side of the section

        public List<Point3d> PointsMidA; //The middle point of the eage of each floor
        public List<Point3d> PointsMidB;  //The middle point of the eage of each floor                   
        public List<Line> SampleLines;
        public List<double> SampleLength;
        public List<List<Point3d>> SamplePoints;
        
    public Model(Point3d anchorPoint,bool reverse, List<double> floorLength, double storeyHeight, int floor, double percision, double initialHeight)
    {
        CalculateFloor(anchorPoint, reverse, floorLength, storeyHeight, floor, initialHeight, out PointsA, out PointsB);
        PointsA.Reverse();
        PointsB.Reverse();

        PointsMidA = SectionCalculate.Arrange(PointsA);
        PointsMidB = SectionCalculate.Arrange(PointsB);
            
        FloorLines = CalculateLineLength(PointsA, PointsB, out FloorLength);
        SampleLines = CalculateLineLength(PointsMidA, PointsMidB, out SampleLength);

        List<Point3d> points =  new List<Point3d>(PointsA);
        List<Point3d> tempPoint = new List<Point3d>(PointsB);
        tempPoint.Reverse();
        points.AddRange(tempPoint);
        points.Add(points[0]);
        Polyline = new PolylineCurve(points);

        SamplePoints = CalculateSamplePoints(SampleLines, Polyline, percision);
    }

        /// <summary>
        /// Calculate all orientation points of the section
        /// </summary>
        /// <param name="anchorPoint">Position of section generation</param>
        /// <param name="reverse">The section is left or right of the anchor point</param>
        /// <param name="floorLength">The length of each floor</param>
        /// <param name="storeyHeight">Storey height</param>
        /// <param name="floor">The number of floors</param>
        /// <param name="initialHeight">The height of the ground floor above the ground</param>
        /// <param name="PointsA">PointsA of this Model</param>
        /// <param name="PointsB">PointsB of this Model</param>
    public static void CalculateFloor(Point3d anchorPoint, bool reverse, List<double> floorLength, 
    double storeyHeight, int floor, double initialHeight, out List<Point3d> PointsA, out List<Point3d> PointsB)
    {
        List<Point3d> pointsA = new List<Point3d>();
        List<Point3d> pointsB = new List<Point3d>();
        for (int i = 0; i < floor; i++)
        {
            var pointA = new Point3d(anchorPoint.X, anchorPoint.Y, anchorPoint.Z + i * storeyHeight + initialHeight);
            var pointB = new Point3d();
            if (!reverse)
                pointB = new Point3d(pointA.X + floorLength[i], pointA.Y, pointA.Z);
            else
                pointB = new Point3d(pointA.X - floorLength[i], pointA.Y, pointA.Z);
            pointsA.Add(pointA);
            pointsB.Add(pointB);
        }
        pointsA.Sort((Point3d pointA, Point3d pointB) => pointA.Z.CompareTo(pointB.Z));
        pointsB.Sort((Point3d pointA, Point3d pointB) => pointA.Z.CompareTo(pointB.Z));
        PointsA = pointsA;
        PointsB = pointsB;
    }

        public static List<Line> CalculateLineLength(List<Point3d> pointsA, List<Point3d> pointsB,out List<double> length)
        {
            var lines = new List<Line>();
            var lengths = new List<double>();

            for(int i = 0; i < pointsA.Count; i++)
            {
                var line = new Line(pointsA[i], pointsB[i]);
                lines.Add(line);
                lengths.Add(line.Length);
            }
            length = lengths;

            return lines;
        }

        public static List<List<Point3d>> CalculateSamplePoints(List<Line> lines, PolylineCurve polyline, double percision)
        {
            Point3d pointStart = polyline.GetBoundingBox(true).Min;
            Point3d pointEnd = polyline.GetBoundingBox(true).Max;

            int number = (int)Math.Ceiling((pointEnd.X - pointStart.X) / percision);
            var usingPoints = new List<List<Point3d>>();
            for (int i = 0; i < lines.Count; i++)
            {
                List<Point3d> stoeryPoints = new List<Point3d>();
                for (int j = 0; j < number; j++)
                {
                    var plane = new Plane(new Point3d(pointStart.X + j * percision, 0, 0), Vector3d.XAxis);
                    var curve = lines[i].ToNurbsCurve();
                    var crossing = Intersection.CurvePlane(curve, plane, 0.001);                   
                    if (crossing != null && crossing.Count == 1)
                    {
                        stoeryPoints.Add(crossing[0].PointA);
                    }
                }
                usingPoints.Add(stoeryPoints);
            }
            return usingPoints;
        }
    }
}
