using LitJson;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Object = UnityEngine.Object;

internal class BundleTreeWin : EditorWindow 
{
	private Rect m_rect;
		
	private List<string> m_Selections = new List<string>();
	
	private List<string> m_LastTimeShowingBundles = new List<string>();
	private List<string> m_CurrentShowingBundles = new List<string>();
	
	private string m_CurrentRecieving = "";
	private string m_CurrentEditing = "";
	
	// Bundle name edit members
	private string m_EditWaitBundle = "";
	private bool m_EditNeedFocus = true;
	private string m_EditString = "";
	private double m_EditWaitStartTime = -1;
	
	private const string m_EditTextFeildName = "nameTextFeild";
		
	private Dictionary<string, bool> m_BundleFoldDict = new Dictionary<string, bool>();
	
	private Vector2 m_ScrollPos = Vector2.zero;
	
	private const float m_IndentWidth = 22f;
	private const float m_NoToggleIndent = 12f;
	private const float m_ItemHeight = 20f;
	
	private GUIDragHandler m_DragHandler = null;
	
	public string LastSelection()
	{
		if(m_Selections.Count > 0)
			return m_Selections[0];
		else
			return "";
	}
	
	public string lastTimeSelection = "";
	
	public Rect Rect()
	{
		return m_rect;
	}
	
	bool HasFocuse()
	{
		return this == EditorWindow.focusedWindow;
	}
	
	void Update ()
	{
		if(lastTimeSelection != LastSelection())
		{	
			lastTimeSelection = LastSelection();
			BundleEditorDrawer.ShowBundle( BundleManager.GetBundleData(lastTimeSelection) );
		}
		
		if(m_EditWaitBundle != "" && m_EditWaitStartTime > 0)
		{
			// See if we can start edit
			if(EditorApplication.timeSinceStartup - m_EditWaitStartTime > 0.6)
			{
				StartEditBundleName(m_EditWaitBundle);
			}
		}
	}
	
