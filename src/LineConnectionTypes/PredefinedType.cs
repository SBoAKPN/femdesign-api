// https://strusoft.com/

using System.Xml.Serialization;
#region dynamo
using Autodesk.DesignScript.Runtime;
#endregion

namespace FemDesign.LineConnectionTypes
{
    /// <summary>
    /// predefined_type
    ///      
    /// Predefined edge connection. Used when using library edge connections.
    /// </summary>
    [System.Serializable]
    [IsVisibleInDynamoLibrary(false)]
    public class PredefinedType: EntityBase
    {
        [XmlAttribute("name")]
        public string name { get; set; } // name40

        [XmlElement("rigidity")]
        public FemDesign.Releases.RigidityDataType3 rigidity { get; set; } // rigidity_data_type3
        
        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        private PredefinedType()
        {

        }
    }
}