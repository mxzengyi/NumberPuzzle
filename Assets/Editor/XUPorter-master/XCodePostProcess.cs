using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.XCodeEditor;
#endif
using System.IO;

public static class XCodePostProcess
{

#if UNITY_EDITOR
	[PostProcessBuild(999)]
	public static void OnPostProcessBuild( BuildTarget target, string pathToBuiltProject )
	{
		if (target != BuildTarget.iOS) {
			Debug.LogWarning("Target is not iPhone. XCodePostProcess will not run");
			return;
		}

		// Create a new project object from build target
		XCProject project = new XCProject( pathToBuiltProject );

		// Find and run through all projmods files to patch the project.
		// Please pay attention that ALL projmods files in your project folder will be excuted!
        //string[] files = Directory.GetFiles( Application.dataPath, "*.projmods", SearchOption.AllDirectories );
        //foreach( string file in files ) {
        //    UnityEngine.Debug.Log("ProjMod File: "+file);
        //    project.ApplyMod( file );
        //}

        //project.AddOtherLinkerFlags("-licucore");

		//TODO implement generic settings as a module option
		//project.overwriteBuildSetting("CODE_SIGN_IDENTITY[sdk=iphoneos*]", "iPhone Distribution", "Release");
        project.overwriteBuildSetting("CODE_SIGN_IDENTITY", "iPhone Developer:yi zeng (4NU4T3EQN2)", "Debug");




        // 编辑plist 文件
        //EditorPlist(pathToBuiltProject);

        //编辑代码文件
        //EditorCode(pathToBuiltProject);


		// Finally save the xcode project
		project.Save();

	}
#endif


    private static void EditorPlist(string filePath)
    {

        XCPlist list = new XCPlist(filePath);
        string bundle = "com.yusong.momo";

        string PlistAdd = @"  
            <key>CFBundleURLTypes</key>
            <array>
            <dict>
            <key>CFBundleTypeRole</key>
            <string>Editor</string>
            <key>CFBundleURLIconFile</key>
            <string>Icon@2x</string>
            <key>CFBundleURLName</key>
            <string>" + bundle + @"</string>
            <key>CFBundleURLSchemes</key>
            <array>
            <string>ww123456</string>
            </array>
            </dict>
            </array>";

        //在plist里面增加一行
        list.AddKey(PlistAdd);
        //在plist里面替换一行
        list.ReplaceKey("<string>com.yusong.${PRODUCT_NAME}</string>", "<string>" + bundle + "</string>");
        //保存
        list.Save();

    }

    private static void EditorCode(string filePath)
    {
        //读取UnityAppController.mm文件
        XClass UnityAppController = new XClass(filePath + "/Classes/UnityAppController.mm");

        //在指定代码后面增加一行代码
        UnityAppController.WriteBelow("#include \"PluginBase/AppDelegateListener.h\"", "#import <ShareSDK/ShareSDK.h>");

        //在指定代码中替换一行
        UnityAppController.Replace("return YES;", "return [ShareSDK handleOpenURL:url sourceApplication:sourceApplication annotation:annotation wxDelegate:nil];");

        //在指定代码后面增加一行
        UnityAppController.WriteBelow("UnityCleanup();\n}", "- (BOOL)application:(UIApplication *)application handleOpenURL:(NSURL *)url\r{\r    return [ShareSDK handleOpenURL:url wxDelegate:nil];\r}");

    }



	public static void Log(string message)
	{
		UnityEngine.Debug.Log("PostProcess: "+message);
	}
}
