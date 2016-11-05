/*************************************************************************************
    * 命名空间：       public
    * 文 件 名：       $safeitemname$
    * 创建时间：       $time$
    * 作    者：       SparkSun
    * 说   明：
    * 修改时间：
    * 修 改 人：
*************************************************************************************/

using System.Collections;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    public static CoroutineManager Instance;

    void Awake()
    {
        Instance = this;
    }

//    private static void CreateInstance()
//    {
//        if (Instance == null)
//        {
//            GameObject go = new GameObject("CoroutineManager");
//            Instance = go.AddComponent<CoroutineManager>();
//            DontDestroyOnLoad(go);
//        }
//    }

    public IEnumerator WaitCoroutine(IEnumerator routine)
    {
//        if (Instance == null)
//            CreateInstance();

        yield return Instance.StartCoroutine(routine);
    }
}
