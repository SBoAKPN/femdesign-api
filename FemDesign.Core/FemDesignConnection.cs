﻿using System;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;

using FemDesign;
using FemDesign.Calculate;
using FemDesign.Bars;
using System.Globalization;
using FemDesign.Results;
using System.Text.RegularExpressions;
using System.Text;

namespace FemDesign
{
    /// <summary>
    /// FEM-Design real-time connection. Use this to open a <see cref="Model"/>, run <see cref="Analysis"/>, <see cref="Design"/>, extract results and more.
    /// </summary>
    public class FemDesignConnection : IDisposable
    {
        private readonly PipeConnection _connection;
        private readonly Process _process;
        public bool HasExited { get; private set; }
        public bool IsConnected => _connection._inputPipe.IsConnected;
        public bool IsDisconnected => !IsConnected;

        /// <summary>
        /// File path of th open file in the current Pipe.
        /// WARNING
        /// If a user manually open a file in a pipe instance, the file path location will be equal to null.
        /// </summary>
        private string CurrentOpenModel;
        public Verbosity Verbosity { get; private set; }
        public const Verbosity DefaultVerbosity = Verbosity.ScriptLogLinesOnly;

        /// <summary>
        /// Keep FEM-Design open after <see cref="Dispose"/> is called.
        /// </summary>
        private bool _keepOpen;


        public delegate void OnOutputEvent(string output);
        /// <summary>
        /// Occurs whenever FEM-Design writes a new log message.
        /// 
        /// Verbosity may be adjusted using <see cref="SetVerbosity(Verbosity)"/>
        /// </summary>
        public OnOutputEvent OnOutput { get; set; } = null;

        /// <summary>
        /// Open a new instance of FEM-Design and connect to it.
        /// </summary>
        /// <param name="fdInstallationDir"></param>
        /// <param name="minimized">Open FEM-Design as a minimized window.</param>
        /// <param name="keepOpen">If true FEM-Design will be left open and have to be manually exited.</param>
        /// <param name="outputDir">The directory to save script files. If set to null, the files will be will be written to a temporary directory and deleted after.</param>
        /// <param name="tempOutputDir"><code>BE CAREFUL!</code>If true the <paramref name="outputDir"/> will be deleted on exit. This option has no effect unless <paramref name="outputDir"/> has been specified.</param>
        /// <param name="verbosity"></param>

        public FemDesignConnection(
            string fdInstallationDir = @"C:\Program Files\StruSoft\FEM-Design 23\",
            bool minimized = false,
            bool keepOpen = false,
            string outputDir = null,
            bool tempOutputDir = false,
            Verbosity verbosity = DefaultVerbosity)
        {
            string pathToFemDesign = Path.Combine(fdInstallationDir, "fd3dstruct.exe");

            string pipeName = "FdPipe" + Guid.NewGuid().ToString();
            var startInfo = new ProcessStartInfo()
            {
                FileName = pathToFemDesign,
                Arguments = "/p " + pipeName,
                UseShellExecute = false,
                Verb = "open",
            };

            startInfo.EnvironmentVariables["FD_NOLOGO"] = "1";

            if (minimized)
            {
                startInfo.EnvironmentVariables["FD_NOGUI"] = "1";
            }

            OutputDir = outputDir;
            if (tempOutputDir)
                _outputDirsToBeDeleted.Add(OutputDir);

            _keepOpen = keepOpen;

            _connection = new PipeConnection(pipeName);
            try
            {
                _process = Process.Start(startInfo);
            }
            catch
            {
                throw new Exception(@"fd3dstruct.exe has not been found. `C:\Program Files\StruSoft\FEM-Design 23\` does not exist!");
            }

            _process.Exited += ProcessExited;
            _connection.WaitForConnection();

            // Forward all output messages from pipe (except echo guid commands).
            _connection.OnOutput += (message) => {
                message = message.Replace(">echo ", "");
                bool isGuid = Guid.TryParse(message, out Guid _);
                if (isGuid == false)
                    OnOutput?.Invoke(message);
            };
            SetVerbosity(verbosity);
        }




        private void ProcessExited(object sender, EventArgs e)
        {
            this.HasExited = true;
        }

        /// <summary>
        /// Disconnects the current connection. FEM-Design will be left open for normal usage.
        /// </summary>
        public void Disconnect()
        {
            this._connection.Send("detach"); // Tell FEM-Design to detach from the pipe
            this._connection.Disconnect();
        }

        /// <summary>
        /// FEM-Design will be left open, when the <see cref="FemDesignConnection"/> is disposed.
        /// </summary>
        public void KeepOpen()
        {
            _keepOpen = true;
        }

        /// <summary>
        /// Run a script and wait for it to finish.
        /// </summary>
        /// <param name="script"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void RunScript(FdScript script, string filename = "script")
        {
            if (script == null) throw new ArgumentNullException("script");

            string scriptPath = OutputFileHelper.GetFdScriptPath(OutputDir, filename);

            script.Serialize(scriptPath);
            this._connection.Send("run " + scriptPath);
            this._connection.WaitForCommandToFinish();
        }

        /// <summary>
        /// Run a script and wait for it to finish.
        /// </summary>
        /// <param name="script"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task RunScriptAsync(FdScript script, string filename = "script")
        {
            if (script == null) throw new ArgumentNullException("script");

            string scriptPath = OutputFileHelper.GetFdScriptPath(OutputDir, filename);

            script.Serialize(scriptPath);
            this._connection.Send("run " + scriptPath);
            await this._connection.WaitForCommandToFinishAsync();
        }

        /// <summary>
        /// Open a file in FEM-Design application.
        /// </summary>
        /// <param name="filePath">The model file to be opened. Typically a .str or .struxml file, but any filetype supported in FEM-Design is valid.</param>
        /// <param name="disconnect">Set to True to disconnect to the pipe and leave FEM-Design Open.</param>
        public void Open(string filePath, bool disconnect = false)
        {
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            this.RunScript(new FdScript(logfile, new CmdOpen(filePath)), "OpenModel");
            this.CurrentOpenModel = filePath;
            if (disconnect) this.Disconnect();
        }

        /// <summary>
        /// Open a file in FEM-Design application.
        /// </summary>
        /// <param name="filePath">The model file to be opened. Typically a .str or .struxml file, but any filetype supported in FEM-Design is valid.</param>
        public async Task OpenAsync(string filePath)
        {
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            await this.RunScriptAsync(new FdScript(logfile, new CmdOpen(filePath)));

        }

        /// <summary>
        /// Open a <see cref="Model"/> in FEM-Design application.
        /// </summary>
        /// <param name="model">Model to be opened.</param>
        /// <param name="disconnect">Set to True to disconnect to the pipe and leave FEM-Design Open.</param>
        public void Open(Model model, bool disconnect = false)
        {
            var struxml = OutputFileHelper.GetStruxmlPath(OutputDir);
            // Model must be serialized to a file to be opened in FEM-Design.
            model.SerializeModel(struxml);
            this.Open(struxml, disconnect);
        }

        public void SetGlobalConfig(Calculate.CmdGlobalCfg cmdglobalconfig)
        {
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            var script = new FdScript(
                logfile,
                cmdglobalconfig
            );
            this.RunScript(script, "SetGlobalConfig");
        }


        public void SetConfig(Calculate.CmdConfig cmdconfig)
        {
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            var script = new FdScript(
                logfile,
                cmdconfig
            );
            this.RunScript(script, "SetConfig");
        }


