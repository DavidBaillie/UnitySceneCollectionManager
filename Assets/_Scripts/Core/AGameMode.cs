using Assets.SceneManagement;

namespace Assets.Core
{
    public abstract class AGameMode : ATag
    {
        protected const string GameModeAssetMenuName = AssetMenuBaseName + "Game Mode/";

        public virtual void StartGameMode(SceneCollection collection = null) { }
        public virtual void EndGameMode(SceneCollection collection = null) { }
        public virtual void OnSceneCollectionChanged(SceneCollection newCollection = null) { }
    }
}
