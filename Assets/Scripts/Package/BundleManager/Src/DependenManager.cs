/*************************************************************************************
    * 命名空间：       public
    * 文 件 名：       $safeitemname$
    * 创建时间：       $time$
    * 作    者：       SparkSun
    * 说   明：
    * 修改时间：
    * 修 改 人：
*************************************************************************************/

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;

public static class DependenManager
{
    //为防止不同的包中有重名资源，所以必须使用dictionary
    private static Dictionary<BundleRequest , List<string>> LoadedAsset = new Dictionary<BundleRequest, List<string>>();
    
    //依赖包为key，value为正在等待此依赖包的asset
    private static Dictionary<BundleRequest , List<string>> CurrentLoadingDependent =
            new Dictionary<BundleRequest, List<string>>();

    public static bool IsAssetLoaded(BundleRequest request , string assetName)
    {
        if (!LoadedAsset.ContainsKey(request))
            return false;

        return LoadedAsset[request].Contains(assetName);
    }

    //将一个将要进行加载的依赖包加入等待加载队列之中
    public static void AddLoadingDependent(BundleRequest bundleRequest)
    {
        if (CurrentLoadingDependent.ContainsKey(bundleRequest))
        {
            Log.E(ELogTag.ResourceSys , bundleRequest.Url + "Already in waitting");
            return;
        }

        CurrentLoadingDependent.Add(bundleRequest , new List<string>());
    }

    //当需要一个依赖包，但是此依赖包还未加载成功，可能未加载，也可能正在加载之中时调用
    //当一个依赖包正在加载之时，此时如果请求此依赖包，则进行依赖登记,加需要之后加载的资源名登记在list中
    public static void AddWattingThisDependent(BundleData bundle, BundleRequest waitedRequest)
    {
        if (!CurrentLoadingDependent.ContainsKey(waitedRequest))
        {
            Log.E(ELogTag.ResourceSys, waitedRequest.Url + "Is not loading!");
            return;
        }

        //直接将依赖的资源提取出来进行登记
        List<string> regDependList = CurrentLoadingDependent[waitedRequest];
        List<string> dependIncludeList = waitedRequest.bundleData.includs;

        if (bundle == null || bundle.dependAssets == null)
        {
            Log.E(ELogTag.ResourceSys,"Bundle dependAssets is null");
        }

        List<string> bundleDependList = bundle.dependAssets;

        int bundleDependCount = bundleDependList.Count;

        for (int i = 0; i < bundleDependCount; i++)
        {
            string fileName = CommonTools.GetPathFileName(bundleDependList[i]);

            if (!regDependList.Contains(fileName) &&
                dependIncludeList.Contains(bundleDependList[i]))
                regDependList.Add(fileName);
        }
    }

    //当一个依赖包加载完成，遍历所有正在等待他的包，将等待包中所需的资源load出来
    public static void OnDenpendentLoadComplete(BundleRequest depRequest)
    {
        if (!CurrentLoadingDependent.ContainsKey(depRequest))
        {
            Log.E(ELogTag.ResourceSys, "Don't contains loading depend " + depRequest.Url);
            return;
        }

        List<string> waittingAsset = CurrentLoadingDependent[depRequest];
        int waittingCount = waittingAsset.Count;

        for (int i = 0; i < waittingCount; ++i)
        {
            Object loadedAsset = depRequest.AssetBundle.LoadAsset(waittingAsset[i]);

            Debug.Log(depRequest.bundleData.name + " load asset : " + loadedAsset.name);
        }

        //从正在loading的dependcy中移除
        CurrentLoadingDependent.Remove(depRequest);

        //加入到已经加载的request之中
        if (LoadedAsset.ContainsKey(depRequest))
        {
            Log.F(ELogTag.ResourceSys , "Loaded request already loaded . " + depRequest.Url);
            return;
        }

        LoadedAsset.Add(depRequest , new List<string>());
    }

    //当需要一个依赖包，并且此依赖包已经加载成功时调用
    //将一个bundle中，自身所需的依赖资源加载出来，并且登记在LoadedAsset之中
    public static void LoadDependent(BundleData bundleData, BundleRequest loadedDepRequest)
    {
        if (!LoadedAsset.ContainsKey(loadedDepRequest))
        {
            Log.F(ELogTag.ResourceSys , loadedDepRequest.Url + " dep request didn't loaded!!");
            return;
        }

        List<string> loadedList = LoadedAsset[loadedDepRequest];

        List<string> dependList = bundleData.dependAssets;

        int dependCount = dependList.Count;

        for (int i = 0; i < dependCount; i++)
        {
            //提取资源名称
            string fileName = System.IO.Path.GetFileName(dependList[i]);

            //確定在depend中是否有此資源
            if (!loadedDepRequest.bundleData.includs.Contains(fileName))
                continue;

            //去除后缀
            fileName = fileName.Substring(0, fileName.LastIndexOf('.'));

            //如果已经加载，则直接跳过
            if (loadedList.Contains(fileName))
            {
                continue;
            }

            //开始加载
            Object loadedAsset = loadedDepRequest.AssetBundle.LoadAsset(fileName);

            if (loadedAsset != null)
            {
                Debug.Log(loadedDepRequest.bundleData.name + " load asset : " + loadedAsset.name + ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");

                loadedList.Add(fileName);
            }
            else
            {
                //竟然无此依赖资源，或者出现同名资源
                Log.F(ELogTag.ResourceSys, "Can't load dependcy asset , " + fileName);
                continue;
            }
        }
    }

}
