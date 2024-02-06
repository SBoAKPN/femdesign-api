// https://strusoft.com/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using System.Reflection;

using FemDesign;


namespace FemDesign.Calculate
{
    /// <summary>
    /// This is a simple "application" for fd3dstruct.
    /// Retreive process information of a running fd3dstruct. 
    /// Start and run processes (open, analysis, design etc.
    /// </summary>
    [System.Serializable]
    [Obsolete("Will be deprecated in 22.0.0. Use FemDesignConnection instead.")]
    public partial class Application
    {
        /// <summary>
        /// Path to fd3dstruct.
        /// </summary>
        /// <value></value>
        internal string FdPath { get; set; }

        /// <summary>
        /// Version of running fd3dstruct process.
        /// </summary>
        /// <value></value>
        internal string FdVersion { get; set; }

        /// <summary>
        /// Target version of class library.
        /// </summary>
        public string FdTargetVersion = "22";

        public string OutputDir
        {
            get { return _outputDir; }
            set
            {
                if (string.IsNullOrEmpty(value)) // Use temp dir
                {
                    _outputDir = Path.Combine(Directory.GetCurrentDirectory(), "FEM-Design API");
                    _outputDirsToBeDeleted.Add(_outputDir);
                }
                else // Use given directory
                    _outputDir = Path.GetFullPath(value);
            }
        }
        private string _outputDir;
        private List<string> _outputDirsToBeDeleted = new List<string>();


        public Application()
        {
            this.GetProcessInformation();
        }

        /// <summary>
        /// Retreive process information of a running fd3dstruct process.
        /// </summary>
        private void GetProcessInformation()
        {
            Process[] processes = Process.GetProcessesByName("fd3dstruct");
            if (processes.Length > 0)
            {
                // get process information
                Process firstProcess = processes[0];
                try
                {
                    this.FdPath = firstProcess.MainModule.FileName;
                }
                catch (System.Exception e)
                {
                    throw e;
                }
                this.FdVersion = firstProcess.MainModule.FileVersionInfo.FileVersion.Split(new char[] { '.' })[0];
            }

            // Check if process information matches target version
            if (this.FdVersion == null || this.FdVersion != this.FdTargetVersion || this.FdPath == null)
            {
                throw new ProgramNotStartedException("FEM-Design " + this.FdTargetVersion + " - 3D Structure must be running! Start FEM-Design " + this.FdTargetVersion + " - 3D Structure and reload script.");
            }
        }

        /// <summary>
        /// Force shutdown of any running fd3dstruct processes.
        /// </summary>
        private void KillProcesses()
        {
            Process[] processes = Process.GetProcessesByName("fd3dstruct");
            foreach (Process process in processes)
            {
                process.Kill();
            }
        }

        /// <summary>
        /// Check if a fd3dstruct process is running.
        /// </summary>
        internal static void CheckProcess()
        {
            var app = new Application();
            app.GetProcessInformation();
        }

        /// <summary>
        /// Check if a fd3dstruct process is running.
        /// </summary>
        public static bool IsRunning()
        {
            try
            {
                CheckProcess();
                return true;
            }
            catch (System.ArgumentException)
            {
                return false;
            }
        }
        /// <summary>
        /// Get a list of all files open in a fd3dstruct process.
        /// </summary>
        public static List<string> GetOpenFileNames()
        {
            Process[] processes = Process.GetProcessesByName("fd3dstruct");
            return processes.Select(p => p.MainWindowTitle.Split(new string[] { " - " }, System.StringSplitOptions.None)[2]).ToList();
        }

        /// <summary>
        /// Check if any of the files are open in femdesign already.
        /// </summary>
        /// <param name="filenames">The files to check if already opened.</param>
        public static void CheckOpenFiles(List<string> filenames)
        {
            var openFiles = GetOpenFileNames();
            foreach (string filename in filenames)
            {
                string fn = Path.GetFileName(filename);
                if (filename != null && openFiles.Contains(fn))
                {
                    throw new System.Exception($"File {filename} already open in fd3dstruct process. Please close the file and try again. ");
                }
            }
        }

        /// <summary>
        /// Open a .struxml file in fd3dstruct.
        /// </summary>
        /// <param name="struxmlPath"></param>
        /// <param name="killProcess"></param>
        public void OpenStruxml(string struxmlPath, bool killProcess)
        {
            // kill processes
            if (killProcess)
            {
                this.KillProcesses();
            }

            string arguments = "\"" + struxmlPath + "\"";
            string processPath = struxmlPath;

            ProcessStartInfo processStartInfo = new ProcessStartInfo(processPath)
            {
                Arguments = arguments,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(struxmlPath),
                FileName = this.FdPath,
                Verb = "open"
            };

            // start process
            Process.Start(processStartInfo);
        }

        /// <summary>
        /// Run fd3dstruct with a .fdscript.
        /// </summary>
        /// <param name="fdScript"></param>
        /// <param name="killProcess"></param>
        /// <param name="endSession"></param>
        /// <param name="checkOpenFiles"></param>
        /// <param name="minimised"></param>
        /// <returns></returns>
        public bool RunFdScript(FdScript fdScript, bool killProcess, bool endSession, bool checkOpenFiles = true, bool minimised = false)
        {
            fdScript.SerializeFdScript();

            // kill processes
            if (killProcess)
            {
                this.KillProcesses();
            }

            // Check if files are already open
            if (checkOpenFiles) {
                CheckOpenFiles(new List<string> {
                    fdScript.CmdOpen?.Filename,
                    fdScript.CmdSave?.FilePath
                });
            }

            return RunFdScript(fdScript.FdScriptPath, killProcess, endSession, minimised);
        }

        /// <summary>
        /// Run fd3dstruct with a .fdscript.
        /// </summary>
        /// <param name="fdScriptPath">Path to an FdScript</param>
        /// <param name="killProcess"></param>
        /// <param name="endSession"></param>
        /// <returns></returns>
        public bool RunFdScript(string fdScriptPath, bool killProcess, bool endSession, bool minimised = false)
        {
            // kill processes
            if (killProcess)
            {
                this.KillProcesses();
            }

            string arguments = "/s " + "\"" + fdScriptPath + "\"";
            string processPath = fdScriptPath;

            ProcessStartInfo processStartInfo = new ProcessStartInfo(processPath)
            {
                Arguments = arguments,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(fdScriptPath),
                FileName = this.FdPath,
                Verb = "open"
            };

            if (minimised)
            {
                processStartInfo.EnvironmentVariables["FD_NOGUI"] = "1";
                processStartInfo.EnvironmentVariables["FD_NOLOGO"] = "1";
            }

            // start process
            Process process = Process.Start(processStartInfo);

            if (endSession)
            {
                process.WaitForExit();
                return process.HasExited;
            }
            else
            {
                return process.HasExited;
            }


        }

    }

    /// <summary>
    /// Represents errors that occured when FEM-Design is needed to be started.
    /// </summary>
    [Serializable]
    public class ProgramNotStartedException : Exception
    {
        public ProgramNotStartedException() { }
        public ProgramNotStartedException(string message) : base(message) { }
        public ProgramNotStartedException(string message, Exception inner) : base(message, inner) { }
    }
    
}