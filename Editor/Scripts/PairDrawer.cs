using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Zlitz.General.Serializables
{
    [CustomPropertyDrawer(typeof(Pair<,>))]
    public class PairDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedPair serializedPair = new SerializedPair(property);

            VisualElement root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;

            Label label = new Label();
            label.style.width = 105.0f;
            label.style.height = 16.0f;
            label.style.marginBottom = 2.0f;
            label.style.marginLeft = 4.0f;
            label.style.marginTop = 2.0f;
            label.text = preferredLabel ?? property.displayName;
            root.Add(label);

            VisualElement container = new VisualElement();
            container.style.flexGrow = 1.0f;
            root.Add(container);

            PropertyField keyPropertyField = new PropertyField();
            keyPropertyField.BindProperty(serializedPair.keyProperty);
            keyPropertyField.style.flexGrow = 1.0f;
            keyPropertyField.label = "Key";
            container.Add(keyPropertyField);

            PropertyField valuePropertyField = new PropertyField();
            valuePropertyField.BindProperty(serializedPair.valueProperty);
            valuePropertyField.style.flexGrow = 1.0f;
            valuePropertyField.label = "Value";
            container.Add(valuePropertyField);

            return root;
        }

        private struct SerializedPair
        {
            public SerializedObject serializedObject { get; private set; }

            public SerializedProperty keyProperty { get; private set; }

            public SerializedProperty valueProperty { get; private set; }

            public SerializedPair(SerializedProperty property)
            {
                serializedObject = property.serializedObject;

                keyProperty   = property.FindPropertyRelative("m_key");
                valueProperty = property.FindPropertyRelative("m_value");
            }
        }
    }
}
