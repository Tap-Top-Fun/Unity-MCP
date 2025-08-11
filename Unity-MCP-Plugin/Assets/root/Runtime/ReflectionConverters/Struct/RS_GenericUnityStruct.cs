#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

namespace com.IvanMurzak.Unity.MCP.Reflection.Convertor
{
    public partial class RS_GenericUnityStruct<T> : RS_GenericUnityNoProperties<T>
    {
        public override bool AllowCascadeSerialization => false;
    }
}