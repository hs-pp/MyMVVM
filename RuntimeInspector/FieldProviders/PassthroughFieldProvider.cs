using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MyMVVM.RuntimeInspect
{
    public class PassthroughFieldProvider : IFieldProvider
    {
        // Original field
        private FieldInfo m_fieldInfo;
        private BaseModel m_baseModel;

        // Passthrough model
        private PassthroughAttribute m_passthroughAttribute;
        private FieldInfo m_passthroughModelField;
        private bool m_passthroughModelIsObservable;
        private BaseModel m_passthroughModel;
        private MethodInfo m_setMethod;
        private FieldInfo m_passthroughModelOnChangedField;
        public Action OnPassthroughModelChanged;
        public Action OnNonObservablePassthroughModelChanged;
        
        // Passthrough field
        private FieldInfo m_passthroughPropertyField;
        private MethodInfo m_passthroughPropertySetMethod;
        private FieldInfo m_onChangedField;
        private FieldInfo m_onChangedDelegatesChangedField;
        private Delegate m_onChangedTriggeredEvent;
        private Action m_onChangedDelegatesChangedTriggeredEvent;
        
        public PassthroughFieldProvider(FieldInfo fieldInfo, BaseModel baseModel)
        {
            m_fieldInfo = fieldInfo;
            m_baseModel = baseModel;
            
            m_passthroughAttribute = m_fieldInfo.GetCustomAttribute<PassthroughAttribute>();
            
            // Passthrough Model
            m_passthroughModelField = m_baseModel.GetType().GetField(m_passthroughAttribute.ModelName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            m_passthroughModelIsObservable = m_passthroughModelField.GetCustomAttribute<ObservableAttribute>() != null;
            
            m_passthroughModel = m_passthroughModelField.GetValue(m_baseModel) as BaseModel;
            m_setMethod = m_baseModel.GetType().GetMethod(GetPassthroughModelSetMethodName(), 
                new[] { GetPassthroughModelType(), typeof(bool) });
            
            // Passthrough Model OnChanged hookup
            if (m_passthroughModelIsObservable)
            {
                m_passthroughModelOnChangedField = m_baseModel.GetType().GetField(
                    GetPassthroughModelOnChangedFieldName(), BindingFlags.NonPublic | BindingFlags.Instance);

                Delegate passthroughModelChangedAction = m_passthroughModelOnChangedField.GetValue(m_baseModel) as Delegate;
                passthroughModelChangedAction =
                    Delegate.Combine(passthroughModelChangedAction, (Action<BaseModel>)HandlePassthroughModelChanged);
                m_passthroughModelOnChangedField.SetValue(m_baseModel, passthroughModelChangedAction);
            }

            // Passthrough Property
            m_passthroughPropertyField = GetPassthroughModelType().GetField(m_passthroughAttribute.ModelPropertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            m_passthroughPropertySetMethod = GetPassthroughModelType().GetMethod(GetPassthroughPropertySetMethodName(),
                new[] { GetFieldType(), typeof(bool) });
            
            m_onChangedField = GetPassthroughModelType().GetField(GetOnChangedFieldName(), BindingFlags.NonPublic | BindingFlags.Instance);
            m_onChangedDelegatesChangedField = GetPassthroughModelType().GetField(GetOnChangedDelegatesChangedFieldName(),
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public Type GetFieldType()
        {
            return m_fieldInfo.FieldType;
        }

        public string GetFieldName()
        {
            if (!string.IsNullOrEmpty(m_passthroughAttribute.NameOverride))
            {
                return m_passthroughAttribute.NameOverride;
            }
            return IFieldProvider.ConvertToPropertyName(m_passthroughAttribute.ModelPropertyName);
        }

        private Type GetPassthroughModelType()
        {
            return m_passthroughModelField.FieldType;
        }

        public bool PassthroughModelExists()
        {
            return m_passthroughModel != null;
        }

        public void CreatePassthroughModelInstanceIfNull()
        {
            if (PassthroughModelExists())
            {
                return;
            }

            BaseModel newPassthroughModel = Activator.CreateInstance(GetPassthroughModelType()) as BaseModel;
            if (m_passthroughModelIsObservable)
            {
                m_setMethod.Invoke(m_baseModel, new object[] { newPassthroughModel, true });
            }
            else
            {
                m_passthroughModelField.SetValue(m_baseModel, newPassthroughModel);
                HandlePassthroughModelChanged(newPassthroughModel);
                OnNonObservablePassthroughModelChanged?.Invoke();
            }
        }

        public object GetValue()
        {
            if (m_passthroughModel == null)
            {
                return null;
            }
            return m_passthroughPropertyField.GetValue(m_passthroughModel);
        }

        public void SetValue(object value)
        {
            if (m_passthroughModel == null)
            {
                return;
            }

            Debug.Log($"Set {GetFieldName()} to {value.ToString()}");
            m_passthroughPropertySetMethod.Invoke(m_passthroughModel, new object[] { value, true });
        }

        public bool ValueIsNull()
        {
            if (m_passthroughModel == null) // If the passthrough model itself is null, we dont know if the value is null so just return false.
            {
                return true;
            }
            
            if (GetFieldType().IsValueType)
            {
                return false;
            }

            return GetValue() == null;
        }

        public void SetValueToNewInstance()
        {
            if (m_passthroughModel == null)
            {
                return;
            }
         
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

            if (m_passthroughModel == null)
            {
                return;
            }
            
            Delegate callbackAction = m_onChangedField.GetValue(m_passthroughModel) as Delegate;
            callbackAction = Delegate.Combine(callbackAction, m_onChangedTriggeredEvent);
            m_onChangedField.SetValue(m_passthroughModel, callbackAction);
            
            Action callbackDelegatesChangedAction = m_onChangedDelegatesChangedField.GetValue(m_passthroughModel) as Action;
            callbackDelegatesChangedAction += m_onChangedDelegatesChangedTriggeredEvent;
            m_onChangedDelegatesChangedField.SetValue(m_passthroughModel, callbackDelegatesChangedAction);
            callbackDelegatesChangedAction?.Invoke();
        }

        private void TeardownOnChangedFields()
        {
            if (m_passthroughModel == null)
            {
                return;
            }

            Delegate callbackAction = m_onChangedField.GetValue(m_passthroughModel) as Delegate;
            callbackAction = Delegate.Remove(callbackAction, m_onChangedTriggeredEvent);
            m_onChangedField.SetValue(m_passthroughModel, callbackAction);
            
            Action callbackDelegatesChangedAction = m_onChangedDelegatesChangedField.GetValue(m_passthroughModel) as Action;
            callbackDelegatesChangedAction -= m_onChangedDelegatesChangedTriggeredEvent;
            m_onChangedDelegatesChangedField.SetValue(m_passthroughModel, callbackDelegatesChangedAction);
            callbackDelegatesChangedAction?.Invoke();
        }

        public void CheckIfPassthroughModelChanged()
        {
            BaseModel current = m_passthroughModelField.GetValue(m_baseModel) as BaseModel;
            HandlePassthroughModelChanged(current);
        }
        
        private void HandlePassthroughModelChanged(BaseModel newValue)
        {
            if (m_passthroughModel == newValue)
            {
                return;
            }
            
            m_passthroughModel = newValue;
            TeardownOnChangedFields();
            SetupOnChangedFields(m_onChangedTriggeredEvent, m_onChangedDelegatesChangedTriggeredEvent);
            OnPassthroughModelChanged?.Invoke();
        }

        public void OnOnChangedButtonPressed()
        {
            if (m_passthroughModel == null)
            {
                return;
            }
            
            Delegate callbackAction = m_onChangedField.GetValue(m_passthroughModel) as Delegate;
            callbackAction.DynamicInvoke(GetValue());
        }

        public Delegate[] GetOnChangedDelegates()
        {
            if (m_passthroughModel == null)
            {
                return Array.Empty<Delegate>();
            }
            
            Delegate onChangedAction = m_onChangedField.GetValue(m_passthroughModel) as Delegate;
            if (onChangedAction != null)
            {
                return onChangedAction.GetInvocationList();
            }

            return null;
        }
        
        private string GetPassthroughModelSetMethodName()
        {
            return $"{IFieldProvider.ConvertToPropertyName(m_passthroughAttribute.ModelName)}_Set";
        }

        private string GetPassthroughPropertySetMethodName()
        {
            return $"{IFieldProvider.ConvertToPropertyName(m_passthroughAttribute.ModelPropertyName)}_Set";
        }

        private string GetPassthroughModelOnChangedFieldName()
        {
            return $"m_{IFieldProvider.ConvertToPropertyName(m_passthroughAttribute.ModelName)}_OnChanged";
        }
        
        public string GetOnChangedFieldName()
        {
            return $"m_{IFieldProvider.ConvertToPropertyName(m_passthroughAttribute.ModelPropertyName)}_OnChanged";
        }

        private string GetOnChangedDelegatesChangedFieldName()
        {
            return $"{GetOnChangedFieldName()}_OnDelegatesChanged";
        }

        public string GetPassthroughIndicatorTooltipString()
        {
            return $"Passthrough\n" +
                   $"Model: {m_passthroughModelField.Name} ({GetPassthroughModelType().Name})\n" +
                   $"Field: {m_passthroughPropertyField.Name} ({GetFieldType().Name})";
        }
        
        public void ClearProvider()
        {
            if (m_passthroughModelIsObservable)
            {
                Delegate passthroughModelChangedAction = m_passthroughModelOnChangedField.GetValue(m_baseModel) as Delegate;
                passthroughModelChangedAction = Delegate.Remove(passthroughModelChangedAction, 
                    (Action<BaseModel>)HandlePassthroughModelChanged);
                m_passthroughModelOnChangedField.SetValue(m_baseModel, passthroughModelChangedAction);
            }

            TeardownOnChangedFields();
        }

        public IFieldProvider GetDirectFieldProvider()
        {
            if (m_passthroughModel == null)
            {
                return null;
            }
            
            if (m_passthroughPropertyField.FieldType.IsGenericType && m_passthroughPropertyField.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return new ListFieldProvider(m_passthroughPropertyField, m_passthroughModel);
            }
            
            return new DefaultFieldProvider(m_passthroughPropertyField, m_passthroughModel);
        }
    }
}