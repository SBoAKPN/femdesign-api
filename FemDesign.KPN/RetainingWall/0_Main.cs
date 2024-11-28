using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FemDesign;
using FemDesign.Calculate;
using FemDesign.GenericClasses;
using FemDesign.Geometry;
using FemDesign.Loads;
using FemDesign.Materials;
using FemDesign.ModellingTools;
using FemDesign.Reinforcement;
using FemDesign.Releases;
using FemDesign.Results;
using FemDesign.Shells;
using FemDesign.Utils;
using RetainingWall;
using StruSoft.Interop.StruXml.Data;

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
            var reinfElements = ReinForcement.reinfElements(slabs);
            // Removing reinforced slabs to prevent multiple instances of same element
            elements.Remove(elements[0]);
            elements.Remove(elements[1]);
            elements.Add(reinfElements[0]);
            elements.Add(reinfElements[1]);

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
            var design = new FemDesign.Calculate.Design(autoDesign: true, check: true, loadCombination: true, applyChanges: true);

            // create a direct link to FEM-Design and comunicate with it
            using (var femDesign = new FemDesign.FemDesignConnection(keepOpen: true))
            {
                femDesign.Open(model);
                femDesign.RunAnalysis(analysis);
                //femDesign.RunDesign(RCDESIGN, design, null);
                //var SurfaceSupportResultant0 = femDesign.GetLoadCombinationResults<Results.SurfaceSupportResultant>();
                //var SurfaceSupportResultant1 = femDesign.GetResults<Results.SurfaceSupportResultant>();
            }

        }

    }
}