using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.Serialization;

namespace Zlitz.General.Serializables
{
    [CustomPropertyDrawer(typeof(Interface<>))]
    public class InterfaceDrawer : PropertyDrawer
    {
        private static readonly Texture2D s_objectIcon = EditorGUIUtility.IconContent("GameObject On Icon").image as Texture2D;

        private static readonly Color s_usingUnityObjectColor    = new Color(0.0f, 0.8f, 1.0f, 1.0f);
        private static readonly Color s_notUsingUnityObjectColor = new Color(1.0f, 0.6f, 0.0f, 1.0f);

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedInterface serializedInterface = new SerializedInterface(property);

            VisualElement root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;

            VisualElement valueContainer = new VisualElement();
            valueContainer.style.flexGrow = 1.0f;
            root.Add(valueContainer);

            ObjectField unityObjectField = new ObjectField(property.displayName);
            unityObjectField.AddToClassList("unity-base-field__aligned");
            unityObjectField.style.flexGrow = 1.0f;
            unityObjectField.allowSceneObjects = true;
            unityObjectField.value = serializedInterface.unityObjectProperty.objectReferenceValue;

            unityObjectField.style.display = serializedInterface.useUnityObjectProperty.boolValue ? DisplayStyle.Flex : DisplayStyle.None;

            valueContainer.Add(unityObjectField);

            VisualElement nonUnityObjectField = new VisualElement();
            nonUnityObjectField.style.flexGrow = 1.0f;

            nonUnityObjectField.style.display = serializedInterface.useUnityObjectProperty.boolValue ? DisplayStyle.None : DisplayStyle.Flex;

            valueContainer.Add(nonUnityObjectField);

            VisualElement useUnityObjectButton = new VisualElement();
            useUnityObjectButton.style.width = 16.0f;
            useUnityObjectButton.style.height = 16.0f;
            useUnityObjectButton.style.flexShrink = 0.0f;
            useUnityObjectButton.style.marginBottom = 2.0f;
            useUnityObjectButton.style.marginTop = 2.0f;
            useUnityObjectButton.style.marginLeft = 4.0f;
            useUnityObjectButton.style.marginRight = 0.0f;
            useUnityObjectButton.style.backgroundImage = s_objectIcon;
            useUnityObjectButton.style.unityBackgroundImageTintColor = serializedInterface.useUnityObjectProperty.boolValue ? s_usingUnityObjectColor : s_notUsingUnityObjectColor;

            useUnityObjectButton.RegisterCallback<ClickEvent>(e =>
            {
                serializedInterface.useUnityObjectProperty.boolValue = !serializedInterface.useUnityObjectProperty.boolValue;
                serializedInterface.serializedObject.ApplyModifiedProperties();

                useUnityObjectButton.style.unityBackgroundImageTintColor = serializedInterface.useUnityObjectProperty.boolValue ? s_usingUnityObjectColor : s_notUsingUnityObjectColor;

                unityObjectField.style.display = serializedInterface.useUnityObjectProperty.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
                nonUnityObjectField.style.display = serializedInterface.useUnityObjectProperty.boolValue ? DisplayStyle.None : DisplayStyle.Flex;
            });

            root.Add(useUnityObjectButton);

            VisualElement objectPickerClicker = new VisualElement();
            objectPickerClicker.style.position = Position.Absolute;
            objectPickerClicker.style.top = 0.0f;
            objectPickerClicker.style.bottom = 0.0f;
            objectPickerClicker.style.right = 0.0f;
            objectPickerClicker.style.width = 18.0f;

