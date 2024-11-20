using System.Reflection;
using System.Collections;

using UnityEngine;
using UnityEditor;
using System;

namespace Zlitz.General.Serializables
{
    internal static class SerializedPropertyHelper
    {
        public static object GetPropertyValue(SerializedProperty property)
        {
            return GetPropertyValue(property, out Type propertyType);
        }

        public static object GetPropertyValue(SerializedProperty property, out Type propertyType)
        {
            propertyType = null;

            object targetObject = property.serializedObject.targetObject;

            string[] pathParts = property.propertyPath.Replace("Array.data[", "[").Split('.');

            foreach (var part in pathParts)
            {
                bool valid = false;
                if (part.Contains("["))
                {
                    int arrayIndex = ExtractArrayIndex(part);

                    if (GetArrayElement(targetObject, arrayIndex, out object value, out Type type))
                    {
                        targetObject = value;
                        propertyType = type;

                        valid = true;
                    } 
                }
                else
                {
                    if (GetPropertyValue(targetObject, part, out object value, out Type type))
                    {
                        targetObject = value;
                        propertyType = type;

                        valid = true;
                    }
                }

                if (!valid)
                {
                    Debug.LogError($"Property path '{property.propertyPath}' is invalid.");
                    propertyType = null;
                    return null;
                }
            }

            return targetObject;
        }

        private static int ExtractArrayIndex(string arrayString)
        {
            int startIdx = arrayString.IndexOf('[') + 1;
            int endIdx = arrayString.IndexOf(']');
            string indexString = arrayString.Substring(startIdx, endIdx - startIdx);
            return int.Parse(indexString);
        }

        private static bool GetArrayElement(object targetObject, int index, out object value, out Type type)
        {
            value = null;
            type  = null;

            if (targetObject is IList list)
            {
                type = typeof(object);
                if (list.GetType().IsGenericType)
                {
                    type = list.GetType().GetGenericArguments()[0];
                }
                value = list[index];
                return true;
            }
            return false;
        }

        private static bool GetPropertyValue(object targetObject, string propertyName, out object value, out Type type)
        {
            value = null;
            type  = null;

            FieldInfo fieldInfo = targetObject.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                type  = fieldInfo.FieldType;
                value = fieldInfo.GetValue(targetObject);
                return true;
            }

            Debug.LogError($"Field '{propertyName}' not found on type {targetObject.GetType().Name}");
            return false;
        }
    }
}
