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


namespace FemDesign.RetainingWall
{
    internal class Program
    {
        static void Main()
        {
            double[] FundIterStop = { 0.95, 1 };
            double Ug = FundIterStop.Average();
            var Ug_List = new List<double>();
            double bx_BPL = 2.5;

            var model = new Model(Country.S);
            var analysis = new Analysis();
            var slabs = new List<Slab>();
            var loadcombinations = new List<LoadCombination>();

            // model analysis slabs loadcombinations

            while (Ug > FundIterStop[0] && Ug < FundIterStop[1])
            {

                //// Parametric design of a retainingwall v0.0
                // Class 1
                double MeshSize = 0.5;
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
                var (slabstoRemove,reinfElements) = ReinForcement.straightReinfElements(slabs, ReinfMaterial);

                // Removing reinforced slabs before adding, to prevent multiple instances of same element
                elements.RemoveAll(slabs => slabstoRemove.Contains(slabs));
                elements.AddRange(reinfElements);

                // Class 6
                var labelledsections = Resultprocessing.labelledsections(slabs);

                // Assemble the model
                model.AddElements(elements);
                model.AddLoads(loads);
                model.AddLoadCases(loadcases);
                model.AddLoadCombinations(loadcombinations);
                // model.AddLabelledSection(LabelledSections);
                // model.AddSection(LabelledSections);

                //Define the analysis settings
                analysis = Analysis.StaticAnalysis(calcCase: true, calccomb: true);

                // Designing foundation dimensions
                // Create a direct link to FEM-Design and comunicate with it
                using (var foundationDesignFEM = new FemDesign.FemDesignConnection(minimized : false, keepOpen: false))
                {
                    foundationDesignFEM.Open(model);

                    //Analysis
                    foundationDesignFEM.RunAnalysis(analysis);

                    var SupportResultant = foundationDesignFEM.GetLoadCombinationResults<Results.SurfaceSupportResultant>();

                    var SupportResultant_UG_B1 = SupportResultant.Where(Lcc => Lcc.CaseIdentifier.Contains("UG_B")).ToList();
                    var SupportResultant_UG_B2 = SupportResultant_UG_B1.Concat(SupportResultant_UG_B1).ToList();
                    
                    var SupportResultant_UG_G = SupportResultant.Where(Lcc => Lcc.CaseIdentifier.Contains("UG_G")).ToList(); ;
                    var SupportResultant_UG_S = SupportResultant.Where(Lcc => Lcc.CaseIdentifier.Contains("UG_S")).ToList(); ;
                    var SupportResultant_UG_SLS = SupportResultant.Where(Lcc => Lcc.CaseIdentifier.Contains("UG_SLS")).ToList(); ;

                    var (Ug_B, qb_rd_ULS) = FundationDesign.Bärighet(SupportResultant_UG_B2, slabs, false);
                    var Ug_G = FundationDesign.Glidning(SupportResultant_UG_G, slabs, true);
                    var Ug_S = FundationDesign.Stjälpning(SupportResultant_UG_S, slabs, true);
                    var Ug_SLS = FundationDesign.Krypdeformation(qb_rd_ULS, SupportResultant_UG_SLS, slabs, true);

                      Ug_List.Concat(Ug_B).Concat(Ug_G).Concat(Ug_S).Concat(Ug_SLS);
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
                }

            }

            Console.WriteLine("foundationDesign complete");

            //Define the design settings
            var design = new FemDesign.Calculate.Design(autoDesign: false, check: true, loadCombination: true, applyChanges: true);
            var config = new FemDesign.Calculate.ConcreteDesignConfig(0, true, false, false, false);

            //Define Path for documentation printing
            string docxFilePath = @"C:\Users\kpn\VS local\FemDesign.KPN\RetainingWall\Appendix.docx";
            string dscTemplateFilePath = @"C:\Users\kpn\VS local\FemDesign.KPN\RetainingWall\Retainingwall_doc_template.dsc";

            // Define Path for outputDir
            string MyoutputDir = @"C:\Users\kpn\VS local\FemDesign.KPN\RetainingWall";

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
                var feaNodes = ReinforcementDesignFEM.GetFeaNodes();
                var disp = ReinforcementDesignFEM.GetResults<NodalDisplacement>();

                var deformation_Sf = Resultprocessing.deformationsX(slabs, feaNodes, disp, loadcombinations[7]);
                var deformation_Sq = Resultprocessing.deformationsX(slabs, feaNodes, disp, loadcombinations[8]);
                var deformation_Sq_ÖLP = Resultprocessing.deformationsX(slabs, feaNodes, disp, loadcombinations[9]);

                var normDeformation_Var = Resultprocessing.normDeformations_Var(slabs, feaNodes, disp, loadcombinations);
                var normDeformation_Sq = Resultprocessing.normDeformations_Sq(slabs, feaNodes, disp, loadcombinations);

                // Resultprinting
                String resultFilepath = Resultprinting.createExcel();
                Resultprinting.deformationsX(resultFilepath, slabs, deformation_Sf, deformation_Sq, deformation_Sq_ÖLP, normDeformation_Var, normDeformation_Sq);
                Resultprinting.GrundResultant(resultFilepath, loadcombinations, SupportResultant);


                // RCDesign
                ReinforcementDesignFEM.RunDesign(Calculate.CmdUserModule.RCDESIGN, design, null);

                // Create documentation from template
                ReinforcementDesignFEM.SetProjDescription(ProjDescruption[0], ProjDescruption[1], ProjDescruption[2], ProjDescruption[3], ProjDescruption[4]);
                ReinforcementDesignFEM.SaveDocx(docxFilePath, dscTemplateFilePath);
            }
            Console.WriteLine("ReinforcementDesign complete");
            Console.WriteLine("Calculation complete");
            Console.ReadKey();
        }
    }
}