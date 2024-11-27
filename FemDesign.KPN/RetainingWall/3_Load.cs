//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using FemDesign.Loads;
//using FemDesign.GenericClasses;
//using FemDesign.Shells;
//using FemDesign.Geometry;

//namespace RetainingWall
//{
//    internal class Load
//    {
//        public static List<FemDesign.GenericClasses.ILoadElement> loads(List<Slab> slabs, List<LoadCase> loadcases)
//        {
//            // Predefinition from input data
//            // slabs
//            Slab SlabBPL = slabs[0];
//            Slab SlabMUR = slabs[1];

//            Point3d P_BPL_0 = slabs[0].SlabPart.Region.Contours[0].Points[0];

//            //loadcases
//            LoadCase lineLoadR          = loadcases[0];
//            LoadCase SurfaceLoadJUV     = loadcases[1];
//            LoadCase SurfaceLoadJUH     = loadcases[2];
//            LoadCase lineLoadJUH        = loadcases[3];
//            LoadCase SurfaceLoadJMUV    = loadcases[4];
//            LoadCase SurfaceLoadJMUH    = loadcases[5];
//            LoadCase lineLoadJMUH       = loadcases[6];
//            LoadCase SurfaceLoadJAV     = loadcases[7];
//            LoadCase SurfaceLoadJAH     = loadcases[8];
//            LoadCase lineLoadJAH        = loadcases[9];
//            LoadCase SurfaceLoadJMAV    = loadcases[10];
//            LoadCase SurfaceLoadJMAH    = loadcases[11];
//            LoadCase lineLoadJMAH       = loadcases[12];
//            LoadCase SurfaceLoadJKV     = loadcases[13];
//            LoadCase SurfaceLoadJKH     = loadcases[14];
//            LoadCase lineLoadJKH        = loadcases[15];
//            LoadCase SurfaceLoadJMKV    = loadcases[16];
//            LoadCase SurfaceLoadJMKH    = loadcases[17];
//            LoadCase lineLoadJMKH       = loadcases[18];
//            LoadCase SurfaceLoadÖLVU    = loadcases[19];
//            LoadCase SurfaceLoadÖLHU    = loadcases[20];
//            LoadCase lineLoadÖLUH       = loadcases[21];
//            LoadCase SurfaceLoadÖLVP    = loadcases[22];
//            LoadCase SurfaceLoadÖLHP    = loadcases[23];
//            LoadCase lineLoadÖLPH       = loadcases[24];

//            // ----- INDATA -----

//            double qR = 1;      // Räckestyngd
//            // --- Earth ---
//            // JordH Tillbakafyll_PO, Tillbakafyll_P1, Motfyll_PO, Motfyll_PO
//            var JordH = new List<double> { 1.0, 1.0, 1e-6, 1e-6 }; // Tillbakafyllning visningshöjd (ÖK MUR) 0/1 / Motfyllning fyllningshöjd (UK BPL) 0/1
//            var AlfaJ = new List<double> { Math.PI / 180 * 0, Math.PI / 180 * 0 }; // Marklutning utifrån MUR Tillbakafyllning, Motfyllning

//            var qJ = new List<double> { 10, 1 };     // Fyllning Naturfuktig, Effektiv tunghet under GVY
//            // Tillbakafyllning , Motfyllning
//            var K_ULS = new List<double> { 0.39, 0.39 };
//            var K_Glid = new List<double> { 0.24, 0.39 };
//            var K_SLS = new List<double> { 0.29, 0.29 };
//            // --- Water --
//            double qW = 10; // Vatten tyngd
//            double HHW = 4; // Vattenpelare från UK BPL 
//            double MW = 2;
//            double LLW = 0;
//            // --- Surcharge ---
//            double QÖLU0 = (270 + 180) / 2;     // Överlaststyngd (kN)
//            double LU0 = 2.2;                   // Längd för spridning vid ytan
//            double BU0 = 3.0;                   // Bredd för spridning vid ytan
//            // Spridökning per djup
//            double SpridLU = Math.Tan(30 * Math.PI / 180); // Eruokod 4.9.1 ansätter spridning till 30 grader genom omsorgsfullt komprimerad fyllning
//            var SpridBU = new List<double> { 0, 0 };  // OBS! KB Tabell 7.1-5, ka 4.9.1 "Spridning får inte göras utanför betraktad konstruktions utsträckning". 

