using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FemDesign;
using FemDesign.Geometry;
using FemDesign.Materials;
using FemDesign.GenericClasses;
using FemDesign.Shells;
using FemDesign.Supports;

namespace RetainingWall
{
    internal class Element
    {
        public static List<FemDesign.Shells.Slab> slabs(double bx_BPL, double MeshSize)
        {

            // ----- INDATA -----
            // Corner points of Slab1 BPL 
            var P1_0 = new Point3d(0.000, 0.000, 0.000);
            var P1_1 = new Point3d(bx_BPL, 0.000, 0.000);
            var P1_2 = new Point3d(bx_BPL, 10.000, 0.000);
            var P1_3 = new Point3d(0.000, 10.000, 0.000);

            var P_BPL = new List<Point3d> { P1_0, P1_1, P1_2 };
            var T_BPL = new List<double> { 0.400, 0.400, 0.400 };

            // Corner points of MUR 
            var P2_0 = new Point3d(0.650, 0.000, 4.000);
            var P2_1 = new Point3d(0.650, 0.000, T_BPL[0]);
            var P2_2 = new Point3d(0.650, 10.000, T_BPL[0]);
            var P2_3 = new Point3d(0.650, 10.000, 4.000);

            var P_MUR = new List<Point3d> { P2_0, P2_1, P2_2 };
            var T_MUR = new List<double> { 0.300, 0.300, 0.300 };

            //Calculated with https://concrete-creep.strusoft.com/
            //RH = 100, Ac=0.40, u=2.8, t0=5
            double creepUlsBPL  = 0.000;
            double creepSlqBPL  = 1.6435;
            double creepSlfBPL  = 1.6435;
            double creepSlcBPL  = 1.6435;
            double shrinkageBPL = 0.0625e-3;

            //RH = 80, Ac=0.30, u=2.6, t0=5
            double creepUlsMUR = 0.000;
            double creepSlqMUR = 2.1179;
            double creepSlfMUR = 2.1179;
            double creepSlcMUR = 2.1179;
            double shrinkageMUR = 0.26921e-3;

            // ------------------

            // Corner points of BPL-MUR

            // Corner points of Slab3 (Point) 
            var P3_0 = new Point3d(P2_1.X, P2_1.Y, P2_1.Z);
            var P3_1 = new Point3d(P2_1.X, P2_1.Y, P1_0.Z);
            var P3_2 = new Point3d(P2_2.X, P2_2.Y, P1_3.Z);
            var P3_3 = new Point3d(P2_2.X, P2_2.Y, P2_2.Z);

            //Define properties
            var materialDatabase = FemDesign.Materials.MaterialDatabase.GetDefault();
            Material materialBPL = materialDatabase.MaterialByName("C35/45");
            Material materialMUR = materialDatabase.MaterialByName("C35/45");

            Material materialBPLNoMass = materialBPL.DeepClone();
            materialBPLNoMass.Concrete.Mass = 0;
            materialBPLNoMass.Name = "C35/45NoMass";
            materialBPLNoMass.Guid = System.Guid.NewGuid();

            materialBPL = FemDesign.Materials.Material.ConcreteMaterialProperties(materialBPL, creepUlsBPL, creepSlqBPL, creepSlfBPL, creepSlcBPL, shrinkageBPL);
            materialMUR = FemDesign.Materials.Material.ConcreteMaterialProperties(materialMUR, creepUlsMUR, creepSlqMUR, creepSlfMUR, creepSlcMUR, shrinkageMUR);

            // Define elements
            var SlabAlignBottom = new ShellEccentricity(VerticalAlignment.Bottom, 0, false, false);
            Slab SlabBPL    = FemDesign.Shells.Slab.FromFourPoints(P1_0, P1_1, P1_2, P1_3, T_BPL[0], materialBPL,       EdgeConnection.Default, SlabAlignBottom, null, "BPL");
            Slab SlabMUR    = FemDesign.Shells.Slab.FromFourPoints(P2_0, P2_1, P2_2, P2_3, T_MUR[0], materialMUR,       EdgeConnection.Default, null, null, "MUR");
            Slab SlabBPLMUR = FemDesign.Shells.Slab.FromFourPoints(P3_0, P3_1, P3_2, P3_3, T_BPL[0], materialBPLNoMass, EdgeConnection.Default, null, null, "BPLMUR");

            SlabBPL.UpdateThickness(P_BPL, T_BPL);
            SlabMUR.UpdateThickness(P_MUR, T_MUR);
            SlabBPL.SlabPart.MeshSize = MeshSize;
            SlabMUR.SlabPart.MeshSize = MeshSize;
            SlabBPLMUR.SlabPart.MeshSize = MeshSize;

            // Set static Guid 
            SlabBPL.SlabPart.Guid = new Guid("A951B736-3394-4FE4-B38F-D17F1161EB3A");
            SlabMUR.SlabPart.Guid = new Guid("F42D2623-FB17-4F39-9A21-EDDA24944298");
            SlabBPLMUR.SlabPart.Guid = new Guid("2E290437-E86A-4E36-AF7D-ACDE6A6146C8");

            //SlabBPL.SlabPart.LocalPos();
            var slabs = new List<Slab> { SlabBPL, SlabMUR, SlabBPLMUR };
            return slabs;

        }

