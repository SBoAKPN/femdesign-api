using FemDesign.Results;
using FemDesign.Shells;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FemDesign.Loads;
using FemDesign.Results;
namespace RetainingWall
{
    internal class FundationDesign
    {
        public static (List<double>,double) Bärighet(List<SurfaceSupportResultant> SupportResultant_B, List<Slab> slabs, Boolean KontrollUtskrift)
        {
            // Beräkning baseras på allmäna bärighetsekvationen och utförs enligt IEG bilaga C.3

            // Allmäna bärighetsekvationen qbd = (1) cdNcd ξc + (2) qdNqdξq +  (3) 0,5 γeq bef Nγd ξγ
            // Beskrivning av allmäna bärighetsekvationens beteckningar:
            //  Dimensionerande brottvärde för fundamentets grundtryck      [qbd]
            // Dimensionerande skjuvhållfasthet (kohesion)                  [cud]
            // Dimensionerande överlagringstryck på grundläggningsnivå      [qd]
            // Viktat värde på jordens effektiva tunghet                    [γeq]
            // under grundläggningsnivå
            // Fundamentets effektiva bredd                                 [bef]

            // Bärighetsfaktorer                                            [N]

            // Korrektionsfaktorer för avvikelser från de förutsättningar   [ξ]
            // under vilka bärighetsfaktorerna framtagits

            // Korrektionsfaktorer består av följande inverkan:
            // Inverkan av hållfasthet hos jorden över grundläggningsnivå   [d]
            // Inverkan av fundamentform                                    [s]
            // Inverkan av lutande last                                     [i]
            // Inverkan av lutande intilliggande markyta                    [g]
            // Inverkan av lutande basyta på fundament                      [b]

            // Predefinition from input data
            // slabs
            Slab SlabBPL = slabs[0];
            var P_BPL = SlabBPL.SlabPart.Region.Contours[0].Points;

            // Variables
            double γM_dr;
            double ηdr_18;
            double φ_eff;
            double φd;

            double γM_odr;
            double ηodr_18;
            double cuk;
            double cud;
            double sc;
            double ic;
            double qd;
            double sq;
            double iq;
            double γeq;
            double Nγd;
            double sγ;
            double iγ;

            // Lists

            var LoadComb_List = new List<String>();
            var γM_dr_List = new List<double>();
            var ηdr_18_List = new List<double>();
            var φ_eff_List = new List<double>();
            var φd_List = new List<double>();
            var γM_odr_List = new List<double>();
            var ηodr_18_List = new List<double>();
            var cuk_List = new List<double>();
            var cud_List = new List<double>();
            var dF_List = new List<double>();
            var dW_List = new List<double>();
            var α_List = new List<double>();
            var β_List = new List<double>();

            var Kontroll_Metod_List = new List<Boolean>();
            var γfyll_List = new List<double>();
            var γ_fyll_List = new List<double>();
            var γuK_BPL_List = new List<double>();
            var γ_uK_BPL_List = new List<double>();
            var Meterstrimla_List = new List<Boolean>();
            var bx_List = new List<double>();
            var ly_List = new List<double>();

            var Fv_List = new List<double>();
            var FHx_List = new List<double>();
            var FHy_List = new List<double>();
            var FH_ed_List = new List<double>();
            var Mx_List = new List<double>();
            var My_List = new List<double>();
            var ex_List = new List<double>();
            var ey_List = new List<double>();
            var θ_List = new List<double>();
            var bef_List = new List<double>();
            var lef_List = new List<double>();
            var Aef_List = new List<double>();

            var mb_List = new List<double>();
            var ml_List = new List<double>();
            var m_List = new List<double>();

            var qd_List = new List<double>();
            var Nqd_List = new List<double>();
            var dq_List = new List<double>();
            var sq_List = new List<double>();
            var iq_List = new List<double>();
            var gq_List = new List<double>();
            var bq_List = new List<double>();
            var ξq_List = new List<double>();
            var qbq_List = new List<double>();

            var Ncd_List = new List<double>();
            var dc_List = new List<double>();
            var sc_List = new List<double>();
            var ic_List = new List<double>();
            var gc_List = new List<double>();
            var bc_List = new List<double>();
            var ξc_List = new List<double>();
            var qbc_List = new List<double>();

            var γeq_List = new List<double>();
            var Nγd_List = new List<double>();
            var dγ_List = new List<double>();
            var sγ_List = new List<double>();
            var iγ_List = new List<double>();
            var gγ_List = new List<double>();
            var bγ_List = new List<double>();
            var ξγ_List = new List<double>();
            var qbγ_List = new List<double>();

            var qb_rd_List = new List<double>();
            var qb_ed_List = new List<double>();
            
            var Ug_B_List = new List<double>();
            Boolean Kontroll_Metod;

            // ----- INDATA -----  
            // Dränerad/Odränerad analys
            // Kontrollera att antalet i listan stämmer med antalet lastkombinationer
            var DränFörhållande = new List<Boolean>
                {
                    //true,       // 1
                    //true,       // 2
                    //true,       // 3
                    //true,       // 4
                    //true,       // 5
                    //true,       // 6
                    //true,       // 7
                    //false,      // 8
                    //false,      // 9
                    //false,      // 10
                    //false,      // 11
                    //false,      // 12
                    //false,      // 13
                    //false,      // 14
                    true,       // 1
                    true,       // 2
                    false,      // 3
                    false,      // 4
                };

            for (int i = 0; i < SupportResultant_B.Count; i++)
            {

                // Minesregel för c# logik
                // Värde = vilkor ? Värde om vilkor är sant : Värde om vilkor är falskt

                // Dränerade egenskaper
                γM_dr = (DränFörhållande[i])   ? 1.3  :   1.3;
                ηdr_18  =  1.05;                          
                φ_eff   = (DränFörhållande[i]) ? 37   :   0;
                                                          
                // Odränerade egenskaper                  
                γM_odr  = (DränFörhållande[i]) ? 1.3  :   1.5;
                ηodr_18 =  0.8;                          
                cuk     = (DränFörhållande[i]) ? 0.00 :   300;
                
                // Geoteknisk indata
                double dF              = 0.8;
                double dW              = 0.0;
                double α               = 0.0;
                double β               = 10;
                
                double γfyll           = 17.5;
                double γ_fyll          = 10.5;
                double γuK_BPL         = 17.5;
                double γ_uK_BPL        = 10.5;

                Boolean Meterstrimla = false;

                // ------------------

                φd = Math.Atan(Math.Tan((φ_eff * Math.PI / 180)) / γM_dr * ηdr_18) * 180 / Math.PI;
                cud = cuk / γM_odr * ηodr_18;
                if (DränFörhållande[i]==true)
                {
                    Kontroll_Metod = (β <= (2.0 / 3.0) * φd) ? true : false;
                }
                else
                {
                    Kontroll_Metod = true;
                }

                double bx = SlabBPL.SlabPart.Region.Contours[0].Edges[0].Length;
                double ly = SlabBPL.SlabPart.Region.Contours[0].Edges[1].Length;

                // Lastfall
                double Fv       = Math.Abs(SupportResultant_B[i].Fz);
                double FHx      = Math.Abs(SupportResultant_B[i].Fx);
                double FHy      = Math.Abs(SupportResultant_B[i].Fy);
                double FH_ed    = Math.Sqrt(Math.Pow(FHx, 2) + Math.Pow(FHy, 2)); 
                double Mx       = Math.Abs(SupportResultant_B[i].Mx);
                double My       = Math.Abs(SupportResultant_B[i].My);
                double ex       =  My / Fv;
                double ey       =  Mx / Fv;
                double θ        = (FHy == 0) ? 90 : Math.Atan(FHx / FHy) * (Math.PI/180);
                double bef      = bx - 2 * ex;
                double lef      = ly - 2 * ey; // OBS! lef > bef
                double Aef      = bef * lef;

                if (bef > lef && Meterstrimla == false)
                {
                    (bef, lef) = (lef, bef);
                }

                // Omvandling av grader till radianer
                double φd_rad = φd * Math.PI / 180;
                double α_rad  = α  * Math.PI / 180;
                double β_rad  = β  * Math.PI / 180;
                double θ_rad  = θ  * Math.PI / 180;

                if (Kontroll_Metod==true )
                {
                    
                    double mb = (Meterstrimla == true) ? 2 : (2 * lef + bef) / (lef + bef);
                    double ml = (Meterstrimla == true) ? 2 : (2 * bef + lef) / (lef + bef);
                    double m  = ml * Math.Pow(Math.Cos(θ_rad), 2) + mb * Math.Pow(Math.Sin(θ_rad), 2);

                    // (2) Bärighet från överlagringstryck
                    
                    if (DränFörhållande[i] == true)
                    {
                        qd = (dW > 0) ? γfyll * dF : (γfyll * Math.Max(dF + dW, 0) + γ_fyll * -dW);
                    }

                    else
                    {
                        qd = γ_fyll * dF;
                    }

                    double Nqd = (1 + Math.Sin(φd_rad)) / (1 - Math.Sin(φd_rad)) * Math.Exp(Math.PI * Math.Tan(φd_rad));
                    double dq  = Math.Min(1 + 0.35 * dF / bef, 1.7);

                    if (Meterstrimla)
                    {
                        sq = 1;
                        iq = 1;
                    }
                    else
                    {
                        sq = 1 + Math.Tan(φd_rad) * bef / lef;
                        iq = (φd == 0) ? 1 : Math.Pow((1 - FH_ed / (Fv + bef * lef * cud * (1 / Math.Tan(φd_rad)))), m);
                    }
                        
                    double gq  = 1 - Math.Sin(2 * β_rad);
                    double bq  = Math.Pow(1 - α_rad * Math.Tan(φd_rad), 2);
                    double ξq  = dq * sq * iq * gq * bq;

                    double qbq = qd * Nqd * ξq;

                    // (1) Bärighet från jordens kohesion
                    double Ncd = (φd == 0) ? Math.PI + 2 : (Nqd - 1) * (1 / Math.Tan(φd_rad));
                    double dc  = Math.Min(1 + 0.35 * dF / bef, 1.7);
                    if (Meterstrimla)
                    {
                        sc = 1;
                        ic = 1;
                    }
                    else
                    {
                        sc = (φd == 0) ? 1 + 0.2 * bef / lef : 1 + (Nqd * bef) / (Ncd * lef);
                        ic = (φd == 0) ? 1 - (m * FH_ed) / (bef * lef * cud * Ncd) : iq - (1 - iq) / (Ncd * Math.Tan(φd_rad));
                    }
                                        
                    double gc  = (φd == 0) ? 1 - 2 * β_rad / Ncd : Math.Exp(-2 * β_rad * Math.Tan(φd_rad));
                    double bc  = (φd == 0) ? 1 - (2 * α_rad) / (Math.PI + 2) : bq - (1 - bq) / (Ncd * Math.Tan(φd_rad));
                    double ξc  = dc * sc * ic * gc * bc;
                    
                      double qbc = cud * Ncd * ξc;

                    // (3) Bärighet från jordens tunghet
                    
                    if (dW >= 0)
                    {
                        γeq = (dW > bef) ? γuK_BPL : γuK_BPL * dW / bef + γ_uK_BPL * (bef - dW) / bef;
                    }
                    else
                    {
                        γeq = γ_uK_BPL;
                    }

                    if (DränFörhållande[i])
                    {
                        Nγd = (φd == 0) ? 0 : (0.08705 + 0.3231 * Math.Sin(2 * φd_rad) - 0.04836 * Math.Pow(Math.Sin(2 * φd_rad), 2)) * ((1 + Math.Sin(φd_rad)) / (1 - Math.Sin(φd_rad)) * Math.Exp(3 * Math.PI / 2 * Math.Tan(φd_rad)) - 1);
                    }
                    else
                    {
                        Nγd = (φd == 0) ? 0 : -2 * Math.Sin(β_rad) ;
                    }
                    

                    double dγ  = 1;
                    if (Meterstrimla)
                    {
                        sγ = 1;
                        iγ = 1;
                    }
                    else
                    {
                        sγ = 1 - 0.4 * bef / lef;
                        iγ = (φd == 0) ? 1 : Math.Pow((1 - FH_ed / (Fv + bef * lef * cud * (1 / Math.Tan(φd_rad)))), (m + 1));
                    }
                    double gγ  = 1 - Math.Sin(2 * β_rad);
                    double bγ  = Math.Pow(1 - α_rad * Math.Tan(φd_rad), 2);
                    double ξγ  = dγ * sγ * iγ * gγ * bγ;
                    
                    double qbγ = 0.5 * γeq * bef * Nγd * ξγ;
                    
                    // Bärighetskapacitet
                    double qb_rd = qbc + qbq + qbγ;
                    double qb_ed = Fv / Aef;
                    double Ug_B  = qb_ed/ qb_rd;

                    // Adding Ug to list

                    qb_rd_List.Add(qb_rd);
                    Ug_B_List.Add(Ug_B);

                    if (KontrollUtskrift)
                    {
                        LoadComb_List.Add(SupportResultant_B[i].CaseIdentifier);
                        γM_dr_List.Add(γM_dr);
                        ηdr_18_List.Add(ηdr_18);
                        φ_eff_List.Add(φ_eff);
                        φd_List.Add(φd);
                        γM_odr_List.Add(γM_odr);
                        ηodr_18_List.Add(ηodr_18);
                        cuk_List.Add(cuk);
                        cud_List.Add(cud);
                        dF_List.Add(dF);
                        dW_List.Add(dW);
                        α_List.Add(α);
                        β_List.Add(β);
                        Kontroll_Metod_List.Add(Kontroll_Metod);
                        γfyll_List.Add(γfyll);
                        γ_fyll_List.Add(γ_fyll);
                        γuK_BPL_List.Add(γuK_BPL);
                        γ_uK_BPL_List.Add(γ_uK_BPL);
                        Meterstrimla_List.Add(Meterstrimla);
                        bx_List.Add(bx);
                        ly_List.Add(ly);
                        Fv_List.Add(Fv);
                        FHx_List.Add(FHx);
                        FHy_List.Add(FHy);
                        FH_ed_List.Add(FH_ed);
                        Mx_List.Add(Mx);
                        My_List.Add(My);
                        ex_List.Add(ex);
                        ey_List.Add(ey);
                        θ_List.Add(θ);
                        bef_List.Add(bef);
                        lef_List.Add(lef);
                        Aef_List.Add(Aef);
                        mb_List.Add(mb);
                        ml_List.Add(ml);
                        m_List.Add(m);
                        qd_List.Add(qd);
                        Nqd_List.Add(Nqd);
                        dq_List.Add(dq);
                        sq_List.Add(sq);
                        iq_List.Add(iq);
                        gq_List.Add(gq);
                        bq_List.Add(bq);
                        ξq_List.Add(ξq);
                        qbq_List.Add(qbq);
                        Ncd_List.Add(Ncd);
                        dc_List.Add(dc);
                        sc_List.Add(sc);
                        ic_List.Add(ic);
                        gc_List.Add(gc);
                        bc_List.Add(bc);
                        ξc_List.Add(ξc);
                        qbc_List.Add(qbc);
                        γeq_List.Add(γeq);
                        Nγd_List.Add(Nγd);
                        dγ_List.Add(dγ);
                        sγ_List.Add(sγ);
                        iγ_List.Add(iγ);
                        gγ_List.Add(gγ);
                        bγ_List.Add(bγ);
                        ξγ_List.Add(ξγ);
                        qbγ_List.Add(qbγ);
                        qb_ed_List.Add(qb_ed);
                        
                    }
                }
                else
                {
                    Console.WriteLine("Allmäna bärighetsekvationen ej giltig. Bärighet bör beräknas med en glidyta beräkning");
                }
                
            }
            double qb_rd_ULS = qb_rd_List.Min();
            if (KontrollUtskrift)
            {
                Console.WriteLine(" LoadCombination name = " + "; " + string.Join("; ", LoadComb_List));
                Console.WriteLine("γM_dr  = " + "; " + string.Join("; ", γM_dr_List));
                Console.WriteLine("ηdr_18  = " + "; " + string.Join("; ", ηdr_18_List));
                Console.WriteLine("φ_eff  = " + "; " + string.Join("; ", φ_eff_List));
                Console.WriteLine("φd  = " + "; " + string.Join("; ", φd_List));
                Console.WriteLine("γM_odr  = " + "; " + string.Join("; ", γM_odr_List));
                Console.WriteLine("ηodr_18  = " + "; " + string.Join("; ", ηodr_18_List));
                Console.WriteLine("cuk  = " + "; " + string.Join("; ", cuk_List));
                Console.WriteLine("cud  = " + "; " + string.Join("; ", cud_List));
                Console.WriteLine("dF  = " + "; " + string.Join("; ", dF_List));
                Console.WriteLine("dW  = " + "; " + string.Join("; ", dW_List));
                Console.WriteLine("α  = " + "; " + string.Join("; ", α_List));
                Console.WriteLine("β  = " + "; " + string.Join("; ", β_List));
                Console.WriteLine("Kontroll_Metod  = " + "; " + string.Join("; ", Kontroll_Metod_List));
                Console.WriteLine("γfyll  = " + "; " + string.Join("; ", γfyll_List));
                Console.WriteLine("γ_fyll  = " + "; " + string.Join("; ", γ_fyll_List));
                Console.WriteLine("γuK_BPL  = " + "; " + string.Join("; ", γuK_BPL_List));
                Console.WriteLine("γ_uK_BPL  = " + "; " + string.Join("; ", γ_uK_BPL_List));
                Console.WriteLine("Meterstrimla  = " + "; " + string.Join("; ", Meterstrimla_List));
                Console.WriteLine("bx  = " + "; " + string.Join("; ", bx_List));
                Console.WriteLine("ly  = " + "; " + string.Join("; ", ly_List));
                Console.WriteLine("Fv  = " + "; " + string.Join("; ", Fv_List));
                Console.WriteLine("FHx  = " + "; " + string.Join("; ", FHx_List));
                Console.WriteLine("FHy  = " + "; " + string.Join("; ", FHy_List));
                Console.WriteLine("FH_ed  = " + "; " + string.Join("; ", FH_ed_List));
                Console.WriteLine("Mx  = " + "; " + string.Join("; ", Mx_List));
                Console.WriteLine("My  = " + "; " + string.Join("; ", My_List));
                Console.WriteLine("ex  = " + "; " + string.Join("; ", ex_List));
                Console.WriteLine("ey  = " + "; " + string.Join("; ", ey_List));
                Console.WriteLine("θ  = " + "; " + string.Join("; ", θ_List));
                Console.WriteLine("bef  = " + "; " + string.Join("; ", bef_List));
                Console.WriteLine("lef  = " + "; " + string.Join("; ", lef_List));
                Console.WriteLine("Aef  = " + "; " + string.Join("; ", Aef_List));
                Console.WriteLine("mb  = " + "; " + string.Join("; ", mb_List));
                Console.WriteLine("ml  = " + "; " + string.Join("; ", ml_List));
                Console.WriteLine("m  = " + "; " + string.Join("; ", m_List));
                Console.WriteLine("qd  = " + "; " + string.Join("; ", qd_List));
                Console.WriteLine("Nqd  = " + "; " + string.Join("; ", Nqd_List));
                Console.WriteLine("dq  = " + "; " + string.Join("; ", dq_List));
                Console.WriteLine("sq  = " + "; " + string.Join("; ", sq_List));
                Console.WriteLine("iq  = " + "; " + string.Join("; ", iq_List));
                Console.WriteLine("gq  = " + "; " + string.Join("; ", gq_List));
                Console.WriteLine("bq  = " + "; " + string.Join("; ", bq_List));
                Console.WriteLine("ξq  = " + "; " + string.Join("; ", ξq_List));
                Console.WriteLine("qbq  = " + "; " + string.Join("; ", qbq_List));
                Console.WriteLine("Ncd  = " + "; " + string.Join("; ", Ncd_List));
                Console.WriteLine("dc  = " + "; " + string.Join("; ", dc_List));
                Console.WriteLine("sc  = " + "; " + string.Join("; ", sc_List));
                Console.WriteLine("ic  = " + "; " + string.Join("; ", ic_List));
                Console.WriteLine("gc  = " + "; " + string.Join("; ", gc_List));
                Console.WriteLine("bc  = " + "; " + string.Join("; ", bc_List));
                Console.WriteLine("ξc  = " + "; " + string.Join("; ", ξc_List));
                Console.WriteLine("qbc  = " + "; " + string.Join("; ", qbc_List));
                Console.WriteLine("γeq  = " + "; " + string.Join("; ", γeq_List));
                Console.WriteLine("Nγd  = " + "; " + string.Join("; ", Nγd_List));
                Console.WriteLine("dγ  = " + "; " + string.Join("; ", dγ_List));
                Console.WriteLine("sγ  = " + "; " + string.Join("; ", sγ_List));
                Console.WriteLine("iγ  = " + "; " + string.Join("; ", iγ_List));
                Console.WriteLine("gγ  = " + "; " + string.Join("; ", gγ_List));
                Console.WriteLine("bγ  = " + "; " + string.Join("; ", bγ_List));
                Console.WriteLine("ξγ  = " + "; " + string.Join("; ", ξγ_List));
                Console.WriteLine("qbγ  = " + "; " + string.Join("; ", qbγ_List));
                Console.WriteLine("qb_rd  = " + "; " + string.Join("; ", qb_rd_List));
                Console.WriteLine("qb_ed  = " + "; " + string.Join("; ", qb_ed_List));
                Console.WriteLine("Ug_b  = " + "; " + string.Join("; ", Ug_B_List));
            }
                return (Ug_B_List, qb_rd_ULS);
        }
        public static List<double> Glidning(List<SurfaceSupportResultant> SupportResultant_G, List<Slab> slabs, Boolean KontrollUtskrift)
        {
            // Predefinition from input data
            // slabs

            Slab SlabBPL = slabs[0];
            var P_BPL = SlabBPL.SlabPart.Region.Contours[0].Points;

            // Variables
            double γM_dr;
            double ηdr_18;
            double φ_eff;
            double φd;

            double γM_odr;
            double ηodr_18;
            double cuk;
            double cud;

            // Lists
            var LoadComb_List = new List<String>();
            var γM_dr_List      = new List<double>();
            var ηdr_18_List     = new List<double>();
            var φ_eff_List      = new List<double>();
            var φd_List         = new List<double>();
            var γM_odr_List     = new List<double>();
            var ηodr_18_List    = new List<double>();
            var cuk_List        = new List<double>();
            var cud_List        = new List<double>();
            var Platttyp_List   = new List<String>();
            var bx_List         = new List<double>();
            var ly_List         = new List<double>();
            var Fv_List         = new List<double>();
            var FHx_List        = new List<double>();
            var FHy_List        = new List<double>();
            var FH_ed_List      = new List<double>();
            var Mx_List         = new List<double>();
            var My_List         = new List<double>();
            var ex_List         = new List<double>();
            var ey_List         = new List<double>();
            var bef_List        = new List<double>();
            var lef_List        = new List<double>();
            var Aef_List        = new List<double>();
            var δd_List         = new List<double>();
            var FH_drän_List    = new List<double>();
            var FH_odrän_List   = new List<double>();
            var FH_rd_List      = new List<double>();

            var Ug_G_List       = new List<double>();

            // ----- INDATA -----
            // Dränerad/Odränerad analys
            // Kontrollera att antalet i listan stämmer med antalet lastkombinationer
            var DränFörhållande = new List<Boolean>
                {
                    //true,       // 1
                    //true,       // 2
                    //true,       // 3
                    //true,       // 4
                    //true,       // 5
                    //true,       // 6
                    //true,       // 7
                    //false,      // 8
                    //false,      // 9
                    //false,      // 10
                    //false,      // 11
                    //false,      // 12
                    //false,      // 13
                    //false,      // 14
                    true,       // 1
                    true,       // 2
                    false,      // 3
                    false,      // 4
                };

            String Platttyp = "PLATSGJUTEN"; // PLATSGJUTEN/FÖRTILLVERKAD

            for (int i = 0; i < SupportResultant_G.Count; i++)
            {

                // Dränerade egenskaper
                γM_dr = (DränFörhållande[i]) ? 1.3 : 1.3;
                ηdr_18 = 1.05;
                φ_eff = (DränFörhållande[i]) ? 37 : 0;

                // Odränerade egenskaper                  
                γM_odr = (DränFörhållande[i]) ? 1.3 : 1.5;
                ηodr_18 = 0.8;
                cuk = (DränFörhållande[i]) ? 0 : 300;

                // ------------------

                φd = Math.Atan(Math.Tan((φ_eff * Math.PI / 180)) / γM_dr * ηdr_18) * 180 / Math.PI;
                cud = cuk / γM_odr * ηodr_18;

                double bx = SlabBPL.SlabPart.Region.Contours[0].Edges[0].Length;
                double ly = SlabBPL.SlabPart.Region.Contours[0].Edges[1].Length;

                // Lastfall
                double Fv =  Math.Abs(SupportResultant_G[i].Fz);
                double FHx = Math.Abs(SupportResultant_G[i].Fx);
                double FHy = Math.Abs(SupportResultant_G[i].Fy);
                double FH_ed = Math.Sqrt(Math.Pow(FHx, 2) + Math.Pow(FHy, 2));

                double Mx = Math.Abs(SupportResultant_G[i].Mx);
                double My = Math.Abs(SupportResultant_G[i].My);
                double ex =  My / Fv;
                double ey =  Mx / Fv;
                double bef = bx - 2 * ex;
                double lef = ly - 2 * ey; // OBS! lef > bef
                double Aef = bef * lef;

                double δd = (Platttyp == "PLATSGJUTEN") ? φd : φd * 2.0 / 3.0;

                // Omvandling av grader till radianer
                double δd_rad = δd * Math.PI / 180;

                // Glidkapacitet
                double FH_drän =  Fv * Math.Tan(δd_rad);
                double FH_odrän = Math.Min(Aef * cud, 0.4 * Fv);
                double FH_rd = (DränFörhållande[i]) ? FH_drän : FH_odrän;
                double Ug_G = FH_ed / FH_rd;

                // // Adding Ug to list 
                Ug_G_List.Add(Ug_G);

                if (KontrollUtskrift)
                {
                    LoadComb_List.Add(SupportResultant_G[i].CaseIdentifier);
                    γM_dr_List.Add(γM_dr);
                    ηdr_18_List.Add(ηdr_18);
                    φ_eff_List.Add(φ_eff);
                    φd_List.Add(φd);
                    γM_odr_List.Add(γM_odr);
                    ηodr_18_List.Add(ηodr_18);
                    cuk_List.Add(cuk);
                    cud_List.Add(cud);
                    Platttyp_List.Add(Platttyp);
                    bx_List.Add(bx);
                    ly_List.Add(ly);
                    Fv_List.Add(Fv);
                    FHx_List.Add(FHx);
                    FHy_List.Add(FHy);
                    FH_ed_List.Add(FH_ed);
                    Mx_List.Add(Mx);
                    My_List.Add(My);
                    ex_List.Add(ex);
                    ey_List.Add(ey);
                    bef_List.Add(bef);
                    lef_List.Add(lef);
                    Aef_List.Add(Aef);
                    δd_List.Add(δd);
                    FH_drän_List.Add(FH_drän);
                    FH_odrän_List.Add(FH_odrän);
                    FH_rd_List.Add(FH_rd);
                }
            }

            if (KontrollUtskrift)
            {
             Console.WriteLine("LoadCombination name  = " + "; " + string.Join("; ", LoadComb_List));
             Console.WriteLine("γM_dr  = " + "; " + string.Join("; ", γM_dr_List));
             Console.WriteLine("ηdr_18  = " + "; " + string.Join("; ", ηdr_18_List));
             Console.WriteLine("φ_eff  = " + "; " + string.Join("; ", φ_eff_List));
             Console.WriteLine("φd  = " + "; " + string.Join("; ", φd_List));
             Console.WriteLine("γM_odr  = " + "; " + string.Join("; ", γM_odr_List));
             Console.WriteLine("ηodr_18  = " + "; " + string.Join("; ", ηodr_18_List));
             Console.WriteLine("cuk  = " + "; " + string.Join("; ", cuk_List));
             Console.WriteLine("cud  = " + "; " + string.Join("; ", cud_List));
             Console.WriteLine("Platttyp  = " + "; " + string.Join("; ", Platttyp_List));
             Console.WriteLine("bx  = " + "; " + string.Join("; ", bx_List));
             Console.WriteLine("ly  = " + "; " + string.Join("; ", ly_List));
             Console.WriteLine("Fv  = " + "; " + string.Join("; ", Fv_List));
             Console.WriteLine("FHx  = " + "; " + string.Join("; ", FHx_List));
             Console.WriteLine("FHy  = " + "; " + string.Join("; ", FHy_List));
             Console.WriteLine("FH_ed  = " + "; " + string.Join("; ", FH_ed_List));
             Console.WriteLine("Mx  = " + "; " + string.Join("; ", Mx_List));
             Console.WriteLine("My  = " + "; " + string.Join("; ", My_List));
             Console.WriteLine("ex  = " + "; " + string.Join("; ", ex_List));
             Console.WriteLine("ey  = " + "; " + string.Join("; ", ey_List));
             Console.WriteLine("bef  = " + "; " + string.Join("; ", bef_List));
             Console.WriteLine("lef  = " + "; " + string.Join("; ", lef_List));
             Console.WriteLine("Aef  = " + "; " + string.Join("; ", Aef_List));
             Console.WriteLine("δd  = " + "; " + string.Join("; ", δd_List));
             Console.WriteLine("FH_drän  = " + "; " + string.Join("; ", FH_drän_List));
             Console.WriteLine("FH_odrän  = " + "; " + string.Join("; ", FH_odrän_List));
             Console.WriteLine("FH_rd  = " + "; " + string.Join("; ", FH_rd_List));
             Console.WriteLine("Ug_g  = " + "; " + string.Join("; ", Ug_G_List));
            }

                return Ug_G_List;
        }
        public static List<double> Stjälpning(List<SurfaceSupportResultant> SupportResultant_S, List<Slab> slabs, Boolean KontrollUtskrift)
        {
            // Predefinition from input data
            // slabs

            Slab SlabBPL = slabs[0];
            var P_BPL = SlabBPL.SlabPart.Region.Contours[0].Points;

            // Lists
            var LoadComb_List = new List<String>();
            var bx_List      = new List<double>();
            var Fv_List      = new List<double>();
            var My_List      = new List<double>();
            var ex_List      = new List<double>();
            var bef_List     = new List<double>();
            var bef_min_List = new List<double>();
            
            var Ug_S_List = new List<double>();

            for (int i = 0; i < SupportResultant_S.Count; i++)
            {

                Boolean Meterstrimla = true;

                double bx = SlabBPL.SlabPart.Region.Contours[0].Edges[0].Length;
                double ly = SlabBPL.SlabPart.Region.Contours[0].Edges[1].Length;

                // Lastfall
                double Fv = Math.Abs(SupportResultant_S[i].Fz);
                double Mx = Math.Abs(SupportResultant_S[i].Mx);
                double My = Math.Abs(SupportResultant_S[i].My);
                double ex = My / Fv;
                double ey = Mx / Fv;
                double bef = bx - 2 * ex;
                double lef = ly - 2 * ey; // OBS! lef > bef


                if (bef > lef && Meterstrimla == false)
                {
                    (bef, lef) = (lef, bef);
                }

                double bef_min = Math.Max(bx / 3, 0.6);
                double Ug_S = bef_min / bef;

                // Adding Ug to list
                Ug_S_List.Add(Ug_S);

                if (KontrollUtskrift)
                {
                    LoadComb_List.Add(SupportResultant_S[i].CaseIdentifier);
                    bx_List.Add(bx);
                    Fv_List.Add(Fv);
                    My_List.Add(My);
                    ex_List.Add(ex);
                    bef_List.Add(bef);
                    bef_min_List.Add(bef_min);
                }
            }
            
            if (KontrollUtskrift)
            {
                Console.WriteLine("LoadCombination name  = " + "; " + string.Join("; ", LoadComb_List));
                Console.WriteLine("bx  = " + "; " + string.Join("; ", bx_List     ));
                Console.WriteLine("Fv  = " + "; " + string.Join("; ", Fv_List      ));
                Console.WriteLine("My  = " + "; " + string.Join("; ", My_List      ));
                Console.WriteLine("ex  = " + "; " + string.Join("; ", ex_List      ));
                Console.WriteLine("bef  = " + "; " + string.Join("; ", bef_List     ));
                Console.WriteLine("bef_min  = " + "; " + string.Join("; ", bef_min_List ));
                Console.WriteLine("Ug_S  = " + "; " + string.Join("; ", Ug_S_List));
            }


            return Ug_S_List;
        }
        public static  List<double> Krypdeformation( double qb_rd_ULS, List<SurfaceSupportResultant> SupportResultant_SLS, List<Slab> slabs, Boolean KontrollUtskrift)
        {

            // Predefinition from input data
            // slabs

            Slab SlabBPL = slabs[0];

            // Lists
            var LoadComb_List = new List<String>();
            var bx_List = new List<double>();
            var ly_List = new List<double>();
            var Fv_List = new List<double>();
            var Mx_List = new List<double>();
            var My_List = new List<double>();
            var ex_List = new List<double>();
            var ey_List = new List<double>();
            var bef_List = new List<double>();
            var lef_List = new List<double>();
            var Aef_List = new List<double>();

            var qb_rd_SLS_List = new List<double>();
            var qb_ed_List = new List<double>();
            var Ug_SLS_List = new List<double>();

            double qb_rd_SLS = (2.0 / 3.0) * qb_rd_ULS;

            double bx = SlabBPL.SlabPart.Region.Contours[0].Edges[0].Length;
            double ly = SlabBPL.SlabPart.Region.Contours[0].Edges[1].Length;

            
            for (int i = 0; i < SupportResultant_SLS.Count; i++) 
            {
                // Lastfall
                double Fv = Math.Abs(SupportResultant_SLS[i].Fz);
                double Mx = Math.Abs(SupportResultant_SLS[i].Mx);
                double My = Math.Abs(SupportResultant_SLS[i].My);
                double ex = My / Fv;
                double ey = Mx / Fv;
                double bef = bx - 2 * ex;
                double lef = ly - 2 * ey; // OBS! lef > bef
                double Aef = bef * lef;

                var qb_ed = Fv / Aef;
                double Ug_SLS = qb_ed / qb_rd_SLS;

                // Adding Ug to list
                Ug_SLS_List.Add(Ug_SLS);

                if (KontrollUtskrift)
                {
                    LoadComb_List.Add(SupportResultant_SLS[i].CaseIdentifier);
                    bx_List.Add(bx);
                    ly_List.Add(ly);
                    Fv_List.Add(Fv);
                    Mx_List.Add(Mx);
                    My_List.Add(My);
                    ex_List.Add(ex);
                    ey_List.Add(ey);
                    bef_List.Add(bef);
                    lef_List.Add(lef);
                    Aef_List.Add(Aef);
                    qb_rd_SLS_List.Add(qb_rd_SLS);
                    qb_ed_List.Add(qb_ed);
                }
            }
            if (KontrollUtskrift)
            {
                Console.WriteLine("LoadCombination name  = " + "; " + string.Join("; ", LoadComb_List));
                Console.WriteLine("bx  = " + "; " + string.Join("; ", bx_List));
                Console.WriteLine("ly  = " + "; " + string.Join("; ", ly_List));
                Console.WriteLine("Fv  = " + "; " + string.Join("; ", Fv_List));
                Console.WriteLine("Mx  = " + "; " + string.Join("; ", Mx_List));
                Console.WriteLine("My  = " + "; " + string.Join("; ", My_List));
                Console.WriteLine("ex  = " + "; " + string.Join("; ", ex_List));
                Console.WriteLine("ey  = " + "; " + string.Join("; ", ey_List));
                Console.WriteLine("bef  = " + "; " + string.Join("; ", bef_List));
                Console.WriteLine("lef  = " + "; " + string.Join("; ", lef_List));
                Console.WriteLine("Aef  = " + "; " + string.Join("; ", Aef_List));
                Console.WriteLine("qb_rd_SLS  = " + "; " + string.Join("; ", qb_rd_SLS_List));
                Console.WriteLine("qb_ed  = " + "; " + string.Join("; ", qb_ed_List));
                Console.WriteLine("Ug_SLS  = " + "; " + string.Join("; ", Ug_SLS_List));
            }
            return Ug_SLS_List;
        }

    }
}