//            double QÖLP0 = 10;
//            double LP0 = 1;
//            double BP0 = 1;
//            double SpridLP = 0;
//            var SpridBP = new List<double> { 0, 0 };
//            // qÖL= QÖL0/((L0+JordH*SpridL)*(B0+JordH*SpridB))
//            // --- Accidental ---
//            double qAcc = 100 / 6;  // Olyckslast / fördelningslängd
//            // ------------------

//            // --- Räcke ---
//            var LineR = SlabMUR.Region.Contours[0].Edges[3];
//            var qR_L = new Vector3d(0, 0, -qR);
//            var lineLoadR = new FemDesign.Loads.LineLoad(LineR, qR_L, loadCaseR, ForceLoadType.Force, "", true, false);

//            // --- 1. Jord U ---
//            //  Tillbakafyllning
//            //  V
//            var PBaktass_0 = new Point3d(PP2_1[0] + SlabThicknessMUR[1] / 2, PP2_1[1], PP1_1[2]);
//            var PBaktass_1 = new Point3d(PP2_2[0] + SlabThicknessMUR[1] / 2, PP2_2[1], PP1_2[2]);
//            var LBaktass = new List<double> { PP1_1[0] - (PP2_1[0] + SlabThicknessMUR[0] / 2), PP1_2[0] - (PP2_2[0] + SlabThicknessMUR[0] / 2) };
//            // JordH in points: PBaktass_0 , P1_1, P1_2, PBaktass_1
//            var JordH_baktass = new List<double> { PP2_0[2] - PP2_1[2] - JordH[0], (PP2_0[2] - PP2_1[2] - JordH[0]) + LBaktass[0] * Math.Tan(AlfaJ[0]),
//                                                 (PP2_3[2] - PP2_2[2] - JordH[1]) + LBaktass[1] * Math.Tan(AlfaJ[0]) , PP2_3[2] - PP2_2[2] - JordH[1] };

//            var posJV = new Point3d(PP2_1[0] + SlabThicknessMUR[1] / 2, 0, PP1_0[2]);
//            var regionBaktass = FemDesign.Geometry.Region.RectangleXY(posJV, PP1_1[0] - (PP2_1[0] + SlabThicknessMUR[1] / 2), PP2_2[1]);

//            var LoaddirJV = new Vector3d(0, 0, -1);

//            var LoadJUV_0 = new LoadLocationValue(PBaktass_0, JordH_baktass[0] * qJ[0]);
//            var LoadJUV_1 = new LoadLocationValue(P1_1, JordH_baktass[1] * qJ[0]);
//            var LoadJUV_2 = new LoadLocationValue(P1_2, JordH_baktass[2] * qJ[0]);
//            var LoadJUV = new List<LoadLocationValue> { LoadJUV_0, LoadJUV_1, LoadJUV_2 };

//            var SurfaceLoadJUV = new FemDesign.Loads.SurfaceLoad(regionBaktass, LoadJUV, LoaddirJV, loadCaseJU, false, "");

//            //  H
//            var P2_0_TillH = new Point3d(PP2_0[0], PP2_0[1], PP2_0[2] - JordH[0]);
//            var P2_3_TillH = new Point3d(PP2_3[0], PP2_3[1], PP2_3[2] - JordH[1]);
//            var P_Load_MUR = new List<Point3d> { P2_0_TillH, P2_1, P2_2, P2_3_TillH };
//            var regionLoadMUR = new Region(P_Load_MUR, Plane.YZ);

//            var LoaddirJH = new Vector3d(-1, 0, 0);

//            var qhJUH = new List<double> { qJ[0] * K_ULS[0], qJ[0] * K_ULS[1] };

//            var LoadJUH_0 = new LoadLocationValue(P2_0_TillH, 0);
//            var LoadJUH_1 = new LoadLocationValue(P2_1, JordH_baktass[1] * qhJUH[0]);
//            var LoadJUH_2 = new LoadLocationValue(P2_2, JordH_baktass[3] * qhJUH[0]);
//            var LoadJUH = new List<LoadLocationValue> { LoadJUH_0, LoadJUH_1, LoadJUH_2 };

//            var SurfaceLoadJUH = new FemDesign.Loads.SurfaceLoad(regionLoadMUR, LoadJUH, LoaddirJH, loadCaseJU, false, "");

