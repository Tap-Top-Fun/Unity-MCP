#nullable enable
using System.Linq;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.ReflectorNet.Utils;
using NUnit.Framework;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests.RefTypes
{
    public partial class AssetObjectRefTests : BaseRefTests
    {
        [Test]
        public void InstanceID_Description_Includes_All_Relevant_Props() => DescriptionContainsKeywords(
            prop: typeof(AssetObjectRef).GetProperty(nameof(AssetObjectRef.InstanceID)),
            expectedKeywords: AssetObjectRef.AssetObjectRefProperty.All
                .Where(name => name != ObjectRef.ObjectRefProperty.InstanceID));
    }
}