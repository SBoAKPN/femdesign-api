using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FemDesign;
using FemDesign.Geometry;
using FemDesign.Results;
using FemDesign.Shells;
using FemDesign.Loads;
using static System.Reflection.Metadata.BlobBuilder;

using FemDesign.Geometry;
using FemDesign.Shells;
using FemDesign.AuxiliaryResults;

namespace RetainingWall
{
    internal class Resultprocessing
    {
        public static List<INamedEntity> labelledsections(List<Slab> slabs)
        {
            // Predefinition from input data
            // slabs
            Slab SlabBPL = slabs[0];
            Slab SlabMUR = slabs[1];

            var P_BPL = SlabBPL.SlabPart.Region.Contours[0].Points;
            var P_MUR = SlabMUR.SlabPart.Region.Contours[0].Points;

            // ----- INDATA -----

            var Mur_Points = new List<Point3d>() { P_MUR[1], P_MUR[2], P_MUR[0], P_MUR[3] };

            var LS_MUR_Points = PointInterpolate(Mur_Points, 1);

            var LS_MUR = new FemDesign.AuxiliaryResults.LabelledSection(LS_MUR_Points, "LS_MUR");

            // ------------------

            //var test3 = new ResultPoint(LS_MUR_Points[0], SlabMUR,"PT1");
            var labelledsections = new List<INamedEntity> { LS_MUR };

            return labelledsections;
        }
        public static List<INamedEntity> resultpoints(List<Slab> slabs)
        {
            // Predefinition from input data
            // slabs
            Slab SlabBPL = slabs[0];
            Slab SlabMUR = slabs[1];

            var P_BPL = SlabBPL.SlabPart.Region.Contours[0].Points;
            var P_MUR = SlabMUR.SlabPart.Region.Contours[0].Points;

            // ----- INDATA -----

            var Mur_Points = new List<Point3d>() { P_MUR[1], P_MUR[2], P_MUR[0], P_MUR[3] };

            var RP_MUR_Points = PointInterpolate(Mur_Points, 4);
            // ------------------
            //FemDesign.GenericClasses.IStageElement
            var resultpoints = new List<INamedEntity>();
            for (int i = 0; i < RP_MUR_Points.Count ; i++)
            {
                int j = i + 1;
                resultpoints.Add(new ResultPoint(RP_MUR_Points[i], SlabMUR, $"PT{j}"));
            }

            return resultpoints;
        }
            public static List<double> deformationsX(List<Slab> slabs, List<FemNode> feaNodes, List<NodalDisplacement> disp, LoadCombination loadcombination)
        {
            // Predefinition from input data
            // slabs
            Slab SlabBPL = slabs[0];
            Slab SlabMUR = slabs[1];

            var P_BPL = SlabBPL.SlabPart.Region.Contours[0].Points;
            var P_MUR = SlabMUR.SlabPart.Region.Contours[0].Points;

            var Name_LComb = loadcombination.Name;

            // ----- INDATA -----
            var Mur_Points_01 = new List<Point3d> { P_MUR[1], P_MUR[0] };

            var MUR_Poits = PointInterpolate(Mur_Points_01, 1);
            // ------------------

            // Find node number and adding it a list
            var NodeIds = new List<int>();
            foreach (Point3d point in MUR_Poits)
            {
                var NodeId = feaNodes.Where(n => (new Point3d(n.X, n.Y, n.Z) - point).Length() < 0.001).FirstOrDefault().NodeId;
                NodeIds.Add(NodeId);
            }

            // Nodal displacements
            var DispAtNodeIds = new List<double>();

            foreach (int Id in NodeIds)
            {
                var DispAtNodeId = disp.Where(r => r.CaseIdentifier == Name_LComb).Where(x => x.NodeId == Id).FirstOrDefault().Ex;
                DispAtNodeIds.Add(DispAtNodeId);
            }
            return DispAtNodeIds;
        }

