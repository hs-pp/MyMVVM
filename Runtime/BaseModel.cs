using System;

namespace MyMVVM
{
    [Serializable]
    public abstract class BaseModel : IDisposable
    {
        protected virtual void AutoGenInit() { }

        public BaseModel()
        {
            AutoGenInit();
        }
        
        protected Action<BaseModel> m_onChanged;
        protected Action m_onChanged_OnDelegatesChanged;
        public event Action<BaseModel> OnChanged
        {
            add
            {
                m_onChanged += value;
                //value?.Invoke(this); DONT DO THIS. There's a million things that bind to OnChanged.
                m_onChanged_OnDelegatesChanged?.Invoke();
            }
            remove
            {
                m_onChanged -= value;
                m_onChanged_OnDelegatesChanged?.Invoke();
            }
        }
        
        ~BaseModel()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (!m_wasDisposed)
            {
                OnDispose();
                m_wasDisposed = true;
            }
        }

        private bool m_wasDisposed = false;
        protected virtual void OnDispose(){ }
    }
}