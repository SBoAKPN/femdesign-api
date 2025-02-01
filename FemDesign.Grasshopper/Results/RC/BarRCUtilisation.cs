using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using System.Linq;
using Rhino.Geometry;
using FemDesign.Results;

namespace FemDesign.Grasshopper
{
    public class BarRCUtilization : FEM_Design_API_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public BarRCUtilization()
          : base("BarRCUtilization",
                "BarRCUtilization",
                "Read the Bar concrete utilization results for the entire model",
                CategoryName.Name(), SubCategoryName.Cat7b())
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Result", "Result", "Result to be Parse", GH_ParamAccess.list);
            pManager.AddTextParameter("Combination Name", "Case/Comb Name", "Name of Load Case/Load Combination for which to return the results.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Id", "Id", "");
            pManager.Register_DoubleParam("Max", "Max", "Maximum utilisation from all the verifications");
            pManager.Register_DoubleParam("Sec", "Sec", "Section utilisation");
            pManager.Register_DoubleParam("St", "St", "Stirrup utilisation");
            pManager.Register_DoubleParam("C", "C", "Concrete utilisation");
            pManager.Register_DoubleParam("T", "T", "Torsional utilisation");
            pManager.Register_DoubleParam("CW", "CW", "Crack width");
            pManager.Register_StringParam("CaseIdentifier", "CaseIdentifier", "CaseIdentifier.");
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<FemDesign.Results.RCBarUtilization> iResult = new List<FemDesign.Results.RCBarUtilization>();
            if (!DA.GetDataList("Result", iResult)) return;

            string loadCombination = null;
            DA.GetData(1, ref loadCombination);

            // Return the unique load case - load combination
            var uniqueLoadCases = iResult.Select(n => n.CaseIdentifier).Distinct().ToList();

            if (loadCombination != null)
            {
                if (uniqueLoadCases.Contains(loadCombination, StringComparer.OrdinalIgnoreCase))
                {
                    iResult = iResult.Where(n => String.Equals(n.CaseIdentifier, loadCombination, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                else
                {
                    var warning = $"Load Combination '{loadCombination}' does not exist";
                    throw new ArgumentException(warning);
                }
            }


            var id = iResult.Select(x => x.Id);
            var max = iResult.Select(x => x.Max);
            var section = iResult.Select(x => x.SEC);
            var stirrup = iResult.Select(x => x.ST);
            var concrete = iResult.Select(x => x.C);
            var torsional = iResult.Select(x => x.T);
            var crackWidth = iResult.Select(x => x.CW);
            var caseIdentifier = iResult.Select(x => x.CaseIdentifier);


            // Set output
            DA.SetDataList(0, id);
            DA.SetDataList(1, section);
            DA.SetDataList(2, max);
            DA.SetDataList(3, stirrup);
            DA.SetDataList(4, concrete);
            DA.SetDataList(5, torsional);
            DA.SetDataList(6, crackWidth);
            DA.SetDataList(7, caseIdentifier);
        }

        public override GH_Exposure Exposure => GH_Exposure.quarternary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return FemDesign.Properties.Resources.Results;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{CCFE3FF1-E19C-408E-A7A4-D4A37E118D4D}"); }
        }
    }
}