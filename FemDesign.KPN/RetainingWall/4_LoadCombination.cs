using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FemDesign.Loads;

namespace RetainingWall
{
    internal class Loadcombination
    {
        public static List<LoadCombination> loadcombinations(List<LoadCase> loadcases)
        {
            // Predefinition from input data
            //loadcases
            LoadCase loadCaseDL = loadcases[0];
            LoadCase loadCaseR = loadcases[1];
            LoadCase loadCaseJU = loadcases[2];
            LoadCase loadCaseJK = loadcases[3];
            LoadCase loadCaseJA = loadcases[4];
            LoadCase loadCaseHHW = loadcases[5];
            LoadCase loadCaseMW = loadcases[6];
            LoadCase loadCaseLLW = loadcases[7];
            LoadCase loadCaseEffJHHW = loadcases[8];
            LoadCase loadCaseEffJMW = loadcases[9];
            LoadCase loadCaseEffJLLW = loadcases[10];
            LoadCase loadCaseÖLVU = loadcases[11];
            LoadCase loadCaseÖLHU = loadcases[12];
            LoadCase loadCaseÖLVP = loadcases[13];
            LoadCase loadCaseÖLHP = loadcases[14];
            LoadCase loadCaseAcc = loadcases[15];

            // ----- Define load combination -----
            // Load combination when water adds lift
            //var loadComb1 = new LoadCombination("LC1  A, 6.10  Vmin_Hmax | UG_B UG_S", LoadCombType.UltimateOrdinary, (loadCaseDL, 0.90), (loadCaseR, 0.90), (loadCaseJU, 1.10), (loadCaseHHW, 1.10), (loadCaseEffJHHW, 1.10), (loadCaseÖLHU, 1.40));
            //var loadComb2 = new LoadCombination("LC2  B, 6.10a Vmin_Hmax | UG_B", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJU, 1.35), (loadCaseHHW, 1.35), (loadCaseEffJHHW, 1.35), (loadCaseÖLHU, 1.05));
            //var loadComb3 = new LoadCombination("LC3  B, 6.10a Vmax_Hmax | UG_B", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.35), (loadCaseR, 1.35), (loadCaseJU, 1.35), (loadCaseLLW, 1.00), (loadCaseEffJLLW, 1.00), (loadCaseÖLHU, 1.05), (loadCaseÖLVU, 1.05));
            //var loadComb4 = new LoadCombination("LC4  B, 6.10a Vmin_Hmax(Glid) | UG_B UG_G", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJA, 1.35), (loadCaseHHW, 1.35), (loadCaseEffJHHW, 1.35), (loadCaseÖLHU, 1.05));
            //var loadComb5 = new LoadCombination("LC5  B, 6.10b Vmin_Hmax", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJU, 1.10), (loadCaseHHW, 1.10), (loadCaseEffJHHW, 1.10), (loadCaseÖLHU, 1.40));
            //var loadComb6 = new LoadCombination("LC6  B, 6.10b Vmax_Hmax | UG_B UG_S", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.35), (loadCaseR, 1.35), (loadCaseJU, 1.10), (loadCaseLLW, 1.00), (loadCaseEffJLLW, 1.00), (loadCaseÖLHU, 1.40), (loadCaseÖLVU, 1.40));
            //var loadComb7 = new LoadCombination("LC7  B, 6.10b Vmin_Hmax(Glid) | UG_B UG_G", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJA, 1.10), (loadCaseHHW, 1.10), (loadCaseEffJHHW, 1.10), (loadCaseÖLHU, 1.40));
            //var loadComb8 = new LoadCombination("LC8  Bruk-frekv, 6.15  Vmin_Hmax", LoadCombType.ServiceabilityFrequent, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJK, 1.00), (loadCaseMW, 1.00), (loadCaseEffJMW, 1.00), (loadCaseÖLHU, 0.75));
            //var loadComb9 = new LoadCombination("LC9  Bruk-kvasi, 6.16 | UG_B UG_SLS", LoadCombType.ServiceabilityQuasiPermanent, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJK, 1.00), (loadCaseMW, 1.00), (loadCaseEffJMW, 1.00));
            //var loadComb10 = new LoadCombination("LC10 Bruk-kvasi, 6.16 ÖLP", LoadCombType.ServiceabilityQuasiPermanent, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJK, 1.00), (loadCaseMW, 1.00), (loadCaseEffJMW, 1.00), (loadCaseÖLHP, 1.00), (loadCaseÖLVP, 1.00));
            //var loadComb11 = new LoadCombination("LC11  Olycks, 6.11", LoadCombType.UltimateAccidental, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJK, 1.00), (loadCaseMW, 1.00), (loadCaseEffJMW, 1.00), (loadCaseAcc, 1.00));


            ////Load combination when water adds weight
            //var loadComb1 = new LoadCombination("LC1  A, 6.10  Vmin_Hmax", LoadCombType.UltimateOrdinary, (loadCaseDL, 0.90), (loadCaseR, 0.90), (loadCaseJU, 1.10), (loadCaseLLW, 0.90), (loadCaseEffJLLW, 0.90), (loadCaseÖLHU, 1.40));
            //var loadComb2 = new LoadCombination("LC2  B, 6.10a Vmin_Hmax", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJU, 1.35), (loadCaseLLW, 1.00), (loadCaseEffJLLW, 1.00), (loadCaseÖLHU, 1.05));
            //var loadComb3 = new LoadCombination("LC3  B, 6.10a Vmax_Hmax", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.35), (loadCaseR, 1.35), (loadCaseJU, 1.35), (loadCaseHHW, 1.35), (loadCaseEffJHHW, 1.35), (loadCaseÖLHU, 1.05), (loadCaseÖLVU, 1.05));
            //var loadComb4 = new LoadCombination("LC4  B, 6.10a Vmin_Hmax(Glid)", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJA, 1.35), (loadCaseLLW, 1.00), (loadCaseEffJLLW, 1.00), (loadCaseÖLHU, 1.05));
            //var loadComb5 = new LoadCombination("LC5  B, 6.10b Vmin_Hmax", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJU, 1.10), (loadCaseLLW, 1.00), (loadCaseEffJLLW, 1.00), (loadCaseÖLHU, 1.40));
            //var loadComb6 = new LoadCombination("LC6  B, 6.10b Vmax_Hmax", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.35), (loadCaseR, 1.35), (loadCaseJU, 1.10), (loadCaseHHW, 1.10), (loadCaseEffJHHW, 1.10), (loadCaseÖLHU, 1.40), (loadCaseÖLVU, 1.40));
            //var loadComb7 = new LoadCombination("LC7  B, 6.10b Vmin_Hmax(Glid)", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJA, 1.10), (loadCaseLLW, 1.00), (loadCaseEffJLLW, 1.00), (loadCaseÖLHU, 1.40));
            //var loadComb8 = new LoadCombination("LC8  Bruk-frekv, 6.15  Vmin_Hmax", LoadCombType.ServiceabilityFrequent, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJK, 1.00), (loadCaseMW, 1.00), (loadCaseEffJMW, 1.00), (loadCaseÖLHU, 0.75));
            //var loadComb9 = new LoadCombination("LC9  Bruk-kvasi, 6.16", LoadCombType.ServiceabilityQuasiPermanent, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJK, 1.00), (loadCaseMW, 1.00), (loadCaseEffJMW, 1.00));
            //var loadComb10 = new LoadCombination("LC10 Bruk-kvasi, 6.16 ÖLP", LoadCombType.ServiceabilityQuasiPermanent, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJK, 1.00), (loadCaseMW, 1.00), (loadCaseEffJMW, 1.00), (loadCaseÖLHP, 1.00), (loadCaseÖLVP, 1.00));
            //var loadComb11 = new LoadCombination("LC11  Olycks, 6.11", LoadCombType.UltimateAccidental, (loadCaseDL, 1.00), (loadCaseR, 1.00), (loadCaseJK, 1.00), (loadCaseMW, 1.00), (loadCaseEffJMW, 1.00), (loadCaseAcc, 1.00));

            // Load combination example
            var yd = 0.91;
            var loadComb1 = new LoadCombination("LC1  B, 6.10a  Vogyn_Hogyn_Mogyn | UG_B UG_G UG_S ", LoadCombType.UltimateOrdinary, (loadCaseDL, yd * 1.35), (loadCaseJU, yd * 1.10), (loadCaseÖLHU, yd * 1.40));
            var loadComb2 = new LoadCombination("LC2  B, 6.10a  Vgyn_Hgyn_Mgyn    | UG_B UG_G UG_S ", LoadCombType.UltimateOrdinary, (loadCaseDL, 1.00), (loadCaseJU, 1.00));

            var loadComb3 = new LoadCombination("LC3  B, Bruk-kvasi, 6.16  Vogyn_Hogyn_Mogyn | UG_SLS", LoadCombType.ServiceabilityQuasiPermanent, (loadCaseDL, 1.00), (loadCaseJK, 1.00), (loadCaseÖLHP, 1.00));
            var loadComb4 = new LoadCombination("LC4  B, Bruk-kvasi, 6.16  Vgyn_Hgyn_Mgyn    | UG_SLS", LoadCombType.ServiceabilityQuasiPermanent, (loadCaseDL, 1.00), (loadCaseJK, 1.00));

            var loadComb5 = new LoadCombination("LC5  B, Bruk-frek, 6.15  Vogyn_Hogyn_Mogyn ", LoadCombType.ServiceabilityFrequent, (loadCaseDL, 1.00), (loadCaseJK, 1.00), (loadCaseÖLHU, 0.75));

            var loadcombinations = new List<LoadCombination>
                {
                loadComb1,
                loadComb2,
                loadComb3,
                loadComb4,
                loadComb5,
                //loadComb6,
                //loadComb7,
                //loadComb8,
                //loadComb9,
                //loadComb10,
                //loadComb11,
                };
            return loadcombinations;

            //var test1 = loadComb1.Name;
            //var test2 = loadComb2.Type;
            //var test3 = loadcombinations[0].Name;


        }
     }
}

