/*************************************************************************************
    * 命名空间：       public
    * 创建时间：       2014/9/11
    * 作    者：       SparkSun
    * 说    明：       资源管理器，负责所有资源的加载，获取，释放，所有模块请求资源，
                       均会从此类调用。 
    * 修改时间：
    * 修 改 人：
*************************************************************************************/

//using forge.system;
//#pragma warning disable 0414
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;

#if LOCAL_EDITOR
using UnityEditor;
#endif

public class AssetManager : MonoBehaviour
{
    private Dictionary<string,Asset> _assetMap = new Dictionary<string, Asset>();

    //private static IFrameUpdateable iFrameUp;

    private static AssetManager _instance;

    //单件实现函数
    public static AssetManager Instance
    {
        get
        {
            if (_instance == null)
            {
                //Instance = new AssetManager();
                //修改成Mono对象
                GameObject go = new GameObject("AssetManager");
                DontDestroyOnLoad(go);
                //GameObject globalObject = GameObject.Find("/GlobalObject");
                //CommonTools.SetParent(go.transform, globalObject.transform);

                _instance = go.AddComponent<AssetManager>();
            }

            return _instance;
        }
    }

    //切换场景时清空资源和引用
    public void OnApplicationSwitchScene()
    {
        
    }

    #region 资源获取和加载

    //获取一个asset，不保证资源一定不为空，所以get后加上判断
    public Asset GetAsset(string assetName, eAssetType assetType)
    {
        string bundlePathKey = GetBundlePath(assetName, assetType);

        //check asset
        Asset tmpAsset = CheckAsset(bundlePathKey);

        //资源已经被加载，直接调用加载完成回调并且返回
        if (tmpAsset != null)
        {
            return tmpAsset;
        }

        return null;
    }

    //同步读取一个资源
    public Asset LoadAssetSync(string assetName, eAssetType assetType)
    {
        //check asset
        Asset tmpAsset = GetAsset(assetName, assetType);

        //资源已经被加载，直接调用加载完成回调并且返回
        if (tmpAsset != null)
        {
            if (!tmpAsset.IsDone)
            {
                Log.E(ELogTag.ResourceSys , "Sync load a aready Aync loading resource " + assetName);
                return null;
            }

            return tmpAsset;
        }

        string bundlePathKey = GetBundlePath(assetName, assetType);

        tmpAsset = new Asset(assetName, assetType, bundlePathKey);

        if (IsAssetInBundle(assetName))
        {
            LoadAssetFromAssetBundleSync(tmpAsset);
        }
        else
        {
            LoadAssetFromResourcesSync(tmpAsset);
        }

        if (!_assetMap.ContainsKey(bundlePathKey))
        {
            _assetMap.Add(bundlePathKey, tmpAsset);
        }

        return tmpAsset;
    }

    //加载一个Asset
    //assetName: 资源名称
    //assetType: 资源类型
    //loadedCallBack: 资源加载完成时回调函数
    public Asset LoadAsset(string assetName, eAssetType assetType, AssetLoaded loadedCallBack = null)
    {
        //check asset
        Asset tmpAsset = GetAsset(assetName, assetType);

        //资源已经被加载，直接调用加载完成回调并且返回
        if (tmpAsset != null)
        {
            if (tmpAsset.IsDone)
            {
                if (loadedCallBack != null)
                {
                    loadedCallBack(tmpAsset);
                }
            }
            else
            {
                if (loadedCallBack != null)
                {
                    tmpAsset.OnAssetLoaded += loadedCallBack;
                }
            }

            // 没有加载成功， 也返回Asset， 只不过IsDone是false
            return tmpAsset;
        }

        string bundlePathKey = GetBundlePath(assetName, assetType);

        //资源未被加载
        tmpAsset = new Asset(assetName, assetType, bundlePathKey, loadedCallBack);

        if (!_assetMap.ContainsKey(bundlePathKey))
        {
            _assetMap.Add(bundlePathKey, tmpAsset);
        }

        //加上协同异步方式

        int time = TimeHelper.CurrentTimeMeasured();
        LoadAsset(tmpAsset);
        //Log.W(ELogTag.NewbieGuide,"-----------------------------------load asset " + assetName + " time=" + (TimeHelper.CurrentTimeMeasured() - time));
        return tmpAsset;
    }

    private Asset CheckAsset(string bundlePathKey)
    {
        if (_assetMap.ContainsKey(bundlePathKey))
            return _assetMap[bundlePathKey];

        //判断是否具有bundle

        return null;
    }

