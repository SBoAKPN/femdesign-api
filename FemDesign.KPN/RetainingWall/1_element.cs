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
        public static List<FemDesign.Shells.Slab> slabs(double MeshSize)
        {

            // ----- INDATA -----
            // Corner points of Slab1 BPL 
            var P1_0 = new Point3d(0.000, 0.000, 0.000);
            var P1_1 = new Point3d(4.000, 0.000, 0.000);
            var P1_2 = new Point3d(4.000, 1.000, 0.000);
            var P1_3 = new Point3d(0.000, 1.000, 0.000);

            var P_BPL = new List<Point3d> { P1_0, P1_1, P1_2 };
            var T_BPL = new List<double> { 1.000, 0.900, 0.900 };

            // Corner points of MUR 
            var P2_0 = new Point3d(1.000, 0.000, 6.000);
            var P2_1 = new Point3d(1.000, 0.000, T_BPL[0]);
            var P2_2 = new Point3d(1.000, 1.000, T_BPL[0]);
            var P2_3 = new Point3d(1.000, 1.000, 6.000);

            var P_MUR = new List<Point3d> { P2_0, P2_1, P2_2 };
            var T_MUR = new List<double> { 0.500, 1.000, 1.000 };

            //Calculated with https://concrete-creep.strusoft.com/
            //RH = 100, Ac=0.95, u=3.9, t0=5
            double creepUlsBPL  = 0.000;
            double creepSlqBPL  = 1.6435;
            double creepSlfBPL  = 1.6435;
            double creepSlcBPL  = 1.6435;
            double shrinkageBPL = 0.0625e-3;

            //RH = 80, Ac=0.75, u=3.5, t0=5
            double creepUlsMUR = 0.000;
            double creepSlqMUR = 2.0238;
            double creepSlfMUR = 2.0238;
            double creepSlcMUR = 2.0238;
            double shrinkageMUR = 0.24258e-3;

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
            Slab SlabBPLMUR = FemDesign.Shells.Slab.FromFourPoints(P3_0, P3_1, P3_2, P3_3, T_BPL[0], materialBPLNoMass, EdgeConnection.Default, null, null, "BPL-MUR");

            SlabBPL.UpdateThickness(P_BPL, T_BPL);
            SlabMUR.UpdateThickness(P_MUR, T_MUR);
            SlabBPL.SlabPart.MeshSize = MeshSize;
            SlabMUR.SlabPart.MeshSize = MeshSize;
            SlabBPLMUR.SlabPart.MeshSize = MeshSize;

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
            // KxNeg [0] , KxPos [1], KyNeg [2] , KyPos [3], KzNeg [4] , KzPos [5]
            // Neg = compression, Pos = Tension
            var KSupp = new List<double> { 5e3, 5e3, 5e3, 5e3, 10e3, 0 };

            // ------------------

            // Define support   
            var regionBPL = SlabBPL.Region;


            var motionsUpplagBPL = new FemDesign.Releases.Motions(KSupp[0], KSupp[1], KSupp[2], KSupp[3], KSupp[4], KSupp[5]);

            var supportBPL = new FemDesign.Supports.SurfaceSupport(regionBPL, motionsUpplagBPL, "BPL_Upplag");
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