        /// <summary>
        /// Open a <see cref="Model"/> in FEM-Design application.
        /// </summary>
        /// <param name="model">Model to be opened.</param>
        public async Task OpenAsync(Model model)
        {
            var struxml = OutputFileHelper.GetStruxmlPath(OutputDir);
            // Model must be serialized to a file to be opened in FEM-Design.
            model.SerializeModel(struxml);
            await this.OpenAsync(struxml);
        }

        /// <summary>
        /// Runs an analysis task on the current model in FEM-Design.
        /// </summary>
        /// <param name="analysis">The analysis to be run. Defaults to static analysis (<see cref="Analysis.StaticAnalysis(Comb, bool, bool)"/>)</param>
        public void RunAnalysis(Analysis analysis)
        {
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            FdScript script;

            if (analysis.Comb.CombItem != null)
            {
                analysis.SetCombAnalysis(this);
                script = new FdScript(
                    logfile,
                    new CmdUser(CmdUserModule.RESMODE),
                    new CmdCalculation(analysis));
                this.RunScript(script, "RunAnalysis");
            }

            if (analysis.Stability != null)
            {
                analysis.SetStabilityAnalysis(this);
                script = new FdScript(
                    logfile,
                    new CmdUser(CmdUserModule.RESMODE),
                    new CmdCalculation(analysis));
                this.RunScript(script, "RunAnalysis");
            }

            if (analysis.Imperfection != null)
            {
                analysis.SetImperfectionAnalysis(this);
                script = new FdScript(
                    logfile,
                    new CmdUser(CmdUserModule.RESMODE),
                    new CmdCalculation(analysis));
                this.RunScript(script, "RunAnalysis");
            }

        }




        /// <summary>
        /// Opens <paramref name="model"/> in FEM-Design and runs the analysis.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="analysis"></param>
        public void RunAnalysis(Model model, Analysis analysis)
        {
            this.Open(model);
            this.RunAnalysis(analysis);
        }

        /// <summary>
        /// Runs a design task on the current model in FEM-Design.
        /// </summary>
        /// <param name="userModule"></param>
        /// <param name="design"></param>
        /// <param name="designGroups"></param>
        /// <exception cref="ArgumentException"></exception>
        public void RunDesign(CmdUserModule userModule, Design design, List<CmdDesignGroup> designGroups = null)
        {
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);

            #region DESIGN_GROUP

            if (designGroups != null && designGroups.Count != 0)
            {
                // delete previously define design group
                var cmdcommands = new List<CmdCommand>();

                foreach (var desGroup in CmdDesignGroup._designGroupCache)
                {
                    var emptyDesignGroup = new CmdDesignGroup(desGroup.Key, new List<FemDesign.GenericClasses.IStructureElement>(), desGroup.Value.Type);
                    cmdcommands.Add(emptyDesignGroup);
                }

                foreach (var desGroup in designGroups)
                {
                    cmdcommands.Add(desGroup);

                    if (!CmdDesignGroup._designGroupCache.ContainsKey(desGroup.Name))
                    {
                        CmdDesignGroup._designGroupCache[desGroup.Name] = desGroup;
                    }
                }

                var _script = new FdScript(logfile, cmdcommands);
                this.RunScript(_script, "DesignGroup");
            }
            else // delete previously define design group
            {
                var cmdcommands = new List<CmdCommand>();

                foreach (var desGroup in CmdDesignGroup._designGroupCache)
                {
                    var emptyDesignGroup = new CmdDesignGroup(desGroup.Key, new List<FemDesign.GenericClasses.IStructureElement>(), desGroup.Value.Type);
                    cmdcommands.Add(emptyDesignGroup);
                }

                var _script = new FdScript(logfile, cmdcommands);
                this.RunScript(_script, "DeleteDesignGroup");

                CmdDesignGroup._designGroupCache.Clear();
            }
            #endregion



            if (userModule == CmdUserModule.RESMODE)
            {
                throw new ArgumentException("User Module can not be 'RESMODE'!");
            }

            var script = new FdScript(
                    logfile,
                    new CmdUser(userModule),
                    new CmdCalculation(design)
                );

            this.RunScript(script, $"RunDesign_{userModule}");
        }


        /// <summary>
        /// 
        /// </summary>
        public void ApplyDesignChanges()
        {
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);

            var script = new FdScript(
                    logfile,
                    new CmdApplyDesignChanges()
                );

