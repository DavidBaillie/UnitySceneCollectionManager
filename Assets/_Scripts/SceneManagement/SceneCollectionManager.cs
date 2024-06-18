using Assets.Core.Common.Decorator;
using Assets.Core.Common.Enums;
using Assets.Core.Initialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.SceneManagement
{
    public static class SceneCollectionManager
    {
        private static SceneCollection savedPreviousCollection = null;
        private static Scene? collectionManagerScene = null;

        public static Action OnSceneCollectionStartedLoading;
        public static Action OnSceneCollectionCompletedLoading;

        private static Coroutine runningCoroutineHandle;
        private static SceneManagerHook sceneHook;

        public static void LoadSceneCollection(SceneCollection collection)
        {
            //Do nothing when the state data is invalid
            if (collection == null || collection.GameMode == null)
            {
                Debug.LogError($"Cannot load scene collection because " + 
                    (collection == null ? " the provided collection is null!" : " the provided collection has no game mode!"));
                return;
            }

            if (collection == savedPreviousCollection)
            {
                Debug.Log($"Ignoring request to load collection {collection.name} because the collection is currently loaded.");
            }

            if (sceneHook == null) 
            {
                sceneHook = new GameObject("SCENE MANAGER HOOK").AddComponent<SceneManagerHook>();
            }

            //Kill the last instance if it's running
            if (runningCoroutineHandle != null)
                sceneHook.StopCoroutine(runningCoroutineHandle);

            //Signal start event
            OnSceneCollectionStartedLoading?.Invoke();
            runningCoroutineHandle = sceneHook.StartCoroutine(HandleSceneChangeProcessing(savedPreviousCollection, collection));
        }

        /// <summary>
        /// Takes scene collectionand handles changing the scenes
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="initializeGameMode"></param>
        private static IEnumerator HandleSceneChangeProcessing(SceneCollection prevCollection, SceneCollection newCollection)
        {
            //Make sure the persistent scene exists
            if (!collectionManagerScene.HasValue)
            {
                collectionManagerScene = SceneManager.CreateScene("SCENE MANAGER");
                SceneManager.SetActiveScene(collectionManagerScene.Value);
            }

            //Figure out which scenes need to have what happen to them
            GenerateSceneLoadingCollectionsForGameModes(prevCollection, newCollection, out List<Scene> unknownScenes,
                out List<SceneReferenceOption> scenesToLoad, out List<SceneReferenceOption> scenesToUnload);

            //If the previous collection is different than the new one, end the game mode before changing scenes
            if (prevCollection != null && prevCollection.GameMode != newCollection.GameMode)
                prevCollection.GameMode.EndGameMode(prevCollection);

            //List to track scene load actions
            List<AsyncOperation> sceneLoadOperations = new();

            //Handle unloading all known scenes from the last collection
            if (scenesToUnload.Count > 0)
            {
                foreach(SceneReferenceOption option in scenesToUnload)
                {
                    sceneLoadOperations.Add(SceneManager.UnloadSceneAsync(option.Scene.ScenePath));
                }

                while (sceneLoadOperations.Any(x => x != null && !x.isDone))
                    yield return null;

                sceneLoadOperations.Clear();
            }

            //Handle loading tracked scenes
            if (scenesToLoad.Count > 0)
            {
                foreach (SceneReferenceOption option in scenesToLoad)
                {
                    sceneLoadOperations.Add(SceneManager.LoadSceneAsync(option.Scene.ScenePath, LoadSceneMode.Additive));
                }

                while (sceneLoadOperations.Any(x => x != null && !x.isDone))
                    yield return null;

                sceneLoadOperations.Clear();
            }

            //Handle unknown scenes
            if (unknownScenes.Count > 0)
            {
                foreach (Scene scene in unknownScenes)
                {
                    AsyncOperation op = SceneManager.UnloadSceneAsync(scene);

                    if (op != null)
                        sceneLoadOperations.Add(op);
                }

                while (sceneLoadOperations.Any(x => !x.isDone))
                    yield return null;

                sceneLoadOperations.Clear();
            }

            //Same game mode, different collection
            if (prevCollection != null && prevCollection.GameMode == newCollection.GameMode)
            {
                newCollection.GameMode.OnSceneCollectionChanged(newCollection);
            }
            //Different game mode
            else if (prevCollection == null || prevCollection.GameMode != newCollection.GameMode)
            {
                newCollection.GameMode.StartGameMode(newCollection);
            }

            //Save new collection
            savedPreviousCollection = newCollection;
        }

        /// <summary>
        /// Returns a list of scene objects that are currently loaded except for the preload scene (build index 0)
        /// </summary>
        private static List<Scene> GetAllLoadedScenes()
        {
            List<Scene> result = new List<Scene>(Mathf.Max(1, SceneManager.sceneCount - 1));

            //For all scenes
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (!collectionManagerScene.HasValue || scene.name != collectionManagerScene.Value.name)
                    result.Add(scene);
            }

            return result;
        }


        private static void GenerateSceneLoadingCollectionsForGameModes(SceneCollection previousCollection, SceneCollection newCollection, out List<Scene> unknownScenes, 
            out List<SceneReferenceOption> scenesToLoad, out List<SceneReferenceOption> scenesToUnload)
        {
            unknownScenes = new List<Scene>();
            scenesToLoad = new List<SceneReferenceOption>();
            scenesToUnload = new List<SceneReferenceOption>();

            List<SceneReferenceOption> loadedSceneReferences = new List<SceneReferenceOption>();

            //If there was a collection before this, match the reference to the scene asset loaded
            //If no reference can be matched, mark the scene as unknown
            if (previousCollection != null)
            {
                foreach (Scene scene in GetAllLoadedScenes())
                {
                    SceneReferenceOption option = previousCollection.Scenes.FirstOrDefault(x => x.Scene.ScenePath == scene.path);
                    if (option != null)
                        loadedSceneReferences.Add(option);
                    else 
                        unknownScenes.Add(scene);
                }
            }
            //No previous collection, mark all loaded scenes as unknown
            else
            {
                unknownScenes = new List<Scene>(GetAllLoadedScenes());
            }

            //For all the new scenes coming in, decide what to keep and what to load
            foreach (SceneReferenceOption option in newCollection.Scenes)
            {
                //See if the inbound option is already loaded
                SceneReferenceOption existingOption = loadedSceneReferences.FirstOrDefault(x => x.Scene.ScenePath == option.Scene.ScenePath);

                //Not loaded, load it
                if (existingOption == null)
                {
                    scenesToLoad.Add(option);
                }
                //Otherwise we have a duplicate scene, act accordingly
                else
                {
                    //If it needs to be reloaded, mark it for a reload
                    if (existingOption.loadingMode == SceneLoadingMode.AlwaysLoad)
                    {
                        scenesToLoad.Add(option);
                    }
                    //Otherwise remove it from the tracked scenes that will be unloaded after this loop
                    else
                    {
                        loadedSceneReferences.Remove(existingOption);
                    }
                }
            }

            //Whatever currently exists that wasn't cleared from this list prior needs to be unloaded
            scenesToUnload.AddRange(loadedSceneReferences);
        }
    }
}
