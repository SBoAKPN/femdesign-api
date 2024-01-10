// https://strusoft.com/
using System;
using Grasshopper.Kernel;

namespace FemDesign.Grasshopper
{
    public class MotionsPlasticLimitsDefine: FEM_Design_API_Component
    {
        public MotionsPlasticLimitsDefine(): base("MotionsPlasticLimits.Define", "Define", "Define translational stiffness values for plastic limits [kN, kN/m or kN/m2].", CategoryName.Name(), SubCategoryName.Cat1())
        {

        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("x_neg", "x_neg", "Kx' compression [kN, kN/m or kN/m2]. Default value empty means no plastic limit.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddNumberParameter("x_pos", "x_pos", "Kx' tension [kN, kN/m or kN/m2]. Default value empty means no plastic limit.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddNumberParameter("y_neg", "y_neg", "Ky' compression [kN, kN/m or kN/m2]. Default value empty means no plastic limit.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddNumberParameter("y_pos", "y_pos", "Ky' tension [kN, kN/m or kN/m2]. Default value empty means no plastic limit.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddNumberParameter("z_neg", "z_neg", "Kz' compression [kN, kN/m or kN/m2]. Default value empty means no plastic limit.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
            pManager.AddNumberParameter("z_pos", "z_pos", "Kz' tension [kN, kN/m or kN/m2]. Default value empty means no plastic limit.", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("MotionsPlasticLimits", "MotionsPlasticLimits", "MotionsPlasticLimits.", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double? x_neg = 0.00;
            double? x_pos = 0.00;

            double? y_neg = 0.00;
            double? y_pos = 0.00;

            double? z_neg = 0.00;
            double? z_pos = 0.00;

            DA.GetData(0, ref x_neg);
            DA.GetData(1, ref x_pos);

            DA.GetData(2, ref y_neg);
            DA.GetData(3, ref y_pos);

            DA.GetData(4, ref z_neg);
            DA.GetData(5, ref z_pos);

            FemDesign.Releases.MotionsPlasticLimits obj = new FemDesign.Releases.MotionsPlasticLimits(x_neg, x_pos, y_neg, y_pos, z_neg, z_pos);

            DA.SetData(0, obj);
        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return FemDesign.Properties.Resources.MotionsDefine;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("834b0c8b-5224-4719-832f-7561f91a3bd2"); }
        }

        public override GH_Exposure Exposure => GH_Exposure.quarternary;

    }
}