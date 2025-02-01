using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FemDesign.Loads;
using FemDesign.GenericClasses;
using FemDesign.Shells;
using FemDesign.Geometry;

namespace RetainingWall
{
    internal class Load
    {
        public static List<FemDesign.GenericClasses.ILoadElement> loads(List<Slab> slabs, List<LoadCase> loadcases)
        {
            // Predefinition from input data
            // slabs
            Slab SlabBPL = slabs[0];
            Slab SlabMUR = slabs[1];

            var P_BPL = SlabBPL.SlabPart.Region.Contours[0].Points;
            var P_MUR = SlabMUR.SlabPart.Region.Contours[0].Points;

            var T_BPL = SlabBPL.SlabPart.Thickness;
            var T_MUR = SlabMUR.SlabPart.Thickness;

            //loadcases
            LoadCase loadCaseDL         =	loadcases[0];
            LoadCase loadCaseR          =	loadcases[1];
            LoadCase loadCaseJU         =	loadcases[2];
            LoadCase loadCaseJK         =	loadcases[3];
            LoadCase loadCaseJA         =	loadcases[4];
            LoadCase loadCaseHHW        =	loadcases[5];
            LoadCase loadCaseMW         =	loadcases[6];
            LoadCase loadCaseLLW        =	loadcases[7];
            //LoadCase loadCaseEffJHHW    =	loadcases[8];
            //LoadCase loadCaseEffJMW     =	loadcases[9];
            //LoadCase loadCaseEffJLLW    =	loadcases[10];
            LoadCase loadCaseÖLVU       =	loadcases[8];
            LoadCase loadCaseÖLHU       =	loadcases[9];
            LoadCase loadCaseÖLVP       =	loadcases[10];
            LoadCase loadCaseÖLHP       =	loadcases[11];
            LoadCase loadCaseAcc        =   loadcases[12];

            //LoadCase Testlast1 = loadcases[XX];
            //LoadCase Testlast2 = loadcases[XX];
            //LoadCase Testlast3 = loadcases[XX];

            // ----- INDATA -----

            double qR = 1;      // Räckestyngd
            // --- Earth ---
            // JordH Tillbakafyll_PO, Tillbakafyll_P1, Motfyll_PO, Motfyll_PO

            var JordH = new List<double> { 0.0, 0.0, 0.8, 0.8 }; // Tillbakafyllning visningshöjd (ÖK MUR) 0/1 / Motfyllning fyllningshöjd (UK BPL) 0/1
            var AlfaJ = new List<double> { Math.PI / 180 * 0, Math.PI / 180 * -10 }; // Marklutning utifrån MUR Tillbakafyllning, Motfyllning

            double qJ = 18.0; // new List<double> { 18, 11 };     // Fyllning Naturfuktig, Effektiv tunghet under GVY
            var Hpack1 = 0.3;
            var qpack = 12; // Vibratorplatta 100 kg enligt PGHB kap 2.33 tabell 2:1

            // Tillbakafyllning , Motfyllning
            var K_ULS = new List<double> { 0.30, 3.30 };
            var K_Glid = new List<double> { 0.30, 3.30 };
            var K_SLS = new List<double> { 0.40, 0.40 };
            // --- Water --
            double qW = 10; // Vatten tyngd
            double HHW = 2; // Vattenpelare från UK BPL 
            double MW = 1;

            double LLW = 0;

            double T_BPL_min = T_BPL[1].Value;
            double T_BPL_max = T_BPL[0].Value;
            // --- Surcharge ---

            double yd = 0.91; // Säkerhetsklass 1 / 2 / 3  0.83 / 0.91 / 1.0
            var pkoff_Geo = new List<double> { 0.91 * 1.1, 0.91 * 1.4 };
            double QÖLU0 = 15; // (270 + 180) / 2;     // Överlaststyngd (kN)
            double LU0 = 1; //  2.2;                   // Längd för spridning vid ytan
            double BU0 = 1; //  3.0;                   // Bredd för spridning vid ytan
            // Spridökning per djup
            double SpridLU = 0;  // Math.Tan(30 * Math.PI / 180); // Eruokod 4.9.1 ansätter spridning till 30 grader genom omsorgsfullt komprimerad fyllning

            //var SpridBU = new List<double> { 0, 0 };  // OBS! KB Tabell 7.1-5, ka 4.9.1 "Spridning får inte göras utanför betraktad konstruktions utsträckning". 

            double QÖLP0 = 4.5;
            double LP0 = 1;
            double BP0 = 1;
            double SpridLP = 0;
            //var SpridBP = new List<double> { 0, 0 };
            // qÖL_V= QÖL0/(L0+JordH*SpridL)
            // --- Accidental ---
            double qAcc = 100 / 6;  // Olyckslast / fördelningslängd
            // ------------------

            //P_BPL[0].X = P_BPL[0].X    P_BPL[0].Y = P_BPL[0].Y   P_BPL[0].Z = P_BPL[0].Z  
            //P_BPL[1].X = P_BPL[1].X    P_BPL[1].Y = P_BPL[1].Y   P_BPL[1].Z = P_BPL[1].Z  
            //P_BPL[2].X = P_BPL[2].X    P_BPL[2].Y = P_BPL[2].Y   P_BPL[2].Z = P_BPL[2].Z  
            //P_BPL[3].X = P_BPL[3].X    P_BPL[3].Y = P_BPL[3].Y   P_BPL[3].Z = P_BPL[3].Z  

            //P_MUR[0].X = P_BPL[0].X    P_MUR[0].Y = P_BPL[0].Y   P_MUR[0].Z = P_BPL[0].Z  
            //P_MUR[1].X = P_BPL[1].X    P_MUR[1].Y = P_BPL[1].Y   P_MUR[1].Z = P_BPL[1].Z  
            //P_MUR[2].X = P_BPL[2].X    P_MUR[2].Y = P_BPL[2].Y   P_MUR[2].Z = P_BPL[2].Z  
            //P_MUR[3].X = P_BPL[3].X    P_MUR[3].Y = P_BPL[3].Y   P_MUR[3].Z = P_BPL[3].Z   

            // --- Räcke ---
            var LineR = SlabMUR.Region.Contours[0].Edges[3];
            var qR_L = new Vector3d(0, 0, -qR);
            var lineLoadR = new FemDesign.Loads.LineLoad(LineR, qR_L, loadCaseR, ForceLoadType.Force, "", true, false);

            // --- 1. Jord U ---
            //  Tillbakafyllning
            //  V
            var PBaktass_0 = new Point3d(P_MUR[1].X + T_MUR[1].Value / 2, P_MUR[1].Y, P_BPL[1].Z);
            //var PBaktass_1 = new Point3d(P_MUR[2].X + T_MUR[1].Value / 2, P_MUR[2].Y, P_BPL[2].Z);

            var PBaktass_1 = new Point3d(P_MUR[2].X + T_MUR[1].Value / 2, P_MUR[2].Y, P_BPL[2].Z);

            var LBaktass = new List<double> { P_BPL[1].X - (P_MUR[1].X + T_MUR[0].Value / 2), P_BPL[2].X - (P_MUR[2].X + T_MUR[0].Value / 2) };
            // JordH in points: PBaktass_0 , P_BPL[1], P_BPL[2], PBaktass_1
            var JordH_baktass = new List<double> { P_MUR[0].Z - JordH[0], (P_MUR[0].Z - JordH[0]) + LBaktass[0] * Math.Tan(AlfaJ[0]),
                                                 (P_MUR[3].Z - JordH[1]) + LBaktass[1] * Math.Tan(AlfaJ[0]) , P_MUR[3].Z - JordH[1] };

            var posJV = new Point3d(P_MUR[1].X + T_MUR[1].Value / 2, 0, P_BPL[0].Z);
            var regionBaktass = FemDesign.Geometry.Region.RectangleXY(posJV, P_BPL[1].X - (P_MUR[1].X + T_MUR[1].Value / 2), P_MUR[2].Y);

            var LoaddirJV = new Vector3d(0, 0, -1);

            var LoadJUV_0 = new LoadLocationValue(PBaktass_0, (JordH_baktass[0] - T_BPL[0].Value) * qJ);
            var LoadJUV_1 = new LoadLocationValue(P_BPL[1],   (JordH_baktass[1] - T_BPL[0].Value) * qJ);
            var LoadJUV_2 = new LoadLocationValue(P_BPL[2],   (JordH_baktass[2] - T_BPL[0].Value) * qJ);
            var LoadJUV = new List<LoadLocationValue> { LoadJUV_0, LoadJUV_1, LoadJUV_2 };

            var SurfaceLoadJUV = new FemDesign.Loads.SurfaceLoad(regionBaktass, LoadJUV, LoaddirJV, loadCaseJU, false, "JUV");

            //  H
            var P2_0_TillH = new Point3d(P_MUR[0].X, P_MUR[0].Y, P_MUR[0].Z - JordH[0]);
            var P2_3_TillH = new Point3d(P_MUR[3].X, P_MUR[3].Y, P_MUR[3].Z - JordH[1]);
            var P_Load_MUR = new List<Point3d> { P2_0_TillH, P_MUR[1], P_MUR[2], P2_3_TillH };
            var regionLoadMUR = new Region(P_Load_MUR, Plane.YZ);

            var LoaddirJH = new Vector3d(-1, 0, 0);

            var qhJUH = new List<double> { qJ * K_ULS[0], qJ * K_ULS[1] };

            var LoadJUH_0 = new LoadLocationValue(P2_0_TillH, 0);
            var LoadJUH_1 = new LoadLocationValue(P_MUR[1], (JordH_baktass[1] - T_BPL[0].Value) * qhJUH[0]);
            var LoadJUH_2 = new LoadLocationValue(P_MUR[2], (JordH_baktass[3] - T_BPL[0].Value) * qhJUH[0]);
            var LoadJUH = new List<LoadLocationValue> { LoadJUH_0, LoadJUH_1, LoadJUH_2 };

            var SurfaceLoadJUH = new FemDesign.Loads.SurfaceLoad(regionLoadMUR, LoadJUH, LoaddirJH, loadCaseJU, false, "JUH");

            var LineBPL_TillH = SlabBPL.Region.Contours[0].Edges[1];
            var qJUH_LL = new List<double> { (JordH_baktass[1] - T_BPL[0].Value / 2) * qhJUH[0], (JordH_baktass[2] - T_BPL[0].Value / 2) * qhJUH[0] };
            var qJUH_L0 = new Vector3d(-qJUH_LL[0] * T_BPL[0].Value, 0, 0);
            var qJUH_L1 = new Vector3d(-qJUH_LL[1] * T_BPL[0].Value, 0, 0);
            var lineLoadJUH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qJUH_L0, qJUH_L1, loadCaseJU, ForceLoadType.Force, "LL_JUH", true, false);

