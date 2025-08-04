using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.Unity.MCP.Common.Reflection.Convertor;
using com.IvanMurzak.Unity.MCP.Reflection.Convertor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.ReflectorNet.Convertor;
using com.IvanMurzak.ReflectorNet.Utils;
using System.Text;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public partial class TestSerializer : BaseTest
    {
        static void PrintSerializers<TTarget>()
        {
            Debug.Log($"Serialize <b>[{typeof(TTarget)}]</b> priority:\n" + string.Join("\n", Reflector.Instance.Convertors.GetAllSerializers()
                .Select(x => $"{x.GetType()}: Priority: {x.SerializationPriority(typeof(TTarget))}")
                .ToList()));
        }
        static void TestGetConvertor<TTarget, TSerializer>()
        {
            PrintSerializers<TTarget>();
            TestGetConvertor(typeof(TTarget), typeof(TSerializer));

            PrintSerializers<IEnumerable<TTarget>>();
            TestGetConvertor(typeof(IEnumerable<TTarget>), typeof(RS_ArrayUnity));

            PrintSerializers<List<TTarget>>();
            TestGetConvertor(typeof(List<TTarget>), typeof(RS_ArrayUnity));

            PrintSerializers<TTarget[]>();
            TestGetConvertor(typeof(TTarget[]), typeof(RS_ArrayUnity));

            Debug.Log($"-------------------------------------------");
        }
        static void TestGetConvertor(Type type, Type serializerType)
        {
            var converter = Reflector.Instance.Convertors.GetConvertor(type);
            Assert.IsNotNull(converter, $"{type}: Converter should not be null.");
            Assert.AreEqual(serializerType, converter.GetType(), $"{type}: The convertor should be {serializerType}.");
        }

        [UnityTest]
        public IEnumerator RS_SerializersOrder()
        {
            TestGetConvertor<int, PrimitiveReflectionConvertor>();
            TestGetConvertor<float, PrimitiveReflectionConvertor>();
            TestGetConvertor<bool, PrimitiveReflectionConvertor>();
            TestGetConvertor<string, PrimitiveReflectionConvertor>();
            TestGetConvertor<DateTime, PrimitiveReflectionConvertor>();
            TestGetConvertor<CultureTypes, PrimitiveReflectionConvertor>(); // enum
            TestGetConvertor<object, RS_GenericUnity<object>>();
            TestGetConvertor<ObjectRef, RS_GenericUnity<object>>();

            TestGetConvertor<UnityEngine.Object, RS_UnityEngineObject>();
            TestGetConvertor<UnityEngine.Vector3, RS_UnityEngineVector3>();
            TestGetConvertor<UnityEngine.Rigidbody, RS_UnityEngineComponent>();
            TestGetConvertor<UnityEngine.Animation, RS_UnityEngineComponent>();
            TestGetConvertor<UnityEngine.Material, RS_UnityEngineMaterial>();
            TestGetConvertor<UnityEngine.Transform, RS_UnityEngineTransform>();
            TestGetConvertor<UnityEngine.SpriteRenderer, RS_UnityEngineRenderer>();
            TestGetConvertor<UnityEngine.MeshRenderer, RS_UnityEngineRenderer>();

            yield return null;
        }

        [UnityTest]
        public IEnumerator SerializeMaterial()
        {
            var material = new Material(Shader.Find("Standard"));

            var serialized = Reflector.Instance.Serialize(material);
            var json = JsonUtils.ToJson(serialized);
            Debug.Log($"[{nameof(TestSerializer)}] Result:\n{json}");

            var glossinessValue = 1.0f;
            var colorValue = new Color(1.0f, 0.0f, 0.0f, 0.5f);

            serialized.SetPropertyValue("_Glossiness", glossinessValue);
            serialized.SetPropertyValue("_Color", colorValue);

            var objMaterial = (object)material;
            var stringBuilder = new StringBuilder();
            Reflector.Instance.TryPopulate(
                ref objMaterial,
                data: serialized,
                stringBuilder: stringBuilder,
                logger: McpPlugin.Instance.Logger);

            Assert.AreEqual(glossinessValue, material.GetFloat("_Glossiness"), 0.001f, $"Material property '_Glossiness' should be {glossinessValue}.");
            Assert.AreEqual(colorValue, material.GetColor("_Color"), $"Material property '_Glossiness' should be {glossinessValue}.");

            var stringResult = stringBuilder.ToString();

            Assert.IsTrue(stringResult.Contains("[Success]"), $"String result should contain '[Success]'. Result: {stringResult}");
            Assert.IsFalse(stringResult.Contains("[Error]"), $"String result should not contain '[Error]'. Result: {stringResult}");

            yield return null;
        }


        [UnityTest]
        public IEnumerator SerializeMaterialArray()
        {
            var material1 = new Material(Shader.Find("Standard"));
            var material2 = new Material(Shader.Find("Standard"));

            var materials = new[] { material1, material2 };

            var serialized = Reflector.Instance.Serialize(materials, logger: McpPlugin.Instance.Logger);
            var json = JsonUtils.ToJson(serialized);
            Debug.Log($"[{nameof(TestSerializer)}] Result:\n{json}");

            // var glossinessValue = 1.0f;
            // var colorValue = new Color(1.0f, 0.0f, 0.0f, 0.5f);

            // serialized.SetPropertyValue("_Glossiness", glossinessValue);
            // serialized.SetPropertyValue("_Color", colorValue);

            // var objMaterial = (object)material;
            // Serializer.Populate(ref objMaterial, serialized);

            // Assert.AreEqual(glossinessValue, material.GetFloat("_Glossiness"), 0.001f, $"Material property '_Glossiness' should be {glossinessValue}.");
            // Assert.AreEqual(colorValue, material.GetColor("_Color"), $"Material property '_Glossiness' should be {glossinessValue}.");

            yield return null;
        }

        void Test_Serialize_Deserialize<T>(T sourceObj)
        {
            var type = typeof(T);
            var serializedObj = Reflector.Instance.Serialize(sourceObj, logger: McpPlugin.Instance.Logger);
            var deserializedObj = Reflector.Instance.Deserialize(serializedObj, logger: McpPlugin.Instance.Logger);

            Debug.Log($"[{type.Name}] Source:\n```json\n{JsonUtils.ToJson(sourceObj)}\n```");
            Debug.Log($"[{type.Name}] Serialized:\n```json\n{JsonUtils.ToJson(serializedObj)}\n```");
            Debug.Log($"[{type.Name}] Deserialized:\n```json\n{JsonUtils.ToJson(deserializedObj)}\n```");

            Assert.AreEqual(sourceObj.GetType(), deserializedObj.GetType(), $"Object type should be {sourceObj.GetType()}.");

            foreach (var field in Reflector.Instance.GetSerializableFields(type) ?? Enumerable.Empty<FieldInfo>())
            {
                try
                {
                    var sourceValue = field.GetValue(sourceObj);
                    var targetValue = field.GetValue(deserializedObj);
                    Assert.AreEqual(sourceValue, targetValue, $"Field '{field.Name}' should be equal. Expected: {sourceValue}, Actual: {targetValue}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error getting field '{type.Name}{field.Name}'\n{ex}");
                    throw ex;
                }
            }
            foreach (var prop in Reflector.Instance.GetSerializableProperties(type) ?? Enumerable.Empty<PropertyInfo>())
            {
                try
                {
                    var sourceValue = prop.GetValue(sourceObj);
                    var targetValue = prop.GetValue(deserializedObj);
                    Assert.AreEqual(sourceValue, targetValue, $"Property '{prop.Name}' should be equal. Expected: {sourceValue}, Actual: {targetValue}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error getting property '{type.Name}{prop.Name}'\n{ex}");
                    throw ex;
                }
            }
        }

        [UnityTest]
        public IEnumerator Serialize_Deserialize()
        {
            Test_Serialize_Deserialize(100);
            Test_Serialize_Deserialize(true);
            Test_Serialize_Deserialize("hello world");
            Test_Serialize_Deserialize(new UnityEngine.Vector3(1, 2, 3));
            Test_Serialize_Deserialize(new UnityEngine.Color(1, 0.5f, 0, 1));
            Test_Serialize_Deserialize(new UnityEditor.Build.NamedBuildTarget());

            yield return null;
        }
    }
}