/*************************************************************************************
    * 命名空间：       public
    * 创建时间：       2014/9/11
    * 作    者：       SparkSun
    * 说    明：       AssetBundleManager，资源包下载管理器，所有的资源包下载，都会经过
 *                      资源包管理器进行下载。其中自动处理了资源包的依赖关系，如果发现
 *                      依赖包没有下载，则会首先载入依赖包。
    * 修改时间：
    * 修 改 人：
*************************************************************************************/

using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Uri = System.Uri;

public class AssetBundleManager : MonoBehaviour
{
    //BundleManager配置，从文件读入
    public BMConfiger BmConfiger = null;

    readonly Dictionary<string, BundleRequest> _processingRequest = new Dictionary<string, BundleRequest>();
    readonly Dictionary<string, BundleRequest> _succeedRequest = new Dictionary<string, BundleRequest>();
    readonly Dictionary<string, BundleRequest> _failedRequest = new Dictionary<string, BundleRequest>();
    readonly List<BundleRequest> _waitingRequests = new List<BundleRequest>();
    readonly List<BundleRequest> _requestedBeforeInit = new List<BundleRequest>();

    static AssetBundleManager _instance = null;
    static string _manualUrl = "";

    [HideInInspector]
    public bool IsInited = false;

	/**
	 * Get the error string of WWW request.
	 * @return The error string of WWW. Return null if WWW request succeed or still in processing.
	 */ 
	public string GetError(string url)
	{
		if(!ConfigLoaded)
			return null;
		
		url = formatUrl(url);
		if(_failedRequest.ContainsKey(url))
			return url + " create fail !";
		else
			return null;
	}
	
	/**
	 * Test if the url is already requested.
	 */
	public bool IsUrlRequested(string url)
	{
		if(!ConfigLoaded)
		{
			return isInBeforeInitList(url);
		}
		else
		{
			url = formatUrl(url);
			bool isRequested = isInWaitingList(url) || _processingRequest.ContainsKey(url) || _succeedRequest.ContainsKey(url) || _failedRequest.ContainsKey(url);
			return isRequested;
		}
	}
	
	/**
	 * Get WWW instance of the url.
	 * @return Return null if the WWW request haven't succeed.
	 */ 
	public WWW GetCreateRequest(string url)
	{
	    BundleRequest request = GetRequest(url);

	    if (request != null)
	        return request.CreateRequest;

	    return null;
	}

    public BundleRequest GetRequest(string url)
    {
        if (!ConfigLoaded)
            return null;

        url = formatUrl(url);

        if (_succeedRequest.ContainsKey(url))
        {
            prepareDependBundles(url);
            return _succeedRequest[url];
        }
        else
            return null;
    }
	
	public IEnumerator WaitDownload(string url)
	{
		yield return StartCoroutine( WaitDownload(url, -1) );
	}

    public AssetBundle DownloadAssetBundleSync(string url)
    {
        url = formatUrl(url);

        if (_succeedRequest.ContainsKey(url))
        {
            return _succeedRequest[url].AssetBundle;
        }

        BundleRequest request = new BundleRequest();
        request.Url = url;

        downloadSync(request);

        return request.AssetBundle;
    }

	/**
	 * Coroutine for download waiting. 
	 * You should use it like this,
	 * yield return StartCoroutine(AssetBundleManager.Instance.WaitDownload("bundle1.assetbundle"));
	 * If this url didn't been requested, the coroutine will start a new request.
	 */ 
	public IEnumerator WaitDownload(string url, int priority)
	{
		while(!ConfigLoaded)
			yield return null;

	    url = formatUrl(url);

		BundleRequest request = new BundleRequest();
        request.Url = url;
	    request.priority = priority;

		download(request);

        while (isDownloadingWWW(request.Url))
			yield return null;
	}
	
//	public BundleRequest StartDownload(string url)
//	{
//		return StartDownload(url, true , -1);
//	}
//	
//	/**
//	 * Start a new download request.
//	 * @param url The url for download. Can be a absolute or relative url.
//	 * @param priority Priority for this request.
//	 */ 
//	public BundleRequest StartDownload(string url , bool sync , int priority)
//	{
//		BundleRequest request = new BundleRequest();
//		request.url = url;
//		request.priority = priority;
//
//		if(!ConfigLoaded)
//		{
//			if(!isInBeforeInitList(url))
//				_requestedBeforeInit.Add(request);
//		}
//        else
//        {
//            download(request);
//        }
//
//	    return request;
//	}
	
