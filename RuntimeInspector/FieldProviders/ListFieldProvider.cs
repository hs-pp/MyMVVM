using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MyMVVM.RuntimeInspect
{
    public class ListFieldProvider : IFieldProvider
    {
        private FieldInfo m_fieldInfo;
        private BaseModel m_baseModel;
        private MethodInfo m_setMethodInfo;
        private string m_propertyName;
        private Type m_elementType;
        private MethodInfo m_setElementMethod;
        
        public ListFieldProvider(FieldInfo fieldInfo, BaseModel model)
        {
            m_fieldInfo = fieldInfo;
            m_baseModel = model;
            m_setMethodInfo = m_baseModel.GetType().GetMethod(GetSetMethodName(), new[] {m_fieldInfo.FieldType, typeof(bool) });
            
            m_propertyName = IFieldProvider.ConvertToPropertyName(m_fieldInfo.Name);
            m_elementType =  m_fieldInfo.FieldType.GetGenericArguments()[0];
            m_setElementMethod = m_baseModel.GetType().GetMethod(GetSetElementMethodName(), new[] { typeof(int), m_elementType, typeof(bool) });
        }
        
        public Type GetFieldType()
        {
            return m_fieldInfo.FieldType;
        }

        public Type GetElementType()
        {
            return m_elementType;
        }

        public string GetFieldName()
        {
            return IFieldProvider.ConvertToPropertyName(m_fieldInfo.Name);
        }

        public object GetValue()
        {
            return m_fieldInfo.GetValue(m_baseModel) as IList;
        }

        public void SetValue(object value)
        {
            Debug.Log($"Set {GetFieldName()} to {value.ToString()}");
            m_setMethodInfo.Invoke(m_baseModel, new[] { value, true });
        }

        public bool ValueIsNull()
        {
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

        #region OnChanged
        private FieldInfo m_onChangedField;
        private FieldInfo m_onChangedFieldDelegatesChangedField;
        private Delegate m_onChangedTriggeredEvent;
        private Action m_onChangedDelegatesChangedTriggeredEvent;
        
        public void SetupOnChangedFields(Delegate onChangedTriggeredEvent,
            Action onChangedDelegatesChangedTriggeredEvent)
        {
            m_onChangedTriggeredEvent = onChangedTriggeredEvent;
            m_onChangedDelegatesChangedTriggeredEvent = onChangedDelegatesChangedTriggeredEvent;
            
            m_onChangedField = m_baseModel.GetType().GetField(GetOnChangedFieldName(), 
                BindingFlags.Instance | BindingFlags.NonPublic);
            Action onChangedAction = m_onChangedField.GetValue(m_baseModel) as Action;
            onChangedAction += m_onChangedTriggeredEvent as Action;
            m_onChangedField.SetValue(m_baseModel, onChangedAction);
            
            m_onChangedFieldDelegatesChangedField = m_baseModel.GetType()
                .GetField(GetDelegatesChangedFieldName(GetOnChangedFieldName()),
                    BindingFlags.Instance | BindingFlags.NonPublic);
            Action onChangedDelegatesChangedAction = m_onChangedFieldDelegatesChangedField.GetValue(m_baseModel) as Action;
            onChangedDelegatesChangedAction += m_onChangedDelegatesChangedTriggeredEvent;
            m_onChangedFieldDelegatesChangedField.SetValue(m_baseModel, onChangedDelegatesChangedAction);
            onChangedDelegatesChangedAction?.Invoke();
        }

        public void TeardownOnChangedFields()
        {
            if (m_onChangedField != null && m_onChangedTriggeredEvent != null)
            {
                Delegate callbackAction = m_onChangedField.GetValue(m_baseModel) as Delegate;
                callbackAction = Delegate.Remove(callbackAction, m_onChangedTriggeredEvent);
                m_onChangedField.SetValue(m_baseModel, callbackAction);   
            }
            m_onChangedTriggeredEvent = null;
            m_onChangedField = null;
            
            if (m_onChangedFieldDelegatesChangedField != null && m_onChangedDelegatesChangedTriggeredEvent != null)
            {
                Action callbackDelegatesChangedAction = m_onChangedFieldDelegatesChangedField.GetValue(m_baseModel) as Action;
                callbackDelegatesChangedAction -= m_onChangedDelegatesChangedTriggeredEvent;
                m_onChangedFieldDelegatesChangedField.SetValue(m_baseModel, callbackDelegatesChangedAction);   
                callbackDelegatesChangedAction?.Invoke();
            }
            m_onChangedDelegatesChangedTriggeredEvent = null;
            m_onChangedFieldDelegatesChangedField = null;
        }

        public void OnOnChangedButtonPressed()
        {
            Action onChangedAction = m_onChangedField.GetValue(m_baseModel) as Action;
            onChangedAction.Invoke();
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
        #endregion
        
        #region OnElementChanged
        private FieldInfo m_onElementChangedField;
        private FieldInfo m_onElementChangedFieldDelegatesChangedField;
        private Action<int> m_onElementChangedTriggeredEvent;
        private Action m_onElementChangedDelegatesChangedTriggeredEvent;
        private MethodInfo m_triggerElementChangedMethod;
        
        public void SetupOnElementChangedFields(Action<int> onElementChangedTriggeredEvent,
            Action onElementChangedDelegatesChangedTriggeredEvent)
        {
            m_triggerElementChangedMethod = m_baseModel.GetType().GetMethod(GetTriggerElementChangedMethodName(), new[] { typeof(int) });

            m_onElementChangedTriggeredEvent = onElementChangedTriggeredEvent;
            m_onElementChangedDelegatesChangedTriggeredEvent = onElementChangedDelegatesChangedTriggeredEvent;
            
            m_onElementChangedField = m_baseModel.GetType().GetField(GetOnElementChangedFieldName(), 
                BindingFlags.Instance | BindingFlags.NonPublic);
            Action<int> onElementChangedAction = m_onElementChangedField.GetValue(m_baseModel) as Action<int>;
            onElementChangedAction += m_onElementChangedTriggeredEvent;
            m_onElementChangedField.SetValue(m_baseModel, onElementChangedAction);
            
            m_onElementChangedFieldDelegatesChangedField = m_baseModel.GetType()
                .GetField(GetDelegatesChangedFieldName(GetOnElementChangedFieldName()),
                    BindingFlags.Instance | BindingFlags.NonPublic);
            Action onElementChangedDelegatesChangedAction = m_onElementChangedFieldDelegatesChangedField.GetValue(m_baseModel) as Action;
            onElementChangedDelegatesChangedAction += m_onElementChangedDelegatesChangedTriggeredEvent;
            m_onElementChangedFieldDelegatesChangedField.SetValue(m_baseModel, onElementChangedDelegatesChangedAction);
            onElementChangedDelegatesChangedAction?.Invoke();
        }

        private void TeardownOnElementChangedFields()
        {
            if (m_onElementChangedField != null && m_onElementChangedTriggeredEvent != null)
            {
                Delegate callbackAction = m_onElementChangedField.GetValue(m_baseModel) as Delegate;
                callbackAction = Delegate.Remove(callbackAction, m_onElementChangedTriggeredEvent);
                m_onElementChangedField.SetValue(m_baseModel, callbackAction);   
            }
            m_onElementChangedTriggeredEvent = null;
            m_onElementChangedField = null;
            
            if (m_onElementChangedFieldDelegatesChangedField != null && m_onElementChangedDelegatesChangedTriggeredEvent != null)
            {
                Action callbackDelegatesChangedAction = m_onElementChangedFieldDelegatesChangedField.GetValue(m_baseModel) as Action;
                callbackDelegatesChangedAction -= m_onElementChangedDelegatesChangedTriggeredEvent;
                m_onElementChangedFieldDelegatesChangedField.SetValue(m_baseModel, callbackDelegatesChangedAction);
                callbackDelegatesChangedAction?.Invoke();
            }
            m_onElementChangedDelegatesChangedTriggeredEvent = null;
            m_onElementChangedFieldDelegatesChangedField = null;

            m_triggerElementChangedMethod = null;
        }

        public void OnElementChangedButtonPressed(int index)
        {
            m_triggerElementChangedMethod.Invoke(m_baseModel, new object[] { index });
        }

        public Delegate[] GetOnElementChangedDelegates()
        {
            Delegate onElementChangedAction = m_onElementChangedField.GetValue(m_baseModel) as Delegate;
            if (onElementChangedAction != null)
            {
                return onElementChangedAction.GetInvocationList();
            }

            return null;
        }

        private string GetOnElementChangedFieldName()
        {
            return $"m_{m_propertyName}_OnElementChanged";
        }
        private string GetTriggerElementChangedMethodName()
        {
            return $"{m_propertyName}_TriggerElementChanged";
        }
        #endregion
        
        #region OnElementInserted
        private FieldInfo m_onElementInsertedField;
        private FieldInfo m_onElementInsertedFieldDelegatesChangedField;
        private Delegate m_onElementInsertedTriggeredEvent;
        private Action m_onElementInsertedDelegatesChangedTriggeredEvent;
        private MethodInfo m_insertElementMethod;
        
        public void SetupOnElementInsertedChangedFields(Delegate onElementInsertedTriggeredEvent,
            Action onElementInsertedDelegatesChangedTriggeredEvent)
        {
            m_insertElementMethod = m_baseModel.GetType().GetMethod(GetInsertElementMethodName(), new[] { typeof(int), m_elementType, typeof(bool) });
            
            m_onElementInsertedTriggeredEvent = onElementInsertedTriggeredEvent;
            m_onElementInsertedDelegatesChangedTriggeredEvent = onElementInsertedDelegatesChangedTriggeredEvent;
            
            m_onElementInsertedField = m_baseModel.GetType().GetField(GetOnElementInsertedFieldName(), 
                BindingFlags.Instance | BindingFlags.NonPublic);
            Delegate onElementInsertedAction = m_onElementInsertedField.GetValue(m_baseModel) as Delegate;
            onElementInsertedAction = Delegate.Combine(onElementInsertedAction, m_onElementInsertedTriggeredEvent);
            m_onElementInsertedField.SetValue(m_baseModel, onElementInsertedAction);
            
            m_onElementInsertedFieldDelegatesChangedField = m_baseModel.GetType()
                .GetField(GetDelegatesChangedFieldName(GetOnElementInsertedFieldName()),
                    BindingFlags.Instance | BindingFlags.NonPublic);
            Action onElementInsertedDelegatesChangedAction = m_onElementInsertedFieldDelegatesChangedField.GetValue(m_baseModel) as Action;
            onElementInsertedDelegatesChangedAction += m_onElementInsertedDelegatesChangedTriggeredEvent;
            m_onElementInsertedFieldDelegatesChangedField.SetValue(m_baseModel, onElementInsertedDelegatesChangedAction);
            onElementInsertedDelegatesChangedAction?.Invoke();
        }

        public void TeardownOnElementInsertedChangedFields()
        {
            if (m_onElementInsertedField != null && m_onElementInsertedTriggeredEvent != null)
            {
                Delegate callbackAction = m_onElementInsertedField.GetValue(m_baseModel) as Delegate;
                callbackAction = Delegate.Remove(callbackAction, m_onElementInsertedTriggeredEvent);
                m_onElementInsertedField.SetValue(m_baseModel, callbackAction);   
            }
            m_onElementInsertedTriggeredEvent = null;
            m_onElementInsertedField = null;
            
            if (m_onElementInsertedFieldDelegatesChangedField != null && m_onElementInsertedDelegatesChangedTriggeredEvent != null)
            {
                Action callbackDelegatesChangedAction = m_onElementInsertedFieldDelegatesChangedField.GetValue(m_baseModel) as Action;
                callbackDelegatesChangedAction -= m_onElementInsertedDelegatesChangedTriggeredEvent;
                m_onElementInsertedFieldDelegatesChangedField.SetValue(m_baseModel, callbackDelegatesChangedAction);
                callbackDelegatesChangedAction?.Invoke();
            }
            m_onElementInsertedDelegatesChangedTriggeredEvent = null;
            m_onElementInsertedFieldDelegatesChangedField = null;
            
            m_insertElementMethod = null;
        }

        public void OnElementInsertedButtonPressed(int index, object val)
        {
            Debug.Log($"Inserted at {GetFieldName()}[{index}]");
            m_insertElementMethod.Invoke(m_baseModel, new object[] { index, val, true });
        }

        public Delegate[] GetOnElementInsertedDelegates()
        {
            Delegate onElementInsertedAction = m_onElementInsertedField.GetValue(m_baseModel) as Delegate;
            if (onElementInsertedAction != null)
            {
                return onElementInsertedAction.GetInvocationList();
            }
            
            return null;
        }

        private string GetOnElementInsertedFieldName()
        {
            return $"m_{m_propertyName}_OnElementInserted";
        }

        private string GetInsertElementMethodName()
        {
            return $"{m_propertyName}_Insert";
        }
        #endregion
        
        #region OnElementRemoved
        private FieldInfo m_onElementRemovedField;
        private FieldInfo m_onElementRemovedFieldDelegatesChangedField;
        private Action<int> m_onElementRemovedTriggeredEvent;
        private Action m_onElementRemovedDelegatesChangedTriggeredEvent;
        private MethodInfo m_removeElementMethod;

        public void SetupOnElementRemovedChangedFields(Action<int> onElementRemovedTriggeredEvent,
            Action onElementRemovedDelegatesChangedTriggeredEvent)
        {
            m_removeElementMethod = m_baseModel.GetType().GetMethod(GetRemoveElementMethodName(), new[] { typeof(int), typeof(bool) });

            m_onElementRemovedTriggeredEvent = onElementRemovedTriggeredEvent;
            m_onElementRemovedDelegatesChangedTriggeredEvent = onElementRemovedDelegatesChangedTriggeredEvent;
            
            m_onElementRemovedField = m_baseModel.GetType().GetField(GetOnElementRemovedFieldName(), 
                BindingFlags.Instance | BindingFlags.NonPublic);
            Action<int>  onElementRemovedAction = m_onElementRemovedField.GetValue(m_baseModel) as Action<int>;
            onElementRemovedAction += m_onElementRemovedTriggeredEvent;
            m_onElementRemovedField.SetValue(m_baseModel, onElementRemovedAction);
            
            m_onElementRemovedFieldDelegatesChangedField = m_baseModel.GetType().GetField(GetDelegatesChangedFieldName(GetOnElementRemovedFieldName()),
                    BindingFlags.Instance | BindingFlags.NonPublic);
            Action onElementRemovedDelegatesChangedAction = m_onElementRemovedFieldDelegatesChangedField.GetValue(m_baseModel) as Action;
            onElementRemovedDelegatesChangedAction += m_onElementRemovedDelegatesChangedTriggeredEvent;
            m_onElementRemovedFieldDelegatesChangedField.SetValue(m_baseModel, onElementRemovedDelegatesChangedAction);
            onElementRemovedDelegatesChangedAction?.Invoke();
        }

        public void TeardownOnElementRemovedChangedFields()
        {
            if (m_onElementRemovedField != null && m_onElementRemovedTriggeredEvent != null)
            {
                Delegate callbackAction = m_onElementRemovedField.GetValue(m_baseModel) as Delegate;
                callbackAction = Delegate.Remove(callbackAction, m_onElementRemovedTriggeredEvent);
                m_onElementRemovedField.SetValue(m_baseModel, callbackAction);   
            }
            m_onElementRemovedTriggeredEvent = null;
            m_onElementRemovedField = null;
            
            if (m_onElementRemovedFieldDelegatesChangedField != null && m_onElementRemovedDelegatesChangedTriggeredEvent != null)
            {
                Action callbackDelegatesChangedAction = m_onElementRemovedFieldDelegatesChangedField.GetValue(m_baseModel) as Action;
                callbackDelegatesChangedAction -= m_onElementRemovedDelegatesChangedTriggeredEvent;
                m_onElementRemovedFieldDelegatesChangedField.SetValue(m_baseModel, callbackDelegatesChangedAction);
                callbackDelegatesChangedAction?.Invoke();
            }
            m_onElementRemovedDelegatesChangedTriggeredEvent = null;
            m_onElementRemovedFieldDelegatesChangedField = null;
            
            m_removeElementMethod = null;
        }

        public void OnElementRemovedButtonPressed(int index)
        {
            Debug.Log($"Removed at {GetFieldName()}[{index}]");
            m_removeElementMethod.Invoke(m_baseModel, new object[] { index, true });
        }

        public Delegate[] GetOnElementRemovedDelegates()
        {
            Delegate onElementInsertedAction = m_onElementRemovedField.GetValue(m_baseModel) as Delegate;
            if (onElementInsertedAction != null)
            {
                return onElementInsertedAction.GetInvocationList();
            }
            
            return null;
        }

        public string GetOnElementRemovedFieldName()
        {
            return $"m_{m_propertyName}_OnElementRemoved";
        }

        public string GetRemoveElementMethodName()
        {
            return $"{m_propertyName}_RemoveAt";
        }
        #endregion

        public void ClearProvider()
        {
            TeardownOnChangedFields();
            TeardownOnElementChangedFields();
            TeardownOnElementInsertedChangedFields();
            TeardownOnElementRemovedChangedFields();
            
            m_fieldInfo = null;
            m_baseModel = null;
            m_propertyName = "";
            m_elementType = null;
            m_setElementMethod = null;
        }
        
        public ListElementFieldProvider CreateListElementFieldProvider(int index)
        {
            ListElementFieldProvider elementFieldProvider = new(
                m_fieldInfo.GetValue(m_baseModel) as IList, 
                index, 
                m_elementType, 
                m_baseModel, 
                m_setElementMethod, 
                m_onElementChangedField,
                m_onElementChangedFieldDelegatesChangedField);

            if (elementFieldProvider.ValueIsNull())
            {
                elementFieldProvider.SetValueToNewInstance();
            }
            
            return elementFieldProvider;
        }

        public List<ListElementFieldProvider> GetElementFieldProviders()
        {
            IList listInstance = m_fieldInfo.GetValue(m_baseModel) as IList;
            if (listInstance == null)
            {
                return new();
            }
            
            List<ListElementFieldProvider> providers = new();
            for (int i = 0; i < listInstance.Count; i++)
            {
                providers.Add(CreateListElementFieldProvider(i));
            }

            return providers;
        }
        
        private string GetSetElementMethodName()
        {
            return $"{m_propertyName}_Set";
        }
        
        private string GetSetMethodName()
        {
            return $"{IFieldProvider.ConvertToPropertyName(m_fieldInfo.Name)}_Set";
        }
                
        private string GetDelegatesChangedFieldName(string fieldName)
        {
            return $"{fieldName}_OnDelegatesChanged";
        }
    }
}