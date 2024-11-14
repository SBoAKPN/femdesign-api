"""
Created with FemDesign API 0.0.6
"""
import pandas as pd
from femdesign.comunication import FemDesignConnection


pipe = FemDesignConnection(minimized= True)

try:
    pipe.Open("model.str")

    pipe.GenerateListTables(r"bsc/comb-nodal-displacements_all.bsc")
    pipe.GenerateListTables(r"bsc/eigenfrequencies.bsc")
    pipe.GenerateListTables(r"bsc/quantity-estimation-concrete.bsc")

except Exception as err:
    pipe.KillProgramIfExists()
    raise err

# the results will be saved in the bsc folder as csv files
comb_disp_all = pd.read_csv("bsc/comb-nodal-displacements_all.csv")
eigen_freq = pd.read_csv("bsc/eigenfrequencies.csv")
concrete_quantity = pd.read_csv("bsc/quantity-estimation-concrete.csv")

print(comb_disp_all)
print(eigen_freq)
print(concrete_quantity)