	void OnGUI()
	{
		if(m_DragHandler == null)
		{
			// Setup GUI handler
			m_DragHandler = new GUIDragHandler();
			m_DragHandler.dragIdentifier = "BundleTreeView";
			m_DragHandler.AddRecieveIdentifier(m_DragHandler.dragIdentifier);
			m_DragHandler.canRecieveCallBack = OnCanRecieve;
			m_DragHandler.reciveDragCallBack = OnRecieve;
		}
		
		if( Event.current.type == EventType.MouseDown || Event.current.type == EventType.DragUpdated  || !HasFocuse())
		{
			// Any mouse down msg or lose focuse will cancle the edit waiting process
			m_EditWaitStartTime = -1;
			m_EditWaitBundle = "";
		}

        Rect curWindowRect = EditorGUILayout.BeginVertical(BMGUIStyles.GetBuildinStyle("OL Box"));
		{
			// Update rect info
			if(Event.current.type != EventType.Layout)
				m_rect = curWindowRect;
			
			// Toobar
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
			{
				// Create drop down
				Rect createBtnRect = GUILayoutUtility.GetRect(new GUIContent("Create"), EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(false));
				if( GUI.Button( createBtnRect, "Create", EditorStyles.toolbarDropDown ) )
				{
					GenericMenu menu = new GenericMenu();
					if(m_Selections.Count <= 1)
					{
						menu.AddItem(new GUIContent("Scene Bundle"), false, CreateSceneBundle);
						menu.AddItem(new GUIContent("Asset Bundle"), false, CreateAssetBundle);
                        //menu.AddItem(new GUIContent("UIView Bundle"), false, CreateUIViewBundle);
                        //menu.AddItem(new GUIContent("Level Bundle"), false, CreateLevelBundle);
                        menu.AddItem(new GUIContent("Player Bundle"), false, CreatePlayerBundle);
                        menu.AddItem(new GUIContent("Monster Bundle"), false, CreateMonsterBundle);
                        menu.AddItem(new GUIContent("Config Bundle"), false, CreateConfigBundle);
                        menu.AddItem(new GUIContent("Equip Bundle"), false, CreateEquipBundle);
                        menu.AddItem(new GUIContent("Audio Bundle"), false, CreateAudioBundle);
                        menu.AddItem(new GUIContent("Skill Bundle"), false, CreateSkillBundle);
                        menu.AddItem(new GUIContent("BigTex Bundle"), false, CreateBigTexBundle);
                        menu.AddItem(new GUIContent("Ai Bundle"), false, CreateAiBundle);
                        menu.AddItem(new GUIContent("Child View"), false, CreateChildView);
                        menu.AddItem(new GUIContent("Big Item Texture"), false, CreateBigItemTex);
               //         menu.AddItem(new GUIContent("Head Icons"), false, CreateHeadIcons);
                        menu.AddItem(new GUIContent("Newbie"), false, CreateNewbie);
                        menu.AddItem(new GUIContent("Tavern Texture"), false, CreateTavernTex);
                        menu.AddItem(new GUIContent("Create all Bundles"), false, CreateAllBundles);
                        menu.AddItem(new GUIContent("Remove all Bundles"), false, RemoveAllBundles);
                        menu.AddItem(new GUIContent("Clean cache Bundles"), false, CleanCacheBundles);
                        menu.AddItem(new GUIContent("Clean old Bundles"), false, CleanOldBundles);

                        menu.AddItem(new GUIContent("Build IIPS IFS"), false, BuildIIPSPack);
					}
					else
					{
						menu.AddItem(new GUIContent("Scene Bundle"), false, null);
						menu.AddItem(new GUIContent("Asset Bundle"), false, null);
                       // menu.AddItem(new GUIContent("UIView Bundle"), false, null);
                       // menu.AddItem(new GUIContent("Level Bundle"), false, null);
                        menu.AddItem(new GUIContent("Player Bundle"), false, null);
                        menu.AddItem(new GUIContent("Monster Bundle"), false, null);
                        menu.AddItem(new GUIContent("Config Bundle"), false, null);
                        menu.AddItem(new GUIContent("Equip Bundle"), false, null);
                        menu.AddItem(new GUIContent("Audio Bundle"), false, null);
                        menu.AddItem(new GUIContent("Skill Bundle"), false, null);
                        menu.AddItem(new GUIContent("BitTex Bundle"), false, null);
                        menu.AddItem(new GUIContent("Ai Bundle"), false, null);
                        menu.AddItem(new GUIContent("Create all Bundle"), false, null);
                        menu.AddItem(new GUIContent("Build IIPS IFS"), false, null);
					}
					menu.DropDown(createBtnRect);
				}
				
				// Build button
				Rect buildBtnRect = GUILayoutUtility.GetRect(new GUIContent("Build"), EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(false));
				if( GUI.Button( buildBtnRect, "Build", EditorStyles.toolbarDropDown ) )
				{
					GenericMenu menu = new GenericMenu();
					menu.AddItem(new GUIContent("Build Selection"), false, BuildSelection);
					menu.AddItem(new GUIContent("Rebuild Selection"), false, RebuildSelection);
					menu.AddItem(new GUIContent("Build All"), false, BuildAll);
					menu.AddItem(new GUIContent("Rebuild All"), false, RebuildAll);
					menu.AddItem(new GUIContent("Clear"), false, ClearOutputs);
					menu.DropDown(buildBtnRect);
				}
				
				GUILayout.FlexibleSpace();

				if(GUILayout.Button("Settings", EditorStyles.toolbarButton))
					BMSettingsEditor.Show();
			}
			EditorGUILayout.EndHorizontal();
			
			// Tree view
			m_ScrollPos = EditorGUILayout.BeginScrollView (m_ScrollPos);
			{
				m_CurrentShowingBundles.Clear();
				
				foreach(BundleData rootBundle in BundleManager.Roots)
				{
					if(!GUI_TreeItem(0, rootBundle.name))
					{
						Repaint();
						break;
					}
				}
				
				m_LastTimeShowingBundles.Clear();
				m_LastTimeShowingBundles.AddRange(m_CurrentShowingBundles);

				if(m_CurrentEditing == "")
				{
					ArrowKeyProcess();
					HotkeyProcess();
				}

				// Empty space for root selection
				Rect spaceRect = EditorGUILayout.BeginVertical();
				GUILayout.Space(m_ItemHeight);
				EditorGUILayout.EndVertical();
				RootSpaceProcess(spaceRect);
				
			}EditorGUILayout.EndScrollView();

			Rect scrollViewRect = GUILayoutUtility.GetLastRect();
			if(scrollViewRect.height != 1)
				UpdateScrollBarBySelection(scrollViewRect.height);
			
		}EditorGUILayout.EndVertical();
	}
	
