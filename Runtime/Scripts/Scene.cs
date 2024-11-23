using System;

using UnityEngine;

namespace Zlitz.General.Serializables
{
    [Serializable]
    public struct Scene
    {
        [SerializeField]
        private UnityEngine.Object m_sceneAsset;

        [SerializeField]
        private string m_scenePath;

        public string scenePath => m_scenePath;
    }
}
