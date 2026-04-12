using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyMVVM.RuntimeInspect
{
    public class ListFieldDrawer<T> : ABaseFieldDrawer
    {
        private static string UXML_PATH = "MyMVVM/RuntimeInspector/ListFieldDrawer";
        
        private static string MAIN_FOLDOUT_TAG = "main-foldout";
        private static string FIELD_TYPE_LABEL_TAG = "field-type-label";
        private static string FIELD_NAME_LABEL_TAG = "field-name-label";
        
        private static string NULL_CREATE_AREA_TAG = "null-create-area";
        private static string NULL_LABEL_TAG = "null-label";
        private static string FIELD_PREVIEW_AREA_TAG = "field-preview-area";
        private static string ELEMENT_COUNT_LABEL_TAG = "element-count-label";
        private static string ELEMENT_LISTVIEW_TAG = "element-listview";

        private static string ONCHANGED_FOLDOUT_TAG = "onchanged-foldout";
        private static string ONCHANGED_COUNT_LABEL_TAG = "onchanged-count-label";
        private static string ONCHANGED_BOUND_LIST_AREA = "onchanged-bound-list-area";
        private static string ONCHANGED_BUTTON_TAG = "onchanged-button";

        private static string ONELEMENTCHANGED_FOLDOUT_TAG = "onelementchanged-foldout";
        private static string ONELEMENTCHANGED_COUNT_LABEL_TAG = "onelementchanged-count-label";
        private static string ONELEMENTCHANGED_INDEX_INTFIELD_TAG = "onelementchanged-index-intfield";
        private static string ONELEMENTCHANGED_BOUND_LIST_AREA_TAG = "onelementchanged-bound-list-area";
        private static string ONELEMENTCHANGED_BUTTON_TAG = "onelementchanged-button";

        private static string ONELEMENTINSERTED_FOLDOUT_TAG = "onelementinserted-foldout";
        private static string ONELEMENTINSERTED_COUNT_LABEL_TAG = "onelementinserted-count-label";
        private static string ONELEMENTINSERTED_INDEX_INTFIELD_TAG = "onelementinserted-index-intfield";
        private static string ONELEMENTINSERTED_BOUND_LIST_AREA_TAG = "onelementinserted-bound-list-area";
        private static string ONELEMENTINSERTED_BUTTON_TAG = "onelementinserted-button";

        private static string ONELEMENTREMOVED_FOLDOUT_TAG = "onelementremoved-foldout";
        private static string ONELEMENTREMOVED_COUNT_LABEL_TAG = "onelementremoved-count-label";
        private static string ONELEMENTREMOVED_INDEX_INTFIELD_TAG = "onelementremoved-index-intfield";
        private static string ONELEMENTREMOVED_BOUND_LIST_AREA_TAG = "onelementremoved-bound-list-area";
        private static string ONELEMENTREMOVED_BUTTON_TAG = "onelementremoved-button";

        private Foldout m_mainFoldout;
        private Label m_fieldTypeLabel;
        private Label m_fieldNameLabel;
        
        private VisualElement m_nullCreateArea;
        private Label m_nullLabel;
        private VisualElement m_fieldPreviewArea;
        private Label m_elementCountLabel;
        private ListView m_elementListView;
        
        private Foldout m_onChangedFoldout;
        private Label m_onChangedCountLabel;
        private VisualElement m_onChangedBoundListArea;
        private Button m_onChangedButton;
        
        private Foldout m_onElementChangedFoldout;
        private Label m_onElementChangedCountLabel;
        private IntegerField m_onElementChangedIndexIntField;
        private VisualElement m_onElementChangedBoundListArea;
        private Button m_onElementChangedButton;
        
        private Foldout m_onElementInsertedFoldout;
        private Label m_onElementInsertedCountLabel;
        private IntegerField m_onElementInsertedIndexIntField;
        private VisualElement m_onElementInsertedBoundListArea;
        private Button m_onElementInsertedButton;
        
        private Foldout m_onElementRemovedFoldout;
        private Label m_onElementRemovedCountLabel;
        private IntegerField m_onElementRemovedIndexIntField;
        private VisualElement m_onElementRemovedBoundListArea;
        private Button m_onElementRemovedButton;
        
        private List<ListElementFieldProvider> m_listElementFieldProviders = new();

        private ListFieldProvider m_listFieldProvider;
        
        public ListFieldDrawer()
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
            m_nullLabel.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Create Instance", HandleCreateNewInstance);
            }));
            m_fieldPreviewArea = this.Q<VisualElement>(FIELD_PREVIEW_AREA_TAG);
            m_elementCountLabel = this.Q<Label>(ELEMENT_COUNT_LABEL_TAG);

            m_elementListView = this.Q<ListView>(ELEMENT_LISTVIEW_TAG);
            m_elementListView.makeItem = () => new ListElementFieldDrawer();
            m_elementListView.bindItem = (element, i) => (element as ListElementFieldDrawer).SetField(m_listElementFieldProviders[i]);
            m_elementListView.unbindItem = (element, i) => (element as ListElementFieldDrawer).ClearDrawer();

            m_onChangedFoldout = this.Q<Foldout>(ONCHANGED_FOLDOUT_TAG);
            m_onChangedCountLabel = this.Q<Label>(ONCHANGED_COUNT_LABEL_TAG);
            m_onChangedBoundListArea = this.Q<VisualElement>(ONCHANGED_BOUND_LIST_AREA);
            m_onChangedButton = this.Q<Button>(ONCHANGED_BUTTON_TAG);
            m_onChangedButton.clicked += OnChangedButtonTriggered;

            m_onElementChangedFoldout = this.Q<Foldout>(ONELEMENTCHANGED_FOLDOUT_TAG);
            m_onElementChangedCountLabel = this.Q<Label>(ONELEMENTCHANGED_COUNT_LABEL_TAG);
            m_onElementChangedIndexIntField = this.Q<IntegerField>(ONELEMENTCHANGED_INDEX_INTFIELD_TAG);
            m_onElementChangedBoundListArea = this.Q<VisualElement>(ONELEMENTCHANGED_BOUND_LIST_AREA_TAG);
            m_onElementChangedButton = this.Q<Button>(ONELEMENTCHANGED_BUTTON_TAG);
            m_onElementChangedButton.clicked += OnElementChangedButtonTriggered;
            
            m_onElementInsertedFoldout = this.Q<Foldout>(ONELEMENTINSERTED_FOLDOUT_TAG);
            m_onElementInsertedCountLabel = this.Q<Label>(ONELEMENTINSERTED_COUNT_LABEL_TAG);
            m_onElementInsertedIndexIntField = this.Q<IntegerField>(ONELEMENTINSERTED_INDEX_INTFIELD_TAG);
            m_onElementInsertedBoundListArea = this.Q<VisualElement>(ONELEMENTINSERTED_BOUND_LIST_AREA_TAG);
            m_onElementInsertedButton = this.Q<Button>(ONELEMENTINSERTED_BUTTON_TAG);
            m_onElementInsertedButton.clicked += OnElementInsertedButtonTriggered;
            
            m_onElementRemovedFoldout = this.Q<Foldout>(ONELEMENTREMOVED_FOLDOUT_TAG);
            m_onElementRemovedCountLabel = this.Q<Label>(ONELEMENTREMOVED_COUNT_LABEL_TAG);
            m_onElementRemovedIndexIntField = this.Q<IntegerField>(ONELEMENTREMOVED_INDEX_INTFIELD_TAG);
            m_onElementRemovedBoundListArea = this.Q<VisualElement>(ONELEMENTREMOVED_BOUND_LIST_AREA_TAG);
            m_onElementRemovedButton = this.Q<Button>(ONELEMENTREMOVED_BUTTON_TAG);
            m_onElementRemovedButton.clicked += OnElementRemovedButtonTriggered;
            
            SetElements();
            ToggleMainFoldout(false);
        }

        public override Type GetFieldDrawerType()
        {
            return m_listFieldProvider.GetFieldType();
        }

        public override void ToggleMainFoldout(bool toggled)
        {
            m_mainFoldout.value = toggled;
        }

        public override void SetField(IFieldProvider fieldProvider)
        {
            m_listFieldProvider = fieldProvider as ListFieldProvider;
            if (m_listFieldProvider == null)
            {
                return;
            }

            m_fieldTypeLabel.text = $"List<{m_listFieldProvider.GetElementType().Name}>";
            m_fieldNameLabel.text = m_listFieldProvider.GetFieldName();

            m_listFieldProvider.SetupOnChangedFields((Action)OnChangedTriggered, OnChangedDelegatesChangedTriggered);
            m_listFieldProvider.SetupOnElementChangedFields(OnElementChangedTriggered, OnElementChangedDelegatesChangedTriggered);
            m_listFieldProvider.SetupOnElementInsertedChangedFields((Action<int, T>)OnElementInsertedTriggered, OnElementInsertedDelegatesChangedTriggered);
            m_listFieldProvider.SetupOnElementRemovedChangedFields(OnElementRemovedTriggered, OnElementRemovedDelegatesChangedTriggered);

            OnChangedDelegatesChangedTriggered();
            OnElementChangedDelegatesChangedTriggered();
            OnElementInsertedDelegatesChangedTriggered();
            OnElementRemovedDelegatesChangedTriggered();
            
            ToggleNullCreateArea(m_listFieldProvider.ValueIsNull());
            m_listElementFieldProviders = m_listFieldProvider.GetElementFieldProviders();
            SetElements();

            if (typeof(BaseModel).IsAssignableFrom(m_listFieldProvider.GetElementType()))
            {
                SetFieldTypeColor(new Color(0.455f, 0.286f, 0.933f));
            }
            else
            {
                SetFieldTypeColor(new Color(0.259f, 0.259f, 1f));
            }
        }

        public override void ClearDrawer()
        {
            if (m_listFieldProvider != null)
            {
                m_listFieldProvider.ClearProvider();
                m_listFieldProvider = null;
            }
            
            m_listElementFieldProviders.Clear();
            m_elementListView.itemsSource = null;
            m_elementListView.Rebuild();
        }

        private void SetElements()
        {
            m_elementListView.itemsSource = null;
            m_elementListView.itemsSource = m_listElementFieldProviders;
            m_elementCountLabel.text = $"({m_listElementFieldProviders.Count})";
        }

        private void OnChangedButtonTriggered()
        {
            if (m_listFieldProvider == null)
            {
                return;
            }
            m_listFieldProvider.OnOnChangedButtonPressed();
        }

        private void OnElementChangedButtonTriggered()
        {
            if (m_listFieldProvider == null)
            {
                return;
            }
            m_listFieldProvider.OnElementChangedButtonPressed(m_onElementChangedIndexIntField.value);
        }

        private void OnElementInsertedButtonTriggered()
        {
            if (m_listFieldProvider == null)
            {
                return;
            }
            m_listFieldProvider.OnElementInsertedButtonPressed(m_onElementInsertedIndexIntField.value, default(T));
        }
        
        private void OnElementRemovedButtonTriggered()
        {
            if (m_listFieldProvider == null)
            {
                return;
            }
            m_listFieldProvider.OnElementRemovedButtonPressed(m_onElementRemovedIndexIntField.value);
        }

        private void OnChangedTriggered()
        {
            ToggleNullCreateArea(m_listFieldProvider.ValueIsNull());
        }

        private void OnElementChangedTriggered(int index)
        {
            // do nothing.
        }

        private void OnElementInsertedTriggered(int index, T element)
        {
            if (m_listFieldProvider == null)
            {
                return;
            }
            
            ListElementFieldProvider listElementFieldProvider = m_listFieldProvider.CreateListElementFieldProvider(index);
            m_listElementFieldProviders.Insert(index, listElementFieldProvider);
            for (int i = index; i < m_listElementFieldProviders.Count; i++)
            {
                m_listElementFieldProviders[i].SetIndex(i);
            }
            
            SetElements();
        }

        private void OnElementRemovedTriggered(int index)
        {
            m_listElementFieldProviders.RemoveAt(index);
            for (int i = index; i < m_listElementFieldProviders.Count; i++)
            {
                m_listElementFieldProviders[i].SetIndex(i);
            }
            SetElements();
        }
        
        private void OnChangedDelegatesChangedTriggered()
        {
            if (m_listFieldProvider == null)
            {
                return;
            }
            UpdateOnChangedElement(m_listFieldProvider.GetOnChangedDelegates(), m_onChangedCountLabel, m_onChangedBoundListArea);
        }
        
        private void OnElementChangedDelegatesChangedTriggered()
        {
            if (m_listFieldProvider == null)
            {
                return;
            }
            UpdateOnChangedElement(m_listFieldProvider.GetOnElementChangedDelegates(), m_onElementChangedCountLabel, m_onElementChangedBoundListArea);
        }

        private void OnElementInsertedDelegatesChangedTriggered()
        {
            if (m_listFieldProvider == null)
            {
                return;
            }
            UpdateOnChangedElement(m_listFieldProvider.GetOnElementInsertedDelegates(), m_onElementInsertedCountLabel, m_onElementInsertedBoundListArea);
        }
        
        private void OnElementRemovedDelegatesChangedTriggered()
        {
            if (m_listFieldProvider == null)
            {
                return;
            }
            UpdateOnChangedElement(m_listFieldProvider.GetOnElementRemovedDelegates(), m_onElementRemovedCountLabel, m_onElementRemovedBoundListArea);
        }

        private void UpdateOnChangedElement(Delegate[] onChangedSubscribers, Label countLabel, VisualElement boundListArea)
        {
            countLabel.text = "-";
            boundListArea.Clear();
            
            if (onChangedSubscribers == null)
            {
                return;
            }
            
            countLabel.text = onChangedSubscribers.Length.ToString();
            foreach (Delegate del in onChangedSubscribers)
            {
                string targetName = "No Target";
                if (del.Target != null)
                {
                    targetName = del.Target.GetType().Name;
                }
                Label label = new Label($" - {targetName} :: {del.Method.Name}({typeof(T).Name} value)");
                label.style.fontSize = 11;
                label.style.unityFontStyleAndWeight = FontStyle.Italic;
                boundListArea.Add(label);
            }
        }
        
        private void ToggleNullCreateArea(bool toggle)
        {
            m_nullCreateArea.style.display = toggle ? DisplayStyle.Flex : DisplayStyle.None;
            m_fieldPreviewArea.style.display = toggle ? DisplayStyle.None : DisplayStyle.Flex;
            m_mainFoldout.style.display = toggle ? DisplayStyle.None : DisplayStyle.Flex;
        }
        
        private void HandleCreateNewInstance(DropdownMenuAction action)
        {
            if (m_listFieldProvider != null)
            {
                m_listFieldProvider.SetValueToNewInstance();
            }
        }
        
        protected void SetFieldTypeColor(Color color)
        {
            m_fieldTypeLabel.style.backgroundColor = color;
        }
    }
}