//using FemDesign.Materials;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Retainingwall_dev
//{
//    internal class test
//    {
//        public static Material ConcreteMaterialProperties(Material material, double creepUls = 0, double creepSlq = 0, double creepSlf = 0, double creepSlc = 0, double shrinkage = 0)
//        {
//            if (material.Concrete != null)
//            {
//                // deep clone. downstreams objs will have contain changes made in this method, upstream objs will not.
//                Material newMaterial = material.DeepClone();

//                // downstream and uppstream objs will NOT share guid.
//                newMaterial.EntityCreated();

//                // set parameters
//                newMaterial.Concrete.SetMaterialParameters(creepUls, creepSlq, creepSlf, creepSlc, shrinkage);
//                newMaterial.EntityModified();

//                // return
//                return newMaterial;
//            }
//            else
//            {
//                throw new System.ArgumentException("Material must be concrete!");
//            }
//        }
//        internal void SetMaterialParameters(double creepUls, double creepSlq, double creepSlf, double creepSlc, double shrinkage)
//        {
//            this.Creep = creepUls;

//        }
//    }
//}