//            var LineBPL_TillH = SlabBPL.Region.Contours[0].Edges[1];
//            var qJUH_LL = new List<double> { (JordH_baktass[1] + SlabThicknessBPL[0] / 2) * qhJUH[0], (JordH_baktass[2] + SlabThicknessBPL[0] / 2) * qhJUH[0] };
//            var qJUH_L0 = new Vector3d(-qJUH_LL[0] * SlabThicknessBPL[0], 0, 0);
//            var qJUH_L1 = new Vector3d(-qJUH_LL[1] * SlabThicknessBPL[0], 0, 0);
//            var lineLoadJUH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qJUH_L0, qJUH_L1, loadCaseJU, ForceLoadType.Force, "", true, false);

//            // ---  Motfyllning ---
//            //  V

//            var LFramtass = new List<double> { (PP2_1[0] - SlabThicknessMUR[1] / 2) - PP1_0[0], (PP2_2[0] - SlabThicknessMUR[1] / 2) - PP1_0[0] };
//            var JordH_framtass = new List<double> {  JordH[2] + LFramtass[0] * Math.Tan(AlfaJ[1]), JordH[2],
//                                                     JordH[3] , JordH[3] + LFramtass[1] * Math.Tan(AlfaJ[1]) };
//            var posJVM = new Point3d(PP1_0[0], PP1_0[1], PP1_0[2]);
//            var regionFramtass = FemDesign.Geometry.Region.RectangleXY(posJVM, (PP2_0[0] - SlabThicknessMUR[1] / 2), PP2_2[1]);

//            var PFramtass_0 = new Point3d(PP2_0[0] - SlabThicknessMUR[0] / 2, PP2_0[1], PP1_0[2]);
//            var PFramtass_1 = new Point3d(PP2_3[0] - SlabThicknessMUR[0] / 2, PP2_3[1], PP1_3[2]);

//            var LoadJUMV_0 = new LoadLocationValue(P1_0, JordH_framtass[0] * qJ[0]);
//            var LoadJUMV_1 = new LoadLocationValue(PFramtass_0, JordH_framtass[1] * qJ[0]);
//            var LoadJUMV_2 = new LoadLocationValue(PFramtass_1, JordH_framtass[2] * qJ[0]);
//            var LoadJMUV = new List<LoadLocationValue> { LoadJUMV_0, LoadJUMV_1, LoadJUMV_2 };

//            var LoadDummy_0 = new LoadLocationValue(P1_0, 0);
//            var LoadDummy_1 = new LoadLocationValue(PFramtass_0, 0);
//            var LoadDummy_2 = new LoadLocationValue(PFramtass_1, 0);
//            var LoadDummylist = new List<LoadLocationValue> { LoadDummy_0, LoadDummy_1, LoadDummy_2 };

//            var SurfaceLoadJMUV = new FemDesign.Loads.SurfaceLoad(regionFramtass, LoadDummylist, LoaddirJV, loadCaseJU, false, "");
//            double JordH_framtass_min = JordH_framtass.AsQueryable().Min();
//            if (JordH_framtass_min > 0)
//            {
//                SurfaceLoadJMUV = new FemDesign.Loads.SurfaceLoad(regionFramtass, LoadJMUV, LoaddirJV, loadCaseJU, false, "");
//            }

//            //  H
//            var P2_0_MfyllH = new Point3d(PP2_0[0], PP2_0[1], PP2_1[2] + JordH[2]);
//            var P2_3_MfyllH = new Point3d(PP2_3[0], PP2_3[1], PP2_2[2] + JordH[3]);
//            var P_Load_Mfyll = new List<Point3d> { P2_0_MfyllH, P2_1, P2_2, P2_3_MfyllH };
//            var region_Load_Mfyll = new Region(P_Load_Mfyll, Plane.YZ);
//            var LoaddirJMH = new Vector3d(1, 0, 0);

//            var LoadJUMH_0 = new LoadLocationValue(P2_0_MfyllH, 0);
//            var LoadJUMH_1 = new LoadLocationValue(P2_1, JordH_framtass[0] * qhJUH[1]);
//            var LoadJUMH_2 = new LoadLocationValue(P2_2, JordH_framtass[3] * qhJUH[1]);
//            var LoadJMUH = new List<LoadLocationValue> { LoadJUMH_0, LoadJUMH_1, LoadJUMH_2 };