	bool GUI_TreeItem(int indent, string bundleName)
	{
		if(!m_CurrentShowingBundles.Contains(bundleName))
			m_CurrentShowingBundles.Add(bundleName);
		
		BundleData bundleData = BundleManager.GetBundleData(bundleName);
		if(bundleData == null)
		{
			UnityEngine.Debug.LogError("Cannot find bundle : " + bundleName);
			return true;
		}
		
		Rect itemRect = GUI_DrawItem(bundleData, indent);

		if(EditProcess(itemRect, bundleName))
			return false;
		
		if( DragProcess(itemRect, bundleName) )
			return false;
		
		SelectProcess(itemRect, bundleName);
		
		RightClickMenu(itemRect);
		
		return GUI_DrawChildren(bundleName, indent);
	}
	
	Rect GUI_DrawItem(BundleData bundle, int indent)
	{
		bool isEditing = m_CurrentEditing == bundle.name;
		bool isRecieving = m_CurrentRecieving == bundle.name;
		bool isSelected = m_Selections.Contains(bundle.name);
		
		GUIStyle currentLableStyle = BMGUIStyles.GetCustomStyle("TreeItemUnSelect");
		if(isRecieving)
			currentLableStyle = BMGUIStyles.GetCustomStyle("receivingLable");
		else if(isSelected && !isEditing)
			currentLableStyle = HasFocuse() ? BMGUIStyles.GetCustomStyle("TreeItemSelectBlue") : BMGUIStyles.GetCustomStyle("TreeItemSelectGray");
		
		Rect itemRect = EditorGUILayout.BeginHorizontal(currentLableStyle);
		
		if(bundle.children.Count == 0)
		{
			GUILayout.Space(m_IndentWidth * indent + m_NoToggleIndent);
		}
		else
		{
			GUILayout.Space(m_IndentWidth * indent);
			bool fold = !GUILayout.Toggle(!IsFold(bundle.name), "", BMGUIStyles.GetCustomStyle("Foldout"));
			SetFold(bundle.name, fold);
		}
		
		GUILayout.Label(bundle.sceneBundle ? BMGUIStyles.GetIcon("sceneBundleIcon") : BMGUIStyles.GetIcon("assetBundleIcon"), BMGUIStyles.GetCustomStyle("BItemLabelNormal"), GUILayout.ExpandWidth(false));
		
		if(!isEditing)
		{
			GUILayout.Label(bundle.name, isSelected ? BMGUIStyles.GetCustomStyle("BItemLabelActive") : BMGUIStyles.GetCustomStyle("BItemLabelNormal"));
		}
		else
		{
			GUI.SetNextControlName(m_EditTextFeildName);
			m_EditString = GUILayout.TextField(m_EditString, BMGUIStyles.GetCustomStyle("TreeEditField"));
		}
		
		EditorGUILayout.EndHorizontal();
		
		return itemRect;
	}
	
	bool GUI_DrawChildren(string bundleName, int indent)
	{
		BundleData bundleData = BundleManager.GetBundleData(bundleName);
		
		if(bundleData.children.Count == 0 || IsFold(bundleName))
			return true;
		
		for(int i = 0; i < bundleData.children.Count; ++i)
		{
			if(!GUI_TreeItem(indent + 1, bundleData.children[i]))
				return false;
		}
		
		return true;
	}
	
	void GUI_DeleteMenuCallback()
	{
		foreach(string bundle in m_Selections)
			BundleManager.RemoveBundle(bundle);
		
		m_Selections.Clear();
		Repaint();
	}

    //处理光标键
	void ArrowKeyProcess()
	{
		if(m_LastTimeShowingBundles.Count == 0)
			return;

		KeyCode key = Event.current.keyCode;
		if(Event.current.type != EventType.keyDown)
		{
			if(Event.current.isKey && (key == KeyCode.UpArrow || key == KeyCode.DownArrow || key == KeyCode.LeftArrow || key == KeyCode.RightArrow))
				Event.current.Use(); // Prevent the system warning sound
			return;
		}

		if(key == KeyCode.UpArrow || key == KeyCode.DownArrow)
		{
			string lastSelect = "";
			if(m_Selections.Count > 0)
				lastSelect = m_Selections[m_Selections.Count - 1];

			int lastIndex = m_LastTimeShowingBundles.FindIndex(x=> x == lastSelect);
			int newIndex = lastIndex + (key == KeyCode.UpArrow ? - 1 : +1);
			if(newIndex < 0)
				newIndex = 0;
			else if(newIndex >= m_LastTimeShowingBundles.Count)
				newIndex = m_LastTimeShowingBundles.Count - 1;
			
			string newAddBundle = m_LastTimeShowingBundles[newIndex];
			if(Event.current.shift)
			{
				ShiftSelection(newAddBundle);
			}
			else
			{
				m_Selections.Clear();
				m_Selections.Add(newAddBundle);
			}

			Event.current.Use();
			Repaint();
		}
		else if(key == KeyCode.LeftArrow || key == KeyCode.RightArrow)
		{
			foreach(string selectName in m_Selections)
			{
				SetFold(selectName, key == KeyCode.LeftArrow);
			}

			Event.current.Use();
			Repaint();
		}
	}

