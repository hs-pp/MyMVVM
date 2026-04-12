using System;
using System.Reflection;
using UnityEngine;

namespace MyMVVM.RuntimeInspect
{
    public class NonObservableModelFieldProvider : IFieldProvider
    {
        private FieldInfo m_fieldInfo;
        private BaseModel m_baseModel;
        private string m_propertyName;

        public NonObservableModelFieldProvider(FieldInfo fieldInfo, BaseModel baseModel)
        {
            m_fieldInfo = fieldInfo;
            m_baseModel = baseModel;
            m_propertyName = IFieldProvider.ConvertToPropertyName(m_fieldInfo.Name);
        }
        
        public Type GetFieldType()
        {
            return m_fieldInfo.FieldType;
        }

        public string GetFieldName()
        {
            return m_propertyName;
        }

        public object GetValue()
        {
            return m_fieldInfo.GetValue(m_baseModel);
        }

        public void SetValue(object value)
        {
            Debug.LogError("Cannot set field value to non-observable model");
        }

        public bool ValueIsNull()
        {
            if (GetFieldType().IsValueType)
            {
                return false;
            }

            if (GetFieldType() == typeof(String)) // Uninitialized strings get recognized as String and not string.
            {
                return false;
            }

            return GetValue() == null;
        }

        public void SetValueToNewInstance()
        {
            object newInstance = IFieldProvider.CreateNewInstanceOfType(GetFieldType());
            if (newInstance != null)
            {
                SetValue(newInstance);
            }
        }

        public void SetupOnChangedFields(Delegate onChangedTriggeredEvent, Action onChangedDelegatesChangedTriggeredEvent)
        {
        }

        public void OnOnChangedButtonPressed()
        {
        }

        public Delegate[] GetOnChangedDelegates()
        {
            return null;
        }
        
        public string GetOnChangedFieldName()
        {
            return $"";
        }

        public void ClearProvider()
        {
            m_fieldInfo = null;
            m_baseModel = null;
        }
        
        private string GetSetMethodName()
        {
            return $"{m_propertyName}_Set";
        }
    }
}