//            var SurfaceLoadJMUH = new FemDesign.Loads.SurfaceLoad(region_Load_Mfyll, LoadDummylist, LoaddirJMH, loadCaseJU, false, "");
//            if (JordH_framtass_min > 0)
//            {
//                SurfaceLoadJMUH = new FemDesign.Loads.SurfaceLoad(region_Load_Mfyll, LoadJMUH, LoaddirJMH, loadCaseJU, false, "");
//            }

//            var LineBPL_MotH = SlabBPL.Region.Contours[0].Edges[3];
//            var qJMUH_LL = new List<double> { (JordH_framtass[1] - SlabThicknessBPL[0] / 2) * qhJUH[1], (JordH_framtass[2] - SlabThicknessBPL[0] / 2) * qhJUH[1] };

//            if (JordH_framtass_min <= SlabThicknessBPL[0])
//            {
//                qJMUH_LL = new List<double> { (JordH_framtass[1] / 2) * qhJUH[1], (JordH_framtass[2] / 2) * qhJUH[1] };
//            }

//            var qJMUH_L0 = new Vector3d(qJMUH_LL[0] * SlabThicknessBPL[0], 0, 0);
//            var qJMUH_L1 = new Vector3d(qJMUH_LL[1] * SlabThicknessBPL[0], 0, 0);
//            var lineLoadJMUH = new FemDesign.Loads.LineLoad(LineBPL_MotH, qJMUH_L1, qJMUH_L0, loadCaseJU, ForceLoadType.Force, "", true, false);

//            //// --- 2. Jord Glid ---

//            //  Tillbakafyllning
//            //  V

//            var LoadJAV_0 = new LoadLocationValue(PBaktass_0, JordH_baktass[0] * qJ[0]);
//            var LoadJAV_1 = new LoadLocationValue(P1_1, JordH_baktass[1] * qJ[0]);
//            var LoadJAV_2 = new LoadLocationValue(P1_2, JordH_baktass[2] * qJ[0]);
//            var LoadJAV = new List<LoadLocationValue> { LoadJAV_0, LoadJAV_1, LoadJAV_2 };

//            var SurfaceLoadJAV = new FemDesign.Loads.SurfaceLoad(regionBaktass, LoadJAV, LoaddirJV, loadCaseJA, false, "");

//            //  H

//            var qhJAH = new List<double> { qJ[0] * K_Glid[0], qJ[0] * K_Glid[1] };

//            var LoadJAH_0 = new LoadLocationValue(P2_0_TillH, 0);
//            var LoadJAH_1 = new LoadLocationValue(P2_1, JordH_baktass[1] * qhJAH[0]);
//            var LoadJAH_2 = new LoadLocationValue(P2_2, JordH_baktass[3] * qhJAH[0]);
//            var LoadJAH = new List<LoadLocationValue> { LoadJAH_0, LoadJAH_1, LoadJAH_2 };

//            var SurfaceLoadJAH = new FemDesign.Loads.SurfaceLoad(regionLoadMUR, LoadJAH, LoaddirJH, loadCaseJA, false, "");

//            var qJAH_LL = new List<double> { (JordH_baktass[1] + SlabThicknessBPL[0] / 2) * qhJAH[0], (JordH_baktass[2] + SlabThicknessBPL[0] / 2) * qhJAH[0] };
//            var qJAH_L0 = new Vector3d(-qJAH_LL[0] * SlabThicknessBPL[0], 0, 0);
//            var qJAH_L1 = new Vector3d(-qJAH_LL[1] * SlabThicknessBPL[0], 0, 0);
//            var lineLoadJAH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qJAH_L0, qJAH_L1, loadCaseJA, ForceLoadType.Force, "", true, false);

//            // ---  Motfyllning ---
//            //  V

//            var LoadJAMV_0 = new LoadLocationValue(P1_0, JordH_framtass[0] * qJ[0]);
//            var LoadJAMV_1 = new LoadLocationValue(PFramtass_0, JordH_framtass[1] * qJ[0]);
//            var LoadJAMV_2 = new LoadLocationValue(PFramtass_1, JordH_framtass[2] * qJ[0]);
//            var LoadJMAV = new List<LoadLocationValue> { LoadJAMV_0, LoadJAMV_1, LoadJAMV_2 };

