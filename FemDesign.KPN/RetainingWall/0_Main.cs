using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FemDesign.Calculate;
using FemDesign.Results;
using FemDesign.Shells;
using RetainingWall;
using FemDesign.Loads;
using FemDesign.Reinforcement;
using FemDesign.Materials;


namespace FemDesign.RetainingWall
{
    internal class Program
    {
        static void Main()
        {
            double[] FundIterStop = { 0.90, 1 };
            double Ug = 0; 
            var Ug_List = new List<double>();
            double bx_BPL = 2.5;
            double MeshSize = 0.5;

            var model = new Model(Country.S);
            //Define the analysis settings
            var analysis = new Analysis();
            analysis = Analysis.StaticAnalysis(calcCase: true, calccomb: true);

            var slabs = new List<Slab>();
            var loadcombinations = new List<LoadCombination>();

            //// model analysis slabs loadcombinations

            while (Ug < FundIterStop[0] || Ug > FundIterStop[1])
            {
                model = new Model(Country.S);
                //// Parametric design of a retainingwall v0.0
                // Class 1
                slabs = Element.slabs(bx_BPL,MeshSize);
                var supports = Element.supports(slabs);
                var elements = Element.elements(slabs,supports);

                // Class 2
                var loadcases = Loadcase.loadcases();
                
                // Class 3
                var loads = Load.loads(slabs,loadcases);

                // Class 4
                loadcombinations = Loadcombination.loadcombinations(loadcases);

                // Class 5
                string ReinfMaterial = "B500B";
                var (slabstoRemove, reinfElements, shearControlRegions) = ReinForcement.straightReinfElements(slabs, ReinfMaterial);

                // Removing reinforced slabs before adding, to prevent multiple instances of same element
                elements.RemoveAll(slabs => slabstoRemove.Contains(slabs));
                elements.AddRange(reinfElements);

                // Class 6
                var labelledsections = Resultprocessing.labelledsections(slabs);
                var rp_model = Resultprocessing.resultpoints_Model(slabs);

                //elements.AddRange(labelledsections);
                //elements.AddRange(rp_model);

                // Assemble the model
                
                model.AddElements(elements);
                model.AddLoads(loads);
                model.AddLoadCases(loadcases);
                model.AddLoadCombinations(loadcombinations);
                //model.Entities.NoShearControlRegions.Add(shearControlRegions[0]);


                // Designing foundation dimensions
                // Create a direct link to FEM-Design and comunicate with it
                using (var foundationDesignFEM = new FemDesign.FemDesignConnection(minimized: false, keepOpen: false))
                {
                    foundationDesignFEM.Open(model);

                    foundationDesignFEM.RunAnalysis(analysis);

                    var SupportResultant = foundationDesignFEM.GetLoadCombinationResults<Results.SurfaceSupportResultant>();

                    var SupportResultant_UG_B1 = SupportResultant.Where(Lcc => Lcc.CaseIdentifier.Contains("UG_B")).ToList();
                    var SupportResultant_UG_B2 = SupportResultant_UG_B1.Concat(SupportResultant_UG_B1).ToList(); // Testar både dränerat och odrännerat tillstånd

                    var SupportResultant_UG_G1 = SupportResultant.Where(Lcc => Lcc.CaseIdentifier.Contains("UG_G")).ToList();
                    var SupportResultant_UG_G2 = SupportResultant_UG_G1.Concat(SupportResultant_UG_G1).ToList(); // Testar både dränerat och odrännerat tillstånd
                    var SupportResultant_UG_S = SupportResultant.Where(Lcc => Lcc.CaseIdentifier.Contains("UG_S ")).ToList();
                    var SupportResultant_UG_SLS = SupportResultant.Where(Lcc => Lcc.CaseIdentifier.Contains("UG_SLS")).ToList();

                    var (Ug_B, qb_rd_ULS) = FundationDesign.Bärighet(SupportResultant_UG_B2, slabs, true);
                    var Ug_G = FundationDesign.Glidning(SupportResultant_UG_G2, slabs, true);
                    var Ug_S = FundationDesign.Stjälpning(SupportResultant_UG_S, slabs, true);
                    var Ug_SLS = FundationDesign.Krypdeformation(qb_rd_ULS, SupportResultant_UG_SLS, slabs, true);

                    Ug_List = Ug_B.Concat(Ug_G).Concat(Ug_S).Concat(Ug_SLS).ToList();
                }

                // Justering av Plattbredd
                Ug = Ug_List.Max();
                if (Ug < FundIterStop[0])
                {
                    
                    if (Ug < 0.8)
                    {
                        bx_BPL -= 0.3;
                    }
                    else
                    {
                        bx_BPL -= 0.1;
                    }
                    Console.WriteLine();
                    Console.WriteLine($"Utnyttjandegrad = {Math.Round(Ug, 2)} (för liten)");
                    Console.WriteLine($"Fundamentlängd minskas till {bx_BPL} m");
                }
                if (Ug > FundIterStop[1])
                {
                    if (Ug > 1.2)
                    {
                        bx_BPL += 0.3;
                    }
                    else
                    {
                        bx_BPL += 0.1;
                    }
                    Console.WriteLine();
                    Console.WriteLine($"Utnyttjandegrad = {Math.Round(Ug, 2)} (för stor)");
                    Console.WriteLine($"Fundamentlängd ökas till {bx_BPL} m");
                }
            }
            Console.WriteLine();
            Console.WriteLine($"Utnyttjandegrad = {Math.Round(Ug,2)} OK!");
            Console.WriteLine("FoundationDesign complete!");

            //Define the design settings
            var design = new FemDesign.Calculate.Design(autoDesign: false, check: true, loadCombination: true, applyChanges: true);
            var config = new FemDesign.Calculate.ConcreteDesignConfig(0, true, false, false, false);
            var rp_result = Resultprocessing.resultpoints_Result(slabs);

            //Define Path for documentation printing

            string docxFilePath = @"C:\Users\kpn\VS local\FemDesign.KPN\RetainingWall\Appendix.docx";
            string dscTemplateFilePath = @"C:\Users\kpn\VS local\FemDesign.KPN\RetainingWall\Retainingwall_doc_template_v2.dsc";

            // Define Path for outputDir

            string MyoutputDir = @"C:\Users\kpn\VS local\FemDesign.KPN\RetainingWall";

            string fullPath_config = @"C:\Users\kpn\VS local\FemDesign.KPN\RetainingWall\cfg.xml";

            // Set project description
            // project, description, designer, signature, comment, items
            var ProjDescruption = new List<string>
            { "Testproj", "Description", "Designer", "Signature", "Comment" };

            // Designing reinforcement and printing documentation
            using (var ReinforcementDesignFEM = new FemDesign.FemDesignConnection(keepOpen: true, outputDir: MyoutputDir))
            {
                ReinforcementDesignFEM.Open(model);
                ReinforcementDesignFEM.SetConfig(config);
                
                ReinforcementDesignFEM.RunAnalysis(analysis);

                var SupportResultant = ReinforcementDesignFEM.GetLoadCombinationResults<Results.SurfaceSupportResultant>();

                // Resultprocessing
                //var feaNodes = ReinforcementDesignFEM.GetFeaNodes();
                //var disp = ReinforcementDesignFEM.GetResults<Results.NodalDisplacement>();

                var unitResults = UnitResults.Default();
                unitResults.Displacement = Displacement.mm;
                var dispAtPoints = ReinforcementDesignFEM.GetResultsOnPoints<Results.ShellDisplacement>(rp_result, unitResults);
                var Disp_table_Sf = Resultprocessing.deformationsX(slabs, dispAtPoints, loadcombinations[0]);
                var Disp_table_Sq = Resultprocessing.deformationsX(slabs, dispAtPoints, loadcombinations[1]);

                var normDeformation_Var = Resultprocessing.normDeformations_Var(slabs, dispAtPoints, loadcombinations);
                var normDeformation_Sq = Resultprocessing.normDeformations_Sq(slabs, dispAtPoints, loadcombinations);

                // Resultprinting
                String resultFilepath = Resultprinting.createExcel();
                Resultprinting.deformationsX(resultFilepath, slabs, Disp_table_Sf, Disp_table_Sq, normDeformation_Var, normDeformation_Sq);
                Resultprinting.GrundResultant(resultFilepath, SupportResultant);

                // Set crack width
                ReinforcementDesignFEM.SetConfig(fullPath_config);

                // Calculate RC Design
                ReinforcementDesignFEM.RunDesign(Calculate.CmdUserModule.RCDESIGN, design, null);

                // Create documentation from template
                ReinforcementDesignFEM.SetProjDescription(ProjDescruption[0], ProjDescruption[1], ProjDescruption[2], ProjDescruption[3], ProjDescruption[4]);
                Console.WriteLine();
                Console.WriteLine("Kör betongberäkning manuellt innan utskrift av bilaga"); // pga att shear controll region inte kan läggas till.
                Console.ReadKey();
                ReinforcementDesignFEM.SaveDocx(docxFilePath, dscTemplateFilePath);

            }
            Console.WriteLine("\n ReinforcementDesign complete!");
            Console.WriteLine("\n Calculation complete!");
            Console.ReadKey();
        }

    }
}