	void HotkeyProcess()
	{
		bool deletePressed = (Application.platform == RuntimePlatform.OSXEditor && Event.current.keyCode == KeyCode.Backspace) ||
							 (Application.platform == RuntimePlatform.WindowsEditor && Event.current.keyCode == KeyCode.Delete);

		if(deletePressed && Event.current.type == EventType.KeyDown && Control())
		{
			GUI_DeleteMenuCallback();
			Event.current.Use();
			Repaint();
		}
	}
	
	bool EditProcess(Rect itemRect, string bundleName)
	{
		if(m_CurrentEditing == bundleName)
		{
			// Bundle name is in editing
			
			if(m_EditNeedFocus)
			{
				// First time after edit started. Set focuse for the text field control
				GUI.FocusControl(m_EditTextFeildName);
				m_EditNeedFocus = false;
				Repaint();
				return false;
			}
		
			// If lose focus end this edit
			bool clickOutSideTheTextField = m_CurrentEditing != "" && Event.current.type == EventType.MouseDown && !IsRectClicked(itemRect);
			bool isFinishedEdit = Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return;
			if(!HasFocuse() || clickOutSideTheTextField || isFinishedEdit)
			{
				BundleManager.RenameBundle(bundleName, m_EditString);
				
				if(Event.current.type == EventType.Layout)
					return false;
				
				m_CurrentEditing = "";	 
				m_EditString = "";
				Repaint();
				GUIUtility.keyboardControl = 0;
				
				return true;
			}
		}
		else if(IsRectClicked(itemRect) && Event.current.button == 0 && m_Selections.Count == 1 && m_Selections[0] == bundleName && !Control() && !Event.current.shift)
		{
			// Try start edit
			m_EditWaitStartTime = EditorApplication.timeSinceStartup;
			m_EditWaitBundle = bundleName;
		}
		
		return false;
	}

	string m_LastNewSelection = "";
	void UpdateScrollBarBySelection(float viewHeight)
	{
		if(m_Selections.Count == 0)
		{
			m_LastNewSelection = "";
			return;
		}

		string newSelection = m_Selections[m_Selections.Count - 1];
		if(newSelection == m_LastNewSelection)
			return;

		m_LastNewSelection = newSelection;

		int selectionRow = m_LastTimeShowingBundles.FindIndex(x=>x == newSelection);
		if(selectionRow < 0)
			return;

		float selectTopOffset = selectionRow * m_ItemHeight;
		if(selectTopOffset < m_ScrollPos.y)
			m_ScrollPos.y = selectTopOffset;
		else if(selectTopOffset + m_ItemHeight > m_ScrollPos.y + viewHeight)
			m_ScrollPos.y = selectTopOffset + m_ItemHeight - viewHeight;

		Repaint();
	}
	
	void StartEditBundleName(string bundleName)
	{
		m_CurrentEditing = bundleName;
		m_EditString = bundleName;
		m_EditNeedFocus = true;
		
		m_EditWaitStartTime = -1;
		m_EditWaitBundle = "";
		
		Repaint();
	}
	
	void SelectProcess(Rect itemRect, string bundleName)
	{
		if(IsRectClicked(itemRect) && m_CurrentEditing != bundleName)
		{
			if(Control())
			{
				if(m_Selections.Contains(bundleName))
					m_Selections.Remove(bundleName);
				else
					m_Selections.Add(bundleName);
			}
			else if(Event.current.shift)
			{
				ShiftSelection(bundleName);
			}
			else if(Event.current.button == 0 || !m_Selections.Contains(bundleName))
			{
				m_Selections.Clear();
				m_Selections.Add(bundleName);
			}
			
			m_CurrentEditing = "";
			Repaint();
		}
	}
	