//            var SurfaceLoadJMAV = new FemDesign.Loads.SurfaceLoad(regionFramtass, LoadDummylist, LoaddirJV, loadCaseJA, false, "");

//            if (JordH_framtass_min > 0)
//            {
//                SurfaceLoadJMAV = new FemDesign.Loads.SurfaceLoad(regionFramtass, LoadJMAV, LoaddirJV, loadCaseJA, false, "");
//            }

//            //  H

//            var LoadJAMH_0 = new LoadLocationValue(P2_0_MfyllH, 0);
//            var LoadJAMH_1 = new LoadLocationValue(P2_1, JordH_framtass[1] * qhJAH[1]);
//            var LoadJAMH_2 = new LoadLocationValue(P2_2, JordH_framtass[2] * qhJAH[1]);
//            var LoadJMAH = new List<LoadLocationValue> { LoadJAMH_0, LoadJAMH_1, LoadJAMH_2 };

//            var SurfaceLoadJMAH = new FemDesign.Loads.SurfaceLoad(region_Load_Mfyll, LoadDummylist, LoaddirJMH, loadCaseJA, false, "");
//            if (JordH_framtass_min > 0)
//            {
//                SurfaceLoadJMAH = new FemDesign.Loads.SurfaceLoad(region_Load_Mfyll, LoadJMAH, LoaddirJMH, loadCaseJA, false, "");
//            }

//            var qJMAH_LL = new List<double> { (JordH_framtass[1] - SlabThicknessBPL[0] / 2) * qhJAH[1], (JordH_framtass[2] - SlabThicknessBPL[0] / 2) * qhJAH[1] };

//            if (JordH_framtass_min <= SlabThicknessBPL[0])
//            {
//                qJMAH_LL = new List<double> { (JordH_framtass[1] / 2) * qhJAH[1], (JordH_framtass[2] / 2) * qhJAH[1] };
//            }
//            var qJMAH_L0 = new Vector3d(qJMAH_LL[0] * SlabThicknessBPL[0], 0, 0);
//            var qJMAH_L1 = new Vector3d(qJMAH_LL[1] * SlabThicknessBPL[0], 0, 0);
//            var lineLoadJMAH = new FemDesign.Loads.LineLoad(LineBPL_MotH, qJMAH_L1, qJMAH_L0, loadCaseJA, ForceLoadType.Force, "", true, false);

//            //// --- 3. Jord K ---
//            //  Tillbakafyllning
//            //  V

//            var SurfaceLoadJKV = new FemDesign.Loads.SurfaceLoad(regionBaktass, LoadJUV, LoaddirJV, loadCaseJK, false, "");

//            //  H
//            var qhJKH = new List<double> { qJ[0] * K_SLS[0], qJ[0] * K_SLS[1] };

//            var LoadJKH_0 = new LoadLocationValue(P2_0_TillH, 0);
//            var LoadJKH_1 = new LoadLocationValue(P2_1, JordH_baktass[0] * qhJKH[0]);
//            var LoadJKH_2 = new LoadLocationValue(P2_2, JordH_baktass[3] * qhJKH[0]);
//            var LoadJKH = new List<LoadLocationValue> { LoadJKH_0, LoadJKH_1, LoadJKH_2 };

//            var SurfaceLoadJKH = new FemDesign.Loads.SurfaceLoad(regionLoadMUR, LoadJKH, LoaddirJH, loadCaseJK, false, "");

//            var qJKH_LL = new List<double> { (JordH_baktass[0] + SlabThicknessBPL[0] / 2) * qhJKH[0], (JordH_baktass[3] + SlabThicknessBPL[0] / 2) * qhJKH[0] };
//            var qJKH_L0 = new Vector3d(-qJKH_LL[0] * SlabThicknessBPL[0], 0, 0);
//            var qJKH_L1 = new Vector3d(-qJKH_LL[1] * SlabThicknessBPL[0], 0, 0);
//            var lineLoadJKH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qJKH_L0, qJKH_L1, loadCaseJK, ForceLoadType.Force, "", true, false);

//            // ---  Motfyllning ---
//            //  V