            var qJUH_L2 = new Vector3d(0, qJUH_L0.X * T_BPL[0].Value / 2, 0);
            var qJUH_L3 = new Vector3d(0, qJUH_L1.X * T_BPL[0].Value / 2, 0);
            var lineMomentJUH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qJUH_L2, qJUH_L3, loadCaseJU, ForceLoadType.Moment, "LM_JUH", true, false);


            var LoadDummypack_0 = new LoadLocationValue(P_MUR[0], 0);
            var LoadDummypack_1 = new LoadLocationValue(P2_0_TillH, 0);
            var LoadDummypack_2 = new LoadLocationValue(P2_3_TillH, 0);
            var LoadDummypack = new List<LoadLocationValue> { LoadDummypack_0, LoadDummypack_1, LoadDummypack_2 };

            var H_Mur = new List<double> { P_MUR[0].Z - P_MUR[1].Z, P_MUR[3].Z - P_MUR[2].Z };

            Point3d PDummy = new Point3d(0, 0, 0);

            Point3d P2_0_TillHpack2 = PDummy;
            Point3d P2_3_TillHpack2 = PDummy;

            SurfaceLoad SurfaceLoadJUH_pack1 = new FemDesign.Loads.SurfaceLoad(regionLoadMUR, LoadDummypack, LoaddirJH, loadCaseJU, false, "");
            SurfaceLoad SurfaceLoadJUH_pack2 = new FemDesign.Loads.SurfaceLoad(regionLoadMUR, LoadDummypack, LoaddirJH, loadCaseJU, false, "");

            LoadLocationValue LoadJUH_pack2_1 = new LoadLocationValue(PDummy, 0);
            LoadLocationValue LoadJUH_pack2_2 = new LoadLocationValue(PDummy, 0);

            var qÖLVU_krit = QÖLU0 / (LU0 + Hpack1 * SpridLU);

            double Hpack0;

