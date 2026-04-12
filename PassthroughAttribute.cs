using System;

namespace MyMVVM
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PassthroughAttribute : Attribute
    {
        public string ModelName;
        public string ModelPropertyName;
        public string NameOverride;
        public PassthroughAttribute(string modelName, string modelPropertyName, string nameOverride = null)
        {
            ModelName = modelName;
            ModelPropertyName = modelPropertyName;
            NameOverride = nameOverride;
        }
    }
}