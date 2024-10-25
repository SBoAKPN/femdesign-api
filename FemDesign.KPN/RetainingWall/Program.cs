using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FemDesign;
using FemDesign.Calculate;
using FemDesign.GenericClasses;
using FemDesign.Geometry;
using FemDesign.Loads;
using FemDesign.Materials;
using FemDesign.ModellingTools;
using FemDesign.Reinforcement;
using FemDesign.Releases;
using FemDesign.Shells;
using StruSoft.Interop.StruXml.Data;

namespace FemDesign.RetainingWall
{
    internal class Program
    {
        static void Main()
        {
            // Parametric design of a retainingwall v0.0

            // ----- define Strucutre -----
            // ----- INDATA -----
            //t = thickness wx = widthx, wy = widthy, 
            var T1  = 0.60;
            var WX1 = 3.50; // 4
            var WY1 = 3.00;
            var WZ1 = 0.00;
            var T2 = 0.50;
            var WX2 = 0.6 + T2 / 2;
            var WY2 = WY1;
            var T3 = 0.50;
            var WZ3_1 = 5.00; // 6
            // Styvhet för upplag
            var KxNeg = 5e3; // Neg = compression 
            var KxPos = 5e3; // Pos = Tension
            var KyNeg = 5e3;
            var KyPos = 5e3;
            var KzNeg = 10e3;
            var KzPos = 0e3;
            // ------------------
            // ap = anchorpoint
            // del 1 bpl

            var AP1 = new Geometry.Point3d(0, 0, 0);

            // del 2 mur
            var WZ2_0 = WZ1;
            var WZ2_1 = WZ1 + T1/2;
            // Corner points of Slab
            var P2_0 = new Point3d(WX2, 0, WZ2_0);
            var P2_1 = new Point3d(WX2, 0, WZ2_1);
            var P2_2 = new Point3d(WX2, WY2, WZ2_1);
            var P2_3 = new Point3d(WX2, WY2, WZ2_0);

            // del 3 mur
            //var WX3 = 0.6 + T2 / 2;
            var WZ3_0 = WZ2_1;

            // Corner points of Slab
            var P3_0 = new Point3d(WX2, 0, WZ3_0);
            var P3_1 = new Point3d(WX2, 0, WZ3_1);
            var P3_2 = new Point3d(WX2, WY2, WZ3_1);
            var P3_3 = new Point3d(WX2, WY2, WZ3_0);

            //Define properties
            var materialDatabase = FemDesign.Materials.MaterialDatabase.GetDefault();
            var material = materialDatabase.MaterialByName("C35/45");

            // Define elements
            var Slab1 = FemDesign.Shells.Slab.Plate(AP1, WX1, WY1, T1, material);
            var Slab2 = FemDesign.Shells.Slab.FromFourPoints(P2_0, P2_1, P2_2, P2_3, T2, material, null , null, null,  "BPL-Mur");
            var Slab3 = FemDesign.Shells.Slab.FromFourPoints(P3_0, P3_1, P3_2, P3_3, T3, material, null, null, null, "Mur");

            // Define support
            var regionBPL = Slab1.Region;

            var motions1 = new FemDesign.Releases.Motions(KxNeg, KxPos, KyNeg, KyPos, KzNeg, KzPos);
            RigidityDataType1 rigidity1 = new RigidityDataType1(motions1);

            var SurfaceSupport1 = new FemDesign.Supports.SurfaceSupport(regionBPL, motions1,"BPL_Upplag");

            //var elements = new List<IStructureElement> { Slab1, Slab2, Slab3, SurfaceSupport1 };

            // ----- Define load cases -----
            var loadCaseDL      = new LoadCase("DL", LoadCaseType.DeadLoad, LoadCaseDuration.Permanent);
            var loadCaseR       = new LoadCase("Räcke", LoadCaseType.Static, LoadCaseDuration.Permanent);
            var loadCaseJU      = new LoadCase("JordU", LoadCaseType.Static, LoadCaseDuration.Permanent);
            var loadCaseJK      = new LoadCase("JordK", LoadCaseType.Static, LoadCaseDuration.Permanent);
            var loadCaseÖLVU    = new LoadCase("ÖLVU", LoadCaseType.Static, LoadCaseDuration.Permanent);
            var loadCaseÖLHU    = new LoadCase("ÖLHU", LoadCaseType.Static, LoadCaseDuration.Permanent);
            var loadCaseÖLVP    = new LoadCase("ÖLVP", LoadCaseType.Static, LoadCaseDuration.Permanent);
            var loadCaseÖLHP    = new LoadCase("ÖLHP", LoadCaseType.Static, LoadCaseDuration.Permanent);
            var loadCases = new List<LoadCase> { loadCaseDL, loadCaseR, loadCaseJU, loadCaseJK, loadCaseÖLVU, loadCaseÖLHU, loadCaseÖLVP, loadCaseÖLHP };

            // ----- Define load combination -----
            var loadComb = new LoadCombination("ULS_V-_H+", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.20), (loadCaseR, 1.2), (loadCaseJU, 1.1), (loadCaseÖLHU, 1.4));

