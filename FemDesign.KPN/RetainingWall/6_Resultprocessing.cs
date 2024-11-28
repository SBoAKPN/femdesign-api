using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FemDesign.Geometry;
using FemDesign.Shells;

namespace RetainingWall
{
    internal class Resultprocessing
    {
        public static List<dynamic> labelledsections(List<Slab> slabs)
        {
            // Predefinition from input data
            // slabs
            Slab SlabBPL = slabs[0];
            Slab SlabMUR = slabs[1];

            var P_BPL = SlabBPL.SlabPart.Region.Contours[0].Points;
            var P_MUR = SlabMUR.SlabPart.Region.Contours[0].Points;

            // ----- INDATA -----

            var MUR_Poits = new List<Point3d>() { P_BPL[1], P_BPL[2], P_BPL[0], P_BPL[3] };

            var LS_MUR_Poits = LabelledSectionInterpolate(MUR_Poits, 8);

            var LS_MUR = new FemDesign.AuxiliaryResults.LabelledSection(LS_MUR_Poits, "LS");

            // ------------------

            var labelledsections = new List<dynamic> { LS_MUR };

            return labelledsections;
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
                    PointlistInterpolate.Add(new Point3d(Pointlist[0].X + Step0[0] * i, Pointlist[0].Y + Step0[1] * i, Pointlist[0].Z + Step0[2] * i));
                    PointlistInterpolate.Add(new Point3d(Pointlist[1].X + Step0[1] * i, Pointlist[1].Y + Step1[1] * i, Pointlist[1].Z + Step1[2] * i));

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
