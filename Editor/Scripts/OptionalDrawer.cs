using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Zlitz.General.Serializables
{
    [CustomPropertyDrawer(typeof(Optional<>))]
    public class OptionalDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedOptional serializedOptional = new SerializedOptional(property);

            VisualElement container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;

            PropertyField valueField = new PropertyField(serializedOptional.valueProperty, preferredLabel ?? property.displayName);
            valueField.style.flexGrow = 1.0f;
            container.Add(valueField);

            Toggle hasValueToggle = new Toggle();
            hasValueToggle.style.height     = 18.0f;
            hasValueToggle.style.marginLeft = 4.0f;
            container.Add(hasValueToggle);

            hasValueToggle.value = serializedOptional.hasValueProperty.boolValue;
            valueField.SetEnabled(hasValueToggle.value);
            hasValueToggle.RegisterValueChangedCallback(e =>
            {
                valueField.SetEnabled(e.newValue);

                serializedOptional.hasValueProperty.boolValue = e.newValue;
                serializedOptional.serializedObject.ApplyModifiedProperties();
            });

            return container;
        }

        private struct SerializedOptional
        {
            public SerializedObject serializedObject { get; private set; }

            public SerializedProperty hasValueProperty { get; private set; }

            public SerializedProperty valueProperty { get; private set; }

            public SerializedOptional(SerializedProperty property)
            {
                serializedObject = property.serializedObject;

                hasValueProperty = property.FindPropertyRelative("m_hasValue");
                valueProperty = property.FindPropertyRelative("m_value");
            }
        }
    }
}
