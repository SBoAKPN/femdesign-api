﻿using FemDesign.Shells;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FemDesign.Results;
using FemDesign.Loads;

using Excel = Microsoft.Office.Interop.Excel;

namespace RetainingWall
{
    internal class Resultprinting
    {
        public static String createExcel() 
        {
            //Create link to excel and read parameters to lists:
            Application oExcelApp = new Microsoft.Office.Interop.Excel.Application();
            oExcelApp.Visible = false;

            Workbook oWorkbook = oExcelApp.Workbooks.Add(XlSheetType.xlWorksheet);
            //workbook.Name = "Resultat";

            Worksheet oSheet1 = (Worksheet)oWorkbook.Sheets.Add();
            oSheet1.Name = "Grundläggning";

            Worksheet oSheet2 = (Worksheet)oWorkbook.Sheets.Add(After: oSheet1);
            oSheet2.Name = "Deformation";

            // Namnge och spara filen
            string filePath = @"C:\Users\kpn\VS local\FemDesign.KPN\RetainingWall\Exported_Data.xlsx"; // Ange sökväg och filnamn
            oWorkbook.SaveAs(filePath);

            // Stäng arbetsboken och Excel
            oWorkbook.Close();
            oExcelApp.Quit();

            // Frigör COM-objekt
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oSheet1);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oSheet2);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oWorkbook);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oExcelApp);

            return filePath;
        }

        public static void deformationsX(String filePath, List<Slab> slabs, List<double> deformation_Sf, List<double> deformation_Sq, List<double> normDeformation_Var, List<double> normDeformation_Sq)
        {
            // Predefinition from input data
            // slabs
            Slab SlabMUR = slabs[1];
            var P_MUR = SlabMUR.SlabPart.Region.Contours[0].Points;

            // Öppna befintlig excelfil Sheet1
            var oExcelApp = new Excel.Application();
            oExcelApp.Visible = false; 
            Workbook oWorkbook = oExcelApp.Workbooks.Open(filePath);
            Worksheet oSheet1 = (Excel.Worksheet)oWorkbook.Sheets[1];

            // Skriv data till Sheet1
            // Definera startrad och relativ längd
            double CountDefValues = deformation_Sf.Count;
            double Rel_L = 1 / (CountDefValues - 1);

            double StartRow_AbsDef = 3;
            double StartRow_RelDef = CountDefValues + 6;

            // Rubriker
            oSheet1.Cells[1, 1] = "Absolut deformation [mm]";
            var RubrikAbsDef = new List<String> { "Rel. L \r\nBPL -> Murtop", "Sf", "Sq", "Sq ÖLP" };

            oSheet1.Cells[CountDefValues + 4, 1] = "Relativ deformation från inspänning (u)"; // Relativ start? Så att antalet resultatpunkter kan variera
            var RubrikRelDef = new List<String> { "Rel. L \r\nBPL -> Murtop", "Var (Sf-Sq)", "Perm def", "Vald överhöjing" };

            //Loopa deformation rad
            for (int i = 0; i < CountDefValues; i++)
            {
                oSheet1.Cells[i + StartRow_AbsDef, 1 ] = Rel_L*i;
                oSheet1.Cells[i + StartRow_AbsDef, 2 ] = deformation_Sf[i];
                oSheet1.Cells[i + StartRow_AbsDef, 3 ] = deformation_Sq[i];

                oSheet1.Cells[i + StartRow_RelDef, 1 ] = Rel_L * i;
                oSheet1.Cells[i + StartRow_RelDef, 2 ] = normDeformation_Var[i];
                oSheet1.Cells[i + StartRow_RelDef, 3 ] = normDeformation_Sq[i];

            }

            //Loopa deformation column
            for (int j = 0; j < RubrikAbsDef.Count; j++)
            {
                oSheet1.Cells[StartRow_AbsDef-1, j + 1] = RubrikAbsDef[j];
                oSheet1.Cells[StartRow_RelDef-1, j + 1] = RubrikRelDef[j];
            }

            // Kontroll av variabel deformation
            var umax = normDeformation_Var.Max();
            var Kravu = (P_MUR[0].Z - P_MUR[1].Z) * 1000 / 200;
            var Kontroll = umax <= Kravu ? "OK" : "NOK";

            oSheet1.Cells[CountDefValues * 2 + 7 , 1] = "u max";
            oSheet1.Cells[CountDefValues * 2 + 8 , 1] = "Krav*): \r\nu ≤ L/200";
            oSheet1.Cells[CountDefValues * 2 + 9 , 1] = "Kontroll: \r\nu max ≤  Krav";
            oSheet1.Cells[CountDefValues * 2 + 10, 1] = "*) KB K135529";

            oSheet1.Cells[CountDefValues * 2 + 7, 2] = umax;
            oSheet1.Cells[CountDefValues * 2 + 8, 2] = Kravu;
            oSheet1.Cells[CountDefValues * 2 + 9, 2] = Kontroll;

            // Stäng arbetsboken och Excel
            oWorkbook.Close();
            oExcelApp.Quit();

            // Frigör COM-objekt
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oSheet1);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oWorkbook);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oExcelApp);
        }

        public static void deformationsX(String filePath, List<Slab> slabs, List<double> deformation_Sf, List<double> deformation_Sq, List<double> deformation_Sq_ÖLP, List<double> normDeformation_Var, List<double> normDeformation_Sq)
        {
            // Predefinition from input data
            // slabs
            Slab SlabMUR = slabs[1];
            var P_MUR = SlabMUR.SlabPart.Region.Contours[0].Points;

            // Öppna befintlig excelfil Sheet1
            var oExcelApp = new Excel.Application();
            oExcelApp.Visible = false;
            Workbook oWorkbook = oExcelApp.Workbooks.Open(filePath);
            Worksheet oSheet1 = (Excel.Worksheet)oWorkbook.Sheets[1];

            // Skriv data till Sheet1
            // Definera startrad och relativ längd
            double CountDefValues = deformation_Sf.Count;
            double Rel_L = 1 / (CountDefValues - 1);

            double StartRow_AbsDef = 3;
            double StartRow_RelDef = CountDefValues + 6;

            // Rubriker
            oSheet1.Cells[1, 1] = "Absolut deformation [mm]";
            var RubrikAbsDef = new List<String> { "Rel. L \r\nBPL -> Murtop", "Sf", "Sq", "Sq ÖLP" };

            oSheet1.Cells[CountDefValues + 4, 1] = "Relativ deformation från inspänning (u)"; // Relativ start? Så att antalet resultatpunkter kan variera
            var RubrikRelDef = new List<String> { "Rel. L \r\nBPL -> Murtop", "Var (Sf-Sq)", "Perm def", "Vald överhöjing" };

            //Loopa deformation rad
            for (int i = 0; i < CountDefValues; i++)
            {
                oSheet1.Cells[i + StartRow_AbsDef, 1] = Rel_L * i;
                oSheet1.Cells[i + StartRow_AbsDef, 2] = deformation_Sf[i];
                oSheet1.Cells[i + StartRow_AbsDef, 3] = deformation_Sq[i];
                oSheet1.Cells[i + StartRow_AbsDef, 4] = deformation_Sq_ÖLP[i];

                oSheet1.Cells[i + StartRow_RelDef, 1] = Rel_L * i;
                oSheet1.Cells[i + StartRow_RelDef, 2] = normDeformation_Var[i];
                oSheet1.Cells[i + StartRow_RelDef, 3] = normDeformation_Sq[i];

            }

            //Loopa deformation column
            for (int j = 0; j < RubrikAbsDef.Count; j++)
            {
                oSheet1.Cells[StartRow_AbsDef - 1, j + 1] = RubrikAbsDef[j];
                oSheet1.Cells[StartRow_RelDef - 1, j + 1] = RubrikRelDef[j];
            }

            // Kontroll av variabel deformation
            var umax = normDeformation_Var.Max();
            var Kravu = (P_MUR[0].Z - P_MUR[1].Z) * 1000 / 200;
            var Kontroll = umax <= Kravu ? "OK" : "NOK";

            oSheet1.Cells[CountDefValues * 2 + 7, 1] = "u max";
            oSheet1.Cells[CountDefValues * 2 + 8, 1] = "Krav*): \r\nu ≤ L/200";
            oSheet1.Cells[CountDefValues * 2 + 9, 1] = "Kontroll: \r\nu max ≤  Krav";
            oSheet1.Cells[CountDefValues * 2 + 10, 1] = "*) KB K135529";

            oSheet1.Cells[CountDefValues * 2 + 7, 2] = umax;
            oSheet1.Cells[CountDefValues * 2 + 8, 2] = Kravu;
            oSheet1.Cells[CountDefValues * 2 + 9, 2] = Kontroll;

            // Stäng arbetsboken och Excel
            oWorkbook.Close();
            oExcelApp.Quit();

            // Frigör COM-objekt
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oSheet1);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oWorkbook);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oExcelApp);
        }

        public static void GrundResultant(String filePath, List<LoadCombination> loadcases, List<FemDesign.Results.SurfaceSupportResultant> SupportResultant)
        {
            // Öppna befintlig excelfil Sheet1
            var oExcelApp = new Excel.Application();
            oExcelApp.Visible = false;
            Workbook oWorkbook = oExcelApp.Workbooks.Open(filePath);
            Worksheet oSheet2 = (Excel.Worksheet)oWorkbook.Sheets[2];

            // Skriv data till Sheet2
            double CountLoadcombinations = loadcases.Count;
            double ColumbCount = 7;
            var Rubrik = new List<String> { "Loadcase name", "Fx'", "Fy'", "Fz'", "Mx'", "My'", "Mz'"};
          
            for (int i = 0; i < CountLoadcombinations; i++)
            {
                for (int j = 0; j < ColumbCount; j++)
                {
                    while (i == 0)
                    {
                        oSheet2.Cells[1, j + 1] = Rubrik[j];
                    }
                    oSheet2.Cells[i + 2, j + 1] = SupportResultant[i].Name;
                    oSheet2.Cells[i + 2, j + 2] = SupportResultant[i].Fx;
                    oSheet2.Cells[i + 2, j + 3] = SupportResultant[i].Fy;
                    oSheet2.Cells[i + 2, j + 4] = SupportResultant[i].Fz;
                    oSheet2.Cells[i + 2, j + 5] = SupportResultant[i].Mx;
                    oSheet2.Cells[i + 2, j + 6] = SupportResultant[i].My;
                    oSheet2.Cells[i + 2, j + 7] = SupportResultant[i].Mz;
                }
            }
            // Stäng arbetsboken och Excel
            oWorkbook.Close();
            oExcelApp.Quit();

            // Frigör COM-objekt
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oSheet2);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oWorkbook);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oExcelApp);
        }
    }
}
