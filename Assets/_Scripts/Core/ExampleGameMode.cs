using Assets.SceneManagement;
using UnityEngine;

namespace Assets.Core
{
    [CreateAssetMenu(menuName = GameModeAssetMenuName + "Example Mode", fileName = "Example Game Mode")]
    public class ExampleGameMode : AGameMode
    {
        public override void StartGameMode(SceneCollection collection = null)
        {
            base.StartGameMode(collection);

            Log($"Started {name} Game Mode");
        }

        public override void OnSceneCollectionChanged(SceneCollection newCollection = null)
        {
            base.OnSceneCollectionChanged(newCollection);

            Log($"Changed collection for {name} Game Mode");
        }

        public override void EndGameMode(SceneCollection collection = null)
        {
            base.EndGameMode(collection);

            Log($"Ended {name} Game Mode");
        }
    }
}
