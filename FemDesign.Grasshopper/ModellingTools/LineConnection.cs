﻿// https://strusoft.com/
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FemDesign.Grasshopper
{
    public class LineConnection : FEM_Design_API_Component
    {
        public LineConnection() : base("LineConnection", "LnConnect", "Construct a Line Connection.", "FEM-Design", "ModellingTools")
        {

        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("ElementsToConnect", "Elements", "Structural elements to be connected (bars, slabs, supports, etc.).", GH_ParamAccess.list);
            
            pManager.AddCurveParameter("MasterLine", "MLine", "LineCurve", GH_ParamAccess.item);
            pManager.AddCurveParameter("SlaveLine", "SLine", "LineCurve", GH_ParamAccess.item);

            pManager.AddGenericParameter("Motion", "Mot", "Motion.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddGenericParameter("MotionsPlasticLimits", "PlaLimM", "Plastic limits forces for motion springs. No plastic limits defined by default.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;

            pManager.AddGenericParameter("Rotation", "Rot", "Rotation.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddGenericParameter("RotationsPlaticLimits", "PlaLimR", "Plastic limits moments for rotation springs. No plastic limits defined by default.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;

            pManager.AddVectorParameter("LocalX", "X", "Set local x-axis. Vector must be perpendicular to Curve mid-point local x-axis.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddVectorParameter("LocalY", "Y", "Set local y-axis. Vector must be perpendicular to Curve mid-point local y-axis.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;

            pManager.AddTextParameter("Identifier", "ID", "Identifier.", GH_ParamAccess.item, "CL");
            pManager[pManager.ParamCount - 1].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("LineConnection", "LineConnection", "LineConnection.", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // default value to do not overwhelm the user by input
            bool movingLocal = true;
            double interfaceStart = 0.50;
            double interfaceEnd = 0.50;

            // get input
            var elements = new List<EntityBase>();
            DA.GetDataList(0, elements);

            Rhino.Geometry.Curve firstEdge = null;
            DA.GetData(1, ref firstEdge);

            Rhino.Geometry.Curve secondEdge = null;
            DA.GetData(2, ref secondEdge);


            Releases.Motions motions = null;
            if (!DA.GetData(3, ref motions))
            {
                motions = Releases.Motions.RigidLine();
            }

            Releases.MotionsPlasticLimits motLimits = null;
            DA.GetData(4, ref motLimits);

            Releases.Rotations rotations = null;
            if (!DA.GetData(5, ref rotations))
            {
                rotations = Releases.Rotations.RigidLine();
            }

            Releases.RotationsPlasticLimits rotLimits = null;
            DA.GetData(6, ref rotLimits);

            Plane plane;
            var averageCurve = Curve.CreateTweenCurves(firstEdge, secondEdge, 1, 0.01)[0];
            averageCurve.PerpendicularFrameAt(averageCurve.GetLength() / 2.0, out plane);

            Rhino.Geometry.Vector3d localX = plane.XAxis;
            DA.GetData(7, ref localX);
            if(localX == null || localX.IsZero)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "LocalX parameter cannot be null or zero.");
            }

            Rhino.Geometry.Vector3d localY = plane.YAxis;
            DA.GetData(8, ref localY);
            if (localY == null || localY.IsZero)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "LocalY parameter cannot be null or zero.");
            }

            string identifier = "CL";
            DA.GetData(9, ref identifier);

            GuidListType[] refs = new GuidListType[elements.Count];
            for (int idx = 0; idx < refs.Length; idx++)
            {
                if (elements[idx] is Shells.Slab slab)
                {
                    refs[idx] = new GuidListType(slab.SlabPart);
                }
                else if (elements[idx] is Bars.Bar bar)
                {
                    refs[idx] = new GuidListType(bar.BarPart);
                }
                else if (elements[idx] is Shells.Panel panel)
                {
                    refs[idx] = new GuidListType(panel.InternalPanels.IntPanels[0].Guid);
                }
                else
                {
                    refs[idx] = new GuidListType(elements[idx]);
                }

            }

            var rigidity = new Releases.RigidityDataType3(motions, motLimits, rotations, rotLimits);

            var connectedLines = new FemDesign.ModellingTools.ConnectedLines(firstEdge.FromRhino(), secondEdge.FromRhino(), localX.FromRhino(), localY.FromRhino(), rigidity, refs, identifier, movingLocal, interfaceStart, interfaceEnd);


            // output
            DA.SetData(0, connectedLines);

        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return FemDesign.Properties.Resources.LineConnection;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("{1240C784-AE43-45B4-9AA2-A75692708979}"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

    }
}