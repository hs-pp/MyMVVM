using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyMVVM.RuntimeInspect
{
    public class ModelFieldDrawer : AFieldDrawer<BaseModel>
    {
        private BaseModel m_baseModel;
        private List<ABaseFieldDrawer> m_fieldDrawers = new();
        private List<PassthroughFieldProvider> m_passthroughFieldProviders = new();
        
        protected override void OnCreateLayout()
        {
            ToggleMainFoldout(false);
            SetFieldTypeColor(new Color(0.455f, 0.286f, 0.933f));
            style.display = DisplayStyle.None;
        }

        protected override void OnSetField()
        {
            SetModel(m_fieldProvider.GetValue() as BaseModel);
        }
        
        private void SetModel(BaseModel model)
        {
            ResetView();
            
            m_baseModel = model;

            if (m_baseModel == null)
            {
                return;
            }
            
            style.display = DisplayStyle.Flex;

            Type modelType = m_baseModel.GetType();
            var observableFields = modelType
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            foreach (FieldInfo field in observableFields)
            {
                if (Attribute.IsDefined(field, typeof(ObservableAttribute)))
                {
                    ABaseFieldDrawer modelFieldDrawer = CreateFieldDrawerForType(field.FieldType);
                    modelFieldDrawer.SetField(IFieldProvider.CreateFieldProvider(field, m_baseModel));

                    m_fieldFoldoutArea.Add(modelFieldDrawer);
                    m_fieldDrawers.Add(modelFieldDrawer);
                }
                else if (Attribute.IsDefined(field, typeof(PassthroughAttribute)))
                {
                    PassthroughFieldDrawer passthroughFieldDrawer = new PassthroughFieldDrawer();
                    PassthroughFieldProvider passthroughFieldProvider =
                        new PassthroughFieldProvider(field, m_baseModel);
                    passthroughFieldDrawer.SetField(passthroughFieldProvider);
                    m_passthroughFieldProviders.Add(passthroughFieldProvider);
                    passthroughFieldProvider.OnNonObservablePassthroughModelChanged += RequestUpdateOnPassthroughModels;
                    
                    m_fieldFoldoutArea.Add(passthroughFieldDrawer);
                    m_fieldDrawers.Add(passthroughFieldDrawer); 
                }
                else if (Attribute.IsDefined(field, typeof(ShowInRuntimeInspectorAttribute)))
                {
                    if (!field.FieldType.IsSubclassOf(typeof(BaseModel)))
                    {
                        continue;
                    }
                    
                    NonObservableModelFieldProvider nonObservableModelFieldProvider = new NonObservableModelFieldProvider(field, m_baseModel);
                    ABaseFieldDrawer modelFieldDrawer = CreateFieldDrawerForType(field.FieldType);
                    modelFieldDrawer.SetField(nonObservableModelFieldProvider);
                    
                    m_fieldFoldoutArea.Add(modelFieldDrawer);
                    m_fieldDrawers.Add(modelFieldDrawer);
                }
            }
        }
        
        protected override void OnOnChangedTriggered(BaseModel newValue)
        {
            if (newValue != m_baseModel)
            {
                SetModel(newValue);
            }
        }

        private void RequestUpdateOnPassthroughModels()
        {
            foreach (PassthroughFieldProvider passthroughFieldProvider in m_passthroughFieldProviders)
            {
                passthroughFieldProvider.CheckIfPassthroughModelChanged();
            }
        }

        public override void ClearDrawer()
        {
            ResetView();
            base.ClearDrawer();
        }

        private void ResetView()
        {
            foreach (ABaseFieldDrawer fieldDrawer in m_fieldDrawers)
            {
                fieldDrawer.ClearDrawer();
            }

            m_baseModel = null;
            m_fieldDrawers.Clear();
            m_passthroughFieldProviders.Clear();
            m_fieldFoldoutArea.Clear();

            style.display = DisplayStyle.None;
        }
    }
}