// https://strusoft.com/
using System;
using System.Data.Common;
using Grasshopper.Kernel;
using GrasshopperAsyncComponent;
using Rhino.Commands;

namespace FemDesign.Grasshopper
{
    public class PipeOpen_OBSOLETE2305 : GH_AsyncComponent
    {
        public PipeOpen_OBSOLETE2305() : base("FEM-Design.OpenModel", "OpenModel", "Open model in FEM-Design.", CategoryName.Name(), SubCategoryName.Cat8())
        {
            BaseWorker = new ModelOpenWorker_OBSOLETE2305(this);
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Connection", "Connection", "FEM-Design connection.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Model", "Model", "Model to open or file path.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("RunNode", "RunNode", "If true node will execute. If false node will not execute.", GH_ParamAccess.item, true);
            pManager[pManager.ParamCount - 1].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Connection", "Connection", "FEM-Design connection.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Model", "Model", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Success", "Success", "", GH_ParamAccess.item);
        }
        protected override System.Drawing.Bitmap Icon => FemDesign.Properties.Resources.FEM_open;

        public override Guid ComponentGuid => new Guid("96dc72e0-c0c1-4081-ac2b-56be85905fb2");
        public override GH_Exposure Exposure => GH_Exposure.hidden;
    }

    /// <summary>
    /// https://github.com/specklesystems/GrasshopperAsyncComponent
    /// </summary>
    public class ModelOpenWorker_OBSOLETE2305 : WorkerInstance
    {
        /* INPUT */
        dynamic model = null;
        Model newModel = null;
        FemDesignConnection connection = null;
        bool runNode = false;

        /* OUTPUT */
        bool success = false;

        public ModelOpenWorker_OBSOLETE2305(GH_Component component) : base(component) { }

        public override void DoWork(Action<string, string> ReportProgress, Action Done)
        {
            //// ?? Check for task cancellation!
            //if (CancellationToken.IsCancellationRequested) return;
            try
            {
                if (connection == null)
                {
                    RuntimeMessages.Add((GH_RuntimeMessageLevel.Warning, "Connection is null."));
                    Done();
                    return;
                }

                if (connection.IsDisconnected)
                {
                    _success = false;
                    throw new Exception("Connection to FEM-Design have been lost.");
                }

                if (connection.HasExited)
                {
                    _success = false;
                    throw new Exception("FEM-Design have been closed.");
                }

                if (runNode == false)
                {
                    _success = false;
                    connection = null;
                    RuntimeMessages.Add((GH_RuntimeMessageLevel.Warning, "Run node set to false."));
                    Done();
                    return;
                }

                ReportProgress("", "");
                connection.Open(model.Value);
                newModel = connection.GetModel();
                success = true;
            }
            catch (Exception ex)
            {
                RuntimeMessages.Add( (GH_RuntimeMessageLevel.Error, ex.Message) );
                connection = null;
                success = false;
            }

            Done();
        }

        public override WorkerInstance Duplicate() => new ModelOpenWorker_OBSOLETE2305(Parent);

        public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
        {
            DA.GetData("Connection", ref connection);
            DA.GetData("Model", ref model);
            DA.GetData("RunNode", ref runNode);
        }

        public override void SetData(IGH_DataAccess DA)
        {
            foreach (var (level, message) in RuntimeMessages)
            {
                Parent.AddRuntimeMessage(level, message);
            }

            DA.SetData("Connection", connection);
            DA.SetData("Model", newModel);
            DA.SetData("Success", success);

        }
    }
}