// https://strusoft.com/
using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using FemDesign.Foundations;
using FemDesign.Grasshopper.Extension.ComponentExtension;

namespace FemDesign.Grasshopper
{
    public class SlabFoundation : FEM_Design_API_Component
    {
        public SlabFoundation() : base("SlabFoundation", "SlabFoundation", "Create a slab foundation element.",
            CategoryName.Name(),
            SubCategoryName.Cat0())
        {

        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "Surface", "Surface must be flat.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness", "Thickness", "Thickness. [m]", GH_ParamAccess.item, 0.30);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddGenericParameter("Material", "Material", "Material.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bedding", "Bedding", "Bedding [kN/m2/m]", GH_ParamAccess.item, 10000);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddNumberParameter("BeddingX", "BeddingX", "BeddingX [kN/m2/m]", GH_ParamAccess.item, 5000);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddNumberParameter("BeddingY", "BeddingY", "BeddingY [kN/m2/m]", GH_ParamAccess.item, 5000);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddGenericParameter("Insulation", "Insulation", "Insulation.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddGenericParameter("ShellEccentricity", "Eccentricity", "ShellEccentricity. Optional.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddGenericParameter("ShellOrthotropy", "Orthotropy", "ShellOrthotropy. Optional.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddGenericParameter("EdgeConnection", "EdgeConnection", "EdgeConnection. Optional.", GH_ParamAccess.list);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddVectorParameter("LocalX", "LocalX", "Set local x-axis. Vector must be perpendicular to surface local z-axis. Local y-axis will be adjusted accordingly. Optional, local x-axis from surface coordinate system used if undefined.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddVectorParameter("LocalZ", "LocalZ", "Set local z-axis. Vector must be perpendicular to surface local x-axis. Local y-axis will be adjusted accordingly. Optional, local z-axis from surface coordinate system used if undefined.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddTextParameter("Identifier", "Identifier", "Identifier. Optional.", GH_ParamAccess.item, "F");
            pManager[pManager.ParamCount - 1].Optional = true;

        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("SlabFoundation", "SlabFoundation", "SlabFoundation.", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get inputs
            Brep surface = null;
            if (!DA.GetData(0, ref surface)) { return; }

            double thickness = 0.15;
            DA.GetData(1, ref thickness);

            FemDesign.Materials.Material material = null;
            if (!DA.GetData(2, ref material)) { return; }

            double bedding = 10000;
            DA.GetData(3, ref bedding);

            double beddingX = 5000;
            DA.GetData(4, ref beddingX);

            double beddingY = 5000;
            DA.GetData(5, ref beddingY);

            FemDesign.Foundations.Insulation insulation = null;
            DA.GetData(6, ref insulation);

            FemDesign.Shells.ShellEccentricity eccentricity = FemDesign.Shells.ShellEccentricity.Default;
            DA.GetData(7, ref eccentricity);

            FemDesign.Shells.ShellOrthotropy orthotropy = FemDesign.Shells.ShellOrthotropy.Default;
            DA.GetData(8, ref orthotropy);

            List<FemDesign.Shells.EdgeConnection> edgeConnections = new List<Shells.EdgeConnection>();
            DA.GetDataList(9, edgeConnections);

            Rhino.Geometry.Vector3d x = Vector3d.Zero;
            DA.GetData(10, ref x);

            Rhino.Geometry.Vector3d z = Vector3d.Zero;
            DA.GetData(11, ref z);

            string identifier = "F";
            DA.GetData(12, ref identifier);

            // check inputs
            if (surface == null || material == null || eccentricity == null || orthotropy == null || identifier == null) { return; }

            // convert geometry
            FemDesign.Geometry.Region region = surface.FromRhino();

            // get thickness
            List<FemDesign.Shells.Thickness> thicknessObj = new List<FemDesign.Shells.Thickness>();
            thicknessObj.Add(new FemDesign.Shells.Thickness(region.Plane.Origin, thickness));


            // create a slab plate
            var obj = FemDesign.Foundations.SlabFoundation.Plate(identifier, material, region, null, eccentricity, orthotropy, thicknessObj, bedding, beddingX, beddingY, insulation);


            // set edge connections on slab
            obj.SlabPart.Region.SetEdgeConnections(edgeConnections);

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

            DA.SetData(0, obj);

        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return FemDesign.Properties.Resources.SlabFoundation;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("{249ABF35-8D75-4A99-8141-3E75707BC770}"); }
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;

    }
}