            this.RunScript(script, $"ApplyDesignChanges");
        }




        /// <summary>
        /// Opens <paramref name="model"/> in FEM-Design and runs the design.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="design"></param>
        /// <param name="userModule"></param>
        public void RunDesign(Model model, Design design, CmdUserModule userModule)
        {
            this.Open(model);
            this.RunDesign(userModule, design);
        }

        public void SetVerbosity(Verbosity verbosity)
        {
            Verbosity = verbosity;
            _connection.Send("v " + (int)verbosity);
        }

        public void EndSession()
        {
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);

            var script = new FdScript(
                logfile,
                new CmdEndSession()
            );

            this.RunScript(script, "EndSession");
        }


        /// <summary>
        /// Retrieves the currently opened model with all available elements as a <see cref="Model"/> object.
        /// </summary>
        public Model GetModel()
        {
            string struxmlPath = OutputFileHelper.GetStruxmlPath(OutputDir, "model_saved");
            string logfilePath = OutputFileHelper.GetLogfilePath(OutputDir);
            RunScript(new FdScript(logfilePath, new CmdSave(struxmlPath)), "GetModel");
            return Model.DeserializeFromFilePath(struxmlPath);
        }

        /// <summary>
        /// Save the documentation in a docx file.
        /// </summary>
        /// <param name="docxFilePath"></param>
        private void SaveDocx(string docxFilePath)
        {
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);

            var script = new FdScript(
                logfile,
                new CmdSaveDocx(docxFilePath)
            );

            this.RunScript(script, "SaveDocx");
        }

        /// <summary>
        /// Save the documentation in a docx file using a .bsc template file
        /// </summary>
        /// <param name="docxFilePath">.docx file path where the documentatio will be saved</param>
        /// <param name="templatePath">template .dsc file path to apply to the documentation</param>
        public void SaveDocx(string docxFilePath, string templatePath = null)
        {
            if(templatePath != null)
                this.ApplyDocumentationTemplate(templatePath);
            this.SaveDocx(docxFilePath);
        }

        /// <summary>
        /// Apply a template to a documentation
        /// </summary>
        /// <param name="templatePath"></param>
        public void ApplyDocumentationTemplate(string templatePath)
        {
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);

            var script = new FdScript(
                logfile,
                new CmdChild(templatePath)
            );

            this.RunScript(script, "SaveDocxFromTemplate");
        }

        /// <summary>
        /// Retrieves the loads from the currently opened model with all available elements as a <see cref="Loads.Loads"/> object.
        /// </summary>
        public Loads.Loads GetLoads()
        {
            string struxmlPath = OutputFileHelper.GetStruxmlPath(OutputDir, "model_loads_saved");
            string logfilePath = OutputFileHelper.GetLogfilePath(OutputDir);
            RunScript(new FdScript(logfilePath, new CmdSave(struxmlPath)), "GetLoads");
            return Loads.Loads.DeserializeFromFilePath(struxmlPath);
        }

        /// <summary>
        /// Retrieves the load combinations from the currently opened model/> object.
        /// </summary>
        public Dictionary<int, Loads.LoadCombination> GetLoadCombinations()
        {
            var loadCombinations = this.GetLoads().LoadCombinations;

            var dictLoadComb = new Dictionary<int, Loads.LoadCombination>();

            if (loadCombinations == null)
                throw new Exception("There are no load combinations in the model");

            int index = 0;
            foreach (var comb in loadCombinations)
            {
                dictLoadComb.Add(index, comb);
                index++;
            }

            return dictLoadComb;
        }


        /// The result seems to be save in memory and not in the .strFEM.
        /// Ask Pasa

        ///// <summary>
        ///// Check if the open model with FemDesignConnection has results
        ///// </summary>
        ///// <returns></returns>
        //public bool HasResult()
        //{
        //    if(CurrentOpenModel == null)
        //    {
        //        throw new Exception("The model has been open manually. Open a file using FemDesignConnection.Open() if you want to read the results!");
        //    }
        //    var directory = System.IO.Path.GetDirectoryName(CurrentOpenModel);
        //    var fileNames = Directory.GetFiles(directory);

        //    var strFEM = System.IO.Path.ChangeExtension(CurrentOpenModel, ".strFEM");

        //    foreach(var filename in fileNames)
        //    {
        //        if (filename == strFEM)
        //            return true;
        //    }

        //    return false;
        //}

        /// <summary>
        /// Retreive results from the opened model.
        /// </summary>
        /// <typeparam name="T">Result type to retrieve. Must be a type that implements the <see cref="Results.IResult"/> interface</typeparam>
        /// <param name="units">Optional. Unit setting for the results.</param>
        /// <param name="options">Optional. Options to set up the output location.</param>
        /// <param name="elements">Structural element for which the results should be return.</param>
        /// <returns>List of results of type <typeparamref name="T"/> if any could be retrieved. If the model has no results of type <typeparamref name="T"/> or cannot access them at the moment, then the list will be empty.</returns>
        public List<T> GetResults<T>(Results.UnitResults units = null, Options options = null, List<FemDesign.GenericClasses.IStructureElement> elements = null) where T : Results.IResult
        {
            if (units is null)
                units = Results.UnitResults.Default();

            // Input bsc files and output csv files
            var listProcs = typeof(T).GetCustomAttribute<Results.ResultAttribute>()?.ListProcs ?? Enumerable.Empty<ListProc>();
            var currentTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_fff");
            var bscPaths = listProcs.Select(l => OutputFileHelper.GetBscPath(OutputDir, l.ToString() + currentTime)).ToList();
            var csvPaths = listProcs.Select(l => OutputFileHelper.GetCsvPath(OutputDir, l.ToString() + currentTime)).ToList();

            var bscs = listProcs.Zip(bscPaths, (l, p) => new Bsc(l, p, null, units, options)).ToList();

            // FdScript commands
            List<CmdCommand> listGenCommands = new List<CmdCommand>();
            listGenCommands.Add(new CmdUser(CmdUserModule.RESMODE));
            for (int i = 0; i < bscPaths.Count; i++)
                listGenCommands.Add(new CmdListGen(bscPaths[i], csvPaths[i], elements));

            // Run the script
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            var script = new FdScript(logfile, listGenCommands.ToArray());
            this.RunScript(script, $"Get{typeof(T).Name}" + currentTime);

            // Read csv results files
            List<T> results = new List<T>();
            foreach (string resultFile in csvPaths)
            {
                results.AddRange(
                    Results.ResultsReader.Parse(resultFile).ConvertAll(r => (T)r)
                );
            }

            return results;
        }



        /// <summary>
        /// Create result points
        /// </summary>
        /// <param name="resultPoints"></param>
        public void CreateResultPoint(List<CmdResultPoint> resultPoints)
        {
            // FdScript commands
            List<CmdCommand> commands = new List<CmdCommand>();

            commands.Add(new CmdUser(CmdUserModule.RESMODE));
            commands.AddRange(resultPoints);

            // Run the script
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            var script = new FdScript(logfile, commands);
            this.RunScript(script, "CreateResultPoints");
        }


        public string GetResultsFromBsc(string inputBscPath, string outputCsvPath = null)
        {
            if (outputCsvPath == null)
            {
                outputCsvPath = System.IO.Path.ChangeExtension(inputBscPath, "csv");
            }

            if (System.IO.Path.GetExtension(outputCsvPath) != ".csv")
                throw new Exception("Extension output file must be .csv");

            // FdScript commands
            List<CmdCommand> commands = new List<CmdCommand> { new CmdUser(CmdUserModule.RESMODE), new CmdListGen(inputBscPath, outputCsvPath) };

            // Run the script
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            var script = new FdScript(logfile, commands);
            this.RunScript(script, "GetResultsFromBsc");

            var results = System.IO.File.ReadAllText(outputCsvPath, System.Text.Encoding.UTF8).Replace("\t",",");

            return results;
        }


        public List<T> GetResultsOnPoints<T>(CmdResultPoint resultPoints, Results.UnitResults units = null) where T : Results.IResult
        {
            if (units is null)
                units = Results.UnitResults.Default();

            // Input bsc files and output csv files
            var listProcs = typeof(T).GetCustomAttribute<Results.ResultAttribute>()?.ListProcs ?? Enumerable.Empty<ListProc>();
            var currentTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_fff");
            var bscPaths = listProcs.Select(l => OutputFileHelper.GetBscPath(OutputDir, l.ToString() + currentTime)).ToList();
            var csvPaths = listProcs.Select(l => OutputFileHelper.GetCsvPath(OutputDir, l.ToString() + currentTime)).ToList();

            var options = new Options(BarResultPosition.ResultPoints, ShellResultPosition.ResultPoints);
            var bscs = listProcs.Zip(bscPaths, (l, p) => new Bsc(l, p, null, units, options)).ToList();

            // FdScript commands
            List<CmdCommand> commands = new List<CmdCommand>();

            commands.Add(new CmdUser(CmdUserModule.RESMODE));
            commands.Add(resultPoints);

            for (int i = 0; i < bscPaths.Count; i++)
                //listGenCommands.Add(new CmdListGen(bscPaths[i], csvPaths[i]));
                commands.Add(new CmdListGen(bscPaths[i], csvPaths[i]));

            // Run the script
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            var script = new FdScript(logfile, commands.ToArray());
            this.RunScript(script, "GetResultsOnPoints");

            // Read csv results files
            List<T> results = new List<T>();
            foreach (string resultFile in csvPaths)
            {
                results.AddRange(
                    Results.ResultsReader.Parse(resultFile).ConvertAll(r => (T)r)
                );
            }

            return results;
        }

        public List<Results.FemNode> GetFeaNodes(Results.Length units = Results.Length.m)
        {
            var _resultType = ListProc.FemNode;

            var unitResults = Results.UnitResults.Default();
            unitResults.Length = units;

            // Input bsc files and output csv files
            var listProcs = new List<ListProc> { _resultType };
            var bscPaths = listProcs.Select(l => OutputFileHelper.GetBscPath(OutputDir, l.ToString())).ToList();
            var csvPaths = listProcs.Select(l => OutputFileHelper.GetCsvPath(OutputDir, l.ToString())).ToList();

            var bscs = listProcs.Zip(bscPaths, (l, p) => new Bsc(l, p, null, unitResults)).ToList();

            // FdScript commands
            List<CmdCommand> listGenCommands = new List<CmdCommand>();
            listGenCommands.Add(new CmdUser(CmdUserModule.RESMODE));
            for (int i = 0; i < bscPaths.Count; i++)
                listGenCommands.Add(new CmdListGen(bscPaths[i], csvPaths[i]));

            // Run the script
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            var script = new FdScript(logfile, listGenCommands.ToArray());
            this.RunScript(script, "GetFeaNode");

            // Read csv results files
            List<FemDesign.Results.FemNode> results = new List<FemDesign.Results.FemNode>();
            foreach (string resultFile in csvPaths)
            {
                results.AddRange(
                    Results.ResultsReader.Parse(resultFile).ConvertAll(r => (Results.FemNode)r)
                );
            }

            return results;
        }
        public List<Results.FemBar> GetFeaBars(Results.Length units = Results.Length.m)
        {
            var _resultType = ListProc.FemBar;

            var unitResults = Results.UnitResults.Default();
            unitResults.Length = units;

            // Input bsc files and output csv files
            var listProcs = new List<ListProc> { _resultType };
            var bscPaths = listProcs.Select(l => OutputFileHelper.GetBscPath(OutputDir, l.ToString())).ToList();
            var csvPaths = listProcs.Select(l => OutputFileHelper.GetCsvPath(OutputDir, l.ToString())).ToList();

            var bscs = listProcs.Zip(bscPaths, (l, p) => new Bsc(l, p, null, unitResults)).ToList();

            // FdScript commands
            List<CmdCommand> listGenCommands = new List<CmdCommand>();
            listGenCommands.Add(new CmdUser(CmdUserModule.RESMODE));
            for (int i = 0; i < bscPaths.Count; i++)
                listGenCommands.Add(new CmdListGen(bscPaths[i], csvPaths[i]));

            // Run the script
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            var script = new FdScript(logfile, listGenCommands.ToArray());
            this.RunScript(script, "GetFeaBar");

            // Read csv results files
            List<FemDesign.Results.FemBar> results = new List<FemDesign.Results.FemBar>();
            foreach (string resultFile in csvPaths)
            {
                results.AddRange(
                    Results.ResultsReader.Parse(resultFile).ConvertAll(r => (Results.FemBar)r)
                );
            }

            return results;
        }
        public List<Results.FemShell> GetFeaShells(Results.Length units = Results.Length.m)
        {
            var _resultType = ListProc.FemShell;

            var unitResults = Results.UnitResults.Default();
            unitResults.Length = units;

            // Input bsc files and output csv files
            var listProcs = new List<ListProc> { _resultType };
            var bscPaths = listProcs.Select(l => OutputFileHelper.GetBscPath(OutputDir, l.ToString())).ToList();
            var csvPaths = listProcs.Select(l => OutputFileHelper.GetCsvPath(OutputDir, l.ToString())).ToList();

            var bscs = listProcs.Zip(bscPaths, (l, p) => new Bsc(l, p, null, unitResults)).ToList();

            // FdScript commands
            List<CmdCommand> listGenCommands = new List<CmdCommand>();
            listGenCommands.Add(new CmdUser(CmdUserModule.RESMODE));
            for (int i = 0; i < bscPaths.Count; i++)
                listGenCommands.Add(new CmdListGen(bscPaths[i], csvPaths[i]));

            // Run the script
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            var script = new FdScript(logfile, listGenCommands.ToArray());
            this.RunScript(script, "GetFeaShell");

            // Read csv results files
            List<FemDesign.Results.FemShell> results = new List<FemDesign.Results.FemShell>();
            foreach (string resultFile in csvPaths)
            {
                results.AddRange(
                    Results.ResultsReader.Parse(resultFile).ConvertAll(r => (Results.FemShell)r)
                );
            }

            return results;
        }
        public Results.FiniteElement GetFeaModel(Results.Length units = Results.Length.m)
        {
            //if( !HasResult())
            //{
            //    throw new Exception("The current open model does not have results!");
            //}

            var feaNode = GetFeaNodes(units);
            var feaBar = GetFeaBars(units);
            var feaShell = GetFeaShells(units);

            var fdFEa = new Results.FiniteElement(feaNode, feaBar, feaShell);

            return fdFEa;
        }

        public List<T> GetLoadCaseResults<T>(string loadCase = null, List<FemDesign.GenericClasses.IStructureElement> elements = null, Results.UnitResults units = null, Options options = null) where T : Results.IResult
        {
            if (units is null)
                units = Results.UnitResults.Default();

            // Input bsc files and output csv files
            var listProcs = typeof(T).GetCustomAttribute<Results.ResultAttribute>()?.ListProcs.Where(p => p.IsLoadCase() == true) ?? Enumerable.Empty<ListProc>();

            if (!listProcs.Any())
            {
                throw new ArgumentException("T parameter must be a LoadCase result type!");
            }

            // listproc that are only load case
            var currentTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_fff");
            var bscPaths = listProcs.Select(l => OutputFileHelper.GetBscPath(OutputDir, l.ToString() + loadCase + currentTime)).ToList();
            var csvPaths = listProcs.Select(l => OutputFileHelper.GetCsvPath(OutputDir, l.ToString() + loadCase + currentTime)).ToList();

            var bscs = listProcs.Zip(bscPaths, (l, p) => new Bsc(l, p, loadCase, units, options)).ToList();
            

            // FdScript commands
            List<CmdCommand> listGenCommands = new List<CmdCommand>();
            listGenCommands.Add(new CmdUser(CmdUserModule.RESMODE));
            for (int i = 0; i < bscPaths.Count; i++)
                listGenCommands.Add(new CmdListGen(bscPaths[i], csvPaths[i], elements));

            // Run the script
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            var script = new FdScript(logfile, listGenCommands.ToArray());
            this.RunScript(script, "GetLoadCase" + currentTime);

            // Read csv results files
            List<T> results = new List<T>();
            foreach (string resultFile in csvPaths)
            {
                results.AddRange(
                    Results.ResultsReader.Parse(resultFile).ConvertAll(r => (T)r)
                );
            }

            return results;
        }

        
        public List<T> GetLoadCombinationResults<T>(string loadCombination = null, List<FemDesign.GenericClasses.IStructureElement> elements = null, Results.UnitResults units = null, Options options = null) where T : Results.IResult
        {
            if (units is null)
                units = Results.UnitResults.Default();

            // Input bsc files and output csv files
            var listProcs = typeof(T).GetCustomAttribute<Results.ResultAttribute>()?.ListProcs.Where(p => p.IsLoadCombination() == true) ?? Enumerable.Empty<ListProc>();

            if (!listProcs.Any())
            {
                throw new ArgumentException("T parameter must be a LoadCombination result type!");
            }

            // listproc that are only load combination
            var currentTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_fff");
            var bscPaths = listProcs.Select(l => OutputFileHelper.GetBscPath(OutputDir, l.ToString() + loadCombination + currentTime)).ToList();
            var csvPaths = listProcs.Select(l => OutputFileHelper.GetCsvPath(OutputDir, l.ToString() + loadCombination + currentTime)).ToList();

            var bscs = listProcs.Zip(bscPaths, (l, p) => new Bsc(l, p, loadCombination, units, options)).ToList();

            // FdScript commands
            List<CmdCommand> listGenCommands = new List<CmdCommand>();
            listGenCommands.Add(new CmdUser(CmdUserModule.RESMODE));
            for (int i = 0; i < bscPaths.Count; i++)
                listGenCommands.Add(new CmdListGen(bscPaths[i], csvPaths[i], elements));

            // Run the script
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            var script = new FdScript(logfile, listGenCommands.ToArray());
            this.RunScript(script, "GetLoadCombo" + currentTime);

            // Read csv results files
            List<T> results = new List<T>();
            foreach (string resultFile in csvPaths)
            {
                results.AddRange(
                    Results.ResultsReader.Parse(resultFile).ConvertAll(r => (T)r)
                );
            }

            return results;
        }


        [Obsolete("Method will be removed in 23.0.0", false)]
        public List<T> GetAllLoadCaseResults<T>(Results.UnitResults units = null, Options options = null, List<FemDesign.GenericClasses.IStructureElement> elements = null) where T : Results.IResult
        {
            if (units is null)
                units = Results.UnitResults.Default();

            // Input bsc files and output csv files
            var listProcs = typeof(T).GetCustomAttribute<Results.ResultAttribute>()?.ListProcs.Where(p => p.IsLoadCase() == true) ?? Enumerable.Empty<ListProc>();

            if (!listProcs.Any())
            {
                throw new ArgumentException("T parameter must be a LoadCase result type!");
            }

            var bscPaths = listProcs.Select(l => OutputFileHelper.GetBscPath(OutputDir, l.ToString())).ToList();
            var csvPaths = listProcs.Select(l => OutputFileHelper.GetCsvPath(OutputDir, l.ToString())).ToList();

            var bscs = listProcs.Zip(bscPaths, (l, p) => new Bsc(l, p, null, units, options)).ToList();

            // FdScript commands
            List<CmdCommand> listGenCommands = new List<CmdCommand>();
            listGenCommands.Add(new CmdUser(CmdUserModule.RESMODE));
            for (int i = 0; i < bscPaths.Count; i++)
                listGenCommands.Add(new CmdListGen(bscPaths[i], csvPaths[i], elements));

            // Run the script
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            var script = new FdScript(logfile, listGenCommands.ToArray());
            this.RunScript(script);

            // Read csv results files
            List<T> results = new List<T>();
            foreach (string resultFile in csvPaths)
            {
                results.AddRange(
                    Results.ResultsReader.Parse(resultFile).ConvertAll(r => (T)r)
                );
            }

            return results;
        }



        [Obsolete("Method will be removed in 23.0.0", false)]
        public List<T> GetAllLoadCombinationResults<T>(Results.UnitResults units = null, Options options = null, List<FemDesign.GenericClasses.IStructureElement> elements = null) where T : Results.IResult
        {
            if (units is null)
                units = Results.UnitResults.Default();

            // Input bsc files and output csv files
            var listProcs = typeof(T).GetCustomAttribute<Results.ResultAttribute>()?.ListProcs.Where(p => p.IsLoadCombination() == true) ?? Enumerable.Empty<ListProc>();

            if (!listProcs.Any())
            {
                throw new ArgumentException("T parameter must be a LoadCombination result type!");
            }

            var bscPaths = listProcs.Select(l => OutputFileHelper.GetBscPath(OutputDir, l.ToString())).ToList();
            var csvPaths = listProcs.Select(l => OutputFileHelper.GetCsvPath(OutputDir, l.ToString())).ToList();

            var bscs = listProcs.Zip(bscPaths, (l, p) => new Bsc(l, p, null, units, options)).ToList();

            // FdScript commands
            List<CmdCommand> listGenCommands = new List<CmdCommand>();
            listGenCommands.Add(new CmdUser(CmdUserModule.RESMODE));
            for (int i = 0; i < bscPaths.Count; i++)
                //listGenCommands.Add(new CmdListGen(bscPaths[i], csvPaths[i]));
                listGenCommands.Add(new CmdListGen(bscPaths[i], csvPaths[i], elements));

            // Run the script
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            var script = new FdScript(logfile, listGenCommands.ToArray());
            this.RunScript(script);

            // Read csv results files
            List<T> results = new List<T>();
            foreach (string resultFile in csvPaths)
            {
                results.AddRange(
                    Results.ResultsReader.Parse(resultFile).ConvertAll(r => (T)r)
                );
            }

            return results;
        }

        public List<T> GetQuantities<T>(Results.UnitResults units = null) where T : Results.IResult
        {
            if (units is null)
                units = Results.UnitResults.Default();

            // Input bsc files and output csv files
            var listProcs = typeof(T).GetCustomAttribute<Results.ResultAttribute>()?.ListProcs.Where(p => p.IsQuantityEstimation() == true) ?? Enumerable.Empty<ListProc>();

            if (!listProcs.Any())
            {
                throw new ArgumentException("T parameter must be a Quantity Estimation result type!");
            }

            var bscPaths = listProcs.Select(l => OutputFileHelper.GetBscPath(OutputDir, l.ToString())).ToList();
            var csvPaths = listProcs.Select(l => OutputFileHelper.GetCsvPath(OutputDir, l.ToString())).ToList();

            var bscs = listProcs.Zip(bscPaths, (l, p) => new Bsc(l, p, null, units)).ToList();

            // FdScript commands
            List<CmdCommand> listGenCommands = new List<CmdCommand>();
            listGenCommands.Add(new CmdUser(CmdUserModule.RESMODE));
            for (int i = 0; i < bscPaths.Count; i++)
                //listGenCommands.Add(new CmdListGen(bscPaths[i], csvPaths[i]));
                listGenCommands.Add(new CmdListGen(bscPaths[i], csvPaths[i]));

            // Run the script
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            var script = new FdScript(logfile, listGenCommands.ToArray());
            this.RunScript(script, "GetQuantities");

            // Read csv results files
            List<T> results = new List<T>();
            foreach (string resultFile in csvPaths)
            {
                results.AddRange(
                    Results.ResultsReader.Parse(resultFile).ConvertAll(r => (T)r)
                );
            }

            return results;
        }

        public List<T> GetStabilityResults<T>(string loadCombination = null, int? shapeId = null, Results.UnitResults units = null, Options options = null) where T : IResult
        {
            if (units is null)
                units = Results.UnitResults.Default();

            // Input bsc files and output csv files
            var listProcs = typeof(T).GetCustomAttribute<Results.ResultAttribute>()?.ListProcs ?? Enumerable.Empty<ListProc>();

            var currentTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_fff");
            var bscPaths = listProcs.Select(l => OutputFileHelper.GetBscPath(OutputDir, l.ToString() + loadCombination + shapeId + "_" + currentTime)).ToList();
            var csvPaths = listProcs.Select(l => OutputFileHelper.GetCsvPath(OutputDir, l.ToString() + loadCombination + shapeId + "_" + currentTime)).ToList();

            var bscs = listProcs.Zip(bscPaths, (l, p) => new Bsc(l, p, null, units, options)).ToList();

            // FdScript commands
            List<CmdCommand> listGenCommands = new List<CmdCommand>();
            listGenCommands.Add(new CmdUser(CmdUserModule.RESMODE));
            for (int i = 0; i < bscPaths.Count; i++)
                listGenCommands.Add(new CmdListGen(bscPaths[i], csvPaths[i]));

            // Run the script
            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            var script = new FdScript(logfile, listGenCommands.ToArray());
            this.RunScript(script, $"Get{typeof(T).Name}" + loadCombination + shapeId + currentTime);

            // Read csv results files
            List<T> results = new List<T>();
            foreach (string resultFile in csvPaths)
            {
                results.AddRange(
                    Results.ResultsReader.Parse(resultFile).ConvertAll(r => (T)r)
                );
            }

            // Filter results by load combination and shape identifier
            string loadCombPropertyName;
            string shapeIdPropertyName;

            if (typeof(T) == typeof(NodalBucklingShape))
            {
                loadCombPropertyName = nameof(NodalBucklingShape.CaseIdentifier);
                shapeIdPropertyName = nameof(NodalBucklingShape.Shape);
            }
            else if (typeof(T) == typeof(CriticalParameter))
            {
                loadCombPropertyName = nameof(CriticalParameter.CaseIdentifier);
                shapeIdPropertyName = nameof(CriticalParameter.Shape);
            }
            else
            {
                throw new ArgumentException("This method cannot be used with the specified type.");
            }

            if (loadCombination != null)
            {
                results = Results.Utils.UtilResultMethods.FilterResultsByLoadCombination(results, loadCombPropertyName, loadCombination);
            }
            if (shapeId != null)
            {
                results = Results.Utils.UtilResultMethods.FilterResultsByShapeId(results, shapeIdPropertyName, (int)shapeId);
            }

            return results;
        }

        ///// <summary>
        ///// Gets results for those result types where FEM-Design uses eigen solver. E.g. imperfections, stability analysis, vibrations.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="loadCombinations">Load combination names for which you want to list the results.</param>
        ///// <param name="shapeIds">Shape identifiers for which you want to list the results. Must be positive, non-zero numbers.</param>
        ///// <param name="elements">Structural elements for which you want to list the results.</param>
        ///// <param name="units"></param>
        ///// <param name="options"></param>
        ///// <returns></returns>
        ///// <exception cref="ArgumentException"></exception>
        //public List<T> GetEigenResults<T>(List<string> loadCombinations, List<int> shapeIds, List<FemDesign.GenericClasses.IStructureElement> elements = null, Results.UnitResults units = null, Options options = null) where T : IResult
        //{
        //    // Check inputs
        //    if (loadCombinations == null || loadCombinations.Count == 0)
        //        throw new ArgumentException("loadCombinations input cannot be null or empty!");
        //    if (shapeIds == null || shapeIds.Count == 0)
        //        throw new ArgumentException("shapeIds input cannot be null or empty!");
            

        //    // Get default units
        //    if (units is null)
        //        units = Results.UnitResults.Default();

        //    // Create .bsc files and output (.csv) file paths
        //    var listProcs = typeof(T).GetCustomAttribute<Results.ResultAttribute>()?.ListProcs ?? Enumerable.Empty<ListProc>();

        //    var currentTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_fff");
        //    var bscPaths = new List<string>();
        //    var csvPaths = new List<string>();
        //    var bscs = new List<Bsc>();
            
        //    foreach(var combo in loadCombinations)
        //    {
        //        foreach(var sh in shapeIds)
        //        {
        //            var bscPth = listProcs.Select(l => OutputFileHelper.GetBscPath(OutputDir, l.ToString() + combo + sh + "_" + currentTime)).ToList();
        //            var csvPth = listProcs.Select(l => OutputFileHelper.GetCsvPath(OutputDir, l.ToString() + combo + sh + "_" + currentTime)).ToList();
        //            bscPaths.AddRange(bscPth);
        //            csvPaths.AddRange(csvPth);

        //            // Generate .bsc files
        //            bscs.AddRange(listProcs.Zip(bscPth, (l, p) => new Bsc(l, p, combo, sh, units, options)).ToList());
        //        }
        //    }


        //    // FdScript commands
        //    List<CmdCommand> listGenCommands = new List<CmdCommand>();
        //    listGenCommands.Add(new CmdUser(CmdUserModule.RESMODE));
        //    for (int i = 0; i < bscPaths.Count; i++)
        //        listGenCommands.Add(new CmdListGen(bscPaths[i], csvPaths[i], elements));

        //    // Run .fdscript
        //    string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
        //    var script = new FdScript(logfile, listGenCommands.ToArray());
        //    this.RunScript(script, $"Get{typeof(T).Name}" + currentTime);

        //    // Read results from .csv files
        //    List<T> results = new List<T>();
        //    foreach (string resultFile in csvPaths)
        //    {
        //        results.AddRange(
        //            Results.ResultsReader.Parse(resultFile).ConvertAll(r => (T)r)
        //        );
        //    }

        //    return results;
        //}


        public void Save(string filePath)
        {
            if(System.IO.Path.GetExtension(filePath) != ".str" && System.IO.Path.GetExtension(filePath) != ".struxml")
            {
                throw new Exception("Only .str and .struxml extensions are valid!");
            }

            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);
            var script = new FdScript(logfile, new CmdSave(filePath));
            this.RunScript(script);
        }

        public FemDesign.Results.InteractionSurface RunInteractionSurface(FemDesign.Bars.Bar bar, double offset = 0.0, bool fUlt = true)
        {
            var bars = new List<GenericClasses.IStructureElement> { bar };
            return RunInteractionSurface(bars, offset, fUlt)[0];
        }

        public void LoadGroupToLoadComb(bool fu = true, bool fua = true, bool fus = true, bool fsq = true, bool fsf = true, bool fsc = true, bool fSeisSigned = true, bool fSeisTorsion = true, bool fSeisZdir = true, bool fSkipMinDL = true, bool fForceTemp = true, bool fShortName = true)
        {
            var version = Int32.Parse(this._process.MainModule.FileVersionInfo.FileVersion.Replace(".", ""), CultureInfo.InvariantCulture);
            if (version < 22040)
                throw new Exception("FEM-Design 22.00.004 or greater is required!");

            var cmdLoadGroupToLoadComb = new CmdLoadGroupToLoadComb(fu, fua, fus, fsq, fsf, fsc, fSeisSigned, fSeisTorsion, fSeisZdir, fSkipMinDL, fForceTemp, fShortName);

            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);

            var script = new FdScript(logfile, cmdLoadGroupToLoadComb);

            this.RunScript(script, "LoadGroupToLoadComb");
        }

        public List<FemDesign.Results.InteractionSurface> RunInteractionSurface(List<FemDesign.GenericClasses.IStructureElement> bars, double offset = 0.0, bool fUlt = true)
        {
            string outFile = OutputFileHelper.GetIntSrffilePath(OutputDir);

            var model = new Model(Country.COMMON, bars, overwrite: true);
            this.Open(model);

            string logfile = OutputFileHelper.GetLogfilePath(OutputDir);

            var script = new FdScript(logfile, new CmdUser(CmdUserModule.RCDESIGN));
            foreach (var bar in bars)
            {
                var _bar = (Bars.Bar)bar;
                var newName = System.IO.Path.GetFileNameWithoutExtension(outFile) + _bar.BarPart.Guid + ".txt";
                script.Add(new CmdInteractionSurface(_bar, newName, offset, fUlt));
            }

            this.RunScript(script);

            var intSurfaces = new List<FemDesign.Results.InteractionSurface>();

            foreach (var bar in bars)
            {
                var _bar = (Bars.Bar)bar;
                var newName = System.IO.Path.GetFileNameWithoutExtension(outFile) + _bar.BarPart.Guid + ".txt";
                var intSrf = FemDesign.Results.InteractionSurface.ReadFromFile(newName);
                intSurfaces.Add(intSrf);
            }
            return intSurfaces;
        }

        public void Dispose()
        {
            if (_keepOpen) Disconnect();

            this._connection.Dispose();

            // TODO: Delete the files when they are not locked by FEM-Design
            this._deleteOutputDirectories();
        }
        private void _deleteOutputDirectories()
        {
            foreach (string dir in _outputDirsToBeDeleted)
                if (Directory.Exists(dir))
                    _deleteFolderIfNotUsed(dir);
        }
        private static void _deleteFolderIfNotUsed(string folderPath)
        {
            try
            {
                Directory.Delete(folderPath, true);
            }
            catch (IOException ex)
            {
                // Check if the exception is due to a file or folder being in use
                if ((ex.HResult & 0xFFFF) == 32 || (ex.HResult & 0xFFFF) == 33)
                {
                    // The folder or a file is in use, so we can't delete it yet
                    return;
                }
                // The exception is not related to a file or folder being in use, rethrow it
                throw;
            }
        }

        private string _outputDir;
        public string OutputDir
        {
            get { return _outputDir; }
            set
            {
                if (string.IsNullOrEmpty(value)) // Use temp dir
                {
                    _outputDir = Path.Combine(Directory.GetCurrentDirectory(), "FEM-Design API");
                    //_outputDirsToBeDeleted.Add(_outputDir);
                }
                else // Use given directory
                    _outputDir = Path.GetFullPath(value);

                // check if special characters
                // not supported
                if (!OutputFileHelper.IsASCII(_outputDir))
                {
                    throw new Exception("`OutputDir` has special characters. Only ASCII characters are supported!");
                }
            }
        }
        private List<string> _outputDirsToBeDeleted = new List<string>();
    }

    /*
    FEM-Design usage with named pipe.

    To initiate:
    1:  Create a WIN32 named pipe for duplex mode, message oriented
    1a: optional: Create another pipe for back channel, named appending 'b'.
    2:  Launch FD with command line argument `/p Name` passing the name you used at creation. FD will open it right at start and exit if can't. After successful launch it listens to commands while the usual interface is active. You can combine it with the windowless / minimized mode to hide the window. It also attaches to the back channel pipe at this moment, if unable, all output is permanently disabled.
    3:  Send commands through the pipe.
    4:  FD will exit if 'exit' command received or the pipe is closed on this end.

    FD only reads the main pipe and only writes the back channel (if supplied), allowing this end to never read.
    While the pipe is duplexand, it can be used in both direction. If it gets clogged in one direction (by not reading what the other end sends), the write can get blocked too.
    The document recommends using another pipe for a back channel.
    By default nothing is written to the back channel, you need to set output level (using verbosity, v, comand) or use commands with implicit reply.
    FD buffers all outgoing messages till they can be sent over, if this end is lazy to read, it will not clog, however they will accumulate in memory.
     */
    internal class PipeConnection : IDisposable
    {
        /// <summary>
        /// Connect to FEM-Design using Named Pipe
        /// </summary>
        /// <param name="pipeBaseName"></param>
        /// <exception cref="Exception"></exception>
        public PipeConnection(string pipeBaseName = "FdPipe1")
        {
            // Encoding that allow to have special character
            // https://nicolaiarocci.com/how-to-read-windows-1252-encoded-files-with-.netcore-and-.net5-/
            //_encoding = System.Text.Encoding.GetEncoding(1252); 

            _encoding = System.Text.Encoding.ASCII;

            string input_name = pipeBaseName;
            string output_name = pipeBaseName + "b";

            _inputPipe = create_pipe(input_name);
            _outputPipe = create_pipe(output_name);

            _startOutputThread();

            // this is what check status does...
            if (_inputPipe == null) { throw new Exception("setup failed"); }
        }

        public void WaitForConnection()
        {
            _inputPipe.WaitForConnection();
        }

        public delegate void OnOutputEvent(string output);
        public OnOutputEvent OnOutput { get; set; } = null;

        /// <summary>
        /// Send command to FEM-Design using the named pipe.
        /// </summary>
        /// <param name="command">Command to excecute in FEM-Design.</param>
        public void Send(string command)
        {
            /*
            The command format is
            [!]<cmd> [args]
            there is no delimiter at the end, the pipe message counts.
            FEM-Design reads the pipe immediately and puts the commands in a queue. The queue is processed when it's READYSTATE
            is ready for another command, finishing execution of the previous or a current script.
            
            The !requests out of bound execution. That is not supported by very command and mainly
            serves to manipulate the queue itself, verbosity or check the communicaiton is alive.
        
            Messages are text in 9bit ANSI (codepage), limited to 4096 bytes.

            Commands:
        
            exit
            Stop the FD process
        
            detach
            Close the pipe and continue in normal interface mode
        
            clear [in|out]
            Flush the FD mesage queue for the direction, or both if send without in or out parameters.
            Has no Effect on what is already issued to the pipe
        
            echo [txt]
            Write txt to output
        
            stat
            Write queue and processing status to output
        
            v [N]
            Set verbosity control (bits)
               1: Enable basic output
               2: Echo all INPUT commands
               4: FEM-Design log-lines
               8: Script log lines
              16: Calculation window messages (except fortran)
             *32: Progress window title
        
            Commands echo and stat always creates output, otherwise nothing is written at V = 0
            * Not yet supported
        
            run [scriptfile]
            Execute script as from tools / run script menu.
        
            cmd [command]
            Execute command as if typed into the command window. No warranty!
        
            esc
            Escape during calculation to break/stop it.
            */

            if (_inputPipe.CanWrite == false) throw new Exception("Can't write to pipe");
            var buffer = _encoding.GetBytes(command);
            try
            {
                _inputPipe.Write(buffer, 0, buffer.Length);
                _inputPipe.Flush();
            }
            catch (Exception ex)
            {
                if (ex.Message == "Cannot access a closed pipe.")
                {
                    throw new Exception("'FEM-Design Connection' has been closed! Open a new FEM-Design connection if you want to comunicate with FEM-Design.");
                }
                else
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        internal void WaitForCommandToFinish()
        {
            var guid = Guid.NewGuid();
            this.Send("echo " + guid);
            this._waitForOutput(guid);
        }

        internal async Task WaitForCommandToFinishAsync()
        {
            var guid = Guid.NewGuid();
            this.Send("echo " + guid);
            await this._waitForOutputAsync(guid);
        }

        // ----------------------------------------------------------------------------------------

        public void Disconnect()
        {
            _inputPipe.Disconnect();
        }

        public void Dispose()
        {
            _inputPipe.Dispose();
            _outputPipe.Dispose();
            _disposeWorker();
        }

        // ----------------------------------------------------------------------------------------

        private static NamedPipeServerStream create_pipe(string name)
        {
            // it seems that c# autoprefixes the \\.\pipe\ string, no need to add
#pragma warning disable CA1416 // Validate platform compatibility
            var pipe = new NamedPipeServerStream(name,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Message,
                PipeOptions.None,
                BUFFER_SIZE, BUFFER_SIZE
                );
#pragma warning restore CA1416 // Validate platform compatibility
            return pipe;
        }

        private void _waitForOutput(Guid guid)
        {
            this._waitForOutput(guid.ToString());
        }

        private async Task _waitForOutputAsync(Guid guid)
        {
            await this._waitForOutputAsync(guid.ToString());
        }

        private void _waitForOutput(string output)
        {
            _waitForOutputAsync(output).Wait();
        }

        private async Task _waitForOutputAsync(string output)
        {
            var tcs = new TaskCompletionSource<bool>();
            void onOutput(string msg)
            {
                if (msg == output)
                {
                    this.OnOutput -= onOutput;
                    tcs.SetResult(true);
                }
            }
            this.OnOutput += onOutput;

            await tcs.Task;
        }

        // ----------------------------------------------------------------------------------------

        private void _disposeWorker()
        {
            _sendOutputToEvent = false;
            if (_outputWorker.IsBusy)
            {
                _outputWorker.CancelAsync();
            }
        }

        private void _startOutputThread()
        {
            _outputWorker.WorkerSupportsCancellation = true;
            _outputWorker.DoWork += _onOutputThread;

            _outputWorker.WorkerReportsProgress = true;
            _outputWorker.ProgressChanged += _onOuputThreadEvent;

            if (_outputWorker.IsBusy == true) { throw new Exception("background thread already started... bug?"); }
            _outputWorker.RunWorkerAsync();
        }

        private int GetProcessCount()
        {
            return Process.GetProcessesByName("fd3dstruct").Length;
        }

        private void _onOutputThread(object sender, DoWorkEventArgs e)
        {
            try
            {
                _outputPipe.WaitForConnection();
                while (_outputWorker.CancellationPending == false)
                {
                    var buffer = new byte[BUFFER_SIZE + 1];
                    var read = _outputPipe.Read(buffer, 0, BUFFER_SIZE);
                    var line = _encoding.GetString(buffer, 0, read);
                    if (line == null) { continue; }

                    _outputWorker.ReportProgress(0, line);

                    if (GetProcessCount() < 1)
                    {
                        this.Dispose();
                        throw new Exception("FEM-Design has been closed!");
                    };
                }
            }
            catch (Exception ex)
            {
                if(ex is Exception)
                {
                    throw new Exception(ex.Message);
                }
                if(ex is Exception)
                {
                    _outputWorker.ReportProgress(1, ex);
                }
            }

        }

        private void _onOuputThreadEvent(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string line)
            {
                if (_sendOutputToEvent)
                {
                    OnOutput?.Invoke(line);
                }
            }
            else if (e.UserState is Exception ex)
            {
                if (_sendOutputToEvent)
                {
                    throw ex;
                }
            }
            else
            {
                throw new Exception("invalid progress object");
            }
        }

        //internal void Disconnect()
        //{
        //    this._inputPipe.Disconnect();
        //    this._outputPipe.Disconnect();
        //}

        // ----------------------------------------------------------------------------------------

        internal readonly NamedPipeServerStream _inputPipe;
        internal readonly NamedPipeServerStream _outputPipe;
        readonly BackgroundWorker _outputWorker = new BackgroundWorker();
        private readonly System.Text.Encoding _encoding;
        private const int BUFFER_SIZE = 4096;
        private bool _sendOutputToEvent = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public enum Verbosity
    {
        EchoAndStatOnly = 0,
        /// <summary>
        /// Enable basic output
        /// </summary>
        BasicOnly = 1,
        /// <summary>
        /// Echo all INPUT commands
        /// </summary>
        InputOnly = 2,
        /// <summary>
        /// FEM-Design log-lines
        /// </summary>
        LogLinesOnly = 4,
        /// <summary>
        /// Script log lines
        /// </summary>
        ScriptLogLinesOnly = 8,
        /// <summary>
        /// Calculation window messages (except fortran)
        /// </summary>
        CalculationMessagesOnly = 16,
        /// <summary>
        /// Progress window title
        /// </summary>
        ProgressWindowTitleOnly = 32,

        None = EchoAndStatOnly,
        Low = BasicOnly | InputOnly,
        Normal = ScriptLogLinesOnly | CalculationMessagesOnly,
        High = BasicOnly | InputOnly | LogLinesOnly | ScriptLogLinesOnly | CalculationMessagesOnly,
        All = BasicOnly | InputOnly | LogLinesOnly | ScriptLogLinesOnly | CalculationMessagesOnly | ProgressWindowTitleOnly,
    }

    public static class OutputFileHelper
    {
        private const string _scriptsDirectory = "scripts";
        private const string _resultsDirectory = "results";
        private const string _bscDirectory = "bsc";

        private const string _logFileName = "logfile.log";
        private const string _struxmlFileName = "model.struxml";
        private const string _strFileName = "model.str";
        private const string _docxFileName = "model.docx";

        private const string _intSrfFileName = "intSrf.txt";

        private const string _fdscriptFileExtension = ".fdscript";
        private const string _bscFileExtension = ".bsc";
        private const string _csvFileExtension = ".csv";
        public static string GetLogfilePath(string baseDir)
        {
            string logfilePath = Path.Combine(baseDir, _logFileName);
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);
            return Path.GetFullPath(Path.Combine(baseDir, _logFileName));
        }

        public static string GetIntSrffilePath(string baseDir)
        {
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);
            return Path.GetFullPath(Path.Combine(baseDir, _intSrfFileName));
        }

        public static string GetStruxmlPath(string baseDir, string modelName = null)
        {
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);
            string path;
            if (string.IsNullOrEmpty(modelName))
                path = Path.GetFullPath(Path.Combine(baseDir, _struxmlFileName));
            else
                path = Path.GetFullPath(Path.Combine(baseDir, Path.ChangeExtension(Path.GetFileName(modelName), "struxml")));
                
            return path;
        }

        public static string GetDocxPath(string baseDir, string modelName = null)
        {
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);
            string path;
            if (string.IsNullOrEmpty(modelName))
                path = Path.GetFullPath(Path.Combine(baseDir, _docxFileName));
            else
                path = Path.GetFullPath(Path.Combine(baseDir, Path.ChangeExtension(Path.GetFileName(modelName), "docx")));

            return path;
        }

        public static string GetStrPath(string baseDir, string modelName = null)
        {
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);
            string path;
            if (string.IsNullOrEmpty(modelName))
                path = Path.GetFullPath(Path.Combine(baseDir, _strFileName));
            else
                path = Path.GetFullPath(Path.Combine(baseDir, Path.ChangeExtension(Path.GetFileName(modelName), "str")));

            return path;
        }
        public static string GetBscPath(string baseDir, string fileName)
        {
            string dir = Path.Combine(baseDir, _scriptsDirectory, _bscDirectory);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            fileName = Path.ChangeExtension(fileName, _bscFileExtension);
            string path = Path.GetFullPath(Path.Combine(dir, fileName));
            return path;
        }
        public static string GetCsvPath(string baseDir, string fileName)
        {
            string dir = Path.Combine(baseDir, _resultsDirectory);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            fileName = Path.ChangeExtension(fileName, _csvFileExtension);
            string path = Path.GetFullPath(Path.Combine(dir, fileName));
            return path;
        }
        public static string GetFdScriptPath(string baseDir, string fileName = "script")
        {
            string dir = Path.Combine(baseDir, _scriptsDirectory);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            fileName = Path.GetFileName(Path.ChangeExtension(fileName, _fdscriptFileExtension));
            string path = Path.GetFullPath(Path.Combine(dir, fileName));
            return path;
        }

        public static bool IsASCII(string filePath)
        {
            // Encode the string using ASCII encoding
            byte[] asciiBytes = Encoding.ASCII.GetBytes(filePath);

            // Decode the ASCII-encoded bytes back to a string
            string decodedString = Encoding.ASCII.GetString(asciiBytes);

            // Compare the original string with the decoded string
            return filePath.Equals(decodedString);
        }

    }
}
