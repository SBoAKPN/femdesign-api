﻿// https://strusoft.com/
using System;
using System.Collections.Generic;
using System.Linq;

using FemDesign.ModellingTools;
using Grasshopper.Kernel;

namespace FemDesign.Grasshopper
{
    public class ModelDeconstruct : FEM_Design_API_Component
    {
        public ModelDeconstruct() : base("Model.Deconstruct", "Deconstruct", "Deconstruct Model.", "FEM-Design", "Deconstruct")
        {

        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Model.", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("CountryCode", "CountryCode", "National annex of calculation code: B/COMMON/D/DK/E/EST/FIN/GB/H/LT/N/NL/PL/RO/S/TR.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Foundations", "Foundations", "Single foundation element or list of foundation elements.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Bars", "Bars", "Single bar element or list of bar elements.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Shells", "Shells", "Single shell element or list of shell elements.", GH_ParamAccess.list);
            pManager.AddGenericParameter("LabelledSection", "LabelledSection", "Single LabelledSection element or list of LabelledSection elements.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Panels", "Panels", "Single panel element or list of panel elements.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Loads", "Loads", "Single load element or list of load elements.", GH_ParamAccess.list);
            pManager.AddGenericParameter("LoadCases", "LoadCases", "Single LoadCase element or list of LoadCase elements.", GH_ParamAccess.list);
            pManager.AddGenericParameter("LoadCombinations", "LoadCombinations", "Single LoadCombination element or list of LoadCombination elements.", GH_ParamAccess.list);
            pManager.AddGenericParameter("LoadGroups", "LoadGroups", "Single load group or list of LoadGroup elements to add", GH_ParamAccess.list);
            pManager.AddGenericParameter("Supports", "Supports", "Single Support element or list of Support elements.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Axes", "Axes", "Single axis element or list of axis elements.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Storeys", "Storeys", "Single storey element or list of storey elements.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Stages", "Stages", "Single Stage element of list of stage elements.", GH_ParamAccess.list);
            pManager.AddGenericParameter("PeakSmoothingRegion", "SmRegion", "Single peak smoothing region element or list of peak smoothing region elements.", GH_ParamAccess.list);
            pManager.AddGenericParameter("ResultPoints", "ResultPoints", "Single result point or list of result points.", GH_ParamAccess.list);
            pManager.AddGenericParameter("ModellingTools", "ModellingTools", "Modelling tools and Covers.", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // set references
            FemDesign.Model model = null;
            if (!DA.GetData("Model", ref model))
            {
                return;
            }
            if (model == null)
            {
                return;
            }

            List<StructureGrid.Axis> axes;
            if (model.Entities.Axes != null)
            {
                axes = model.Entities.Axes.Axis;
            }
            else
            {
                axes = null;
            }

            List<StructureGrid.Storey> storeys;
            if (model.Entities.Storeys != null)
            {
                storeys = model.Entities.Storeys.Storey;
            }
            else
            {
                storeys = null;
            }

            List<FemDesign.Stage> stages = null;
            if (model.ConstructionStages != null)
            {
                stages = model.ConstructionStages.Stages;
            }

            List<FemDesign.AuxiliaryResults.LabelledSection> labelledSections = null;
            if (model.Entities.LabelledSections != null)
            {
                labelledSections = model.Entities.LabelledSections.LabelledSections;
            }

            List<AuxiliaryResults.ResultPoint> resPoints;
            if (model.Entities.ResultPoints != null)
            {
                resPoints = model.Entities.ResultPoints.ResultPoints;
            }
            else
            {
                resPoints = null;
            }

            ModellingTools.AdvancedFem modTools = null;
            ModellingTools.AdvancedFem advancedFEM = model.Entities.AdvancedFem;
            bool hasConnPts = advancedFEM.ConnectedPoints?.Any() ?? false;
            bool hasConnLns = advancedFEM.ConnectedLines?.Any() ?? false;
            bool hasSrfConn = advancedFEM.SurfaceConnections?.Any() ?? false;
            bool hasFictBars = advancedFEM.FictitiousBars?.Any() ?? false;
            bool hasFictShells = advancedFEM.FictitiousShells?.Any() ?? false;
            bool hasDiaphragms = advancedFEM.Diaphragms?.Any() ?? false;
            bool hasCovers = advancedFEM.Covers?.Any() ?? false;

            if (hasConnPts || hasConnLns || hasSrfConn || hasFictBars || hasFictShells || hasDiaphragms || hasCovers)
            {
                modTools = advancedFEM;
            }

            // return data
            DA.SetData("CountryCode", model.Country.ToString());
            DA.SetDataList("Foundations", model.Entities.Foundations.GetFoundations());
            DA.SetDataList("Bars", model.Entities.Bars);
            DA.SetDataList("Shells", model.Entities.Slabs);
            DA.SetDataList("LabelledSection", labelledSections);
            DA.SetDataList("Panels", model.Entities.Panels);
            DA.SetDataList("Loads", model.Entities.Loads.GetLoads());
            DA.SetDataList("LoadCases", model.Entities.Loads.LoadCases);
            DA.SetDataList("LoadCombinations", model.Entities.Loads.LoadCombinations);
            DA.SetDataList("LoadGroups", model.Entities.Loads.GetLoadGroups());
            DA.SetDataList("Supports", model.Entities.Supports.GetSupports());
            DA.SetDataList("Axes", axes);
            DA.SetDataList("Storeys", storeys);
            DA.SetDataList("Stages", stages);
            DA.SetDataList("PeakSmoothingRegion", model.Entities.PeakSmoothingRegions);
            DA.SetDataList("ResultPoints", resPoints);
            DA.SetData("ModellingTools", modTools);
        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return FemDesign.Properties.Resources.ModelDeconstruct;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("{99062FE8-8F0D-4F2D-8483-77573958469C}"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

    }
}