/*************************************************************************************
    * 命名空间：       public
    * 文 件 名：       $safeitemname$
    * 创建时间：       $time$
    * 作    者：       SparkSun
    * 说   明：        管理bundle config的对比，更新，合并，一共会有2套配置文件存储，
 *                      一套是合并报错的，另一套是当更新了IIPS版本后，从包中得到的需要刷新的配置
    * 修改时间：
    * 修 改 人：
*************************************************************************************/

using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEngine;
using System.Collections;

public static class BundleConfigManager
{
    public static string DataSavedConfig = "";

    public static string StateSavedConfig = "";

    public static string DataLastConfig = "";

    public static string StateLastConfig = "";

    public static List<BundleData> DatasSaved;
    public static List<BundleBuildState> StatesSaved;

    //bundle信息，从文件读入
    public static Dictionary<string, BundleData> BundleDataDict = new Dictionary<string, BundleData>();

    //每个bundle的状态，从文件读入
    public static Dictionary<string, BundleBuildState> BundleStatesDict = new Dictionary<string, BundleBuildState>();

    private const string _dataSavedConfigFileName = "SavedBundleData.json";

    private static readonly string _stateSavedConfigFileName = "SavedBundleState.json";


    //BundleManager配置，从文件读入
    public static BMConfiger BmConfiger = null;

    public static bool Inited = false;

    public static string SavedConfigPathPrefix
    {
        get
        {
            return Application.persistentDataPath + "/AssetBundles/assetbundles/";
        }
    }

    //初始化函数，加载N项配置文件
    public static IEnumerator Init()
    {
        if ( Inited )
            yield break;
        
        //读取本地存储的需要使用AssetBundle的配置文件，如果这个文件为空，那最好了
        //读取本地的bundleData文件
        string downloadPath = Path.Combine(SavedConfigPathPrefix, _dataSavedConfigFileName);

        downloadPath = "file:///" + downloadPath;

        WWW configWww = new WWW(downloadPath);
        yield return configWww;

        if (configWww.isDone &&
            string.IsNullOrEmpty(configWww.error))
            DataSavedConfig = configWww.text;

        //读取本地的bundleState
        downloadPath = Path.Combine(SavedConfigPathPrefix, _stateSavedConfigFileName);

        downloadPath = "file:///" + downloadPath;

        configWww = new WWW(downloadPath);
        yield return configWww;

        if (configWww.isDone &&
            string.IsNullOrEmpty(configWww.error))
            StateSavedConfig = configWww.text;

        yield return CoroutineManager.Instance.StartCoroutine(LoadUpdateBundleConfig());

        yield return CoroutineManager.Instance.StartCoroutine(LoadAndSaveBundleManagerConfig());

        CombineBundleDataAndState();

        Inited = true;
    }

    //读取刚刚更新下来的AssetBundle中的配置文件
    public static IEnumerator LoadUpdateBundleConfig()
    {
        string downloadPath = Path.Combine(AssetBundleManager.DownloadRootUrl, "BundleData.json");

        downloadPath = "file:///" + downloadPath;

        WWW configWww = new WWW(downloadPath);
        yield return configWww;

        if (configWww.isDone &&
            string.IsNullOrEmpty(configWww.error))
            DataLastConfig = configWww.text;

        //读取本地的bundleState
        downloadPath = Path.Combine(AssetBundleManager.DownloadRootUrl, "BuildStates.json");

        downloadPath = "file:///" + downloadPath;

        configWww = new WWW(downloadPath);
        yield return configWww;

        if (configWww.isDone &&
            string.IsNullOrEmpty(configWww.error))
            StateLastConfig = configWww.text;
    }

    public static IEnumerator LoadAndSaveBundleManagerConfig()
    {
        //读取本地的bundleState
        string downloadPath = Path.Combine(AssetBundleManager.DownloadRootUrl, "BMConfiger.json");

        downloadPath = "file:///" + downloadPath;

        WWW configWww = new WWW(downloadPath);
        yield return configWww;

        if (configWww.isDone &&
            string.IsNullOrEmpty(configWww.error))
        {
            string configText = configWww.text;

            BmConfiger = JsonMapper.ToObject<BMConfiger>(configText);
        }
        else
        {
            BmConfiger = new BMConfiger();
        }
    }

    //合并配置文件
    public static void CombineBundleDataAndState()
    {
        if ( string.IsNullOrEmpty(DataLastConfig) &&
             string.IsNullOrEmpty(StateLastConfig) )
            return;

        DatasSaved = JsonMapper.ToObject<List<BundleData>>(DataSavedConfig);
        if (DatasSaved==null) DatasSaved = new List<BundleData>();
        List<BundleData> lastBundleDatas = JsonMapper.ToObject<List<BundleData>>(DataLastConfig);

        //将saved之中没有的bundledata，但是在last之中存在，则合并到saved之中
        foreach ( var lastBundleData in lastBundleDatas )
        {
            if ( DatasSaved.Find( x=>x.name.Equals(lastBundleData.name)) == null )
            {
                DatasSaved.Add(lastBundleData);
            }
            else
            {
                //将last中较新的bundle信息，更新到savedBundle中
                DatasSaved.RemoveAll(x => x.name.Equals(lastBundleData.name));
                DatasSaved.Add(lastBundleData);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        StatesSaved = JsonMapper.ToObject<List<BundleBuildState>>(StateSavedConfig);
        if (StatesSaved==null) StatesSaved = new List<BundleBuildState>();
        List<BundleBuildState> lastBundleStates = JsonMapper.ToObject<List<BundleBuildState>>(StateLastConfig);

        //将saved之中没有的bundleState，但是在last之中存在，则合并到saved之中
        foreach (var lastBundleState in lastBundleStates)
        {
            if (StatesSaved.Find(x=>x.bundleName.Equals(lastBundleState.bundleName)) == null)
            {
                StatesSaved.Add(lastBundleState);
            }
            else
            {
                //将last中较新的bundle信息，更新到savedBundle中
                StatesSaved.RemoveAll(x => x.bundleName.Equals(lastBundleState.bundleName));
                StatesSaved.Add(lastBundleState);
            }
        }

        SaveCombinedConfigFile();

        //转存入map中，提供给全局查询
        foreach (var bundle in DatasSaved)
        {
            BundleDataDict.Add(bundle.name, bundle);
        }

        foreach (var buildState in StatesSaved)
        {
            BundleStatesDict.Add(buildState.bundleName, buildState);
        }
    }

    public static void SaveCombinedConfigFile()
    {
        string savePath = Path.Combine(Application.persistentDataPath , _dataSavedConfigFileName);
        CommonTools.SaveObjectToJsonFile<List<BundleData>>(DatasSaved, savePath);

        savePath = Path.Combine(Application.persistentDataPath, _stateSavedConfigFileName);
        CommonTools.SaveObjectToJsonFile(StatesSaved, savePath);
    }

}
