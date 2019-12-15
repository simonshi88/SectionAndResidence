using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Grasshopper;

namespace residence
{
    public class BuildingComponent : GH_Component
    {
         

        /// <summary>
        /// Initializes a new instance of the BuildingComponent class.
        /// </summary>
        public BuildingComponent()
          : base("Residential Arrangement", "Definition for the Residence",
              "the core programme for how to build the residence",
              "Complexity", "Residence")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "the points regard as buildings", GH_ParamAccess.list);
            pManager.AddNumberParameter("Radius", "R", "the radius of buildings", GH_ParamAccess.list);
            pManager.AddNumberParameter("Height", "H", "the height of buildings", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Object", "O", "result", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Buildings", "B", "return buildings", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> points = new List<Point3d>();
            List<double> r = new List<double>();
            List<double> h = new List<double>();

            if ((!DA.GetDataList(0, points)))
                return;
            if (!DA.GetDataList(1, r))
                return;
            if (!DA.GetDataList(2, h))
                return;

            List<Curve> sunRegulation = new List<Curve>();
            List<Curve> residence = new List<Curve>();
            List<Curve> spaceRegulation = new List<Curve>();
            List<Building> buildings = new List<Building>();

            for (int i = 0; i < points.Count; i++)
            {
                var building = new Building(points[i], r[i], h[i]);
                sunRegulation.Add(building.SunRegulation);
                residence.Add(building.Residence);
                spaceRegulation.Add(building.SpaceRegulation);
                buildings.Add(building);

            }

            DataTree<Curve> dataTree = new DataTree<Curve>();
            dataTree.AddRange(residence, new GH_Path(0));
            dataTree.AddRange(spaceRegulation, new GH_Path(1));
            dataTree.AddRange(sunRegulation, new GH_Path(2));

            DA.SetDataList(0, buildings);
            DA.SetDataTree(1, dataTree);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f7ec320e-0352-4ffe-b92d-e6d296315314"); }
        }
    }
}