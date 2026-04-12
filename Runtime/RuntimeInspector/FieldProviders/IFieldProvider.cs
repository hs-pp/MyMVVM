using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MyMVVM.RuntimeInspect
{
    public interface IFieldProvider
    {
        public Type GetFieldType();
        public string GetFieldName();
        
        public object GetValue();
        public void SetValue(object value);
        public bool ValueIsNull();
        public void SetValueToNewInstance();

        public void SetupOnChangedFields(Delegate onChangedTriggeredEvent, Action onChangedDelegatesChangedTriggeredEvent);
        public void OnOnChangedButtonPressed();
        public Delegate[] GetOnChangedDelegates();
        public string GetOnChangedFieldName();

        public void ClearProvider();
        
        public static string ConvertToPropertyName(string fieldName)
        {
            if (fieldName.StartsWith("m_"))
            {
                fieldName = fieldName.Substring(2);
            }
            if (fieldName.StartsWith("_"))
            {
                fieldName = fieldName.Substring(1);
            }
            string propName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);
            if (propName == fieldName)
            {
                propName = "_" + propName;
            }
        
            return propName;
        }
        
        public static IFieldProvider CreateFieldProvider(FieldInfo fieldInfo, BaseModel baseModel)
        {
            if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return new ListFieldProvider(fieldInfo, baseModel);
            }
            
            return new DefaultFieldProvider(fieldInfo, baseModel);
        }
        
        public static object CreateNewInstanceOfType(Type type)
        {
            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                Debug.LogError($"No parameterless constructor exists for type {type}");
                return null;
            }

            if (type == typeof(GameObject))
            {
                Debug.LogError("Can't create GameObject instance.");
                return null;
            }

            return Activator.CreateInstance(type);
        }
    }
}