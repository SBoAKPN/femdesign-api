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

            double creepUlsBPL  = 0.1;
            double creepSlqBPL  = 0.1;
            double creepSlfBPL  = 0.1;
            double creepSlcBPL  = 0.1;
            double shrinkageBPL = 0.1;

            double creepUlsMUR = 0.1;
            double creepSlqMUR = 0.1;
            double creepSlfMUR = 0.1;
            double creepSlcMUR = 0.1;
            double shrinkageMUR = 0.1;

            // ------------------

            // Corner points of Slab1 BPL 
            var P1_0 = new Point3d(PP1_0[0], PP1_0[1], PP1_0[2]);
            var P1_1 = new Point3d(PP1_1[0], PP1_1[1], PP1_1[2]);
            var P1_2 = new Point3d(PP1_2[0], PP1_2[1], PP1_2[2]);
            var P1_3 = new Point3d(PP1_3[0], PP1_3[1], PP1_3[2]);
            // Points for thickness
            var PointsBPL = new List<Point3d> { P1_0, P1_1, P1_2 };
            var ThicknessBPL = new List<double> { PP1_0[3], PP1_1[3], PP1_2[3] };

            // Corner points of Slab2 MUR 
            var P2_0 = new Point3d(PP2_0[0], PP2_0[1], PP2_0[2]);
            var P2_1 = new Point3d(PP2_1[0], PP2_1[1], PP2_1[2]);
            var P2_2 = new Point3d(PP2_2[0], PP2_2[1], PP2_2[2]);
            var P2_3 = new Point3d(PP2_3[0], PP2_3[1], PP2_3[2]);
            // Points for thickness
            var PointsMUR = new List<Point3d> { P2_0, P2_1, P2_2 };
            var ThicknessMUR = new List<double> { PP2_0[3], PP2_1[3], PP2_2[3] };

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
            Slab SlabBPL    = FemDesign.Shells.Slab.FromFourPoints(P1_0, P1_1, P1_2, P1_3, ThicknessBPL[0], materialBPL,       EdgeConnection.Default, SlabAlignBottom, null, "BPL");
            Slab SlabMUR    = FemDesign.Shells.Slab.FromFourPoints(P2_0, P2_1, P2_2, P2_3, ThicknessMUR[0], materialMUR,       EdgeConnection.Default, null, null, "MUR");
            Slab SlabBPLMUR = FemDesign.Shells.Slab.FromFourPoints(P3_0, P3_1, P3_2, P3_3, ThicknessBPL[0], materialBPLNoMass, EdgeConnection.Default, null, null, "BPL-MUR");

            SlabBPL.UpdateThickness(PointsBPL, ThicknessBPL);
            SlabMUR.UpdateThickness(PointsMUR, ThicknessMUR);
            SlabBPL.SlabPart.MeshSize = MeshSize;
            SlabMUR.SlabPart.MeshSize = MeshSize;

            var slabs = new List<Slab> { SlabBPL, SlabMUR };
            return slabs;

        }

        public static List<FemDesign.GenericClasses.IStructureElement> supports(List<Slab> slabs)
        {
            // Definition from input data
            Slab SlabBPL = slabs[0];

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
            var supports = new List<FemDesign.GenericClasses.IStructureElement> { supportBPL };
            return supports;
        }

        public static List<FemDesign.GenericClasses.IStructureElement> elements(List<Slab> slabs, List<IStructureElement> supports )
        {
            Slab SlabBPL = slabs[0];
            Slab SlabMUR = slabs[1];
            IStructureElement supportBPL = supports[0];

            var elements = new List<FemDesign.GenericClasses.IStructureElement> { SlabBPL, SlabMUR, supportBPL };
            return elements;
        }

    }
}
