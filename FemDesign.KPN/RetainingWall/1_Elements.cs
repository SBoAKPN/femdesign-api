using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FemDesign;
using FemDesign.Geometry;
using FemDesign.Materials;
using FemDesign.Shells;
using FemDesign.GenericClasses;

namespace RetainingWall
{
    internal class Elements
    {
        public List<FemDesign.GenericClasses.IStructureElement> Element() 
        {
            // ----- INDATA ELEMENTS -----

            //// Corner points of BPL 
            var P1_0 = new Point3d(0, 0, 0);
            var P1_1 = new Point3d(4.0, 0, 0);
            var P1_2 = new Point3d(4.0, 1.0, 0);
            var P1_3 = new Point3d(0, 1.0, 0);
            //// Points for thickness
            var PointsBPL = new List<Point3d> { P1_0, P1_1, P1_2, P1_3 };
            var SlabThicknessBPL = new List<double> { 1.0, 0.9, 0.9, 1.0 };

            //// Corner points of Mur 
            var P2_0 = new Point3d(1.0, 0.0, 6.0);
            var P2_1 = new Point3d(1.0, 0.0, SlabThicknessBPL[0]);
            var P2_2 = new Point3d(1.0, 1.0, SlabThicknessBPL[3]);
            var P2_3 = new Point3d(1.0, 1.0, 6.0);
            //// Points for thickness
            var PointsMur = new List<Point3d> { P2_0, P2_1, P2_2, P2_3 };
            var SlabThicknessMur = new List<double> { 0.5, 1.0, 1.0, 0.5 };

            // Styvhet för upplag
            // KxNeg [0] , KxPos [1], KyNeg [2] , KyPos [3], KzNeg [4] , KzPos [5]
            // Neg = compression, Pos = Tension
            var KSupp = new List<double> { 5e3, 5e3, 5e3, 5e3, 10e3, 0 };

            double creepUls = 0.5;
            double creepSlq = 0.5;
            double creepSlf = 0.5;
            double creepSlc = 0.5;
            double shrinkage = 0.5;

            //// ------------------

            // Corner points of BPLMur  
            var P3_0 = new Point3d(P2_1.X, P2_1.Y, P2_1.Z);
            var P3_1 = new Point3d(P2_1.X, P2_1.Y, P1_1.Z);
            var P3_2 = new Point3d(P2_2.X, P2_2.Y, P1_2.Z);
            var P3_3 = new Point3d(P2_2.X, P2_2.Y, P2_2.Z);

            //// Points for thickness
            var PointsBPLMur = new List<Point3d> { P3_0, P3_1, P3_2, P3_3 };
            var SlabThicknessBPLMur = new List<double> { SlabThicknessBPL[0], SlabThicknessBPL[0], SlabThicknessBPL[3], SlabThicknessBPL[3] };

            //Define properties
            var materialDatabase = FemDesign.Materials.MaterialDatabase.GetDefault();
            Material materialBtg = materialDatabase.MaterialByName("C35/45");

            Material materialBtgNoMass = materialBtg.DeepClone();
            materialBtgNoMass.Concrete.Mass = 0;
            materialBtgNoMass.Name = "C35/45NoMass";
            materialBtgNoMass.Guid = System.Guid.NewGuid();

            materialBtg = FemDesign.Materials.Material.ConcreteMaterialProperties(materialBtg, creepUls, creepSlq, creepSlf, creepSlc, shrinkage);

            //// Define elements
            var SlabAlignBottom = new ShellEccentricity(VerticalAlignment.Bottom, 0, false, false);
            var SlabBPL = FemDesign.Shells.Slab.FromFourPoints(PointsBPL[0], PointsBPL[1], PointsBPL[2], PointsBPL[3], SlabThicknessBPL[0], materialBtg, EdgeConnection.Default, SlabAlignBottom, null, "BPL");
            var SlabMur = FemDesign.Shells.Slab.FromFourPoints(PointsMur[0], PointsMur[1], PointsMur[2], PointsMur[3], SlabThicknessMur[0], materialBtg, EdgeConnection.Default, null, null, "Mur");
            var SlabBPLMur = FemDesign.Shells.Slab.FromFourPoints(PointsBPLMur[0], PointsBPLMur[1], PointsBPLMur[2], PointsBPLMur[3], SlabThicknessBPLMur[0], materialBtgNoMass, EdgeConnection.Default, null, null, "BPL-Mur");

            SlabBPL.UpdateThickness(PointsBPL, SlabThicknessBPL);
            SlabMur.UpdateThickness(PointsMur, SlabThicknessMur);

            // Define support   
            var regionBPL = SlabBPL.Region;

            var motionsUpplagBPL = new FemDesign.Releases.Motions(KSupp[0], KSupp[1], KSupp[2], KSupp[3], KSupp[4], KSupp[5]);

            var SurfaceSupport1 = new FemDesign.Supports.SurfaceSupport(regionBPL, motionsUpplagBPL, "BPL_Upplag");
 
            return new List<FemDesign.GenericClasses.IStructureElement> { SlabBPL, SlabMur, SlabBPLMur, SurfaceSupport1 };
        }
    }

        
}
