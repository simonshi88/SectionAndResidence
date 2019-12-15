using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;


namespace residence
{
    class Move
    {
        public Plane Pln;
        public List<Building> Buildings;
        public Curve Boundary { get; set; }

        public Move(List<Building> bds, Plane PLN, Curve bo, int time)
        {
            Pln = PLN;
            Boundary = bo;
            Buildings = Apart(bds, time);
        }
        public bool Traversal(List<Building> buildings)
        {

            for (int i = 0; i < buildings.Count; i++)
            {
                for (int j = 0; j < buildings.Count; j++)
                {
                    Curve CI = buildings[i].SunRegulation;
                    Curve CJ = buildings[j].Residence;

                    if (i == j)
                        continue;
                    else
                    {
                        if (State(CJ, CI, Pln, 9) == CurveState.Inside)
                        {
                            buildings[j].MoveBuilding(new Vector3d((new Random().NextDouble() + 1) * buildings[j].Radius,
                                (new Random().NextDouble() + 1) * buildings[j].Radius, 0));
                            CJ = buildings[j].Residence;
                        }
                        var crossings = Rhino.Geometry.Intersect.Intersection.CurveCurve(CI, CJ, 0.0001, 0.0001);
                        if (crossings.Count > 1)
                            return true;
                    }
                }
            }
            return false;
        }

        public enum CurveState
        {
            Inside,
            IntersectionMajority,
            Intersection,
            Outside
        }
        public static CurveState State(Curve cA, Curve cB, Plane plane, int precision)
        {
            Point3d[] points;
            cA.DivideByCount(precision, true, out points);
            int count = 0;
            foreach (var point in points)
            {
                if (cB.Contains(point, plane, 0.0001) == PointContainment.Inside)
                    count++;
            }
            if (count == precision)
                return CurveState.Inside;
            else if (count >= precision / 2.0)
                return CurveState.IntersectionMajority;
            else if (count == 0)
                return CurveState.Outside;
            else
                return CurveState.Intersection;

        }

        public List<Building> Apart(List<Building> buildings, int time)
        {
            int count = 0;

            while (((Traversal(buildings) || TraversalBoundary(buildings, Boundary, Pln) || TraversalSpace(buildings))))
            {
                BoundaryTest(buildings, Boundary, Pln, 24);  
                SpaceApart(buildings);

                for (int i = 0; i < buildings.Count; i++)
                {
                    for (int j = 0; j < buildings.Count; j++)
                    {
                        if (i == j)
                            continue;
                        else
                        {
                            Curve CI = buildings[i].SunRegulation;
                            Curve CJ = buildings[j].Residence;

                            if (State(CJ, CI, Pln, 9) == CurveState.Inside)
                            {
                                buildings[j].MoveBuilding(new Vector3d((new Random().NextDouble() - 0.5) * buildings[j].Radius,
                                    (new Random().NextDouble() - 0.5) * buildings[j].Radius, 0));
                                CJ = buildings[j].Residence;
                            }

                            var crossings = Rhino.Geometry.Intersect.Intersection.CurveCurve(CI, CJ, 0.0001, 0.0001);

                            if (crossings.Count > 1)
                            {
                                var XA = new Point3d();
                                var XB = new Point3d();

                                double Max = CI.Domain.T1;
                                double t1 = 0;
                                double t2 = 0;
                                if ((crossings[1].ParameterA - crossings[0].ParameterA) < 0.5 * Max)
                                {
                                    t1 = 0.5 * (crossings[1].ParameterA + crossings[0].ParameterA);
                                }
                                else
                                {
                                    double diff = Max + crossings[0].ParameterA - crossings[1].ParameterA;
                                    t1 = (crossings[1].ParameterA + 0.5 * diff) % Max;
                                }
                                XA = CI.PointAt(t1);

                                Max = CJ.Domain.T1;
                                if ((crossings[1].ParameterB - crossings[0].ParameterB) < 0.5 * Max)
                                {
                                    if (State(CJ, CI, Pln, 9) == CurveState.IntersectionMajority)
                                        t2 = Math.Abs(Max / 2 - 0.5 * (crossings[1].ParameterB + crossings[0].ParameterB));
                                    else
                                        t2 = 0.5 * (crossings[1].ParameterB + crossings[0].ParameterB);
                                }
                                else
                                {
                                    double diff = Max + crossings[0].ParameterB - crossings[1].ParameterB;
                                    t2 = (crossings[1].ParameterB + 0.5 * diff) % Max;
                                }
                                XB = CJ.PointAt(t2);

                                var vectorJ = 0.6 * (XA - XB);
                                var vectorI = -0.4 * (XA - XB);
                                buildings[j].MoveBuilding(vectorJ);
                                buildings[i].MoveBuilding(vectorI);

                                if (!buildings.Contains(buildings[j])) buildings.Add(buildings[j]);
                                if (!buildings.Contains(buildings[i])) buildings.Add(buildings[i]);

                            }
                            else
                            {
                                if (!buildings.Contains(buildings[j])) buildings.Add(buildings[j]);
                                if (!buildings.Contains(buildings[i])) buildings.Add(buildings[i]);

                            }
                        }
                    }
                }
                if(count >= time)
                {
                    buildings.Remove(buildings[buildings.Count - 1]);
                }
                count += 1;
            }
            return buildings;
        }