	void RightClickMenu(Rect itemRect)
	{
		GenericMenu rightClickMenu = new GenericMenu();
		rightClickMenu.AddItem(new GUIContent("Delete"), false, GUI_DeleteMenuCallback);
		if(IsMouseOn(itemRect) && Event.current.type == EventType.MouseUp && Event.current.button == 1)
		{
			Vector2 mousePos = Event.current.mousePosition;
			rightClickMenu.DropDown(new Rect(mousePos.x, mousePos.y, 0, 0));
		}
	}
	
	bool DragProcess(Rect itemRect, string bundleName)
	{
		if( Event.current.type == EventType.Repaint || itemRect.height <= 0)
			return false;
		
		if( !IsMouseOn(itemRect) )
		{
			if( m_CurrentRecieving != "" && m_CurrentRecieving == bundleName )
			{
				m_CurrentRecieving = "";
				Repaint();
			}
			
			return false;
		}
		
		m_DragHandler.detectRect = itemRect;
		m_DragHandler.dragData.customDragData = (object)bundleName;
		m_DragHandler.dragAble = bundleName != "";
		
		var dragState = m_DragHandler.GUIDragUpdate();
		if(dragState == GUIDragHandler.DragState.Receiving)
		{
			m_CurrentRecieving = bundleName;
			Repaint();
		}
		else if(dragState == GUIDragHandler.DragState.Received)
		{
			BundleEditorDrawer.Refresh();
			m_CurrentRecieving = "";
		}
		else if(m_CurrentRecieving == bundleName)
		{
			// Drag cursor leaved
			m_CurrentRecieving = "";
			Repaint();
		}
		
		return dragState == GUIDragHandler.DragState.Received;
	}
	
	void RootSpaceProcess(Rect spaceRect)
	{
		if(IsRectClicked(spaceRect) && !(Control() || Event.current.shift))
		{
			m_Selections.Clear();
			m_CurrentEditing = "";
			Repaint();
			Event.current.Use();
		}
		
		DragProcess(spaceRect, "");
	}
	
	void ShiftSelection(string newSelect)
	{
		if(m_Selections.Count == 0)
		{
			m_Selections.Add(newSelect);
			return;
		}
		
		int minIndex = int.MaxValue;
		int maxIndex = int.MinValue;
		foreach(string bundle in m_Selections)
		{
			int selIndex = m_LastTimeShowingBundles.IndexOf(bundle);
			if(selIndex == -1)
				continue;
			
			if(minIndex > selIndex)
				minIndex = selIndex;
			if(maxIndex < selIndex)
				maxIndex = selIndex;
		}
		
		if(minIndex == int.MaxValue || maxIndex == int.MinValue)
		{
			m_Selections.Add(newSelect);
			return;
		}
		
		int fromIndex = 0;
		int toIndex = m_LastTimeShowingBundles.IndexOf(newSelect);
		if(toIndex >= minIndex && toIndex <= maxIndex)
			fromIndex = m_LastTimeShowingBundles.IndexOf(m_Selections[0]);
		else if(toIndex < minIndex)
			fromIndex = maxIndex;
		else if(toIndex > maxIndex)
			fromIndex = minIndex;

		int step = toIndex > fromIndex ? 1 : -1;
		m_Selections.Clear();
		while(fromIndex != toIndex + step)
		{
			m_Selections.Add(m_LastTimeShowingBundles[fromIndex]);
			fromIndex += step;
		}
	}
	
	bool IsRectClicked(Rect rect)
	{
		return Event.current.type == EventType.MouseDown && IsMouseOn(rect);
	}
	
	bool Control()
	{
		return (Event.current.control && Application.platform == RuntimePlatform.WindowsEditor) ||
			(Event.current.command && Application.platform == RuntimePlatform.OSXEditor);
	}
	
	bool IsMouseOn(Rect rect)
	{
		return rect.Contains(Event.current.mousePosition);
	}
	
	bool IsFold(string name)
	{
		if(!m_BundleFoldDict.ContainsKey(name))
			m_BundleFoldDict.Add(name, true);
			
		return m_BundleFoldDict[name];
	}
	
	void SetFold(string name, bool isFold)
	{
		m_BundleFoldDict[name] = isFold;
	}