            // Packning
            if ( K_ULS[0] * (pkoff_Geo[1] * qÖLVU_krit + pkoff_Geo[0] * qJ * Hpack1) < qpack)
            {
                // Packning del 1
                if (SpridLU == 0)
                {
                    // Start ekvation: 0 = Kx ( yd 1.4 Qv + yd 1.1 qj hj) - (qack/Hpack1) * hj
                    Hpack0 = (- pkoff_Geo[1] * (QÖLU0 / LU0)) / (pkoff_Geo[0] * qJ - qpack / (K_ULS[0] * Hpack1));
                }
                else
                {
                    double qpack_Δ = qpack / Hpack1;
                    // Start ekvation: 0 = Kx ( yd 1.4 * Qv / (LU0 + hj tan(a)) + yd 1.1 qj hj) - qpack_Δ hj 
                    double A = SpridLU * pkoff_Geo[0] * qJ;                          // A = SpridLU yd 1.1 qj
                    double B = pkoff_Geo[0] * qJ * LU0 - qpack_Δ * SpridLU;          // B = yd 1.1 * qj * LU0 - qpack_Δ * SpridLU
                    double C = pkoff_Geo[1] * QÖLU0 - (qpack_Δ / K_ULS[0]) * LU0;       // C = yd 1.4 Qv - (qpack_Δ / Kx) * LU0; 
                    // pq-formel --> Hpack2 = (-B +  √(B^2- 4AC))/ 2A
                    Hpack0 = (-B + Math.Sqrt(Math.Pow(B, 2) - 4 * A * C)) / (2 * A);

                }

                var P2_0_TillHpack0 = new Point3d(P_MUR[0].X, P_MUR[0].Y, P_MUR[0].Z - Hpack0);
                var P2_3_TillHpack0 = new Point3d(P_MUR[3].X, P_MUR[3].Y, P_MUR[3].Z - Hpack0);

                var P2_0_TillHpack1 = new Point3d(P_MUR[0].X, P_MUR[0].Y, P_MUR[0].Z - Hpack1);
                var P2_3_TillHpack1 = new Point3d(P_MUR[3].X, P_MUR[3].Y, P_MUR[3].Z - Hpack1);

                var P_Load_MUR_pack1 = new List<Point3d> { P2_0_TillHpack0, P2_0_TillHpack1, P2_3_TillHpack1, P2_3_TillHpack0 };
                var regionLoadMUR_pack1 = new Region(P_Load_MUR_pack1, Plane.YZ);

                var qJ_krit = K_ULS[0] * (pkoff_Geo[1] * (QÖLU0 / (LU0+ Hpack1*SpridLU)) + pkoff_Geo[0] * qJ * Hpack1);

                var LoadJUH_pack1_0 = new LoadLocationValue(P2_0_TillHpack0, 0);
                var LoadJUH_pack1_1 = new LoadLocationValue(P2_0_TillHpack1, qpack - qJ_krit);
                var LoadJUH_pack1_2 = new LoadLocationValue(P2_3_TillHpack1, qpack - qJ_krit);
                var LoadJUH_pack1 = new List<LoadLocationValue> { LoadJUH_pack1_0, LoadJUH_pack1_1, LoadJUH_pack1_2 };

                SurfaceLoadJUH_pack1 = new FemDesign.Loads.SurfaceLoad(regionLoadMUR_pack1, LoadJUH_pack1, LoaddirJH, loadCaseJU, false, "pack1");

                var Hpack2 =0.0;
                // Packning del 2
                // Förutsätter paritalfaktor 1.4 används för överlast

                // Höjd då Överlast är konstant
                if (SpridLU == 0)
                {
                    // Start ekvation: qpack = Kx ( yd 1.4 qÖLV + yd 1.1 qj Hpack2) 
                    Hpack2 = (qpack / K_ULS[0] - pkoff_Geo[1] * (QÖLU0 / LU0)) / (pkoff_Geo[0] * qJ);
                    //var Hpack3 = (qpack / K_ULS[0] - (QÖLU0 / LU0)) / (qJ[0]);
                }
                else // Höjd då Överlast minskar med murhöjd
                {
                    // Start ekvation: qpack = Kx ( yd 1.4 qÖLU0/ ( LU0 + Hpack2 tan(a) ) + yd 1.1 qj Hpack2)
                    double A = pkoff_Geo[0] * qJ * SpridLU;                              // A = yd 1.1 qj SpridLU
                    double B = pkoff_Geo[0] * qJ * LU0 - (qpack / K_ULS[0]) * SpridLU;   // B = yd 1.1 qj LU0 - (qpack / Kx) * SpridLU
                    double C = pkoff_Geo[1] * QÖLU0 - (qpack / K_ULS[0]) * LU0;             // C = yd 1.4 QÖLU0 -  (qpack / Kx) *  LU0
                    // pq-formel --> Hpack2 = (-B +  √(B^2- 4AC))/ 2A
                    Hpack2 = (-B + Math.Sqrt(Math.Pow(B, 2) - 4 * A * C)) / (2 * A);
                }

                // Sida 0
                if (Hpack2 > H_Mur[0]) // packningstryck når != 0 innan full murhöjd 
                {
                    P2_0_TillHpack2 = P_MUR[1];
                    LoadJUH_pack2_1 = new LoadLocationValue(P2_0_TillHpack2, qpack - H_Mur[0] * qhJUH[0]);
                }
                else // (Hpack2 < H_Mur[0]) // packningstryck når 0 innan full murhöjd
                {
                    P2_0_TillHpack2 = new Point3d(P_MUR[0].X, P_MUR[0].Y, P_MUR[0].Z - Hpack2);
                    LoadJUH_pack2_1 = new LoadLocationValue(P2_0_TillHpack2, 0);
                }

                // Sida 1
                if (Hpack2 > H_Mur[1]) // packningstryck når != 0 innan full murhöjd 
                {
                    P2_3_TillHpack2 = P_MUR[2];
                    LoadJUH_pack2_2 = new LoadLocationValue(P2_3_TillHpack2, qpack - H_Mur[1] * qhJUH[0]);
                }
                else // (Hpack2 < H_Mur[1]) // packningstryck når 0 innan full murhöjd
                {
                    P2_3_TillHpack2 = new Point3d(P_MUR[3].X, P_MUR[3].Y, P_MUR[3].Z - Hpack2);
                    LoadJUH_pack2_2 = new LoadLocationValue(P2_3_TillHpack2, 0);
                }

                var P_Load_MUR_pack2 = new List<Point3d> { P2_0_TillHpack1, P2_0_TillHpack2, P2_3_TillHpack2, P2_3_TillHpack1 };
                var regionLoadMUR_pack2 = new Region(P_Load_MUR_pack2, Plane.YZ);

                var LoadJUH_pack2_0 = new LoadLocationValue(P2_0_TillHpack1, qpack - qJ_krit); // Last i 1 & 2 kan variera och bestämms i if sats ovan
                var LoadJUH_pack2 = new List<LoadLocationValue> { LoadJUH_pack2_0, LoadJUH_pack2_1, LoadJUH_pack2_2 };

                SurfaceLoadJUH_pack2 = new FemDesign.Loads.SurfaceLoad(regionLoadMUR_pack2, LoadJUH_pack2, LoaddirJH, loadCaseJU, false, "pack2");

            }

            // ---  Motfyllning ---
            //  V

            var LFramtass = new List<double> { (P_MUR[1].X - T_MUR[1].Value / 2) - P_BPL[0].X, (P_MUR[2].X - T_MUR[1].Value / 2) - P_BPL[0].X };
            var JordH_framtass = new List<double> {  JordH[2] + LFramtass[0] * Math.Tan(AlfaJ[1]), JordH[2],
                                                     JordH[3] , JordH[3] + LFramtass[1] * Math.Tan(AlfaJ[1]) };
            var posJVM = new Point3d(P_BPL[0].X, P_BPL[0].Y, P_BPL[0].Z);
            var regionFramtass = FemDesign.Geometry.Region.RectangleXY(posJVM, (P_MUR[0].X - T_MUR[1].Value / 2), P_MUR[2].Y);

            var PFramtass_0 = new Point3d(P_MUR[0].X - T_MUR[0].Value / 2, P_MUR[0].Y, P_BPL[0].Z);
            var PFramtass_1 = new Point3d(P_MUR[3].X - T_MUR[0].Value / 2, P_MUR[3].Y, P_BPL[3].Z);

            var LoadJUMV_0 = new LoadLocationValue(P_BPL[0],    (JordH_framtass[0] - T_BPL[0].Value ) * qJ);
            var LoadJUMV_1 = new LoadLocationValue(PFramtass_0, (JordH_framtass[1] - T_BPL[0].Value ) * qJ);
            var LoadJUMV_2 = new LoadLocationValue(PFramtass_1, (JordH_framtass[2] - T_BPL[0].Value ) * qJ);
            var LoadJMUV = new List<LoadLocationValue> { LoadJUMV_0, LoadJUMV_1, LoadJUMV_2 };

            var LoadDummyJM_0 = new LoadLocationValue(P_BPL[0], 0);
            var LoadDummyJM_1 = new LoadLocationValue(PFramtass_0, 0);
            var LoadDummyJM_2 = new LoadLocationValue(PFramtass_1, 0);
            var LoadDummyJM = new List<LoadLocationValue> { LoadDummyJM_0, LoadDummyJM_1, LoadDummyJM_2 };

            var SurfaceLoadJMUV = new FemDesign.Loads.SurfaceLoad(regionFramtass, LoadDummyJM, LoaddirJV, loadCaseJU, false, "");
            double JordH_framtass_min = JordH_framtass.AsQueryable().Min();
            if (JordH_framtass_min > T_BPL[0].Value)
            {
                SurfaceLoadJMUV = new FemDesign.Loads.SurfaceLoad(regionFramtass, LoadJMUV, LoaddirJV, loadCaseJU, false, "JMUV");
            }

            //  H
            var P2_0_MfyllH = new Point3d(P_MUR[0].X, P_MUR[0].Y, P_BPL[0].Z + JordH[2]);
            var P2_3_MfyllH = new Point3d(P_MUR[3].X, P_MUR[3].Y, P_BPL[0].Z + JordH[3]);
            var P_Load_Mfyll = new List<Point3d> { P2_0_MfyllH, P_MUR[1], P_MUR[2], P2_3_MfyllH };
            var region_Load_Mfyll = new Region(P_Load_Mfyll, Plane.YZ);
            var LoaddirJMH = new Vector3d(1, 0, 0);

