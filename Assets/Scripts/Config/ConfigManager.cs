/********************************************************************
    created:	2014/09/05
    created:	5:9:2014   15:23
    file base:	ConfigManager.cs
    author:		Adam
    
    purpose:	This file is used to implement the framework 
                of local configuration management.
*********************************************************************/


using System;
using System.Collections;
using System.Collections.Generic;

using ScoreCfg;
using System.Threading;
using UnityEngine;





public class ConfigManager
{
    private static ConfigManager _instance = null;
    private static object _lockHelper = new object();

    public static ConfigManager GetInstance()
    {
        if (_instance == null)
        {
            lock (_lockHelper)
            {
                if (_instance == null)
                {
                    _instance = new ConfigManager();
                }
            }
        }

        return _instance;
    }

    public T GetConfig<T>() where T : Config
    {
        System.Type type = typeof(T);

        BaseConfigManager baseConfig = MainConfigManager.GetInstance();
        Config config = baseConfig.GetConfig<T>();
        if (config != null)
        {
            return config as T;
        }

        return null;
    }
}

public class MainConfigManager : BaseConfigManager
{
    private static MainConfigManager _instance = null;
    private static object _lockHelper = new object();

    private MainConfigManager() : base()
    {
        //新游戏配置开始处
        _configMap.Add(typeof(ScoreConfig), ScoreConfig.GetInst().Initialize("ScoreCfg"));
    }

    public override string GetName()
    {
        return "MainConfigManager";
    }

    public static MainConfigManager GetInstance()
    {
        if (_instance == null)
        {
            lock (_lockHelper)
            {
                if (_instance == null)
                {
                    _instance = new MainConfigManager();
                }
            }
        }

        return _instance;
    }
}

public abstract class BaseConfigManager
{
    protected Hashtable _configMap;
    protected int _loadedConfigCount;

    private bool _threadStop;
    private Thread _parseThread;
    private Queue _parseQueue;

    protected BaseConfigManager()
    {
        _loadedConfigCount = 0;
        _configMap = new Hashtable();

        _parseQueue = Queue.Synchronized(new Queue());
    }

    public IEnumerator Initialize()
    {
        _threadStop = false;

        _parseThread = new Thread(new ThreadStart(ParseLoop));
        _parseThread.Start();

        IDictionaryEnumerator configEnumerator = _configMap.GetEnumerator();
        while (configEnumerator.MoveNext())
        {
            Config config = configEnumerator.Value as Config;
            if (config == null)
            {
                Log.E(ELogTag.Config, "can't convert config data struct.");
                break;
            }

            config.SetLoadListener(ConfigLoadMonitor);
            config.StartLoad();

            //yield return new WaitForEndOfFrame();
        }

        yield return null;
    }

    public void InitializeEditor()
    {
        IDictionaryEnumerator configEnumerator = _configMap.GetEnumerator();
        while (configEnumerator.MoveNext())
        {
            Config config = configEnumerator.Value as Config;
            if (config == null)
            {
                Log.E(ELogTag.Config, "can't convert config data struct.");
                break;
            }

            config.SetLoadListener(ConfigLoadMonitorEditor);
            config.StartLoad();
        }
    }

    private void ConfigLoadMonitorEditor(Config config)
    {
        config.StartParse();

        _loadedConfigCount++;
    }

    private void ParseLoop()
    {
        Log.D(ELogTag.Config, "parse thread started");

        while (!_threadStop)
        {
            if (_threadStop)
            {
                break;
            }

            if (_parseQueue.Count <= 0 && _threadStop)
            {
                lock (_parseQueue.SyncRoot)
                {
                    Monitor.Wait(_parseQueue.SyncRoot);
                }
            }

            if (_parseQueue.Count <= 0)
            {
                continue;
            }

            Config config = _parseQueue.Dequeue() as Config;
            config.StartParse();

            _loadedConfigCount++;

            if (_loadedConfigCount >= _configMap.Count)
            {
                break;
            }
        }

        Log.D(ELogTag.Config, "parse thread stop");
    }

    public abstract string GetName();

    public bool IgnoreCheckReady()
    {
        return false;
    }

    public virtual bool IsReady()
    {
        return _loadedConfigCount >= _configMap.Count;
    }

    private void ConfigLoadMonitor(Config config)
    {
        _parseQueue.Enqueue(config);
        lock (_parseQueue.SyncRoot)
        {
            Monitor.PulseAll(_parseQueue.SyncRoot);
        }
    }

    public void Destroy()
    {
        _configMap.Clear();

        if (_parseThread != null && _parseThread.IsAlive)
        {
            _threadStop = true;
            lock (_parseQueue.SyncRoot)
            {
                Monitor.PulseAll(_parseQueue.SyncRoot);
            }

            _parseThread.Join();
        }
        _parseThread = null;

    }

    public T GetConfig<T>() where T : Config
    {
        System.Type type = typeof(T);
        if (_configMap.ContainsKey(type))
        {
            return _configMap[type] as T;
        }

        return null;
    }
}

public abstract class Config
{
    protected bool mLoadFinished;
    protected Action<Config> mLoadingListener;