    #region Create bundle

    private void CreateSceneBundle()
    {
        if (m_Selections.Count == 1)
            newDefBundleTo(m_Selections[0], true);
        else
            newDefBundleTo("", true);
    }

    private void CreateAssetBundle()
    {
        if (m_Selections.Count == 1)
            newDefBundleTo(m_Selections[0], false);
        else
            newDefBundleTo("", false);
    }

    private static void CreateUIViewBundle()
    {
        ArangeBundleDependent.ArangeDependent("RTP/UIView", "UI");
    }

    private static void CreateEquipBundle()
    {
        //ArangeBundleDependent.ArangeDependent("RTP/Equip", "Equip");

        BundleManager.CreateNewBundle("EquipCommon", "", false);
        BundleData commonBundleData = BundleManager.GetBundleData("EquipCommon");
        
        commonBundleData.includs.Add("Assets/CommonEmpty.txt");

        CreateBundle("RTP/Equip", "EquipCommon");
    }

    private static void CreateAudioBundle()
    {
        ArangeBundleDependent.ArangeDependent("RTP/Audio", "Audio");
    }

    private static void CreateLevelBundle()
    {
        ArangeBundleDependent.ArangeDependent("RTP/Levels", "Level");
    }

    private static void CreatePlayerBundle()
    {
        ArangeBundleDependent.ArangeDependent("RTP/Characters/player", "Player");
    }

    private static void CreateMonsterBundle()
    {
        ArangeBundleDependent.ArangeDependent("RTP/Characters/monster", "Monster");
    }

    private static void CreateSkillBundle()
    {
        ArangeBundleDependent.ArangeDependent("RTP/FightSkills", "Skill");
//        BundleManager.CreateNewBundle("SkillCommon", "", false);
//        BundleData commonBundleData = BundleManager.GetBundleData("SkillCommon");
//
//        commonBundleData.includs.Add("Assets/CommonEmpty.txt");
//
//        CreateBundle("RTP/FightSkills", "SkillCommon");
    }

    private static void CreateConfigBundle()
    {
        //delete confige bundle
        BundleManager.RemoveBundle("ConfigCommon");

        ArangeBundleDependent.ArangeDependent(new List<string>()
        {
            "RTP/Config/App",
            "RTP/Config/Sys"
        }, "Config");
    }

    private static void CreateBigTexBundle()
    {
        ArangeBundleDependent.ArangeDependent("RTP/BigTex", "BigTex");
    }

    private static void CreateAiBundle()
    {
        ArangeBundleDependent.ArangeDependent("RTP/AiConfig", "Ai");
    }


    private static void CreateChildView()
    {
        ArangeBundleDependent.ArangeDependent("RTP/ChildView", "ChildView");
    }

    private static void CreateBigItemTex()
    {
        ArangeBundleDependent.ArangeDependent("RTP/BigItemTex", "BigItemTex");
    }

    private static void CreateHeadIcons()
    {
        ArangeBundleDependent.ArangeDependent("RTP/HeadIcons", "HeadIcons");
    }

    private static void CreateNewbie()
    {
        ArangeBundleDependent.ArangeDependent("RTP/Newbie", "Newbie");
    }

    private static void CreateTavernTex()
    {
        ArangeBundleDependent.ArangeDependent("RTP/TavernTex", "TavernTex");
    }

    static void CleanCacheBundles()
    {
        string cacheFolder = Application.persistentDataPath + "/AssetBundles/";

        if (!Directory.Exists(cacheFolder))
            return;

        foreach (var file in Directory.GetFiles(cacheFolder))
        {
            File.Delete(file);
        }
    }

    static void CleanOldBundles()
    {
        string outputPath = BuildConfiger.InterpretedOutputPath;
        if (!Directory.Exists(outputPath))
            return;

        foreach (string file in Directory.GetFiles(outputPath))
        {
            File.Delete(file);
            UnityEngine.Debug.Log("Remove " + file);
        }
    }

    static void RemoveAllBundles()
    {
        BundleManager.RemoveAllBundles();
    }

