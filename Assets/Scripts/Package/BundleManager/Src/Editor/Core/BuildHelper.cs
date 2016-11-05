using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/**
 * Build helper contains APIs for bundle building.
 * You can use this to custom your own build progress.
 */ 
public class BuildHelper 
{
	/**
	 * Copy the bundle datas to target directory.
	 */ 
	public static void ExportBundleDataFileToOutput()
	{
		File.Copy( 	BMDataAccessor.BundleDataPath, 
					Path.Combine( BuildConfiger.InterpretedOutputPath, Path.GetFileName(BMDataAccessor.BundleDataPath) ), 
					true );
	}
	
	/**
	 * Copy the bundle build states to target directory.
	 */ 
	public static void ExportBundleBuildDataFileToOutput()
	{
		File.Copy( 	BMDataAccessor.BundleBuildStatePath, 
					Path.Combine( BuildConfiger.InterpretedOutputPath, Path.GetFileName(BMDataAccessor.BundleBuildStatePath) ), 
					true );
	}
	
	/**
	 * Copy the bundle manager configeration file to target directory.
	 */ 
	public static void ExportBMConfigerFileToOutput()
	{
		File.Copy( 	BMDataAccessor.BMConfigerPath, 
					Path.Combine( BuildConfiger.InterpretedOutputPath, Path.GetFileName(BMDataAccessor.BMConfigerPath) ), 
					true );
	}
	
	/**
	 * Detect if the bundle need update.
	 */ 
	public static bool IsBundleNeedBunild(BundleData bundle)
	{	
		string outputPath = GenerateOutputPathForBundle(bundle.name);
		if(!File.Exists(outputPath))
			return true;
		
		BundleBuildState bundleBuildState = BundleManager.GetBuildStateOfBundle(bundle.name);
		DateTime lastBuildTime = File.GetLastWriteTime(outputPath);
		DateTime bundleChangeTime = bundleBuildState.changeTime == -1 ? DateTime.MaxValue : DateTime.FromBinary(bundleBuildState.changeTime);
		if( System.DateTime.Compare(lastBuildTime, bundleChangeTime) < 0 )
		{
			return true;
		}
		
		string[] assetPathes = GetAssetsFromPaths(bundle.includs.ToArray(), bundle.sceneBundle);
		string[] dependencies = AssetDatabase.GetDependencies(assetPathes);
		if( !EqualStrArray(dependencies, bundleBuildState.lastBuildDependencies) )
			return true; // Build depenedencies list changed.
		
		foreach(string file in dependencies)
		{
			if(DateTime.Compare(lastBuildTime, File.GetLastWriteTime(file)) < 0)
				return true;
		}
		
		if(bundle.parent != "")
		{
			BundleData parentBundle = BundleManager.GetBundleData(bundle.parent);
			if(parentBundle != null)
			{
				if(IsBundleNeedBunild(parentBundle))
					return true;
			}
			else
			{
				UnityEngine.Debug.LogError("Cannot find bundle");
			}
		}
		
		return false;
	}
	
	/**
	 * Build all bundles.
	 */
	public static void BuildAll()
	{	
		BuildBundles(BundleManager.bundles.Select(bundle=>bundle.name).ToArray());
	}
	
	/**
	 * Force rebuild all bundles.
	 */
	public static void RebuildAll()
	{
		foreach(BundleBuildState bundle in BundleManager.buildStates)
			bundle.lastBuildDependencies = null;
		
		BuildAll();
	}
	
	/**
	 * Build bundles.
	 */
	public static void BuildBundles(string[] bundles)
	{
		Dictionary<string, List<string>> buildingRoutes = new Dictionary<string, List<string>>();
		foreach(string bundle in bundles)
			AddBundleToBuildList(bundle, ref buildingRoutes);
		
		foreach(var buildRoute in buildingRoutes)
		{
			BundleData bundle = BundleManager.GetBundleData( buildRoute.Key );
			if(bundle != null)
				BuildBundleTree(bundle, buildRoute.Value);
		}
	}
	
