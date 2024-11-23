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
using FemDesign.Results;
using FemDesign.Shells;
using FemDesign.Utils;
using StruSoft.Interop.StruXml.Data;

namespace FemDesign.RetainingWall
{
    internal class Program
    {
        static void Main()
        {
            // Parametric design of a retainingwall v0.0

            // ----- Define Strucutre -----

            // ----- INDATA -----

            // Corner points of BPL (part1)
            // X-kord , Y-kord , Z-kord, thickness
            var PP1_0 = new List<double> { 0, 0, 0, 1.0 };
            var PP1_1 = new List<double> { 4.0, 0, 0, 0.9 };
            var PP1_2 = new List<double> { 4.0, 1.0, 0, 0.9 };
            var PP1_3 = new List<double> { 0, 1.0, 0 };
            // Corner points of MUR (parSlabThickness2)
            // X-kord [0] , Y-kord [1], Z-kord [2], thickness [3]
            var PP2_0 = new List<double> { 0.5 + 1.0 / 2, 0, 6.0, 0.5 };
            var PP2_1 = new List<double> { 0.5 + 1.0 / 2, 0, PP1_0[3], 1.0 };
            var PP2_2 = new List<double> { 0.5 + 1.0 / 2, 1, PP1_0[3], 1.0 };
            var PP2_3 = new List<double> { 0.5 + 1.0 / 2, 1, 6.0 };

            // Styvhet för upplag
            // KxNeg [0] , KxPos [1], KyNeg [2] , KyPos [3], KzNeg [4] , KzPos [5]
            // Neg = compression, Pos = Tension
            var KSupp = new List<double> { 5e3, 5e3, 5e3, 5e3, 10e3, 0 };

            double creepUls = 0.5;
            double creepSlq = 0.5;
            double creepSlf = 0.5;
            double creepSlc = 0.5;
            double shrinkage = 0.5;

            // ------------------

            // Corner points of Slab1 BPL 
            var P1_0 = new Point3d(PP1_0[0], PP1_0[1], PP1_0[2]);
            var P1_1 = new Point3d(PP1_1[0], PP1_1[1], PP1_1[2]);
            var P1_2 = new Point3d(PP1_2[0], PP1_2[1], PP1_2[2]);
            var P1_3 = new Point3d(PP1_3[0], PP1_3[1], PP1_3[2]);
            // Points for thickness
            var P1 = new List<Point3d> { P1_0, P1_1, P1_2 };
            var SlabThickness1 = new List<double> { PP1_0[3], PP1_1[3], PP1_2[3] };

            // Corner points of Slab2 MUR 
            var P2_0 = new Point3d(PP2_0[0], PP2_0[1], PP2_0[2]);
            var P2_1 = new Point3d(PP2_1[0], PP2_1[1], PP2_1[2]);
            var P2_2 = new Point3d(PP2_2[0], PP2_2[1], PP2_2[2]);
            var P2_3 = new Point3d(PP2_3[0], PP2_3[1], PP2_3[2]);
            // Points for thickness
            var P2 = new List<Point3d> { P2_0, P2_1, P2_2 };
            var SlabThickness2 = new List<double> { PP2_0[3], PP2_1[3], PP2_2[3] };

            // Corner points of Slab 3 BPL-MUR
            // X-kord , Y-kord , Z-kord, thickness
            var PP3_0 = new List<double> { PP2_1[0], PP2_1[1], PP2_1[2], PP1_0[3] };
            var PP3_1 = new List<double> { PP2_1[0], PP2_1[1], PP1_0[2], PP1_0[3] };
            var PP3_2 = new List<double> { PP2_2[0], PP2_2[1], PP1_3[2], PP1_0[3] };
            var PP3_3 = new List<double> { PP2_2[0], PP2_2[1], PP2_2[2] };

            // Corner points of Slab3 (Point) 
            var P3_0 = new Point3d(PP3_0[0], PP3_0[1], PP3_0[2]);
            var P3_1 = new Point3d(PP3_1[0], PP3_1[1], PP3_1[2]);
            var P3_2 = new Point3d(PP3_2[0], PP3_2[1], PP3_2[2]);
            var P3_3 = new Point3d(PP3_3[0], PP3_3[1], PP3_3[2]);

            //Define properties
            var materialDatabase = FemDesign.Materials.MaterialDatabase.GetDefault();
            Material materialBtg = materialDatabase.MaterialByName("C35/45");
            //var materialBtgNoEG = materialDatabase.MaterialByName("C35/45NoEG");

            Material materialBtgNoMass = materialDatabase.MaterialByName("C35/45");
            materialBtgNoMass.Concrete.Mass = 0;
            materialBtgNoMass.Name = "C35/45NoMass";
            //materialBtgNoMass.Guid = System.Guid.NewGuid(); 

            materialBtg = FemDesign.Materials.Material.ConcreteMaterialProperties(materialBtg, creepUls, creepSlq, creepSlf, creepSlc, shrinkage);

            // Define elements
            var SlabAlignBottom = new ShellEccentricity(VerticalAlignment.Bottom, 0, false, false);
            var Slab1 = FemDesign.Shells.Slab.FromFourPoints(P1_0, P1_1, P1_2, P1_3, PP1_0[3], materialBtg, null, SlabAlignBottom, null, "BPL");
            var Slab2 = FemDesign.Shells.Slab.FromFourPoints(P2_0, P2_1, P2_2, P2_3, PP2_0[3], materialBtg, null, null, null, "Mur");
            var Slab3 = FemDesign.Shells.Slab.FromFourPoints(P3_0, P3_1, P3_2, P3_3, PP3_0[3], materialBtgNoMass, null, null, null, "BPL-Mur");

            Slab1.UpdateThickness(P1, SlabThickness1);
            Slab2.UpdateThickness(P2, SlabThickness2);

            // Define support   
            var regionBPL = Slab1.Region;

            var motions1 = new FemDesign.Releases.Motions(KSupp[0], KSupp[1], KSupp[2], KSupp[3], KSupp[4], KSupp[5]);

            var SurfaceSupport1 = new FemDesign.Supports.SurfaceSupport(regionBPL, motions1, "BPL_Upplag");

            //var elements = new List<IStructureElement> { Slab1, Slab2, Slab3, SurfaceSupport1 };

            // ----- Define load cases -----
            LoadCase loadCaseDL = new LoadCase("DL", LoadCaseType.DeadLoad, LoadCaseDuration.Permanent);
            LoadCase loadCaseR = new LoadCase("Räcke", LoadCaseType.Static, LoadCaseDuration.Permanent);

            LoadCase loadCaseJU = new LoadCase("JordU", LoadCaseType.Static, LoadCaseDuration.Permanent);
            LoadCase loadCaseJK = new LoadCase("JordK", LoadCaseType.Static, LoadCaseDuration.Permanent);
            LoadCase loadCaseJA = new LoadCase("JordA", LoadCaseType.Static, LoadCaseDuration.Permanent);

            LoadCase loadCaseHHW = new LoadCase("HHW", LoadCaseType.Static, LoadCaseDuration.Permanent);
            LoadCase loadCaseMW = new LoadCase("MW", LoadCaseType.Static, LoadCaseDuration.Permanent);
            LoadCase loadCaseLLW = new LoadCase("LLW", LoadCaseType.Static, LoadCaseDuration.Permanent);

            LoadCase loadCaseEffJHHW = new LoadCase("Eff J HHW", LoadCaseType.Static, LoadCaseDuration.Permanent);
            LoadCase loadCaseEffJMW = new LoadCase("Eff J MW", LoadCaseType.Static, LoadCaseDuration.Permanent);
            LoadCase loadCaseEffJLLW = new LoadCase("Eff J LLW", LoadCaseType.Static, LoadCaseDuration.Permanent);

            LoadCase loadCaseÖLVU = new LoadCase("ÖLVU", LoadCaseType.Static, LoadCaseDuration.Permanent);
            LoadCase loadCaseÖLHU = new LoadCase("ÖLHU", LoadCaseType.Static, LoadCaseDuration.Permanent);

            LoadCase loadCaseÖLVP = new LoadCase("ÖLVP", LoadCaseType.Static, LoadCaseDuration.Permanent);
            LoadCase loadCaseÖLHP = new LoadCase("ÖLHP", LoadCaseType.Static, LoadCaseDuration.Permanent);

            LoadCase loadCaseAcc = new LoadCase("Olycka", LoadCaseType.Static, LoadCaseDuration.Permanent);

            var loadCases = new List<LoadCase> { loadCaseDL, loadCaseR, loadCaseJU, loadCaseJK, loadCaseJA, loadCaseHHW, loadCaseMW, loadCaseLLW, 
                                                 loadCaseEffJHHW, loadCaseEffJMW, loadCaseEffJLLW,loadCaseÖLVU, loadCaseÖLHU, loadCaseÖLVP, loadCaseÖLHP };

            //// CREATING LOAD CASES
            //var loadCasesDeadLoads = new List<LoadCase> { loadCaseDL, loadCaseR };

            //var loadCasesEarthloadsULS = new List<LoadCase> { loadCaseJU };
            //var loadCasesEarthloadsKar = new List<LoadCase> { loadCaseJK };

            //var loadCasesWaterloadsULS = new List<LoadCase> { loadCaseHHW, loadCaseEffJHHW, loadCaseLLW, loadCaseEffJLLW };
            //var loadCasesWaterloadsKar = new List<LoadCase> { loadCaseMW, loadCaseEffJMW };

            //var loadCasesSurchargeloadsULS = new List<LoadCase> { loadCaseÖLVU, loadCaseÖLHU };
            //var loadCasesSurchargeloadsPerm = new List<LoadCase> { loadCaseÖLVP, loadCaseÖLHP };

            //var loadCasesAccLoads = new List<LoadCase> { loadCaseAcc };

            //// FETCHING LOAD CATEGORY DATABASE
            //var loadCategoryDatabase = LoadCategoryDatabase.GetDefault();
            //LoadCategory loadCategory = loadCategoryDatabase.LoadCategoryByName("A");

            //// CREATING LOAD GROUPS
            //var LGPerm = new LoadGroupPermanent(1, 1.35, 1, 1, loadCasesDeadLoads, ELoadGroupRelationship.Entire, 1.2/1.35, "LGPerm");

            //var LGEarthUls = new LoadGroupPermanent(1, 1.35, 1, 1, loadCasesEarthloadsULS, ELoadGroupRelationship.Entire, 1.1/1.35, "LGEarthUls");
            //var LGEarthKar = new LoadGroupPermanent(1, 1, 1, 1, loadCasesEarthloadsKar, ELoadGroupRelationship.Entire, 1, "LGEarthKar");

            //var LGWaterULS = new LoadGroupPermanent(1, 1.35, 1, 1, loadCasesWaterloadsULS, ELoadGroupRelationship.Custom, 1.1 / 1.35, "LGWaterULS");
            //var LGWaterKar = new LoadGroupPermanent(1, 1.35, 1, 1, loadCasesWaterloadsKar, ELoadGroupRelationship.Entire, 1.1 / 1.35, "LGWaterKar");

            //var LGÖlUls = new LoadGroupTemporary(1.5, 0.75, 0.75, 0.0, true, loadCasesSurchargeloadsULS, ELoadGroupRelationship.Simultaneous, "LGÖlUls");
            //var LGÖlPerm = new LoadGroupTemporary(1.5, 0.75, 0.75, 0.0, true, loadCasesSurchargeloadsPerm, ELoadGroupRelationship.Simultaneous, "LGÖlPerm");

            ////  var LGAcc = new ModelGeneralLoadGroup(new LoadGroupAccidental(Safty factor 1.00, using psi1 = true, loadCasesAccLoads, ELoadGroupRelationship.Alternative, "LGAcc"));

            //var loadGroups = new List<LoadGroupBase>() { LGPerm, LGEarthUls, LGEarthKar, LGWaterULS, LGWaterKar, LGÖlUls, LGÖlPerm }; //, LGAcc };

            //// Wrap the load groups so that they can be added to the load group table 
            //var generalLoadGroups = new List<ModelGeneralLoadGroup>() 
            //{ 
            //    new ModelGeneralLoadGroup(LGPerm), 
            //    new ModelGeneralLoadGroup(LGEarthUls), 
            //    new ModelGeneralLoadGroup(LGEarthKar),
            //    new ModelGeneralLoadGroup(LGWaterULS),
            //    new ModelGeneralLoadGroup(LGWaterKar),
            //    new ModelGeneralLoadGroup(LGÖlUls),
            //    new ModelGeneralLoadGroup(LGÖlPerm),
            //};

            //// Generate ULS and SLS Combinations
            //LoadCombinationTable loadCombinationTable = new LoadCombinationTable();
            //CombineULS(loadGroups, loadCombinationTable);
            //CombineSLS(loadGroups, loadCombinationTable);

            // Combination method??? Ex EC 6.10.a Sup 1


            // ----- Define loads -----

            // ----- INDATA -----
            double qR = 1;      // Räckestyngd
            // --- Earth ---
            var VisningH = new List<double> { 1.0, 1.0 };    // Visningshöjd tillbakafyllning 
            // JordH Tillbakafyll_PO, Tillbakafyll_P1, Motfyll_PO, Motfyll_PO
            var JordH = new List<double> { 1.0, 1.0, 0.9, 0.9 }; // Tillbakafyllning visningshöjd (ÖK MUR) 0/1 / Motfyllning fyllningshöjd (UK BPL) 0/1
            var AlfaJ = new List<double> { Math.PI / 180 * 0, Math.PI / 180 * 0 }; // Marklutning utifrån Mur Tillbakafyllning, Motfyllning

            var qJ = new List<double> { 10, 1 };     // Fyllning Naturfuktig, Effektiv tunghet under GVY
            // Tillbakafyllning , Motfyllning
            var K_ULS  = new List<double> { 0.39, 0.39 };
            var K_Glid = new List<double> { 0.24, 0.39 };
            var K_SLS  = new List<double> { 0.29, 0.29 };
            // --- Water --
            double qW   = 10; // Vatten tyngd
            double HHW  = 4; // Vattenpelare från UK BPL 
            double MW   = 2;
            double LLW  = 0;
            // --- Surcharge ---
            double QÖLU0    = (270 + 180) / 2;     // Överlaststyngd (kN)
            double LU0      = 2.2;                   // Längd för spridning vid ytan
            double BU0      = 3.0;                   // Bredd för spridning vid ytan
            // Spridökning per djup
            double SpridLU  = Math.Tan(30 * Math.PI / 180); // Eruokod 4.9.1 ansätter spridning till 30 grader genom omsorgsfullt komprimerad fyllning
            var SpridBU     = new List<double> { 0 , 0 };  // OBS! KB Tabell 7.1-5, ka 4.9.1 "Spridning får inte göras utanför betraktad konstruktions utsträckning". 

            double QÖLP0   = 10;
            double LP0     = 1;
            double BP0     = 1;
            double SpridLP = 0;
            var SpridBP = new List<double> { 0, 0 };
            // qÖL= QÖL0/((L0+JordH*SpridL)*(B0+JordH*SpridB))
            // --- Accidental ---
            double qAcc = 100 / 6;  // Olyckslast / fördelningslängd
            // ------------------

            // --- Räcke ---
            var LineR = Slab2.Region.Contours[0].Edges[3];
            var qR_L = new Vector3d(0, 0, -qR);
            var lineLoadR = new FemDesign.Loads.LineLoad(LineR, qR_L, loadCaseR, ForceLoadType.Force, "", true, false);

            // --- 1. Jord U ---
            //  Tillbakafyllning
            //  V
            var PBaktass_0 = new Point3d(PP2_1[0] + SlabThickness2[1] / 2, PP2_1[1], PP1_1[2]);
            var PBaktass_1 = new Point3d(PP2_2[0] + SlabThickness2[1] / 2, PP2_2[1], PP1_2[2]);
            var LBaktass = new List<double> { PP1_1[0] - (PP2_1[0] + SlabThickness2[0] / 2), PP1_2[0] - (PP2_2[0] + SlabThickness2[0] / 2) };
            // JordH in points: PBaktass_0 , P1_1, P1_2, PBaktass_1
            var JordH_baktass = new List<double> { PP2_0[2] - PP2_1[2] - JordH[0], (PP2_0[2] - PP2_1[2] - JordH[0]) + LBaktass[0] * Math.Tan(AlfaJ[0]),
                                                 (PP2_3[2] - PP2_2[2] - JordH[1]) + LBaktass[1] * Math.Tan(AlfaJ[0]) , PP2_3[2] - PP2_2[2] - JordH[1] };

            var posJV = new Point3d(PP2_1[0] + SlabThickness2[1] / 2, 0, PP1_0[2]);
            var regionBaktass = FemDesign.Geometry.Region.RectangleXY(posJV, PP1_1[0] - (PP2_1[0] + SlabThickness2[1] / 2), PP2_2[1]);

            var LoaddirJV = new Vector3d(0, 0, -1);

            var LoadJUV_0 = new LoadLocationValue(PBaktass_0, JordH_baktass[0] * qJ[0]);
            var LoadJUV_1 = new LoadLocationValue(P1_1, JordH_baktass[1] * qJ[0]);
            var LoadJUV_2 = new LoadLocationValue(P1_2, JordH_baktass[2] * qJ[0]);
            var LoadJUV = new List<LoadLocationValue> { LoadJUV_0, LoadJUV_1, LoadJUV_2 };

            var SurfaceLoadJUV = new FemDesign.Loads.SurfaceLoad(regionBaktass, LoadJUV, LoaddirJV, loadCaseJU, false, "");

            //  H
            var P2_0_TillH = new Point3d(PP2_0[0], PP2_0[1], PP2_0[2] - JordH[0]);
            var P2_3_TillH = new Point3d(PP2_3[0], PP2_3[1], PP2_3[2] - JordH[1]);
            var P_Load_mur = new List<Point3d> { P2_0_TillH, P2_1, P2_2, P2_3_TillH };
            var regionLoadMur = new Region(P_Load_mur, Plane.YZ);

            var LoaddirJH = new Vector3d(-1, 0, 0);

            var qhJUH = new List<double> { qJ[0] * K_ULS[0], qJ[0] * K_ULS[1] };

            var LoadJUH_0 = new LoadLocationValue(P2_0_TillH, 0);
            var LoadJUH_1 = new LoadLocationValue(P2_1, JordH_baktass[1] * qhJUH[0]);
            var LoadJUH_2 = new LoadLocationValue(P2_2, JordH_baktass[3] * qhJUH[0]);
            var LoadJUH = new List<LoadLocationValue> { LoadJUH_0, LoadJUH_1, LoadJUH_2 };

            var SurfaceLoadJUH = new FemDesign.Loads.SurfaceLoad(regionLoadMur, LoadJUH, LoaddirJH, loadCaseJU, false, "");

            var LineBPL_TillH = Slab1.Region.Contours[0].Edges[1];
            var qJUH_LL = new List<double> { (JordH_baktass[1] + SlabThickness1[0] / 2) * qhJUH[0], (JordH_baktass[2] + SlabThickness1[0] / 2) * qhJUH[0] };
            var qJUH_L0 = new Vector3d(-qJUH_LL[0] * SlabThickness1[0], 0, 0);
            var qJUH_L1 = new Vector3d(-qJUH_LL[1] * SlabThickness1[0], 0, 0);
            var lineLoadJUH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qJUH_L0, qJUH_L1, loadCaseJU, ForceLoadType.Force, "", true, false);

