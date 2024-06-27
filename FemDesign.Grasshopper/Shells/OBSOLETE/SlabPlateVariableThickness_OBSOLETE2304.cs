// https://strusoft.com/
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FemDesign.Grasshopper
{
    public class SlabPlateVariableThickness_OBSOLETE2304 : FEM_Design_API_Component
    {
        public SlabPlateVariableThickness_OBSOLETE2304(): base("PlateVariableThickness.Construct", "Construct", "Construct a plate element with variable thickness.", CategoryName.Name(), SubCategoryName.Cat2b())
        {

        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "Surface", "Surface must be flat", GH_ParamAccess.item);
            pManager.AddGenericParameter("Thickness", "Thickness", "Thickness. List of 3 items [t1, t2, t3]. [m]", GH_ParamAccess.list);
            pManager.AddGenericParameter("Material", "Material", "Material.", GH_ParamAccess.item);
            pManager.AddGenericParameter("ShellEccentricity", "Eccentricity", "ShellEccentricity. Optional.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddGenericParameter("ShellOrthotropy", "Orthotropy", "ShellOrthotropy. Optional.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddGenericParameter("EdgeConnection", "EdgeConnection", "EdgeConnection. Optional.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddVectorParameter("LocalX", "LocalX", "Set local x-axis. Vector must be perpendicular to surface local z-axis. Local y-axis will be adjusted accordingly. Optional, local x-axis from surface coordinate system used if undefined.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddVectorParameter("LocalZ", "LocalZ", "Set local z-axis. Vector must be perpendicular to surface local x-axis. Local y-axis will be adjusted accordingly. Optional, local z-axis from surface coordinate system used if undefined.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true; 
            pManager.AddTextParameter("Identifier", "Identifier", "Identifier.", GH_ParamAccess.item, "P");
            pManager[pManager.ParamCount - 1].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Slab", "Slab", "Slab.", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get inputs            
            Brep surface = null;
            if(!DA.GetData(0, ref surface)) { return; }

            List<FemDesign.Shells.Thickness> thickness = new List<FemDesign.Shells.Thickness>();
            if(!DA.GetDataList(1, thickness)) { return; }
            
            FemDesign.Materials.Material material = null;
            if(!DA.GetData(2, ref material)) { return; }
            
            FemDesign.Shells.ShellEccentricity eccentricity = FemDesign.Shells.ShellEccentricity.Default;
            DA.GetData(3, ref eccentricity);
            
            FemDesign.Shells.ShellOrthotropy orthotropy = FemDesign.Shells.ShellOrthotropy.Default;
            DA.GetData(4, ref orthotropy);
            
            FemDesign.Shells.EdgeConnection edgeConnection = FemDesign.Shells.EdgeConnection.Rigid;
            DA.GetData(5, ref edgeConnection);

            Rhino.Geometry.Vector3d x = Vector3d.Zero;
            DA.GetData(6, ref x);

            Rhino.Geometry.Vector3d z = Vector3d.Zero;
            DA.GetData(7, ref z);
            
            string identifier = "P";
            DA.GetData(8, ref identifier);


            if (surface == null || thickness == null || material == null || eccentricity == null || orthotropy == null || edgeConnection == null) { return; }
            if (thickness.Count != 3)
            {
                throw new System.ArgumentException("Thickness must contain exactly 3 items.");
            }

            //
            FemDesign.Geometry.Region region = surface.FromRhino();

            //
            FemDesign.Shells.Slab obj = FemDesign.Shells.Slab.Plate(identifier, material, region, edgeConnection, eccentricity, orthotropy, thickness);

            // set local x-axis
            if (!x.Equals(Vector3d.Zero))
            {
                obj.SlabPart.LocalX = x.FromRhino();
            }

            // set local z-axis
            if (!z.Equals(Vector3d.Zero))
            {
                obj.SlabPart.LocalZ = z.FromRhino();
            }

            // return
            DA.SetData(0, obj);
        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return FemDesign.Properties.Resources.PlateVariableThickness;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("1a00487a-beed-49ba-b9e9-d4b45c201c4b"); }
        }
        public override GH_Exposure Exposure => GH_Exposure.hidden;

    }
}