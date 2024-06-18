using Assets.SceneManagement;
using UnityEngine;

namespace Assets.Core.Canvas
{
    public class GameModeChangeController : MonoBehaviour
    {
        public void ChangeGameMode(SceneCollection collection)
        {
            SceneCollectionManager.LoadSceneCollection(collection);
        }
    }
}