            // ---  Motfyllning ---
            //  V

            var LFramtass = new List<double> { (PP2_1[0] - SlabThickness2[1] / 2) - PP1_0[0], (PP2_2[0] - SlabThickness2[1] / 2) - PP1_0[0] };
            var JordH_framtass = new List<double> {  JordH[2] - SlabThickness1[0] + LFramtass[0] * Math.Tan(AlfaJ[1]), JordH[2] - SlabThickness1[0],
                                                     JordH[3] - SlabThickness1[0], JordH[3] - SlabThickness1[0] + LFramtass[1] * Math.Tan(AlfaJ[1]) };
            var posJVM = new Point3d(PP1_0[0], PP1_0[1], PP1_0[2]);
            var regionFramtass = FemDesign.Geometry.Region.RectangleXY(posJVM, (PP2_0[0] - SlabThickness2[1] / 2), PP2_2[1]);

            var PFramtass_0 = new Point3d(PP2_0[0] - SlabThickness2[0] / 2, PP2_0[1], PP1_0[2]);
            var PFramtass_1 = new Point3d(PP2_3[0] - SlabThickness2[0] / 2, PP2_3[1], PP1_3[2]);

            var LoadJUMV_0 = new LoadLocationValue(P1_0, JordH_framtass[0] * qJ[0]);
            var LoadJUMV_1 = new LoadLocationValue(PFramtass_0, JordH_framtass[1] * qJ[0]);
            var LoadJUMV_2 = new LoadLocationValue(PFramtass_1, JordH_framtass[2] * qJ[0]);
            var LoadJMUV = new List<LoadLocationValue> { LoadJUMV_0, LoadJUMV_1, LoadJUMV_2 };