//            var LoadJKMV_0 = new LoadLocationValue(P1_0, JordH_framtass[0] * qJ[0]);
//            var LoadJKMV_1 = new LoadLocationValue(PFramtass_0, JordH_framtass[1] * qJ[0]);
//            var LoadJKMV_2 = new LoadLocationValue(PFramtass_1, JordH_framtass[2] * qJ[0]);
//            var LoadJMKV = new List<LoadLocationValue> { LoadJKMV_0, LoadJKMV_1, LoadJKMV_2 };

//            var SurfaceLoadJMKV = new FemDesign.Loads.SurfaceLoad(regionFramtass, LoadDummylist, LoaddirJV, loadCaseJK, false, "");

//            if (JordH_framtass_min > 0)
//            {
//                SurfaceLoadJMKV = new FemDesign.Loads.SurfaceLoad(regionFramtass, LoadJMKV, LoaddirJV, loadCaseJK, false, "");
//            }

//            //  H

//            var LoadJKMH_0 = new LoadLocationValue(P2_0_MfyllH, 0);
//            var LoadJKMH_1 = new LoadLocationValue(P2_1, JordH_framtass[1] * qhJKH[1]);
//            var LoadJKMH_2 = new LoadLocationValue(P2_2, JordH_framtass[2] * qhJKH[1]);
//            var LoadJMKH = new List<LoadLocationValue> { LoadJKMH_0, LoadJKMH_1, LoadJKMH_2 };

//            var SurfaceLoadJMKH = new FemDesign.Loads.SurfaceLoad(region_Load_Mfyll, LoadDummylist, LoaddirJMH, loadCaseJK, false, "");
//            if (JordH_framtass_min > 0)
//            {
//                SurfaceLoadJMKH = new FemDesign.Loads.SurfaceLoad(region_Load_Mfyll, LoadJMKH, LoaddirJMH, loadCaseJK, false, "");
//            }

//            var qJMKH_LL = new List<double> { (JordH_framtass[1] - SlabThicknessBPL[0] / 2) * qhJKH[1], (JordH_framtass[2] - SlabThicknessBPL[0] / 2) * qhJKH[1] };

//            if (JordH_framtass_min <= SlabThicknessBPL[0])
//            {
//                qJMKH_LL = new List<double> { (JordH_framtass[1] / 2) * qhJKH[1], (JordH_framtass[2] / 2) * qhJKH[1] };
//            }
//            var qJMKH_L0 = new Vector3d(qJMKH_LL[0] * SlabThicknessBPL[0], 0, 0);
//            var qJMKH_L1 = new Vector3d(qJMKH_LL[1] * SlabThicknessBPL[0], 0, 0);
//            var lineLoadJMKH = new FemDesign.Loads.LineLoad(LineBPL_MotH, qJMKH_L1, qJMKH_L0, loadCaseJK, ForceLoadType.Force, "", true, false);

//            // --- 4. HHW ---

//            double SlabThicknessBPL_min = SlabThicknessBPL.AsQueryable().Min();
//            double SlabThicknessBPL_max = SlabThicknessBPL.AsQueryable().Max();

//            if (HHW >= SlabThicknessBPL_max)
//            {
//                //  V
//                var HHWLoadV = new Vector3d(0, 0, -(HHW - SlabThicknessBPL[0]) * qW);
//                var SurfaceLoadHHW_V_baktass = new FemDesign.Loads.SurfaceLoad(regionBaktass, HHWLoadV, loadCaseHHW, false, "");
//                var SurfaceLoadHHW_V_framtass = new FemDesign.Loads.SurfaceLoad(regionFramtass, HHWLoadV, loadCaseHHW, false, "");

//                //  lift
//                // BPL
//                // var LiftLoadBPL = new Vector3d(0,0, VolumeBPL * qW/ AreaBPL);
//                // var SurfaceLoadHHW_liftBPL = new FemDesign.Loads.SurfaceLoad(regionBPL, LiftLoadBPL, loadCaseHHW,false,"");

//                // MUR
//                double meanZMUR = (PP2_0[2] + PP2_3[2]) / 2;
//                double AndelliftMUR = (HHW - SlabThicknessBPL_max) / meanZMUR;
//                //  double AreaMUR = FemDesign.Results.QuantityEstimationConcrete.Slab2.Area;
//                //  double VolumeMUR = FemDesign.Results.QuantityEstimationConcrete.Slab2.Volume;
//                //  var LiftLoadMUR = new Vector3d(0, 0, AndelliftMUR * VolumeMUR * qW/ AreaMUR);
//                var regionMUR = SlabMUR.Region;
//                //  var SurfaceLoadHHW_liftMUR = new FemDesign.Loads.SurfaceLoad(regionMUR, LiftLoadMUR, loadCaseHHW, false, "");

