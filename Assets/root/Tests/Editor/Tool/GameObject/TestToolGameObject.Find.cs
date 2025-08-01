using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Editor.API;
using com.IvanMurzak.Unity.MCP.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public partial class TestToolGameObject : BaseTest
    {
        [UnityTest]
        public IEnumerator FindByInstanceId()
        {
            var child = new GameObject(GO_ParentName).AddChild(GO_Child1Name);
            var result = new Tool_GameObject().Find(
                gameObjectRef: new GameObjectRef
                {
                    instanceID = child.GetInstanceID()
                });
            ResultValidation(result);

            Assert.IsTrue(result.Contains(GO_Child1Name), $"{GO_Child1Name} should be found in the path");
            yield return null;
        }

        [UnityTest]
        public IEnumerator FindByPath()
        {
            var child = new GameObject(GO_ParentName).AddChild(GO_Child1Name);
            var result = new Tool_GameObject().Find(
                gameObjectRef: new GameObjectRef
                {
                    path = $"{GO_ParentName}/{GO_Child1Name}"
                });
            ResultValidation(result);

            Assert.IsTrue(result.Contains(GO_Child1Name), $"{GO_Child1Name} should be found in the path");
            yield return null;
        }

        [UnityTest]
        public IEnumerator FindByName()
        {
            var child = new GameObject(GO_ParentName).AddChild(GO_Child1Name);
            var result = new Tool_GameObject().Find(
                gameObjectRef: new GameObjectRef
                {
                    name = GO_Child1Name
                });
            ResultValidation(result);

            Assert.IsTrue(result.Contains(GO_Child1Name), $"{GO_Child1Name} should be found in the path");
            yield return null;
        }

        [UnityTest]
        public IEnumerator FindByInstanceId_IncludeChildrenDepth_1_BriefData_False()
        {
            var go = new GameObject(GO_ParentName);
            go.AddChild(GO_Child1Name).AddComponent<SphereCollider>();
            go.AddChild(GO_Child2Name).AddComponent<SphereCollider>();
            go.AddComponent<SolarSystem>();
            yield return null;
            var result = new Tool_GameObject().Find(
                gameObjectRef: new GameObjectRef
                {
                    instanceID = go.GetInstanceID()
                },
                includeChildrenDepth: 1,
                briefData: false);

            ResultValidation(result);

            Assert.IsTrue(result.Contains(GO_ParentName), $"{GO_ParentName} should be found in the path");
            Assert.IsTrue(result.Contains(GO_Child1Name), $"{GO_Child1Name} should be found in the path");
            Assert.IsTrue(result.Contains(GO_Child2Name), $"{GO_Child2Name} should be found in the path");
            yield return null;
        }

        IResponseData<ResponseCallTool> FindByJson(string json) => RunTool("GameObject_Find", json);

        [UnityTest]
        public IEnumerator FindByJson_IncludeChildrenDepth_0_BriefData_True()
        {
            // WORKS
            var go = new GameObject(GO_ParentName);
            var json = $@"
            {{
              ""gameObjectRef"": {{
                ""instanceID"": {go.GetInstanceID()}
              }},
              ""includeChildrenDepth"": 0,
              ""briefData"": true
            }}";
            FindByJson(json);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FindByJson_IncludeChildrenDepth_0()
        {
            // FAILS
            var go = new GameObject(GO_ParentName);
            var json = $@"
            {{
              ""gameObjectRef"": {{
                ""instanceID"": {go.GetInstanceID()}
              }},
              ""includeChildrenDepth"": 0
            }}";
            FindByJson(json);
            yield return null;
        }
    }
}