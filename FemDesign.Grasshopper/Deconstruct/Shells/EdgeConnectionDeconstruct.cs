﻿// https://strusoft.com/
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FemDesign.Grasshopper
{
    public class EdgeConnectionDeconstruct : FEM_Design_API_Component
    {
        public EdgeConnectionDeconstruct() : base("EdgeConnection.Deconstruct", "Deconstruct", "Deconstruct a EdgeConnection element.", "FEM-Design", "Deconstruct")
        {

        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("EdgeConnection", "EdgeConnection", "EdgeConnection.", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Guid", "Guid", "Guid of edge connection.", GH_ParamAccess.item);
            pManager.AddTextParameter("AnalyticalID", "AnalyticalID", "Analytical element ID.", GH_ParamAccess.item);
            pManager.AddCurveParameter("Edge", "Edge", "", GH_ParamAccess.item);
            pManager.AddPlaneParameter("LocalPlane", "LocalPlane", "Plane oriented according to the edge local coordinate system.", GH_ParamAccess.item);
            pManager.AddTextParameter("PredefinedName", "PredefinedName", "Name of predefined type.", GH_ParamAccess.item);
            pManager.AddTextParameter("PredefinedGuid", "PredefinedGuid", "Guid of predefined type.", GH_ParamAccess.item);
            pManager.AddTextParameter("Friction", "Friction", "Friction.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Motions", "Motions", "Motions", GH_ParamAccess.item);
            pManager.AddGenericParameter("Rotations", "Rotations", "Rotations", GH_ParamAccess.item);
            pManager.Register_GenericParam("RigidityGroup", "RigidityGroup", "RigidityGroup");
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get input
            FemDesign.Shells.EdgeConnection obj = null;
            if (!DA.GetData(0, ref obj))
            {
                return;
            }
            if (obj == null)
            {
                return;
            }

            // set output
            DA.SetData(0, obj.Guid);
            DA.SetData(1, obj.Name);

            Curve edgeCurve = obj.Edge.ToRhino();
            DA.SetData(2, edgeCurve);

            // local coordinate system
            Point3d midPoint = edgeCurve.PointAtNormalizedLength(0.5);
            Point3d startPoint = edgeCurve.PointAtNormalizedLength(0.0);
            Point3d endPoint = edgeCurve.PointAtNormalizedLength(1.0);

            Vector3d xDir = new Vector3d(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y, endPoint.Z - startPoint.Z);
            Vector3d zDir = obj.Normal.ToRhino();
            Vector3d yDir = Vector3d.CrossProduct(xDir, zDir);

            Plane localPlane = new Plane(midPoint, xDir, yDir);
            DA.SetData(3, localPlane);

            // catch pre-defined rigidity
            if (obj.Rigidity != null)
            {
                DA.SetData(4, null);
                DA.SetData(5, null);
                if (obj.Rigidity._friction == null)
                {
                    DA.SetData(6, null);
                }
                else
                {
                    DA.SetData(6, obj.Rigidity.Friction);
                }
                DA.SetData(7, obj.Rigidity.Motions);
                DA.SetData(8, obj.Rigidity.Rotations);
            }
            else if (obj.Rigidity == null && obj.RigidityGroup != null)
            {
                DA.SetDataList(9, obj.RigidityGroup.Springs);
            }
            else
            {
                DA.SetData(4, obj.PredefRigidity.Name);
                DA.SetData(5, obj.PredefRigidity.Guid);
                if (obj.PredefRigidity.Rigidity._friction == null)
                {
                    DA.SetData(6, null);
                }
                else
                {
                    DA.SetData(6, obj.PredefRigidity.Rigidity.Friction);
                }
                DA.SetData(7, obj.PredefRigidity.Rigidity.Motions);
                DA.SetData(8, obj.PredefRigidity.Rigidity.Rotations);
            }
        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return FemDesign.Properties.Resources.EdgeConnectionDeconstruct;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("{82FD1C58-ED3B-4014-A076-7014F5EC6221}"); }
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;

    }
}