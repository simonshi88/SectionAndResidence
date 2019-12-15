using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace residence
{
    public class distanceComponent : GH_Component
    {      
        public distanceComponent()
          : base("Residential Arrangement", "Definition for Movement",
              "the core programme for custom movement",
              "Complexity", "Residence")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("polyline", "pl", "The circle to slice", GH_ParamAccess.list);
            pManager.AddPlaneParameter("plan", "P", "Slicing line", GH_ParamAccess.item);
            pManager.AddCurveParameter("Boundary", "B", "the site", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Cycle Number", "N", "Stop running until n steps", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
           
            pManager.AddGenericParameter("goals", "g", "sssss", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Buildings", "B", "return buildings", GH_ParamAccess.tree);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Building> buildings = new List<Building>();
            Plane plane = new Plane();
            Curve boundary = new PolylineCurve();
            int number = 0;
            if ((!DA.GetDataList(0, buildings)))
                return;
            if (!DA.GetData(1, ref plane))
                return;
            if (!DA.GetData(2, ref boundary))
                return;
            if (!DA.GetData(3, ref number))
                return;

            Move move = new Move(buildings, plane, boundary, number);

            List<Curve> sunRegulation = new List<Curve>();
            List<Curve> residence = new List<Curve>();
            List<Curve> spaceRegulation = new List<Curve>();
            List<Point3d> points = new List<Point3d>();

            for (int i = 0; i < buildings.Count; i++)
            {
                var building = buildings[i];
                sunRegulation.Add(building.SunRegulation);
                residence.Add(building.Residence);
                spaceRegulation.Add(building.SpaceRegulation);
                points.Add(building.Point);

            }

            DataTree<Curve> dataTree = new DataTree<Curve>();
            dataTree.AddRange(residence, new GH_Path(0));
            dataTree.AddRange(spaceRegulation, new GH_Path(1));
            dataTree.AddRange(sunRegulation, new GH_Path(2));

           

            DA.SetDataList(0, points);
            DA.SetDataTree(1, dataTree);

        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("daeaed41-e0f6-4953-a150-b4f1709dc006"); }
        }
    }

}
