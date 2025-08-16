#nullable enable
using System.Collections.Generic;
using System.Reflection;
using com.IvanMurzak.ReflectorNet.Utils;
using NUnit.Framework;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests.RefTypes
{
    public partial class BaseRefTests : BaseTest
    {
        protected void DescriptionContainsKeywords(PropertyInfo? prop, IEnumerable<string> expectedKeywords)
        {
            var desc = TypeUtils.GetDescription(prop);

            Assert.IsNotNull(desc, "Description attribute is missing.");

            foreach (var keyword in expectedKeywords)
                StringAssert.Contains($"'{keyword}'", desc, $"Description must mention '{keyword}'.");
        }
    }
}