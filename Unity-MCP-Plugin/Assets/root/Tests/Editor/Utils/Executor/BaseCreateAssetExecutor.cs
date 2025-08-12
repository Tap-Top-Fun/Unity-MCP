#nullable enable
using System;
using UnityEditor;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests.Utils
{
    public abstract class BaseCreateAssetExecutor<T> : LazyNodeExecutor
    {
        protected readonly string _assetName;
        protected readonly CreateFolderExecutor _createFolderExecutor;

        public T? Asset { get; protected set; }
        public string AssetPath => $"{_createFolderExecutor.FullPath}/{_assetName}";

        public BaseCreateAssetExecutor(string assetName, params string[] folders) : base()
        {
            _assetName = assetName ?? throw new ArgumentNullException(nameof(assetName));

            AddDependency(_createFolderExecutor = new CreateFolderExecutor(folders));
        }

        protected override void DoAfter(object? input)
        {
            Debug.Log($"Deleting asset: {AssetPath}");
            AssetDatabase.DeleteAsset(AssetPath);
            AssetDatabase.Refresh();
            base.DoAfter();
        }
    }
}