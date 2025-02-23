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
        public static List<FemDesign.GenericClasses.IStructureElement> labelledsections(List<Slab> slabs)
        {
            // Predefinition from input data
            // slabs
            Slab SlabBPL = slabs[0];
            Slab SlabMUR = slabs[1];

            var P_BPL = SlabBPL.SlabPart.Region.Contours[0].Points;
            var P_MUR = SlabMUR.SlabPart.Region.Contours[0].Points;

            // ----- INDATA -----
            var P1_Utj0 = new Point3d(P_MUR[1].X, P_MUR[1].Y + 1, P_MUR[1].Z);
            var P3_Utj0 = new Point3d(P_MUR[0].X, P_MUR[0].Y + 1, P_MUR[0].Z);
            var Mur_Points0 = new List<Point3d>() { P_MUR[1], P1_Utj0, P_MUR[0], P3_Utj0 };

            var P1_Utj1 = new Point3d(P_MUR[2].X, P_MUR[2].Y - 1, P_MUR[2].Z);
            var P3_Utj1 = new Point3d(P_MUR[3].X, P_MUR[3].Y - 1, P_MUR[3].Z);
            var Mur_Points1 = new List<Point3d>() { P_MUR[2], P1_Utj1, P_MUR[3], P3_Utj1 };


            var LS_MUR_Points0 = PointInterpolate(Mur_Points0, 8);
            var LS_MUR_Points1 = PointInterpolate(Mur_Points1, 8);
            // ------------------

            var labelledsections = new List<FemDesign.GenericClasses.IStructureElement>();
            for (int i = 0; i < LS_MUR_Points0.Count; i+= 2)
            {
                List<Point3d> labelledsection_i0 = new List<Point3d> { LS_MUR_Points0[i], LS_MUR_Points0[i+1] };
                labelledsections.Add(new FemDesign.AuxiliaryResults.LabelledSection(labelledsection_i0, "LS"));

                List<Point3d> labelledsection_i1 = new List<Point3d> { LS_MUR_Points1[i], LS_MUR_Points1[i + 1] };
                labelledsections.Add(new FemDesign.AuxiliaryResults.LabelledSection(labelledsection_i1, "LS"));
            }

            return labelledsections;
        }
        public static List<FemDesign.GenericClasses.IStructureElement> resultpoints_Model(List<Slab> slabs)
        {
            // Predefinition from input data
            // slabs
            Slab SlabBPL = slabs[0];
            Slab SlabMUR = slabs[1];

            var P_BPL = SlabBPL.SlabPart.Region.Contours[0].Points;
            var P_MUR = SlabMUR.SlabPart.Region.Contours[0].Points;

            // ----- INDATA -----

            var Mur_Points0 = new List<Point3d>() { P_MUR[1], P_MUR[0] };
            var Mur_Points1 = new List<Point3d>() { P_MUR[2], P_MUR[3] };
            var RP_MUR_IP0= PointInterpolate(Mur_Points0, 4);
            var RP_MUR_IP1 = PointInterpolate(Mur_Points1, 4);

            List<Point3d> RP_MUR_Points = new List<Point3d>();
            RP_MUR_Points.AddRange(RP_MUR_IP0);
            RP_MUR_Points.AddRange(RP_MUR_IP1);
            // ------------------

            var rp_model = new List<FemDesign.GenericClasses.IStructureElement>();
            for (int i = 0; i < RP_MUR_Points.Count ; i++)
            {
                rp_model.Add(new ResultPoint(RP_MUR_Points[i], SlabMUR, "RPM"));
            }

            return rp_model;
        }

        public static List<FemDesign.Calculate.CmdResultPoint> resultpoints_Result(List<Slab> slabs)
        {
            // Predefinition from input data
            // slabs
            Slab SlabBPL = slabs[0];
            Slab SlabMUR = slabs[1];

            var P_BPL = SlabBPL.SlabPart.Region.Contours[0].Points;
            var P_MUR = SlabMUR.SlabPart.Region.Contours[0].Points;

            // ----- INDATA -----

            var Mur_Points0 = new List<Point3d>() { P_MUR[1], P_MUR[0] };
            var Mur_Points1 = new List<Point3d>() { P_MUR[2], P_MUR[3] };
            var RP_MUR_IP0 = PointInterpolate(Mur_Points0, 4);
            var RP_MUR_IP1 = PointInterpolate(Mur_Points1, 4);

            List<Point3d> RP_MUR_Points = new List<Point3d>();
            RP_MUR_Points.AddRange(RP_MUR_IP0);
            RP_MUR_Points.AddRange(RP_MUR_IP1);
            // ------------------

            var rp_result = new List<FemDesign.Calculate.CmdResultPoint>();
            for (int i = 0; i < RP_MUR_Points.Count; i++)
            {
                rp_result.Add(new FemDesign.Calculate.CmdResultPoint(RP_MUR_Points[i], (FemDesign.GenericClasses.IStructureElement)SlabMUR, "@RPR" + (i+1)));
            }


            return rp_result;
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

        public static List<double> deformationsX(List<Slab> slabs, List<FemNode> feaNodes, List<ShellDisplacement> disp, LoadCombination loadcombination)
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

        public static List<double> normDeformations_Var(List<Slab> slabs, List<FemNode> feaNodes, List<ShellDisplacement> disp, List<LoadCombination> loadcombinations)
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
        public static List<double> normDeformations_Sq(List<Slab> slabs, List<FemNode> feaNodes, List<ShellDisplacement> disp, List<LoadCombination> loadcombinations)
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

        // ------------------ FIXA START ----------------------------------------


        public static List<List<double>> deformationsX(List<Slab> slabs, List<FemDesign.Results.ShellDisplacement> dispAtPoints, LoadCombination loadcombination)
        {
            // Predefinition from input data
            // slabs
            Slab SlabBPL = slabs[0];
            Slab SlabMUR = slabs[1];

            var P_BPL = SlabBPL.SlabPart.Region.Contours[0].Points;
            var P_MUR = SlabMUR.SlabPart.Region.Contours[0].Points;

            var Name_LComb = loadcombination.Name;

            // ----- INDATA -----
            int PointsperSnitt = 5;
            int AntalSnitt = 2;
            String PointName_start = "MUR.2.1.@RPR";
            String PointName_slut = ".0";
            // ------------------

            List<List<double>> Disp_table = new List<List<double>>();
            for (int i = 0; i < AntalSnitt; i++)
            {
                List<double> Disp_row = new List<double>();
                for (int j = 0; j < PointsperSnitt; j++)
                {
                    int punktnr = i * PointsperSnitt + j + 1;
                    string PointId = PointName_start + punktnr + PointName_slut;

                    double ezValue = dispAtPoints
                    .Where(r => r.CaseIdentifier == Name_LComb)
                    .Where(x => x.Id == PointId)
                    .FirstOrDefault()?.Ez ?? 0;

                    Disp_row.Add(ezValue);
                }
                Disp_table.Add(Disp_row);
            }

            return Disp_table;
        }

        public static List<List<double>> normDeformations_Var(List<Slab> slabs, List<FemDesign.Results.ShellDisplacement> dispAtPoints, List<LoadCombination> loadcombinations)
        {
            // Predefinition from input data
            var loadcombination_Sf = loadcombinations[0];
            var loadcombination_Sq = loadcombinations[1];

            // ----- INDATA -----
            var Deformation_Sf = Resultprocessing.deformationsX(slabs, dispAtPoints, loadcombination_Sf);
            var Deformation_Sq = Resultprocessing.deformationsX(slabs, dispAtPoints, loadcombination_Sq);

            // Normering efter deformation i X-led vid inspänningssnitt
            var NomDeformation_Sf_table  = new List<List<double>>();
            var NomDeformation_Sq_table = new List<List<double>>();
            var NomDeformation_Var_table = new List<List<double>>();

            int PointsperSnitt = Deformation_Sf[0].Count;
            int AntalSnitt = Deformation_Sf.Count;

            for (int i = 0; i < AntalSnitt; i++)
            {
                List<double> NomDeformation_Sf_row = new List<double>();
                List<double> NomDeformation_Sq_row = new List<double>();
                List<double> NomDeformation_Var_row = new List<double>();

                for (int j = 0; j < PointsperSnitt; j++)
                {
                    NomDeformation_Sf_row.Add(Deformation_Sf[i][j] - Deformation_Sf[i][0]);
                    NomDeformation_Sq_row.Add(Deformation_Sq[i][j] - Deformation_Sq[i][0]);
                    NomDeformation_Var_row.Add(Deformation_Sf[i][j] - Deformation_Sq[i][j]);
                }
                NomDeformation_Var_table.Add(NomDeformation_Var_row);
            }
            // ------------------

            return NomDeformation_Var_table;
        }
        public static List<List<double>> normDeformations_Sq(List<Slab> slabs, List<FemDesign.Results.ShellDisplacement> dispAtPoints, List<LoadCombination> loadcombinations)
        {
            // Predefinition from input data
            var loadcombination_Sq = loadcombinations[1];

            // ----- INDATA -----
            var Deformation_Sq = Resultprocessing.deformationsX(slabs, dispAtPoints, loadcombination_Sq);

            // Normering efter deformation i X-led vid inspänningssnitt
            var NomDeformation_Sq_table = new List<List<double>>();

            int PointsperSnitt = Deformation_Sq[0].Count;
            int AntalSnitt = Deformation_Sq.Count;

            for (int i = 0; i < AntalSnitt; i++)
            {
                List<double> NomDeformation_Sq_row = new List<double>();

                for (int j = 0; j < PointsperSnitt; j++)
                {
                    NomDeformation_Sq_row.Add(Deformation_Sq[i][j] - Deformation_Sq[i][0]);
                }
                NomDeformation_Sq_table.Add(NomDeformation_Sq_row);
            }
            // ------------------

            return NomDeformation_Sq_table;
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