    public void SetLoadListener(Action<Config> listener)
    {
        mLoadingListener = listener;
    }

    public virtual void StartLoad() {}
    public virtual void StartParse() {}
    public virtual void Destroy() {}
}

public abstract class ConfigBase<T> : Config where T : IResLoader, new()
{
    protected byte[] mDataBuffer;
    protected string mConfigName;

    public abstract List<T> GetConfigs();
    public abstract int GetConfigCount();

    protected abstract void AddConfig(T config);

    protected virtual void OnLoadFinished()
    {
        //AssetManager.Instance.DisposeAsset(mConfigName, eAssetType.Config);

        mLoadFinished = true;
    }

    public ConfigBase<T> Initialize(string configRes)
    {
        mConfigName = configRes;

        return this;
    }

    public override void StartLoad()
    {
        //int timeStart = TimeHelper.CurrentTimeMeasured();
        AssetManager.Instance.LoadAsset(mConfigName, eAssetType.Config, AssetLoaded);

        //int deltaTime = TimeHelper.TimeEscaped(timeStart);
        //Log.D(ELogTag.Config, "asset {0} load cost {1}", mConfigName, deltaTime);
    }

    private void AssetLoaded(IAsset asset)
    {
        do
        {
            //Log.D(ELogTag.Config, "asset {0} loaded", mConfigName);

            if (asset == null)
            {
                Log.E(ELogTag.Config, "Can't read config data from {0}", mConfigName);
                break;
            }

            TextAsset textAsset = asset.Value as TextAsset;
            if (textAsset == null)
            {
                Log.E(ELogTag.Config, "Can't read config data from {0}", mConfigName);
                break;
            }

            mDataBuffer = textAsset.bytes;
        } while (false);

        if (mLoadingListener != null)
        {
            mLoadingListener.Invoke(this);
        }
    }

    public override void StartParse()
    {
        do
        {
            //Log.D(ELogTag.Config, "asset {0} start parse", mConfigName);

            byte[] data = mDataBuffer;
            if (data == null)
            {
                Log.E(ELogTag.Config, "Can't get config data from {0}", mConfigName);
                break;
            }

            //int timeStart = TimeHelper.CurrentTimeMeasured();

            ResLoaderHelper<T> configReadHelper = new ResLoaderHelper<T>();

            int ret = configReadHelper.LoadFromBin(ref data);
            if (ret != 0)
            {
                Log.E(ELogTag.Config, String.Format("Parse config bin({0}) failed({1})!", mConfigName, ret));
                break;
            }

            T[] configs = configReadHelper.mConfigs;
            if (configs == null)
            {
                Log.E(ELogTag.Config, String.Format("Config({0}) don't exist...", mConfigName));
                break;
            }

            Log.D(ELogTag.Config, String.Format("Loaded {0} config entry from {1}.", configs.Length, mConfigName));

            foreach (T config in configs)
            {
                AddConfig(config);
            }

            //int deltaTime = TimeHelper.TimeEscaped(timeStart);
            //Log.D(ELogTag.Config, "asset {0} parse cost {1}", mConfigName, deltaTime);
        } while (false);

        OnLoadFinished();
    }

}

public abstract class ConfigList<T> : ConfigBase<T> where T : IResLoader, new()
{
    protected List<T> _configList = new List<T>();

    public override List<T> GetConfigs()
    {
        List<T> configList = new List<T>();
        configList.AddRange(_configList);

        return configList;
    }

    public override int GetConfigCount()
    {
        return _configList.Count;
    }

    protected override void AddConfig(T config)
    {
        _configList.Add(config);
    }

    public override void Destroy()
    {
        _configList.Clear();
    }
}

public abstract class ConfigMap<T> : ConfigBase<T> where T : class, IResLoader, new()
{
    private int count = 0;
    protected Hashtable _configMap = new Hashtable();
    private List<T> configList = new List<T>();

    protected void AddConfig(uint id, T config)
    {
        if (_configMap.ContainsKey(id)) {
            Log.E(ELogTag.Config, String.Format("duplicate id {0} in {1}", id, mConfigName));
        } else {
            _configMap.Add(id, config);
            configList.Add(config);
            count++;
        }
    }

    public T GetConfig(uint id)
    {
        return (T) _configMap[id];
    }

    public override List<T> GetConfigs()
    {
        if (configList.Count != count)
        {
            configList.Clear();
            IDictionaryEnumerator configEnumerator = _configMap.GetEnumerator();
            while (configEnumerator.MoveNext())
            {
                configList.Add((T)configEnumerator.Value);
            }
        }
        return configList;
    }

    public override int GetConfigCount()
    {
        return _configMap.Count;
    }

    public override void Destroy()
    {
        _configMap.Clear();
    }
}




public class ScoreConfig : ConfigMap<TScoreCfg>
{

    public static ScoreConfig GetInst()
    {
        return SingletonObject<ScoreConfig>.GetInstance();
    }

    protected override void AddConfig(TScoreCfg config)
    {
        AddConfig((uint)config.Sum, config);
    }
}

