﻿using FemDesign.GenericClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FemDesign.Loads
{
    /// <summary>
    /// mass_point_type
    /// </summary>
    [System.Serializable]
    public partial class Mass : EntityBase, ILoadElement
    {
        /// <summary>
        /// Position of mass.
        /// </summary>
        [XmlElement("position")]
        public Geometry.Point3d Position { get; set; }

        /// <summary>
        /// Value of mass in kg.
        /// </summary>
        [XmlAttribute("value")]
        public double Value { get; set; }

        [XmlElement("colouring")]
        public EntityColor Colouring { get; set; }

        [XmlAttribute("comment")]
        public string Comment { get; set; } = "";

        [XmlAttribute("apply_on_ecc")]
        public bool ApplyOnEcc { get; set; } = false;

        private Mass()
        {
        }

        public Mass(Geometry.Point3d position, double value, bool applyEcc = false, string comment = "")
        {
            this.EntityCreated();
            this.Position = position;
            this.Value = value;
            this.Comment = comment;
            this.ApplyOnEcc = applyEcc;
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} Pos: ({this.Position}) Mass: ({this.Value} kg)";
        }


    }
}
