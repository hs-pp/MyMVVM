using System;
using System.Reflection;
using UnityEngine;

namespace MyMVVM.RuntimeInspect
{
    public class DefaultFieldProvider : IFieldProvider
    {
        private FieldInfo m_fieldInfo;
        private BaseModel m_baseModel;
        private MethodInfo m_setMethodInfo;
        private string m_propertyName;
        private FieldInfo m_onChangedField;
        private FieldInfo m_onChangedDelegatesChangedField;
        private Delegate m_onChangedTriggeredEvent;
        private Action m_onChangedDelegatesChangedTriggeredEvent;

        public DefaultFieldProvider(FieldInfo fieldInfo, BaseModel baseModel)
        {
            m_fieldInfo = fieldInfo;
            m_baseModel = baseModel;
            m_propertyName = IFieldProvider.ConvertToPropertyName(m_fieldInfo.Name);
            
            m_setMethodInfo = m_baseModel.GetType().GetMethod(GetSetMethodName(), new[] { GetFieldType(), typeof(bool) });
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
            Debug.Log($"Set {GetFieldName()} to {value.ToString()}");
            m_setMethodInfo.Invoke(m_baseModel, new[] { value, true });
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
            m_onChangedTriggeredEvent = onChangedTriggeredEvent;
            m_onChangedDelegatesChangedTriggeredEvent = onChangedDelegatesChangedTriggeredEvent;
            
            m_onChangedField = m_baseModel.GetType().GetField(GetOnChangedFieldName(), 
                BindingFlags.NonPublic | BindingFlags.Instance);
            Delegate callbackAction = m_onChangedField.GetValue(m_baseModel) as Delegate;
            callbackAction = Delegate.Combine(callbackAction, m_onChangedTriggeredEvent);
            m_onChangedField.SetValue(m_baseModel, callbackAction);

            m_onChangedDelegatesChangedField = m_baseModel.GetType().GetField(GetOnChangedDelegatesChangedFieldName(),
                BindingFlags.NonPublic | BindingFlags.Instance);
            Action callbackDelegatesChangedAction = m_onChangedDelegatesChangedField.GetValue(m_baseModel) as Action;
            callbackDelegatesChangedAction += m_onChangedDelegatesChangedTriggeredEvent;
            m_onChangedDelegatesChangedField.SetValue(m_baseModel, callbackDelegatesChangedAction);
            callbackDelegatesChangedAction?.Invoke();
        }

        public void OnOnChangedButtonPressed()
        {
            Delegate callbackAction = m_onChangedField.GetValue(m_baseModel) as Delegate;
            callbackAction.DynamicInvoke(GetValue());
        }

        public Delegate[] GetOnChangedDelegates()
        {
            Delegate onChangedAction = m_onChangedField.GetValue(m_baseModel) as Delegate;
            if (onChangedAction != null)
            {
                return onChangedAction.GetInvocationList();
            }

            return null;
        }
        
        public string GetOnChangedFieldName()
        {
            return $"m_{m_propertyName}_OnChanged";
        }

        public void ClearProvider()
        {
            if (m_onChangedField != null && m_onChangedTriggeredEvent != null)
            {
                Delegate callbackAction = m_onChangedField.GetValue(m_baseModel) as Delegate;
                callbackAction = Delegate.Remove(callbackAction, m_onChangedTriggeredEvent);
                m_onChangedField.SetValue(m_baseModel, callbackAction);   
            }
            m_onChangedTriggeredEvent = null;
            m_onChangedField = null;
            
            if (m_onChangedDelegatesChangedField != null && m_onChangedDelegatesChangedTriggeredEvent != null)
            {
                Action callbackDelegatesChangedAction = m_onChangedDelegatesChangedField.GetValue(m_baseModel) as Action;
                callbackDelegatesChangedAction -= m_onChangedDelegatesChangedTriggeredEvent;
                m_onChangedDelegatesChangedField.SetValue(m_baseModel, callbackDelegatesChangedAction);
                callbackDelegatesChangedAction?.Invoke();
            }
            m_onChangedDelegatesChangedTriggeredEvent = null;
            m_onChangedDelegatesChangedField = null;

            m_fieldInfo = null;
            m_baseModel = null;
            m_setMethodInfo = null;
        }
        
        private string GetSetMethodName()
        {
            return $"{m_propertyName}_Set";
        }
        
        private string GetOnChangedDelegatesChangedFieldName()
        {
            return $"{GetOnChangedFieldName()}_OnDelegatesChanged";
        }
    }
}