using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Zlitz.General.Serializables
{
    [Serializable]
    public struct SceneReference
    {
        [SerializeField]
        private UnityEngine.Object m_sceneAsset;

        [SerializeField]
        private string m_scenePath;

        public string scenePath => m_scenePath;

        public string sceneName => Path.GetFileNameWithoutExtension(m_scenePath);

        public int buildIndex => SceneUtility.GetBuildIndexByScenePath(m_scenePath);

        public override bool Equals(object obj)
        {
            if (obj is SceneReference other)
            {
                return EqualityComparer<UnityEngine.Object>.Default.Equals(m_sceneAsset, m_sceneAsset);
            }
            return m_sceneAsset == null && obj == null;
        }

        public override int GetHashCode()
        {
            return m_sceneAsset?.GetHashCode() ?? 0;
        }
    }
}
