using Assets.SceneManagement;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Core.Initialization
{
    public class PreloadInitializer : MonoBehaviour
    {
        [SerializeField]
        private SceneCollection collectionToLoad = null;

        [SerializeField]
        private List<GameObject> persistentPrefabs = new();

        [SerializeField]
        private List<ATag> preloadedTags = new();


        private void Awake()
        {
            GameObject spawn = null;

            foreach (GameObject prefab in persistentPrefabs )
            {
                if (prefab == null ) continue;

                spawn = Instantiate(prefab, null);
                DontDestroyOnLoad(spawn);
            }

            foreach (ATag tag in preloadedTags)
            {
                if (tag == null) continue;

                try { tag.InitializeTag(); } catch { UnityEngine.Debug.LogError($"Failed to initialize tag {tag}"); }
            }

            if (collectionToLoad != null)
                SceneCollectionManager.LoadSceneCollection(collectionToLoad);
        }
    }
}
