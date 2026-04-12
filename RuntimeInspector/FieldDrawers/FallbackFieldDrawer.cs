using UnityEngine;
using UnityEngine.UIElements;

namespace MyMVVM.RuntimeInspect
{
    public class FallbackFieldDrawer<T> : AFieldDrawer<T>
    {
        private Label m_label;
        
        protected override void OnCreateLayout()
        {
            m_label = new Label();
            m_label.style.fontSize = 11;
            m_label.style.color = Color.gray;
            m_fieldPreviewArea.Add(m_label);
        }
        
        protected override void OnSetField()
        {
            object value = m_fieldProvider.GetValue();
            m_label.text = value == null ? "NULL" : $"{value.ToString()}";
        }

        protected override void OnOnChangedTriggered(T newValue)
        {
            
        }
    }
}