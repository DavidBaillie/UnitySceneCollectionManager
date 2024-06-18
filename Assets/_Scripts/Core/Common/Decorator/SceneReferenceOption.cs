using Assets.Core.Common.Enums;
using System;

namespace Assets.Core.Common.Decorator
{
    [Serializable]
    public class SceneReferenceOption
    {
        public SceneReference Scene;
        public SceneLoadingMode loadingMode;
    }
}
