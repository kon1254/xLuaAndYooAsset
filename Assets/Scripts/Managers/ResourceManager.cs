using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public class ResourceManager : MonoSingleton<ResourceManager>
{
    private ResourcePackage _defaultPackage;
    public bool isInitialized = false;
    protected override void Awake()
    {
        base.Awake();

        StartCoroutine(InitYooAssets(() =>
        {
            isInitialized = true;
        }));
    }
    IEnumerator InitYooAssets(Action onDownloadComplete)
    {
        YooAssets.Initialize();

        string packageName = "xLuaAndYooAssets";
        var package = YooAssets.CreatePackage(packageName);
        YooAssets.SetDefaultPackage(package);
        if (AppConst.PlayMode == EPlayMode.EditorSimulateMode)
        {
            //编辑器下模拟模式
            var buildResult = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
            var packageRoot = buildResult.PackageRootDirectory;
            var editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            var initParameters = new EditorSimulateModeParameters();
            initParameters.EditorFileSystemParameters = editorFileSystemParams;
            yield return package.InitializeAsync(initParameters);
        }
        else if (AppConst.PlayMode == EPlayMode.HostPlayMode)
        {
            string defaultHostServer = AppConst.HOSTSERVERPATH + "/" + packageName;
            string fallbackHostServer = AppConst.HOSTSERVERPATH + "/" + packageName;
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            var cacheFileSystemParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
            var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();

            var initParameters = new HostPlayModeParameters();
            initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
            initParameters.CacheFileSystemParameters = cacheFileSystemParams;
            var initOperation = package.InitializeAsync(initParameters);
            yield return initOperation;

            if (initOperation.Status == EOperationStatus.Succeed)
            {
                Debug.Log("资源包 初始化成功");
            }
            else
            {
                Debug.LogError($"资源包 初始化失败 : {initOperation.Error} ");
            }
        }

        var operation = package.RequestPackageVersionAsync();
        yield return operation;

        if (operation.Status != EOperationStatus.Succeed)
        {
            Debug.LogError(operation.Error);
            yield break;
        }

        string packageVersion = operation.PackageVersion;

        Debug.Log($"资源包版本 : {packageVersion}");
        var operaiton2 = package.UpdatePackageManifestAsync(packageVersion);
        yield return operaiton2;

        if (operaiton2.Status != EOperationStatus.Succeed)
        {
            Debug.LogError(operaiton2.Error);
            yield break;
        }

        yield return Download();

        _defaultPackage = package;
        onDownloadComplete?.Invoke();
    }

    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    private class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }
        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }
        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }

    #region  下载相关
    IEnumerator Download()
    {
        int downloadingMaxNumber = 10;
        int failedTryAgain = 3;
        var package = YooAssets.GetPackage("xLuaAndYooAssets");
        var downloader = package.CreateResourceDownloader(downloadingMaxNumber, failedTryAgain);

        if (downloader.TotalDownloadCount == 0)
        {
            yield break;
        }

        int totalDownloadCount = downloader.TotalDownloadCount;
        long totalDownloadBytes = downloader.TotalDownloadBytes;

        downloader.DownloadFinishCallback = OnDownloadOverFunction;
        downloader.DownloadErrorCallback = OnDownloadErrorFunction;
        downloader.DownloadUpdateCallback = OnDownloadUpdateFunction;
        downloader.DownloadFileBeginCallback = OnDownloadFileBeginFunction;

        downloader.BeginDownload();
        yield return downloader;

        if (downloader.Status == EOperationStatus.Succeed)
        {
            Debug.Log($"下载完成，下载文件总数：{totalDownloadCount}，下载总大小：{totalDownloadBytes}字节");
        }
        else
        {
            Debug.LogError($"下载失败，错误信息：{downloader.Error}");
        }
    }

    private void OnDownloadErrorFunction(DownloadErrorData downloadErrorData)
    {
        Debug.Log(string.Format("下载出错：packageName: {0} 文件名：{1}，错误信息：{2}", downloadErrorData.PackageName, downloadErrorData.FileName, downloadErrorData.ErrorInfo));
    }

    private void OnDownloadOverFunction(DownloaderFinishData downloaderFinishData)
    {
        Debug.Log("下载" + (downloaderFinishData.Succeed ? "成功" : "失败"));
    }

    private void OnDownloadUpdateFunction(DownloadUpdateData downloadUpdate)
    {
        Debug.Log(string.Format("所属PackageName ：{0} 文件总数：{1}，已下载文件数：{2}，下载总大小：{3}，已下载大小{4}", downloadUpdate.PackageName, downloadUpdate.TotalDownloadCount, downloadUpdate.CurrentDownloadCount, downloadUpdate.TotalDownloadBytes, downloadUpdate.CurrentDownloadBytes));
    }

    private void OnDownloadFileBeginFunction(DownloadFileData downloadFileData)
    {
        Debug.Log(string.Format("开始下载：packageName: {0} 文件名：{1}，文件大小：{2}", downloadFileData.PackageName, downloadFileData.FileName, downloadFileData.FileSize));
    }

    #endregion

    #region 资源加载
    public void LoadAsset<T>(string assetName, Action<T> onLoadComplete) where T : UnityEngine.Object
    {
        if (_defaultPackage == null)
        {
            Debug.LogError("资源包未初始化");
            return;
        }

        var operation = _defaultPackage.LoadAssetAsync<T>(assetName);
        operation.Completed += (op) =>
        {
            if (op.Status == EOperationStatus.Succeed)
            {
                onLoadComplete?.Invoke(op.AssetObject as T);
            }
            else
            {
                Debug.LogError($"加载资源失败：{op.Status}");
            }
        };
    }
    #endregion

}