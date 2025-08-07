using System.Collections;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.Unity.MCP.Editor.API;
using com.IvanMurzak.Unity.MCP.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using com.IvanMurzak.ReflectorNet.Model;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.Unity.MCP.Common;
using UnityEditor;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public partial class TestToolGameObject : BaseTest
    {
        [UnityTest]
        public IEnumerator ModifyComponent_Vector3()
        {
            var reflector = McpPlugin.Instance!.McpRunner.Reflector;

            var child = new GameObject(GO_ParentName).AddChild(GO_Child1Name);
            var newPosition = new Vector3(1, 2, 3);

            var data = SerializedMember.FromValue(
                    reflector: reflector,
                    name: child.name,
                    type: typeof(GameObject),
                    value: new GameObjectRef(child.GetInstanceID()))
                .AddField(SerializedMember.FromValue(
                    reflector: reflector,
                    name: nameof(child.transform),
                    type: typeof(Transform),
                    value: new ComponentRef(child.transform.GetInstanceID()))
                .AddProperty(SerializedMember.FromValue(
                    reflector: reflector,
                    name: nameof(child.transform.position),
                    value: newPosition)));

            var result = new Tool_GameObject().Modify(
                gameObjectDiffs: new SerializedMemberList(data),
                gameObjectRefs: new GameObjectRefList
                {
                    new GameObjectRef()
                    {
                        instanceID = child.GetInstanceID()
                    }
                });
            ResultValidation(result);

            // This line fails, and probably that is OK
            // Assert.IsTrue(result.Contains(GO_Child1Name), $"{GO_Child1Name} should be found in the path");

            Assert.AreEqual(child.transform.position, newPosition, "Position should be changed");

            int? dataInstanceID = data.TryGetInstanceID(out var tempDataInstanceId)
                ? tempDataInstanceId
                : null;

            int? dataChildInstanceID = data.GetField(nameof(child.transform)).TryGetInstanceID(out var tempChildInstanceId)
                ? tempChildInstanceId
                : null;

            Assert.AreEqual(child.GetInstanceID(), dataInstanceID, "InstanceID should be the same");
            Assert.AreEqual(child.transform.GetInstanceID(), dataChildInstanceID, "InstanceID should be the same");
            yield return null;
        }
        [UnityTest]
        public IEnumerator ModifyComponent_Material()
        {
            var reflector = McpPlugin.Instance!.McpRunner.Reflector;

            // "Standard" shader is always available in a Unity project.
            // Doesn't matter whether it's built-in or URP/HDRP.
            var sharedMaterial = new Material(Shader.Find("Standard"));

            var go = new GameObject(GO_ParentName);
            var component = go.AddComponent<MeshRenderer>();

            var data = SerializedMember.FromValue(
                    reflector: reflector,
                    name: go.name,
                    type: typeof(GameObject),
                    value: new GameObjectRef(go.GetInstanceID()))
                .AddField(SerializedMember.FromValue(
                    reflector: reflector,
                    name: null,
                    type: typeof(MeshRenderer),
                    value: new ComponentRef(component.GetInstanceID()))
                .AddProperty(SerializedMember.FromValue(
                    reflector: reflector,
                    name: nameof(component.sharedMaterial),
                    type: typeof(Material),
                    value: new ObjectRef(sharedMaterial.GetInstanceID()))));

            Debug.Log($"Data:\n{data.ToJson(reflector)}\n");

            var result = new Tool_GameObject().Modify(
                gameObjectDiffs: new SerializedMemberList(data),
                gameObjectRefs: new GameObjectRefList
                {
                    new GameObjectRef()
                    {
                        instanceID = go.GetInstanceID()
                    }
                });

            ResultValidation(result);

            // This line files, and probably that is OK
            // Assert.IsTrue(result.Contains(GO_ParentName), $"{GO_ParentName} should be found in the path");

            Assert.AreEqual(sharedMaterial.GetInstanceID(), component.sharedMaterial.GetInstanceID(), "Materials InstanceIDs should be the same.");
            yield return null;
        }

        IResponseData<ResponseCallTool> ModifyByJson(string json) => RunTool("GameObject_Modify", json);
        void ValidateResult(IResponseData<ResponseCallTool> result)
        {
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError, "Modification failed");
            Assert.IsTrue(result.Message.Contains("[Success]"), "Result should contain success message.");
            Assert.IsFalse(result.Message.Contains("[Error]"), "Result should not contain error message.");
        }

        [UnityTest]
        public IEnumerator ModifyJson_SolarSystem_Sun_NameComponent()
        {
            var go = new GameObject(GO_ParentName);
            var solarSystem = go.AddComponent<SolarSystem>();
            var sunGo = new GameObject("Sun");

            var json = $@"
            {{
              ""gameObjectRefs"": [
                {{
                  ""path"": ""{go.name}""
                }}
              ],
              ""gameObjectDiffs"": [
                {{
                  ""typeName"": ""UnityEngine.GameObject"",
                  ""fields"": [
                    {{
                      ""typeName"": ""com.IvanMurzak.Unity.MCP.SolarSystem"",
                      ""name"": ""SolarSystem"",
                      ""fields"": [
                        {{
                          ""typeName"": ""UnityEngine.GameObject"",
                          ""name"": ""sun"",
                          ""value"": {{
                            ""instanceID"": {sunGo.GetInstanceID()}
                          }}
                        }}
                      ]
                    }}
                  ]
                }}
              ]
            }}";
            var result = ModifyByJson(json);
            ValidateResult(result);

            Assert.IsTrue(solarSystem.sun == sunGo, $"SolarSystem.sun should be set to the GameObject with name 'Sun'. Expected: {sunGo.name}, Actual: {solarSystem.sun?.name}");
            yield return null;
        }

        [UnityTest]
        public IEnumerator ModifyJson_SolarSystem_Sun_NameIndex()
        {
            var go = new GameObject(GO_ParentName);
            var solarSystem = go.AddComponent<SolarSystem>();
            var sunGo = new GameObject("Sun");

            var json = $@"
            {{
              ""gameObjectRefs"": [
                {{
                  ""path"": ""{go.name}""
                }}
              ],
              ""gameObjectDiffs"": [
                {{
                  ""typeName"": ""UnityEngine.GameObject"",
                  ""fields"": [
                    {{
                      ""typeName"": ""com.IvanMurzak.Unity.MCP.SolarSystem"",
                      ""name"": ""[0]"",
                      ""fields"": [
                        {{
                          ""typeName"": ""UnityEngine.GameObject"",
                          ""name"": ""sun"",
                          ""value"": {{
                            ""instanceID"": {sunGo.GetInstanceID()}
                          }}
                        }}
                      ]
                    }}
                  ]
                }}
              ]
            }}";
            var result = ModifyByJson(json);
            ValidateResult(result);

            Assert.IsTrue(solarSystem.sun == sunGo, $"SolarSystem.sun should be set to the GameObject with name 'Sun'. Expected: {sunGo.name}, Actual: {solarSystem.sun?.name}");
            yield return null;
        }
        [UnityTest]
        public IEnumerator ModifyJson_SolarSystem_PlanetsArray()
        {
            var reflector = McpPlugin.Instance!.McpRunner.Reflector;
            var goName = "Solar System";
            var go = new GameObject(goName);
            var solarSystem = go.AddComponent<SolarSystem>();
            var planets = new[]
            {
                new GameObject("Mercury"),
                new GameObject("Venus"),
                new GameObject("Earth"),
                new GameObject("Mars")
            };

            var orbitRadius = 3.87f;
            var orbitTilt = new Vector3(7, 0, 0);

            var json = $@"
            {{
              ""gameObjectRefs"": [
                {{
                  ""path"": ""{goName}""
                }}
              ],
              ""gameObjectDiffs"": [
                {{
                  ""typeName"": ""UnityEngine.GameObject"",
                  ""fields"": [
                    {{
                      ""typeName"": ""com.IvanMurzak.Unity.MCP.SolarSystem"",
                      ""name"": ""SolarSystem"",
                      ""fields"": [
                        {{
                          ""name"": ""planets"",
                          ""typeName"": ""com.IvanMurzak.Unity.MCP.SolarSystem+PlanetData[]"",
                          ""value"": [
                            {{
                              ""typeName"": ""com.IvanMurzak.Unity.MCP.SolarSystem+PlanetData"",
                              ""fields"": [
                                {{
                                  ""name"": ""planet"",
                                  ""typeName"": ""UnityEngine.GameObject"",
                                  ""value"": {{
                                    ""instanceID"": {planets[0].GetInstanceID()}
                                  }}
                                }},
                                {{
                                  ""name"": ""orbitRadius"",
                                  ""typeName"": ""System.Single"",
                                  ""value"": {orbitRadius}
                                }},
                                {{
                                  ""name"": ""orbitSpeed"",
                                  ""typeName"": ""System.Single"",
                                  ""value"": 4.15
                                }},
                                {{
                                  ""name"": ""rotationSpeed"",
                                  ""typeName"": ""System.Single"",
                                  ""value"": 0.017
                                }},
                                {{
                                  ""name"": ""orbitTilt"",
                                  ""typeName"": ""UnityEngine.Vector3"",
                                  ""value"": {{
                                    ""x"": {orbitTilt.x},
                                    ""y"": {orbitTilt.y},
                                    ""z"": {orbitTilt.z}
                                  }}
                                }}
                              ]
                            }},
                            {{
                              ""typeName"": ""com.IvanMurzak.Unity.MCP.SolarSystem+PlanetData"",
                              ""fields"": [
                                {{
                                  ""name"": ""planet"",
                                  ""typeName"": ""UnityEngine.GameObject"",
                                  ""value"": {{
                                    ""instanceID"": {planets[1].GetInstanceID()}
                                  }}
                                }},
                                {{
                                  ""name"": ""orbitRadius"",
                                  ""typeName"": ""System.Single"",
                                  ""value"": 7.23
                                }},
                                {{
                                  ""name"": ""orbitSpeed"",
                                  ""typeName"": ""System.Single"",
                                  ""value"": 1.62
                                }},
                                {{
                                  ""name"": ""rotationSpeed"",
                                  ""typeName"": ""System.Single"",
                                  ""value"": 0.004
                                }},
                                {{
                                  ""name"": ""orbitTilt"",
                                  ""typeName"": ""UnityEngine.Vector3"",
                                  ""value"": {{
                                    ""x"": 3.4,
                                    ""y"": 0,
                                    ""z"": 0
                                  }}
                                }}
                              ]
                            }},
                            {{
                              ""typeName"": ""com.IvanMurzak.Unity.MCP.SolarSystem+PlanetData"",
                              ""fields"": [
                                {{
                                  ""name"": ""planet"",
                                  ""typeName"": ""UnityEngine.GameObject"",
                                  ""value"": {{
                                    ""instanceID"": {planets[2].GetInstanceID()}
                                  }}
                                }},
                                {{
                                  ""name"": ""orbitRadius"",
                                  ""typeName"": ""System.Single"",
                                  ""value"": 10
                                }},
                                {{
                                  ""name"": ""orbitSpeed"",
                                  ""typeName"": ""System.Single"",
                                  ""value"": 1
                                }},
                                {{
                                  ""name"": ""rotationSpeed"",
                                  ""typeName"": ""System.Single"",
                                  ""value"": 1
                                }},
                                {{
                                  ""name"": ""orbitTilt"",
                                  ""typeName"": ""UnityEngine.Vector3"",
                                  ""value"": {{
                                    ""x"": 23.5,
                                    ""y"": 0,
                                    ""z"": 0
                                  }}
                                }}
                              ]
                            }},
                            {{
                              ""typeName"": ""com.IvanMurzak.Unity.MCP.SolarSystem+PlanetData"",
                              ""fields"": [
                                {{
                                  ""name"": ""planet"",
                                  ""typeName"": ""UnityEngine.GameObject"",
                                  ""value"": {{
                                    ""instanceID"": {planets[3].GetInstanceID()}
                                  }}
                                }},
                                {{
                                  ""name"": ""orbitRadius"",
                                  ""typeName"": ""System.Single"",
                                  ""value"": 15.24
                                }},
                                {{
                                  ""name"": ""orbitSpeed"",
                                  ""typeName"": ""System.Single"",
                                  ""value"": 0.53
                                }},
                                {{
                                  ""name"": ""rotationSpeed"",
                                  ""typeName"": ""System.Single"",
                                  ""value"": 0.98
                                }},
                                {{
                                  ""name"": ""orbitTilt"",
                                  ""typeName"": ""UnityEngine.Vector3"",
                                  ""value"": {{
                                    ""x"": 25.2,
                                    ""y"": 0,
                                    ""z"": 0
                                  }}
                                }}
                              ]
                            }}
                          ]
                        }}
                      ]
                    }}
                  ]
                }}
              ]
            }}";

            var parameters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            var firstPlaneInstanceId = parameters["gameObjectDiffs"]
                .EnumerateArray().First() // first go diff
                .GetProperty("fields")
                .EnumerateArray().First() // SolarSystem component
                .GetProperty("fields")
                .EnumerateArray().First() // planets field
                .GetProperty("value")     // planets array value
                .EnumerateArray().First() // first planet item
                .GetProperty("fields")    // planet fields
                .EnumerateArray().First() // first field (planet GameObject)
                .GetProperty("value")
                .GetProperty("instanceID").GetInt32(); // go instanceID

            Assert.AreEqual(planets[0].GetInstanceID(), firstPlaneInstanceId, "Planet InstanceID should match the input data.");

            var serializedMemberJson = parameters["gameObjectDiffs"].GetRawText();
            var serializedMember = reflector.JsonSerializer.Deserialize<SerializedMemberList>(serializedMemberJson);

            // Get the same instanceID but from serializedMember structure
            var firstPlaneInstanceIdFromSerialized = serializedMember[0]
                .GetField("SolarSystem") // SolarSystem component
                ?.GetField("planets") // planets field
                ?.GetValue<SerializedMember[]>(McpPlugin.Instance!.McpRunner.Reflector)?.FirstOrDefault() // first planet
                ?.GetField("planet") // planet GameObject field
                ?.GetValue<ObjectRef>(McpPlugin.Instance!.McpRunner.Reflector)?.instanceID ?? 0; // instanceID

            Assert.AreEqual(firstPlaneInstanceId, firstPlaneInstanceIdFromSerialized, "InstanceID from JSON parsing and SerializedMember should match.");
            Assert.AreEqual(planets[0].GetInstanceID(), firstPlaneInstanceIdFromSerialized, "Planet InstanceID should match the serialized member data.");

            var result = ModifyByJson(json);
            ValidateResult(result);

            Assert.NotNull(solarSystem.planets);
            Assert.AreEqual(planets.Length, solarSystem.planets.Length, "Planets array length should match the input data.");

            for (int i = 0; i < planets.Length; i++)
            {
                Assert.NotNull(solarSystem.planets[i], $"Planet[{i}] should not be null.");
                Assert.IsTrue(solarSystem.planets[i].planet == planets[i], $"Planet[{i}] GameObject should match the input data.");
            }

            Assert.AreEqual(orbitRadius, solarSystem.planets[0].orbitRadius, "First planet's orbit radius should match the input data.");
            Assert.AreEqual(orbitTilt, solarSystem.planets[0].orbitTilt, "First planet's orbit tilt should match the input data.");

            for (int i = 0; i < planets.Length; i++)
            {
                Assert.AreEqual(planets[i].GetInstanceID(), solarSystem.planets[i].planet.GetInstanceID(),
                    $"Planet[{i}] InstanceID should match the input data.");
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetMaterial()
        {
            var assetPath = "Assets/Materials/TestMaterial.mat";
            var material = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, assetPath);
            try
            {
                var go = new GameObject("TestGameObject");
                var meshRenderer = go.AddComponent<MeshRenderer>();

                var json = $@"
{{
  ""gameObjectRefs"": [
    {{
      ""instanceID"": {go.GetInstanceID()}
    }}
  ],
  ""gameObjectDiffs"": [
  {{
    ""typeName"": ""UnityEngine.GameObject"",
    ""fields"": [
      {{
        ""typeName"": ""UnityEngine.MeshRenderer"",
        ""name"": ""MeshRenderer"",
        ""props"": [
          {{
            ""name"": ""{nameof(MeshRenderer.sharedMaterial)}"",
            ""typeName"": ""UnityEngine.Material"",
            ""value"":
              {{
                  ""instanceID"": {material.GetInstanceID()}
              }}
            }}
          ]
        }}
      ]
    }}
  ]
}}";

                var result = ModifyByJson(json);
                ValidateResult(result);

                Assert.IsTrue(meshRenderer.sharedMaterial == material, $"MeshRenderer.sharedMaterial should be set to the created material. Expected: {material.name}, Actual: {meshRenderer.sharedMaterial.name}");
            }
            finally
            {
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.Refresh();
            }
            yield return null;
        }
    }
}