            var LoadDummy_0 = new LoadLocationValue(P1_0,        0);
            var LoadDummy_1 = new LoadLocationValue(PFramtass_0, 0);
            var LoadDummy_2 = new LoadLocationValue(PFramtass_1, 0);
            var LoadDummylist = new List<LoadLocationValue> { LoadDummy_0, LoadDummy_1, LoadDummy_2 };

            var SurfaceLoadJMUV = new FemDesign.Loads.SurfaceLoad(regionFramtass, LoadDummylist, LoaddirJV, loadCaseJU, false, "");
            double JordH_framtass_min = JordH_framtass.AsQueryable().Min();
            if (JordH_framtass_min > 0)
            {
                SurfaceLoadJMUV = new FemDesign.Loads.SurfaceLoad(regionFramtass, LoadJMUV, LoaddirJV, loadCaseJU, false, "");
            }

            //  H
            var P2_0_MfyllH = new Point3d(PP2_0[0], PP2_0[1], PP2_1[2] + JordH[2]);
            var P2_3_MfyllH = new Point3d(PP2_3[0], PP2_3[1], PP2_2[2] + JordH[3]);
            var P_Load_Mfyll = new List<Point3d> { P2_0_MfyllH, P2_1, P2_2, P2_3_MfyllH };
            var region_Load_Mfyll = new Region(P_Load_Mfyll, Plane.YZ);
            var LoaddirJMH = new Vector3d(1, 0, 0);

