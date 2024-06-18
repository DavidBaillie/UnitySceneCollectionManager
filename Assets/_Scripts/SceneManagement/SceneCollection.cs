using Assets.Core;
using Assets.Core.Common.Decorator;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.SceneManagement
{
    [CreateAssetMenu(menuName = AssetMenuBaseName + "Scene Collection", fileName = "New Scene Collection")]
    public class SceneCollection : ATag
    {
        [SerializeField]
        public AGameMode GameMode = null;

        [SerializeField]
        public List<SceneReferenceOption> Scenes = new();
    }
}
