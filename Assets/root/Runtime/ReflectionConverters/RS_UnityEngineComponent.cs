#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

using System.Collections.Generic;

namespace com.IvanMurzak.Unity.MCP.Reflection.Convertor
{
    public partial class RS_UnityEngineComponent : RS_UnityEngineObject<UnityEngine.Component>
    {
        protected override IEnumerable<string> GetIgnoredProperties()
        {
            foreach (var property in base.GetIgnoredProperties())
                yield return property;

            yield return nameof(UnityEngine.Component.gameObject);
            yield return nameof(UnityEngine.Component.transform);
        }
    }
}