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
using System.Net.Http.Headers;

namespace RetainingWall
{
    internal class ReinForcement
    {
        public static (List<Slab> , List<FemDesign.GenericClasses.IStructureElement>) straightReinfElements(List<Slab> slabs, string ReinfMaterial)
        {
            // Predefinition from input data
            // slabs
            Slab SlabBPL = slabs[0];
            Slab SlabMUR = slabs[1];
            Slab SlabBPLMUR = slabs[2];

            var P_BPL = SlabBPL.SlabPart.Region.Contours[0].Points;
            var P_MUR = SlabMUR.SlabPart.Region.Contours[0].Points;
            var P_BPLMUR = SlabBPLMUR.SlabPart.Region.Contours[0].Points;

            double ly = SlabMUR.SlabPart.Region.Contours[0].Edges[1].Length;

            var slabstoRemove = new List<Slab>{ SlabBPL , SlabMUR, SlabBPLMUR };

            // ----- INDATA -----

            var reinforcementMat = FemDesign.Materials.MaterialDatabase.GetDefault().MaterialByName(ReinfMaterial);
            double diameter_BasArm_xB_BPL = 20e-3;
            double diameter_BasArm_xT_BPL = 20e-3;
            double diameter_BasArm_yB_BPL = 20e-3;
            double diameter_BasArm_yT_BPL = 20e-3;

            double diameter_ExtArm_xB_BPL = 16e-3;
            double diameter_ExtArm_xT_BPL = 16e-3;
            double diameter_ExtArm_yB_BPL = 16e-3;
            double diameter_ExtArm_yT_BPL = 16e-3;

            double cover_bottom_BPL_Y = 100e-3;
            double cover_topp_BPL_Y = 55e-3;
            double cover_bottom_BPL_X = cover_bottom_BPL_Y + diameter_BasArm_xB_BPL;
            double cover_topp_BPL_X   = cover_topp_BPL_Y + diameter_BasArm_xT_BPL;
            
            var space_BasArm_BPL_xB_BPL = 0.20;
            var space_BasArm_BPL_xT_BPL = 0.20;
            var space_BasArm_BPL_yB_BPL = 0.20;
            var space_BasArm_BPL_yT_BPL = 0.20;

            var space_ExtArm_BPL_xB_BPL = 0.40;
            var space_ExtArm_BPL_xT_BPL = 0.40;
            var space_ExtArm_BPL_yB_BPL = 0.40;
            var space_ExtArm_BPL_yT_BPL = 0.40;

            //Region for reinforcement
            Region region_BasArm_BPL = SlabBPL.Region;

            var P_ExtArm_BPL = new List<Point3d>
            {
                new Point3d(0   , 0   , 0),
                new Point3d(1.5 , 0   , 0),
                new Point3d(1.5 , ly  , 0),
                new Point3d(0   , ly  , 0),
            };

            double diameter_BasArm_xB_MUR = 20e-3;
            double diameter_BasArm_xT_MUR = 20e-3;
            double diameter_BasArm_yB_MUR = 20e-3;
            double diameter_BasArm_yT_MUR = 20e-3;

            double diameter_ExtArm_xB_MUR = 16e-3;
            double diameter_ExtArm_xT_MUR = 16e-3;
            double diameter_ExtArm_yB_MUR = 16e-3;
            double diameter_ExtArm_yT_MUR = 16e-3;

            double cover_bottom_MUR_Y = 55e-3;
            double cover_topp_MUR_Y = 55e-3;
            double cover_bottom_MUR_X = cover_bottom_MUR_Y + diameter_BasArm_xB_MUR;
            double cover_topp_MUR_X = cover_topp_MUR_Y + diameter_BasArm_xT_MUR;

            var space_BasArm_MUR_xB_MUR = 0.20;
            var space_BasArm_MUR_xT_MUR = 0.20;
            var space_BasArm_MUR_yB_MUR = 0.20;
            var space_BasArm_MUR_yT_MUR = 0.20;

            var space_ExtArm_MUR_xB_MUR = 0.40;
            var space_ExtArm_MUR_xT_MUR = 0.40;
            var space_ExtArm_MUR_yB_MUR = 0.40;
            var space_ExtArm_MUR_yT_MUR = 0.40;

            //Region for reinforcement
            Region region_BasArm_MUR = SlabMUR.Region;
            Region region_BasArm_BPLMUR = SlabBPLMUR.Region;

            var P_ExtArm_MUR = new List<Point3d>
            {
                new Point3d(P_MUR[1].X       , 0   , 2.5),
                new Point3d(P_MUR[1].X       , 0   , P_MUR[1].Z),
                new Point3d(P_BPLMUR[2].X    , ly  , P_MUR[2].Z),
                new Point3d(P_BPLMUR[2].X    , ly  , 2.5),
            };

            // ------------------

            // ---- BPL ----

            Region region_ExtArm_BPL = new FemDesign.Geometry.Region(P_ExtArm_BPL, Plane.XY);

            var wire_BasArm_xB_BPL = new Wire(diameter_BasArm_xB_BPL, reinforcementMat, WireProfileType.Ribbed);
            var wire_BasArm_xT_BPL = new Wire(diameter_BasArm_xT_BPL, reinforcementMat, WireProfileType.Ribbed);
            var wire_BasArm_yB_BPL = new Wire(diameter_BasArm_yB_BPL, reinforcementMat, WireProfileType.Ribbed);
            var wire_BasArm_yT_BPL = new Wire(diameter_BasArm_yT_BPL, reinforcementMat, WireProfileType.Ribbed);

            var wire_ExtArm_xB_BPL = new Wire(diameter_ExtArm_xB_BPL, reinforcementMat, WireProfileType.Ribbed);
            var wire_ExtArm_xT_BPL = new Wire(diameter_ExtArm_xT_BPL, reinforcementMat, WireProfileType.Ribbed);
            var wire_ExtArm_yB_BPL = new Wire(diameter_ExtArm_yB_BPL, reinforcementMat, WireProfileType.Ribbed);
            var wire_ExtArm_yT_BPL = new Wire(diameter_ExtArm_yT_BPL, reinforcementMat, WireProfileType.Ribbed);

            var BasArm_xB_BPL = new Straight(ReinforcementDirection.X, space_BasArm_BPL_xB_BPL, FemDesign.GenericClasses.Face.Bottom, cover_bottom_BPL_X);
            var BasArm_xT_BPL = new Straight(ReinforcementDirection.X, space_BasArm_BPL_xT_BPL, FemDesign.GenericClasses.Face.Top, cover_topp_BPL_X);
            var BasArm_yB_BPL = new Straight(ReinforcementDirection.Y, space_BasArm_BPL_yB_BPL, FemDesign.GenericClasses.Face.Bottom, cover_bottom_BPL_Y);
            var BasArm_yT_BPL = new Straight(ReinforcementDirection.Y, space_BasArm_BPL_yT_BPL, FemDesign.GenericClasses.Face.Top, cover_topp_BPL_Y);

            var ExtArm_xB_BPL = new Straight(ReinforcementDirection.X, space_ExtArm_BPL_xB_BPL, FemDesign.GenericClasses.Face.Bottom, cover_bottom_BPL_X);
            var ExtArm_xT_BPL = new Straight(ReinforcementDirection.X, space_ExtArm_BPL_xT_BPL, FemDesign.GenericClasses.Face.Top, cover_topp_BPL_X);
            var ExtArm_yB_BPL = new Straight(ReinforcementDirection.Y, space_ExtArm_BPL_yB_BPL, FemDesign.GenericClasses.Face.Bottom, cover_bottom_BPL_Y);
            var ExtArm_yT_BPL = new Straight(ReinforcementDirection.Y, space_ExtArm_BPL_yT_BPL, FemDesign.GenericClasses.Face.Top, cover_topp_BPL_Y);

            var ArmList_BPL = new List<Straight>
            {
                BasArm_xB_BPL,
                BasArm_xT_BPL,
                BasArm_yB_BPL,
                BasArm_yT_BPL,
                ExtArm_xB_BPL,
                ExtArm_xT_BPL,
                ExtArm_yB_BPL,
                ExtArm_yT_BPL,
            };

            var srfReinf_BPL = new List<SurfaceReinforcement>
            {
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_BPL,   BasArm_xB_BPL, wire_BasArm_xB_BPL),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_BPL,   BasArm_xT_BPL, wire_BasArm_xT_BPL),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_BPL,   BasArm_yB_BPL, wire_BasArm_yB_BPL),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_BPL,   BasArm_yT_BPL, wire_BasArm_yT_BPL),

                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_ExtArm_BPL, ExtArm_xB_BPL, wire_ExtArm_xB_BPL),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_ExtArm_BPL, ExtArm_xT_BPL, wire_ExtArm_xT_BPL),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_ExtArm_BPL, ExtArm_yB_BPL, wire_ExtArm_yB_BPL),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_ExtArm_BPL, ExtArm_yT_BPL, wire_ExtArm_yT_BPL),
            };

            var reinfBPL = FemDesign.Reinforcement.SurfaceReinforcement.AddReinforcementToSlab(SlabBPL, srfReinf_BPL);

            // ---- MUR ----

            Region region_ExtArm_MUR = new FemDesign.Geometry.Region(P_ExtArm_MUR, Plane.YZ);

            var wire_BasArm_xB_MUR = new Wire(diameter_BasArm_xB_MUR, reinforcementMat, WireProfileType.Ribbed);
            var wire_BasArm_xT_MUR = new Wire(diameter_BasArm_xT_MUR, reinforcementMat, WireProfileType.Ribbed);
            var wire_BasArm_yB_MUR = new Wire(diameter_BasArm_yB_MUR, reinforcementMat, WireProfileType.Ribbed);
            var wire_BasArm_yT_MUR = new Wire(diameter_BasArm_yT_MUR, reinforcementMat, WireProfileType.Ribbed);

            var wire_ExtArm_xB_MUR = new Wire(diameter_ExtArm_xB_MUR, reinforcementMat, WireProfileType.Ribbed);
            var wire_ExtArm_xT_MUR = new Wire(diameter_ExtArm_xT_MUR, reinforcementMat, WireProfileType.Ribbed);
            var wire_ExtArm_yB_MUR = new Wire(diameter_ExtArm_yB_MUR, reinforcementMat, WireProfileType.Ribbed);
            var wire_ExtArm_yT_MUR = new Wire(diameter_ExtArm_yT_MUR, reinforcementMat, WireProfileType.Ribbed);

            var BasArm_xB_MUR = new Straight(ReinforcementDirection.X, space_BasArm_MUR_xB_MUR, FemDesign.GenericClasses.Face.Bottom, cover_bottom_MUR_X);
            var BasArm_xT_MUR = new Straight(ReinforcementDirection.X, space_BasArm_MUR_xT_MUR, FemDesign.GenericClasses.Face.Top, cover_topp_MUR_X);
            var BasArm_yB_MUR = new Straight(ReinforcementDirection.Y, space_BasArm_MUR_yB_MUR, FemDesign.GenericClasses.Face.Bottom, cover_bottom_MUR_Y);
            var BasArm_yT_MUR = new Straight(ReinforcementDirection.Y, space_BasArm_MUR_yT_MUR, FemDesign.GenericClasses.Face.Top, cover_topp_MUR_Y);

            var ExtArm_xB_MUR = new Straight(ReinforcementDirection.X, space_ExtArm_MUR_xB_MUR, FemDesign.GenericClasses.Face.Bottom, cover_bottom_MUR_X);
            var ExtArm_xT_MUR = new Straight(ReinforcementDirection.X, space_ExtArm_MUR_xT_MUR, FemDesign.GenericClasses.Face.Top, cover_topp_MUR_X);
            var ExtArm_yB_MUR = new Straight(ReinforcementDirection.Y, space_ExtArm_MUR_yB_MUR, FemDesign.GenericClasses.Face.Bottom, cover_bottom_MUR_Y);
            var ExtArm_yT_MUR = new Straight(ReinforcementDirection.Y, space_ExtArm_MUR_yT_MUR, FemDesign.GenericClasses.Face.Top, cover_topp_MUR_Y);

            var ArmList_MUR = new List<Straight>
            {
                BasArm_xB_MUR,
                BasArm_xT_MUR,
                BasArm_yB_MUR,
                BasArm_yT_MUR,
                ExtArm_xB_MUR,
                ExtArm_xT_MUR,
                ExtArm_yB_MUR,
                ExtArm_yT_MUR,
            };

            var srfReinf_MUR = new List<SurfaceReinforcement>
            {
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_MUR,   BasArm_xB_MUR, wire_BasArm_xB_MUR),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_MUR,   BasArm_xT_MUR, wire_BasArm_xT_MUR),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_MUR,   BasArm_yB_MUR, wire_BasArm_yB_MUR),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_MUR,   BasArm_yT_MUR, wire_BasArm_yT_MUR),

                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_ExtArm_MUR, ExtArm_xB_MUR, wire_ExtArm_xB_MUR),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_ExtArm_MUR, ExtArm_xT_MUR, wire_ExtArm_xT_MUR),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_ExtArm_MUR, ExtArm_yB_MUR, wire_ExtArm_yB_MUR),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_ExtArm_MUR, ExtArm_yT_MUR, wire_ExtArm_yT_MUR),
            };

            var srfReinf_BPLMUR = new List<SurfaceReinforcement>
            {
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_BPLMUR,   BasArm_xB_MUR, wire_BasArm_xB_MUR),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_BPLMUR,   BasArm_xT_MUR, wire_BasArm_xT_MUR),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_BPLMUR,   BasArm_yB_MUR, wire_BasArm_yB_MUR),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_BPLMUR,   BasArm_yT_MUR, wire_BasArm_yT_MUR),

                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_BPLMUR,   ExtArm_xB_MUR, wire_ExtArm_xB_MUR),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_BPLMUR,   ExtArm_xT_MUR, wire_ExtArm_xT_MUR),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_BPLMUR,   ExtArm_yB_MUR, wire_ExtArm_yB_MUR),
                SurfaceReinforcement.DefineStraightSurfaceReinforcement(region_BasArm_BPLMUR,   ExtArm_yT_MUR, wire_ExtArm_yT_MUR),
            };

            
            var reinfMUR = FemDesign.Reinforcement.SurfaceReinforcement.AddReinforcementToSlab(SlabMUR, srfReinf_MUR);
            var reinfBPLMUR = FemDesign.Reinforcement.SurfaceReinforcement.AddReinforcementToSlab(SlabBPLMUR, srfReinf_BPLMUR);

            var shearControlRegion = new ShearControlRegionType
            {
                BasePlate = reinfBPLMUR.SlabPart.Guid,
                IgnoreShearCheck = true, // Sätt ignore shear check till true
                X = 0.5 // Sätt x-koordinaten
            };
            var entities = new FemDesign.Entities();
            entities.NoShearControlRegions.Add(shearControlRegion);


            var straightReinfElements = new List<FemDesign.GenericClasses.IStructureElement> { reinfBPL, reinfMUR, reinfBPLMUR };
            

            return (slabstoRemove, straightReinfElements);
        }
        public static List<FemDesign.GenericClasses.IStructureElement> shearReinfElements (List<Slab> slabs, string ReinfMaterial)
        {
            // Predefinition from input data
            // slabs

            // ----- INDATA -----
            double diameter_BasArm_BPL = 12e-3;
            double diameter_ExtArm_BPL = 12e-3;

            double spaceX_BasArm_BPL = 400e-3;
            double spaceY_BasArm_BPL = 400e-3;

            double spaceX_ExtArm_BPL = 400e-3;
            double spaceY_ExtArm_BPL = 400e-3;

            //Region for reinforcement
            var P_ExtArm_BPL = new List<Point3d>
            {
                new Point3d(1   ,0   ,0),
                new Point3d(2.0 ,0   ,0),
                new Point3d(2.0 ,1.0 ,0),
                new Point3d(1   ,1.0 ,0),
            };
            double start_BPL = P_ExtArm_BPL[0].X;
            double end_BPL   = P_ExtArm_BPL[1].X;
            double distace_BPL = end_BPL - start_BPL;

            // ------------------

            Region region_BasArm_BPL = new FemDesign.Geometry.Region(P_ExtArm_BPL, Plane.XY);

            var reinforcementMat = FemDesign.Materials.MaterialDatabase.GetDefault().MaterialByName(ReinfMaterial);

            var wire_BPL = new Wire(diameter_BasArm_BPL, reinforcementMat, WireProfileType.Ribbed);

            var shearReinf = new Stirrups(region_BasArm_BPL, start_BPL, end_BPL, distace_BPL);
            //StirrupReinforcement

            //var test = 
            //var shearReinf2 = new FemDesign.Reinforcement.StirrupReinforcement();

            var shearReinfElements = new List<FemDesign.GenericClasses.IStructureElement> {  };
            return shearReinfElements;
        }
    }
}
