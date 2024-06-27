// https://strusoft.com/
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FemDesign.Grasshopper
{
    public class SlabWallVariableThickness_OBSOLETE2304 : FEM_Design_API_Component
    {
        public SlabWallVariableThickness_OBSOLETE2304(): base("Slab.WallVariableThickness", "WallVariable", "Create a wall element with variable thickness.", CategoryName.Name(), SubCategoryName.Cat2b())
        {

        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "Surface", "Surface must be flat and vertical", GH_ParamAccess.item);
            pManager.AddGenericParameter("Thickness", "Thickness", "Thickness. List of 2 items [t1, t2]. [m]", GH_ParamAccess.list);
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
            pManager.AddTextParameter("Identifier", "Identifier", "Identifier.", GH_ParamAccess.item, "W");
            pManager[pManager.ParamCount - 1].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Slab", "Slab", "Slab.", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get input
            
            Brep surface = null;
            if(!DA.GetData(0, ref surface)) { return; }
            
            List<FemDesign.Shells.Thickness> thickness = new List<FemDesign.Shells.Thickness>();
            if(!DA.GetDataList(1, thickness)) { return; }
            
            FemDesign.Materials.Material material = null;
            if(!DA.GetData(2, ref material)) { return; }
            
            FemDesign.Shells.ShellEccentricity eccentricity = FemDesign.Shells.ShellEccentricity.Default;
            if(!DA.GetData(3, ref eccentricity))
            {
                // pass
            }
            
            FemDesign.Shells.ShellOrthotropy orthotropy = FemDesign.Shells.ShellOrthotropy.Default;
            if(!DA.GetData(4, ref orthotropy))
            {
                // pass
            }
            
            FemDesign.Shells.EdgeConnection edgeConnection = FemDesign.Shells.EdgeConnection.Rigid;
            if(!DA.GetData(5, ref edgeConnection))
            {
                // pass
            }

            Rhino.Geometry.Vector3d x = Vector3d.Zero;
            if (!DA.GetData(6, ref x))
            {
                // pass
            }

            Rhino.Geometry.Vector3d z = Vector3d.Zero;
            if (!DA.GetData(7, ref z))
            {
                // pass
            }
            
            string identifier = "W";
            if(!DA.GetData(8, ref identifier))
            {
                // pass
            }
            if (surface == null || thickness == null || material == null || eccentricity == null || orthotropy == null || edgeConnection == null) { return; }
            if (thickness.Count != 2)
            {
                throw new System.ArgumentException("Thickness must contain exactly 2 items.");
            }

            // convert geometry
            FemDesign.Geometry.Region region = surface.FromRhino();

            //
            FemDesign.Shells.Slab obj = FemDesign.Shells.Slab.Wall(identifier, material, region, edgeConnection, eccentricity, orthotropy, thickness);

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
                return FemDesign.Properties.Resources.WallVariableThickness;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("fb3f47d6-f58d-42ec-9a24-14419b2dfa2f"); }
        }
        public override GH_Exposure Exposure => GH_Exposure.hidden;

    }
}