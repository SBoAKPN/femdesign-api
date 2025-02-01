using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using System.Linq;
using Rhino.Geometry;
using FemDesign.Results;
using FemDesign.Loads;
using Rhino.Commands;
using Grasshopper.Kernel.Data;

namespace FemDesign.Grasshopper
{
    public class ParseCSV : FEM_Design_API_Component
    {
        /// <summary>
        /// Initializes a new instance of the NodalDisplacement class.
        /// </summary>
        public ParseCSV()
          : base("ParseCSV",
                "ParseCSV",
                "Try to convert the content of a .csv file into a result object. Not all the result type can be converted yet.",
                CategoryName.Name(), SubCategoryName.Cat7b())
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("CsvFilePath", "CsvFilePath", "File path of the listed results.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("Results", "Results", "");
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get indata
            var filePath = "";
            DA.GetData(0, ref filePath);

            var csvPaths = new List<string> { filePath };
            var results = Results.ResultsReader.ParseCsvFiles<Results.IResult>(csvPaths);
            // Set output
            DA.SetDataList(0, results);
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return FemDesign.Properties.Resources.parseAll;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{777BF0DA-CA41-4A8D-9CCB-57B88B33E0A4}"); }
        }
    }
}