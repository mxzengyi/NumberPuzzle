﻿using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(AssetBundleInspectorObj))]
public class AssetBundleEditor : Editor
{
#if UNITY_4_3
	public override bool UseDefaultMargins()
	{
		return false;
	}
#endif

	public override void OnInspectorGUI()
	{
		BundleEditorDrawer.DrawInspector();
	}

	void OnEnable()
	{
		BundleEditorDrawer.CurrentBundleEditor = this;
	}
}
