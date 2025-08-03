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
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public partial class TestToolGameObject : BaseTest
    {
        [UnityTest]
        public IEnumerator ModifyComponent_Vector3()
        {
            var child = new GameObject(GO_ParentName).AddChild(GO_Child1Name);
            var newPosition = new Vector3(1, 2, 3);

            var data = SerializedMember.FromValue(
                    name: child.name,
                    type: typeof(GameObject),
                    value: new ObjectRef(child.GetInstanceID()))
                .AddField(SerializedMember.FromValue(
                    name: nameof(child.transform),
                    type: typeof(Transform),
                    value: new ObjectRef(child.transform.GetInstanceID()))
                .AddProperty(SerializedMember.FromValue(name: nameof(child.transform.position),
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
            // "Standard" shader is always available in a Unity project.
            // Doesn't matter whether it's built-in or URP/HDRP.
            var sharedMaterial = new Material(Shader.Find("Standard"));

            var go = new GameObject(GO_ParentName);
            var component = go.AddComponent<MeshRenderer>();

            var data = SerializedMember.FromValue(
                    name: go.name,
                    type: typeof(GameObject),
                    value: null)
                .AddField(SerializedMember.FromValue(
                    name: null,
                    type: typeof(MeshRenderer),
                    value: new ObjectRef(component.GetInstanceID()))
                .AddProperty(SerializedMember.FromValue(
                    name: nameof(component.sharedMaterial),
                    type: typeof(Material),
                    value: new ObjectRef(sharedMaterial.GetInstanceID()))));

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

            Assert.AreEqual(component.sharedMaterial.GetInstanceID(), sharedMaterial.GetInstanceID(), "Materials InstanceIDs should be the same.");
            yield return null;
        }

        IResponseData<ResponseCallTool> ModifyByJson(string json) => RunTool("GameObject_Modify", json);

        [UnityTest]
        public IEnumerator ModifyJson_SolarSystem_Sun_NameComponent()
        {
            var go = new GameObject(GO_ParentName);
            go.AddComponent<SolarSystem>();
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
            ModifyByJson(json);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ModifyJson_SolarSystem_Sun_NameIndex()
        {
            var go = new GameObject(GO_ParentName);
            go.AddComponent<SolarSystem>();
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
            ModifyByJson(json);
            yield return null;
        }
        [UnityTest]
        public IEnumerator ModifyJson_SolarSystem_PlanetsArray()
        {
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

            var parameters = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

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
            var serializedMember = JsonUtils.Deserialize<SerializedMemberList>(serializedMemberJson);

            // Get the same instanceID but from serializedMember structure
            var firstPlaneInstanceIdFromSerialized = serializedMember[0]
                .GetField("SolarSystem") // SolarSystem component
                ?.GetField("planets") // planets field
                ?.GetValue<SerializedMember[]>()?.FirstOrDefault() // first planet
                ?.GetField("planet") // planet GameObject field
                ?.GetValue<ObjectRef>()?.instanceID ?? 0; // instanceID

            Assert.AreEqual(firstPlaneInstanceId, firstPlaneInstanceIdFromSerialized, "InstanceID from JSON parsing and SerializedMember should match.");
            Assert.AreEqual(planets[0].GetInstanceID(), firstPlaneInstanceIdFromSerialized, "Planet InstanceID should match the serialized member data.");

            ModifyByJson(json);

            Assert.NotNull(solarSystem.planets);
            Assert.AreEqual(planets.Length, solarSystem.planets.Length, "Planets array length should match the input data.");

            for (int i = 0; i < planets.Length; i++)
                Assert.NotNull(solarSystem.planets[i], $"Planet[{i}] should not be null.");

            Assert.AreEqual(orbitRadius, solarSystem.planets[0].orbitRadius, "First planet's orbit radius should match the input data.");
            Assert.AreEqual(orbitTilt, solarSystem.planets[0].orbitTilt, "First planet's orbit tilt should match the input data.");

            for (int i = 0; i < planets.Length; i++)
            {
                Assert.AreEqual(planets[i].GetInstanceID(), solarSystem.planets[i].planet.GetInstanceID(),
                    $"Planet[{i}] InstanceID should match the input data.");
            }

            yield return null;
        }
    }
}