using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(BMSettingsInspectorObj))]
internal class BMSettingsEditor : Editor 
{
	static BMSettingsInspectorObj settingsInspectorObj = null;

	public static void Show()
	{
		// Show dummy object in inspector
		if(settingsInspectorObj == null)
		{
			settingsInspectorObj = ScriptableObject.CreateInstance<BMSettingsInspectorObj>();
			settingsInspectorObj.hideFlags = HideFlags.DontSave;
			settingsInspectorObj.name = "BundleManager Settings";
		}

		Selection.activeObject = settingsInspectorObj;
	}

#if UNITY_4_3
	public override bool UseDefaultMargins()
	{
		return false;
	}
#endif

	public override void OnInspectorGUI()
	{
		EditorGUILayout.BeginVertical();
		{
			BuildConfiger.Compress = EditorGUILayout.Toggle("Compress", BuildConfiger.Compress);
			BuildConfiger.DeterministicBundle = EditorGUILayout.Toggle("Deterministic", BuildConfiger.DeterministicBundle);
			BuildConfiger.BundleSuffix = EditorGUILayout.TextField("Bundle Suffix", BuildConfiger.BundleSuffix);
			DownloadConfiger.useCache = EditorGUILayout.Toggle("Use Unity Cache", DownloadConfiger.useCache);
			DownloadConfiger.downloadThreadsCount = EditorGUILayout.IntField("Download Thread Limit", DownloadConfiger.downloadThreadsCount);
			DownloadConfiger.retryTime = EditorGUILayout.IntField("Error Retry Time", DownloadConfiger.retryTime);

			EditorGUILayout.BeginVertical(BMGUIStyles.GetBuildinStyle("Wizard Box"));
			{
				BuildConfiger.UseEditorTarget = EditorGUILayout.Toggle("Use Editor Target", BuildConfiger.UseEditorTarget);

				if(!BuildConfiger.UseEditorTarget)
				{
					BuildPlatform origPlatform = BuildConfiger.BundleBuildTarget;
					BuildPlatform newPlatform = (BuildPlatform)EditorGUILayout.EnumPopup("Build Target", (System.Enum)origPlatform);
					if(origPlatform != newPlatform)
					{
						GUIUtility.keyboardControl = 0; // Remove the focuse on path text field
						BuildConfiger.BundleBuildTarget = newPlatform;
						PlayerPrefs.SetInt("BundleManagerPlatform", (int)newPlatform);
					}
				}
				else
				{
					BuildConfiger.UnityBuildTarget = EditorUserBuildSettings.activeBuildTarget;
				}

				EditorGUILayout.BeginHorizontal();
				{
					BuildConfiger.BuildOutputStr = EditorGUILayout.TextField("Output Path", BuildConfiger.BuildOutputStr);
					if(GUILayout.Button("...", GUILayout.MaxWidth(24)))
					{
						GUIUtility.keyboardControl = 0; // Remove the focuse on path text field
						string origPath = BuildConfiger.InterpretedOutputPath;
						string newOutputPath = EditorUtility.OpenFolderPanel("Choose Output Path", origPath, "");
						if(newOutputPath != "" && newOutputPath != origPath)
							BuildConfiger.BuildOutputStr = newOutputPath;
					}
				}EditorGUILayout.EndHorizontal();
				
				if(!DownloadConfiger.downloadFromOutput)
					DownloadConfiger.downloadUrl = EditorGUILayout.TextField("Download Url", DownloadConfiger.downloadUrl);
				
				DownloadConfiger.downloadFromOutput = EditorGUILayout.Toggle("Download from Output", DownloadConfiger.downloadFromOutput);
			}EditorGUILayout.EndVertical();
			
			if( GUILayout.Button("Export Config File To Output") )
				BuildHelper.ExportBMConfigerFileToOutput();
			
			GUILayout.FlexibleSpace();
		}EditorGUILayout.EndVertical();
		
		if(GUI.changed)
		{
			BMDataAccessor.SaveBMConfiger();
			BMDataAccessor.SaveUrls();
		}
	}
}