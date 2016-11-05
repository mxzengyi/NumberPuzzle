using UnityEditor;
using System.Collections.Generic;

internal class BundleAssetModificationDetector : AssetPostprocessor 
{
	static void OnPostprocessAllAssets(string[] importAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPathes)
    {
#if IIPS
        List<string> changedAssets = new List<string>();
		changedAssets.AddRange(importAssets);
		changedAssets.AddRange(deletedAssets);
		changedAssets.AddRange(movedFromPathes);
		
		List<string> createdAssets = new List<string>();
		createdAssets.AddRange(importAssets);
		createdAssets.AddRange(movedAssets);
		
		bool dataChanged = false;
		foreach(string assetPath in changedAssets)
		{
			List<BundleData> bundles = BundleManager.GetRelatedBundles(assetPath);
			if(bundles == null)
				continue;
				
			foreach(BundleData bundle in bundles.ToArray()) // Use ToArray to make a copy to prevent change the org bundle list.
			{
				BundleManager.RefreshBundleDependencies(bundle);
				dataChanged = true;
			}
		}
		
		foreach(string assetPath in createdAssets)
		{
			foreach(BundleData bundle in BundleManager.bundles)
			{
				foreach(string include in bundle.includs)
				{
					if(assetPath.Contains(include))
					{
						BundleManager.RefreshBundleDependencies(bundle);
						dataChanged = true;
					}
				}
			}
		}
		
		if(dataChanged)
			BMDataAccessor.SaveBundleData();
#endif

	}
}
