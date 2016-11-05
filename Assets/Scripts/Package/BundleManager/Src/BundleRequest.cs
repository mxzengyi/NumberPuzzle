/*************************************************************************************
    * 命名空间：       public
    * 创建时间：       2014/9/11
    * 作    者：       SparkSun
    * 说    明：       Bundle请求器，真正加载Bundle的地方，DownloadManager只管理BundleRequest
 *                      BundleRequest负责将资源包加载进来，并且保存了Bundle的信息，数据信息、
 *                      状态信息等。
    * 修改时间：
    * 修 改 人：
*************************************************************************************/

using System;
using System.IO;
using UnityEngine;

public class BundleRequest
{
    public string url = "";
    public int triedTimes = 0;
    public int priority = 0;

    private bool _isDependency = false;

    public BundleData bundleData = null;
    public BundleBuildState bundleBuildState = null;

    public string Url
    {
        get { return url; }
        set
        {
            url = value;
            
            bundleData = BundleConfigManager.BundleDataDict[
            CommonTools.GetPathFileName(url)];

            bundleBuildState = BundleConfigManager.BundleStatesDict[CommonTools.GetPathFileName(url)];
        }
    }

public bool IsDone
    {
        get
        {
            return (CreateRequest != null &&
                   CreateRequest.isDone) ||
                   AssetBundle != null;
        }
    }

    //只在创建此资源包的时候调用
    public bool IsDependency
    {
        get { return _isDependency; }
        set
        {
            _isDependency = value;

            if (_isDependency)
            {
                DependenManager.AddLoadingDependent(this);
            }
        }
    }

    public AssetBundle AssetBundle = null;
    public WWW CreateRequest;

    public void CreateAssetBundle()
    {
        triedTimes++;

        string wwwPath = "file:///" + Url;

        CreateRequest = new WWW(wwwPath);
    }

    public AssetBundle CreateAssetBundleSync()
    {
        triedTimes++;

        AssetBundle = AssetBundle.LoadFromFile(Url);

        if (AssetBundle != null)
        {
            LoadComplete();
        }
        else
        {
            Log.E(ELogTag.ResourceSys,"Don't has assetbunel : " + Url);
        }

        return AssetBundle;
    }

    public void LoadComplete()
    {
        //如果异步加载，则会产生CreateRequest，如果同步加载，则已直接为成型AssetBundle，不用再处理
        if (CreateRequest != null && CreateRequest.isDone )
        {
            if (!string.IsNullOrEmpty(CreateRequest.error))
            {
                Log.E(ELogTag.ResourceSys , CreateRequest.url + CreateRequest.error);
                return;
            }

            try
            {
                AssetBundle = CreateRequest.assetBundle;
            }
            catch (Exception)
            {
                Log.F(ELogTag.ResourceSys ,  Url +  " load done , but assetBundle is null");
                return;
            }
        }

        //如果这是一个依赖包
        if (AssetBundle != null && IsDependency)
        {
            DependenManager.OnDenpendentLoadComplete(this);
        }
        
    }

    public void Dispose()
    {
        if (CreateRequest != null)
        {
            CreateRequest.Dispose();
        }
    }
}