        public static List<double> normDeformations_Var(List<Slab> slabs, List<FemNode> feaNodes, List<NodalDisplacement> disp, List<LoadCombination> loadcombinations)
        {
            // Predefinition from input data
            var loadcombination_Sf = loadcombinations[4];
            var loadcombination_Sq = loadcombinations[2];

            // ----- INDATA -----
            var Deformation_Sf = Resultprocessing.deformationsX(slabs, feaNodes, disp, loadcombination_Sf);
            var Deformation_Sq = Resultprocessing.deformationsX(slabs, feaNodes, disp, loadcombination_Sq);

            // Normering efter deformation i X-led vid inspänningssnitt
            var NomDeformation_Sf = new List<double>();
            var NomDeformation_Sq = new List<double>();
            var NomDeformation_Var = new List<double>();

            for (int i = 0; i < Deformation_Sq.Count; i++)
            {
                NomDeformation_Sf.Add(Deformation_Sf[i] - Deformation_Sf[0]);
                NomDeformation_Sq.Add(Deformation_Sq[i] - Deformation_Sq[0]);
            }

            for (int i = 0; i < Deformation_Sf.Count; i++)
            {
                NomDeformation_Var.Add(NomDeformation_Sf[i] - NomDeformation_Sq[i]);
            }
            // ------------------

            return NomDeformation_Var;
        }
        public static List<double> normDeformations_Sq(List<Slab> slabs, List<FemNode> feaNodes, List<NodalDisplacement>  disp, List<LoadCombination> loadcombinations)
        {
            // Predefinition from input data
            var loadcombination_Sq = loadcombinations[2];
            var loadcombination_Sq_ÖLP = loadcombinations[2];

            // ----- INDATA -----
            var Deformation_Sq = Resultprocessing.deformationsX(slabs, feaNodes, disp, loadcombination_Sq);
            var Deformation_Sq_ÖLP = Resultprocessing.deformationsX(slabs, feaNodes, disp, loadcombination_Sq_ÖLP);

            // Normering efter deformation i X-led vid inspänningssnitt
            var NomDeformation_Sq = new List<double>();
            var NomDeformation_Sq_ÖLP = new List<double>();

            for (int i = 0; i < Deformation_Sq.Count; i++)
            {
                NomDeformation_Sq.Add(Deformation_Sq[i] - Deformation_Sq[0]);
                NomDeformation_Sq_ÖLP.Add(Deformation_Sq_ÖLP[i] - Deformation_Sq_ÖLP[0]);
            }
            // ------------------

            return NomDeformation_Sq;
            //return NomDeformation_Sq_ÖLP;
        }

        public static List<Point3d> PointInterpolate(List<Point3d> Pointlist, int InterpolatePoints)
        {
            // Create a list of linear interpolate values from a list with start/endpoints with given interpolate points

            List<Point3d> PointlistInterpolate = new List<Point3d>();

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
                for (int i = 0; i <= InterpolatePoints; i++)
                {
                    PointlistInterpolate.Add(new Point3d(Pointlist[0].X + Step0[0] * i, Pointlist[0].Y + Step0[1] * i, Pointlist[0].Z + Step0[2] * i));
                    PointlistInterpolate.Add(new Point3d(Pointlist[1].X + Step0[1] * i, Pointlist[1].Y + Step1[1] * i, Pointlist[1].Z + Step1[2] * i));

                }


            }

            else if (Pointlist.Count == 2 && InterpolatePoints > 0)
            {
                //Step size
                Double[] Step =
                {
                    (Pointlist[1].X - Pointlist[0].X)/ InterpolatePoints,
                    (Pointlist[1].Y - Pointlist[0].Y) / InterpolatePoints,
                    (Pointlist[1].Z - Pointlist[0].Z) / InterpolatePoints
                };

                //Adding interpolate points
                for (int i = 0; i <= InterpolatePoints; i++)
                {
                    PointlistInterpolate.Add(new Point3d(Pointlist[0].X + Step[0] * i, Pointlist[0].Y + Step[1] * i, Pointlist[0].Z + Step[2] * i));

                }
            }

            else
            {
                throw new Exception("Pointlist should contain 2 or 4 points and InterpolatePoints > 0 ");
            }

            return PointlistInterpolate;

        }
    }
}