	internal static void AddBundleToBuildList(string bundleName, ref Dictionary<string, List<string>> buildingRoutes)	
	{
		BundleData bundle = BundleManager.GetBundleData(bundleName);
		if(bundle == null)
		{
			UnityEngine.Debug.LogError("Cannot find bundle " + bundleName);
			return;
		}
			
		if( BuildHelper.IsBundleNeedBunild(bundle) )
		{
			string rootName = BundleManager.GetRootOf(bundle.name);
			if(buildingRoutes.ContainsKey(rootName))
			{
				if(!buildingRoutes[rootName].Contains(bundle.name))
					buildingRoutes[rootName].Add(bundle.name);
				else
					UnityEngine.Debug.LogError("Bundle name duplicated: " + bundle.name);
			}
			else
			{
				List<string> buildingList = new List<string>();
				buildingList.Add(bundle.name);
				buildingRoutes.Add(rootName, buildingList);
			}
		}
		else
		{
			UnityEngine.Debug.Log("Bundle " + bundle.name + " skiped.");
		}
	}
	
	internal static SingleBundleEnd BuildBundleTree(BundleData bundle, List<string> requiredBuildList)
	{
		BuildPipeline.PushAssetDependencies();
		
		SingleBundleEnd end = BuildSingleBundle(bundle);
		if( end == SingleBundleEnd.eFail )
		{
			UnityEngine.Debug.LogError(bundle.name + " build failed.");
			BuildPipeline.PopAssetDependencies();
			return end;
		}
		else if ( end == SingleBundleEnd.eSucc )
		{
			UnityEngine.Debug.Log(bundle.name + " build succeed.");
		}
		else
		{
            UnityEngine.Debug.Log(bundle.name + " is empty , remove it.");
            BuildPipeline.PopAssetDependencies();
            return end;
		}

        //即将要移除的空资源包
        List<string> bundleToRemove = new List<string>();
		
		foreach(string childName in bundle.children)
		{
			BundleData child = BundleManager.GetBundleData(childName);
			if(child == null)
			{
				UnityEngine.Debug.LogError("Cannnot find bundle [" + childName + "]. Sth wrong with the bundle config data.");
				BuildPipeline.PopAssetDependencies();
                return end;
			}
			
			bool isDependingBundle = false;
			foreach(string requiredBundle in requiredBuildList)
			{
				if(BundleManager.IsBundleDependOn(requiredBundle, childName))
				{
					isDependingBundle = true;
					break;
				}
			}
			
			if(isDependingBundle || !BuildConfiger.DeterministicBundle)
			{
				end = BuildBundleTree(child, requiredBuildList);
				if(end == SingleBundleEnd.eFail)
				{
					BuildPipeline.PopAssetDependencies();
                    return end;
				}
                else if (end == SingleBundleEnd.eEmptyRemove)
                {
                    bundleToRemove.Add(childName);
                }
			}
		}

	    foreach (var removeBundle in bundleToRemove)
	    {
	        BundleManager.RemoveBundle(removeBundle);
	    }
		
		BuildPipeline.PopAssetDependencies();
        return end;
	}
	
	// Get scene or plain assets from include pathes
	internal static string[] GetAssetsFromPaths(string[] includeList, bool onlySceneFiles)
	{
		// Get all the includes file's pathes
		List<string> files = new List<string>();
		foreach(string includPath in includeList)
		{
			if(!File.Exists(includPath) && !Directory.Exists(includPath))
				continue;
			
			bool isDir = (File.GetAttributes(includPath) & FileAttributes.Directory) == FileAttributes.Directory;
			bool isSceneFile = Path.GetExtension(includPath) == ".unity";
			if(!isDir)
			{
				if(onlySceneFiles && !isSceneFile)
					// If onlySceneFiles is true, we can't add file without "unity" extension
					continue;
				
				files.Add(includPath);
			}
			else
			{
				string[] subFiles = null;
				if(onlySceneFiles)
					subFiles = FindSceneFileInDir(includPath, SearchOption.AllDirectories);
				else
					subFiles = FindAssetsInDir(includPath, SearchOption.AllDirectories);
					
				files.AddRange(subFiles);
			}
		}
		
		return files.ToArray();
	}
	
	private static string[] FindSceneFileInDir(string dir, SearchOption option)
	{
		return Directory.GetFiles(dir, "*.unity", option);
	}
	
