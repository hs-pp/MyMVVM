using UnityEngine;
using UnityEngine.UIElements;

namespace MyMVVM.RuntimeInspect
{
    public class ListElementFieldDrawer : VisualElement
    {
        private static string UXML_PATH = "MyMVVM/RuntimeInspector/ListElementFieldDrawer";
        private static string INDEX_LABEL_TAG = "index-label";
        private static string FIELD_DRAWER_AREA_TAG = "field-drawer-area";

        private Label m_indexLabel;
        private VisualElement m_fieldDrawerArea;
        private ABaseFieldDrawer m_baseFieldDrawer;
        
        public ListElementFieldDrawer()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            VisualTreeAsset root = Resources.Load<VisualTreeAsset>(UXML_PATH);
            root.CloneTree(this);
            
            m_indexLabel = this.Q<Label>(INDEX_LABEL_TAG);
            m_fieldDrawerArea = this.Q<VisualElement>(FIELD_DRAWER_AREA_TAG);
        }

        public void SetField(ListElementFieldProvider listElementFieldProvider)
        {
            m_indexLabel.text = listElementFieldProvider.GetIndex().ToString();
            if (m_baseFieldDrawer != null && m_baseFieldDrawer.GetFieldDrawerType() != listElementFieldProvider.GetFieldType())
            {
                m_fieldDrawerArea.Remove(m_baseFieldDrawer);
                m_baseFieldDrawer = null;
            }
            if (m_baseFieldDrawer == null)
            {
                m_baseFieldDrawer = ABaseFieldDrawer.CreateFieldDrawerForType(listElementFieldProvider.GetFieldType());
                m_fieldDrawerArea.Add(m_baseFieldDrawer);
            }

            m_baseFieldDrawer.ToggleMainFoldout(false);
            m_baseFieldDrawer.SetField(listElementFieldProvider);  
        }

        public void ClearDrawer()
        {
            m_baseFieldDrawer.ClearDrawer();
        }
    }
}