    public static void CreateAllBundles()
    {
        BundleManager.RemoveAllBundles();

        // 删除InterpretedOutputPath下的文件
        CleanOldBundles();

        // 删除AssetBundle文件
        CleanCacheBundles();

       // CreateUIViewBundle();
        CreateEquipBundle();
        CreateAudioBundle();
      //  CreateLevelBundle();
        CreatePlayerBundle();
        CreateMonsterBundle();
        CreateConfigBundle();
        CreateSkillBundle();
        CreateBigTexBundle();
        CreateAiBundle();

        CreateChildView();
        CreateBigItemTex();
    //    CreateHeadIcons();
        CreateNewbie();
        CreateTavernTex();

        BuildAll();
    }

    // 以下方法是build script调用
    public static void CreateAllBundlesFromCommand()
    {
        BundleManager.RemoveAllBundles();

        // 删除InterpretedOutputPath下的文件
        CleanOldBundles();

        // 删除AssetBundle文件
        CleanCacheBundles();

        // CreateUIViewBundle();
        CreateEquipBundle();
        CreateAudioBundle();
    //    CreateLevelBundle();
        CreatePlayerBundle();
        CreateMonsterBundle();
        CreateConfigBundle();
        CreateSkillBundle();
        CreateBigTexBundle();
        CreateAiBundle();

        CreateChildView();
        CreateBigItemTex();
 //       CreateHeadIcons();
        CreateNewbie();
        CreateTavernTex();

        BuildHelper.BuildAll();
        BuildHelper.ExportBundleDataFileToOutput();
        BuildHelper.ExportBundleBuildDataFileToOutput();
        BuildHelper.ExportBMConfigerFileToOutput();
    }

    private static void BuildIIPSPack()
    {
        //运行打包
        System.Diagnostics.Process buildProcess = new System.Diagnostics.Process();

        string processPath = Application.dataPath.Replace("Assets", "");
        processPath = processPath + "IIPSPackTools/AutoPheroIFS.bat";

        UnityEngine.Debug.Log("BuildTool path is : " + processPath);

        buildProcess.StartInfo.FileName = processPath;

        buildProcess.Start();
        buildProcess.WaitForExit();

        //打包完成
        UnityEngine.Debug.Log("Build IFS end");

//        //加密数据包
//        processPath = Application.dataPath.Replace("Assets", "");
//        processPath = processPath + "IIPSPackTools/EncryptIFS.bat";
//
//        UnityEngine.UnityEngine.Debug.Log("EncryptTool path is : " + processPath);
//
//        buildProcess.StartInfo.FileName = processPath;
//
//        buildProcess.Start();
//        buildProcess.WaitForExit();
//
//        UnityEngine.UnityEngine.Debug.Log("Encrypt IFS end");

        //生成json
//        GenerateJson();
    }

    public class JsonFileItemInfo
    {
        public string url;
        public string filename;
        public string filesize;
    }

    private static void GenerateJson()
    {
        List<JsonFileItemInfo> jsonItemList = new List<JsonFileItemInfo>();

        JsonFileItemInfo itemInfo = new JsonFileItemInfo();
        itemInfo.url = @"http://dlied5.qq.com/Pet/AssetBundles/pherortppack.ifs";
        itemInfo.filename = "NewIFSPack.ifs";

        string ifsPath = Application.dataPath.Replace("Assets", "") + "IIPSPackTools/NewIFSPack/pherortppack.ifs";
        UnityEngine.Debug.Log(ifsPath);
        FileInfo ifsFileInfo = new FileInfo(ifsPath);

        itemInfo.filesize = ifsFileInfo.Length.ToString();

        jsonItemList.Add(itemInfo);

        JsonData jsonFileData = new JsonData();

        jsonFileData["filelist"] = JsonMapper.ToJson(jsonItemList);

        string jsonFormatString = JsonFormatter.PrettyPrint(jsonFileData.ToJson());

        UnityEngine.Debug.Log( jsonFormatString );

        //保存json文件
        string jsonFilePath = Application.dataPath.Replace("Assets", "") + "IIPSPackTools/NewIFSPack/PHero-1.0.0.0-1.0.0.1.json";

        StreamWriter jsonWriter = null;

        if (File.Exists(jsonFilePath))
        {
            File.Delete(jsonFilePath);
        }

        jsonWriter = File.CreateText(jsonFilePath);

        jsonWriter.Write(jsonFormatString);

        jsonWriter.Close();
    }

