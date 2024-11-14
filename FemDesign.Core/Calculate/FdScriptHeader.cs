// https://strusoft.com/
using System.Xml.Serialization;
using System.Xml.Linq;

namespace FemDesign.Calculate
{
    /// <summary>
    /// fdscript.xsd    
    /// FDSCRIPTHEADER
    /// </summary>
    [XmlRoot("fdscriptheader")]
    [System.Serializable]
    public partial class FdScriptHeader : CmdCommand
    {
        [XmlElement("title")]
        public string Title { get; set; } // SZBUF
        [XmlElement("version")]
        public string Version { get; set; } // SZNAME
        [XmlElement("module")]
        public string Module { get; set; } // SZPATH (?)
        [XmlElement("logfile")]
        public string LogFile { get; set; } // SZPATH

        [XmlAttribute("fContinueOnError")]
        public int _continueOnError { get; set; } = 1;

        [XmlAttribute("fIgnoreParseError")]
        public int _ignoreParseError { get; set; } = 1;

        [XmlIgnore]
        public bool ContinueOnError
        {
            get { return this._continueOnError == 1; }
            set { this._continueOnError = System.Convert.ToInt32(value); }
        }

        [XmlIgnore]
        public bool IgnoreParseError
        {
            get { return this._ignoreParseError == 1; }
            set { this._ignoreParseError = System.Convert.ToInt32(value); }
        }


        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        public FdScriptHeader()
        {
        }
        public FdScriptHeader(string title, string logfile)
        {
            this.Title = title;
            this.Version = FdScript.Version;
            this.Module = "sframe";
            this.LogFile = logfile;
        }

        public FdScriptHeader(string logFilePath)
        {
            Title = "FEM-Design script";
            Version = FdScript.Version;
            Module = "SFRAME";
            LogFile = System.IO.Path.GetFullPath(logFilePath);
        }

        public override XElement ToXElement()
        {
            return Extension.ToXElement<FdScriptHeader>(this);
        }

    }

}