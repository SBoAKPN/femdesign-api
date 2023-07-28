// https://strusoft.com/
using System;
using System.Xml.Serialization;


namespace FemDesign.Calculate
{
    /// <summary>
    /// fdscript.xsd
    /// ANALFREQ
    /// </summary>
    public partial class Freq
    {
        [XmlAttribute("Numshapes")]
        public int NumShapes { get; set; } // int
        [XmlAttribute("MaxSturm")]
        public int MaxSturm { get; set; } // int
        [XmlAttribute("X")]
        public bool _x; // bool
        [XmlIgnore]
        public bool X
        {
            get
            {
                return this._x;
            }
            set
            {
                this._x = value;
            }
        }
        [XmlAttribute("Y")]
        public bool _y; // bool
        [XmlIgnore]
        public bool Y
        {
            get
            {
                return this._y;
            }
            set
            {
                this._y = value;
            }
        }
        [XmlAttribute("Z")]
        public bool _z; // bool
        [XmlIgnore]
        public bool Z
        {
            get
            {
                return this._z;
            }
            set
            {
                this._z = value;
            }
        }
        [XmlAttribute("top")]
        public double Top { get; set; } // double

        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        private Freq()
        {
            
        }

        /// <summary>
        /// Define calculation parameters for an eigenfrequency calculation.
        /// </summary>
        /// <param name="numShapes">Number of shapes.</param>
        /// <param name="maxSturm">Max number of Sturm check steps (checking missing eigenvalues).</param>
        /// <param name="x">Consider masses in global x-direction.</param>
        /// <param name="y">Consider masses in global y-direction.</param>
        /// <param name="z">Consider masses in global z-direction.</param>
        /// <param name="top">Top of substructure. Masses on this level and below are not considered in Eigenfrequency calculation.</param>
        public Freq(int numShapes = 3, int maxSturm = 0, bool x = true, bool y = true, bool z = true, double top = -0.01)
        {
            this.NumShapes = numShapes;
            this.MaxSturm = maxSturm;
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Top = top;
        }

        /// <summary>
        /// Define default calculation parameters for an eigenfrequency calculation.
        /// </summary>
        /// <returns></returns>
        public static Freq Default()
        {
            return new Freq(3, 0, true, true, true, -0.01);
        }

    }
}