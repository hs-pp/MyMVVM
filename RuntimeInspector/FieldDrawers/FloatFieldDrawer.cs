using UnityEngine.UIElements;

namespace MyMVVM.RuntimeInspect
{
    public class FloatFieldDrawer : AFieldDrawer<float>
    {
        private VisualElement m_wrapper;
        private FloatField m_floatField;
        
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
            
            m_floatField = new FloatField();
            m_floatField.style.minWidth = 140;
            m_floatField.isDelayed = true;
            m_floatField.SetEnabled(false);
            m_floatField.RegisterValueChangedCallback(OnValueChanged);
            
            m_wrapper.Add(m_floatField);
            m_fieldPreviewArea.Add(m_wrapper);
        }

        protected override void OnSetField()
        {
            m_floatField.value = GetValue();
        }

        protected override void OnOnChangedTriggered(float newValue)
        {
            m_floatField.SetValueWithoutNotify(newValue);
        }

        private void OnEditEnabled()
        {
            if (!m_fieldIsSet)
            {
                return;
            }
            
            m_floatField.SetEnabled(true);
            m_floatField.Focus();
        }

        private void OnValueChanged(ChangeEvent<float> evt)
        {
            SetValue(evt.newValue);
            m_floatField.SetEnabled(false);
        }
    }
}