	/**
	 * Stop a request.
	 */ 
	public void StopDownload(string url)
	{
		if(!ConfigLoaded)
		{
			_requestedBeforeInit.RemoveAll(x => x.Url == url);
		}
		else
		{
			url = formatUrl(url);
			
			_waitingRequests.RemoveAll(x => x.Url == url);
			
            //停止正在处理
			if(_processingRequest.ContainsKey(url))
			{
				_processingRequest[url].Dispose();
				_processingRequest.Remove(url);
			}
		}
	}
	
	/**
	 * Dispose a finished WWW request.
	 */ 
	public void DisposeBundleRequest(string url)
	{
		url = formatUrl(url);
		StopDownload(url);
		
		if(_succeedRequest.ContainsKey(url))
		{
			_succeedRequest[url].Dispose();
			_succeedRequest.Remove(url);
		}
		
		if(_failedRequest.ContainsKey(url))
		{
			_failedRequest[url].Dispose();
			_failedRequest.Remove(url);
		}
	}
	
	/**
	 * This function will stop all request in processing.
	 */ 
	public void StopAll()
	{
		_requestedBeforeInit.Clear();
		_waitingRequests.Clear();
		
        //停止正在处理
        foreach (BundleRequest request in _processingRequest.Values)
        {
            if (request.AssetBundle != null)
            {
                request.Dispose();
            }
        }
		
        //清理处理
		_processingRequest.Clear();
	}

    public void DisposeAll()
    {
        StopAll();

        List<string> toDispose = new List<string>();

        foreach (KeyValuePair<string, BundleRequest> request in _succeedRequest)
        {
            toDispose.Add(request.Key);
        }

        foreach (string key in toDispose)
        {
            DisposeBundleRequest(key);
        }
    }
	
	/**
	 * Get download progress of bundles.
	 * All bundle dependencies will be counted too.
	 * This method can only used on self built bundles.
	 */ 
	public float ProgressOfBundles(string[] bundlefiles)
	{
		if(!ConfigLoaded)
			return 0f;
		
		List<string> bundles = new List<string>();
		foreach(string bundlefile in bundlefiles)
		{
			if(!bundlefile.EndsWith( "." + BmConfiger.BundleSuffix, System.StringComparison.OrdinalIgnoreCase))
			{
				Debug.LogWarning("ProgressOfBundles only accept bundle files. " + bundlefile + " is not a bundle file.");
				continue;
			}
			
			bundles.Add(Path.GetFileNameWithoutExtension(bundlefile));
		}
		
		HashSet<string> allInludeBundles = new HashSet<string>();
		foreach(string bundle in bundles)
		{
			foreach(string depend in getDependList(bundle))
			{
				if(!allInludeBundles.Contains(depend))
					allInludeBundles.Add(depend);
			}
			
			if(!allInludeBundles.Contains(bundle))
				allInludeBundles.Add(bundle);
		}
		
		long currentSize = 0;
		long totalSize = 0;
		foreach(string bundleName in allInludeBundles)
		{
            if (!BundleConfigManager.BundleStatesDict.ContainsKey(bundleName))
			{
				Debug.LogError("Cannot get progress of [" + bundleName + "]. It's not such bundle in bundle build states list.");		
				continue;
			}

            long bundleSize = BundleConfigManager.BundleStatesDict[bundleName].size;
			totalSize += bundleSize;
			
			string url = formatUrl( bundleName );

            //获取正在处理的bundle进度
			if(_processingRequest.ContainsKey(url))
				currentSize += (long)(_processingRequest[url].CreateRequest.progress * bundleSize);
			
			if(_succeedRequest.ContainsKey(url))
				currentSize += bundleSize;
		}
		
		if(totalSize == 0)
			return 0;
		else
			return ((float)currentSize)/totalSize;
	}

	/**
	 * Check if the config files downloading finished.
	 */
	public bool ConfigLoaded
	{
		get { return BundleConfigManager.Inited; }
	}

	/**
	 * Get list of the built bundles. 
	 * Before use this, please make sure ConfigLoaded is true.
	 */ 
	public BundleData[] BuiltBundles
	{
		get
		{
			if(BundleConfigManager.DatasSaved == null)
				return null;
			else
                return BundleConfigManager.DatasSaved.ToArray();
		}
	}

	/**
	 * Get list of the BuildStates. 
	 * Before use this, please make sure ConfigLoaded is true.
	 */ 
	public BundleBuildState[] BuildStates
	{
		get
		{
			if(BundleConfigManager.StatesSaved == null)
				return null;
			else
                return BundleConfigManager.StatesSaved.ToArray();
		}
	}