            var LoadJUMH_0 = new LoadLocationValue(P2_0_MfyllH, 0);
            var LoadJUMH_1 = new LoadLocationValue(P2_1, JordH_framtass[1] * qhJUH[1]);
            var LoadJUMH_2 = new LoadLocationValue(P2_2, JordH_framtass[2] * qhJUH[1]);
            var LoadJMUH = new List<LoadLocationValue> { LoadJUMH_0, LoadJUMH_1, LoadJUMH_2 };

            var SurfaceLoadJMUH = new FemDesign.Loads.SurfaceLoad(region_Load_Mfyll, LoadJMUH, LoaddirJMH, loadCaseJU, false, "");

            var LineBPL_MotH = Slab1.Region.Contours[0].Edges[3];
            var qJMUH_LL = new List<double> { (JordH_framtass[1] + SlabThickness1[0] / 2) * qhJUH[1], (JordH_framtass[2] + SlabThickness1[0] / 2) * qhJUH[1] };
            var qJMUH_L0 = new Vector3d(qJMUH_LL[0] * SlabThickness1[0], 0, 0);
            var qJMUH_L1 = new Vector3d(qJMUH_LL[1] * SlabThickness1[0], 0, 0);
            var lineLoadJMUH = new FemDesign.Loads.LineLoad(LineBPL_MotH, qJMUH_L1, qJMUH_L0, loadCaseJU, ForceLoadType.Force, "", true, false);

            //// --- 2. Jord Glid ---

            //  Tillbakafyllning
            //  V

            var LoadJAV_0 = new LoadLocationValue(PBaktass_0, JordH_baktass[0] * qJ[0]);
            var LoadJAV_1 = new LoadLocationValue(P1_1, JordH_baktass[1] * qJ[0]);
            var LoadJAV_2 = new LoadLocationValue(P1_2, JordH_baktass[2] * qJ[0]);
            var LoadJAV = new List<LoadLocationValue> { LoadJUV_0, LoadJUV_1, LoadJUV_2 };

            var SurfaceLoadJAV = new FemDesign.Loads.SurfaceLoad(regionBaktass, LoadJAV, LoaddirJV, loadCaseJA, false, "");

            //  H

            var qhJAH = new List<double> { qJ[0] * K_Glid[0], qJ[0] * K_Glid[1] };

            var LoadJAH_0 = new LoadLocationValue(P2_0_TillH, 0);
            var LoadJAH_1 = new LoadLocationValue(P2_1, JordH_baktass[1] * qhJUH[0]);
            var LoadJAH_2 = new LoadLocationValue(P2_2, JordH_baktass[3] * qhJUH[0]);
            var LoadJAH = new List<LoadLocationValue> { LoadJUH_0, LoadJUH_1, LoadJUH_2 };

            var SurfaceLoadJAH = new FemDesign.Loads.SurfaceLoad(regionLoadMur, LoadJAH, LoaddirJH, loadCaseJA, false, "");

            var qJAH_LL = new List<double> { (JordH_baktass[1] + SlabThickness1[0] / 2) * qhJAH[0], (JordH_baktass[2] + SlabThickness1[0] / 2) * qhJAH[0] };
            var qJAH_L0 = new Vector3d(-qJAH_LL[0] * SlabThickness1[0], 0, 0);
            var qJAH_L1 = new Vector3d(-qJAH_LL[1] * SlabThickness1[0], 0, 0);
            var lineLoadJAH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qJAH_L0, qJAH_L1, loadCaseJA, ForceLoadType.Force, "", true, false);

            // ---  Motfyllning ---
            //  V

            var LoadJAMV_0 = new LoadLocationValue(P1_0, JordH_framtass[0] * qJ[0]);
            var LoadJAMV_1 = new LoadLocationValue(PFramtass_0, JordH_framtass[1] * qJ[0]);
            var LoadJAMV_2 = new LoadLocationValue(PFramtass_1, JordH_framtass[2] * qJ[0]);
            var LoadJMAV = new List<LoadLocationValue> { LoadJAMV_0, LoadJAMV_1, LoadJAMV_2 };

            var SurfaceLoadJMAV = new FemDesign.Loads.SurfaceLoad(regionFramtass, LoadDummylist, LoaddirJV, loadCaseJA, false, "");

            if (JordH_framtass_min > 0)
            {
                SurfaceLoadJMAV = new FemDesign.Loads.SurfaceLoad(regionFramtass, LoadJMAV, LoaddirJV, loadCaseJA, false, "");
            }

            //  H

