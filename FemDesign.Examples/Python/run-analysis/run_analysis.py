"""
Created with FemDesign API 0.0.6
"""

from femdesign.comunication import FemDesignConnection, Verbosity
from femdesign.calculate.command import DesignModule
from femdesign.calculate.analysis import Analysis, Design, CombSettings, CombItem


pipe = FemDesignConnection(minimized= False)
try:
    pipe.Open(r"simple_beam.str")
    pipe.SetProjectDescription(project_name="Amazing project",
                            project_description="Created through Python",
                            designer="Marco Pellegrino Engineer",
                            signature="MP",
                            comment="Wish for the best",
                            additional_info={"italy": "amazing", "sweden": "amazing_too"})


    static_analysis = Analysis.StaticAnalysis()
    pipe.RunAnalysis(static_analysis)

    freq_analysis = Analysis.FrequencyAnalysis(num_shapes=5)
    pipe.RunAnalysis(freq_analysis)

    periodic_analysis = Analysis.PeriodicExcitation(deltat=0.01, tend=5, dampening=0, alpha=0, beta=0, ksi=5)
    pipe.RunAnalysis(periodic_analysis)

    excitation_analysis = Analysis.ExcitationForce(nres=5, tcend=20.0, method=0, alpha=0.000, beta=0.000, ksi=5.0)
    pipe.RunAnalysis(excitation_analysis)

    ground_acceleration_analysis = Analysis.GroundAcceleration(flevelspectra=1, dts=0.20, tsend=5.0, q=1.0, facc=1, nres=5, tcend=20.0, method=0, alpha=0.000, beta=0.000, ksi=5.0)
    pipe.RunAnalysis(ground_acceleration_analysis)

    

    pipe.Save(r"simple_beam_out_2.str")
    pipe.GenerateListTables(bsc_file=r"bsc\finite-elements-nodes.bsc",
                            csv_file=r"output\finite-elements-nodes.csv")
    pipe.GenerateListTables(bsc_file=r"bsc\quantity-estimation-steel.bsc",
                            csv_file=r"output\quantity-estimation-steel.csv")
except Exception as err:
    pipe.KillProgramIfExists()
    raise err