            // ----- Define loads -----

            // ----- INDATA -----
            double qR       = 1;      // Räcke
            double qJ       = 22;     // Fyllning
            double q0JH     = 0;      // Start tryck från jordH
            double K0_ULS   = 0.39;   
            double K0_SLS   = 0.29;
            double qÖLVU    = 20;     // Överlast ULS
            double qÖLVP    = 10;     // Överlast Perm
            // ------------------

            // Räcke
            var LineR = Slab3.Region.Contours[0].Edges[1];
            
            var qR_L = new Vector3d(0, 0, -qR);
            var lineLoadR = new FemDesign.Loads.LineLoad(LineR, qR_L, loadCaseR, ForceLoadType.Force, "", true, false);

            // Jord U
            
            // V
            var qJV_L = new Vector3d(0, 0, -qJ*(WZ3_1-WZ3_0));
            var posJV = new Point3d(WX2 + T2 / 2, 0, WZ1);
            var regionBaktass = FemDesign.Geometry.Region.RectangleXY(posJV, WX1 - (WX2 + T2 / 2), WY1);

            var SurfaceLoadJUV = new FemDesign.Loads.SurfaceLoad(regionBaktass, qJV_L, loadCaseJU, false, "");

            // H
            var regionMur = Slab3.Region; // (WY2, WZ3_1);
            var LoaddirJH = new Vector3d(-1, 0, 0);
            double z0JH = WZ3_1;
            
            double qhJUH = qJ * K0_ULS;

            var PressureLoadJUH = new FemDesign.Loads.PressureLoad(regionMur, LoaddirJH, z0JH,q0JH,qhJUH, loadCaseJU,"",false,ForceLoadType.Force);

            // Jord K
            //  V
            var SurfaceLoadJKV = new FemDesign.Loads.SurfaceLoad(regionBaktass, qJV_L, loadCaseJK, false, "");

            // H

            double qhJKH = qJ * K0_SLS;
            var PressureLoadJKH = new FemDesign.Loads.PressureLoad(regionMur, LoaddirJH, z0JH, q0JH, qhJKH, loadCaseJK, "", false, ForceLoadType.Force);

            // ÖL U
            // V
            var qÖLVU_L = new Vector3d(0, 0, -qÖLVU);
            var SurfaceLoadÖLVU = new FemDesign.Loads.SurfaceLoad(regionBaktass, qÖLVU_L, loadCaseÖLVU, false, "");

            // H
            double qÖLHU = qÖLVU*K0_ULS;
            var qÖLHU_L = new Vector3d(-qÖLHU,0,0);
            var SurfaceLoadÖLHU = new FemDesign.Loads.SurfaceLoad(regionMur, qÖLHU_L, loadCaseÖLHU, false, "");