            var LoadJAMH_0 = new LoadLocationValue(P2_0_MfyllH, 0);
            var LoadJAMH_1 = new LoadLocationValue(P2_1, JordH_framtass[1] * qhJAH[1]);
            var LoadJAMH_2 = new LoadLocationValue(P2_2, JordH_framtass[2] * qhJAH[1]);
            var LoadJMAH = new List<LoadLocationValue> { LoadJAMH_0, LoadJAMH_1, LoadJAMH_2 };

            var SurfaceLoadJMAH = new FemDesign.Loads.SurfaceLoad(region_Load_Mfyll, LoadJMAH, LoaddirJMH, loadCaseJA, false, "");

            var qJMAH_LL = new List<double> { (JordH_framtass[1] + SlabThickness1[0] / 2) * qhJAH[1], (JordH_framtass[2] + SlabThickness1[0] / 2) * qhJAH[1] };
            var qJMAH_L0 = new Vector3d(qJMUH_LL[0] * SlabThickness1[0], 0, 0);
            var qJMAH_L1 = new Vector3d(qJMUH_LL[1] * SlabThickness1[0], 0, 0);
            var lineLoadJMAH = new FemDesign.Loads.LineLoad(LineBPL_MotH, qJMAH_L1, qJMAH_L0, loadCaseJA, ForceLoadType.Force, "", true, false);


            //// --- 3. Jord K ---
            //  Tillbakafyllning
            //  V

            var SurfaceLoadJKV = new FemDesign.Loads.SurfaceLoad(regionBaktass, LoadJUV, LoaddirJV, loadCaseJK, false, "");

            //  H
            var qhJKH = new List<double> { qJ[0] * K_SLS[0], qJ[0] * K_SLS[1] };

            var LoadJKH_0 = new LoadLocationValue(P2_0_TillH, 0);
            var LoadJKH_1 = new LoadLocationValue(P2_1, JordH_baktass[0] * qhJKH[0]);
            var LoadJKH_2 = new LoadLocationValue(P2_2, JordH_baktass[3] * qhJKH[0]);
            var LoadJKH = new List<LoadLocationValue> { LoadJKH_0, LoadJKH_1, LoadJKH_2 };

            var SurfaceLoadJKH = new FemDesign.Loads.SurfaceLoad(regionLoadMur, LoadJKH, LoaddirJH, loadCaseJK, false, "");

