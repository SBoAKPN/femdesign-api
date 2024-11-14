"""
Created with FemDesign API 0.0.6
"""

import os

from femdesign.comunication import FemDesignConnection, Verbosity
from femdesign.calculate.command import DesignModule
from femdesign.calculate.analysis import Analysis, Design, CombSettings, CombItem

def find_struxml_files(folder_path):
    struxml_files = []
    for root, dirs, files in os.walk(folder_path):
        for file in files:
            if file.endswith('.struxml'):
                struxml_files.append(os.path.abspath(os.path.join(root, file)))
    return struxml_files

# select file with .struxml extension
folder_path = os.path.join(os.getcwd(), 'struxml_files')
struxml_files = find_struxml_files(folder_path)


pipe = FemDesignConnection(minimized= False)

for file in struxml_files:
    try:
        pipe.Open(file)

        static_analysis = Analysis.StaticAnalysis()
        pipe.RunAnalysis(static_analysis)
        
        pipe.Save(file.replace(".struxml", ".str"))
    except Exception as err:
        pipe.KillProgramIfExists()
        raise err