//            }
//            else if (HHW > 0)
//            {
//                //  lift BPL
//                double meanZBPL = (SlabThicknessBPL_min + SlabThicknessBPL_max) / 2;
//                double AndelliftBPL = HHW / meanZBPL;
//                //   var LiftLoadBPL = new Vector3d(0, 0, AndelliftBPL*VolumeBPL * qW / AreaBPL);
//                //   var SurfaceLoadHHW_liftBPL = new FemDesign.Loads.SurfaceLoad(regionBPL, LiftLoadBPL, loadCaseHHW, false, "");
//            }

//            // --- 4.1 Eff J HHW ---
//            if (HHW >= SlabThicknessBPL_max)
//            {
//                //  V
//                double EffJbaktass_max = Math.Min(HHW, JordH_baktass[0]);
//                var HHWLoadVbaktass = new Vector3d(0, 0, -(EffJbaktass_max - SlabThicknessBPL[0]) * qW);
//                var SurfaceLoadHHW_V_baktass = new FemDesign.Loads.SurfaceLoad(regionBaktass, HHWLoadVbaktass, loadCaseEffJHHW, false, "");

//                double EffJframtass_max = Math.Min(HHW, JordH_baktass[0]);
//                var HHWLoadVframtass = new Vector3d(0, 0, -(EffJbaktass_max - SlabThicknessBPL[0]) * qW);
//                var SurfaceLoadHHW_V_framtass = new FemDesign.Loads.SurfaceLoad(regionFramtass, HHWLoadVframtass, loadCaseEffJHHW, false, "");
//            }
//            // --- 5.0 MW ---

//            // --- 5.1 Eff J MW ---

//            // --- 6.0 LLW ---

//            // --- 6.1 Eff J LLW ---

//            // --- 7. ÖL U ---
//            //  V
//            // Definition of qÖL in point P2_0, P2_1 , P2_2, P2_3 
//            // qÖL= QÖL0/((L0+JordH*SpridL)*(B0+JordH*SpridB))
//            var qÖLVU = new List<double> { QÖLU0 / (LU0 * BU0), QÖLU0 /((LU0+JordH_baktass[0]*SpridLU) * (BU0 + JordH_baktass[0] * (SpridBU[0] + SpridBU[1]))),
//                QÖLU0 / ((LU0 + JordH_baktass[1] * SpridLU) * (BU0 + JordH_baktass[1] * (SpridBU[0] + SpridBU[1]))), QÖLU0 / (LU0 * BU0) };

//            var LoadÖLUV_0 = new LoadLocationValue(PBaktass_0, qÖLVU[1]);
//            var LoadÖLUV_1 = new LoadLocationValue(P1_1, qÖLVU[1]);
//            var LoadÖLUV_2 = new LoadLocationValue(P1_2, qÖLVU[2]);
//            var LoadÖLUV = new List<LoadLocationValue> { LoadÖLUV_0, LoadÖLUV_1, LoadÖLUV_2 };

//            var SurfaceLoadÖLVU = new FemDesign.Loads.SurfaceLoad(regionBaktass, LoadÖLUV, LoaddirJV, loadCaseÖLVU, false, "");

//            //  H
//            var qÖLHU = new List<double> { qÖLVU[0] * K_ULS[0], qÖLVU[1] * K_ULS[0], qÖLVU[2] * K_ULS[0] };

//            var LoadÖLHU_0 = new LoadLocationValue(P2_0_TillH, qÖLHU[0]);
//            var LoadÖLHU_1 = new LoadLocationValue(P2_1, qÖLHU[1]);
//            var LoadÖLHU_2 = new LoadLocationValue(P2_2, qÖLHU[2]);
//            var LoadÖLHU = new List<LoadLocationValue> { LoadÖLHU_0, LoadÖLHU_1, LoadÖLHU_2 };

//            var SurfaceLoadÖLHU = new FemDesign.Loads.SurfaceLoad(regionLoadMUR, LoadÖLHU, LoaddirJH, loadCaseÖLHU, false, "");

