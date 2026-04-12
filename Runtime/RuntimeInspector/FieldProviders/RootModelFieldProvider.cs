using System;
using System.Reflection;

namespace MyMVVM.RuntimeInspect
{
    public class RootModelFieldProvider : IFieldProvider
    {
        private BaseModel m_baseModel;
        private string m_label;
        
        private FieldInfo m_callbackField;
        private FieldInfo m_callbackDelegatesChangedField;
        private Delegate m_onChangedTriggeredEvent;
        private Action m_onChangedDelegatesChangedTriggeredEvent;
        
        public RootModelFieldProvider(BaseModel model, string label)
        {
            m_baseModel = model;
            m_label = label;
        }
        
        public Type GetFieldType()
        {
            return m_baseModel.GetType();
        }

        public string GetFieldName()
        {
            return m_label;
        }

        public string GetOnChangedFieldName()
        {
            return "m_onChanged";
        }

        public object GetValue()
        {
            return m_baseModel;
        }

        public void SetValue(object value)
        {
            
        }
        
        public bool ValueIsNull()
        {
            return false;
        }

        public void SetValueToNewInstance() // Can we even do this??
        {
            
        }

        public void SetupOnChangedFields(Delegate onChangedTriggeredEvent, Action onChangedDelegatesChangedTriggeredEvent)
        {
            m_onChangedTriggeredEvent = onChangedTriggeredEvent;
            m_onChangedDelegatesChangedTriggeredEvent = onChangedDelegatesChangedTriggeredEvent;
            
            m_callbackField = m_baseModel.GetType().GetField(GetOnChangedFieldName(), 
                BindingFlags.NonPublic | BindingFlags.Instance);
            Delegate callbackAction = m_callbackField.GetValue(m_baseModel) as Delegate;
            callbackAction = Delegate.Combine(callbackAction, m_onChangedTriggeredEvent);
            m_callbackField.SetValue(m_baseModel, callbackAction);

            m_callbackDelegatesChangedField = m_baseModel.GetType().GetField(GetOnChangedDelegatesChangedCallbackFieldName(),
                BindingFlags.NonPublic | BindingFlags.Instance);
            Action callbackDelegatesChangedAction = m_callbackDelegatesChangedField.GetValue(m_baseModel) as Action;
            callbackDelegatesChangedAction += m_onChangedDelegatesChangedTriggeredEvent;
            m_callbackDelegatesChangedField.SetValue(m_baseModel, callbackDelegatesChangedAction);
            callbackDelegatesChangedAction?.Invoke();
        }

        public void OnOnChangedButtonPressed()
        {
            Action<BaseModel> callbackAction = m_callbackField.GetValue(m_baseModel) as Action<BaseModel>;
            callbackAction?.Invoke(m_baseModel);
        }

        public Delegate[] GetOnChangedDelegates()
        {
            Delegate onChangedAction = m_callbackField.GetValue(m_baseModel) as Delegate;
            if (onChangedAction != null)
            {
                return onChangedAction.GetInvocationList();
            }

            return null;
        }

        public void ClearProvider()
        {
            if (m_callbackField != null && m_onChangedTriggeredEvent != null)
            {
                Delegate callbackAction = m_callbackField.GetValue(m_baseModel) as Delegate;
                callbackAction = Delegate.Remove(callbackAction, m_onChangedTriggeredEvent);
                m_callbackField.SetValue(m_baseModel, callbackAction);   
            }
            m_onChangedTriggeredEvent = null;
            
            if (m_callbackDelegatesChangedField != null && m_onChangedDelegatesChangedTriggeredEvent != null)
            {
                Action callbackDelegatesChangedAction = m_callbackDelegatesChangedField.GetValue(m_baseModel) as Action;
                callbackDelegatesChangedAction -= m_onChangedDelegatesChangedTriggeredEvent;
                m_callbackDelegatesChangedField.SetValue(m_baseModel, callbackDelegatesChangedAction);
                callbackDelegatesChangedAction?.Invoke();
            }
            m_onChangedDelegatesChangedTriggeredEvent = null;
        }
        
        private string GetOnChangedDelegatesChangedCallbackFieldName()
        {
            return $"{GetOnChangedFieldName()}_OnDelegatesChanged";
        }
    }
}