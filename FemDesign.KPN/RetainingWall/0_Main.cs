using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FemDesign.Calculate;
using FemDesign.Results;
using RetainingWall;


namespace FemDesign.RetainingWall
{
    internal class Program
    {
        static void Main()
        {
            //// Parametric design of a retainingwall v0.0
            // Class 1
            double MeshSize = 0.5;
            var slabs = Element.slabs(MeshSize);
            var supports = Element.supports(slabs);
            var elements = Element.elements(slabs,supports);

            // Class 2
            var loadcases = Loadcase.loadcases();

            // Class 3
            var loads = Load.loads(slabs,loadcases);

            // Class 4
            var loadcombinations = Loadcombination.loadcombinations(loadcases);

            // Class 5
            string ReinfMaterial = "B500B";
            var reinfElements = ReinForcement.straightReinfElements(slabs, ReinfMaterial);
            // Removing reinforced slabs to prevent multiple instances of same element
            for (int i = 0; i <= 1; i++)
            {
                elements.Remove(elements[0]);
                elements.Add(reinfElements[i]);
            }

            // Class 6
            var labelledsections = Resultprocessing.labelledsections(slabs);

            // Assemble the model
            var model = new Model(Country.S);
            model.AddElements(elements);
            model.AddLoads(loads);
            model.AddLoadCases(loadcases);
            model.AddLoadCombinations(loadcombinations);
            // model.AddLabelledSection(LabelledSections);
            // model.AddSection(LabelledSections);

            //Define the analysis settings
            var analysis = Analysis.StaticAnalysis(calcCase: true, calccomb: true);

            //Define the design settings
            var design = new FemDesign.Calculate.Design(autoDesign: false, check: true, loadCombination: true, applyChanges: true);
            var config = new FemDesign.Calculate.ConcreteDesignConfig(0, true,false,false,false);

            //RCShellCrackWidth

            //Define Path for documentation printing
            string docxFilePath =  @"C:\Users\kpn\VS local\FemDesign.KPN\RetainingWall\Appendix.docx";
            string dscTemplateFilePath = @"C:\Users\kpn\VS local\FemDesign.KPN\RetainingWall\Retainingwall_doc_template.dsc";

            // Define Path for outputDir
            string MyoutputDir = @"C:\Users\kpn\VS local\FemDesign.KPN\RetainingWall";

            // Set project description
            // project, description, designer, signature, comment, items
            var ProjDescruption = new List<string> 
            { "Testproj", "Description", "Designer", "Signature", "Comment" };

            // create a direct link to FEM-Design and comunicate with it
            using (var femDesign = new FemDesign.FemDesignConnection(keepOpen: true, outputDir: MyoutputDir))
            {
                femDesign.Open(model);
                femDesign.SetConfig(config);
                
                //Analysis
                femDesign.RunAnalysis(analysis);

                // Resultprocessing
                var feaNodes = femDesign.GetFeaNodes();
                var disp = femDesign.GetResults<NodalDisplacement>();

                var deformation_Sf = Resultprocessing.deformationsX(slabs, feaNodes, disp, loadcombinations[7]);
                var deformation_Sq = Resultprocessing.deformationsX(slabs, feaNodes, disp, loadcombinations[8]);
                var deformation_Sq_ÖLP = Resultprocessing.deformationsX(slabs, feaNodes, disp, loadcombinations[9]);

                var normDeformation_Var = Resultprocessing.normDeformations_Var(slabs, feaNodes, disp, loadcombinations);
                var normDeformation_Sq = Resultprocessing.normDeformations_Sq(slabs, feaNodes, disp, loadcombinations);

                var SupportResultant = femDesign.GetLoadCombinationResults<Results.SurfaceSupportResultant>();

                // Resultprinting
                String resultFilepath = Resultprinting.createExcel();
                Resultprinting.deformationsX(resultFilepath, slabs, deformation_Sf, deformation_Sq, deformation_Sq_ÖLP, normDeformation_Var, normDeformation_Sq);
                Resultprinting.GrundResultant(resultFilepath, loadcombinations, SupportResultant);

                // RCDesign
                femDesign.RunDesign(Calculate.CmdUserModule.RCDESIGN, design, null);
                
                // Create documentation from template
                femDesign.SetProjDescription(ProjDescruption[0], ProjDescruption[1], ProjDescruption[2], ProjDescruption[3], ProjDescruption[4]);
                femDesign.SaveDocx(docxFilePath, dscTemplateFilePath);
            }
            Console.WriteLine("Calculation complete");
            Console.ReadKey();
        }
    }
}