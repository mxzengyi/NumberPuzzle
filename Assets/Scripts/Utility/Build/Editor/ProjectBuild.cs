using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEditor;

public class ProjectBuild : Editor {

    static string[] GetBuildScenes()
    {
        List<string> names = new List<string>();

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene == null)
                continue;
            if (scene.enabled)
                names.Add(scene.path);
        }
        return names.ToArray();
    }

    public static string projectName
    {
        get
        {
            foreach (string arg in System.Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("project"))
                {
                    return arg.Split("-"[0])[1];
                }
            }
            return PlayerSettings.productName;
        }
    }

    static void BuildForIPhone()
    {
        //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "USE_SHARE");
        //这里就是构建xcode工程的核心方法了， 
        //参数1 需要打包的所有场景
        //参数2 需要打包的名子
        //参数3 打包平台
        BuildPipeline.BuildPlayer(GetBuildScenes(), projectName, BuildTarget.iOS, BuildOptions.None);
    }

}
