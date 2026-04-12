
using UnityEngine.UIElements;

namespace MyMVVM.RuntimeInspect
{
    public class BaseModelInspector : VisualElement
    {
        public BaseModel BaseModel { get; private set; }
        private ModelFieldDrawer m_modelFieldDrawer;
        
        public BaseModelInspector()
        {
            m_modelFieldDrawer = new ModelFieldDrawer();
            Add(m_modelFieldDrawer);
            
            RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                m_modelFieldDrawer.Clear();
            });
        }
        
        public void SetModel(BaseModel model, string label)
        {
            ClearDrawer();
            
            BaseModel = model;
            m_modelFieldDrawer.SetField(new RootModelFieldProvider(model, label));
            m_modelFieldDrawer.ToggleMainFoldout(true);
        }

        public void ClearDrawer()
        {
            if (BaseModel != null)
            {
                BaseModel = null;
            }

            if (m_modelFieldDrawer != null)
            {
                m_modelFieldDrawer.ClearDrawer();
            }
        }
    }
}