using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Zlitz.General.Serializables
{
    [CustomPropertyDrawer(typeof(Set<>))]
    public class SetDrawer : PropertyDrawer
    {
        private static Texture2D s_warningIcon = EditorGUIUtility.IconContent("Warning@2x").image as Texture2D;
        private static Texture2D s_errorIcon   = EditorGUIUtility.IconContent("Error@2x").image as Texture2D;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedSet serializedSet = new SerializedSet(property);

            ListView listView = new ListView();
            listView.BindProperty(serializedSet.listProperty);

            listView.headerTitle             = property.displayName;
            listView.showAddRemoveFooter     = true;
            listView.showFoldoutHeader       = true;
            listView.showBoundCollectionSize = false;
            listView.reorderable             = true;
            listView.reorderMode             = ListViewReorderMode.Animated;
            listView.virtualizationMethod    = CollectionVirtualizationMethod.DynamicHeight;

            listView.makeItem = () =>
            {
                SetItem setItem = new SetItem();

                listView.TrackPropertyValue(serializedSet.listProperty, (p) =>
                {
                    SerializedProperty currentElementProperty = setItem.index >= serializedSet.listProperty.arraySize ? null : serializedSet.listProperty.GetArrayElementAtIndex(setItem.index);
                    if (currentElementProperty == null)
                    {
                        return;
                    }

                    object currentElementValue = SerializedPropertyHelper.GetPropertyValue(currentElementProperty, out Type propertyType);
                    if (currentElementValue is UnityEngine.Object unityObject && unityObject == null)
                    {
                        currentElementValue = null;
                    }

                    if (typeof(UnityEngine.Object).IsAssignableFrom(propertyType) && currentElementValue == null)
                    {
                        setItem.conflictIcon.style.backgroundImage = s_errorIcon;
                        setItem.conflictIcon.tooltip = "Object reference value must not be null";
                    }
                    else
                    {
                        setItem.conflictIcon.style.backgroundImage = null;
                        setItem.conflictIcon.tooltip = "";
                        for (int j = 0; j < serializedSet.listProperty.arraySize; j++)
                        {
                            if (j == setItem.index)
                            {
                                continue;
                            }

                            SerializedProperty otherElementProperty = serializedSet.listProperty.GetArrayElementAtIndex(j);
                            object otherElementValue = SerializedPropertyHelper.GetPropertyValue(otherElementProperty);

                            if (EqualityComparer<object>.Default.Equals(otherElementValue, currentElementValue))
                            {
                                setItem.conflictIcon.style.backgroundImage = s_warningIcon;
                                setItem.conflictIcon.tooltip = "Duplicated value will be ignored";
                                break;
                            }
                        }
                    }
                });

                return setItem;
            };

            listView.bindItem = (e, i) =>
            {
                e.Unbind();
                if (e is SetItem setItem)
                {
                    SerializedProperty elementProperty = serializedSet.listProperty.GetArrayElementAtIndex(i);
                    if (elementProperty == null)
                    {
                        return;
                    }

                    setItem.propertyField.BindProperty(elementProperty);
                    setItem.propertyField.label = $"Element {i}";

                    setItem.index = i;

                    object currentElementValue = SerializedPropertyHelper.GetPropertyValue(elementProperty, out Type propertyType);
                    if (currentElementValue is UnityEngine.Object unityObject && unityObject == null)
                    {
                        currentElementValue = null;
                    }

                    if (typeof(UnityEngine.Object).IsAssignableFrom(propertyType) && currentElementValue == null)
                    {
                        setItem.conflictIcon.style.backgroundImage = s_errorIcon;
                        setItem.conflictIcon.tooltip = "Object reference value must not be null";
                    }
                    else
                    {
                        setItem.conflictIcon.style.backgroundImage = null;
                        setItem.conflictIcon.tooltip = "";
                        for (int j = 0; j < serializedSet.listProperty.arraySize; j++)
                        {
                            if (j == setItem.index)
                            {
                                continue;
                            }

                            SerializedProperty otherElementProperty = serializedSet.listProperty.GetArrayElementAtIndex(j);
                            object otherElementValue = SerializedPropertyHelper.GetPropertyValue(otherElementProperty);

                            if (EqualityComparer<object>.Default.Equals(otherElementValue, currentElementValue))
                            {
                                setItem.conflictIcon.style.backgroundImage = s_warningIcon;
                                setItem.conflictIcon.tooltip = "Duplicated value will be ignored";
                                break;
                            }
                        }
                    }
                }
            };

            return listView;
        }

        private class SetItem : VisualElement
        {
            public int index;

            public PropertyField propertyField { get; private set; }

            public VisualElement conflictIcon { get; private set; }

            public SetItem()
            {
                style.flexDirection = FlexDirection.Row;

                propertyField = new PropertyField();
                propertyField.style.flexGrow = 1.0f;
                Add(propertyField);

                conflictIcon = new VisualElement();
                conflictIcon.style.width  = 16.0f;
                conflictIcon.style.height = 16.0f;
                conflictIcon.style.marginBottom = 2.0f;
                conflictIcon.style.marginTop    = 2.0f;
                conflictIcon.style.marginLeft   = 4.0f;
                conflictIcon.style.marginRight  = 0.0f;
                Add(conflictIcon);
            }
        }

        private struct SerializedSet
        {
            public SerializedObject serializedObject { get; private set; }

            public SerializedProperty listProperty { get; private set; }

            public SerializedSet(SerializedProperty property)
            {
                serializedObject = property.serializedObject;

                listProperty = property.FindPropertyRelative("m_list");
            }
        }
    }
}
