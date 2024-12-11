using FemDesign.Shells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetainingWall
{
    internal class FundationDesign
    {
        public static List<double> ABE(Loadcombination loadcombination, Slab SlabBPL, Boolean KontrollUtskrift)
        {
            // ----- INDATA -----
            // Dränerade egenskaper
            double γM_dr  = 1.3;
            double ηdr_18 = 1.00;
            double φ_eff = 30;

            // Odränerade egenskaper
            double γM_odr = 1.3;
            double ηodr_18 = 0.8;
            double cuk = 30;
            // ------------------
            

            if (KontrollUtskrift)
            {
                Console.WriteLine($"γM_dr = { γM_dr}");
            }

            var UtnyttjandeG = new List<double> { 2, 1, 1 };
            return UtnyttjandeG;
        }
    }
}
