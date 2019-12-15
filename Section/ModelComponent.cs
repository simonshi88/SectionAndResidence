using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace Section
{
    public class ModelComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ModelComponent()
          : base("Create Model", "Definition for the Model",
              "the core programme for how to build the model",
              "Complexity", "Section")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("OriginalPoint", "OP", "input the original point", GH_ParamAccess.item);
            pManager.AddNumberParameter("FloorLength", "L", "variable floor length", GH_ParamAccess.list);
            pManager.AddNumberParameter("StoreyHeight", "SH", "input storey height", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Floor", "F", "input the number of floors", GH_ParamAccess.item);
            pManager.AddNumberParameter("PercisionDistance", "PD", "percision of calculation sample ", GH_ParamAccess.item);
            pManager.AddNumberParameter("InitialHeight", "H", "percision of calculation sample ", GH_ParamAccess.item);
            pManager.AddBooleanParameter("ReverseDirection", "R", "reverse the direction of section", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("CalculationA", "CA", "Output intersection as points", GH_ParamAccess.list);
            pManager.AddGenericParameter("CalculationB", "CB", "Output intersection as points", GH_ParamAccess.tree);
            pManager.AddGenericParameter("CalculationB", "CB", "Output intersection as points", GH_ParamAccess.list);
            pManager.AddGenericParameter("CalculationB", "CB", "Output intersection as points", GH_ParamAccess.list);
            pManager.AddGenericParameter("CalculationB", "CB", "Output intersection as points", GH_ParamAccess.list);
            pManager.AddGenericParameter("CalculationB", "CB", "Output intersection as points", GH_ParamAccess.list);
            pManager.AddGenericParameter("CalculationB", "CB", "Output intersection as points", GH_ParamAccess.list);
            pManager.AddGenericParameter("CalculationB", "CB", "Output intersection as points", GH_ParamAccess.list);
            pManager.AddGenericParameter("CalculationB", "CB", "Output intersection as points", GH_ParamAccess.item);
            pManager.AddGenericParameter("Model", "M", "Output model", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d point = new Point3d();
            List<double> length = new List<double>();
            double height = 0.0;
            int floor = 0;
            double percision = 0.0;
            double initialHeight = 0;
            bool reverse = false;

            if (!DA.GetData(0, ref point))
                return;
            if (!DA.GetDataList(1, length))
                return;
            if (!DA.GetData(2, ref height))
                height = 3;
            if (!DA.GetData(3, ref floor))
                floor = 6;
            if (!DA.GetData(4, ref percision))
                percision = 5.0;
            if (!DA.GetData(5, ref initialHeight))
                initialHeight = 1.0;
            if (!DA.GetData(6, ref reverse))
                reverse = false;


            var model = new Model(point, reverse, length, height, floor,percision, initialHeight);

            DataTree<Point3d> dataTree = new DataTree<Point3d>();
            for(int i = 0; i < model.SamplePoints.Count; i++)
            {
                dataTree.AddRange(model.SamplePoints[i], new GH_Path(i));
            }

            DA.SetDataList(0, model.SampleLines);
            DA.SetDataTree(1, dataTree);
            DA.SetDataList(2, model.PointsA);
            DA.SetDataList(3, model.PointsB);
            
            DA.SetDataList(4, model.PointsMidA);
            DA.SetDataList(5, model.PointsMidB);
            DA.SetDataList(6, model.FloorLines);
            DA.SetDataList(7, model.SampleLines);
            DA.SetData(8, model.Polyline);
            DA.SetData(9, model);

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
            get { return new Guid("cbe06b3c-a97d-4258-802a-b6263c828eb2"); }
        }
    }
}