            // ÖL P
            // V

            var qÖLVP_L = new Vector3d(0, 0, -qÖLVP);
            var SurfaceLoadÖLVP = new FemDesign.Loads.SurfaceLoad(regionBaktass, qÖLVP_L, loadCaseÖLVP, false, "");

            // H
            double qÖLHP = qÖLVP * K0_SLS;
            var qÖLHP_L = new Vector3d(-qÖLHP, 0, 0);
            var SurfaceLoadÖLHP = new FemDesign.Loads.SurfaceLoad(regionMur, qÖLHP_L, loadCaseÖLHP, false, "");

            var loads = new List<ILoadElement> 
            { 
              lineLoadR,        
              SurfaceLoadJUV ,  
              PressureLoadJUH,  
              SurfaceLoadJKV,   
              PressureLoadJKH,  
              SurfaceLoadÖLVU,
              SurfaceLoadÖLHU,
              SurfaceLoadÖLVP,
              SurfaceLoadÖLHP
            };

            // ----- TODO - include reinforcement -----
            // Reinforcement from Example 8 - SlabReinforcement

            // define reinforcement properties
            var reinforcement = FemDesign.Materials.MaterialDatabase.GetDefault().MaterialByName("B500B");
            var diamenter = 0.020;
            var cover = 0.055;
            var space = 0.20;
            var wire = new Wire(diamenter, reinforcement, WireProfileType.Ribbed);

            var straight_x_top = new Straight(ReinforcementDirection.X, space, GenericClasses.Face.Top, cover);
            var straight_x_bottom = new Straight(ReinforcementDirection.X, space, GenericClasses.Face.Bottom, cover);

            var straight_y_top = new Straight(ReinforcementDirection.Y, space, GenericClasses.Face.Top, cover);
            var straight_y_bottom = new Straight(ReinforcementDirection.Y, space, GenericClasses.Face.Bottom, cover);

            var straight = new List<Straight>
            {
                straight_x_top,
                straight_x_bottom,
                straight_y_top,
                straight_y_bottom,
            };

            // create the straight reinforcement objects
            // Note: Region can be a subregion of the slab.

            var srfReinf = new List<SurfaceReinforcement>();
            foreach (var s in straight)
            {
                var straightReinf = SurfaceReinforcement.DefineStraightSurfaceReinforcement(regionBPL, s, wire);
                srfReinf.Add(straightReinf);
            }

            // define the slab with reinforcement
            var reinfSlab1 = FemDesign.Reinforcement.SurfaceReinforcement.AddReinforcementToSlab(Slab1, srfReinf);
            var reinfSlab3 = FemDesign.Reinforcement.SurfaceReinforcement.AddReinforcementToSlab(Slab3, srfReinf);

            //var elements = new List<FemDesign.GenericClasses.IStructureElement> { reinfSlab };
            var elements = new List<FemDesign.GenericClasses.IStructureElement> { Slab1, Slab2, Slab3, SurfaceSupport1, reinfSlab1, reinfSlab3 };

            //var model = new Model(Country.S, elements);

            // ----------

            // Assemble the model
            var model = new Model(Country.S);
            model.AddElements(elements);
            model.AddLoads(loads);
            model.AddLoadCases(loadCases);
            model.AddLoadCombinations(loadComb);

            // Define the analysis settings
            var analysis = Analysis.StaticAnalysis(calcCase: true, calccomb: true);
            // Define the design settings

            //var design = FemDesign.Calculate.Design(autoDesign: false, check: true, LoadCombination:true, applyChanges: true);

            // create a direct link to FEM-Design and comunicate with it
            using (var femDesign = new FemDesign.FemDesignConnection(keepOpen: true))
            {
                // Inside the "using..." we can send commands to FEM-Design.
                femDesign.Open(model);
                femDesign.RunAnalysis(analysis);
                //femDesign.RunDesign()
                //TODO reinforcement
            }
        }
    }
}