//            var qÖLUH_L0 = new Vector3d(-qÖLHU[2] * SlabThicknessBPL[0], 0, 0);
//            var qÖLUH_L1 = new Vector3d(-qÖLHU[1] * SlabThicknessBPL[0], 0, 0);
//            var lineLoadÖLUH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qÖLUH_L0, qÖLUH_L1, loadCaseÖLHU, ForceLoadType.Force, "", true, false);
//            // --- 8. ÖL P ---
//            //  V
//            var qÖLVP = new List<double> { QÖLP0 / (LP0 * BP0), QÖLP0 / ((LP0 + JordH_baktass[0] * SpridLP) * (BP0 + JordH_baktass[0] * (SpridBP[0] + SpridBP[1]))),
//                QÖLP0 / ((LP0 + JordH_baktass[1] * SpridLP) * (BP0 + JordH_baktass[1] * (SpridBP[0] + SpridBP[1]))), QÖLP0 / (LP0 * BP0) };

//            var LoadÖLPV_0 = new LoadLocationValue(PBaktass_0, qÖLVP[1]);
//            var LoadÖLPV_1 = new LoadLocationValue(P1_1, qÖLVP[1]);
//            var LoadÖLPV_2 = new LoadLocationValue(P1_2, qÖLVP[2]);
//            var LoadÖLPV = new List<LoadLocationValue> { LoadÖLPV_0, LoadÖLPV_1, LoadÖLPV_2 };

//            var SurfaceLoadÖLVP = new FemDesign.Loads.SurfaceLoad(regionBaktass, LoadÖLPV, LoaddirJV, loadCaseÖLVP, false, "");

//            //  H
//            var qÖLHP = new List<double> { qÖLVP[0] * K_SLS[0], qÖLVP[1] * K_SLS[0], qÖLVP[2] * K_SLS[0] };

//            var LoadÖLHP_0 = new LoadLocationValue(P2_0_TillH, qÖLHP[0]);
//            var LoadÖLHP_1 = new LoadLocationValue(P2_1, qÖLHP[1]);
//            var LoadÖLHP_2 = new LoadLocationValue(P2_2, qÖLHP[2]);
//            var LoadÖLHP = new List<LoadLocationValue> { LoadÖLHP_0, LoadÖLHP_1, LoadÖLHP_2 };

//            var SurfaceLoadÖLHP = new FemDesign.Loads.SurfaceLoad(regionLoadMUR, LoadÖLHP, LoaddirJH, loadCaseÖLHP, false, "");

//            var qÖLPH_L0 = new Vector3d(-qÖLHP[2] * SlabThicknessBPL[0], 0, 0);
//            var qÖLPH_L1 = new Vector3d(-qÖLHP[1] * SlabThicknessBPL[0], 0, 0);
//            var lineLoadÖLPH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qÖLPH_L0, qÖLPH_L1, loadCaseÖLHP, ForceLoadType.Force, "", true, false);

//            // --- 9. Olycka ---
//            var qAcc_L = new Vector3d(-qAcc, 0, 0);
//            var lineLoadAcc = new FemDesign.Loads.LineLoad(LineR, qAcc_L, loadCaseAcc, ForceLoadType.Force, "", true, false);

//            var loads = new List<ILoadElement>
//            {
//              lineLoadR,
//              SurfaceLoadJUV,
//              SurfaceLoadJUH,
//              lineLoadJUH,
//              SurfaceLoadJMUV,
//              SurfaceLoadJMUH,
//              lineLoadJMUH,
//              SurfaceLoadJAV,
//              SurfaceLoadJAH,
//              lineLoadJAH,
//              SurfaceLoadJMAV,
//              SurfaceLoadJMAH,
//              lineLoadJMAH,
//              SurfaceLoadJKV,
//              SurfaceLoadJKH,
//              lineLoadJKH,
//              SurfaceLoadJMKV,
//              SurfaceLoadJMKH,
//              lineLoadJMKH,
//              SurfaceLoadÖLVU,
//              SurfaceLoadÖLHU,
//              lineLoadÖLUH,
//              SurfaceLoadÖLVP,
//              SurfaceLoadÖLHP,
//              lineLoadÖLPH,
//            };

//            return loads();
//        }
//    }
//}
