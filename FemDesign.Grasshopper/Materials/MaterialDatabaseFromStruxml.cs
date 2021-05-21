// https://strusoft.com/
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace FemDesign.Grasshopper
{
    public class MaterialDatabaseFromStruxml: GH_Component
    {
       public MaterialDatabaseFromStruxml(): base("MaterialDatabase.FromStruxml", "FromStruxml", "Load a custom MaterialDatabase from a .struxml file.", "FemDesign", "Materials")
       {

       }
       protected override void RegisterInputParams(GH_InputParamManager pManager)
       {
           pManager.AddTextParameter("FilePath", "FilePath", "File path to .struxml file.", GH_ParamAccess.item);
       } 
       protected override void RegisterOutputParams(GH_OutputParamManager pManager)
       {
           pManager.AddGenericParameter("MaterialDatabase", "MaterialDatabase", "MaterialDatabase representation of .struxml file.", GH_ParamAccess.item);
       }
       protected override void SolveInstance(IGH_DataAccess DA)
       {
           // get input
           string filePath = null;
           if (!DA.GetData(0, ref filePath)) { return; }
           if (filePath == null) { return; }

           //
           FemDesign.Materials.MaterialDatabase materialDatabase = FemDesign.Materials.MaterialDatabase.DeserializeStruxml(filePath);

           // set output
           DA.SetData(0, materialDatabase);
       }
       protected override System.Drawing.Bitmap Icon
       {
           get
           {
                return null;
           }
       }
       public override Guid ComponentGuid
       {
           get { return new Guid("831686d3-0300-4c76-9da6-28548fc9f36d"); }
       }

    }   
}