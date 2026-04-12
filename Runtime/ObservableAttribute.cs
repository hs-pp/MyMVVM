using System;

namespace MyMVVM
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ObservableAttribute : Attribute
    {
        public ObservableAttribute(string nameOverride = null) { }
    }
}