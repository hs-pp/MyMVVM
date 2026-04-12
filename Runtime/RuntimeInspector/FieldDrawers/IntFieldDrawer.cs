using UnityEngine;
using UnityEngine.UIElements;

namespace MyMVVM.RuntimeInspect
{
    public class IntFieldDrawer : AFieldDrawer<int>
    {
        private VisualElement m_wrapper;
        private IntegerField m_integerField;
        
        protected override void OnCreateLayout()
        {
            m_wrapper = new VisualElement();
            m_wrapper.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Edit", (evt) => { OnEditEnabled(); });
            }));
            Clickable doubleClick = new Clickable(OnEditEnabled);
            doubleClick.activators.Clear();
            doubleClick.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, clickCount = 2 });
            m_wrapper.AddManipulator(doubleClick);
            
            m_integerField = new IntegerField();
            m_integerField.style.minWidth = 140;
            m_integerField.isDelayed = true;
            m_integerField.SetEnabled(false);
            m_integerField.RegisterValueChangedCallback(OnValueChanged);
            
            m_wrapper.Add(m_integerField);
            m_fieldPreviewArea.Add(m_wrapper);
        }

        protected override void OnSetField()
        {
            m_integerField.value = GetValue();
        }

        protected override void OnOnChangedTriggered(int newValue)
        {
            m_integerField.SetValueWithoutNotify(newValue);
        }

        private void OnEditEnabled()
        {
            if (!m_fieldIsSet)
            {
                return;
            }
            
            m_integerField.SetEnabled(true);
            m_integerField.Focus();
        }

        private void OnValueChanged(ChangeEvent<int> evt)
        {
            SetValue(evt.newValue);
            m_integerField.SetEnabled(false);
        }
    }
}