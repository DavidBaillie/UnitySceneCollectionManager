using UnityEngine;

namespace Assets.Core.Initialization
{
    public class SceneManagerHook : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
