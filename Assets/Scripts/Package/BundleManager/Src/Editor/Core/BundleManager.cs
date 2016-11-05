using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class BundleManager
{
	/**
	 * Get the BundleData by bundle's name. Method will return null  if there's no such bundle.
	 */
	static public BundleData GetBundleData(string name)
	{
		if(getInstance().bundleDict.ContainsKey(name))
			return getInstance().bundleDict[name];
		else
			return null;
	}
	
	/**
	 * Get the build state by bundle's name. Method will return null if there's no such bundle.
	 */
	static public BundleBuildState GetBuildStateOfBundle(string name)
	{
		if(getInstance().statesDict.ContainsKey(name))
			return getInstance().statesDict[name];
		else
			return null;
	}
	
	/**
	 * Return array of all bundles.
	 */
	static public BundleData[] bundles
	{
		get{return BMDataAccessor.Bundles.ToArray();}
	}	
	
	/**
	 * Return array of all bundle build states.
	 */
	static public BundleBuildState[] buildStates
	{
		get{return BMDataAccessor.BuildStates.ToArray();}
	}
	
	/**
	 * Return the list of all root bundles.
	 */
	static public List<BundleData> Roots
	{
		get
		{
			var bundleList = BMDataAccessor.Bundles;
			int childBundleStartIndex = bundleList.FindIndex(x => x.parent != "");
			List<BundleData> parents = null;
			if(childBundleStartIndex != -1)
				parents = bundleList.GetRange(0, childBundleStartIndex);
			else
				parents = bundleList.GetRange(0, bundleList.Count);
			
			parents.Sort((x,y)=>x.name.CompareTo(y.name));
			return parents;
		}
	}
	
	/**
	 * Detect if the the set parent operation is valid.
	 */
	static public bool CanBundleParentTo(string child, string newParent)
	{
		if(child == newParent)
			return false;
		
		if(newParent == "")
			return true;
		
		var bundleDict = getInstance().bundleDict;
		if( !bundleDict.ContainsKey(child) )
			return false;
		
		if( newParent != "" && !bundleDict.ContainsKey(newParent))
			return false;
		
		string tarParent = bundleDict[newParent].parent;
		while(bundleDict.ContainsKey(tarParent))
		{
			if(tarParent == child)
				// Parent's tree cannot contains this child
				return false;
			
			tarParent = bundleDict[tarParent].parent;
		}
		
		return true;
	}
	
	/**
	 * Set the parent of the bundle.
	 * @param parent New parent bundle's name. Set the parent to empty if you want the childe bundle become a root bundle.
	 */
	static public void SetParent(string childe, string parent)
	{
		if(!CanBundleParentTo(childe, parent))
			return;

		var bundleDict = getInstance().bundleDict;
		if(!bundleDict.ContainsKey(childe) || (parent != "" && !bundleDict.ContainsKey(parent)))
			return;
		
		BundleData childeBundle = bundleDict[childe];
		
		if(bundleDict.ContainsKey(childeBundle.parent))
			bundleDict[childeBundle.parent].children.Remove(childe);
				
		string origParent = childeBundle.parent;
		childeBundle.parent = parent;
		
		if(parent != "")
		{
			BundleData newParentBundle = bundleDict[parent];
			newParentBundle.children.Add(childe);
			newParentBundle.children.Sort();
		}
		
		if(parent == "" || origParent == "")
		{
			BMDataAccessor.Bundles.Remove(childeBundle);
			InsertBundleToBundleList(childeBundle);
		}
		
		UpdateBundleChangeTime(childeBundle.name);
		BMDataAccessor.SaveBundleData();
	}

	/**
	 * Create a new bundle.
	 * @param name Name of the bundle name.
	 * @param parent New parent's name. Set the parent to empty string if you want create a new root bundle.
	 * @param sceneBundle Is the bundle a scene bundle?  
	 */
	static public bool CreateNewBundle(string name, string parent, bool sceneBundle)
	{
		var bundleDict = getInstance().bundleDict;

		if(bundleDict.ContainsKey(name))
			return false;
		
		BundleData newBundle = new BundleData();
		newBundle.name = name;
		newBundle.sceneBundle = sceneBundle;
		
		if(parent != "")
		{
			if(!bundleDict.ContainsKey(parent))
				return false;
			else
				bundleDict[parent].children.Add(name);
			
			newBundle.parent = parent;
		}
	
		bundleDict.Add(name, newBundle);
		InsertBundleToBundleList(newBundle);
		
		BundleBuildState newBuildState = new BundleBuildState();
		newBuildState.bundleName = name;
		getInstance().statesDict.Add(name, newBuildState);
		BMDataAccessor.BuildStates.Add(newBuildState);
		
		UpdateBundleChangeTime(newBundle.name);
		
		BMDataAccessor.SaveBundleData();
		BMDataAccessor.SaveBundleBuildeStates();
		return true;
	}

    //remove all bundle
    static public void RemoveAllBundles()
    {
        List<string> allBundleName = new List<string>();

        foreach (var bundleName in getInstance().bundleDict.Keys)
        {
            allBundleName.Add(bundleName);
        }

        foreach (var bundleName in allBundleName)
        {
            var bundleDict = getInstance().bundleDict;
            var bundlelist = BMDataAccessor.Bundles;
            var dependRefDict = getInstance().dependRefDict;
            var includeRefDict = getInstance().includeRefDict;

            if (!bundleDict.ContainsKey(bundleName))
                continue;

            BundleData bundle = bundleDict[bundleName];
            bundlelist.Remove(bundle);
            bundleDict.Remove(bundleName);

            var buildStatesDict = getInstance().statesDict;
            BMDataAccessor.BuildStates.Remove(buildStatesDict[bundleName]);
            buildStatesDict.Remove(bundleName);

            // Remove parent ref
            if (bundle.parent != "" && bundleDict.ContainsKey(bundle.parent))
            {
                bundleDict[bundle.parent].children.Remove(bundleName);
            }

            // Remove include ref
            foreach (string assetPath in bundle.includs)
            {
                includeRefDict[assetPath].Remove(bundle);
            }

            // Remove depend asssets ref
            foreach (string assetPath in bundle.dependAssets)
            {
                dependRefDict[assetPath].Remove(bundle);
            }

            // Delete children recursively
            foreach (string childName in bundle.children)
            {
                RemoveBundle(childName);
            }
        }

        BMDataAccessor.SaveBundleData();
        BMDataAccessor.SaveBundleBuildeStates();
    }
	
	/**
	 * Remove the bundle by the given name.
	 * @Return Return false if no such bundle.
	 */
	static public bool RemoveBundle(string name)
	{
		var bundleDict = getInstance().bundleDict;
		var bundlelist = BMDataAccessor.Bundles;
		var dependRefDict = getInstance().dependRefDict;
		var includeRefDict = getInstance().includeRefDict;
		
		if(!bundleDict.ContainsKey(name))
			return false;
		
		BundleData bundle = bundleDict[name];
		bundlelist.Remove(bundle);
		bundleDict.Remove(name);
		
		var buildStatesDict = getInstance().statesDict;
		BMDataAccessor.BuildStates.Remove(buildStatesDict[name]);
		buildStatesDict.Remove(name);
		
		// Remove parent ref
		if(bundle.parent != "" && bundleDict.ContainsKey(bundle.parent))
		{
			bundleDict[bundle.parent].children.Remove(name);
		}
		
		// Remove include ref
		foreach(string assetPath in bundle.includs)
		{
			includeRefDict[assetPath].Remove(bundle);
		}
		
		// Remove depend asssets ref
		foreach(string assetPath in bundle.dependAssets)
		{
			dependRefDict[assetPath].Remove(bundle);
		}
		
		// Delete children recursively
		foreach(string childName in bundle.children)
		{
			RemoveBundle(childName);
		}
		
		BMDataAccessor.SaveBundleData();
		BMDataAccessor.SaveBundleBuildeStates();
		return true;
	}
	
	/**
	 * Get the root of the give bundle.
	 */
	public static string GetRootOf(string bundleName)
	{
		BundleData root = GetBundleData(bundleName);
		while(root.parent != "")
		{
			root = GetBundleData(root.parent);
			if(root == null)
			{
				UnityEngine.Debug.LogError("Cannnot find root of [" + bundleName + "]. Sth wrong with the bundle config data.");
				return "";
			}
		}
		
		return root.name;
	}
	
	/**
	 * Rename the bundle.
	 * @Return Return false if there's no such bundle, or the new name is used.
	 */
	static public bool RenameBundle(string origName, string newName)
	{
		if(newName == "" || origName == newName || getInstance().bundleDict.ContainsKey(newName) || !getInstance().bundleDict.ContainsKey(origName))
			return false;
		
		BundleData bundle = getInstance().bundleDict[origName];
		bundle.name = newName;
		
		Dictionary<string, BundleData> bundleDict = getInstance().bundleDict;
		bundleDict.Remove(origName);
		bundleDict.Add(newName, bundle);
		
		if(bundle.parent != "")
		{
			BundleData parentBundle = bundleDict[bundle.parent];
			parentBundle.children.Remove(origName);
			parentBundle.children.Add(newName);
		}
		
		foreach(string childName in bundle.children)
			getInstance().bundleDict[childName].parent = newName;
		
		var buildStatesDic = getInstance().statesDict;
		BundleBuildState buildState = buildStatesDic[origName];
		buildState.bundleName = newName;
		buildStatesDic.Remove(origName);
		buildStatesDic.Add(newName, buildState);
		
		BMDataAccessor.SaveBundleData();
		BMDataAccessor.SaveBundleBuildeStates();
		return true;
	}
	
	/**
	 * Test if path can be added to bundle.
	 * @param path The path must be a path under Asset. Can be path of diretory or file.
	 * @param bundleName The bundle's name.
	 */
	public static bool CanAddPathToBundle(string path, string bundleName)
	{
		BundleData bundle = GetBundleData(bundleName);
		if(bundle == null || Path.IsPathRooted(path) || (!File.Exists(path) && !Directory.Exists(path)))
		{
			return false;
		}
		
		if(bundle.includs.Contains(path))
			return false;
		
		if(ContainsFileInPath(path, sceneDetector) && !bundle.sceneBundle)
			return false;
		else if(ContainsFileInPath(path, assetDetector) && bundle.sceneBundle)
			return false;
		else
			return true;
	}
	
	/**
	 * Add a path to bundle's include list.
	 * @param path The path must be a path under Asset. Can be path of diretory or file.
	 * @param bundleName The bundle's name.
	 */
	public static void AddPathToBundle(string path, string bundleName)
	{
		BundleData bundle = GetBundleData(bundleName);

		if(IsNameDuplicatedAsset(bundle, path))
            UnityEngine.Debug.LogWarning("Name of new add asset will be duplicate with asset in bundle " + bundleName + ". This may cause problem when you trying to load them.");

		bundle.includs.Add(path);
		
		AddIncludeRef(path, bundle);
		RefreshBundleDependencies(bundle);
		UpdateBundleChangeTime(bundle.name);
		
		BMDataAccessor.SaveBundleData();
	}
	
	/**
	 * Remove path from bundle's include list.
	 */
	public static void RemovePathFromBundle(string path, string bundleName)
	{
		BundleData bundle = GetBundleData(bundleName);
		bundle.includs.Remove(path);
		
		getInstance().includeRefDict[path].Remove(bundle);
		RefreshBundleDependencies(bundle);
		UpdateBundleChangeTime(bundle.name);
		
		BMDataAccessor.SaveBundleData();
	}
	
	/**
	 * Test if the bundle is depend on another.
	 */
	public static bool IsBundleDependOn(string bundleName, string dependence)
	{
		BundleData bundle = GetBundleData(bundleName);
		if(bundle != null && bundleName == dependence)
			return true;
		
		while(bundle != null && bundle.parent != "")
		{
			if(bundle.parent == dependence)
				return true;
			
			bundle = GetBundleData(bundle.parent);
		}
		
		return false;
	}
	
	internal static void UpdateAllBundleChangeTime()
	{
		foreach(BundleData bundle in bundles)
			UpdateBundleChangeTime(bundle.name);
		
		BMDataAccessor.SaveBundleBuildeStates();
	}
	
	internal static List<BundleData> GetIncludeBundles(string assetPath)
	{
		var assetDict = getInstance().includeRefDict;
		if(!assetDict.ContainsKey(assetPath))
			return null;
		else
			return assetDict[assetPath];
	}
	
	internal static List<BundleData> GetRelatedBundles(string assetPath)
	{
		var assetDict = getInstance().dependRefDict;
		if(!assetDict.ContainsKey(assetPath))
			return null;
		else
			return assetDict[assetPath];
	}
	
	public static void RefreshBundleDependencies(BundleData bundle)
	{
		// Get all the includes files path
		string[] files = BuildHelper.GetAssetsFromPaths(bundle.includs.ToArray(), bundle.sceneBundle);
		string[] dependAssetNames = AssetDatabase.GetDependencies(files);
	    
		List<string> oldMetaList = bundle.dependAssets;
		bundle.dependAssets = new List<string>(dependAssetNames);
		
		// Remove the old ones
		foreach(string asset in oldMetaList)
		{
			if(getInstance().dependRefDict.ContainsKey(asset))
				getInstance().dependRefDict[asset].Remove(bundle);
		}
		
		// Add new asset connection
		foreach(string asset in bundle.dependAssets)
		{
            //这个depend有哪些asset在用，都会记录下来
			if(!getInstance().dependRefDict.ContainsKey(asset))
			{
				List<BundleData> sharedBundleList = new List<BundleData>();
				sharedBundleList.Add(bundle);
				getInstance().dependRefDict.Add(asset, sharedBundleList);
			}
			else
			{
                //直接在此资源的依赖上增加上此资源
				getInstance().dependRefDict[asset].Add(bundle);
			}
		}
	}
	
	public static void AddIncludeRef(string asset, BundleData bundle)
	{
		if(!getInstance().includeRefDict.ContainsKey(asset))
		{
			List<BundleData> sharedBundleList = new List<BundleData>();
			sharedBundleList.Add(bundle);
			getInstance().includeRefDict.Add(asset, sharedBundleList);
		}
		else if(!getInstance().includeRefDict[asset].Contains(bundle))
		{
			getInstance().includeRefDict[asset].Add(bundle);
		}
	}
	
	private static bool assetDetector(string filePath)
	{
		return !filePath.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase) && 
			!filePath.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase);
	}
	
	private static bool sceneDetector(string filePath)
	{
		return filePath.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase);
	}
	
	private delegate bool FileTypeDetector(string filePath);
	private static bool ContainsFileInPath(string path, FileTypeDetector fileDetector)
	{
		bool isDir = (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
		if(!isDir)
		{
			return fileDetector(path);
		}
		else
		{
			foreach(string subPath in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
			{
				if(fileDetector(subPath))
					return true;
			}
			
			return false;
		}
	}
	
	public static void UpdateBundleChangeTime(string bundleName)
	{
		GetBuildStateOfBundle(bundleName).changeTime = DateTime.Now.ToBinary();
	}
	
	private static void InsertBundleToBundleList(BundleData bundle)
	{
		List<BundleData> bundleList = BMDataAccessor.Bundles;
		if(bundleList.Contains(bundle))
			return;
		
		if(bundle.parent == "")
		{
            // 没有parent的插在前面
			int childBundleStartIndex = bundleList.FindIndex(x => x.parent != "");
			childBundleStartIndex = childBundleStartIndex == -1 ? bundleList.Count : childBundleStartIndex;
			bundleList.Insert(childBundleStartIndex, bundle);
		}
		else
			bundleList.Add(bundle);
	}

	private static bool IsNameDuplicatedAsset(BundleData bundle, string newAsset)
	{
		string newName = Path.GetFileNameWithoutExtension(newAsset);
		foreach(string asset in bundle.includs)
		{
			string file = Path.GetFileNameWithoutExtension(asset);
			if(file == newName)
				return true;
		}

		return false;
	}

	private void Init()
	{
		foreach(BundleBuildState buildState in BMDataAccessor.BuildStates)
		{
			if(!statesDict.ContainsKey(buildState.bundleName))
				statesDict.Add(buildState.bundleName, buildState);
			else
                UnityEngine.Debug.LogError("Bundle manger -- Cannot have two build states with same name [" + buildState.bundleName + "]");
		}
		
		foreach(BundleData bundle in BMDataAccessor.Bundles)
		{
			if(!bundleDict.ContainsKey(bundle.name))
				bundleDict.Add(bundle.name, bundle);
			else
                UnityEngine.Debug.LogError("Bundle manger -- Cannot have two bundle with same name [" + bundle.name + "]");
			
			if(!statesDict.ContainsKey(bundle.name))
				statesDict.Add(bundle.name, new BundleBuildState()); // Don't have build state of the this bundle. Add a new one.
			
			foreach(string asset in bundle.includs)
				AddIncludeRef(asset, bundle);
			
			RefreshBundleDependencies(bundle);
		}
	}
	
	static private BundleManager instance = null;
	static private BundleManager getInstance()
	{
		if(instance != null)
			return instance;
		
		instance = new BundleManager();
		instance.Init();
		return instance;
	}
	
	private Dictionary<string, BundleData> bundleDict = new Dictionary<string, BundleData>();
	private Dictionary<string, BundleBuildState> statesDict = new Dictionary<string, BundleBuildState>();
	private Dictionary<string, List<BundleData>> dependRefDict = new Dictionary<string, List<BundleData>>();// key: asset path, value: bundles depend this asset
	private Dictionary<string, List<BundleData>> includeRefDict = new Dictionary<string, List<BundleData>>();// key: asset path, value: bundles include this asset
}