    bool IsAssetInBundle(string assetName)
    {
#if QGameMode && !UNITY_EDITOR
        return false;
#endif
        BundleBuildState assetBundleState;

        //此处使用字符串对比很可能无法真正正确，所以使用linq对比
        //因为配置文件中的资源名可能是“xxxx\0\0\0\0”, 而某些请求的资源名是“xxxx\0”
        if (BundleConfigManager.BundleStatesDict.TryGetValue(assetName, out assetBundleState))
        {
            if (assetBundleState != null &&
                assetBundleState.version != -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    void LoadAsset(Asset loadAsset)
    {
        if (IsAssetInBundle(loadAsset.AssetName))
        {
            LoadAssetMobile(loadAsset);
        }
        else
        {
            LoadAssetPC(loadAsset);
        }
    }

    void LoadAssetMobile(Asset loadAsset)
    {
        switch (loadAsset.AssetType)
        {
            case eAssetType.Config:
            case eAssetType.UIPrefab:
            case eAssetType.AiConfig:
            case eAssetType.Audio:
            case eAssetType.Equip:
            case eAssetType.Level:
            case eAssetType.FightSkill:
            case eAssetType.Character_Hero:
            case eAssetType.Character_Enemy:
            case eAssetType.BigTex:
            case eAssetType.HeadIcons:
            case eAssetType.ChildView:
            case eAssetType.Newbie:
                LoadAssetFromAssetBundleSync(loadAsset);
                break;
            case eAssetType.Atlas:
            case eAssetType.TextureTavern:
            case eAssetType.TextureBigItem:
                {
                    StartCoroutine(LoadAssetFromAssetBundleAsync(loadAsset));
                }
                break;
            default:
                break;
        }
    }

    void LoadAssetPC(Asset loadAsset)
    {
        switch (loadAsset.AssetType)
        {
            case eAssetType.Config:
            case eAssetType.UIPrefab:
            case eAssetType.AiConfig:
            case eAssetType.Audio:
            case eAssetType.Equip:
            case eAssetType.Level:
            case eAssetType.FightSkill:
            case eAssetType.Character_Hero:
            case eAssetType.Character_Enemy:
            case eAssetType.BigTex:
                LoadAssetFromResourcesSync(loadAsset);
                break;
            case eAssetType.Atlas:
            
            case eAssetType.TextureTavern:
            case eAssetType.TextureBigItem:
                {
                    StartCoroutine(LoadAssetFromResourcesAsync(loadAsset));
                }
                break;
            default:
                break;
        }
    }

    //Resources同步读取，用于PC开发环境同步加载UI
    void LoadAssetFromResourcesSync(Asset loadAsset)
    {
        SetLoadAssetValue(loadAsset, Resources.Load(loadAsset.BundlePathKey));
    }

    //Resources异步读取，用于PC环境异步加载
    private IEnumerator LoadAssetFromResourcesAsync(Asset loadAsset)
    {
 //#if UNITY_4_5_3
         ResourceRequest loadRequest = Resources.LoadAsync(loadAsset.BundlePathKey);
 
         yield return loadRequest;
         
         SetLoadAssetValue(loadAsset, loadRequest.asset);
// #else
//        SetLoadAssetValue(loadAsset, Resources.Load(loadAsset.BundlePathKey));
//// #endif
        //yield break;
    }

    //AssetBundle同步读取，用于读取UI
    private void LoadAssetFromAssetBundleSync(Asset loadAsset)
    {
        AssetBundle loadedBundle = AssetBundleManager.Instance.DownloadAssetBundleSync(loadAsset.AssetName);

        if (loadedBundle != null)
        {
            SetLoadAssetValue(loadAsset, loadedBundle.LoadAsset(loadAsset.AssetName, loadAsset.ValueType));
        }
    }

    //AssetBundle异步读取
    private IEnumerator LoadAssetFromAssetBundleAsync(Asset loadAsset)
    {
        while (!AssetBundleManager.Instance.IsInited)
        {
            Log.D(ELogTag.Res, "Wait download manager to init");
            yield return new WaitForEndOfFrame();
        }

        yield return StartCoroutine(AssetBundleManager.Instance.WaitDownload(loadAsset.AssetName));

        BundleRequest assetRequest = AssetBundleManager.Instance.GetRequest(loadAsset.AssetName);

        if (assetRequest == null)
        {
            Log.F(ELogTag.ResourceSys, loadAsset.BundlePathKey + " load Fail , WWW is null");
            yield break;
        }

        SetLoadAssetValue(loadAsset, assetRequest.AssetBundle.LoadAsset(loadAsset.AssetName, loadAsset.ValueType));
    }

    void SetLoadAssetValue(Asset loadedAsset , Object value)
    {
        loadedAsset.Value = value;

        if (loadedAsset.Value != null)
        {
            loadedAsset.IsDone = true;

            if (!_assetMap.ContainsKey(loadedAsset.BundlePathKey))
            {
                _assetMap.Add(loadedAsset.BundlePathKey, loadedAsset);
            }
        }
        else
        {
            return;
            Log.E(ELogTag.ResourceSys, "资源加载失败，缺少资源: {0}, 类型 :{1}  请联系梆总（kantxu）跟进Check！", loadedAsset.AssetName, loadedAsset.AssetType.ToString());
        }
    }

    //获取一个资源bundle的路径
    private string GetBundlePath(string assetName, eAssetType assetType)
    {
        string bundlePath = string.Empty;

        switch (assetType)
        {
            case eAssetType.Config:
                {
                    bundlePath = "/Config/App/";
                    break;
                }
            case eAssetType.UIPrefab:
                {
                    bundlePath = "/UIView/";
                    break;
                }
            case eAssetType.Atlas:
                {
                    bundlePath = "/Atlas/";
                    break;
                }
            case eAssetType.Level:
                {
                    bundlePath = "/Levels/";
                }
                break;
            case eAssetType.Audio:
                {
                    bundlePath = "/Audio/";
                    break;
                }
            case eAssetType.Character_Hero:
                {
                    bundlePath = "/Characters/player/";
                    break;
                }
            case eAssetType.Character_Enemy:
                {
                    bundlePath = "/Characters/monster/";
                    break;
                }
            case eAssetType.Equip:
                {
                    bundlePath = "/Equip/";
                    break;
                }
            case eAssetType.FightSkill:
                {
                    bundlePath = "/FightSkills/";
                    break;
                }
            case eAssetType.BigTex:
                {
                    bundlePath = "/BigTex/";
                    break;
                }
            case eAssetType.TextureTavern:
                {
                    bundlePath = "/TavernTex/";
                    break;
                }
            case eAssetType.TextureBigItem:
                {
                    bundlePath = "/BigItemTex/";
                    break;
                }
            case eAssetType.AiConfig:
                {
                    bundlePath = "/AiConfig/";
                    break;
                }
            case eAssetType.HeadIcons:
                {
                    bundlePath = "/HeadIcons/";
                    break;
                }
            case eAssetType.ChildView:
                {
                    bundlePath = "/ChildView/";
                    break;
                }
            case eAssetType.Newbie:
                {
                    bundlePath = "/Newbie/";
                    break;
                }
            default:
                {
                    break;
                }
        }

        bundlePath = LoadPathPrefix + bundlePath + assetName;
        
        return bundlePath;
    }

    private string LoadPathPrefix
    {
        get
        {
                return "RTP";
        }
    }

    #endregion

    #region 复制和删除资源

    //复制资源
    public Object Instantiate(IAsset initAsset)
    {
        if (!initAsset.IsDone || initAsset.Value == null)
        {
            Log.E(ELogTag.ResourceSys, "AssetManager Instantiate Asset , initAsset value is null, asset : " + initAsset.AssetName);
            return null;
        }

        return initAsset.Instantiate();
    }

    //移除资源
    public void DisposeAsset(Asset removeAsset)
    {
        if (removeAsset == null)
        {
            Log.E(ELogTag.ResourceSys, "AssetManager Remove asset is null");
            return;
        }

        if (_assetMap.ContainsKey(removeAsset.BundlePathKey))
            _assetMap.Remove(removeAsset.BundlePathKey);
    }

    //移除资源,不接受管理了，即为释放掉
    public void DisposeAsset(string assetName, eAssetType assetType)
    {
        string bundlePathKey = GetBundlePath(assetName, assetType);

        if (_assetMap.ContainsKey(bundlePathKey))
            DisposeAsset(_assetMap[bundlePathKey]);
    }

    //销毁所有资源，一般在切换场景时使用
    public void DisposeAllAsset()
    {
        List<Asset> assetToDispose = new List<Asset>();

        foreach (KeyValuePair<string, Asset> asset in _assetMap)
        {
            //不需要在场景中保留的，则进行统计
            if (!asset.Value.IsStay)
                assetToDispose.Add(asset.Value);
        }

        int assetCount = assetToDispose.Count;

        for (int i = 0; i < assetCount; i++)
        {
            assetToDispose[i].Dispose();
        }

        //如果bundleManager已经初始化
        if (AssetBundleManager.Instance.IsInited)
        {
            AssetBundleManager.Instance.DisposeAll();
        }

        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    #endregion
}
