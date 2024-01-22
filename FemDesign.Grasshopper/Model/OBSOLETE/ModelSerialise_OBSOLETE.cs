// https://strusoft.com/
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;



namespace FemDesign.Grasshopper
{
    public class ModelSerialise_OBSOLETE: FEM_Design_API_Component
    {
        public ModelSerialise_OBSOLETE(): base("Model.Serialise", "Serialise", "Serialise a model to .struxml", CategoryName.Name(), SubCategoryName.Cat6())
        {

        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Model to open.", GH_ParamAccess.item);
            pManager.AddTextParameter("FilePathStruxml", "FilePathStruxml", "File path where to save the model as .struxml.\nIf not specified, the file will be saved using the name and location folder of your .gh script.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Done", "Done", "Returns true if model was serialized", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 
            FemDesign.Model model = null;

            // get data
            if(!DA.GetData(0, ref model))
            {
                return;
            }

            string filePath = null;
            if (!DA.GetData(1, ref filePath))
            {
                bool fileExist = OnPingDocument().IsFilePathDefined;
                if (!fileExist)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Save your .gh script or specfy a FilePath.");
                    return;
                }
                filePath = OnPingDocument().FilePath;
                filePath = System.IO.Path.ChangeExtension(filePath, "struxml");
            }

            // serialize model
            model.SerializeModel(filePath);

            if (_FileName.IsASCII(filePath))
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File path has special characters. This might cause problems.");
            
            // return true
            DA.SetData(0, true);
            }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return FemDesign.Properties.Resources.ModelSave;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("ba85f198-112f-404e-a759-8417308849d7"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.hidden;

    }
}