        //Determine whether the residences intersect the boundary
        public static bool TraversalBoundary(List<Building> buildings, Curve boundary, Plane plane)
        {
            for (int i = 0; i < buildings.Count; i++)
            {
                Curve residence = buildings[i].Residence;
                if (State(residence, boundary, plane, 9) == CurveState.Intersection ||
                    State(residence, boundary, plane, 9) == CurveState.IntersectionMajority)
                    return true;
            }
            return false;
        }


        public static List<Building> BoundaryTest(List<Building> buildings, Curve boundary, Plane plane, int precision)
        {

            for (int i = 0; i < buildings.Count; i++)
            {
                Curve residence = buildings[i].Residence;

                Point3d[] points;
                residence.DivideByCount(precision, true, out points);
                var pointBoundaryCenter = boundary.GetBoundingBox(true).Center;
                var pointCenter = residence.GetBoundingBox(true).Center;
                var line = new LineCurve(pointBoundaryCenter, pointCenter);
                var crossings = Rhino.Geometry.Intersect.Intersection.CurveCurve(line, boundary, 0.001, 0.001);
                var vector = new Vector3d();
                for (int j = 0; j < points.Length; j++)
                {
                    if (boundary.Contains(points[j], plane, 0.0001) == PointContainment.Outside)
                    {
                        double t = new double();
                        boundary.ClosestPoint(points[j], out t);
                        vector = boundary.PointAt(t) - points[j];
                    }
                    
                }
                buildings[i].MoveBuilding(vector);
                if (!buildings.Contains(buildings[i])) buildings.Add(buildings[i]);

                else
                {
                    if (!buildings.Contains(buildings[i])) buildings.Add(buildings[i]);
                }

            }

            return buildings;
        }

        //Determine whether the residences satisfy the spacing regulation
        public static bool TraversalSpace(List<Building> buildings)
        {
            for (int i = 0; i < buildings.Count; i++)
            {
                for (int j = 0; j < buildings.Count; j++)
                {
                    if (i == j)
                        continue;
                    else
                    {
                        Curve CrvI = buildings[i].SpaceRegulation;
                        Curve CrvJ = buildings[j].SpaceRegulation;
                        Circle CI = new Circle();
                        Circle CJ = new Circle();

                        if (CrvI.IsCircle())
                            CrvI.TryGetCircle(out CI);
                        if (CrvJ.IsCircle())
                            CrvJ.TryGetCircle(out CJ);

                        var vector = CI.Center - CJ.Center;
                    if (vector.Length < (buildings[i].Regulation + buildings[j].Regulation) / 2)
                        return true;                              
                    }
                }
            }
            return false;
        }
        public static List<Building> SpaceApart(List<Building> buildings)
        {
            for (int i = 0; i < buildings.Count; i++)
            {
                for (int j = 0; j < buildings.Count; j++)
                {
                    if (i == j)
                        continue;
                    else
                    {
                        Curve CrvI = buildings[i].SpaceRegulation;
                        Curve CrvJ = buildings[j].SpaceRegulation;
                        Circle CI = new Circle();
                        Circle CJ = new Circle();

                        if (CrvI.IsCircle())
                            CrvI.TryGetCircle(out CI);
                        if (CrvJ.IsCircle())
                            CrvJ.TryGetCircle(out CJ);

                        var vector = CI.Center - CJ.Center;
                        if (vector.Length < (buildings[i].Regulation + buildings[j].Regulation) / 2)
                        {
                            buildings[i].MoveBuilding(0.5 * vector);
                            buildings[j].MoveBuilding(-0.5 * vector);

                            if (!buildings.Contains(buildings[j])) buildings.Add(buildings[j]);
                            if (!buildings.Contains(buildings[i])) buildings.Add(buildings[i]);
                        }
                        else
                        {
                            if (!buildings.Contains(buildings[j])) buildings.Add(buildings[j]);
                            if (!buildings.Contains(buildings[i])) buildings.Add(buildings[i]);
                        }

                    }
                }
            }
            return buildings;
        }
    }
}
