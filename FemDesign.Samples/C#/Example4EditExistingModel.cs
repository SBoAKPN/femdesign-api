﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FemDesign.Samples
{
    public partial class SampleProgram
    {
        private static void Example4EditExistingModel()
        {
            // EXAMPLE 4: EDITING AN EXISTING MODEL
            // In this example, we will edit an existing model by isolating a floor and replacing supporting
            // walls and pillars with appropriate supports. Using height as a point of comparison we can find
            // which elements to reuse from the old model, and create a new model with our selected elements.

            // This example was last updated 2022-04-13, using the ver. 21.1.0 FEM-Design API.
            

            // READ THE MODEL:
            // Deserialize the current model to access all the data in the .struxml file.
            FemDesign.Model model = FemDesign.Model.DeserializeFromFilePath(@"C:\Users\SamuelNyberg\Documents\GitHub\femdesign-api\FemDesign.Samples\C#\ExampleModels\Example 4 - model.struxml");


            // ISOLATE A FLOOR:
            // Choose which floor will be singled out.
            int floor = 3;

            FemDesign.StructureGrid.Storey storey = model.Entities.Storeys.Storey[floor];
            double zCoord = storey.Origo.Z;


            // POINT SUPPORTS:
            // Find all pillars supporting the chosen floor, and place point supports in their place.
            var supports = new List<GenericClasses.ISupportElement>();
            for (int i = 0; i < model.Entities.Bars.Count; i++)
            {
                Bars.Bar tempBar = model.Entities.Bars[i];
                if (tempBar.BarPart.Type != Bars.BarType.Column)
                {
                    continue;
                }

                if (Math.Abs(tempBar.BarPart.Edge.Points[0].Z-zCoord) < Tolerance.LengthComparison)
                {
                    var tempSupport = new Supports.PointSupport(
                        point: new Geometry.FdPoint3d(tempBar.BarPart.Edge.Points[0].X, tempBar.BarPart.Edge.Points[0].Y, zCoord),
                        motions: Releases.Motions.RigidPoint(),
                        rotations: Releases.Rotations.Free()
                        );
                    supports.Add(tempSupport);
                }
            }
            

            // ELEMENTS:
            // The model only contains plates and walls, so we will not be looking for beams etc.
            // We are looking for the floor plate at the right height, and the walls below it to
            // replace them with line supports.
            var elements = new List<GenericClasses.IStructureElement>();
            int n;

            // TESTING SLABS:
            // Slabs have a property which indicates if they are floors (plate) or walls (wall).
            // Based on this, we can sort out if we want to use them as an element or place a
            // line support in their stead.
            for (int i = 0; i < model.Entities.Slabs.Count; i++)
            {
                Shells.Slab tempSlab = model.Entities.Slabs[i];
                if (tempSlab.Type == Shells.SlabType.Plate && tempSlab.SlabPart.LocalPos.Z == zCoord)
                {
                    elements.Add(tempSlab);
                }
                else if (tempSlab.Type == Shells.SlabType.Wall)
                {
                    if (tempSlab.SlabPart.Region.Contours[0].Edges[2].Points[0].Z == zCoord)
                    {
                        var tempSupport = new Supports.LineSupport(
                            edge: tempSlab.SlabPart.Region.Contours[0].Edges[2],
                            motions: new Releases.Motions(0, 0, 0, 0, 10 ^ 7, 10 ^ 7),
                            rotations: new Releases.Rotations(0, 0, 0, 0, 0, 0),
                            movingLocal: true
                            );
                        supports.Add(tempSupport);
                    }
                }
            }

            // TESTING PANELS:
            // Panels do not have the same property as slabs. Instead, we compare the height of all the
            // edge curves of the panel to discern if it is horizontal or not.
            for (int i = 0; i < model.Entities.Panels.Count; i++)
            {
                Shells.Panel tempPanel = model.Entities.Panels[i];
                n = 0;
                for (int j = 0; j < tempPanel.Region.Contours[0].Edges.Count; j++)
                {
                    if (tempPanel.Region.Contours[0].Edges[j].Points[0].Z != tempPanel.Region.Contours[0].Edges[j].Points[1].Z)
                    {
                        n++;
                        return;
                    }
                }
                if (n == 0 && tempPanel.Region.Contours[0].Edges[0].Points[0].Z == zCoord)
                {
                    elements.Add(tempPanel);
                }
                else if (n != 0 && tempPanel.Region.Contours[0].Edges[2].Points[0].Z == zCoord)
                {
                    var tempSupport = new Supports.LineSupport(
                            edge: tempPanel.Region.Contours[0].Edges[2],
                            motions: new Releases.Motions(0, 0, 0, 0, 10 ^ 7, 10 ^ 7),
                            rotations: new Releases.Rotations(0, 0, 0, 0, 0, 0),
                            movingLocal: true
                            );
                    supports.Add(tempSupport);
                }
            }


            // LOADS:
            // Similar to supports and elements, we will reuse loads from the model if they are on the correct height
            var loads = new List<GenericClasses.ILoadElement>();
            for (int i = 0; i < model.Entities.Loads.LineLoads.Count; i++)
            {
                if (model.Entities.Loads.LineLoads[i].Edge.XAxis.Z == zCoord)
                {
                    loads.Add(model.Entities.Loads.LineLoads[i]);
                }
            }
            for (int i = 0; i < model.Entities.Loads.SurfaceLoads.Count; i++)
            {
                Loads.SurfaceLoad tempLoad = model.Entities.Loads.SurfaceLoads[i];
                if (tempLoad.Region.Contours[0].Edges[0].Points[0].Z == zCoord)
                {
                    loads.Add(tempLoad);
                }
            }


            // CREATE NEW MODEL:
            // With a new model, we can add all our gathered elements to it. We can also take load cases,
            // load combinations, and the storey marker directly from the old model.
            FemDesign.Model newModel = new FemDesign.Model(Country.S);
            newModel.AddElements(elements);
            newModel.AddSupports(supports);
            newModel.AddLoads(loads);
            newModel.AddLoadCases(model.Entities.Loads.LoadCases);
            newModel.AddLoadCombinations(model.Entities.Loads.LoadCombinations);
            // newModel.AddEntities(model.Entities.Storeys.Storey[floor]);                    <------- finns det en motsvarande metod för att bara lägga till en grej?
            

            // SAVE AND RUN:
            // Create a file path for the new model, serialize it, and run the script!
            string path = @"C:\Users\SamuelNyberg\OneDrive - StruSoft AB\Samuels arbetshörna\14. C#-exempel\edited_model.struxml"; //Borde jag spara med samma namn?
            newModel.SerializeModel(path);

            var app = new Calculate.Application();
            app.OpenStruxml(path, true);
        }
    }
}