	private static string[] FindAssetsInDir(string dir, SearchOption option)
	{
		List<string> files = new List<string>( Directory.GetFiles(dir, "*.*", option) );
		files.RemoveAll(x => x.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase) || x.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase));
		return files.ToArray();
	}
	
	private static bool BuildAssetBundle(string[] assetsList, string outputPath)
	{
		// Load all of assets in this bundle
		List<UnityEngine.Object> dependAssets = new List<UnityEngine.Object>();
        List<string> names=new List<string>();
		foreach(string assetPath in assetsList)
		{
			UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
            if (asset != null)
            {
                dependAssets.Add(asset);
                names.Add(asset.name);
            }
            else
                UnityEngine.Debug.LogError("Cannnot load [" + assetPath + "] as asset object");
		}

	    if (dependAssets.Count == 0)
	    {
	        UnityEngine.Debug.Log(" Try to build empty package : " + outputPath);
	        return true;
	    }
		
		// Build bundle
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
		return BuildPipeline.BuildAssetBundle(	null, 
												dependAssets.ToArray(), 
												outputPath, 
												(BMDataAccessor.BMConfiger.Compress ? 0 : BuildAssetBundleOptions.UncompressedAssetBundle) | BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.DeterministicAssetBundle, 
												BuildConfiger.UnityBuildTarget);
#else
        uint crc;
        return BuildPipeline.BuildAssetBundleExplicitAssetNames( 
												dependAssets.ToArray(),
                                                names.ToArray(),
                                                outputPath, out crc);
        
#endif
	}
	
	private static bool BuildSceneBundle(string[] sceneList, string outputPath)
	{
		if(sceneList.Length == 0)
		{
			UnityEngine.Debug.LogError("No scenes were provided for the scene bundle");
			return false;
		}
		
		string error = BuildPipeline.BuildStreamedSceneAssetBundle(sceneList, outputPath, BuildConfiger.UnityBuildTarget);
		return error == "";
	}

    public enum SingleBundleEnd
    {
        eSucc,
        eFail,
        eEmptyRemove,
    }

    private static SingleBundleEnd BuildSingleBundle(BundleData bundle)
	{
		// Prepare bundle output dictionary
		string outputPath = GenerateOutputPathForBundle(bundle.name);
		string bundleStoreDir = Path.GetDirectoryName(outputPath);
		if(!Directory.Exists(bundleStoreDir))
			Directory.CreateDirectory(bundleStoreDir);
		
		// Start build
		string[] assetPathes = GetAssetsFromPaths(bundle.includs.ToArray(), bundle.sceneBundle);

        //如果无资源，并且不是common，则说明是一个废弃包
	    if (assetPathes.Length == 0)
	    {
            return SingleBundleEnd.eEmptyRemove;
	    }

		bool succeed = false;
		if(bundle.sceneBundle)
			succeed = BuildSceneBundle(assetPathes, outputPath);
		else
			succeed = BuildAssetBundle(assetPathes, outputPath);
		
		// Remember the assets for next time build test
		BundleBuildState buildState = BundleManager.GetBuildStateOfBundle(bundle.name);
		if(succeed)
		{
			buildState.lastBuildDependencies = AssetDatabase.GetDependencies(assetPathes);
			buildState.version++;
			if(buildState.version == int.MaxValue)
				buildState.version = 0;
			
			FileInfo bundleFileInfo = new FileInfo(outputPath);
			buildState.size = bundleFileInfo.Length;
		}
		else
		{
			buildState.lastBuildDependencies = null;
		}
		
		BMDataAccessor.SaveBundleBuildeStates();
		return succeed ? SingleBundleEnd.eSucc : SingleBundleEnd.eFail;
	}
	
	private static bool EqualStrArray(string[] strList1, string[] strList2)
	{
		if(strList1 == null || strList2 == null)
			return false;
		
		if(strList1.Length != strList2.Length)
			return false;
		
		for(int i = 0; i < strList1.Length; ++i)
		{
			if(strList1[i] != strList2[i])
				return false;
		}
		
		return true;
	}
	
	private static string GenerateOutputPathForBundle(string bundleName)
	{
		return Path.Combine(BuildConfiger.InterpretedOutputPath, bundleName + "." + BuildConfiger.BundleSuffix);
	}
}
