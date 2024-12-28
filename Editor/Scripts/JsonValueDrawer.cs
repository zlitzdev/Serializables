using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Reflection;
using System.Linq;
using UnityEditor.Search;

namespace Zlitz.General.Serializables
{
    [CustomPropertyDrawer(typeof(JsonValue))]
    public class JsonValueDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedJsonValue serializedJsonValue = new SerializedJsonValue(property);

            VisualElement root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;

            JsonValueField jsonValueField = new JsonValueField(property.displayName);
            jsonValueField.style.flexGrow = 1.0f;
            jsonValueField.SetProperty(property);
            root.Add(jsonValueField);

            return root;
        }

        private struct SerializedJsonValue
        {
            public SerializedObject serializedObject { get; private set; }

            public SerializedProperty valueProperty { get; private set; }

            public SerializedJsonValue(SerializedProperty property)
            {
                serializedObject = property.serializedObject;

                valueProperty = property.FindPropertyRelative("m_value");
            }
        }

        internal class JsonValueField : VisualElement 
        {
            public JsonPrimitiveDrawer.JsonPrimitiveField primitiveField { get; private set; }

            private DropdownField m_dataTypeDropdown;
            
            private SerializedProperty m_property;
            private SerializedJsonValue m_serializedJsonValue;

            public string label
            {
                get => primitiveField.label;
                set => primitiveField.label = value;
            }

            public void SetProperty(SerializedProperty property)
            {
                m_property = property;
                m_serializedJsonValue = new SerializedJsonValue(m_property);

                HandleDiplicateManagedReferences();

                string currentOption = m_serializedJsonValue.valueProperty.managedReferenceValue switch
                {
                    JsonBool   => "Bool",
                    JsonNumber => "Number",
                    JsonString => "String",
                    JsonArray  => "Array",
                    JsonObject => "Object",
                    _ => "Null"
                };
                m_dataTypeDropdown.value = currentOption;

                this.TrackPropertyValue(m_serializedJsonValue.valueProperty, p =>
                {
                    primitiveField.SetProperty(m_serializedJsonValue.valueProperty);
                });

                primitiveField.SetProperty(m_serializedJsonValue.valueProperty);
            }

            public JsonValueField(string label, bool editableLabel = false)
            {
                style.flexDirection = FlexDirection.Row;

                primitiveField = new JsonPrimitiveDrawer.JsonPrimitiveField(label, editableLabel, true);
                primitiveField.style.flexGrow = 1.0f;
                Add(primitiveField);

                List<string> options = new List<string>()
                {
                    "Null",
                    "Bool",
                    "Number",
                    "String",
                    "Array",
                    "Object"
                };

                m_dataTypeDropdown = new DropdownField("", options, 0);
                m_dataTypeDropdown.RegisterValueChangedCallback(e =>
                {
                    if (e.newValue != e.previousValue)
                    {
                        switch (e.newValue)
                        {
                            case "Bool":
                                {
                                    if (m_serializedJsonValue.valueProperty.managedReferenceValue is not JsonBool)
                                    {
                                        m_serializedJsonValue.valueProperty.managedReferenceValue = new JsonBool();
                                        m_serializedJsonValue.serializedObject.ApplyModifiedProperties();
                                        primitiveField.SetProperty(m_serializedJsonValue.valueProperty);
                                    }
                                    break;
                                }
                            case "Number":
                                {
                                    if (m_serializedJsonValue.valueProperty.managedReferenceValue is not JsonNumber)
                                    {
                                        m_serializedJsonValue.valueProperty.managedReferenceValue = new JsonNumber();
                                        m_serializedJsonValue.serializedObject.ApplyModifiedProperties();
                                        primitiveField.SetProperty(m_serializedJsonValue.valueProperty);
                                    }
                                    break;
                                }
                            case "String":
                                {
                                    if (m_serializedJsonValue.valueProperty.managedReferenceValue is not JsonString)
                                    {
                                        m_serializedJsonValue.valueProperty.managedReferenceValue = new JsonString();
                                        m_serializedJsonValue.serializedObject.ApplyModifiedProperties();
                                        primitiveField.SetProperty(m_serializedJsonValue.valueProperty);
                                    }
                                    break;
                                }
                            case "Array":
                                {
                                    if (m_serializedJsonValue.valueProperty.managedReferenceValue is not JsonArray)
                                    {
                                        m_serializedJsonValue.valueProperty.managedReferenceValue = new JsonArray();
                                        m_serializedJsonValue.serializedObject.ApplyModifiedProperties();
                                        primitiveField.SetProperty(m_serializedJsonValue.valueProperty);
                                    }
                                    break;
                                }
                            case "Object":
                                {
                                    if (m_serializedJsonValue.valueProperty.managedReferenceValue is not JsonObject)
                                    {
                                        m_serializedJsonValue.valueProperty.managedReferenceValue = new JsonObject();
                                        m_serializedJsonValue.serializedObject.ApplyModifiedProperties();
                                        primitiveField.SetProperty(m_serializedJsonValue.valueProperty);
                                    }
                                    break;
                                }
                            default:
                                {
                                    if (m_serializedJsonValue.valueProperty.managedReferenceValue != null)
                                    {
                                        m_serializedJsonValue.valueProperty.managedReferenceValue = null;
                                        m_serializedJsonValue.serializedObject.ApplyModifiedProperties();
                                        primitiveField.SetProperty(m_serializedJsonValue.valueProperty);
                                    }
                                    break;
                                }
                        }
                    }
                });
                m_dataTypeDropdown.style.width = 72.0f;
                primitiveField.alignedField.fieldContainer.Add(m_dataTypeDropdown);
            }
        
            private void HandleDiplicateManagedReferences()
            {
                if (m_serializedJsonValue.valueProperty.managedReferenceValue == null || m_serializedJsonValue.serializedObject == null || m_serializedJsonValue.valueProperty == null)
                {
                    return;
                }

                SerializedProperty it = m_serializedJsonValue.serializedObject.GetIterator();
                while (it.Next(true))
                {
                    if (it.propertyPath == m_serializedJsonValue.valueProperty.propertyPath)
                    {
                        break;
                    }

                    if (it.propertyType == SerializedPropertyType.ManagedReference && it.managedReferenceValue != null && it.managedReferenceId == m_serializedJsonValue.valueProperty.managedReferenceId)
                    {
                        Type currentValueType = it.managedReferenceValue.GetType();
                        if (currentValueType.IsSerializable)
                        {
                            using (MemoryStream stream = new MemoryStream())
                            {
                                IFormatter formatter = new BinaryFormatter();
                                formatter.Serialize(stream, it.managedReferenceValue);
                                stream.Seek(0, SeekOrigin.Begin);

                                m_serializedJsonValue.valueProperty.managedReferenceValue = formatter.Deserialize(stream);
                            }
                        }
                        else
                        {
                            m_serializedJsonValue.valueProperty.managedReferenceValue = Activator.CreateInstance(currentValueType);
                        }

                        m_serializedJsonValue.valueProperty.serializedObject.ApplyModifiedProperties();
                        break;
                    }
                }
            }
        }
    }

    [CustomPropertyDrawer(typeof(JsonBool))]
    [CustomPropertyDrawer(typeof(JsonNumber))]
    [CustomPropertyDrawer(typeof(JsonString))]
    [CustomPropertyDrawer(typeof(JsonArray))]
    [CustomPropertyDrawer(typeof(JsonObject))]
    public class JsonPrimitiveDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            JsonPrimitiveField field = new JsonPrimitiveField(property.displayName);
            field.SetProperty(property);
            return field;
        }

        internal class JsonPrimitiveField : VisualElement
        {
            private static Texture2D s_warningIcon = EditorGUIUtility.IconContent("Warning@2x").image as Texture2D;
            private static Texture2D s_infoIcon    = EditorGUIUtility.IconContent("console.infoicon@2x").image as Texture2D;

            public AlignedField alignedField { get; private set; }

            public Label labelElement { get; private set; }

            public TextField editableLabelElement { get; private set; }

            public event Action onLabelEdit;

            private Label m_nullLabel;
            private Label m_nonPrimitiveLabel;
            
            private PropertyField m_simpleValueField;

            private Foldout m_stringAreaFoldout;
            private TextField m_stringArea;
            private VisualElement m_stringPlaceholder;

            private ListView m_arrayListView;
            private ListView m_objectListView;

            private VisualElement m_infoIcon;

            private SerializedProperty m_property;
            private SerializedJsonPrimitive m_serializedJsonPrimitive;

            private bool m_useColors;

            public string label
            {
                get => labelElement == null ? editableLabelElement.value : labelElement.text;
                set 
                {
                    if (labelElement == null)
                    {
                        editableLabelElement.value = value;
                    }
                    else
                    {
                        labelElement.text = value;
                    }
                }
            }

            public void SetProperty(SerializedProperty property)
            {
                m_property = property;
                m_serializedJsonPrimitive = new SerializedJsonPrimitive(m_property);

                m_nullLabel.style.display = DisplayStyle.None;
                
                m_simpleValueField.style.display = DisplayStyle.None;

                m_nonPrimitiveLabel.style.display = DisplayStyle.None;

                m_arrayListView.style.display = DisplayStyle.None;
                m_objectListView.style.display = DisplayStyle.None;

                m_stringPlaceholder.style.display = DisplayStyle.None;
                m_stringAreaFoldout.style.display = DisplayStyle.None;

                m_infoIcon.style.display = DisplayStyle.None;

                // Null

                if (m_property.propertyType == SerializedPropertyType.ManagedReference && m_property.managedReferenceValue == null)
                {
                    m_nullLabel.style.display = DisplayStyle.Flex;

                    SetLabelColor(new Color(0.5f, 0.5f, 0.5f, 1.0f));

                    return;
                }

                // Bool, Number, String

                if (m_serializedJsonPrimitive.valueProperty != null)
                {
                    if (m_serializedJsonPrimitive.valueProperty.propertyType != SerializedPropertyType.String)
                    {
                        m_simpleValueField.BindProperty(m_serializedJsonPrimitive.valueProperty);
                        m_simpleValueField.style.display = DisplayStyle.Flex;

                        switch (m_serializedJsonPrimitive.valueProperty.propertyType)
                        {
                            case SerializedPropertyType.Boolean:
                                {
                                    SetLabelColor(new Color(0.3f, 0.8f, 0.3f, 1.0f));
                                    break;
                                }
                            case SerializedPropertyType.Float:
                                {
                                    SetLabelColor(new Color(0.1f, 0.6f, 0.8f, 1.0f));
                                    break;
                                }
                        }
                    }
                    else
                    {
                        m_simpleValueField.BindProperty(m_serializedJsonPrimitive.valueProperty);
                        m_stringArea.BindProperty(m_serializedJsonPrimitive.valueProperty);

                        m_simpleValueField.style.display = m_serializedJsonPrimitive.valueProperty.isExpanded ? DisplayStyle.None : DisplayStyle.Flex;
                        m_stringPlaceholder.style.display = m_serializedJsonPrimitive.valueProperty.isExpanded ? DisplayStyle.Flex : DisplayStyle.None;

                        m_stringAreaFoldout.style.display = DisplayStyle.Flex;
                        m_stringAreaFoldout.value = m_serializedJsonPrimitive.valueProperty.isExpanded;

                        if (!m_serializedJsonPrimitive.valueProperty.isExpanded && m_serializedJsonPrimitive.valueProperty.stringValue.Contains("\n"))
                        {
                            m_infoIcon.style.display = DisplayStyle.Flex;
                            m_infoIcon.style.backgroundImage = s_infoIcon;
                            m_infoIcon.tooltip = "String value contains multiple lines. Expand to edit in multi-line mode";
                        }

                        EditorApplication.delayCall += () =>
                        {
                            TextElement textElement = m_simpleValueField.Q<TextElement>();
                            if (textElement != null)
                            {
                                textElement.style.unityTextAlign = TextAnchor.UpperLeft;
                            }
                        };

                        SetLabelColor(new Color(0.8f, 0.8f, 0.1f, 1.0f));
                    }

                    return;
                }

                // Array

                if (m_serializedJsonPrimitive.valuesProperty != null)
                {
                    m_arrayListView.BindProperty(m_serializedJsonPrimitive.valuesProperty);
                    m_arrayListView.bindItem = (e, i) =>
                    {
                        if (e is JsonValueDrawer.JsonValueField jsonValueField)
                        {
                            jsonValueField.label = $"Element {i}";
                            jsonValueField.SetProperty(m_serializedJsonPrimitive.valuesProperty.GetArrayElementAtIndex(i));
                        }
                    };

                    m_arrayListView.Rebuild();

                    m_nonPrimitiveLabel.style.display = DisplayStyle.Flex;
                    m_nonPrimitiveLabel.text = $"{m_serializedJsonPrimitive.valuesProperty.arraySize} element(s)";
                    
                    m_arrayListView.style.display = DisplayStyle.Flex;

                    SetLabelColor(new Color(0.9f, 0.6f, 0.1f, 1.0f));

                    return;
                }

                // Object

                if (m_serializedJsonPrimitive.listProperty != null)
                {
                    m_objectListView.BindProperty(m_serializedJsonPrimitive.listProperty);
                    m_objectListView.bindItem = (e, i) =>
                    {
                        SerializedProperty objectElementProperty = m_serializedJsonPrimitive.listProperty.GetArrayElementAtIndex(i);

                        SerializedProperty keyProperty   = objectElementProperty.FindPropertyRelative("m_key");
                        SerializedProperty valueProperty = objectElementProperty.FindPropertyRelative("m_value");

                        if (e is JsonValueDrawer.JsonValueField jsonValueField)
                        {
                            jsonValueField.primitiveField.editableLabelElement.BindProperty(keyProperty);
                            jsonValueField.SetProperty(valueProperty);

                            VisualElement conflicIcon = jsonValueField.Q<VisualElement>("ConflictIcon");
                            conflicIcon.style.backgroundImage = null;
                            conflicIcon.tooltip = null;

                            for (int j = 0; j < m_serializedJsonPrimitive.listProperty.arraySize; j++)
                            {
                                if (j == i)
                                {
                                    continue;
                                }

                                SerializedProperty siblingElementProperty = m_serializedJsonPrimitive.listProperty.GetArrayElementAtIndex(j);

                                SerializedProperty siblingKeyProperty = siblingElementProperty.FindPropertyRelative("m_key");
                                if (siblingKeyProperty.stringValue == keyProperty.stringValue)
                                {
                                    conflicIcon.style.backgroundImage = s_warningIcon;
                                    conflicIcon.tooltip = "Duplicated value will be ignored";

                                    break;
                                }
                            }
                        }
                    };

                    m_objectListView.Rebuild();

                    m_nonPrimitiveLabel.style.display = DisplayStyle.Flex;
                    m_nonPrimitiveLabel.text = $"{m_serializedJsonPrimitive.listProperty.arraySize} element(s)";

                    m_objectListView.style.display = DisplayStyle.Flex;

                    SetLabelColor(new Color(0.7f, 0.2f, 0.7f, 1.0f));

                    return;
                }
            }

            public JsonPrimitiveField(string label, bool editableLabel = false, bool useColors = false)
            {
                m_useColors = useColors;

                style.minHeight    = 18.0f;
                style.marginTop    = 1.0f;
                style.marginBottom = 1.0f;

                alignedField = new AlignedField();
                alignedField.style.marginLeft = 3.0f;
                Add(alignedField);

                if (editableLabel)
                {
                    editableLabelElement = new TextField("");
                    editableLabelElement.value = label;
                    editableLabelElement.RegisterValueChangedCallback(e =>
                    {
                        if (e.newValue != e.previousValue)
                        {
                            onLabelEdit?.Invoke();
                        }
                    });
                    alignedField.labelContainer.Add(editableLabelElement);
                }
                else
                {
                    labelElement = new Label();
                    labelElement.text = label;
                    alignedField.labelContainer.Add(labelElement);
                }

                m_nullLabel = new Label("Null");
                m_nullLabel.style.marginLeft = 4.0f;
                m_nullLabel.style.marginTop = 1.0f;
                m_nullLabel.style.marginBottom = 1.0f;
                m_nullLabel.style.height = 16.0f;
                m_nullLabel.style.flexGrow = 1.0f;
                alignedField.fieldContainer.Add(m_nullLabel);

                m_stringAreaFoldout = new Foldout();
                m_stringAreaFoldout.style.minHeight = 18.0f;
                m_stringAreaFoldout.style.marginTop = -18.0f;
                alignedField.expandedContainer.Add(m_stringAreaFoldout);

                m_stringPlaceholder = new VisualElement();
                m_stringPlaceholder.style.flexGrow = 1.0f;
                alignedField.fieldContainer.Add(m_stringPlaceholder);

                m_stringAreaFoldout.pickingMode = PickingMode.Ignore;
                m_stringAreaFoldout.RegisterValueChangedCallback(e =>
                {
                    m_serializedJsonPrimitive.valueProperty.isExpanded = e.newValue;

                    m_simpleValueField.style.display  = m_serializedJsonPrimitive.valueProperty.isExpanded ? DisplayStyle.None : DisplayStyle.Flex;
                    m_stringPlaceholder.style.display = m_serializedJsonPrimitive.valueProperty.isExpanded ? DisplayStyle.Flex : DisplayStyle.None;

                    if (!m_serializedJsonPrimitive.valueProperty.isExpanded && m_serializedJsonPrimitive.valueProperty.stringValue.Contains("\n"))
                    {
                        m_infoIcon.style.display = DisplayStyle.Flex;
                        m_infoIcon.style.backgroundImage = s_infoIcon;
                        m_infoIcon.tooltip = "String value contains multiple lines. Expand to edit in multi-line mode";
                    }
                    else
                    {
                        m_infoIcon.style.display = DisplayStyle.None;
                    }
                });

                Toggle stringAreaFoldoutToggle = m_stringAreaFoldout.Q<Toggle>();
                stringAreaFoldoutToggle.style.marginLeft = -18.0f;
                stringAreaFoldoutToggle.style.width = 18.0f;
                stringAreaFoldoutToggle.style.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

                m_stringArea = new TextField("");
                m_stringArea.multiline = true;
                m_stringArea.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
                m_stringArea.style.height = 108.0f;
                m_stringArea.style.marginLeft = -14.0f;
                m_stringArea.style.marginBottom = 8.0f;

                m_stringAreaFoldout.Add(m_stringArea);

                m_infoIcon = new VisualElement();
                m_infoIcon.style.width = 18.0f;
                m_infoIcon.style.height = 18.0f;
                m_infoIcon.style.marginTop = 1.0f;
                m_infoIcon.style.marginBottom = 1.0f;
                m_infoIcon.style.marginLeft = -20.0f;
                m_infoIcon.style.marginRight = 1.0f;
                alignedField.fieldContainer.Add(m_infoIcon);

                m_simpleValueField = new PropertyField();
                m_simpleValueField.label = "";
                m_simpleValueField.style.flexGrow = 1.0f;
                m_simpleValueField.style.height = 18.0f;
                alignedField.fieldContainer.Add(m_simpleValueField);

                m_simpleValueField.RegisterValueChangeCallback(e =>
                {
                    if (m_serializedJsonPrimitive.valueProperty != null && m_serializedJsonPrimitive.valueProperty.propertyType == SerializedPropertyType.String)
                    {
                        if (!m_serializedJsonPrimitive.valueProperty.isExpanded && m_serializedJsonPrimitive.valueProperty.stringValue.Contains("\n"))
                        {
                            m_infoIcon.style.display = DisplayStyle.Flex;
                            m_infoIcon.style.backgroundImage = s_infoIcon;
                            m_infoIcon.tooltip = "String value contains multiple lines. Expand to edit in multi-line mode";
                        }
                        else
                        {
                            m_infoIcon.style.display = DisplayStyle.None;
                        }
                    }
                });

                m_nonPrimitiveLabel = new Label();
                m_nonPrimitiveLabel.style.height = 18.0f;
                m_nonPrimitiveLabel.style.flexGrow = 1.0f;
                m_nonPrimitiveLabel.style.marginLeft = 4.0f;
                m_nonPrimitiveLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
                alignedField.fieldContainer.Add(m_nonPrimitiveLabel);

                m_arrayListView = new ListView();
                m_arrayListView.showAddRemoveFooter = true;
                m_arrayListView.showFoldoutHeader = true;
                m_arrayListView.showBoundCollectionSize = false;
                m_arrayListView.showBorder = true;
                m_arrayListView.reorderable = true;
                m_arrayListView.reorderMode = ListViewReorderMode.Animated;
                m_arrayListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

                m_arrayListView.style.marginTop = -20.0f;

                VisualElement arrayHeader = m_arrayListView.Q("unity-list-view__foldout-header");
                arrayHeader.pickingMode = PickingMode.Ignore;

                Toggle arrayHeaderToggle = arrayHeader.Q<Toggle>();
                arrayHeaderToggle.style.marginLeft = -18.0f;
                arrayHeaderToggle.style.width = 18.0f;
                arrayHeaderToggle.style.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

                m_arrayListView.makeItem = () =>
                {
                    JsonValueDrawer.JsonValueField result = new JsonValueDrawer.JsonValueField("");
                    result.style.marginRight = 20.0f;
                    return result;
                };
                m_arrayListView.itemsAdded += (i) =>
                {
                    if (m_serializedJsonPrimitive.valuesProperty != null)
                    {
                        m_serializedJsonPrimitive.serializedObject.Update();
                        m_nonPrimitiveLabel.text = $"{m_serializedJsonPrimitive.valuesProperty.arraySize} element(s)";
                    }
                };
                m_arrayListView.itemsRemoved += (i) =>
                {
                    if (m_serializedJsonPrimitive.valuesProperty != null)
                    {
                        m_serializedJsonPrimitive.serializedObject.Update();
                        m_nonPrimitiveLabel.text = $"{m_serializedJsonPrimitive.valuesProperty.arraySize - i.Count()} element(s)";
                    }
                };

                alignedField.expandedContainer.Add(m_arrayListView);

                m_objectListView = new ListView();
                m_objectListView.showAddRemoveFooter = true;
                m_objectListView.showFoldoutHeader = true;
                m_objectListView.showBoundCollectionSize = false;
                m_objectListView.showBorder = true;
                m_objectListView.reorderable = true;
                m_objectListView.reorderMode = ListViewReorderMode.Animated;
                m_objectListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

                m_objectListView.style.marginTop = -20.0f;

                VisualElement objectHeader = m_objectListView.Q("unity-list-view__foldout-header");
                objectHeader.pickingMode = PickingMode.Ignore;

                Toggle objectHeaderToggle = objectHeader.Q<Toggle>();
                objectHeaderToggle.style.marginLeft = -18.0f;
                objectHeaderToggle.style.width = 18.0f;
                objectHeaderToggle.style.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

                m_objectListView.makeItem = () =>
                {
                    JsonValueDrawer.JsonValueField result = new JsonValueDrawer.JsonValueField("", true);
                    result.style.marginRight = 0.0f;

                    VisualElement conflictIcon = new VisualElement();
                    conflictIcon.name = "ConflictIcon";
                    conflictIcon.style.width = 18.0f;
                    conflictIcon.style.height = 18.0f;
                    conflictIcon.style.marginTop = 1.0f;
                    conflictIcon.style.marginBottom = 1.0f;
                    conflictIcon.style.marginLeft = 2.0f;

                    result.primitiveField.alignedField.fieldContainer.Add(conflictIcon);

                    result.primitiveField.onLabelEdit += () =>
                    {
                        if (m_property != null)
                        {
                            EditorApplication.delayCall += m_objectListView.RefreshItems;
                        }
                    };

                    return result;
                };
                m_objectListView.itemsAdded += (i) =>
                {
                    if (m_serializedJsonPrimitive.listProperty != null)
                    {
                        m_serializedJsonPrimitive.serializedObject.Update();
                        m_nonPrimitiveLabel.text = $"{m_serializedJsonPrimitive.listProperty.arraySize} element(s)";
                    }
                };
                m_objectListView.itemsRemoved += (i) =>
                {
                    if (m_serializedJsonPrimitive.listProperty != null)
                    {
                        m_serializedJsonPrimitive.serializedObject.Update();
                        m_nonPrimitiveLabel.text = $"{m_serializedJsonPrimitive.listProperty.arraySize - i.Count()} element(s)";
                    }
                };

                alignedField.expandedContainer.Add(m_objectListView);
            }

            private void SetLabelColor(Color color)
            {
                if (!m_useColors)
                {
                    return;
                }

                if (labelElement != null)
                {
                    labelElement.style.color = color;
                }
                if (editableLabelElement != null)
                {
                    TextElement textElement = editableLabelElement.Q<TextElement>();
                    textElement.style.color = color;
                }
            }

            private struct SerializedJsonPrimitive
            {
                public SerializedObject serializedObject { get; private set; }

                public SerializedProperty valueProperty { get; private set; } // For primitive

                public SerializedProperty valuesProperty { get; private set; } // For array

                public SerializedProperty listProperty { get; private set; } // For object

                public SerializedJsonPrimitive(SerializedProperty property)
                {
                    serializedObject = property.serializedObject;

                    valueProperty  = property.FindPropertyRelative("m_value");
                    valuesProperty = property.FindPropertyRelative("m_values");
                    listProperty   = property.FindPropertyRelative("m_list");
                }
            }
        }
    }

    internal class AlignedField : VisualElement
    {
        public VisualElement labelContainer { get; private set; }

        public VisualElement fieldContainer { get; private set; }

        public VisualElement expandedContainer { get; private set; }

        private float m_labelWidthRatio;
        private float m_labelExtraPadding;
        private float m_labelBaseMinWidth;
        private float m_labelExtraContextWidth;

        private VisualElement m_cachedContextWidthElement;
        private VisualElement m_cachedInspectorElement;

        public AlignedField()
        {
            VisualElement top = new VisualElement();
            top.style.flexDirection = FlexDirection.Row;
            top.style.minHeight = 20.0f;
            Add(top);

            expandedContainer = new VisualElement();
            Add(expandedContainer);

            labelContainer = new VisualElement();
            labelContainer.style.flexDirection = FlexDirection.Column;
            labelContainer.style.justifyContent = Justify.Center;
            top.Add(labelContainer);

            fieldContainer = new VisualElement();
            fieldContainer.style.flexDirection = FlexDirection.Row;
            fieldContainer.style.flexGrow = 1.0f;
            top.Add(fieldContainer);

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent e)
        {
            if (e.destinationPanel == null)
            {
                return;
            }

            if (e.destinationPanel.contextType == ContextType.Player)
            {
                return;
            }

            m_cachedInspectorElement = null;
            m_cachedContextWidthElement = null;

            var currentElement = parent;
            while (currentElement != null)
            {
                if (currentElement.ClassListContains("unity-inspector-element"))
                {
                    m_cachedInspectorElement = currentElement;
                }

                if (currentElement.ClassListContains("unity-inspector-main-container"))
                {
                    m_cachedContextWidthElement = currentElement;
                    break;
                }

                currentElement = currentElement.parent;
            }

            if (m_cachedInspectorElement == null)
            {
                RemoveFromClassList("unity-base-field__inspector-field");
                return;
            }

            m_labelWidthRatio = 0.45f;

            m_labelExtraPadding = 37.0f;
            m_labelBaseMinWidth = 123.0f;

            m_labelExtraContextWidth = 1.0f;

            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            AddToClassList("unity-base-field__inspector-field");
            RegisterCallback<GeometryChangedEvent>(OnInspectorFieldGeometryChanged);
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            AlignLabel();
        }

        private void OnInspectorFieldGeometryChanged(GeometryChangedEvent e)
        {
            AlignLabel();
        }

        private void AlignLabel()
        {
            if (labelContainer == null)
            {
                return;
            }

            float totalPadding = m_labelExtraPadding;
            float spacing = worldBound.x - m_cachedInspectorElement.worldBound.x - m_cachedInspectorElement.resolvedStyle.paddingLeft;

            totalPadding += spacing;
            totalPadding += resolvedStyle.paddingLeft;

            var minWidth = m_labelBaseMinWidth - spacing - resolvedStyle.paddingLeft;
            var contextWidthElement = m_cachedContextWidthElement ?? m_cachedInspectorElement;

            labelContainer.style.minWidth = Mathf.Max(minWidth, 0);

            var newWidth = (contextWidthElement.resolvedStyle.width + m_labelExtraContextWidth) * m_labelWidthRatio - totalPadding;
            if (Mathf.Abs(labelContainer.resolvedStyle.width - newWidth) > 1E-30f)
            {
                labelContainer.style.width = Mathf.Max(0f, newWidth);
            }
        }
    }
}