            var LoadJUMH_0 = new LoadLocationValue(P2_0_MfyllH, 0);
            var LoadJUMH_1 = new LoadLocationValue(P_MUR[1], (JordH_framtass[1] - T_BPL[0].Value) * qhJUH[1]);
            var LoadJUMH_2 = new LoadLocationValue(P_MUR[2], (JordH_framtass[2] - T_BPL[0].Value) * qhJUH[1]);
            var LoadJMUH = new List<LoadLocationValue> { LoadJUMH_0, LoadJUMH_1, LoadJUMH_2 };

            var SurfaceLoadJMUH = new FemDesign.Loads.SurfaceLoad(region_Load_Mfyll, LoadDummyJM, LoaddirJMH, loadCaseJU, false, "");
            if (JordH_framtass_min > T_BPL[0].Value)
            {
                SurfaceLoadJMUH = new FemDesign.Loads.SurfaceLoad(region_Load_Mfyll, LoadJMUH, LoaddirJMH, loadCaseJU, false, "JMUH");
            }

            var LineBPL_MotH = SlabBPL.Region.Contours[0].Edges[3];

            var qJMUH_LL = new List<double> { (JordH_framtass[0] / 2) * qhJUH[1], (JordH_framtass[3] / 2) * qhJUH[1] };

            if (JordH_framtass_min > T_BPL[0].Value)
            {
                qJMUH_LL = new List<double> { (JordH_framtass[0] - T_BPL[0].Value / 2) * qhJUH[1], (JordH_framtass[3] - T_BPL[0].Value / 2) * qhJUH[1] };
            }

            var qJMUH_L0 = new Vector3d(qJMUH_LL[0] * T_BPL[0].Value, 0, 0);
            var qJMUH_L1 = new Vector3d(qJMUH_LL[1] * T_BPL[0].Value, 0, 0);
            var lineLoadJMUH = new FemDesign.Loads.LineLoad(LineBPL_MotH, qJMUH_L1, qJMUH_L0, loadCaseJU, ForceLoadType.Force, "LL_JMUH", true, false);

            var qJMUH_L2 = new Vector3d(0, qJMUH_L0.X * T_BPL[0].Value / 2, 0);
            var qJMUH_L3 = new Vector3d(0, qJMUH_L1.X * T_BPL[0].Value / 2, 0);
            var lineMomentJMUH = new FemDesign.Loads.LineLoad(LineBPL_MotH, qJMUH_L2, qJMUH_L3, loadCaseJU, ForceLoadType.Moment, "LM_JMUH", true, false);

            //// --- 2. Jord Glid ---

            //  Tillbakafyllning
            //  V

            var LoadJAV_0 = new LoadLocationValue(PBaktass_0,   (JordH_baktass[0] - T_BPL[0].Value) * qJ);
            var LoadJAV_1 = new LoadLocationValue(P_BPL[1],     (JordH_baktass[1] - T_BPL[0].Value) * qJ);
            var LoadJAV_2 = new LoadLocationValue(P_BPL[2],     (JordH_baktass[2] - T_BPL[0].Value) * qJ);
            var LoadJAV = new List<LoadLocationValue> { LoadJAV_0, LoadJAV_1, LoadJAV_2 };

            var SurfaceLoadJAV = new FemDesign.Loads.SurfaceLoad(regionBaktass, LoadJAV, LoaddirJV, loadCaseJA, false, "JAV");

            //  H

            var qhJAH = new List<double> { qJ * K_Glid[0], qJ * K_Glid[1] };

            var LoadJAH_0 = new LoadLocationValue(P2_0_TillH, 0);
            var LoadJAH_1 = new LoadLocationValue(P_MUR[1], (JordH_baktass[1] - T_BPL[0].Value) * qhJAH[0]);
            var LoadJAH_2 = new LoadLocationValue(P_MUR[2], (JordH_baktass[3] - T_BPL[0].Value) * qhJAH[0]);
            var LoadJAH = new List<LoadLocationValue> { LoadJAH_0, LoadJAH_1, LoadJAH_2 };

            var SurfaceLoadJAH = new FemDesign.Loads.SurfaceLoad(regionLoadMUR, LoadJAH, LoaddirJH, loadCaseJA, false, "JAH");

