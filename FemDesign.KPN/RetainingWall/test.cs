//public static Material ConcreteMaterialProperties(Material material, double mass)
//{
//    if (material.Concrete != null)
//    {
//        // deep clone. downstreams objs will have contain changes made in this method, upstream objs will not.
//        Material newMaterial = material.DeepClone();

//        // downstream and uppstream objs will NOT share guid.
//        newMaterial.EntityCreated();

//        // set parameters
//        newMaterial.Concrete.SetMaterialParameters(mass);
//        newMaterial.EntityModified();

//        // return
//        return newMaterial;
//    }
//    else
//    {
//        throw new System.ArgumentException("Material must be concrete!");
//    }
//}
//internal void SetMaterialParameters(double mass)
//        {
//            this.Mass = mass;
//        }