    private static void CreateBundle(string bunlePath , string parentName )
    {
        Object[] assetObjects = Resources.LoadAll(bunlePath);

        foreach (var asset in assetObjects)
        {
            string bundleName = asset.name;

            if (BundleManager.GetBundleData(bundleName) != null)
                continue;

            bool created = BundleManager.CreateNewBundle(bundleName, parentName, false);

            if (!created)
                UnityEngine.Debug.LogError("Can't create bundle tree bundle : " + bundleName);

            string bundleAssetDataPath = AssetDatabase.GetAssetPath(asset);

            if (BundleManager.CanAddPathToBundle(bundleAssetDataPath, bundleName))
                BundleManager.AddPathToBundle(bundleAssetDataPath, bundleName);
        }
    }

    #endregion

	
	void newDefBundleTo(string parent, bool sceneBundle)
	{
		// Find a new bundle name
		string defBundleName = "EmptyBundle";
		string currentBundleName = defBundleName;
		int index = 0;
		while(BundleManager.GetBundleData(currentBundleName) != null)
		{
			currentBundleName = defBundleName + (++index);
		}
		
		bool created = BundleManager.CreateNewBundle(currentBundleName, parent, sceneBundle);
		if(created)
		{
			if(IsFold( parent ))
				SetFold(parent, false);
			
			m_Selections.Clear();
			m_Selections.Add(currentBundleName);
		}
		
		StartEditBundleName(currentBundleName);
	}
	
	public static void BuildAll()
	{
		BuildHelper.BuildAll();
		BuildHelper.ExportBundleDataFileToOutput();
		BuildHelper.ExportBundleBuildDataFileToOutput();
		BuildHelper.ExportBMConfigerFileToOutput();

	    BuildIIPSPack();
	}
	
	void RebuildAll()
	{
		BuildHelper.RebuildAll();
		BuildHelper.ExportBundleDataFileToOutput();
		BuildHelper.ExportBundleBuildDataFileToOutput();
		BuildHelper.ExportBMConfigerFileToOutput();
	}
	
	void ClearOutputs()
	{
		string outputPath = BuildConfiger.InterpretedOutputPath;
		if( !Directory.Exists(outputPath) )
			return;
		
		foreach(string file in Directory.GetFiles(outputPath) )
		{
			File.Delete(file);
			UnityEngine.Debug.Log("Remove " + file);
		}
	}
	
	void BuildSelection()
	{	
		BuildHelper.BuildBundles(m_Selections.ToArray());
		BuildHelper.ExportBundleDataFileToOutput();
		BuildHelper.ExportBundleBuildDataFileToOutput();
		BuildHelper.ExportBMConfigerFileToOutput();
	}
	
	void RebuildSelection()
	{
		foreach(string bundleName in m_Selections)
		{
			BundleBuildState buildState = BundleManager.GetBuildStateOfBundle(bundleName);
			buildState.lastBuildDependencies = null;
		}
		
		BuildHelper.BuildBundles(m_Selections.ToArray());
		BuildHelper.ExportBundleDataFileToOutput();
		BuildHelper.ExportBundleBuildDataFileToOutput();
		BuildHelper.ExportBMConfigerFileToOutput();
	}
	
	bool OnCanRecieve(GUIDragHandler.DragDatas recieverData, GUIDragHandler.DragDatas dragData)
	{
		if(dragData.customDragData == null && dragData.dragPaths.Length != 0)
		{
			foreach(string dragPath in dragData.dragPaths)
			{
				if(dragPath == null)
					continue;

				if(BundleManager.CanAddPathToBundle(dragPath, (string)recieverData.customDragData))
					return true;
			}
			
			return false;
		}
		else
		{
			return BundleManager.CanBundleParentTo((string)dragData.customDragData, (string)recieverData.customDragData);
		}
	}
	
	void OnRecieve(GUIDragHandler.DragDatas recieverData, GUIDragHandler.DragDatas dragData)
	{
	    if (dragData.customDragData == null && dragData.dragPaths.Length != 0)
	    {
	        foreach (string dragPath in dragData.dragPaths)
	        {
	            if (dragPath == null)
	                continue;

	            if (BundleManager.CanAddPathToBundle(dragPath, (string) recieverData.customDragData))
	                BundleManager.AddPathToBundle(dragPath, (string) recieverData.customDragData);
	        }
	    }
	    else
	    {
            BundleManager.SetParent((string)dragData.customDragData, (string)recieverData.customDragData);
	    }

			
	}
	
	[MenuItem("Window/Bundle Manager")]
	static void Init()
	{
		EditorWindow.GetWindow<BundleTreeWin>("Bundles");
	}
}