    public IEnumerator Initialize()
    {
        if (IsInited)
        {
            yield break;
        }

        //首先初始化ConfigManager，各种数据由configManager中得到
        yield return StartCoroutine(BundleConfigManager.Init());

        BmConfiger = BundleConfigManager.BmConfiger;

        // Start download for requests before init
        foreach (BundleRequest request in _requestedBeforeInit)
        {
            download(request);
        }

        IsInited = true;
    }

    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    readonly List<string> _finishList = new List<string>();
    readonly List<string> _failedsList = new List<string>(); 

	void Update()
	{
		if(!ConfigLoaded)
			return;
		
        _finishList.Clear();
        _failedsList.Clear();

        //处理正在处理的request,如果已经OK，则切换到newFinish队列
        //如果失败，再切换到newFail中
		foreach(BundleRequest request in _processingRequest.Values)
		{
            if (!request.IsDone)
                continue;

			if(request.CreateRequest == null)
			{
				if(request.triedTimes - 1 < BmConfiger.DownloadRetryTime)
				{
                    //use asset bundle
                    request.CreateAssetBundle();
				}
				else
				{
					_failedsList.Add( request.Url );
					Debug.LogError("Download " + request.Url + " failed for " + request.triedTimes + " times.");
				}
			}
			else if(request.CreateRequest.isDone)
			{
				_finishList.Add( request.Url );
			}
		}
		
		//将newFinish中的移动到successd队列之中
		foreach(string finishedUrl in _finishList)
		{
            BundleRequest finishRequest = _processingRequest[finishedUrl];

            _succeedRequest.Add(finishedUrl, finishRequest);
            
            _processingRequest.Remove(finishedUrl);

            finishRequest.LoadComplete();
		}
		
		// 将失败的request加入到失败队列中
		foreach(string finishedUrl in _failedsList)
		{
			if(!_failedRequest.ContainsKey(finishedUrl))
				_failedRequest.Add(finishedUrl, _processingRequest[finishedUrl]);
			_processingRequest.Remove(finishedUrl);
		}
		
		// Start download new bundles
		int waitingIndex = 0;

        //处理等待队列，等待队列之中，有dependRequest,也有真正需要使用的depend
		while( _processingRequest.Count < BmConfiger.DownloadThreadsCount && 
			   waitingIndex < _waitingRequests.Count)
		{
			BundleRequest curRequest = _waitingRequests[waitingIndex++];
			
			bool canStartDownload = curRequest.bundleData == null || isBundleDependenciesReady( curRequest.bundleData.name );

			if(canStartDownload)
			{
				_waitingRequests.Remove(curRequest);

                curRequest.CreateAssetBundle();

				_processingRequest.Add(curRequest.Url, curRequest);
			}
		}
	}
	
	bool isBundleDependenciesReady(string bundleName)
	{
		List<string> dependencies = getDependList(bundleName);
		foreach(string dependBundle in dependencies)
		{
			string url = formatUrl(dependBundle);
			if(!_succeedRequest.ContainsKey(url))
				return false;
		}
		
		return true;
	}

	void prepareDependBundles(string url)
	{
	    if (!url.EndsWith(".ph"))
	    {
	        Debug.LogError("Url is error : " + url);
	        return;
	    }

		string bundleName = Path.GetFileNameWithoutExtension(url);
		List<string> dependencies = getDependList(bundleName);
		foreach(string dependBundle in dependencies)
		{
			string dependUrl = formatUrl(dependBundle);
			if(_succeedRequest.ContainsKey(dependUrl))
			{
				#pragma warning disable 0168
				var assertBundle = _succeedRequest[dependUrl].AssetBundle;
				#pragma warning restore 0168
			}
		}
	}