        public static List<FemDesign.GenericClasses.IStructureElement> supports(List<Slab> slabs)
        {
            // Predefinition from input data
            Slab SlabBPL = slabs[0];
            
            var Conerpoints = SlabBPL.SlabPart.Region.Contours[0].Points;
            double AvrageX = new List<double> { Conerpoints[0].X, Conerpoints[1].X, Conerpoints[2].X, Conerpoints[3].X }.Average();
            double AvrageY = new List<double> { Conerpoints[0].Y, Conerpoints[1].Y, Conerpoints[2].Y, Conerpoints[3].Y }.Average();
            double AvrageZ = new List<double> { Conerpoints[0].Z, Conerpoints[1].Z, Conerpoints[2].Z, Conerpoints[3].Z }.Average();

            // ----- INDATA -----
            // Styvhet för upplag
            // KxNeg [0] = KSupp_H , KxPos [1] = KSupp_H, KyNeg [2] = KSupp_H , KyPos [3] = KSupp_H, KzNeg [4] = KSupp_V_Neg , KzPos [5] = KSupp_V_Pos
            // Neg = compression, Pos = Tension
            var KSupp_V_Neg = 10e3;
            var KSupp_V_Pos = 0;
            var Phi_d = Math.Atan(Math.Tan(37.0 * Math.PI/180)/1.3*1.05);
            var KSupp_H = KSupp_V_Neg * Math.Tan(Phi_d);
            
            // ------------------


            // Define support   
            var regionBPL = SlabBPL.Region;

            var motionsUpplagBPL = new FemDesign.Releases.Motions(KSupp_H, KSupp_H, KSupp_H, KSupp_H, KSupp_V_Neg, KSupp_V_Pos);

            var supportBPL = new FemDesign.Supports.SurfaceSupport(regionBPL, motionsUpplagBPL, "BMBPL");
            supportBPL.Plane.Origin = new Point3d(AvrageX, AvrageY, AvrageZ);

            var supports = new List<FemDesign.GenericClasses.IStructureElement> { supportBPL };
            return supports;
        }

        public static List<FemDesign.GenericClasses.IStructureElement> elements(List<Slab> slabs, List<IStructureElement> supports )
        {
            // Predefinition from input data
            Slab SlabBPL = slabs[0];
            Slab SlabMUR = slabs[1];
            Slab SlabBPLMUR = slabs[2];
            IStructureElement supportBPL = supports[0];

            var elements = new List<FemDesign.GenericClasses.IStructureElement> { SlabBPL, SlabMUR, SlabBPLMUR, supportBPL };
            return elements;
        }

    }
}
