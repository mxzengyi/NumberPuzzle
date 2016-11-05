using System.Security.Permissions;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;

public class BMDataAccessor
{
	static public List<BundleData> Bundles
	{
		get
		{
            //if(m_Bundles == null)
            //    m_Bundles = LoadObjectFromJsonFile< List<BundleData> >(BundleDataPath);
			
			if(m_Bundles == null)
				m_Bundles = new List<BundleData>();
			
			return m_Bundles;
		}
	}
	
	static public List<BundleBuildState> BuildStates
	{
		get
		{
			if(m_BuildStates == null)
				m_BuildStates = LoadObjectFromJsonFile< List<BundleBuildState> >(BundleBuildStatePath);
			
			if(m_BuildStates == null)
				m_BuildStates = new List<BundleBuildState>();
			
			return m_BuildStates;
		}
	}
	
	static public BMConfiger BMConfiger
	{
		get
		{
			if(m_BMConfier == null)
				m_BMConfier = LoadObjectFromJsonFile<BMConfiger>(BMConfigerPath);
			
			if(m_BMConfier == null)
				m_BMConfier = new BMConfiger();
			
			return m_BMConfier;
		}
	}
	
	static public BMUrls Urls
	{
		get
		{
			if(m_Urls == null)
				m_Urls = LoadObjectFromJsonFile<BMUrls>(UrlDataPath);
			
			if(m_Urls == null)
				m_Urls = new BMUrls();
			
			return m_Urls;
		}
	}
	
	static public void SaveBMConfiger()
	{
		SaveObjectToJsonFile(BMConfiger, BMConfigerPath);
	}
	
	static public void SaveBundleData()
	{
		SaveObjectToJsonFile(Bundles, BundleDataPath);
	}
	
	static public void SaveBundleBuildeStates()
	{
		SaveObjectToJsonFile(BuildStates, BundleBuildStatePath);
	}
	
	static public void SaveUrls()
	{
		SaveObjectToJsonFile(Urls, UrlDataPath);
	}
	
	static public T LoadObjectFromJsonFile<T>(string path)
	{
	    if (!File.Exists(path))
	    {
           // pDebug.LogError("Cannot find " + path);
            return default(T);
	    }

		TextReader reader = new StreamReader(path);
		
		T data = JsonMapper.ToObject<T>(reader.ReadToEnd());
		if(data == null)
		{
			UnityEngine.Debug.LogError("Cannot read data from " + path);
		}
		
		reader.Close();
		return data;
	}
	
	static public void SaveObjectToJsonFile<T>(T data, string path)
	{
		TextWriter tw = new StreamWriter(path);
		if(tw == null)
		{
            UnityEngine.Debug.LogError("Cannot write to " + path);
			return;
		}
		
		string jsonStr = JsonFormatter.PrettyPrint( JsonMapper.ToJson(data) );
		
		tw.Write(jsonStr);
		tw.Flush();
		tw.Close();
		
		AssetDatabase.ImportAsset(path);
	}
	
	static private List<BundleData> m_Bundles = null;
	static private List<BundleBuildState> m_BuildStates = null;
	static private BMConfiger m_BMConfier = null;
	static private BMUrls m_Urls = null;

    public const string BundleDataPath = "Assets/Scripts/Plugin/BundleManager/BundleData.json";
    public const string BMConfigerPath = "Assets/Scripts/Plugin/BundleManager/BMConfiger.json";
    public const string BundleBuildStatePath = "Assets/Scripts/Plugin/BundleManager/BuildStates.json";
    public const string UrlDataPath = "Assets/Scripts/Plugin/BundleManager/Resources/Urls.json";
}