    // This private method should be called after init
    void downloadSync(BundleRequest request)
    {
        if (isDownloadingWWW(request.Url) || _succeedRequest.ContainsKey(request.Url))
        {
            return;
        }

        if (isBundleUrl(request.Url))
        {
            string bundleName = Path.GetFileNameWithoutExtension(request.Url);
            if (!BundleConfigManager.BundleDataDict.ContainsKey(bundleName))
            {
                Debug.LogError("Cannot download bundle [" + bundleName + "]. It's not in the bundle config.");
                return;
            }

            List<string> dependlist = getDependList(bundleName);

            //遍历依赖包
            foreach (string bundle in dependlist)
            {
                string depBundleUrl = formatUrl(bundle);

                //如果依赖包还未加载，并且没有在处理，则进行加载,并且登记依赖
                if (!_processingRequest.ContainsKey(depBundleUrl) && !_succeedRequest.ContainsKey(depBundleUrl))
                {
                    BundleRequest dependRequest = new BundleRequest();

                    dependRequest.Url = depBundleUrl;
                    dependRequest.priority = dependRequest.bundleData.priority;

                    //处理依赖
                    dependRequest.IsDependency = true;
                    DependenManager.AddWattingThisDependent(request.bundleData, dependRequest);

                    downloadSync(dependRequest);

                    if (dependRequest.AssetBundle != null)
                    {
                        if (!_succeedRequest.ContainsKey(dependRequest.Url))
                        {
                            _succeedRequest.Add(dependRequest.Url, dependRequest);
                        }
                    }
                }
                //未加载完成，目前正在加载中
                else if (_processingRequest.ContainsKey(depBundleUrl))
                {
                    //进行依赖登记
                    DependenManager.AddWattingThisDependent(request.bundleData, _processingRequest[depBundleUrl]);
                }
                //已经加载完成
                else if (_succeedRequest.ContainsKey(depBundleUrl))
                {
                    //直接加载依赖
                    DependenManager.LoadDependent(request.bundleData, _succeedRequest[depBundleUrl]);
                }

            }

            if (request.priority == -1)
                request.priority = request.bundleData.priority;  // User didn't change the default priority

            request.CreateAssetBundleSync();

            if (request.AssetBundle != null)
            {
                if (!_succeedRequest.ContainsKey(request.Url))
                    _succeedRequest.Add(request.Url, request);
            }
        }
        else
        {
            if (request.priority == -1)
                request.priority = 0; // User didn't give the priority
            request.CreateAssetBundleSync();

            if (request.AssetBundle != null)
            {
                if (!_succeedRequest.ContainsKey(request.Url))
                    _succeedRequest.Add(request.Url, request);
            }
        }
    }

	// This private method should be called after init
	void download(BundleRequest request)
	{
		if (isDownloadingWWW(request.Url) || _succeedRequest.ContainsKey(request.Url))
			return;
		
		if (isBundleUrl(request.Url))
		{
			string bundleName = Path.GetFileNameWithoutExtension(request.Url);
            if (!BundleConfigManager.BundleDataDict.ContainsKey(bundleName))
			{
				Debug.LogError("Cannot download bundle [" + bundleName + "]. It's not in the bundle config.");
				return;
			}
			
			List<string> dependlist = getDependList(bundleName);

            //遍历依赖包
			foreach ( string bundle in dependlist )
			{
                string depBundleUrl = formatUrl(bundle);
				
                //如果依赖包还未加载，并且没有在处理，则进行加载
                if (!_processingRequest.ContainsKey(depBundleUrl) && !_succeedRequest.ContainsKey(depBundleUrl))
				{
					BundleRequest dependRequest = new BundleRequest();
                    dependRequest.Url = depBundleUrl;
					dependRequest.priority = dependRequest.bundleData.priority;

                    //处理依赖
                    dependRequest.IsDependency = true;
                    DependenManager.AddWattingThisDependent(request.bundleData, dependRequest);

					addRequestToWaitingList(dependRequest);
				}
                //未加载完成，目前正在加载中
                else if (_processingRequest.ContainsKey(depBundleUrl))
                {
                    //进行依赖登记
                    DependenManager.AddWattingThisDependent(request.bundleData, _processingRequest[depBundleUrl]);
                }
                //已经加载完成
                else if (_succeedRequest.ContainsKey(depBundleUrl))
                {
                    //直接加载依赖
                    DependenManager.LoadDependent(request.bundleData, _succeedRequest[depBundleUrl]);
                }
			}

			if(request.priority == -1)
				request.priority = request.bundleData.priority;  // User didn't change the default priority
			addRequestToWaitingList(request);
		}
		else
		{
			if(request.priority == -1)
				request.priority = 0; // User didn't give the priority
			addRequestToWaitingList(request);
		}
	}

	
	bool isInWaitingList(string url)
	{
		foreach(BundleRequest request in _waitingRequests)
			if(request.Url == url)
				return true;
		
		return false;
	}
	
	void addRequestToWaitingList(BundleRequest request)
	{
		if(_succeedRequest.ContainsKey(request.Url) || isInWaitingList(request.Url))
			return;
		
		int insertPos = _waitingRequests.FindIndex(x => x.priority < request.priority);
		insertPos = insertPos == -1 ? _waitingRequests.Count : insertPos;
		_waitingRequests.Insert(insertPos, request);
	}
	
