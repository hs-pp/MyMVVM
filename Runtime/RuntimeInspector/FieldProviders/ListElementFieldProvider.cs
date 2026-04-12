using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace MyMVVM.RuntimeInspect
{
    public class ListElementFieldProvider : IFieldProvider
    {
        private IList m_listInstance;
        private int m_index;
        private Type m_fieldType;
        private BaseModel m_baseModel;
        private MethodInfo m_setMethod;
        
        private FieldInfo m_onElementChangedField;
        private FieldInfo m_onElementChangedDelegatesChangedField;
        private Delegate m_onChangedTriggeredEvent;

        public ListElementFieldProvider(IList listInstance, int index, Type fieldType, BaseModel baseModel,
            MethodInfo setMethod, FieldInfo onElementChangedField, FieldInfo onElementChangedDelegatesChangedField)
        {
            m_listInstance = listInstance;
            m_index = index;
            m_fieldType = fieldType;
            m_baseModel = baseModel;
            m_setMethod = setMethod;
            m_onElementChangedField = onElementChangedField;
            m_onElementChangedDelegatesChangedField = onElementChangedDelegatesChangedField;
        }

        public Type GetFieldType()
        {
            return m_fieldType;
        }
        
        public string GetFieldName()
        {
            return "";
        }

        public object GetValue()
        {
            return m_listInstance[m_index];
        }
        
        public void SetValue(object value)
        {
            Debug.Log($"Set {m_listInstance.GetType().Name}[{m_index}] to {value.ToString()}");
            m_setMethod.Invoke(m_baseModel, new[] { m_index, value, true});
        }

        public bool ValueIsNull()
        {
            if (GetFieldType().IsValueType)
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

        public void SetIndex(int index)
        {
            m_index = index;
            OnElementChangedCallback(m_index);
        }

        public int GetIndex()
        {
            return m_index;
        }

        public void SetupOnChangedFields(Delegate onChangedTriggeredEvent, Action onChangedDelegatesChangedTriggeredEvent)
        {
            m_onChangedTriggeredEvent = onChangedTriggeredEvent;
            
            Action<int> callbackAction = m_onElementChangedField.GetValue(m_baseModel) as Action<int>;
            callbackAction += OnElementChangedCallback;
            m_onElementChangedField.SetValue(m_baseModel, callbackAction);

            (m_onElementChangedDelegatesChangedField.GetValue(m_baseModel) as Action).Invoke();
        }

        private void OnElementChangedCallback(int index)
        {
            if (index != m_index)
            {
                return;
            }

            m_onChangedTriggeredEvent?.DynamicInvoke(m_listInstance[m_index]);
        }

        public void OnOnChangedButtonPressed()
        {
            (m_onElementChangedField.GetValue(m_baseModel) as Action<int>).Invoke(m_index);
        }

        public Delegate[] GetOnChangedDelegates()
        {
            return null;
        }
        
        public string GetOnChangedFieldName()
        {
            // This is a list element so there's no OnChanged.
            return "";
        }
        
        public void ClearProvider()
        {
            if (m_onElementChangedField != null)
            {
                Action<int> callbackAction = m_onElementChangedField.GetValue(m_baseModel) as Action<int>;
                callbackAction -= OnElementChangedCallback;
                m_onElementChangedField.SetValue(m_baseModel, callbackAction);
                
                (m_onElementChangedDelegatesChangedField.GetValue(m_baseModel) as Action)?.Invoke();
            }
            m_onChangedTriggeredEvent = null;
            
            // Don't clear these things. This FieldProvider gets reused.
            // m_onElementChangedField = null;
            //
            // m_listInstance = null;
            // m_index = 0;
            // m_fieldType = null;
            // m_baseModel = null;
            // m_setMethod = null;
        }
    }
}