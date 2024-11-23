using System;
using System.Reflection;

using UnityEditor;

namespace Zlitz.General.Serializables
{
    internal static class AddressableHelper
    {
        private static Type s_addressableSettingsDefaultObjectType;
        private static object s_addressableSettings;

        public static bool isAddressableSupported
        {
            get
            {
                if (s_addressableSettings == null)
                {
                    s_addressableSettingsDefaultObjectType = Type.GetType("UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject, Unity.Addressables.Editor");

                    s_addressableSettings = s_addressableSettingsDefaultObjectType.GetProperty("Settings", BindingFlags.Static | BindingFlags.Public).GetValue(null);
                }
                return s_addressableSettings != null;
            }
        }

        public static bool IsAddressable(UnityEngine.Object obj)
        {
            if (obj == null || !isAddressableSupported)
            {
                return false;
            }

            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            string sceneGuid = AssetDatabase.AssetPathToGUID(assetPath);

            object entry = s_addressableSettings.GetType().GetMethod("FindAssetEntry", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(string) }, null)?.Invoke(s_addressableSettings, new object[] { sceneGuid });
            return entry != null;
        }
    
        public static bool AddToDefaultGroup(UnityEngine.Object obj)
        {
            if (obj == null || !isAddressableSupported)
            {
                return false;
            }

            string assetpath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetpath))
            {
                return false;
            }

            string guid = AssetDatabase.AssetPathToGUID(assetpath);

            Type settingType = s_addressableSettings.GetType();

            PropertyInfo defaultGroupProperty = settingType.GetProperty("DefaultGroup", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo   createOrMoveMethod   = settingType.GetMethod("CreateOrMoveEntry", BindingFlags.Instance | BindingFlags.Public);

            object defaultGroup = defaultGroupProperty?.GetValue(s_addressableSettings);
            if (defaultGroup != null)
            {
                Type groupType = defaultGroup.GetType();

                object newEntry = createOrMoveMethod.Invoke(s_addressableSettings, new object[]
                {
                    guid,
                    defaultGroup,
                    false,
                    false
                });

                return true;
            }

            return false;
        } 
    
        public static bool RemoveFromCurrentGroup(UnityEngine.Object obj)
        {
            if (obj == null || !isAddressableSupported)
            {
                return false;
            }

            string assetpath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetpath))
            {
                return false;
            }

            string guid = AssetDatabase.AssetPathToGUID(assetpath);

            Type settingType = s_addressableSettings.GetType();

            MethodInfo removeEntryMethod = settingType.GetMethod("RemoveAssetEntry", BindingFlags.Instance | BindingFlags.Public);
            if (removeEntryMethod != null)
            {
                bool result = (bool)removeEntryMethod.Invoke(s_addressableSettings, new object[]
                {
                    guid,
                    false
                });

                if (result)
                {
                    return true;
                }
            }

            return false;
        }
    
        public static void OpenGroups()
        {
            if (isAddressableSupported)
            {
                EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Groups");
            }
        }
    }
}
