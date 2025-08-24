using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Zlitz.General.Serializables
{
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneSceneReferenceDrawer : PropertyDrawer
    {
        private Action m_onSceneListChanged;

        private SerializedSceneReference m_serializedSceneReference;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            m_serializedSceneReference = new SerializedSceneReference(property);

            ObjectField sceneAssetField = new ObjectField(preferredLabel ?? property.displayName);
            sceneAssetField.AddToClassList("unity-base-field__aligned");
            sceneAssetField.style.flexGrow = 1.0f;
            sceneAssetField.objectType = typeof(SceneAsset);

            m_onSceneListChanged = () =>
            {
                SceneAsset sceneAsset = m_serializedSceneReference.sceneAssetProperty.objectReferenceValue as SceneAsset;
                SceneState sceneState = GetSceneState(sceneAsset);

                m_serializedSceneReference.scenePathProperty.stringValue = sceneAsset == null ? "" : AssetDatabase.GetAssetPath(sceneAsset);

                m_serializedSceneReference.serializedObject.ApplyModifiedProperties();

                sceneAssetField.value = sceneAsset;
                sceneAssetField.labelElement.style.color = sceneState switch
                {
                    SceneState.Disabled => Color.yellow,
                    SceneState.NotAdded => Color.red,
                    SceneState.Addressable => Color.cyan,
                    _ => EditorStyles.label.normal.textColor
                };
            };
            m_onSceneListChanged?.Invoke();

            EditorBuildSettings.sceneListChanged += m_onSceneListChanged;

            sceneAssetField.RegisterValueChangedCallback(e =>
            {
                SceneAsset newSceneAsset = e.newValue as SceneAsset;

                m_serializedSceneReference.sceneAssetProperty.objectReferenceValue = newSceneAsset;
                m_serializedSceneReference.scenePathProperty.stringValue = newSceneAsset == null ? "" : AssetDatabase.GetAssetPath(newSceneAsset);

                m_serializedSceneReference.serializedObject.ApplyModifiedProperties();

                sceneAssetField.labelElement.style.color = GetSceneState(e.newValue as SceneAsset) switch
                {
                    SceneState.Disabled    => Color.yellow,
                    SceneState.NotAdded    => Color.red,
                    SceneState.Addressable => Color.cyan,
                    _                      => EditorStyles.label.normal.textColor
                };
            });

            sceneAssetField.labelElement.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Add to build",
                    action =>
                    {
                        SceneAsset scene = m_serializedSceneReference.sceneAssetProperty.objectReferenceValue as SceneAsset;
                        if (scene == null)
                        {
                            return;
                        }

                        string scenePath = AssetDatabase.GetAssetPath(scene);
                        if (string.IsNullOrEmpty(scenePath))
                        {
                            return;
                        }

                        List<EditorBuildSettingsScene> buildScenes = EditorBuildSettings.scenes.ToList();
                        foreach (EditorBuildSettingsScene buildScene in buildScenes)
                        {
                            if (buildScene != null && buildScene.path == scenePath)
                            {
                                return;
                            }
                        }

                        EditorBuildSettingsScene newScene = new EditorBuildSettingsScene(scenePath, true);
                        buildScenes.Add(newScene);

                        EditorBuildSettings.scenes = buildScenes.ToArray();
                    },
                    action =>
                    {
                        SceneState state = GetSceneState(m_serializedSceneReference.sceneAssetProperty.objectReferenceValue as SceneAsset);
                        return (state == SceneState.NotAdded) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                    }
                );

                evt.menu.AppendAction("Remove from build",
                    action =>
                    {
                        SceneAsset scene = m_serializedSceneReference.sceneAssetProperty.objectReferenceValue as SceneAsset;
                        if (scene == null)
                        {
                            return;
                        }

                        string scenePath = AssetDatabase.GetAssetPath(scene);
                        if (string.IsNullOrEmpty(scenePath))
                        {
                            return;
                        }

                        List<EditorBuildSettingsScene> buildScenes = EditorBuildSettings.scenes.ToList();
                        for (int i = 0; i < buildScenes.Count; i++)
                        {
                            EditorBuildSettingsScene buildScene = buildScenes[i];
                            if (buildScene != null && buildScene.path == scenePath)
                            {
                                buildScenes.RemoveAt(i);
                                EditorBuildSettings.scenes = buildScenes.ToArray();
                                return;
                            }
                        }
                    },
                    action =>
                    {
                        SceneState state = GetSceneState(m_serializedSceneReference.sceneAssetProperty.objectReferenceValue as SceneAsset);
                        return (state == SceneState.Valid || state == SceneState.Disabled) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                    }
                );

                evt.menu.AppendAction("Enable in build",
                    action =>
                    {
                        SceneAsset scene = m_serializedSceneReference.sceneAssetProperty.objectReferenceValue as SceneAsset;
                        if (scene == null)
                        {
                            return;
                        }

                        string scenePath = AssetDatabase.GetAssetPath(scene);
                        if (string.IsNullOrEmpty(scenePath))
                        {
                            return;
                        }

                        List<EditorBuildSettingsScene> buildScenes = EditorBuildSettings.scenes.ToList();
                        foreach (EditorBuildSettingsScene buildScene in buildScenes)
                        {
                            if (buildScene != null && buildScene.path == scenePath)
                            {
                                buildScene.enabled = true;
                                EditorBuildSettings.scenes = buildScenes.ToArray();
                                return;
                            }
                        }
                    },
                    action =>
                    {
                        SceneState state = GetSceneState(m_serializedSceneReference.sceneAssetProperty.objectReferenceValue as SceneAsset);
                        return (state == SceneState.Disabled) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                    }
                );
                
                evt.menu.AppendAction("Disable in build",
                    action =>
                    {
                        SceneAsset scene = m_serializedSceneReference.sceneAssetProperty.objectReferenceValue as SceneAsset;
                        if (scene == null)
                        {
                            return;
                        }

                        string scenePath = AssetDatabase.GetAssetPath(scene);
                        if (string.IsNullOrEmpty(scenePath))
                        {
                            return;
                        }

                        List<EditorBuildSettingsScene> buildScenes = EditorBuildSettings.scenes.ToList();
                        foreach (EditorBuildSettingsScene buildScene in buildScenes)
                        {
                            if (buildScene != null && buildScene.path == scenePath)
                            {
                                buildScene.enabled = false;
                                EditorBuildSettings.scenes = buildScenes.ToArray();
                                return;
                            }
                        }
                    },
                    action =>
                    {
                        SceneState state = GetSceneState(m_serializedSceneReference.sceneAssetProperty.objectReferenceValue as SceneAsset);
                        return (state == SceneState.Valid) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                    }
                );

                evt.menu.AppendSeparator();

                evt.menu.AppendAction("Add to default addressable group",
                    action =>
                    {
                        SceneAsset scene = m_serializedSceneReference.sceneAssetProperty.objectReferenceValue as SceneAsset;
                        if (AddressableHelper.AddToDefaultGroup(scene))
                        {
                            m_onSceneListChanged?.Invoke();
                        }
                    },
                    action =>
                    {
                        if (!AddressableHelper.isAddressableSupported)
                        {
                            return DropdownMenuAction.Status.Disabled;
                        }
                        SceneState state = GetSceneState(m_serializedSceneReference.sceneAssetProperty.objectReferenceValue as SceneAsset);
                        return (state == SceneState.Valid || state == SceneState.Disabled || state == SceneState.NotAdded) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                    }
                );

                evt.menu.AppendAction("Remove from current addressable group",
                    action =>
                    {
                        SceneAsset scene = m_serializedSceneReference.sceneAssetProperty.objectReferenceValue as SceneAsset;
                        if (AddressableHelper.RemoveFromCurrentGroup(scene))
                        {
                            m_onSceneListChanged?.Invoke();
                        }
                    },
                    action =>
                    {
                        if (!AddressableHelper.isAddressableSupported)
                        {
                            return DropdownMenuAction.Status.Disabled;
                        }
                        SceneState state = GetSceneState(m_serializedSceneReference.sceneAssetProperty.objectReferenceValue as SceneAsset);
                        return (state == SceneState.Addressable) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                    }
                );

                evt.menu.AppendAction("Open addressable groups",
                    action =>
                    {
                        AddressableHelper.OpenGroups();
                    },
                    action =>
                    {
                        return AddressableHelper.isAddressableSupported ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                    }
                );
            }));

            return sceneAssetField;
        }

        ~SceneSceneReferenceDrawer()
        {
            if (m_onSceneListChanged != null)
            {
                EditorBuildSettings.sceneListChanged -= m_onSceneListChanged;
                m_onSceneListChanged = null;
            }
        }
        
        private SceneState GetSceneState(SceneAsset sceneAsset)
        {
            if (sceneAsset == null)
            {
                return SceneState.Null;
            }

            if (AddressableHelper.IsAddressable(sceneAsset))
            {
                return SceneState.Addressable;
            }

            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            foreach (EditorBuildSettingsScene includedScene in EditorBuildSettings.scenes)
            {
                if (scenePath == includedScene.path)
                {
                    return includedScene.enabled ? SceneState.Valid : SceneState.Disabled;
                }
            }

            return SceneState.NotAdded;
        }

        private enum SceneState
        {
            Null,
            Valid,
            Disabled,
            NotAdded,
            Addressable
        }

        private struct SerializedSceneReference
        {
            public SerializedObject serializedObject { get; private set; }

            public SerializedProperty sceneAssetProperty { get; private set; }

            public SerializedProperty scenePathProperty { get; private set; }

            public SerializedSceneReference(SerializedProperty property)
            {
                serializedObject = property.serializedObject;

                sceneAssetProperty = property.FindPropertyRelative("m_sceneAsset");
                scenePathProperty  = property.FindPropertyRelative("m_scenePath");
            }
        }
    }
}
