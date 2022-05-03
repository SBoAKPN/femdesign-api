﻿using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using System.Linq;
using Rhino.Geometry;
using FemDesign.Results;

namespace FemDesign.Grasshopper
{
    public class BarDisplacement : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public BarDisplacement()
          : base("Results.BarDisplacement",
                "BarDisplacement",
                "Read the bar displacement for the elements",
                "FEM-Design",
                "Results")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Result", "Result", "Result to be Parse", GH_ParamAccess.list);
            pManager.AddTextParameter("LoadCase", "LoadCase", "Name of Load Case for which to return the results. Default value returns the displacement for the first load case", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("CaseIdentifier", "CaseIdentifier", "CaseIdentifier.", GH_ParamAccess.tree);
            pManager.AddTextParameter("ElementId", "ElementId", "Element Id", GH_ParamAccess.tree);
            pManager.AddNumberParameter("PositionResult", "PositionResult", "Position Result", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Translation", "Translation", "Element translations in local x, y, z for all nodes. [m]", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Rotation", "Rotation", "Element rotations in local x, y, z for all nodes. [rad]", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get indata
            List<FemDesign.Results.BarDisplacement> iResult = new List<FemDesign.Results.BarDisplacement>();
            DA.GetDataList("Result", iResult);

            string iLoadCase = null;
            DA.GetData("LoadCase", ref iLoadCase);

            // Read Result from Abstract Method
            Dictionary<string, object> result;

            try
            {
                result = FemDesign.Results.BarDisplacement.DeconstructBarDisplacements(iResult, iLoadCase);
            }
            catch (ArgumentException ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message);
                return;
            }

            var loadCases = (List<string>)result["CaseIdentifier"];
            var elementId = (List<string>)result["ElementId"];
            var positionResult = (List<double>)result["PositionResult"];
            var iTranslation = (List<FemDesign.Geometry.FdVector3d>)result["Translation"];
            var iRotation = (List<FemDesign.Geometry.FdVector3d>)result["Rotation"];

            // Convert the FdVector to Dynamo
            var oTranslation = iTranslation.Select(x => x.ToRhino());
            var oRotation = iRotation.Select(x => x.ToRhino());


            // Convert Data in DataTree structure
            var uniqueId = elementId.Distinct().ToList();

            DataTree<object> elementIdTree = new DataTree<object>();
            DataTree<object> positionResultTree = new DataTree<object>();
            DataTree<object> oTranslationTree = new DataTree<object>();
            DataTree<object> oRotationTree = new DataTree<object>();


            var i = 0;
            var j = 0;
            foreach (var id in uniqueId)
            {
                // find where the index in List where the Id is found
                var indexes = elementId.Select((value, index) => new { value, index })
                  .Where(a => string.Equals(a.value, id))
                  .Select(a => a.index);

                foreach (int index in indexes)
                {
                    //loadCasesTree.Add(loadCases.ElementAt(index), new GH_Path(i));
                    elementIdTree.Add(elementId.ElementAt(index), new GH_Path(i));
                    positionResultTree.Add(positionResult.ElementAt(index), new GH_Path(i));
                    oTranslationTree.Add(oTranslation.ElementAt(index), new GH_Path(i));
                    oRotationTree.Add(oRotation.ElementAt(index), new GH_Path(i));
                }
                i++;
            }

            // Set output
            DA.SetDataList(0, loadCases);
            DA.SetDataTree(1, elementIdTree);
            DA.SetDataTree(2, positionResultTree);
            DA.SetDataTree(3, oTranslationTree);
            DA.SetDataTree(4, oRotationTree);
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
            get { return new Guid("A30A0D60-ECBE-4B34-8B38-0F95E9Fbbbbb"); }
        }
    }
}