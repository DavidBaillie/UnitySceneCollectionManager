using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Core
{
    public abstract class ATag : ScriptableObject
    {
        public const string AssetMenuBaseName = "Game/Tags/";

        public Guid Id = Guid.NewGuid();

        public virtual void InitializeTag(Dictionary<string, object> args = null) { }
        public virtual void CleanupTag() { }


        protected void Log(string message, UnityEngine.Object reference = null) => UnityEngine.Debug.Log($"[{GetType().Name}] {message}", reference);
        protected void LogWarning(string message, UnityEngine.Object reference = null) => UnityEngine.Debug.LogWarning($"[{GetType().Name}] {message}", reference);
        protected void LogError(string message, UnityEngine.Object reference = null) => UnityEngine.Debug.LogError($"[{GetType().Name}] {message}", reference);
    }
}