            var qJKH_LL = new List<double> { (JordH_baktass[0] + SlabThickness1[0] / 2) * qhJKH[0], (JordH_baktass[3] + SlabThickness1[0] / 2) * qhJKH[0] };
            var qJKH_L0 = new Vector3d(-qJKH_LL[0] * SlabThickness1[0], 0, 0);
            var qJKH_L1 = new Vector3d(-qJKH_LL[1] * SlabThickness1[0], 0, 0);
            var lineLoadJKH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qJKH_L0, qJKH_L1, loadCaseJK, ForceLoadType.Force, "", true, false);

            // ---  Motfyllning ---
            //  V

            var SurfaceLoadJMKV = new FemDesign.Loads.SurfaceLoad(regionFramtass, LoadDummylist, LoaddirJV, loadCaseJK, false, "");
            if (JordH_framtass_min > 0)
            {
                SurfaceLoadJMKV = new FemDesign.Loads.SurfaceLoad(regionFramtass, LoadJMUV, LoaddirJV, loadCaseJK, false, "");
            }
            
            // H
            var LoadJKMH_0 = new LoadLocationValue(P2_0_MfyllH, 0);
            var LoadJKMH_1 = new LoadLocationValue(P2_1, JordH_framtass[0] * qhJKH[1]);
            var LoadJKMH_2 = new LoadLocationValue(P2_2, JordH_framtass[1] * qhJKH[1]);
            var LoadJMKH = new List<LoadLocationValue> { LoadJKMH_0, LoadJKMH_1, LoadJKMH_2 }; 

            var SurfaceLoadJMKH = new FemDesign.Loads.SurfaceLoad(region_Load_Mfyll, LoadJMKH, LoaddirJMH, loadCaseJK, false, "");

            var qJMKH_LL = new List<double> { (JordH_framtass[0] + SlabThickness1[0]/2) * qhJKH[1], (JordH_framtass[1] + SlabThickness1[0]/2) * qhJKH[1] };
            var qJMKH_L0 = new Vector3d(qJMKH_LL[0] * SlabThickness1[0], 0, 0);
            var qJMKH_L1 = new Vector3d(qJMKH_LL[1] * SlabThickness1[0], 0, 0);
            var lineLoadJMKH = new FemDesign.Loads.LineLoad(LineBPL_MotH, qJMKH_L1, qJMKH_L0, loadCaseJK, ForceLoadType.Force, "", true, false);

            // --- 4. HHW ---

            double SlabThickness1_min = SlabThickness1.AsQueryable().Min();
            double SlabThickness1_max = SlabThickness1.AsQueryable().Max();

            if (HHW >= SlabThickness1_max)
            {
                //  V
                var HHWLoadV = new Vector3d(0, 0, -(HHW - SlabThickness1[0])*qW);
                var SurfaceLoadHHW_V_baktass = new FemDesign.Loads.SurfaceLoad(regionBaktass, HHWLoadV, loadCaseHHW,false,"");
                var SurfaceLoadHHW_V_framtass = new FemDesign.Loads.SurfaceLoad(regionFramtass, HHWLoadV, loadCaseHHW, false, "");

                //  lift
                // BPL
               // var LiftLoadBPL = new Vector3d(0,0, VolumeBPL * qW/ AreaBPL);
               // var SurfaceLoadHHW_liftBPL = new FemDesign.Loads.SurfaceLoad(regionBPL, LiftLoadBPL, loadCaseHHW,false,"");

                // MUR
                double meanZMur = (PP2_0[2] + PP2_3[2]) / 2;
                double AndelliftMur = (HHW - SlabThickness1_max)/ meanZMur;
              //  double AreaMur = FemDesign.Results.QuantityEstimationConcrete.Slab2.Area;
              //  double VolumeMur = FemDesign.Results.QuantityEstimationConcrete.Slab2.Volume;
              //  var LiftLoadMur = new Vector3d(0, 0, AndelliftMur * VolumeMur * qW/ AreaMur);
                var regionMur = Slab2.Region;
              //  var SurfaceLoadHHW_liftMur = new FemDesign.Loads.SurfaceLoad(regionMur, LiftLoadMur, loadCaseHHW, false, "");

            }
            else if (HHW > 0)
            {
                //  lift BPL
                double meanZBPL = (SlabThickness1_min + SlabThickness1_max) / 2;
                double AndelliftBPL = HHW / meanZBPL;
             //   var LiftLoadBPL = new Vector3d(0, 0, AndelliftBPL*VolumeBPL * qW / AreaBPL);
             //   var SurfaceLoadHHW_liftBPL = new FemDesign.Loads.SurfaceLoad(regionBPL, LiftLoadBPL, loadCaseHHW, false, "");
            }

            // --- 4.1 Eff J HHW ---
            if (HHW >= SlabThickness1_max)
            {
                //  V
                double EffJbaktass_max = Math.Min(HHW, JordH_baktass[0]);
                var HHWLoadVbaktass = new Vector3d(0, 0,-(EffJbaktass_max - SlabThickness1[0]) * qW);
                var SurfaceLoadHHW_V_baktass = new FemDesign.Loads.SurfaceLoad(regionBaktass, HHWLoadVbaktass, loadCaseEffJHHW, false, "");

                double EffJframtass_max = Math.Min(HHW, JordH_baktass[0]);
                var HHWLoadVframtass = new Vector3d(0, 0, -(EffJbaktass_max - SlabThickness1[0]) * qW);
                var SurfaceLoadHHW_V_framtass = new FemDesign.Loads.SurfaceLoad(regionFramtass, HHWLoadVframtass, loadCaseEffJHHW, false, "");
            }
            // --- 5.0 MW ---

            // --- 5.1 Eff J MW ---

            // --- 6.0 LLW ---

            // --- 6.1 Eff J LLW ---

            // --- 7. ÖL U ---
            //  V
            // Definition of qÖL in point P2_0, P2_1 , P2_2, P2_3 
            // qÖL= QÖL0/((L0+JordH*SpridL)*(B0+JordH*SpridB))
            var qÖLVU = new List<double> { QÖLU0 / (LU0 * BU0), QÖLU0 /((LU0+JordH_baktass[0]*SpridLU) * (BU0 + JordH_baktass[0] * (SpridBU[0] + SpridBU[1]))), 
                QÖLU0 / ((LU0 + JordH_baktass[1] * SpridLU) * (BU0 + JordH_baktass[1] * (SpridBU[0] + SpridBU[1]))), QÖLU0 / (LU0 * BU0) };

            var LoadÖLUV_0 = new LoadLocationValue(PBaktass_0,  qÖLVU[1]);
            var LoadÖLUV_1 = new LoadLocationValue(P1_1,        qÖLVU[1]);
            var LoadÖLUV_2 = new LoadLocationValue(P1_2,        qÖLVU[2]);
            var LoadÖLUV = new List<LoadLocationValue> { LoadÖLUV_0, LoadÖLUV_1, LoadÖLUV_2 };

            var SurfaceLoadÖLVU = new FemDesign.Loads.SurfaceLoad(regionBaktass, LoadÖLUV, LoaddirJV, loadCaseÖLVU, false, "");

            //  H
            var qÖLHU = new List<double> { qÖLVU[0] * K_ULS[0], qÖLVU[1] * K_ULS[0], qÖLVU[2] * K_ULS[0] };

            var LoadÖLHU_0 = new LoadLocationValue(P2_0_TillH,  qÖLHU[0]);
            var LoadÖLHU_1 = new LoadLocationValue(P2_1,        qÖLHU[1]);
            var LoadÖLHU_2 = new LoadLocationValue(P2_2,        qÖLHU[2]);
            var LoadÖLHU = new List<LoadLocationValue> { LoadÖLHU_0, LoadÖLHU_1, LoadÖLHU_2 };

            var SurfaceLoadÖLHU = new FemDesign.Loads.SurfaceLoad(regionLoadMur, LoadÖLHU, LoaddirJH, loadCaseÖLHU, false, "");

            var qÖLUH_L0 = new Vector3d(-qÖLHU[2] * SlabThickness1[0], 0, 0);
            var qÖLUH_L1 = new Vector3d(-qÖLHU[1] * SlabThickness1[0], 0, 0);
            var lineLoadÖLUH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qÖLUH_L0, qÖLUH_L1, loadCaseÖLHU, ForceLoadType.Force, "", true, false);
            // --- 8. ÖL P ---
            //  V
            var qÖLVP = new List<double> { QÖLP0 / (LP0 * BP0), QÖLP0 / ((LP0 + JordH_baktass[0] * SpridLP) * (BP0 + JordH_baktass[0] * (SpridBP[0] + SpridBP[1]))), 
                QÖLP0 / ((LP0 + JordH_baktass[1] * SpridLP) * (BP0 + JordH_baktass[1] * (SpridBP[0] + SpridBP[1]))), QÖLP0 / (LP0 * BP0) };

            var LoadÖLPV_0 = new LoadLocationValue(PBaktass_0,  qÖLVP[1]);
            var LoadÖLPV_1 = new LoadLocationValue(P1_1,        qÖLVP[1]);
            var LoadÖLPV_2 = new LoadLocationValue(P1_2,        qÖLVP[2]);
            var LoadÖLPV = new List<LoadLocationValue> { LoadÖLPV_0, LoadÖLPV_1, LoadÖLPV_2 };

            var SurfaceLoadÖLVP = new FemDesign.Loads.SurfaceLoad(regionBaktass, LoadÖLPV, LoaddirJV, loadCaseÖLVP, false, "");

            //  H
            var qÖLHP = new List<double> { qÖLVP[0] * K_SLS[0], qÖLVP[1] * K_SLS[0], qÖLVP[2] * K_SLS[0] };

            var LoadÖLHP_0 = new LoadLocationValue(P2_0_TillH,  qÖLHP[0]);
            var LoadÖLHP_1 = new LoadLocationValue(P2_1,        qÖLHP[1]);
            var LoadÖLHP_2 = new LoadLocationValue(P2_2,        qÖLHP[2]);
            var LoadÖLHP = new List<LoadLocationValue> { LoadÖLHP_0, LoadÖLHP_1, LoadÖLHP_2 };

            var SurfaceLoadÖLHP = new FemDesign.Loads.SurfaceLoad(regionLoadMur, LoadÖLHP, LoaddirJH, loadCaseÖLHP, false, "");

            var qÖLPH_L0 = new Vector3d(-qÖLHP[2] * SlabThickness1[0], 0, 0);
            var qÖLPH_L1 = new Vector3d(-qÖLHP[1] * SlabThickness1[0], 0, 0);
            var lineLoadÖLPH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qÖLPH_L0, qÖLPH_L1, loadCaseÖLHP, ForceLoadType.Force, "", true, false);

            // --- 9. Olycka ---
            var qAcc_L = new Vector3d(-qAcc, 0, 0);
            var lineLoadAcc = new FemDesign.Loads.LineLoad(LineR, qAcc_L, loadCaseAcc, ForceLoadType.Force, "", true, false);

            // ----- Define load combination -----
            // Load combination when water adds lift
            var loadComb1 = new LoadCombination("LC1  A, 6.10  Vmin_Hmax", LoadCombType.UltimateOrdinary, (loadCaseDL, 0.90), (loadCaseR, 0.90), (loadCaseJU, 1.10), (loadCaseHHW, 1.10), (loadCaseEffJHHW, 1.10), (loadCaseÖLHU, 1.40));
            //var loadComb2 = new LoadCombination("LC2  B, 6.10a Vmin_Hmax", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJU, 1.35), (loadCaseHHW, 1.35), (loadCaseEffJHHW, 1.35), (loadCaseÖLHU, 1.05));
            //var loadComb3 = new LoadCombination("LC3  B, 6.10a Vmax_Hmax", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.35), (loadCaseR, 1.35), (loadCaseJU, 1.35), (loadCaseLLW, 1.00), (loadCaseEffJLLW, 1.00), (loadCaseÖLHU, 1.05), (loadCaseÖLVU, 1.05));
            //var loadComb4 = new LoadCombination("LC4  B, 6.10a Vmin_Hmax(Glid)", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJA, 1.35), (loadCaseHHW, 1.35), (loadCaseEffJHHW, 1.35), (loadCaseÖLHU, 1.05));
            //var loadComb5 = new LoadCombination("LC5  B, 6.10b Vmin_Hmax", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJU, 1.10), (loadCaseHHW, 1.10), (loadCaseEffJHHW, 1.10), (loadCaseÖLHU, 1.40));
            //var loadComb6 = new LoadCombination("LC6  B, 6.10b Vmax_Hmax", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.35), (loadCaseR, 1.35), (loadCaseJU, 1.10), (loadCaseLLW, 1.00), (loadCaseEffJLLW, 1.00), (loadCaseÖLHU, 1.40), (loadCaseÖLVU, 1.40));
            //var loadComb7 = new LoadCombination("LC7  B, 6.10b Vmin_Hmax(Glid)", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJA, 1.10), (loadCaseHHW, 1.10), (loadCaseEffJHHW, 1.10), (loadCaseÖLHU, 1.40));
            //var loadComb8 = new LoadCombination("LC8  Bruk-frekv, 6.15  Vmin_Hmax", LoadCombType.ServiceabilityFrequent, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJK, 1.00), (loadCaseMW, 1.00), (loadCaseEffJMW, 1.00), (loadCaseÖLHU, 0.75));
            //var loadComb9 = new LoadCombination("LC9  Bruk-kvasi, 6.16", LoadCombType.ServiceabilityQuasiPermanent, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJK, 1.00), (loadCaseMW, 1.00), (loadCaseEffJMW, 1.00));
            //var loadComb10 = new LoadCombination("LC10 Bruk-kvasi, 6.16 ÖLP", LoadCombType.ServiceabilityQuasiPermanent, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJK, 1.00), (loadCaseMW, 1.00), (loadCaseEffJMW, 1.00), (loadCaseÖLHP, 1.00), (loadCaseÖLVP, 1.00));
            var loadComb11 = new LoadCombination("LC11  Olycks, 6.11", LoadCombType.UltimateAccidental, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJK, 1.00), (loadCaseMW, 1.00), (loadCaseEffJMW, 1.00), (loadCaseAcc, 1.00));

            // Load combination when water adds weight
            //var loadComb1  = new LoadCombination("LC1  A, 6.10  Vmin_Hmax",             LoadCombType.UltimateOrdinary,                (loadCaseDL, 0.90), (loadCaseR, 0.90), (loadCaseJU, 1.10), (loadCaseLLW, 0.90), (loadCaseEffJLLW, 0.90), (loadCaseÖLHU, 1.40));
            //var loadComb2  = new LoadCombination("LC2  B, 6.10a Vmin_Hmax",             LoadCombType.UltimateOrdinary,                (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJU, 1.35), (loadCaseLLW, 1.00), (loadCaseEffJLLW, 1.00), (loadCaseÖLHU, 1.05));
            //var loadComb3  = new LoadCombination("LC3  B, 6.10a Vmax_Hmax",             LoadCombType.UltimateOrdinary,                (loadCaseDL, 1.35), (loadCaseR, 1.35), (loadCaseJU, 1.35), (loadCaseHHW, 1.35), (loadCaseEffJHHW, 1.35), (loadCaseÖLHU, 1.05), (loadCaseÖLVU, 1.05));
            //var loadComb4  = new LoadCombination("LC4  B, 6.10a Vmin_Hmax(Glid)",       LoadCombType.UltimateOrdinary,                (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJA, 1.35), (loadCaseLLW, 1.00), (loadCaseEffJLLW, 1.00), (loadCaseÖLHU, 1.05));
            //var loadComb5  = new LoadCombination("LC5  B, 6.10b Vmin_Hmax",             LoadCombType.UltimateOrdinary,                (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJU, 1.10), (loadCaseLLW, 1.00), (loadCaseEffJLLW, 1.00), (loadCaseÖLHU, 1.40));
            //var loadComb6  = new LoadCombination("LC6  B, 6.10b Vmax_Hmax",             LoadCombType.UltimateOrdinary,                (loadCaseDL, 1.35), (loadCaseR, 1.35), (loadCaseJU, 1.10), (loadCaseHHW, 1.10), (loadCaseEffJHHW, 1.10), (loadCaseÖLHU, 1.40), (loadCaseÖLVU, 1.40));
            //var loadComb7  = new LoadCombination("LC7  B, 6.10b Vmin_Hmax(Glid)",       LoadCombType.UltimateOrdinary,                (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJA, 1.10), (loadCaseLLW, 1.00), (loadCaseEffJLLW, 1.00), (loadCaseÖLHU, 1.40));
            //var loadComb8  = new LoadCombination("LC8  Bruk-frekv, 6.15  Vmin_Hmax",    LoadCombType.ServiceabilityFrequent,          (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJK, 1.00), (loadCaseMW, 1.00),  (loadCaseEffJMW, 1.00),  (loadCaseÖLHU, 0.75));
            //var loadComb9  = new LoadCombination("LC9  Bruk-kvasi, 6.16",               LoadCombType.ServiceabilityQuasiPermanent,    (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJK, 1.00), (loadCaseMW, 1.00),  (loadCaseEffJMW, 1.00));
            //var loadComb10 = new LoadCombination("LC10 Bruk-kvasi, 6.16 ÖLP",           LoadCombType.ServiceabilityQuasiPermanent,    (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJK, 1.00), (loadCaseMW, 1.00),  (loadCaseEffJMW, 1.00),  (loadCaseÖLHP, 1.00), (loadCaseÖLVP, 1.00));
            //var loadComb11 = new LoadCombination("LC11  Olycks, 6.11",                  LoadCombType.UltimateAccidental,              (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJK, 1.00), (loadCaseMW, 1.00),  (loadCaseEffJMW, 1.00),  (loadCaseAcc, 1.00));

            var loadCombList = new List<LoadCombination>
            {
                loadComb1,
                //loadComb2,
                //loadComb3,
                //loadComb4,
                //loadComb5,
                //loadComb6,
                //loadComb7,
                //loadComb8,
                //loadComb9,
                //loadComb10,
                loadComb11
            };


            // ----- Define Load Groups  -----

            // old
            //var qJV_L = new Vector3d(0, 0, -qJ * (2 - 1));
            //var posJV = new Point3d(PP2_0[0], 0, PP1_0[2]);
            //var regionBaktass = FemDesign.Geometry.Region.RectangleXY(posJV, 2.4, 1.0);

            //var SurfaceLoadJUV = new FemDesign.Loads.SurfaceLoad(regionBaktass, qJV_L, loadCaseJU, false, "");

            //var qJV_L = new Vector3d(0, 0, -qJ * (PP2_2[2] - PP2_0[2]));
            //var SurfaceLoadJUV = new FemDesign.Loads.SurfaceLoad(regionBaktass, LoaddirJV, loadCaseJU, false, "");

            //var SurfaceLoadJKV = new FemDesign.Loads.SurfaceLoad(regionBaktass, qJV_L, loadCaseJK, false, "");

            //var PressureLoadJKH = new FemDesign.Loads.PressureLoad(region_Load_Mur, LoaddirJH, z0JH, q0JH, qhJKH, loadCaseJK, "", false, ForceLoadType.Force);


            //var qÖLVU_L = new Vector3d(0, 0, -qÖLVU[1]);
            //var SurfaceLoadÖLVU = new FemDesign.Loads.SurfaceLoad(regionBaktass, qÖLVU_L, loadCaseÖLVU, false, "");

            //double qÖLHU = qÖLVU*K0_ULS;
            //var qÖLHU_L = new Vector3d(-qÖLHU,0,0);
            //var SurfaceLoadÖLHU = new FemDesign.Loads.SurfaceLoad(region_Load_Mur, qÖLHU_L, loadCaseÖLHU, false, "");

            //var LineJH = Slab1.Region.Contours[0].Edges[1];

            //var qÖLVP_L = new Vector3d(0, 0, -qÖLVP[1]);
            //var SurfaceLoadÖLVP = new FemDesign.Loads.SurfaceLoad(regionBaktass, qÖLVP_L, loadCaseÖLVP, false, "");

            //double qÖLHP = qÖLVP * K0_SLS;
            //var qÖLHP_L = new Vector3d(-qÖLHP, 0, 0);
            //var SurfaceLoadÖLHP = new FemDesign.Loads.SurfaceLoad(region_Load_Mur, qÖLHP_L, loadCaseÖLHP, false, "");

            var loads = new List<ILoadElement> 
            { 
              lineLoadR,        
              SurfaceLoadJUV,
              SurfaceLoadJUH,
              lineLoadJUH,
              SurfaceLoadJMUV,
              SurfaceLoadJMUH,
              lineLoadJMUH,
              SurfaceLoadJKV,
              SurfaceLoadJKH,  
              lineLoadJKH,
              SurfaceLoadJMKV,
              SurfaceLoadJMKH,
              lineLoadJMKH,
              SurfaceLoadÖLVU,
              SurfaceLoadÖLHU,
              lineLoadÖLUH,
              SurfaceLoadÖLVP,
              SurfaceLoadÖLHP,
              lineLoadÖLPH,
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
            var reinfSlab2 = FemDesign.Reinforcement.SurfaceReinforcement.AddReinforcementToSlab(Slab2, srfReinf);

            //var elements = new List<FemDesign.GenericClasses.IStructureElement> { Slab1, Slab2, Slab3, SurfaceSupport1, reinfSlab1, reinfSlab3 };
            var elements = new List<FemDesign.GenericClasses.IStructureElement> { Slab1, Slab2, Slab3, SurfaceSupport1};
            //var model = new Model(Country.S, elements);

            // Calculation parameters crack width use RCShellCrackWidth constructor? Example of "string resultCase" can it be "Maximum As"?
            // Design parameters for auto design example?

            // ----------

            var LabelledS_StartEnd = new List<Point3d>() { P2_1, P2_2, P2_0, P2_3 };

            var LabelledS_Points = LabelledSectionInterpolate(LabelledS_StartEnd, 8);

            var LabelledSection = new FemDesign.AuxiliaryResults.LabelledSection(LabelledS_Points, "LS");

            // Assemble the model
            var model = new Model(Country.S, elements, loads, loadCases, loadCombList, null,null,null,false);
            //model.addelements(elements);
            //model.addloads(loads);
            //model.addloadcases(loadcases);
            //model.addloadcombinations(loadcomb1);
           // model.AddLoadGroupTable(generalLoadGroups,false);
           //model.AddLabelledSection(LabelledSection);

            // Define the analysis settings
            var analysis = Analysis.StaticAnalysis(calcCase: true, calccomb: true);
            // Define the design settings

            var design = new FemDesign.Calculate.Design(autoDesign: true, check: true, loadCombination: true, applyChanges: true);

            // create a direct link to FEM-Design and comunicate with it
            using (var femDesign = new FemDesign.FemDesignConnection(keepOpen: true))
            {
                // Inside the "using..." we can send commands to FEM-Design.
                femDesign.Open(model);
                femDesign.RunAnalysis(analysis);
                //femDesign.RunDesign(RCDESIGN, design, null);
                var SurfaceSupportResultant0 = femDesign.GetLoadCombinationResults<Results.SurfaceSupportResultant>();
                var SurfaceSupportResultant1 = femDesign.GetResults<Results.SurfaceSupportResultant>();
            }

        }

        public static List<Point3d> LabelledSectionInterpolate(List<Point3d> Pointlist, int InterpolatePoints)
        {
            // Create a list of linear interpolate values from a list with start/endpoints with given interpolate points


            if (Pointlist.Count == 4 && InterpolatePoints > 0)
            {
                //Step size
                Double[] Step0 = 
                {  
                    (Pointlist[2].X - Pointlist[0].X)/ InterpolatePoints,
                    (Pointlist[2].Y - Pointlist[0].Y) / InterpolatePoints,
                    (Pointlist[2].Z - Pointlist[0].Z) / InterpolatePoints
                };

                Double[] Step1 =
                {
                    (Pointlist[3].X - Pointlist[1].X) / InterpolatePoints,
                    (Pointlist[3].Y - Pointlist[1].Y) / InterpolatePoints,
                    (Pointlist[3].Z - Pointlist[1].Z) / InterpolatePoints
                };
                
                //Adding interpolate points
                List<Point3d> PointlistInterpolate = new List<Point3d>();
                for (int i = 0; i <= InterpolatePoints; i++)
                {
                    PointlistInterpolate.Add(new Point3d (Pointlist[0].X + Step0[0] * i, Pointlist[0].Y + Step0[1] * i, Pointlist[0].Z + Step0[2] * i));
                    PointlistInterpolate.Add(new Point3d (Pointlist[1].X + Step0[1] * i, Pointlist[1].Y + Step1[1] * i, Pointlist[1].Z + Step1[2] * i));

                }

                return PointlistInterpolate;
            }

            else
            {
                throw new Exception("Pointlist should contain 4 points and InterpolatePoints > 0 ");
            }



        }

    }
}