using UnityEngine.UIElements;

namespace MyMVVM.RuntimeInspect
{
    public class StringFieldDrawer : AFieldDrawer<string>
    {
        private VisualElement m_wrapper;
        private TextField m_textField;
        
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
            
            m_textField = new TextField();
            m_textField.style.minWidth = 140;
            m_textField.style.height = 16;
            m_textField.isDelayed = true;
            m_textField.SetEnabled(false);
            m_textField.RegisterValueChangedCallback(OnValueChanged);
            
            m_wrapper.Add(m_textField);
            m_fieldPreviewArea.Add(m_wrapper);
        }

        protected override void OnSetField()
        {
            m_textField.value = GetValue();
        }

        protected override void OnOnChangedTriggered(string newValue)
        {
            m_textField.SetValueWithoutNotify(newValue);
        }

        private void OnEditEnabled()
        {
            if (!m_fieldIsSet)
            {
                return;
            }
            
            m_textField.SetEnabled(true);
            m_textField.Focus();
        }

        private void OnValueChanged(ChangeEvent<string> evt)
        {
            SetValue(evt.newValue);
            m_textField.SetEnabled(false);
        }
    }
}