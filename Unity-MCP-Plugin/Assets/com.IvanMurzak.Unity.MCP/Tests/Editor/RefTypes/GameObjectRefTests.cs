#nullable enable
using System.Linq;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.ReflectorNet.Utils;
using NUnit.Framework;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests.RefTypes
{
    public partial class GameObjectRefTests : BaseRefTests
    {
        [Test]
        public void InstanceID_Description_Includes_All_Relevant_Props() => DescriptionContainsKeywords(
            prop: typeof(GameObjectRef).GetProperty(nameof(GameObjectRef.InstanceID)),
            expectedKeywords: GameObjectRef.GameObjectRefProperty.All
                .Where(name => name != ObjectRef.ObjectRefProperty.InstanceID));
    }
}