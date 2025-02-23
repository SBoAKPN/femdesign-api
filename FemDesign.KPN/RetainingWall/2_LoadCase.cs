using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FemDesign.Loads;

namespace RetainingWall
{
    internal class Loadcase
    {
        public static List<LoadCase> loadcases ()
        {
            // ----- Define load cases -----
            LoadCase loadCaseDL         = new LoadCase("DL", LoadCaseType.DeadLoad, LoadCaseDuration.Permanent);
            LoadCase loadCaseR          = new LoadCase("Räcke", LoadCaseType.Static, LoadCaseDuration.Permanent);

            LoadCase loadCaseJU         = new LoadCase("JordU", LoadCaseType.Static, LoadCaseDuration.Permanent);
            LoadCase loadCaseJK         = new LoadCase("JordK", LoadCaseType.Static, LoadCaseDuration.Permanent);
            LoadCase loadCaseJA         = new LoadCase("JordA", LoadCaseType.Static, LoadCaseDuration.Permanent);

            LoadCase loadCaseHHW        = new LoadCase("HHW", LoadCaseType.Static, LoadCaseDuration.LongTerm);
            LoadCase loadCaseMW         = new LoadCase("MW", LoadCaseType.Static, LoadCaseDuration.Permanent);
            LoadCase loadCaseLLW        = new LoadCase("LLW", LoadCaseType.Static, LoadCaseDuration.LongTerm);

            //LoadCase loadCaseEffJHHW    = new LoadCase("Eff J HHW", LoadCaseType.Static, LoadCaseDuration.Permanent);
            //LoadCase loadCaseEffJMW     = new LoadCase("Eff J MW", LoadCaseType.Static, LoadCaseDuration.Permanent);
            //LoadCase loadCaseEffJLLW    = new LoadCase("Eff J LLW", LoadCaseType.Static, LoadCaseDuration.Permanent);

            LoadCase loadCaseÖLVU       = new LoadCase("ÖLVU", LoadCaseType.Static, LoadCaseDuration.ShortTerm);
            LoadCase loadCaseÖLHU       = new LoadCase("ÖLHU", LoadCaseType.Static, LoadCaseDuration.ShortTerm);

            LoadCase loadCaseÖLVP       = new LoadCase("ÖLVP", LoadCaseType.Static, LoadCaseDuration.Permanent);
            LoadCase loadCaseÖLHP       = new LoadCase("ÖLHP", LoadCaseType.Static, LoadCaseDuration.Permanent);

            LoadCase loadCaseAcc        = new LoadCase("Olycka", LoadCaseType.Static, LoadCaseDuration.Instantaneous);

            //LoadCase Testlast1 = new LoadCase("Testlast1", LoadCaseType.Static, LoadCaseDuration.Instantaneous);
            //LoadCase Testlast2 = new LoadCase("Testlast2", LoadCaseType.Static, LoadCaseDuration.Instantaneous);
            //LoadCase Testlast3 = new LoadCase("Testlast3", LoadCaseType.Static, LoadCaseDuration.Instantaneous);

            var loadcases = new List<LoadCase> 
            { 
                loadCaseDL,
                loadCaseR,
                loadCaseJU,
                loadCaseJK,
                loadCaseJA,
                loadCaseHHW,
                loadCaseMW,
                loadCaseLLW,
                //loadCaseEffJHHW,
                //loadCaseEffJMW,
                //loadCaseEffJLLW,
                loadCaseÖLVU,
                loadCaseÖLHU,
                loadCaseÖLVP,
                loadCaseÖLHP,
                loadCaseAcc,
                //Testlast1,
                //Testlast2,
                //Testlast3,
            };
            return loadcases;
        }
    }
}
