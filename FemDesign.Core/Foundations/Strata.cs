﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Drawing;
using StruSoft.Interop.StruXml.Data;
using System.ComponentModel;

namespace FemDesign.Foundations
{
    public partial class Strata : NamedEntityBase
    {

        [XmlIgnore]
        internal static int _instance = 0; // Shared instance counter for both PointSupport and LineSupport
        protected override int GetUniqueInstanceCount() => ++_instance;

        [XmlElement("stratum")]
        public List<Stratum> Stratum { get; set; }

        [XmlElement("water_level")]
        public List<WaterLevel> WaterLevel { get; set; }

        [XmlElement("contour")]
        public List<FemDesign.Geometry.Point2d> _contour;

        [XmlIgnore]
        public List<FemDesign.Geometry.Point2d> Contour
        {
            get
            {
                return this._contour;
            }
            set
            {
                if(this.Contour.Count < 3)
                {
                    throw new ArgumentOutOfRangeException("List should have at least 3 items!");
                }
                else
                    this._contour = value;
            }
        }


        [XmlAttribute("depth_level_limit")]
        public double _depthLevelLimit { get; set; }

        [XmlIgnore]
        public double DepthLevelLimit
        {
            get
            {
                return this._depthLevelLimit;
            }
            set
            {
                this._depthLevelLimit = RestrictedDouble.ValueInRange(value, -1000000, 0);
            }
        }

        public Strata(List<Stratum> stratum, List<WaterLevel> waterLevel, List<Geometry.Point2d> contour, double levelLimit, string identifier = "SOIL")
        {
            this.Stratum = stratum;
            this.WaterLevel = waterLevel;
            this.Contour = contour;
            this.DepthLevelLimit = levelLimit;
            this.Identifier = identifier;

            // Strata Object does not have a Guid. Therefore this.EntityCreated() should not be use
            this.EntityModified();
        }
    }

    public partial class Stratum
    {
        [XmlAttribute("material")]
        public Guid Guid { get; set; }

        [XmlIgnore]
        public Materials.Material _material { get; set; }

        [XmlIgnore]
        public Materials.Material Material
        {
            get
            {
                return _material;
            }
            set
            {
                this._material = value;
                this.Guid = this._material.Guid;
            }
        }

        [XmlAttribute("colour")]
        public string _colour { get; set; }
        [XmlIgnore]
        public Color Color
        {
            get
            {
                Color col = System.Drawing.ColorTranslator.FromHtml("#" + this._colour);
                return col;
            }
            set
            {
                this._colour = ColorTranslator.ToHtml(value).Substring(1);
            }
        }


        /// <remarks/>
        [XmlAttribute("default_fillings_colour")]
        [DefaultValue("B97A57")]
        public string _defaultFillingsColour { get; set; }

        [XmlIgnore]
        public Color DefaultFillingsColour
        {
            get
            {
                Color col = System.Drawing.ColorTranslator.FromHtml("#" + this._defaultFillingsColour);
                return col;
            }
            set
            {
                this._defaultFillingsColour = ColorTranslator.ToHtml(value).Substring(1);
            }
        }


    }


    public partial class WaterLevel
    {
        [XmlAttribute("colour")]
        public string _colour { get; set; }
        [XmlIgnore]
        public Color Color
        {
            get
            {
                Color col = System.Drawing.ColorTranslator.FromHtml(this._colour);
                return col;
            }
            set
            {
                this._colour = ColorTranslator.ToHtml(value);
            }
        }

        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}