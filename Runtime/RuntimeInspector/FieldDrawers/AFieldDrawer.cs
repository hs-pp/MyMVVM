using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyMVVM.RuntimeInspect
{
    public abstract class AFieldDrawer<T> : ABaseFieldDrawer
    {
        private static string UXML_PATH = "MyMVVM/RuntimeInspector/AFieldDrawer";
        private static string MAIN_FOLDOUT_TAG = "main-foldout";
        private static string FIELD_TYPE_LABEL_TAG = "field-type-label";
        private static string FIELD_NAME_LABEL_TAG = "field-name-label";
        private static string NULL_CREATE_AREA_TAG = "null-create-area";
        private static string NULL_LABEL_TAG = "null-label";
        private static string FIELD_PREVIEW_AREA_TAG = "field-preview-area";
        private static string FIELD_FOLDOUT_AREA_TAG = "field-foldout-area";
        private static string ONCHANGED_FOLDOUT_TAG = "onchanged-foldout";
        private static string ONCHANGED_BOUND_LIST_AREA_TAG = "onchanged-bound-list-area";
        private static string ONCHANGED_NAME_LABEL_TAG = "onchanged-name-label";
        private static string ONCHANGED_TRIGGER_BUTTON_TAG = "onchanged-trigger-button";
        private static string ONCHANGED_COUNT_LABEL_TAG = "onchanged-count-label";

        private Foldout m_mainFoldout;
        private Label m_fieldTypeLabel;
        private Label m_fieldNameLabel;
        private VisualElement m_nullCreateArea;
        private Label m_nullLabel;
        protected VisualElement m_fieldPreviewArea;
        protected VisualElement m_fieldFoldoutArea;
        private Foldout m_onChangedFoldout;
        private VisualElement m_onChangedBoundListArea;
        private Label m_onChangedNameLabel;
        private Button m_onChangedTriggerButton;
        private Label m_onChangedCountLabel;

        protected IFieldProvider m_fieldProvider;
        protected bool m_fieldIsSet;

        public AFieldDrawer()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            VisualTreeAsset root = Resources.Load<VisualTreeAsset>(UXML_PATH);
            root.CloneTree(this);

            m_mainFoldout = this.Q<Foldout>(MAIN_FOLDOUT_TAG);
            m_fieldTypeLabel = this.Q<Label>(FIELD_TYPE_LABEL_TAG);
            m_fieldNameLabel = this.Q<Label>(FIELD_NAME_LABEL_TAG);
            m_nullCreateArea = this.Q<VisualElement>(NULL_CREATE_AREA_TAG);
            m_nullLabel = this.Q<Label>(NULL_LABEL_TAG);
            m_fieldPreviewArea = this.Q<VisualElement>(FIELD_PREVIEW_AREA_TAG);
            m_fieldFoldoutArea = this.Q<VisualElement>(FIELD_FOLDOUT_AREA_TAG);
            m_onChangedFoldout = this.Q<Foldout>(ONCHANGED_FOLDOUT_TAG);
            m_onChangedBoundListArea = this.Q<VisualElement>(ONCHANGED_BOUND_LIST_AREA_TAG);
            m_onChangedNameLabel = this.Q<Label>(ONCHANGED_NAME_LABEL_TAG);
            m_onChangedTriggerButton = this.Q<Button>(ONCHANGED_TRIGGER_BUTTON_TAG);
            m_onChangedCountLabel = this.Q<Label>(ONCHANGED_COUNT_LABEL_TAG);

            m_nullLabel.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Create Instance", HandleCreateNewInstance);
            }));
            m_onChangedTriggerButton.clicked += OnCallbackTriggerButtonPressed;

            ToggleMainFoldout(false);
            ToggleOnChangedFoldout(false);
            SetFieldTypeColor(new Color(0.259f, 0.259f, 1f));
            
            OnCreateLayout();
        }

        public override Type GetFieldDrawerType()
        {
            return typeof(T);
        }
        
        public override void ToggleMainFoldout(bool open)
        {
            m_mainFoldout.value = open;
        }

        protected void ToggleOnChangedFoldout(bool open)
        {
            m_onChangedFoldout.value = open;
        }

        protected void SetFieldTypeColor(Color color)
        {
            m_fieldTypeLabel.style.backgroundColor = color;
        }

        public override void SetField(IFieldProvider fieldProvider)
        {
            m_fieldIsSet = true;
            m_fieldProvider = fieldProvider;
            m_fieldTypeLabel.text = m_fieldProvider.GetFieldType().Name;
            m_fieldNameLabel.text = m_fieldProvider.GetFieldName();
            m_onChangedNameLabel.text = m_fieldProvider.GetOnChangedFieldName();
            
            m_fieldProvider.SetupOnChangedFields((Action<T>)HandleOnChangedTriggered, OnCallbackDelegatesChanged);
            OnCallbackDelegatesChanged();
            ToggleNullCreateArea(m_fieldProvider.ValueIsNull());
            
            OnSetField();
        }
        
        public override void ClearDrawer()
        {
            if (m_fieldProvider != null)
            {
                m_fieldProvider.ClearProvider();
                m_fieldProvider = null;
            }

            m_fieldIsSet = false;
        }
        
        private void OnCallbackDelegatesChanged()
        {
            m_onChangedCountLabel.text = "-";
            m_onChangedBoundListArea.Clear();
            
            Delegate[] subscribedCallbackDelegates = m_fieldProvider.GetOnChangedDelegates();
            if (subscribedCallbackDelegates == null)
            {
                return;
            }

            m_onChangedCountLabel.text = subscribedCallbackDelegates.Length.ToString();
            foreach (Delegate del in subscribedCallbackDelegates)
            {
                string targetName = "No Target";
                if (del.Target != null)
                {
                    targetName = del.Target.GetType().Name;
                }
                Label label = new Label($" - {targetName} :: {del.Method.Name}({typeof(T).Name} value)");
                label.style.fontSize = 11;
                label.style.unityFontStyleAndWeight = FontStyle.Italic;
                m_onChangedBoundListArea.Add(label);
            }
        }

        private void OnCallbackTriggerButtonPressed()
        {
            if (m_fieldProvider == null)
            {
                return;
            }
            m_fieldProvider.OnOnChangedButtonPressed();
        }

        protected T GetValue()
        {
            if (m_fieldProvider == null)
            {
                return default(T);
            }
            return (T) m_fieldProvider.GetValue();
        }

        protected void SetValue(T value)
        {
            m_fieldProvider?.SetValue(value);
        }

        private void HandleOnChangedTriggered(T newValue)
        {
            ToggleNullCreateArea(m_fieldProvider.ValueIsNull());
            OnOnChangedTriggered(newValue);
        }

        private void ToggleNullCreateArea(bool toggle)
        {
            m_nullCreateArea.style.display = toggle ? DisplayStyle.Flex : DisplayStyle.None;
            m_fieldPreviewArea.style.display = toggle ? DisplayStyle.None : DisplayStyle.Flex;
            m_mainFoldout.style.display = toggle ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void HandleCreateNewInstance(DropdownMenuAction action)
        {
            if (m_fieldProvider != null)
            {
                m_fieldProvider.SetValueToNewInstance();
            }
        }

        protected abstract void OnCreateLayout();
        protected abstract void OnSetField();
        protected abstract void OnOnChangedTriggered(T newValue);
    }
}