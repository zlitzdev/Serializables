using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace Zlitz.General.Serializables
{
    public static class ObjectSelectorHelper
    {
        public static void ShowInterfaceObjectPicker(UnityEngine.Object obj, Type interfaceType, int controlID, Action<UnityEngine.Object> onObjectSelectorClosed, Action<UnityEngine.Object> onObjectSelectedUpdated)
        {
            Type[] validTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => typeof(UnityEngine.Object).IsAssignableFrom(t) && interfaceType.IsAssignableFrom(t)).ToArray();
            if (validTypes == null || validTypes.Length <= 0)
            {
                Debug.LogWarning($"No UnityEngine.Object-based type that implement {interfaceType.Name} found.");
                return;
            }

            if (Event.current?.commandName == "ObjectSelectorClosed")
            {
                EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
                {
                    SetupObjectSelector(obj, validTypes, true, "", controlID, onObjectSelectorClosed, onObjectSelectedUpdated);
                });
            }
            else
            {
                SetupObjectSelector(obj, validTypes, true, "", controlID, onObjectSelectorClosed, onObjectSelectedUpdated);
            }
        }

        private static Assembly s_editorAssembly;

        private static Type s_objectSelectorType;

        private static MethodInfo s_objectSelectorGetAccessor;
        private static MethodInfo s_objectSelectorFilterAccessor;
        private static MethodInfo s_objectSelectorShowMethod;

        private static FieldInfo s_objectSelectorIdField;

        private static object s_objectSelector;

        private static void Initialize()
        {
            if (s_objectSelectorGetAccessor == null)
            {
                s_editorAssembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "UnityEditor");

                s_objectSelectorType = s_editorAssembly.GetType("UnityEditor.ObjectSelector");

                PropertyInfo getProperty = s_objectSelectorType.GetProperty("get", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                s_objectSelectorGetAccessor = getProperty.GetAccessors(true).FirstOrDefault();

                PropertyInfo searchFilterProperty = s_objectSelectorType.GetProperty("searchFilter", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                s_objectSelectorFilterAccessor = searchFilterProperty.GetAccessors(true).Where(m => m.ReturnType == typeof(void)).FirstOrDefault();

                s_objectSelectorShowMethod = s_objectSelectorType.GetMethod("Show", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[]
                {
                    typeof(UnityEngine.Object),
                    typeof(Type[]),
                    typeof(UnityEngine.Object),
                    typeof(bool),
                    typeof(List<int>),
                    typeof(Action<UnityEngine.Object>),
                    typeof(Action<UnityEngine.Object>),
                    typeof(bool)
                }, null);

                s_objectSelectorIdField = s_objectSelectorType.GetField("objectSelectorID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            s_objectSelector = s_objectSelectorGetAccessor.Invoke(null, Enumerable.Empty<object>().ToArray());
        }

        private static void Show(UnityEngine.Object obj, Type[] requiredTypes, UnityEngine.Object objectBeingEdited, bool allowSceneObjects, List<int> allowedInstanceIDs = null, Action<UnityEngine.Object> onObjectSelectorClosed = null, Action<UnityEngine.Object> onObjectSelectedUpdated = null, bool showNoneItem = true)
        {
            s_objectSelectorShowMethod.Invoke(s_objectSelector, new object[]
            {
                obj,
                requiredTypes,
                objectBeingEdited,
                allowSceneObjects,
                allowedInstanceIDs,
                onObjectSelectorClosed,
                onObjectSelectedUpdated,
                showNoneItem
            });
        }

        private static void SetControlID(int controlID)
        {
            s_objectSelectorIdField.SetValue(s_objectSelector, controlID);
        }

        private static void SetFilter(string filter)
        {
            s_objectSelectorFilterAccessor.Invoke(s_objectSelector, new object[] { filter });
        }

        private static void SetupObjectSelector(UnityEngine.Object obj, Type[] objTypes, bool allowSceneObjects, string searchFilter, int controlID, Action<UnityEngine.Object> onObjectSelectorClosed, Action<UnityEngine.Object> onObjectSelectedUpdated)
        {
            Initialize();
            Show(obj, objTypes, null, allowSceneObjects, onObjectSelectorClosed: onObjectSelectorClosed, onObjectSelectedUpdated: onObjectSelectedUpdated);
            SetControlID(controlID);
            SetFilter("");
        }
    }
}
