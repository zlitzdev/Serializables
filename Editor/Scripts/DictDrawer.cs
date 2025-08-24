using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Zlitz.General.Serializables
{
    [CustomPropertyDrawer(typeof(Dict<,>))]
    public class DictDrawer : PropertyDrawer
    {
        private static Texture2D s_warningIcon = EditorGUIUtility.IconContent("Warning@2x").image as Texture2D;
        private static Texture2D s_errorIcon   = EditorGUIUtility.IconContent("Error@2x").image as Texture2D;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedDict serializedDict = new SerializedDict(property);

            ListView listView = new ListView();
            listView.BindProperty(serializedDict.listProperty);

            listView.headerTitle = preferredLabel ?? property.displayName;
            listView.showAddRemoveFooter = true;
            listView.showFoldoutHeader = true;
            listView.showBoundCollectionSize = false;
            listView.reorderable = true;
            listView.reorderMode = ListViewReorderMode.Animated;
            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

            listView.makeItem = () =>
            {
                DictItem dictItem = new DictItem();

                listView.TrackPropertyValue(serializedDict.listProperty, (p) =>
                {
                    SerializedProperty currentElementProperty = dictItem.index >= serializedDict.listProperty.arraySize ? null : serializedDict.listProperty.GetArrayElementAtIndex(dictItem.index);
                    if (currentElementProperty == null)
                    {
                        return;
                    }

                    SerializedProperty keyProperty = currentElementProperty.FindPropertyRelative("m_key");

                    object currentElementKey = SerializedPropertyHelper.GetPropertyValue(keyProperty, out Type propertyType);
                    if (currentElementKey is UnityEngine.Object unityObject && unityObject == null)
                    {
                        currentElementKey = null;
                    }

                    if (typeof(UnityEngine.Object).IsAssignableFrom(propertyType) && currentElementKey == null)
                    {
                        dictItem.conflictIcon.style.backgroundImage = s_errorIcon;
                        dictItem.conflictIcon.tooltip = "Object reference value must not be null";
                    }
                    else
                    {
                        dictItem.conflictIcon.style.backgroundImage = null;
                        dictItem.conflictIcon.tooltip = "";
                        for (int j = 0; j < serializedDict.listProperty.arraySize; j++)
                        {
                            if (j == dictItem.index)
                            {
                                continue;
                            }

                            SerializedProperty otherElementProperty = serializedDict.listProperty.GetArrayElementAtIndex(j);
                            SerializedProperty otherElementKeyProperty = otherElementProperty.FindPropertyRelative("m_key");

                            object otherElementKey = SerializedPropertyHelper.GetPropertyValue(otherElementKeyProperty);

                            if (EqualityComparer<object>.Default.Equals(otherElementKey, currentElementKey))
                            {
                                dictItem.conflictIcon.style.backgroundImage = s_warningIcon;
                                dictItem.conflictIcon.tooltip = "Duplicated value will be ignored";
                                break;
                            }
                        }
                    }
                });

                return dictItem;
            };

            listView.bindItem = (e, i) =>
            {
                e.Unbind();
                if (e is DictItem dictItem)
                {
                    SerializedProperty elementProperty = serializedDict.listProperty.GetArrayElementAtIndex(i);
                    if (elementProperty == null)
                    {
                        return;
                    }

                    SerializedProperty keyProperty   = elementProperty.FindPropertyRelative("m_key");
                    SerializedProperty valueProperty = elementProperty.FindPropertyRelative("m_value");

                    dictItem.keyPropertyField.BindProperty(keyProperty);
                    dictItem.valuePropertyField.BindProperty(valueProperty);

                    dictItem.index = i;

                    dictItem.label = $"Element {i}";

                    object currentElementKey = SerializedPropertyHelper.GetPropertyValue(keyProperty, out Type propertyType);
                    if (currentElementKey is UnityEngine.Object unityObject && unityObject == null)
                    {
                        currentElementKey = null;
                    }

                    if (typeof(UnityEngine.Object).IsAssignableFrom(propertyType) && currentElementKey == null)
                    {
                        dictItem.conflictIcon.style.backgroundImage = s_errorIcon;
                        dictItem.conflictIcon.tooltip = "Object reference value must not be null";
                    }
                    else
                    {
                        dictItem.conflictIcon.style.backgroundImage = null;
                        dictItem.conflictIcon.tooltip = "";
                        for (int j = 0; j < serializedDict.listProperty.arraySize; j++)
                        {
                            if (j == dictItem.index)
                            {
                                continue;
                            }

                            SerializedProperty otherElementProperty = serializedDict.listProperty.GetArrayElementAtIndex(j);

                            SerializedProperty otherElementKeyProperty = otherElementProperty.FindPropertyRelative("m_key");

                            object otherElementKey = SerializedPropertyHelper.GetPropertyValue(otherElementKeyProperty);

                            if (EqualityComparer<object>.Default.Equals(otherElementKey, currentElementKey))
                            {
                                dictItem.conflictIcon.style.backgroundImage = s_warningIcon;
                                dictItem.conflictIcon.tooltip = "Duplicated value will be ignored";
                                break;
                            }
                        }
                    }
                }
            };

            return listView;
        }

        private class DictItem : VisualElement
        {
            public int index;

            public PropertyField keyPropertyField { get; private set; }

            public PropertyField valuePropertyField { get; private set; }

            public VisualElement conflictIcon { get; private set; }

            private Label m_label;

            public string label
            {
                get => m_label.text;
                set => m_label.text = value;
            }

            public DictItem()
            {
                style.flexDirection = FlexDirection.Row;

                m_label = new Label();
                m_label.style.width        = 80.0f;
                m_label.style.height       = 16.0f;
                m_label.style.marginBottom = 2.0f;
                m_label.style.marginTop    = 2.0f;
                Add(m_label);

                VisualElement container = new VisualElement();
                container.style.flexGrow = 1.0f;
                Add(container);

                keyPropertyField = new PropertyField();
                keyPropertyField.style.flexGrow = 1.0f;
                keyPropertyField.label = "Key";
                container.Add(keyPropertyField);

                valuePropertyField = new PropertyField();
                valuePropertyField.style.flexGrow   = 1.0f;
                valuePropertyField.label = "Value";
                container.Add(valuePropertyField);

                conflictIcon = new VisualElement();
                conflictIcon.style.width = 16.0f;
                conflictIcon.style.height = 16.0f;
                conflictIcon.style.marginBottom = 2.0f;
                conflictIcon.style.marginTop = 2.0f;
                conflictIcon.style.marginLeft = 4.0f;
                conflictIcon.style.marginRight = 0.0f;
                Add(conflictIcon);
            }
        }

        private struct SerializedDict
        {
            public SerializedObject serializedObject { get; private set; }

            public SerializedProperty listProperty { get; private set; }

            public SerializedDict(SerializedProperty property)
            {
                serializedObject = property.serializedObject;

                listProperty = property.FindPropertyRelative("m_list");
            }
        }
    }
}
