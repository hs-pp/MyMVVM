using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyMVVM.RuntimeInspect
{
    public class PassthroughFieldDrawer : ABaseFieldDrawer
    {
        private static string UXML_PATH = "MyMVVM/RuntimeInspector/PassthroughFieldDrawer";
        private static string FIELD_TYPE_LABEL_TAG = "field-type-label";
        private static string FIELD_NAME_LABEL_TAG = "field-name-label";
        private static string PASSTHROUGH_NULL_AREA_TAG = "passthrough-null-area";
        private static string PASSTHROUGH_NULL_LABEL_TAG = "passthrough-null-label";
        private static string FIELDDRAWER_AREA_TAG = "fielddrawer-area";
        private static string PASSTHROUGH_INDICATOR_TAG = "passthrough-indicator";
        
        private Label m_fieldTypeLabel;
        private Label m_fieldNameLabel;
        private VisualElement m_passthroughNullArea;
        private Label m_passthroughNullLabel;
        private VisualElement m_fieldDrawerArea;
        private Label m_passthroughIndicator;

        private PassthroughFieldProvider m_passthroughFieldProvider;
        private ABaseFieldDrawer m_fieldDrawer;
        
        public PassthroughFieldDrawer()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            VisualTreeAsset root = Resources.Load<VisualTreeAsset>(UXML_PATH);
            root.CloneTree(this);
            
            m_fieldTypeLabel = this.Q<Label>(FIELD_TYPE_LABEL_TAG);
            m_fieldNameLabel = this.Q<Label>(FIELD_NAME_LABEL_TAG);
            m_passthroughNullArea = this.Q<VisualElement>(PASSTHROUGH_NULL_AREA_TAG);
            m_passthroughNullLabel = this.Q<Label>(PASSTHROUGH_NULL_LABEL_TAG);
            m_passthroughNullLabel.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Create Instance", TryCreatePassthroughModelInstance);
            }));
            m_fieldDrawerArea = this.Q<VisualElement>(FIELDDRAWER_AREA_TAG);
            m_passthroughIndicator = this.Q<Label>(PASSTHROUGH_INDICATOR_TAG);

            ToggleNullArea(true);
        }
        
        public override Type GetFieldDrawerType()
        {
            return m_passthroughFieldProvider?.GetFieldType();
        }

        public override void ToggleMainFoldout(bool toggled)
        {
        }

        public override void SetField(IFieldProvider fieldProvider)
        {
            m_passthroughFieldProvider = fieldProvider as PassthroughFieldProvider;
            if (m_passthroughFieldProvider == null)
            {
                return;
            }
            m_fieldTypeLabel.text = m_passthroughFieldProvider.GetFieldType().Name;
            m_fieldNameLabel.text = m_passthroughFieldProvider.GetFieldName();
            m_passthroughIndicator.tooltip = m_passthroughFieldProvider.GetPassthroughIndicatorTooltipString();
            m_passthroughFieldProvider.OnPassthroughModelChanged += UpdateFieldDrawer;

            UpdateFieldDrawer();
        }

        private void ToggleNullArea(bool showNullArea)
        {
            m_passthroughNullArea.style.display = showNullArea ? DisplayStyle.Flex : DisplayStyle.None;
            m_fieldDrawerArea.style.display = showNullArea ? DisplayStyle.None : DisplayStyle.Flex;
        }

        public void UpdateFieldDrawer()
        {
            bool passthroughExists = m_passthroughFieldProvider.PassthroughModelExists();
            ToggleNullArea(!passthroughExists);
            
            if (passthroughExists)
            {
                if (m_fieldDrawer != null)
                {
                    m_fieldDrawer.ClearDrawer();
                    if (m_fieldDrawer.GetFieldDrawerType() != m_passthroughFieldProvider.GetFieldType())
                    {
                        m_fieldDrawerArea.Remove(m_fieldDrawer);
                        m_fieldDrawer = null;
                    }
                }

                if (m_fieldDrawer == null) // Create a new field drawer
                {
                    m_fieldDrawer = CreateFieldDrawerForType(m_passthroughFieldProvider.GetFieldType());
                    m_fieldDrawerArea.Add(m_fieldDrawer);
                }
                
                m_fieldDrawer.SetField(m_passthroughFieldProvider.GetDirectFieldProvider());
            }
        }

        private void TryCreatePassthroughModelInstance(DropdownMenuAction action)
        {
            if (m_passthroughFieldProvider == null)
            {
                return;
            }

            m_passthroughFieldProvider.CreatePassthroughModelInstanceIfNull();
            UpdateFieldDrawer();
        }

        public override void ClearDrawer()
        {
            if (m_passthroughFieldProvider != null)
            {
                m_passthroughFieldProvider.ClearProvider();
                m_passthroughFieldProvider = null;
            }
        }
    }
}
