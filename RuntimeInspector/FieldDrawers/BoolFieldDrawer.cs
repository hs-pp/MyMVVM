using UnityEngine.UIElements;

namespace MyMVVM.RuntimeInspect
{
    public class BoolFieldDrawer : AFieldDrawer<bool>
    {
        private VisualElement m_wrapper;
        private Toggle m_toggle;
        
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
            
            m_toggle = new Toggle();
            m_toggle.SetEnabled(false);
            m_toggle.RegisterValueChangedCallback(OnValueChanged);
            
            m_wrapper.Add(m_toggle);
            m_fieldPreviewArea.Add(m_wrapper);
        }

        protected override void OnSetField()
        {
            m_toggle.value = GetValue();
        }

        protected override void OnOnChangedTriggered(bool newValue)
        {
            m_toggle.SetValueWithoutNotify(newValue);
        }

        private void OnEditEnabled()
        {
            if (!m_fieldIsSet)
            {
                return;
            }
            
            m_toggle.SetEnabled(true);
            m_toggle.Focus();
        }

        private void OnValueChanged(ChangeEvent<bool> evt)
        {
            SetValue(evt.newValue);
            m_toggle.SetEnabled(false);
        }
    }
}