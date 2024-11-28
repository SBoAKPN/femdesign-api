using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FemDesign;
using FemDesign.Geometry;
using FemDesign.Shells;
using FemDesign.Reinforcement;
using FemDesign.GenericClasses;

namespace RetainingWall
{
    internal class ReinForcement
    {
        public static new List<FemDesign.GenericClasses.IStructureElement> reinfElements(List<Slab> slabs)
        {
            // Predefinition from input data
            // slabs
            Slab SlabBPL = slabs[0];
            Slab SlabMUR = slabs[1];

            var P_BPL = SlabBPL.SlabPart.Region.Contours[0].Points;
            var P_MUR = SlabMUR.SlabPart.Region.Contours[0].Points;

            // ----- INDATA -----

            var reinforcementMat = FemDesign.Materials.MaterialDatabase.GetDefault().MaterialByName("B500B");
            double diameter_BasArm_xB_BPL = 20e-3;
            double diameter_BasArm_xT_BPL = 20e-3;
            double diameter_BasArm_yB_BPL = 20e-3;
            double diameter_BasArm_yT_BPL = 20e-3;

            double diameter_ExtraArm_xB_BPL = 16e-3;
            double diameter_ExtraArm_xT_BPL = 16e-3;
            double diameter_ExtraArm_yB_BPL = 16e-3;
            double diameter_ExtraArm_yT_BPL = 16e-3;

            double cover_bottom_BPL = 100e-3;
            double cover_topp_BPL = 55e-3;

            var space_BasArm_BPL_xB_BPL = 0.20;
            var space_BasArm_BPL_xT_BPL = 0.20;
            var space_BasArm_BPL_yB_BPL = 0.20;
            var space_BasArm_BPL_yT_BPL = 0.20;

            var space_ExtraArm_BPL_xB_BPL = 0.40;
            var space_ExtraArm_BPL_xT_BPL = 0.40;
            var space_ExtraArm_BPL_yB_BPL = 0.40;
            var space_ExtraArm_BPL_yT_BPL = 0.40;

            Region region_BasArm_BPL = SlabBPL.Region;

            var P_ExtraArm_BPL = new List<Point3d>
            {
                new Point3d(0   ,0   ,0),
                new Point3d(2.5 ,0   ,0),
                new Point3d(2.5 ,1.0 ,0),
                new Point3d(0   ,1.0 ,0),
            };

            double diameter_BasArm_xB_MUR = 20e-3;
            double diameter_BasArm_xT_MUR = 20e-3;
            double diameter_BasArm_yB_MUR = 20e-3;
            double diameter_BasArm_yT_MUR = 20e-3;

            double diameter_ExtraArm_xB_MUR = 16e-3;
            double diameter_ExtraArm_xT_MUR = 16e-3;
            double diameter_ExtraArm_yB_MUR = 16e-3;
            double diameter_ExtraArm_yT_MUR = 16e-3;

            double cover_bottom_MUR = 100e-3;
            double cover_topp_MUR = 55e-3;

            var space_BasArm_MUR_xB_MUR = 0.20;
            var space_BasArm_MUR_xT_MUR = 0.20;
            var space_BasArm_MUR_yB_MUR = 0.20;
            var space_BasArm_MUR_yT_MUR = 0.20;

            var space_ExtraArm_MUR_xB_MUR = 0.40;
            var space_ExtraArm_MUR_xT_MUR = 0.40;
            var space_ExtraArm_MUR_yB_MUR = 0.40;
            var space_ExtraArm_MUR_yT_MUR = 0.40;

            Region region_BasArm_MUR = SlabMUR.Region;

            var P_ExtraArm_MUR = new List<Point3d>
            {
                new Point3d(P_MUR[1].X    ,0   , 3.5),
                new Point3d(P_MUR[1].X    ,0   , P_MUR[1].Z),
                new Point3d(P_MUR[2].X    ,1.0 , P_MUR[2].Z),
                new Point3d(P_MUR[2].X    ,1.0 , 3.5),
            };

            // ------------------

            // ---- BPL ----

            Region region_ExtraArm_BPL = new FemDesign.Geometry.Region(P_ExtraArm_BPL, Plane.XY);

            var wire_BasArm_xB_BPL = new Wire(diameter_BasArm_xB_BPL, reinforcementMat, WireProfileType.Ribbed);
            var wire_BasArm_xT_BPL = new Wire(diameter_BasArm_xT_BPL, reinforcementMat, WireProfileType.Ribbed);
            var wire_BasArm_yB_BPL = new Wire(diameter_BasArm_yB_BPL, reinforcementMat, WireProfileType.Ribbed);
            var wire_BasArm_yT_BPL = new Wire(diameter_BasArm_yT_BPL, reinforcementMat, WireProfileType.Ribbed);

            var wire_ExtraArm_xB_BPL = new Wire(diameter_ExtraArm_xB_BPL, reinforcementMat, WireProfileType.Ribbed);
            var wire_ExtraArm_xT_BPL = new Wire(diameter_ExtraArm_xT_BPL, reinforcementMat, WireProfileType.Ribbed);
            var wire_ExtraArm_yB_BPL = new Wire(diameter_ExtraArm_yB_BPL, reinforcementMat, WireProfileType.Ribbed);
            var wire_ExtraArm_yT_BPL = new Wire(diameter_ExtraArm_yT_BPL, reinforcementMat, WireProfileType.Ribbed);

            var BasArm_xB_BPL = new Straight(ReinforcementDirection.X, space_BasArm_BPL_xB_BPL, FemDesign.GenericClasses.Face.Bottom, cover_bottom_BPL);
            var BasArm_xT_BPL = new Straight(ReinforcementDirection.X, space_BasArm_BPL_xT_BPL, FemDesign.GenericClasses.Face.Top, cover_topp_BPL);
            var BasArm_yB_BPL = new Straight(ReinforcementDirection.Y, space_BasArm_BPL_yB_BPL, FemDesign.GenericClasses.Face.Bottom, cover_bottom_BPL);
            var BasArm_yT_BPL = new Straight(ReinforcementDirection.Y, space_BasArm_BPL_yT_BPL, FemDesign.GenericClasses.Face.Top, cover_topp_BPL);

            var ExtraArm_xB_BPL = new Straight(ReinforcementDirection.X, space_ExtraArm_BPL_xB_BPL, FemDesign.GenericClasses.Face.Bottom, cover_bottom_BPL);
            var ExtraArm_xT_BPL = new Straight(ReinforcementDirection.X, space_ExtraArm_BPL_xT_BPL, FemDesign.GenericClasses.Face.Top, cover_topp_BPL);
            var ExtraArm_yB_BPL = new Straight(ReinforcementDirection.Y, space_ExtraArm_BPL_yB_BPL, FemDesign.GenericClasses.Face.Bottom, cover_bottom_BPL);
            var ExtraArm_yT_BPL = new Straight(ReinforcementDirection.Y, space_ExtraArm_BPL_yT_BPL, FemDesign.GenericClasses.Face.Top, cover_topp_BPL);

            var ArmList_BPL = new List<Straight>
            {
                BasArm_xB_BPL,
                BasArm_xT_BPL,
                BasArm_yB_BPL,
                BasArm_yT_BPL,
                ExtraArm_xB_BPL,
                ExtraArm_xT_BPL,
                ExtraArm_yB_BPL,
                ExtraArm_yT_BPL,
            };

            var srfReinf_BPL = new List<SurfaceReinforcement>
            {
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_BPL,   BasArm_xB_BPL, wire_BasArm_xB_BPL),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_BPL,   BasArm_xT_BPL, wire_BasArm_xT_BPL),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_BPL,   BasArm_yB_BPL, wire_BasArm_yB_BPL),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_BPL,   BasArm_yT_BPL, wire_BasArm_yT_BPL),

                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_ExtraArm_BPL, ExtraArm_xB_BPL, wire_ExtraArm_xB_BPL),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_ExtraArm_BPL, ExtraArm_xT_BPL, wire_ExtraArm_xT_BPL),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_ExtraArm_BPL, ExtraArm_yB_BPL, wire_ExtraArm_yB_BPL),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_ExtraArm_BPL, ExtraArm_yT_BPL, wire_ExtraArm_yT_BPL),
            };

            var reinfBPL = FemDesign.Reinforcement.SurfaceReinforcement.AddReinforcementToSlab(SlabBPL, srfReinf_BPL);

            // ---- MUR ----

            Region region_ExtraArm_MUR = new FemDesign.Geometry.Region(P_ExtraArm_MUR, Plane.YZ);

            var wire_BasArm_xB_MUR = new Wire(diameter_BasArm_xB_MUR, reinforcementMat, WireProfileType.Ribbed);
            var wire_BasArm_xT_MUR = new Wire(diameter_BasArm_xT_MUR, reinforcementMat, WireProfileType.Ribbed);
            var wire_BasArm_yB_MUR = new Wire(diameter_BasArm_yB_MUR, reinforcementMat, WireProfileType.Ribbed);
            var wire_BasArm_yT_MUR = new Wire(diameter_BasArm_yT_MUR, reinforcementMat, WireProfileType.Ribbed);

            var wire_ExtraArm_xB_MUR = new Wire(diameter_ExtraArm_xB_MUR, reinforcementMat, WireProfileType.Ribbed);
            var wire_ExtraArm_xT_MUR = new Wire(diameter_ExtraArm_xT_MUR, reinforcementMat, WireProfileType.Ribbed);
            var wire_ExtraArm_yB_MUR = new Wire(diameter_ExtraArm_yB_MUR, reinforcementMat, WireProfileType.Ribbed);
            var wire_ExtraArm_yT_MUR = new Wire(diameter_ExtraArm_yT_MUR, reinforcementMat, WireProfileType.Ribbed);

            var BasArm_xB_MUR = new Straight(ReinforcementDirection.X, space_BasArm_MUR_xB_MUR, FemDesign.GenericClasses.Face.Bottom, cover_bottom_MUR);
            var BasArm_xT_MUR = new Straight(ReinforcementDirection.X, space_BasArm_MUR_xT_MUR, FemDesign.GenericClasses.Face.Top, cover_topp_MUR);
            var BasArm_yB_MUR = new Straight(ReinforcementDirection.Y, space_BasArm_MUR_yB_MUR, FemDesign.GenericClasses.Face.Bottom, cover_bottom_MUR);
            var BasArm_yT_MUR = new Straight(ReinforcementDirection.Y, space_BasArm_MUR_yT_MUR, FemDesign.GenericClasses.Face.Top, cover_topp_MUR);

            var ExtraArm_xB_MUR = new Straight(ReinforcementDirection.X, space_ExtraArm_MUR_xB_MUR, FemDesign.GenericClasses.Face.Bottom, cover_bottom_MUR);
            var ExtraArm_xT_MUR = new Straight(ReinforcementDirection.X, space_ExtraArm_MUR_xT_MUR, FemDesign.GenericClasses.Face.Top, cover_topp_MUR);
            var ExtraArm_yB_MUR = new Straight(ReinforcementDirection.Y, space_ExtraArm_MUR_yB_MUR, FemDesign.GenericClasses.Face.Bottom, cover_bottom_MUR);
            var ExtraArm_yT_MUR = new Straight(ReinforcementDirection.Y, space_ExtraArm_MUR_yT_MUR, FemDesign.GenericClasses.Face.Top, cover_topp_MUR);

            var ArmList_MUR = new List<Straight>
            {
                BasArm_xB_MUR,
                BasArm_xT_MUR,
                BasArm_yB_MUR,
                BasArm_yT_MUR,
                ExtraArm_xB_MUR,
                ExtraArm_xT_MUR,
                ExtraArm_yB_MUR,
                ExtraArm_yT_MUR,
            };

            var srfReinf_MUR = new List<SurfaceReinforcement>
            {
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_MUR,   BasArm_xB_MUR, wire_BasArm_xB_MUR),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_MUR,   BasArm_xT_MUR, wire_BasArm_xT_MUR),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_MUR,   BasArm_yB_MUR, wire_BasArm_yB_MUR),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_MUR,   BasArm_yT_MUR, wire_BasArm_yT_MUR),

                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_ExtraArm_MUR, ExtraArm_xB_MUR, wire_ExtraArm_xB_MUR),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_ExtraArm_MUR, ExtraArm_xT_MUR, wire_ExtraArm_xT_MUR),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_ExtraArm_MUR, ExtraArm_yB_MUR, wire_ExtraArm_yB_MUR),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_ExtraArm_MUR, ExtraArm_yT_MUR, wire_ExtraArm_yT_MUR),
            };

            var reinfMUR = FemDesign.Reinforcement.SurfaceReinforcement.AddReinforcementToSlab(SlabMUR, srfReinf_MUR);
            var reinfElements = new List<FemDesign.GenericClasses.IStructureElement> { reinfBPL, reinfMUR };
            return reinfElements;
        }
    }
}