            objectPickerClicker.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button == 0)
                {
                    ObjectSelectorHelper.ShowInterfaceObjectPicker(
                        serializedInterface.unityObjectProperty.objectReferenceValue,
                        serializedInterface.interfaceType,
                        EditorGUIUtility.GetControlID(FocusType.Passive),
                        obj =>
                        {
                            serializedInterface.unityObjectProperty.objectReferenceValue = obj;
                            serializedInterface.serializedObject.ApplyModifiedProperties();

                            unityObjectField.value = serializedInterface.unityObjectProperty.objectReferenceValue;
                        },
                        null
                    );
                    e.StopImmediatePropagation();
                }
            });

            unityObjectField.Add(objectPickerClicker);

            VisualElement objectDragHandler = new VisualElement();
            objectDragHandler.style.position = Position.Absolute;
            objectDragHandler.style.top    = 0.0f;
            objectDragHandler.style.bottom = 0.0f;
            objectDragHandler.style.right  = 18.0f;

            float width = unityObjectField.resolvedStyle.width;
            if (unityObjectField.labelElement != null)
            {
                width -= unityObjectField.labelElement.resolvedStyle.width;
            }

            objectDragHandler.style.width = width - 20.0f;

            objectDragHandler.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button == 0)
                {
                    UnityEngine.Object objectToPing = serializedInterface.unityObjectProperty.objectReferenceValue;
                    if (objectToPing != null)
                    {
                        EditorGUIUtility.PingObject(objectToPing);
                    }
                }
            });

            objectDragHandler.RegisterCallback<DragUpdatedEvent>(e =>
            {
                UnityEngine.Object obj = ValidateDragAndDropObject(serializedInterface.interfaceType);
            });

            objectDragHandler.RegisterCallback<DragPerformEvent>(e =>
            {
                UnityEngine.Object obj = ValidateDragAndDropObject(serializedInterface.interfaceType);
                if (DragAndDrop.visualMode == DragAndDropVisualMode.Copy)
                {
                    serializedInterface.unityObjectProperty.objectReferenceValue = obj;
                    serializedInterface.serializedObject.ApplyModifiedProperties();

                    unityObjectField.value = serializedInterface.unityObjectProperty.objectReferenceValue;

                    DragAndDrop.AcceptDrag();
                }
            });

            unityObjectField.RegisterCallback<GeometryChangedEvent>(e =>
            {
                float width = unityObjectField.resolvedStyle.width;
                if (unityObjectField.labelElement != null)
                {
                    width -= unityObjectField.labelElement.resolvedStyle.width;
                }

                objectDragHandler.style.width = width - 20.0f;
            });

            unityObjectField.Add(objectDragHandler);

            if (SerializationUtility.HasManagedReferencesWithMissingTypes(serializedInterface.serializedObject.targetObject))
            {
                foreach (ManagedReferenceMissingType missingType in SerializationUtility.GetManagedReferencesWithMissingTypes(serializedInterface.serializedObject.targetObject))
                {
                    if (serializedInterface.managedReferenceIdProperty.longValue == missingType.referenceId)
                    {
                        serializedInterface.managedReferenceIdProperty.longValue = ManagedReferenceUtility.RefIdNull;
                        serializedInterface.valueProperty.managedReferenceValue  = null;
                        serializedInterface.serializedObject.ApplyModifiedProperties();

                        SerializationUtility.ClearManagedReferenceWithMissingType(serializedInterface.serializedObject.targetObject, missingType.referenceId);

                        break;
                    }
                }
            }

            if (serializedInterface.valueProperty.managedReferenceValue != null && SerializedPropertyHelper.IsPropertyArrayElement(property, out int elementIndex, out string arrayPath))
            {
                SerializedProperty arrayProperty = SerializedPropertyHelper.GetPropertyByPath(serializedInterface.serializedObject, arrayPath);
                for (int i = 0; i < elementIndex; i++)
                {
                    SerializedProperty siblingProperty = arrayProperty.GetArrayElementAtIndex(i);
                    SerializedInterface siblingInterface = new SerializedInterface(siblingProperty);
                    if (siblingInterface.valueProperty.managedReferenceValue == null)
                    {
                        continue;
                    }

                    if (serializedInterface.valueProperty.managedReferenceId == siblingInterface.valueProperty.managedReferenceId)
                    {
                        Type currentValueType = siblingInterface.valueProperty.managedReferenceValue.GetType();
                        if (currentValueType.IsSerializable)
                        {
                            using (MemoryStream stream = new MemoryStream())
                            {
                                IFormatter formatter = new BinaryFormatter();
                                formatter.Serialize(stream, siblingInterface.valueProperty.managedReferenceValue);
                                stream.Seek(0, SeekOrigin.Begin);

                                serializedInterface.valueProperty.managedReferenceValue = formatter.Deserialize(stream);
                            }
                        }
                        else
                        {
                            serializedInterface.valueProperty.managedReferenceValue = Activator.CreateInstance(currentValueType);
                        }

                        serializedInterface.managedReferenceIdProperty.longValue = serializedInterface.valueProperty.managedReferenceId;

                        serializedInterface.serializedObject.ApplyModifiedProperties();

                        break;
                    }
                }
            }

            Type currentType = serializedInterface.valueProperty.managedReferenceValue?.GetType();
            if (!serializedInterface.interfaceType.IsAssignableFrom(currentType) || typeof(UnityEngine.Object).IsAssignableFrom(currentType))
            {
                currentType = null;
                serializedInterface.valueProperty.managedReferenceValue = null;
                serializedInterface.serializedObject.ApplyModifiedProperties();
            }

            TypeSelector typeSelector = new TypeSelector(property.displayName);
            typeSelector.AddToClassList("unity-base-field__aligned");
            typeSelector.filter = t =>
            {
                if (t == null)
                {
                    return true;
                }

                if (typeof(UnityEngine.Object).IsAssignableFrom(t))
                {
                    return false;
                }

                if (!serializedInterface.interfaceType.IsAssignableFrom(t))
                {
                    return false;
                }

                if (t.IsAbstract)
                {
                    return false;
                }

                return true;
            };
            typeSelector.value = currentType;

            nonUnityObjectField.Add(typeSelector);

            PropertyField actualNonUnityObjectField = new PropertyField();
            actualNonUnityObjectField.label = "Interface value";
            actualNonUnityObjectField.style.marginLeft = 16.0f;
            actualNonUnityObjectField.style.marginTop  = 2.0f;
            actualNonUnityObjectField.style.display = currentType == null ? DisplayStyle.None : DisplayStyle.Flex;
            actualNonUnityObjectField.BindProperty(serializedInterface.valueProperty);

            nonUnityObjectField.Add(actualNonUnityObjectField);

            typeSelector.RegisterValueChangedCallback(e =>
            {
                if (e.newValue != currentType)
                {
                    currentType = e.newValue;
                    serializedInterface.valueProperty.managedReferenceValue = e.newValue != null ? Activator.CreateInstance(e.newValue) : null;
                    serializedInterface.managedReferenceIdProperty.longValue = serializedInterface.valueProperty.managedReferenceId;
                    serializedInterface.serializedObject.ApplyModifiedProperties();

                    actualNonUnityObjectField.Unbind();
                    actualNonUnityObjectField.BindProperty(serializedInterface.valueProperty);

                    actualNonUnityObjectField.style.display = currentType == null ? DisplayStyle.None : DisplayStyle.Flex;
                }
            });

            return root;
        }

        private static UnityEngine.Object ValidateDragAndDropObject(Type interfaceType)
        {
            UnityEngine.Object obj = null;
            DragAndDrop.visualMode = DragAndDropVisualMode.None;

            if (DragAndDrop.objectReferences.Length <= 0)
            {
                return null;
            }

            UnityEngine.Object dragging = DragAndDrop.objectReferences[0];
            if (dragging == null)
            {
                return null;
            }

            if (dragging is GameObject gameObject)
            {
                UnityEngine.Object component = gameObject.GetComponent(interfaceType);
                if (component != null)
                {
                    obj = component;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                }
                else
                {
                    obj = null;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                }
            }
            else
            {
                if (interfaceType.IsAssignableFrom(dragging.GetType()))
                {
                    obj = dragging;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                }
                else
                {
                    obj = null;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                }
            }

            return obj;
        }

        private struct SerializedInterface
        {
            public SerializedObject serializedObject { get; private set; }

            public SerializedProperty useUnityObjectProperty { get; private set; }

            public SerializedProperty unityObjectProperty { get; private set; }

            public SerializedProperty managedReferenceIdProperty { get; private set; }

            public SerializedProperty valueProperty { get; private set; }

            public Type interfaceType { get; private set; }

            public SerializedInterface(SerializedProperty property)
            {
                serializedObject = property.serializedObject;

                useUnityObjectProperty     = property.FindPropertyRelative("m_useUnityObject");
                unityObjectProperty        = property.FindPropertyRelative("m_unityObject");
                managedReferenceIdProperty = property.FindPropertyRelative("m_managedReferenceId");
                valueProperty              = property.FindPropertyRelative("m_value");

                interfaceType = GetInterfaceType(property);
            }
        }

        private static Type GetInterfaceType(SerializedProperty property)
        {
            object interfaceValue = SerializedPropertyHelper.GetPropertyValue(property, out Type interfaceType);
            if (interfaceType != null && interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(Interface<>))
            {
                return interfaceType.GetGenericArguments()[0];
            }
            return null;
        }
    }
}
