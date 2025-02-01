

<a href="https://strusoft.com/software/3d-structural-analysis-software-fem-design/" target="_blank">
    <p align="center">
        <img src="https://raw.githubusercontent.com/strusoft/femdesign-api/refs/heads/master/FemDesign.Core/Resources/icons/FemDesignAPI.png" alt="isolated" width="200" style="centre"/>
    </p>
</a>

# Description

FEM-Design is an advanced and intuitive structural analysis software. We support all aspects of your structural engineering requirements: from 3D modelling, design and finite element analysis (FEA) of concrete, steel, timber, composite, masonry and foundation structures. All calculations are performed to Eurocode standards, with some specific National annexes.

The quick and easy nature of FEM-Design makes it an ideal choice for all types of construction tasks, from single element design to global stability analysis of large buildings, making it the best practical program for structural engineers to use for their day to day tasks.


## Scope

The python package is mainly focus on [`fdscript`](https://femdesign-api-docs.onstrusoft.com/docs/advanced/fdscript) automation which will help you in automatise processes as running analysis, design and read results.

The construction of the `Database` object is currently out of scope as it is delegated to the users. `Database` is based on `xml` sintax and you can use library such as `xml.etree.ElementTree` to manipulate the file.

## Example

```python
from femdesign.comunication import FemDesignConnection, Verbosity
from femdesign.calculate.command import DesignModule
from femdesign.calculate.analysis import Analysis, Design, CombSettings, CombItem


pipe = FemDesignConnection()
try:
    pipe.SetVerbosity(Verbosity.SCRIPT_LOG_LINES)
    pipe.Open(r"simple_beam.str")

    static_analysis = Analysis.StaticAnalysis()
    pipe.RunAnalysis(static_analysis)

    pipe.RunAnalysis(Analysis.FrequencyAnalysis(num_shapes=5))
    pipe.Save(r"simple_beam_out_2.str")

    pipe.Exit()
except Exception as err:
    pipe.KillProgramIfExists()
    raise err
```

A wider list of examples can be found in [example](https://github.com/strusoft/femdesign-api/tree/master/FemDesign.Examples/Python)

## Documentation

https://femdesign-api-docs.onstrusoft.com/docs/intro