using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Section
{
    public class SectionComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SectionComponent()
          : base("Calculation", "Definition for the Calculation",
              "the core programme for how to calculate the data",
              "Complexity", "Section")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("AnalyseCurveA", "CA", "curve with section", GH_ParamAccess.item);
            pManager.AddGenericParameter("AnalyseCurveB", "CB", "curve with section", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("AnalyseCurveA", "CA", "curve with section", GH_ParamAccess.list);
            pManager.AddNumberParameter("BuildingA", "BA", "curve with section", GH_ParamAccess.tree);
            pManager.AddNumberParameter("BuildingB", "BB", "curve with section", GH_ParamAccess.tree);
            pManager.AddGenericParameter("BuildingB", "BB", "curve with section", GH_ParamAccess.tree);
            pManager.AddGenericParameter("BuildingB", "BB", "curve with section", GH_ParamAccess.tree);
            pManager.AddGenericParameter("EnergyEachPoint", "A", "curve with section", GH_ParamAccess.tree);
            pManager.AddGenericParameter("EnergyEachPoint", "B", "curve with section", GH_ParamAccess.tree);
            pManager.AddNumberParameter("EnergyAverage", "B", "curve with section", GH_ParamAccess.item);
            pManager.AddNumberParameter("StandardDeviationTotal", "B", "curve with section", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Model modelA = null;
            Model modelB = null;
            List<Point3d> pointsA = new List<Point3d>();
            List<Point3d> pointsB = new List<Point3d>();

            if (!DA.GetData(0, ref modelA))
                return;
            if (!DA.GetData(1, ref modelB))
                return;


            var section = new SectionCalculate(modelA, modelB);

            DataTree<double> dataTreeA = new DataTree<double>();
            dataTreeA.AddRange(section.ElevationAngleA, new GH_Path(0));
            dataTreeA.AddRange(section.ProjectAngleA, new GH_Path(1));
            DataTree<double> dataTreeB = new DataTree<double>();
            dataTreeB.AddRange(section.ElevationAngleB, new GH_Path(0));
            dataTreeB.AddRange(section.ProjectAngleB, new GH_Path(1));

            DataTree<Line> dataTreeLineA = new DataTree<Line>();
            for (int i = 0; i < section.ElevationLineA.Count; i++)
            {
                dataTreeLineA.AddRange(section.ElevationLineA[i], new GH_Path(i));
            }

            DataTree<Line> dataTreeLineB = new DataTree<Line>();
            for (int i = 0; i < section.ElevationLineB.Count; i++)
            {
                dataTreeLineB.AddRange(section.ElevationLineB[i], new GH_Path(i));
            }

            DataTree<double> dataTreeEneryA = new DataTree<double>();
            for (int i = 0; i < section.EnergyEachPointA.Count; i++)
            {
                dataTreeEneryA.AddRange(section.EnergyEachPointA[i], new GH_Path(i));
            }
            DataTree<double> dataTreeEneryB = new DataTree<double>();
            for (int i = 0; i < section.EnergyEachPointB.Count; i++)
            {
                dataTreeEneryB.AddRange(section.EnergyEachPointB[i], new GH_Path(i));
            }

            var energyAverage = (section.EnergyAverageA + section.EnergyAverageB) / 2;
            var standardDeviationTotal = (section.StandardDeviationTotalA + section.StandardDeviationTotalB) / 2;

            DA.SetDataList(0, section.Curves);
            DA.SetDataTree(1, dataTreeA);
            DA.SetDataTree(2, dataTreeB);
            DA.SetDataTree(3, dataTreeLineA);
            DA.SetDataTree(4, dataTreeLineB);
            DA.SetDataTree(5, dataTreeEneryA);
            DA.SetDataTree(6, dataTreeEneryB);
            DA.SetData(7, energyAverage);
            DA.SetData(8, standardDeviationTotal);

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
            get { return new Guid("59beb968-7666-49d0-b180-e49175ac5168"); }
        }
    }
}