            var qJAH_LL = new List<double> { (JordH_baktass[1] - T_BPL[0].Value / 2) * qhJAH[0], (JordH_baktass[2] - T_BPL[0].Value / 2) * qhJAH[0] };
            var qJAH_L0 = new Vector3d(-qJAH_LL[0] * T_BPL[0].Value, 0, 0);
            var qJAH_L1 = new Vector3d(-qJAH_LL[1] * T_BPL[0].Value, 0, 0);
            var lineLoadJAH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qJAH_L0, qJAH_L1, loadCaseJA, ForceLoadType.Force, "LL_JAH", true, false);

            var qJAH_L2 = new Vector3d(0, qJAH_L0.X * T_BPL[0].Value / 2, 0);
            var qJAH_L3 = new Vector3d(0, qJAH_L1.X * T_BPL[0].Value / 2, 0);
            var lineMomentJAH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qJAH_L2, qJAH_L3, loadCaseJA, ForceLoadType.Moment, "", true, false);

            SurfaceLoad SurfaceLoadJAH_pack1 = new FemDesign.Loads.SurfaceLoad(regionLoadMUR, LoadDummypack, LoaddirJH, loadCaseJA, false, "");
            SurfaceLoad SurfaceLoadJAH_pack2 = new FemDesign.Loads.SurfaceLoad(regionLoadMUR, LoadDummypack, LoaddirJH, loadCaseJA, false, "");

            LoadLocationValue LoadJAH_pack2_1 = new LoadLocationValue(PDummy, 0);
            LoadLocationValue LoadJAH_pack2_2 = new LoadLocationValue(PDummy, 0);


            // Packning
            if (K_Glid[0] * (pkoff_Geo[1] * qÖLVU_krit + pkoff_Geo[0] * qJ * Hpack1) < qpack)
            {
                // Packning del 1
                if (SpridLU == 0)
                {
                    // Start ekvation: 0 = Kx ( yd 1.4 Qv + yd 1.1 qj hj) - (qack/Hpack1) * hj
                    Hpack0 = (-pkoff_Geo[1] * (QÖLU0 / LU0)) / (pkoff_Geo[0] * qJ - qpack / (K_Glid[0] * Hpack1));
                }
                else
                {
                    double qpack_Δ = qpack / Hpack1;
                    // Start ekvation: 0 = Kx ( yd 1.4 * Qv / (LU0 + hj tan(a)) + yd 1.1 qj hj) - qpack_Δ hj 
                    double A = SpridLU * pkoff_Geo[0] * qJ;                          // A = SpridLU yd 1.1 qj
                    double B = pkoff_Geo[0] * qJ * LU0 - qpack_Δ * SpridLU;          // B = yd 1.1 * qj * LU0 - qpack_Δ * SpridLU
                    double C = pkoff_Geo[1] * QÖLU0 - (qpack_Δ / K_Glid[0]) * LU0;       // C = yd 1.4 Qv - (qpack_Δ / Kx) * LU0; 
                    // pq-formel --> Hpack2 = (-B +  √(B^2- 4AC))/ 2A
                    Hpack0 = (-B + Math.Sqrt(Math.Pow(B, 2) - 4 * A * C)) / (2 * A);

                }

                var P2_0_TillHpack0 = new Point3d(P_MUR[0].X, P_MUR[0].Y, P_MUR[0].Z - Hpack0);
                var P2_3_TillHpack0 = new Point3d(P_MUR[3].X, P_MUR[3].Y, P_MUR[3].Z - Hpack0);

                var P2_0_TillHpack1 = new Point3d(P_MUR[0].X, P_MUR[0].Y, P_MUR[0].Z - Hpack1);
                var P2_3_TillHpack1 = new Point3d(P_MUR[3].X, P_MUR[3].Y, P_MUR[3].Z - Hpack1);

                var P_Load_MUR_pack1 = new List<Point3d> { P2_0_TillHpack0, P2_0_TillHpack1, P2_3_TillHpack1, P2_3_TillHpack0 };
                var regionLoadMUR_pack1 = new Region(P_Load_MUR_pack1, Plane.YZ);

                var qJ_krit = K_Glid[0] * (pkoff_Geo[1] * (QÖLU0 / (LU0 + Hpack1 * SpridLU)) + pkoff_Geo[0] * qJ * Hpack1);

                var LoadJAH_pack1_0 = new LoadLocationValue(P2_0_TillHpack0, 0);
                var LoadJAH_pack1_1 = new LoadLocationValue(P2_0_TillHpack1, qpack - qJ_krit);
                var LoadJAH_pack1_2 = new LoadLocationValue(P2_3_TillHpack1, qpack - qJ_krit);
                var LoadJAH_pack1 = new List<LoadLocationValue> { LoadJAH_pack1_0, LoadJAH_pack1_1, LoadJAH_pack1_2 };

                SurfaceLoadJAH_pack1 = new FemDesign.Loads.SurfaceLoad(regionLoadMUR_pack1, LoadJAH_pack1, LoaddirJH, loadCaseJA, false, "pack1");

                var Hpack2 = 0.0;
                // Packning del 2
                // Förutsätter paritalfaktor 1.4 används för överlast

                // Höjd då Överlast är konstant
                if (SpridLU == 0)
                {
                    // Start ekvation: qpack = Kx ( yd 1.4 qÖLV + yd 1.1 qj Hpack2) 
                    Hpack2 = (qpack / K_Glid[0] - pkoff_Geo[1] * (QÖLU0 / LU0)) / (pkoff_Geo[0] * qJ);
                    //var Hpack3 = (qpack / K_Glid[0] - (QÖLU0 / LU0)) / (qJ[0]);
                }
                else // Höjd då Överlast minskar med murhöjd
                {
                    // Start ekvation: qpack = Kx ( yd 1.4 qÖLU0/ ( LU0 + Hpack2 tan(a) ) + yd 1.1 qj Hpack2)
                    double A = pkoff_Geo[0] * qJ * SpridLU;                              // A = yd 1.1 qj SpridLU
                    double B = pkoff_Geo[0] * qJ * LU0 - (qpack / K_Glid[0]) * SpridLU;   // B = yd 1.1 qj LU0 - (qpack / Kx) * SpridLU
                    double C = pkoff_Geo[1] * QÖLU0 - (qpack / K_Glid[0]) * LU0;             // C = yd 1.4 QÖLU0 -  (qpack / Kx) *  LU0
                    // pq-formel --> Hpack2 = (-B +  √(B^2- 4AC))/ 2A
                    Hpack2 = (-B + Math.Sqrt(Math.Pow(B, 2) - 4 * A * C)) / (2 * A);
                }

                // Sida 0
                if (Hpack2 > H_Mur[0]) // packningstryck når != 0 innan full murhöjd 
                {
                    P2_0_TillHpack2 = P_MUR[1];
                    LoadJAH_pack2_1 = new LoadLocationValue(P2_0_TillHpack2, qpack - H_Mur[0] * qhJAH[0]);
                }
                else // (Hpack2 < H_Mur[0]) // packningstryck når 0 innan full murhöjd
                {
                    P2_0_TillHpack2 = new Point3d(P_MUR[0].X, P_MUR[0].Y, P_MUR[0].Z - Hpack2);
                    LoadJAH_pack2_1 = new LoadLocationValue(P2_0_TillHpack2, 0);
                }

                // Sida 1
                if (Hpack2 > H_Mur[1]) // packningstryck når != 0 innan full murhöjd 
                {
                    P2_3_TillHpack2 = P_MUR[2];
                    LoadJAH_pack2_2 = new LoadLocationValue(P2_3_TillHpack2, qpack - H_Mur[1] * qhJAH[0]);
                }
                else // (Hpack2 < H_Mur[1]) // packningstryck når 0 innan full murhöjd
                {
                    P2_3_TillHpack2 = new Point3d(P_MUR[3].X, P_MUR[3].Y, P_MUR[3].Z - Hpack2);
                    LoadJAH_pack2_2 = new LoadLocationValue(P2_3_TillHpack2, 0);
                }

                var P_Load_MUR_pack2 = new List<Point3d> { P2_0_TillHpack1, P2_0_TillHpack2, P2_3_TillHpack2, P2_3_TillHpack1 };
                var regionLoadMUR_pack2 = new Region(P_Load_MUR_pack2, Plane.YZ);

                var LoadJAH_pack2_0 = new LoadLocationValue(P2_0_TillHpack1, qpack - qJ_krit); // Last i 1 & 2 kan variera och bestämms i if sats ovan
                var LoadJAH_pack2 = new List<LoadLocationValue> { LoadJAH_pack2_0, LoadJAH_pack2_1, LoadJAH_pack2_2 };

                SurfaceLoadJAH_pack2 = new FemDesign.Loads.SurfaceLoad(regionLoadMUR_pack2, LoadJAH_pack2, LoaddirJH, loadCaseJA, false, "pack2");

            }

            // ---  Motfyllning ---
            //  V

            var SurfaceLoadJMAV = new FemDesign.Loads.SurfaceLoad(regionFramtass, LoadDummyJM, LoaddirJV, loadCaseJA, false, "");

            if (JordH_framtass_min > T_BPL[0].Value)
            {
                SurfaceLoadJMAV = new FemDesign.Loads.SurfaceLoad(regionFramtass, LoadJMUV, LoaddirJV, loadCaseJA, false, "JMAV");
            }

            //  H

            var LoadJAMH_0 = new LoadLocationValue(P2_0_MfyllH, 0);
            var LoadJAMH_1 = new LoadLocationValue(P_MUR[1], (JordH_framtass[1] - T_BPL[0].Value) * qhJAH[1]);
            var LoadJAMH_2 = new LoadLocationValue(P_MUR[2], (JordH_framtass[2] - T_BPL[0].Value) * qhJAH[1]);
            var LoadJMAH = new List<LoadLocationValue> { LoadJAMH_0, LoadJAMH_1, LoadJAMH_2 };

            var SurfaceLoadJMAH = new FemDesign.Loads.SurfaceLoad(region_Load_Mfyll, LoadDummyJM, LoaddirJMH, loadCaseJA, false, "");

            if (JordH_framtass_min > T_BPL[0].Value)
            {
                SurfaceLoadJMAH = new FemDesign.Loads.SurfaceLoad(region_Load_Mfyll, LoadJMAH, LoaddirJMH, loadCaseJA, false, "JMAH");
            }

            var qJMAH_LL = new List<double> { (JordH_framtass[0] / 2) * qhJAH[1], (JordH_framtass[3]/ 2) * qhJAH[1] };

            if (JordH_framtass_min > T_BPL[0].Value)
            {
                qJMAH_LL = new List<double> { (JordH_framtass[0] - T_BPL[0].Value / 2) * qhJAH[1], (JordH_framtass[3] - T_BPL[0].Value / 2) * qhJAH[1] };
            }

            var qJMAH_L0 = new Vector3d(qJMAH_LL[0] * T_BPL[0].Value, 0, 0);
            var qJMAH_L1 = new Vector3d(qJMAH_LL[1] * T_BPL[0].Value, 0, 0);
            var lineLoadJMAH = new FemDesign.Loads.LineLoad(LineBPL_MotH, qJMAH_L1, qJMAH_L0, loadCaseJA, ForceLoadType.Force, "LL_JMAH", true, false);

            var qJMAH_L2 = new Vector3d(0, qJMAH_L0.X * T_BPL[0].Value / 2, 0);
            var qJMAH_L3 = new Vector3d(0, qJMAH_L1.X * T_BPL[0].Value / 2, 0);
            var lineMomentJMAH = new FemDesign.Loads.LineLoad(LineBPL_MotH, qJMAH_L2, qJMAH_L3, loadCaseJA, ForceLoadType.Moment, "LM_JMAH", true, false);

            //// --- 3. Jord K ---
            //  Tillbakafyllning
            //  V

            var SurfaceLoadJKV = new FemDesign.Loads.SurfaceLoad(regionBaktass, LoadJUV, LoaddirJV, loadCaseJK, false, "JKV");

            //  H
            var qhJKH = new List<double> { qJ * K_SLS[0], qJ * K_SLS[1] };

            var LoadJKH_0 = new LoadLocationValue(P2_0_TillH, 0);
            var LoadJKH_1 = new LoadLocationValue(P_MUR[1], (JordH_baktass[0] - T_BPL[0].Value ) * qhJKH[0]);
            var LoadJKH_2 = new LoadLocationValue(P_MUR[2], (JordH_baktass[3] - T_BPL[0].Value ) * qhJKH[0]);
            var LoadJKH = new List<LoadLocationValue> { LoadJKH_0, LoadJKH_1, LoadJKH_2 };

            var SurfaceLoadJKH = new FemDesign.Loads.SurfaceLoad(regionLoadMUR, LoadJKH, LoaddirJH, loadCaseJK, false, "JKH");

            var qJKH_LL = new List<double> { (JordH_baktass[0] - T_BPL[0].Value / 2) * qhJKH[0], (JordH_baktass[3] - T_BPL[0].Value / 2) * qhJKH[0] };
            var qJKH_L0 = new Vector3d(-qJKH_LL[0] * T_BPL[0].Value, 0, 0);
            var qJKH_L1 = new Vector3d(-qJKH_LL[1] * T_BPL[0].Value, 0, 0);
            var lineLoadJKH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qJKH_L0, qJKH_L1, loadCaseJK, ForceLoadType.Force, "LL_JKH", true, false);

            var qJKH_L2 = new Vector3d(0, qJKH_L0.X * T_BPL[0].Value / 2, 0);
            var qJKH_L3 = new Vector3d(0, qJKH_L1.X * T_BPL[0].Value / 2, 0);
            var lineMomentJKH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qJKH_L2, qJKH_L3, loadCaseJK, ForceLoadType.Moment, "LM_JKH", true, false);

            SurfaceLoad SurfaceLoadJKH_pack1 = new FemDesign.Loads.SurfaceLoad(regionLoadMUR, LoadDummypack, LoaddirJH, loadCaseJK, false, "");
            SurfaceLoad SurfaceLoadJKH_pack2 = new FemDesign.Loads.SurfaceLoad(regionLoadMUR, LoadDummypack, LoaddirJH, loadCaseJK, false, "");

            LoadLocationValue LoadJKH_pack2_1 = new LoadLocationValue(PDummy, 0);
            LoadLocationValue LoadJKH_pack2_2 = new LoadLocationValue(PDummy, 0);

            // Packning
            if (K_SLS[0] * (  qÖLVU_krit +  qJ * Hpack1) < qpack)
            {
                // Packning del 1
                if (SpridLP == 0)
                {
                    // Start ekvation: 0 = Kx ( yd 1.4 Qv + yd 1.1 qj hj) - (qack/Hpack1) * hj
                    Hpack0 = (-  (QÖLP0 / LP0)) / ( qJ - qpack / (K_SLS[0] * Hpack1));
                }
                else
                {
                    double qpack_Δ = qpack / Hpack1;
                    // Start ekvation: 0 = Kx ( yd 1.4 * Qv / (LP0 + hj tan(a)) + yd 1.1 qj hj) - qpack_Δ hj 
                    double A = SpridLP *  qJ;                          // A = SpridLP yd 1.1 qj
                    double B =  qJ * LP0 - qpack_Δ * SpridLP;          // B = yd 1.1 * qj * LP0 - qpack_Δ * SpridLP
                    double C =  QÖLP0 - (qpack_Δ / K_SLS[0]) * LP0;       // C = yd 1.4 Qv - (qpack_Δ / Kx) * LP0; 
                    // pq-formel --> Hpack2 = (-B +  √(B^2- 4AC))/ 2A
                    Hpack0 = (-B + Math.Sqrt(Math.Pow(B, 2) - 4 * A * C)) / (2 * A);

                }

                var P2_0_TillHpack0 = new Point3d(P_MUR[0].X, P_MUR[0].Y, P_MUR[0].Z - Hpack0);
                var P2_3_TillHpack0 = new Point3d(P_MUR[3].X, P_MUR[3].Y, P_MUR[3].Z - Hpack0);

                var P2_0_TillHpack1 = new Point3d(P_MUR[0].X, P_MUR[0].Y, P_MUR[0].Z - Hpack1);
                var P2_3_TillHpack1 = new Point3d(P_MUR[3].X, P_MUR[3].Y, P_MUR[3].Z - Hpack1);

                var P_Load_MUR_pack1 = new List<Point3d> { P2_0_TillHpack0, P2_0_TillHpack1, P2_3_TillHpack1, P2_3_TillHpack0 };
                var regionLoadMUR_pack1 = new Region(P_Load_MUR_pack1, Plane.YZ);

                var qJ_krit = K_SLS[0] * ( (QÖLP0 / (LP0 + Hpack1 * SpridLP)) +  qJ * Hpack1);

                var LoadJKH_pack1_0 = new LoadLocationValue(P2_0_TillHpack0, 0);
                var LoadJKH_pack1_1 = new LoadLocationValue(P2_0_TillHpack1, qpack - qJ_krit);
                var LoadJKH_pack1_2 = new LoadLocationValue(P2_3_TillHpack1, qpack - qJ_krit);
                var LoadJKH_pack1 = new List<LoadLocationValue> { LoadJKH_pack1_0, LoadJKH_pack1_1, LoadJKH_pack1_2 };

                SurfaceLoadJKH_pack1 = new FemDesign.Loads.SurfaceLoad(regionLoadMUR_pack1, LoadJKH_pack1, LoaddirJH, loadCaseJK, false, "pack1");

                var Hpack2 = 0.0;
                // Packning del 2
                // Förutsätter paritalfaktor 1.4 används för överlast

                // Höjd då Överlast är konstant
                if (SpridLP == 0)
                {
                    // Start ekvation: qpack = Kx ( yd 1.4 qÖLV + yd 1.1 qj Hpack2) 
                    Hpack2 = (qpack / K_SLS[0] -  (QÖLP0 / LP0)) / ( qJ);
                    //var Hpack3 = (qpack / K_SLS[0] - (QÖLP0 / LP0)) / (qJ[0]);
                }
                else // Höjd då Överlast minskar med murhöjd
                {
                    // Start ekvation: qpack = Kx ( yd 1.4 qÖLP0/ ( LP0 + Hpack2 tan(a) ) + yd 1.1 qj Hpack2)
                    double A =  qJ * SpridLP;                              // A = yd 1.1 qj SpridLP
                    double B =  qJ * LP0 - (qpack / K_SLS[0]) * SpridLP;   // B = yd 1.1 qj LP0 - (qpack / Kx) * SpridLP
                    double C =  QÖLP0 - (qpack / K_SLS[0]) * LP0;             // C = yd 1.4 QÖLP0 -  (qpack / Kx) *  LP0
                    // pq-formel --> Hpack2 = (-B +  √(B^2- 4AC))/ 2A
                    Hpack2 = (-B + Math.Sqrt(Math.Pow(B, 2) - 4 * A * C)) / (2 * A);
                }

                // Sida 0
                if (Hpack2 > H_Mur[0]) // packningstryck når != 0 innan full murhöjd 
                {
                    P2_0_TillHpack2 = P_MUR[1];
                    LoadJKH_pack2_1 = new LoadLocationValue(P2_0_TillHpack2, qpack - H_Mur[0] * qhJKH[0]);
                }
                else // (Hpack2 < H_Mur[0]) // packningstryck når 0 innan full murhöjd
                {
                    P2_0_TillHpack2 = new Point3d(P_MUR[0].X, P_MUR[0].Y, P_MUR[0].Z - Hpack2);
                    LoadJKH_pack2_1 = new LoadLocationValue(P2_0_TillHpack2, 0);
                }

                // Sida 1
                if (Hpack2 > H_Mur[1]) // packningstryck når != 0 innan full murhöjd 
                {
                    P2_3_TillHpack2 = P_MUR[2];
                    LoadJKH_pack2_2 = new LoadLocationValue(P2_3_TillHpack2, qpack - H_Mur[1] * qhJKH[0]);
                }
                else // (Hpack2 < H_Mur[1]) // packningstryck når 0 innan full murhöjd
                {
                    P2_3_TillHpack2 = new Point3d(P_MUR[3].X, P_MUR[3].Y, P_MUR[3].Z - Hpack2);
                    LoadJKH_pack2_2 = new LoadLocationValue(P2_3_TillHpack2, 0);
                }

                var P_Load_MUR_pack2 = new List<Point3d> { P2_0_TillHpack1, P2_0_TillHpack2, P2_3_TillHpack2, P2_3_TillHpack1 };
                var regionLoadMUR_pack2 = new Region(P_Load_MUR_pack2, Plane.YZ);

                var LoadJKH_pack2_0 = new LoadLocationValue(P2_0_TillHpack1, qpack - qJ_krit); // Last i 1 & 2 kan variera och bestämms i if sats ovan
                var LoadJKH_pack2 = new List<LoadLocationValue> { LoadJKH_pack2_0, LoadJKH_pack2_1, LoadJKH_pack2_2 };

                SurfaceLoadJKH_pack2 = new FemDesign.Loads.SurfaceLoad(regionLoadMUR_pack2, LoadJKH_pack2, LoaddirJH, loadCaseJK, false, "pack2");

            }

            // ---  Motfyllning ---
            //  V

            //var LoadJKMV_0 = new LoadLocationValue(P_BPL[0],    (JordH_framtass[0] - T_BPL[0].Value) * qJ[0]);
            //var LoadJKMV_1 = new LoadLocationValue(PFramtass_0, (JordH_framtass[1] - T_BPL[0].Value) * qJ[0]);
            //var LoadJKMV_2 = new LoadLocationValue(PFramtass_1, (JordH_framtass[2] - T_BPL[0].Value) * qJ[0]);
            //var LoadJMKV = new List<LoadLocationValue> { LoadJKMV_0, LoadJKMV_1, LoadJKMV_2 };

            var SurfaceLoadJMKV = new FemDesign.Loads.SurfaceLoad(regionFramtass, LoadDummyJM, LoaddirJV, loadCaseJK, false, "");

            if (JordH_framtass_min > T_BPL[0].Value)
            {
                SurfaceLoadJMKV = new FemDesign.Loads.SurfaceLoad(regionFramtass, LoadJMUV, LoaddirJV, loadCaseJK, false, "JMKV");
            }

            //  H

            var LoadJKMH_0 = new LoadLocationValue(P2_0_MfyllH, 0);
            var LoadJKMH_1 = new LoadLocationValue(P_MUR[1], (JordH_framtass[1]- T_BPL[0].Value) * qhJKH[1]);
            var LoadJKMH_2 = new LoadLocationValue(P_MUR[2], (JordH_framtass[2]- T_BPL[0].Value) * qhJKH[1]);
            var LoadJMKH = new List<LoadLocationValue> { LoadJKMH_0, LoadJKMH_1, LoadJKMH_2 };

            var SurfaceLoadJMKH = new FemDesign.Loads.SurfaceLoad(region_Load_Mfyll, LoadDummyJM, LoaddirJMH, loadCaseJK, false, "");

            if (JordH_framtass_min > T_BPL[0].Value)
            {
                SurfaceLoadJMKH = new FemDesign.Loads.SurfaceLoad(region_Load_Mfyll, LoadJMKH, LoaddirJMH, loadCaseJK, false, "JMKH");
            }

            var qJMKH_LL = new List<double> { (JordH_framtass[0] / 2) * qhJKH[1], (JordH_framtass[3] / 2) * qhJKH[1] };

            if (JordH_framtass_min > T_BPL[0].Value)
            {
                qJMKH_LL = new List<double> { (JordH_framtass[0] - T_BPL[0].Value / 2) * qhJKH[1], (JordH_framtass[3] - T_BPL[0].Value / 2) * qhJKH[1] };
            }

            var qJMKH_L0 = new Vector3d(qJMKH_LL[0] * T_BPL[0].Value, 0, 0);
            var qJMKH_L1 = new Vector3d(qJMKH_LL[1] * T_BPL[0].Value, 0, 0);
            var lineLoadJMKH = new FemDesign.Loads.LineLoad(LineBPL_MotH, qJMKH_L1, qJMKH_L0, loadCaseJK, ForceLoadType.Force, "LL_JMKH", true, false);

            var qJMKH_L2 = new Vector3d(0, qJMKH_L0.X * T_BPL[0].Value / 2, 0);
            var qJMKH_L3 = new Vector3d(0, qJMKH_L1.X * T_BPL[0].Value / 2, 0);
            var lineMomentJMKH = new FemDesign.Loads.LineLoad(LineBPL_MotH, qJMKH_L2, qJMKH_L3, loadCaseJK, ForceLoadType.Moment, "LM_JMKH", true, false);

            // --- 4. HHW ---

            if (HHW >= T_BPL_max)
            {
                //  V
                var HHWLoadV = new Vector3d(0, 0, -(HHW - T_BPL[0].Value) * qW);
                var SurfaceLoadHHW_V_baktass = new FemDesign.Loads.SurfaceLoad(regionBaktass, HHWLoadV, loadCaseHHW, false, "");
                var SurfaceLoadHHW_V_framtass = new FemDesign.Loads.SurfaceLoad(regionFramtass, HHWLoadV, loadCaseHHW, false, "");

                //  lift
                // BPL
                // var LiftLoadBPL = new Vector3d(0,0, VolumeBPL * qW/ AreaBPL);
                // var SurfaceLoadHHW_liftBPL = new FemDesign.Loads.SurfaceLoad(regionBPL, LiftLoadBPL, loadCaseHHW,false,"");

                // MUR
                double meanZMUR = (P_MUR[0].Z + P_MUR[3].Z) / 2;
                double AndelliftMUR = (HHW - T_BPL_max) / meanZMUR;
                //  double AreaMUR = FemDesign.Results.QuantityEstimationConcrete.Slab2.Area;
                //  double VolumeMUR = FemDesign.Results.QuantityEstimationConcrete.Slab2.Volume;
                //  var LiftLoadMUR = new Vector3d(0, 0, AndelliftMUR * VolumeMUR * qW/ AreaMUR);
                var regionMUR = SlabMUR.Region;
                //  var SurfaceLoadHHW_liftMUR = new FemDesign.Loads.SurfaceLoad(regionMUR, LiftLoadMUR, loadCaseHHW, false, "");

            }
            else if (HHW > 0)
            {
                //  lift BPL
                double meanZBPL = (T_BPL_min + T_BPL_max) / 2;
                double AndelliftBPL = HHW / meanZBPL;
                //   var LiftLoadBPL = new Vector3d(0, 0, AndelliftBPL*VolumeBPL * qW / AreaBPL);
                //   var SurfaceLoadHHW_liftBPL = new FemDesign.Loads.SurfaceLoad(regionBPL, LiftLoadBPL, loadCaseHHW, false, "");
            }

            // --- 5.0 MW ---


            // --- 6.0 LLW ---


            // --- 7. ÖL U ---
            //  V
            // Definition of qÖL in point P2_0, P_MUR[1] , P_MUR[2], P2_3 
            // qÖL= QÖL0/(L0+JordH*SpridL)
            var qÖLVU = new List<double> { QÖLU0 / (LU0 * BU0), QÖLU0 /(LU0+JordH_baktass[0]*SpridLU) ,
                QÖLU0 / (LU0 + JordH_baktass[1] * SpridLU), QÖLU0 / (LU0 * BU0) };

            var LoadÖLUV_0 = new LoadLocationValue(PBaktass_0, qÖLVU[1]);
            var LoadÖLUV_1 = new LoadLocationValue(P_BPL[1], qÖLVU[1]);
            var LoadÖLUV_2 = new LoadLocationValue(P_BPL[2], qÖLVU[2]);
            var LoadÖLUV = new List<LoadLocationValue> { LoadÖLUV_0, LoadÖLUV_1, LoadÖLUV_2 };

            var SurfaceLoadÖLVU = new FemDesign.Loads.SurfaceLoad(regionBaktass, LoadÖLUV, LoaddirJV, loadCaseÖLVU, false, "ÖLVU");

            //  H
            var qÖLHU = new List<double> { qÖLVU[0] * K_ULS[0], qÖLVU[1] * K_ULS[0], qÖLVU[2] * K_ULS[0] };

            var LoadÖLHU_0 = new LoadLocationValue(P2_0_TillH, qÖLHU[0]);
            var LoadÖLHU_1 = new LoadLocationValue(P_MUR[1], qÖLHU[1]);
            var LoadÖLHU_2 = new LoadLocationValue(P_MUR[2], qÖLHU[2]);
            var LoadÖLHU = new List<LoadLocationValue> { LoadÖLHU_0, LoadÖLHU_1, LoadÖLHU_2 };

            var SurfaceLoadÖLHU = new FemDesign.Loads.SurfaceLoad(regionLoadMUR, LoadÖLHU, LoaddirJH, loadCaseÖLHU, false, "ÖLHU");

            var qÖLUH_L0 = new Vector3d(-qÖLHU[2] * T_BPL[0].Value, 0, 0);
            var qÖLUH_L1 = new Vector3d(-qÖLHU[1] * T_BPL[0].Value, 0, 0);
            var lineLoadÖLUH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qÖLUH_L0, qÖLUH_L1, loadCaseÖLHU, ForceLoadType.Force, "LL_ÖLUH", true, false);

            var qÖLUH_L2 = new Vector3d(0, qÖLUH_L0.X * T_BPL[0].Value / 2, 0);
            var qÖLUH_L3 = new Vector3d(0, qÖLUH_L1.X * T_BPL[0].Value / 2, 0);
            var lineMomentÖLUH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qÖLUH_L2, qÖLUH_L3, loadCaseÖLHU, ForceLoadType.Moment, "LM_ÖLUH", true, false);
            // --- 8. ÖL P ---
            //  V
            var qÖLVP = new List<double> { QÖLP0 / (LP0 * BP0), QÖLP0 / (LP0 + JordH_baktass[0] * SpridLP),
                QÖLP0 / (LP0 + JordH_baktass[1] * SpridLP), QÖLP0 / (LP0 * BP0) };

            var LoadÖLPV_0 = new LoadLocationValue(PBaktass_0, qÖLVP[1]);
            var LoadÖLPV_1 = new LoadLocationValue(P_BPL[1], qÖLVP[1]);
            var LoadÖLPV_2 = new LoadLocationValue(P_BPL[2], qÖLVP[2]);
            var LoadÖLPV = new List<LoadLocationValue> { LoadÖLPV_0, LoadÖLPV_1, LoadÖLPV_2 };

            var SurfaceLoadÖLVP = new FemDesign.Loads.SurfaceLoad(regionBaktass, LoadÖLPV, LoaddirJV, loadCaseÖLVP, false, "ÖLVP");

            //  H
            var qÖLHP = new List<double> { qÖLVP[0] * K_SLS[0], qÖLVP[1] * K_SLS[0], qÖLVP[2] * K_SLS[0] };

            var LoadÖLHP_0 = new LoadLocationValue(P2_0_TillH, qÖLHP[0]);
            var LoadÖLHP_1 = new LoadLocationValue(P_MUR[1], qÖLHP[1]);
            var LoadÖLHP_2 = new LoadLocationValue(P_MUR[2], qÖLHP[2]);
            var LoadÖLHP = new List<LoadLocationValue> { LoadÖLHP_0, LoadÖLHP_1, LoadÖLHP_2 };

            var SurfaceLoadÖLHP = new FemDesign.Loads.SurfaceLoad(regionLoadMUR, LoadÖLHP, LoaddirJH, loadCaseÖLHP, false, "ÖLHP");

            var qÖLPH_L0 = new Vector3d(-qÖLHP[2] * T_BPL[0].Value, 0, 0);
            var qÖLPH_L1 = new Vector3d(-qÖLHP[1] * T_BPL[0].Value, 0, 0);
            var lineLoadÖLPH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qÖLPH_L0, qÖLPH_L1, loadCaseÖLHP, ForceLoadType.Force, "LL_ÖLPH", true, false);

            var qÖLPH_L2 = new Vector3d(0, qÖLPH_L0.X * T_BPL[0].Value / 2, 0);
            var qÖLPH_L3 = new Vector3d(0, qÖLPH_L1.X * T_BPL[0].Value / 2, 0);
            var lineMomentÖLPH = new FemDesign.Loads.LineLoad(LineBPL_TillH, qÖLPH_L2, qÖLPH_L3, loadCaseÖLHP, ForceLoadType.Moment, "LM_ÖLPH", true, false);

            // --- 9. Olycka ---
            var qAcc_L = new Vector3d(-qAcc, 0, 0);
            var lineLoadAcc = new FemDesign.Loads.LineLoad(LineR, qAcc_L, loadCaseAcc, ForceLoadType.Force, "Acc", true, false);

            var loads = new List<ILoadElement>
            {
              lineLoadR,
              SurfaceLoadJUV,
              SurfaceLoadJUH,
              lineLoadJUH,
              lineMomentJUH,
              SurfaceLoadJUH_pack1,
              SurfaceLoadJUH_pack2,
              SurfaceLoadJMUV,
              SurfaceLoadJMUH,
              lineLoadJMUH,
              lineMomentJMUH,
              SurfaceLoadJAV,
              SurfaceLoadJAH,
              lineLoadJAH,
              lineMomentJAH,
              SurfaceLoadJAH_pack1,
              SurfaceLoadJAH_pack2,
              SurfaceLoadJMAV,
              SurfaceLoadJMAH,
              lineLoadJMAH,
              lineMomentJMAH,
              SurfaceLoadJKV,
              SurfaceLoadJKH,
              lineLoadJKH,
              lineMomentJKH,
              SurfaceLoadJKH_pack1,
              SurfaceLoadJKH_pack2,
              SurfaceLoadJMKV,
              SurfaceLoadJMKH,
              lineLoadJMKH,
              lineMomentJMKH,
              SurfaceLoadÖLVU,
              SurfaceLoadÖLHU,
              lineLoadÖLUH,
              lineMomentÖLUH,
              SurfaceLoadÖLVP,
              SurfaceLoadÖLHP,
              lineLoadÖLPH,
              lineMomentÖLPH,
              lineLoadAcc,
            };

            return loads;
        }
    }
}
