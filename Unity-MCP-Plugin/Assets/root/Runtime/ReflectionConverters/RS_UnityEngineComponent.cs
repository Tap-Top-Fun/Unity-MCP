#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

using System;
using System.Collections.Generic;
using System.Text;
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.Unity.MCP.Utils;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace com.IvanMurzak.Unity.MCP.Reflection.Convertor
{
    public partial class RS_UnityEngineComponent : RS_UnityEngineObject<UnityEngine.Component>
    {
        public override bool AllowSetValue => false;

        protected override IEnumerable<string> GetIgnoredProperties()
        {
            foreach (var property in base.GetIgnoredProperties())
                yield return property;

            yield return nameof(UnityEngine.Component.gameObject);
            yield return nameof(UnityEngine.Component.transform);
        }
        protected override object DeserializeValueAsJsonElement(
            Reflector reflector,
            SerializedMember data,
            Type type,
            int depth = 0,
            StringBuilder stringBuilder = null,
            ILogger logger = null)
        {
            return data.valueJsonElement
                .ToObjectRef(
                    reflector: reflector,
                    stringBuilder: stringBuilder,
                    logger: logger)
                .FindObject() as UnityEngine.Component;
        }
    }
}