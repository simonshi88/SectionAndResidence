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
    class SectionCalculate
    {
        public List<Point3d> PointsA;
        public List<Point3d> PointsB;

        public List<Point3d> PointsMidA;
        public List<Point3d> PointsMidB;

        public List<PolylineCurve> Curves;

        public List<double> ElevationAngleA;
        public List<double> ElevationAngleB;

        public List<double> ProjectAngleA;
        public List<double> ProjectAngleB;

        public List<List<Line>> ElevationLineA;
        public List<List<Line>> ElevationLineB;

        public Model ModelA;
        public Model ModelB;

        public double Lux = 4500 * 9 / 7 / Math.PI;

        public List<List<double>> EnergyEachPointA;
        public List<List<double>> EnergyEachPointB;

        public double StandardDeviationTotalA;
        public double StandardDeviationTotalB;
        public double EnergyAverageA;
        public double EnergyAverageB;


        public SectionCalculate(Model modelA, Model modelB)
        {           
            Point3d modelACenter = modelA.Polyline.GetBoundingBox(true).Center;
            Point3d modelBCenter = modelB.Polyline.GetBoundingBox(true).Center;
            Point3d calculateCenter = new Point3d((modelACenter.X + modelBCenter.X) / 2, (modelACenter.Y + modelBCenter.Y) / 2, 
                (modelACenter.Z + modelBCenter.Z) / 2);

            double number1 = 0.0;
            foreach(var point in modelA.PointsMidA)
            {
                number1 += point.DistanceTo(calculateCenter);
            }
            double number2 = 0.0;
            foreach (var point in modelA.PointsMidB)
            {
                number2 += point.DistanceTo(calculateCenter);
            }
            double number3 = 0.0;
            foreach (var point in modelB.PointsMidA)
            {
                number3 += point.DistanceTo(calculateCenter);
            }
            double number4 = 0.0;
            foreach (var point in modelB.PointsMidB)
            {
                number4 += point.DistanceTo(calculateCenter);
            }

            var pointsA = (number1 < number2) ? modelA.PointsA : modelA.PointsB;
            var pointsB = (number3 < number4) ? modelB.PointsA : modelB.PointsB;


            pointsA.Sort((Point3d pointA, Point3d pointB) => pointA.Z.CompareTo(pointB.Z));
            pointsA.Reverse();

            pointsB.Sort((Point3d pointA, Point3d pointB) => pointA.Z.CompareTo(pointB.Z));
            pointsB.Reverse();

            PointsA = pointsA;
            PointsB = pointsB;

            PointsMidA = Arrange(pointsA);
            PointsMidB = Arrange(pointsB);

            Curves = ConstructModel(pointsA, pointsB);

            ElevationLineA = CalculateView(PointsMidA, PointsA, PointsB, out ElevationAngleA, out ProjectAngleA);
            ElevationLineB = CalculateView(PointsMidB, PointsB, PointsA, out ElevationAngleB, out ProjectAngleB);

            ModelA = modelA;
            ModelB = modelB;

            EnergyEachPointA = EnergyEachPoint(ElevationAngleA, ProjectAngleA, modelA, PointsMidA, PointsA, Lux, out EnergyAverageA);
            EnergyEachPointB = EnergyEachPoint(ElevationAngleB, ProjectAngleB, modelB, PointsMidB, PointsB, Lux, out EnergyAverageB);

            StandardDeviationTotalA = StandardDeviation(EnergyEachPointA, EnergyAverageA);
            StandardDeviationTotalB = StandardDeviation(EnergyEachPointB, EnergyAverageB);
        }

        public static List<Point3d> Arrange(List<Point3d> points)
        {
            var pointMid = new List<Point3d>();
                        
            for(int i = 0; i < points.Count - 1; i++)
            {
                var line = new Line(points[i], points[i + 1]);
                var point = line.PointAt(0.5);
                pointMid.Add(point);
            }
            return pointMid;
        }

        public static List<PolylineCurve> ConstructModel(List<Point3d> pointsA, List<Point3d> pointsB)
        {
            List<PolylineCurve> curves = new List<PolylineCurve>();
            Vector3d vector = new Vector3d(pointsA[0] - pointsB[0]);
            var xf1 = Transform.Translation(vector);
            var xf2 = Transform.Translation(vector * -1);

            Point3d pointA1 = new Point3d(pointsA[0]);
            pointA1.Transform(xf1);
            Point3d pointB1 = new Point3d(pointsB[0]);
            pointB1.Transform(xf2);

            Point3d pointA2 = new Point3d(pointsA[pointsA.Count - 1]);
            pointA2.Transform(xf1);
            Point3d pointB2 = new Point3d(pointsB[pointsB.Count - 1]);
            pointB2.Transform(xf2);

            var pA = new List<Point3d>(pointsA);
            pA.Add(pointA2);
            pA.Add(pointA1);
            pA.Add(new Point3d(pointsA[0]));

            var pB = new List<Point3d>(pointsB);
            pB.Add(pointB2);
            pB.Add(pointB1);
            pB.Add(new Point3d(pointsB[0]));

            PolylineCurve polylineA = new PolylineCurve(pA);
            PolylineCurve polylineB = new PolylineCurve(pB);
            curves.Add(polylineA);
            curves.Add(polylineB);

            return curves;
        }
        /// <summary>
        /// Calculate the Angle information of the main sampling point
        /// </summary>
        /// <param name="pointsMid">the main sampling point</param>
        /// <param name="pointsA">The end point of the floor where the main sampling point is located</param>
        /// <param name="pointsB">The point involved in calculating another section</param>
        /// <param name="elevationAngle">Elevation Angle</param>
        /// <param name="solidAngle">Solid Angle</param>
        /// <returns>The boundary line of the included Angle</returns>
        private List<List<Line>> CalculateView(List<Point3d> pointsMid,List<Point3d> pointsA, List<Point3d> pointsB, out List<double> elevationAngle, out List<double> solidAngle)
        {
            List<List<Line>> linesCollection = new List<List<Line>>();
            var elevationAngles = new List<double>();
            var solidAngles = new List<double>();
            Curve curve1 = null;
            Curve curve2 = null;

            Plane plane = Plane.WorldZX;
            foreach (var curve in Curves)
            {                
                curve.TryGetPlane(out plane);
                if (curve.Contains(pointsA[0], plane, 0.001) == PointContainment.Coincident)
                {
                    curve1 = curve;
                }
                else
                {
                    curve2 = curve;
                }
            }
            for (int i = 0; i < pointsMid.Count; i++)
            {
                List<Line> lines = new List<Line>();

                for(int j = 0; j < pointsA.Count; j++)
                {
                    if (pointsA[j].Z > pointsMid[i].Z)
                    {
                        var vector = pointsA[j] - pointsMid[i];
                        var line = new Line(pointsMid[i], vector, vector.Length * 5);
                        var crossing = Intersection.CurveCurve(curve1, line.ToNurbsCurve(), 0.001, 0.001);
                        if(crossing != null && crossing.Count <= 2)
                        {
                            if(crossing.Count == 2)
                            {
                                var point1 = crossing[0].PointA;
                                var point2 = crossing[1].PointA;
                                var pointMid = new Line(point1, point2).PointAt(0.5);
                                if (curve1.Contains(pointMid, plane, 0.001) != PointContainment.Inside)
                                {
                        lines.Add(line);
                        break;
                                }
                            }
                            else
                            {
                                lines.Add(line);
                                break;
                            }                                                       
                        }                                              
                    }
                }
                for(int k = 0; k < PointsB.Count; k++)
                {
                    if (pointsB[k].Z > pointsMid[i].Z)
                    {
                        var vector = pointsB[k] - pointsMid[i];
                        var line = new Line(pointsMid[i], vector, vector.Length * 5);
                        var crossing2 = Intersection.CurveCurve(curve2, line.ToNurbsCurve(), 0.001, 0.001);
                        if (crossing2.Count == 1)
                        {
                            lines.Add(line);
                            break;
                        }
                    }
                }               
                if(lines.Count == 2)
                {                  
                    var vector1 = lines[0].To - lines[0].From;
                    var vector2 = lines[1].To - lines[1].From;
                    var angle = Vector3d.VectorAngle(vector1, vector2);

                    var tempAngle1 = Vector3d.VectorAngle(vector1, Vector3d.XAxis);
                    var tempAngle2 = Vector3d.VectorAngle(vector2, Vector3d.XAxis);
                    var tempProject1 = (tempAngle1 < Math.PI / 2) ? tempAngle1 : Math.PI - tempAngle1;
                    var tempProject2 = (tempAngle2 < Math.PI / 2) ? tempAngle2 : Math.PI - tempAngle2;
                    if (tempProject1 - tempProject2 > 0)
                    {
                        linesCollection.Add(lines);
                        elevationAngles.Add(angle);
                        var ProjectAngle = angle / 2 + tempProject2;
                        solidAngles.Add(ProjectAngle);
                    }                
                }                               
            }
            elevationAngle = elevationAngles;
            solidAngle = solidAngles;
            return linesCollection;
        }

        private static List<List<double>> EnergyEachPoint(List<double> elevationAngle, List<double> solidAngle,Model model, 
            List<Point3d> pointsMid, List<Point3d> points, double lux, out double energiesAverage)
        {
            var samplePoints = model.SamplePoints;
            var sampleEnergy = new List<List<double>>();
            double energyTotal = 0.0;
            double energyAverage = 0.0;

            if (samplePoints.Count == pointsMid.Count)
            {
                int count = 0;
                for (int i = 0; i < pointsMid.Count; i++)
                {
                    var eachFloorEnergy = new List<double>();
                    double energy = 0.0;
                    try
                    {
                        energy = (1 + Math.Sin(solidAngle[i]) * lux * elevationAngle[i] * Math.Cos(solidAngle[i]) / 3);
                    }
                    catch
                    {
                        energy = 0.0;
                    }

            for (int j = 0; j < samplePoints[i].Count; j++)
            {
                double interiorAngle = Vector3d.VectorAngle(points[i] - samplePoints[i][j], points[i + 1] - samplePoints[i][j]);
                double energySamplePoint = energy * 1 * interiorAngle * Math.Cos(0) / 3;
                eachFloorEnergy.Add(energySamplePoint);
                energyTotal += energySamplePoint;
                        count++;
                    }
                    sampleEnergy.Add(eachFloorEnergy);
                }
                energyAverage = energyTotal / count;

                energiesAverage = energyAverage;
                return sampleEnergy;
            }
            else
            {
                energiesAverage = 0.0;
                return null;
            }

                
        }

    private static double StandardDeviation(List<List<double>> energyEachPoint, double energyAverage)
    {
        double deviation = 0.0;
        foreach(var i in energyEachPoint)
        {
            foreach(var j in i)
            {
                deviation += Math.Pow(j - energyAverage, 2);
            }
        }
        var standardDeviation = Math.Pow(deviation, 0.5);
        return standardDeviation;
    }

    }
}