	bool isDownloadingWWW(string url)
	{
		foreach(BundleRequest request in _waitingRequests)
			if(request.Url == url)
				return true;
		
        //判断是否正在处理
		return _processingRequest.ContainsKey(url);
	}
	
	bool isInBeforeInitList(string url)
	{
		foreach(BundleRequest request in _requestedBeforeInit)
		{
			if(request.Url == url)
				return true;
		}

		return false;
	}

	List<string> getDependList(string bundle)
	{
		if(!ConfigLoaded)
		{
			Debug.LogError("getDependList() should be call after download manager initiated.");
			return null;
		}
		
		List<string> res = new List<string>();

        if (!BundleConfigManager.BundleDataDict.ContainsKey(bundle))
		{
			Debug.LogError("Cannot find parent bundle [" + bundle + "], Please check your bundle config.");
			return res;
		}

        while (BundleConfigManager.BundleDataDict[bundle].parent != "")
		{
            bundle = BundleConfigManager.BundleDataDict[bundle].parent;
            if (BundleConfigManager.BundleDataDict.ContainsKey(bundle))
			{
				res.Add(bundle);
			}
			else
			{
				Debug.LogError("Cannot find parent bundle [" + bundle + "], Please check your bundle config.");
				break;
			}
		}
		
		res.Reverse();
		return res;
	}
	
	BuildPlatform getRuntimePlatform()
	{
		if(	Application.platform == RuntimePlatform.WindowsPlayer ||
			Application.platform == RuntimePlatform.OSXPlayer )
		{
			return BuildPlatform.Standalones;
		}
		else if(Application.platform == RuntimePlatform.OSXWebPlayer ||
				Application.platform == RuntimePlatform.WindowsWebPlayer)
		{
			return BuildPlatform.WebPlayer;
		}
		else if(Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return BuildPlatform.IOS;
		}
		else if(Application.platform == RuntimePlatform.Android)
		{
			return BuildPlatform.Android;
		}
		else
		{
			Debug.LogError("Platform " + Application.platform + " is not supported by BundleManager.");
			return BuildPlatform.Standalones;
		}
	}

    public static string DownloadRootUrl
    {
        get
        {
#if UNITY_EDITOR
        return  Application.persistentDataPath + "/AssetBundles/assetbundles/";
#elif UNITY_IPHONE
        return  Application.dataPath + "/Raw/AssetBundles/assetbundles/";
#elif UNITY_ANDROID
        return Application.persistentDataPath + "/AssetBundles/assetbundles/";
#elif UNITY_STANDALONE
            return  Application.persistentDataPath + "/AssetBundles/assetbundles/";
#endif
        }
    }
	
	string formatUrl(string urlstr)
	{
	    urlstr = urlstr.TrimEnd(new char[1] {'\0'});
	    if (urlstr.Contains(DownloadRootUrl))
	        return urlstr;
	    else
	    {
	        List<Char> chars = urlstr.ToList();

	        int stringEnd = chars.IndexOf('\0');

	        string endString = string.Empty;

	        if (chars.Count != 0 &&
                stringEnd != -1 &&
                Char.IsDigit(chars[stringEnd - 1]))
	        {
	            string surfix = ".ph";

	            chars.InsertRange(stringEnd , surfix.ToList());

	            endString = new string(chars.ToArray());
	        }
	        else
	        {
	            endString = urlstr + ".ph";
	        }

            return DownloadRootUrl + endString;
	    }

//		Uri url;
//		if(!isAbsoluteUrl(urlstr))
//		{
//			url = new Uri(new Uri(DownloadRootUrl + '/'), urlstr);
//		}
//		else
//			url = new Uri(urlstr);
//		
//		return url.AbsoluteUri;
	}
	
	bool isAbsoluteUrl(string url)
	{
	    Uri result;
	    return Uri.TryCreate(url, System.UriKind.Absolute, out result);
	}
	
	bool isBundleUrl(string url)
	{
	    return url.EndsWith(".ph");
	}
	
	/**
	 * Get instance of AssetBundleManager.
	 * This prop will create a GameObject named Downlaod Manager in scene when first time called.
	 */ 
	public static AssetBundleManager Instance
	{
		get
		{
			return _instance;
		}
	}

	public static void SetManualUrl(string url)
	{
		if(_instance != null)
		{
			Debug.LogError("Cannot use SetManualUrl after accessed AssetBundleManager.Instance. Make sure call SetManualUrl before access to AssetBundleManager.Instance.");
			return;
		}

		_manualUrl = url;
	}

}