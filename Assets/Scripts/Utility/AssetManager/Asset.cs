/********************************************************************
    created:	2014/12/16
    created:	16:12:2014   20:24
    file base:	SceneSwitch
    file ext:	cs
    author:		Sparksun
    
    purpose:	资源类
*********************************************************************/

using System;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

[Serializable]
public class Asset : IAsset
{
    //member
    private Object _loadedAsset = null;
    private Type _valueType = null;

    //indexer
    public eAssetType AssetType { get; set; }
    public string AssetName { get; set; }

    private string _bundlePathKey;

    public string BundlePathKey
    {
        get { return _bundlePathKey; }
        set { _bundlePathKey = value; }
    }

    public bool IsDone { get; set; }

    public bool IsStay { get; set; }

    //记录所有从自身Instantiate的资源
    private List<Object> _initList; 

    public Object Value
    {
        get { return _loadedAsset; }
        set
        {
            _loadedAsset = value;
            if (OnAssetLoaded != null)
            {
                OnAssetLoaded(this);
                OnAssetLoaded = null;
            }
        }
    }

    public Type ValueType
    {
        get
        {
            if (_valueType == null)
                JudgeValueType();
            return _valueType;
        }
        set { _valueType = value; }
    }

    public event AssetLoaded OnAssetLoaded;

    //function
    
    //无参构造函数
    public Asset()
    {
        Value = null;
        IsDone = false;
    }

    //有参构造函数
    public Asset(string assetName, eAssetType assetType)
    {
        Value = null;
        IsDone = false;
        AssetName = assetName;
        AssetType = assetType;
    }

    //包含bundlePath的构造函数
    public Asset(string assetName, eAssetType assetType, string bundlePath)
    {
        Value = null;
        IsDone = false;
        AssetName = assetName;
        AssetType = assetType;
        BundlePathKey = bundlePath;
    }

    //附带完成回调的构造函数
    public Asset(string assetName, eAssetType assetType, string bundlePath, AssetLoaded callBack)
    {
        Value = null;
        IsDone = false;
        AssetName = assetName;
        AssetType = assetType;
        BundlePathKey = bundlePath;
        OnAssetLoaded += callBack;
    }

    //确定Value的类型
    void JudgeValueType()
    {
        switch (AssetType)
        {
            case eAssetType.BigTex:
            case eAssetType.TextureTavern:
            case eAssetType.TextureBigItem:
            case eAssetType.HeadIcons:
            case eAssetType.Newbie:
                ValueType = typeof (Texture2D);
                break;
            case eAssetType.Config:
                ValueType = typeof (TextAsset);
                break;
            case eAssetType.Atlas:
            case eAssetType.UIPrefab:
            case eAssetType.Character_Enemy:
            case eAssetType.Character_Hero:
            case eAssetType.Equip:
            case eAssetType.Level:
            case eAssetType.FightSkill:
            case eAssetType.AiConfig:
            case eAssetType.ChildView:
                ValueType = typeof(GameObject);
                break;
            case eAssetType.Audio:
                ValueType = typeof(UnityEngine.AudioClip);
                break;
            default:
                Log.E(ELogTag.ResourceSys, "Asset JudegeValueType : Unknown AssetType " + AssetType.ToString());
                break;
        }
    }

    public Object Instantiate()
    {
        if (_initList == null)
            _initList = new List<Object>();

        Object assetClone = Object.Instantiate(Value);
        _initList.Add(assetClone);

        return assetClone;
    }

    public void Dispose()
    {
        //dispose
        if (_initList != null)
        {
            foreach (var o in _initList)
            {
                Object.Destroy(o);
            }
        }

        AssetManager.Instance.DisposeAsset(this);

//#if !ASSETBUNDLE

        if (Value != null &&
            !(Value is AudioClip) &&
            !(Value is GameObject))
        {
            try
            {
                Log.D(ELogTag.ResourceSys,"Dispose asset value " + Value.name);
                Resources.UnloadAsset(Value);
            }
            catch (Exception)
            {
                Debug.LogError("Value type can't Unload : " + Value.GetType());
                throw;
            }
        }
            
//#endif

        Value = null;
        IsDone = false;
    }
}
