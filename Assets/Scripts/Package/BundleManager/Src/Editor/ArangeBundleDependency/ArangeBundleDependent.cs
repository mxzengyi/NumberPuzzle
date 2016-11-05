/*************************************************************************************
    * 命名空间：       public
    * 文 件 名：       $safeitemname$
    * 创建时间：       $time$
    * 作    者：       SparkSun
    * 说   明：
    * 修改时间：
    * 修 改 人：
*************************************************************************************/

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class ArangeBundleDependent
{
    private static List<BundleData> _orignalBundleDatas = new List<BundleData>();

    //不用将所有依赖资源转成资源包，需要用string，然后用普通包中的dependAsset String来查询依赖次数
    private static Dictionary<string, int> _dependAssetNum = new Dictionary<string, int>();

    //依赖树的最顶端，每个子bundle都可以在_dependBundle中找到
    static BundleData commonBundleData = null;
    private static Dictionary<string, BundleData> _treedBundleDatas = new Dictionary<string, BundleData>();

    private static string _currentCategre;

    static void CreateAllBundleData(Object[] assetObjects)
    {
        //用于去除重复的List
        List<string> assetNameList = new List<string>();

        foreach ( var assetObject in assetObjects )
        {
            string dataName = AssetDatabase.GetAssetPath(assetObject);

            if (assetNameList.Contains(dataName))
                continue;
            else
                assetNameList.Add(dataName);

            BundleData newBundleData = new BundleData();
            newBundleData.name = assetObject.name;
            newBundleData.includs.Add( dataName );

            //get denpendency
            string[] files = BuildHelper.GetAssetsFromPaths(newBundleData.includs.ToArray(), newBundleData.sceneBundle);
            string[] dependAssetNames = AssetDatabase.GetDependencies(files);

            newBundleData.dependAssets = new List<string>(dependAssetNames);

            _orignalBundleDatas.Add(newBundleData);
        }
    }

    static void GetDependentNum()
    {
        foreach (var bundleData in _orignalBundleDatas)
        {
            foreach (var depend in bundleData.dependAssets)
            {
                if (!depend.EndsWith(".cs"))
                {
                    if (_dependAssetNum.ContainsKey(depend))
                    {
                        _dependAssetNum[depend]++;
                    }
                    else
                    {
                        _dependAssetNum.Add(depend, 1);
                    }
                }
            }
        }
    }

    static int CreateTopCommonBundle()
    {
        //找到最大依赖，创建顶层common包
        var maxNum = _dependAssetNum.Values.Max();

        if (maxNum == 1)
        {
            // 所有的AssetBundle没有公共依赖，加入一个占位文件
            commonBundleData = new BundleData();
            commonBundleData.name = _currentCategre + "Common";

            commonBundleData.includs.Add("Assets/CommonEmpty.txt");
        }
        else
        {
            var maxAsset = _dependAssetNum.Where(pair => maxNum.Equals(pair.Value)).Select(pair => pair.Key);

            commonBundleData = new BundleData();
            commonBundleData.name = _currentCategre + "Common";
            foreach (var asset in maxAsset)
            {
                commonBundleData.includs.Add(asset);
            }
        }

        return maxNum;
    }

    static void MakeNumBundleTree()
    {
        //sort bundle num
        _dependAssetNum = (from entry in _dependAssetNum
                            orderby entry.Value descending
                            select entry).ToDictionary(pair => pair.Key, pair => pair.Value);

        //找出被最多依赖的包，作为顶层包
        int lastNum = CreateTopCommonBundle();
        BundleData lastBundleData = commonBundleData;

        _treedBundleDatas.Add(lastBundleData.name , lastBundleData);

        foreach ( KeyValuePair< string , int > dependInfo in _dependAssetNum )
        {
            if ( dependInfo.Value == 1 )
                continue;

            string dependAsset = dependInfo.Key;
            int bundleRefNum = dependInfo.Value;

            if (bundleRefNum == lastNum &&
                !lastBundleData.includs.Contains(dependAsset) &&
                lastNum != 1)
            {
                //将包整合进已有的包之中
                lastBundleData.includs.Add(dependAsset);
            }
            else if (bundleRefNum < lastNum)
            {
                //创建新的依赖整合包,不走多线依赖，走单线依赖，创建一个新的依赖包
                BundleData newBundleData = new BundleData();
                newBundleData.name = _currentCategre + "Dep" + bundleRefNum;
                newBundleData.includs.Add(dependAsset);

                newBundleData.parent = lastBundleData.name;
                lastBundleData.children.Add( newBundleData.name );

                lastBundleData = newBundleData;
                lastNum = bundleRefNum;

                if (!_treedBundleDatas.ContainsKey(newBundleData.name))
                    _treedBundleDatas.Add(newBundleData.name, newBundleData);
                else
                {
                    UnityEngine.Debug.LogError(newBundleData.name + " already in bundle tree , 同类资源重名！ ");
                }

            }
        }

//        string logtext = "";
//        int treeDepth = 0;
//
//        TraceBundleInfo(commonBundleData , treeDepth, ref logtext);
//
//        UnityEngine.Debug.Log(logtext);
    }

    static void TraceBundleInfo(BundleData bundleData, int treeDepth, ref string log)
    {
        return;
        string empty = "";

        for (int i = 0; i < treeDepth; i++)
        {
            empty += "   ";
        }

        log = log + empty + bundleData.name + " include : ";

        foreach (string include in bundleData.includs)
        {
            log = log + include + "  ";
        }

        log = log + "\n";

        treeDepth += 1;

        int childCount = bundleData.children.Count;

        if (childCount != 0)
        {
            for (int i = childCount - 1; i >= 0 ; i--)
            {
                TraceBundleInfo(_treedBundleDatas[bundleData.children[i]], treeDepth, ref log);
            }
        }
    }

    static void InsertBundleToNumTree()
    {
        foreach ( var bundleData in _orignalBundleDatas )
        {
            //找到最小依赖资源
            string minDependAsset = FindMinDependAsset(bundleData);

            //无依赖，是个独立包
            if ( string.IsNullOrEmpty(minDependAsset) )
            {                
                bundleData.parent = commonBundleData.name;
                commonBundleData.children.Add(bundleData.name);
            }
            else
            {
                //找到最小依赖包
                BundleData minDependBundle = FindMinDependBundle(minDependAsset);

                //UnityEngine.Debug.Log(bundleData.name + " find min Asset " + minDependAsset + " find min Bundle " + minDependBundle.name);

                //挂接到最小依赖包之下
                bundleData.parent = minDependBundle.name;
                minDependBundle.children.Add(bundleData.name);
            }

            //加入到树bundle中
            if (!_treedBundleDatas.ContainsKey(bundleData.name))
                _treedBundleDatas.Add(bundleData.name, bundleData);
            else
            { 
                UnityEngine.Debug.LogError( bundleData.name + " already in bundle tree , " + _currentCategre + " 同类资源重名！ ");
            }
        }
    }

    //找到最小依赖资源
    static string FindMinDependAsset(BundleData bundleData)
    {
        string minAsset = "";
        int minAssetNum = Int32.MaxValue;

        foreach (var dependAsset in bundleData.dependAssets)
        {
            if (dependAsset.EndsWith(".cs"))
                continue;

            if (_dependAssetNum[dependAsset] < minAssetNum &&
                _dependAssetNum[dependAsset] != 1)
            {
                minAssetNum = _dependAssetNum[dependAsset];
                minAsset = dependAsset;
            }
        }

        return minAsset;
    }
    
    //找到最小依赖包
    static BundleData FindMinDependBundle(string minAsset)
    {
        foreach (var treedBundleData in _treedBundleDatas)
        {
            if (treedBundleData.Value.includs.Contains(minAsset))
                return treedBundleData.Value;
        }

        return null;
    }

    public static void ArangeDependent(string assetFolder, string categre)
    {
        ArangeDependent(new List<string>() { assetFolder }, categre);
    }

    public static void ArangeDependent(List<string> assetFolder, string categre)
    {
        _orignalBundleDatas.Clear();
        _dependAssetNum.Clear();
        commonBundleData = null;
        _treedBundleDatas.Clear();

        _currentCategre = categre;

        //collect all asset
        List<Object> assetObjects = new List<Object>();

        foreach (var folder in assetFolder)
        {
            assetObjects.AddRange(Resources.LoadAll(folder));
        }

        // 找到每个资源的依赖
        CreateAllBundleData(assetObjects.ToArray());

        // 统计每个被依赖资源的被依赖次数
        GetDependentNum();

        // 创建公共AssetBundle，按照被依赖的次数，形成单个链条
        MakeNumBundleTree();

        // 将我们的资源AssetBundle插入到公共AssetBundle中
        InsertBundleToNumTree();

        CreateBundle(commonBundleData,"");
    }

    static void CreateBundle(BundleData bundleData , string parent )
    {
        BundleManager.CreateNewBundle(bundleData.name, bundleData.parent, false);

        BundleData addedBundleData = BundleManager.GetBundleData(bundleData.name);

        addedBundleData.includs.AddRange(bundleData.includs);

        foreach (var asset in bundleData.includs)
        {
            BundleManager.AddIncludeRef(asset , addedBundleData);
            BundleManager.RefreshBundleDependencies(addedBundleData);
            BundleManager.UpdateBundleChangeTime(addedBundleData.name);
        }

        foreach (var child in bundleData.children)
        {
            CreateBundle(_treedBundleDatas[child] , bundleData.name);
        }
    }
}
