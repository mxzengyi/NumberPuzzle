//using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;
using System.Reflection;
using System.ComponentModel;
using Formatting = System.Xml.Formatting;
using Object = System.Object;
using Random = System.Random;
using Newtonsoft.Json;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class CommonTools
{
    //模板函数， 读取json文件储存为一个对象
    static public T LoadObjectFromJsonFile<T>(string path)
    {
        if (!File.Exists(path))
        {
            return default(T);
        }

        TextReader reader = new StreamReader(path);

        T data = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());

        reader.Close();
        return data;
    }

    //模板函数，将一个对象存储成json文件，文件对象的目录一定要存在
     public static void SaveObjectToJsonFile<T>(T data, string path)
     {
         TextWriter tw = new StreamWriter(path);

         string jsonStr = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
 
         tw.Write(jsonStr);
         tw.Flush();
         tw.Close();
     }

    //列表乱序函数
    public static void RandomizeList(IList pList)
    {
        System.Random rng = new System.Random();
        int n = pList.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var value = pList[k];
            pList[k] = pList[n];
            pList[n] = value;
        }

    }

    //模板函数， 读取json文件储存为一个对象
//     static public T LoadObjectFromJsonFile<T>(string path)
//     {
//         if (!File.Exists(path))
//         {
//             return default(T);
//         }
// 
//         TextReader reader = new StreamReader(path);
// 
//         T data = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
// 
//         reader.Close();
//         return data;
//     }

    public static string GetPathFileName(string path)
    {
        //提取资源名称
        string fileName = System.IO.Path.GetFileName(path);

        //去除后缀
        fileName = fileName.Substring(0, fileName.LastIndexOf('.'));

        return fileName;
    }

    //递归设置一个GameObject的层
    public static void SetObjectLayer(Transform transform, int layer)
    {
        transform.gameObject.layer = layer;

        int transChildCount = transform.childCount;

        if (transChildCount != 0)
        {
            for (int i = 0; i < transChildCount; i++)
            {
                SetObjectLayer(transform.GetChild(i), layer);
            }
        }
    }




    //public static void EmitParticle(Transform particleTransform, ParticleSystem[] ps = null)
    //{
    //    if (particleTransform == null)
    //        return;

    //    particleTransform.gameObject.SetActive(true);
    //    ParticleSystem particle;
    //    if (ps == null)
    //    {
    //        particle = particleTransform.gameObject.GetComponent<ParticleSystem>();

    //        if (particle != null)
    //        {
    //            if (((ParticleSystemRenderer) particle.renderer).renderMode == ParticleSystemRenderMode.Mesh ||
    //                (particle.simulationSpace == ParticleSystemSimulationSpace.Local &&
    //                 ((ParticleSystemRenderer) particle.renderer).renderMode == ParticleSystemRenderMode.Stretch))
    //            {
    //                particle.renderer.enabled = true;
    //            }
    //            //reset first
    //            particle.Stop(true);
    //            particle.Clear(true);
    //            particle.enableEmission = true;
    //            particle.Play(true);
    //        }
    //    }
    //    else
    //    {
    //        int count = ps.Count();
    //        for (int i = 0; i < count; i++)
    //        {
    //            particle = ps[i];
    //            if (particle != null)
    //            {
    //                if (((ParticleSystemRenderer) particle.renderer).renderMode == ParticleSystemRenderMode.Mesh ||
    //                    (particle.simulationSpace == ParticleSystemSimulationSpace.Local &&
    //                     ((ParticleSystemRenderer) particle.renderer).renderMode == ParticleSystemRenderMode.Stretch))
    //                {
    //                    particle.renderer.enabled = true;
    //                }
    //                //reset first
    //                particle.Stop(true);
    //                particle.Clear(true);
    //                particle.enableEmission = true;
    //                particle.Play(true);
    //            }
    //        }
    //    }

    //}

    //public static void EmitParticle(Transform particleTransform, List<Renderer> allRenderers, List<Animation> allAnimations, List<ParticleSystem> allParticleSystems)
    //{
    //    if (particleTransform == null)
    //        return;

    //    if (allRenderers == null && allAnimations == null && allParticleSystems == null)
    //    {
    //        EmitParticle(particleTransform);
    //        return;
    //    }



    //    particleTransform.gameObject.SetActive(true);

    //    if (allRenderers != null && allRenderers.Count > 0)
    //    {
    //        List<Renderer>.Enumerator rendererEnumerator = allRenderers.GetEnumerator();
    //        while (rendererEnumerator.MoveNext())
    //        {
    //            Renderer renderer = rendererEnumerator.Current;

    //            if (renderer == null) continue;

    //            if (renderer is MeshRenderer)
    //            {
    //                renderer.enabled = true;
    //                if (renderer.gameObject.GetComponent<NcSpriteAnimation>() != null)
    //                {
    //                    renderer.gameObject.GetComponent<NcSpriteAnimation>().ResetAnimation();
    //                }
    //            }

    //            if (renderer is SkinnedMeshRenderer)
    //            {
    //                renderer.enabled = true;
    //            }

    //            if (renderer is TrailRenderer)
    //            {
    //                renderer.enabled = true;
    //            }
    //        }
    //    }

    //    if (allAnimations != null && allAnimations.Count > 0)
    //    {
    //        List<Animation>.Enumerator animationEnumerator = allAnimations.GetEnumerator();
    //        while (animationEnumerator.MoveNext())
    //        {
    //            Animation animation = animationEnumerator.Current;

    //            if (animation == null) continue;

    //            animation.Stop();
    //            AnimationClip clip = animation.clip;
    //            if (clip != null)
    //            {
    //                if (animation[clip.name] == null)
    //                {
    //                    animation.AddClip(clip, clip.name);
    //                }
    //                if (Application.isPlaying)
    //                {
    //                    animation.Play(clip.name);
    //                }
    //                else
    //                {
    //                    animation.gameObject.SampleAnimation(clip, 0.0f);
    //                }
    //            }
    //        }
    //    }

    //    if (allParticleSystems != null && allParticleSystems.Count > 0)
    //    {
    //        List<ParticleSystem>.Enumerator particleEnumerator = allParticleSystems.GetEnumerator();
    //        while (particleEnumerator.MoveNext())
    //        {
    //            ParticleSystem particle = particleEnumerator.Current;

    //            if (particle == null) continue;

    //            if (((ParticleSystemRenderer) particle.renderer).renderMode == ParticleSystemRenderMode.Mesh ||
    //                (particle.simulationSpace == ParticleSystemSimulationSpace.Local &&
    //                 ((ParticleSystemRenderer) particle.renderer).renderMode == ParticleSystemRenderMode.Stretch))
    //            {
    //                particle.renderer.enabled = true;
    //            }
    //            //reset first
    //            particle.Stop(true);
    //            particle.Clear(true);
    //            particle.enableEmission = true;
    //            particle.Play(true);
    //        }
    //    }
    //}


    public static void ResetTransfrom(Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
        transform.localScale = Vector3.one;
    }

    public static void EnableComponent(Transform rootTrans , string componentName , bool enable )
    {
        if (rootTrans == null)
            return;

        UnityEngine.Component tmpCompoent = rootTrans.GetComponent(componentName);

        if (tmpCompoent != null)
            tmpCompoent.gameObject.SetActive(enable);

        int childCount = rootTrans.childCount;

        for (int i = 0; i < childCount; i++)
        {
            EnableComponent(rootTrans.GetChild(i),componentName,enable);
        }
    }

    //public static void StopParticle(Transform particleTransform)
    //{
    //    if (particleTransform == null)
    //        return;

    //    Renderer renderer = particleTransform.GetComponent<Renderer>();

    //    if (renderer is MeshRenderer)
    //    {
    //        renderer.enabled = false;
    //        NcSpriteAnimation ncSpriteAnimation = particleTransform.gameObject.GetComponent<NcSpriteAnimation>();
    //        if (ncSpriteAnimation != null)
    //        {
    //            ncSpriteAnimation.enabled = false;
    //        }
    //    }

    //    if (renderer is SkinnedMeshRenderer)
    //    {
    //        renderer.enabled = false;
    //    }

    //    if (renderer is TrailRenderer)
    //    {
    //        renderer.enabled = false;

    //    }

    //    ParticleSystem particle = particleTransform.GetComponent<ParticleSystem>();


    //    if (particle != null)
    //    {
    //        particle.Stop(true);
    //        particle.enableEmission = false;
    //        particle.Clear(true);
    //    }

    //    particleTransform.gameObject.SetActive(false);

    //    int transChildCount = particleTransform.childCount;

    //    if (transChildCount != 0)
    //    {
    //        for (int i = 0; i < transChildCount; i++)
    //        {
    //            StopParticle(particleTransform.GetChild(i));
    //        }
    //    }
    //}

    //public static void StopParticle(Transform particleTransform, List<Renderer> allRenderers, List<ParticleSystem> allParticleSystems)
    //{
    //    if (particleTransform == null)
    //        return;

    //    if (allRenderers == null  && allParticleSystems == null)
    //    {
    //        StopParticle(particleTransform);
    //        return;
    //    }



    //    if (allRenderers != null && allRenderers.Count > 0)
    //    {
    //        List<Renderer>.Enumerator rendererEnumerator = allRenderers.GetEnumerator();
    //        while (rendererEnumerator.MoveNext())
    //        {
    //            Renderer renderer = rendererEnumerator.Current;

    //            if (renderer == null) continue;

    //            if (renderer is MeshRenderer)
    //            {
    //                renderer.enabled = false;
    //                if (particleTransform.gameObject.GetComponent<NcSpriteAnimation>() != null)
    //                {
    //                    particleTransform.gameObject.GetComponent<NcSpriteAnimation>().enabled = false;
    //                }
    //            }

    //            if (renderer is SkinnedMeshRenderer)
    //            {
    //                renderer.enabled = false;
    //            }

    //            if (renderer is TrailRenderer)
    //            {
    //                renderer.enabled = false;

    //            }
    //        }
    //    }

    //    if (allParticleSystems != null && allParticleSystems.Count > 0)
    //    {
    //        List<ParticleSystem>.Enumerator particleEnumerator = allParticleSystems.GetEnumerator();
    //        while (particleEnumerator.MoveNext())
    //        {
    //            ParticleSystem particle = particleEnumerator.Current;

    //            if (particle == null) continue;

    //            particle.Stop(true);
    //            particle.enableEmission = false;
    //            particle.Clear(true);
    //        }
    //    }

    //    particleTransform.gameObject.SetActive(false);
    //}

    //public static void FadeOutParticle(Transform particleTransform)
    //{
    //    if (particleTransform == null)
    //        return;

    //    Renderer renderer = particleTransform.GetComponent<Renderer>();

    //    if (renderer is MeshRenderer)
    //    {
    //        renderer.enabled = false;
    //    }

    //    if (renderer is SkinnedMeshRenderer)
    //    {
    //        renderer.enabled = false;
    //    }

    //    if (renderer is TrailRenderer)
    //    {
    //        renderer.enabled = false;
    //    }

    //    ParticleSystem particle = particleTransform.GetComponent<ParticleSystem>();

    //    if (particle != null)
    //    {
    //        particle.enableEmission = false;
    //        ParticleSystemRenderer render = particle.renderer as ParticleSystemRenderer;
    //        if (render != null)
    //        {
    //            if (render.renderMode == ParticleSystemRenderMode.Mesh || (particle.simulationSpace == ParticleSystemSimulationSpace.Local && render.renderMode == ParticleSystemRenderMode.Stretch))
    //            {
    //                 particle.renderer.enabled = false;
    //            }
    //        }
    //    }

        
    //    int transChildCount = particleTransform.childCount;

    //    if (transChildCount != 0)
    //    {
    //        for (int i = 0; i < transChildCount; i++)
    //        {
    //            FadeOutParticle(particleTransform.GetChild(i));
    //        }
    //    }
    //}

    //public static void FadeOutParticle(Transform particleTransform, List<Renderer> allRenderers, List<ParticleSystem> allParticleSystems)
    //{
    //    if (particleTransform == null)
    //        return;

    //    if (allRenderers == null && allParticleSystems == null)
    //    {
    //        FadeOutParticle(particleTransform);
    //        return;
    //    }



    //    if (allRenderers != null && allRenderers.Count > 0)
    //    {
    //        List<Renderer>.Enumerator rendererEnumerator = allRenderers.GetEnumerator();
    //        while (rendererEnumerator.MoveNext())
    //        {
    //            Renderer renderer = rendererEnumerator.Current;

    //            if (renderer == null) continue;

    //            if (renderer is MeshRenderer)
    //            {
    //                renderer.enabled = false;
    //            }

    //            if (renderer is SkinnedMeshRenderer)
    //            {
    //                renderer.enabled = false;
    //            }

    //            if (renderer is TrailRenderer)
    //            {
    //                renderer.enabled = false;
    //            }
    //        }
    //    }

    //    if (allParticleSystems != null && allParticleSystems.Count > 0)
    //    {
    //        List<ParticleSystem>.Enumerator particleEnumerator = allParticleSystems.GetEnumerator();
    //        while (particleEnumerator.MoveNext())
    //        {
    //            ParticleSystem particle = particleEnumerator.Current;

    //            if (particle == null) continue;

    //            particle.enableEmission = false;
    //            ParticleSystemRenderer render = particle.renderer as ParticleSystemRenderer;
    //            if (render != null)
    //            {
    //                if (render.renderMode == ParticleSystemRenderMode.Mesh || (particle.simulationSpace == ParticleSystemSimulationSpace.Local && render.renderMode == ParticleSystemRenderMode.Stretch))
    //                {
    //                    particle.renderer.enabled = false;
    //                }
    //            }
    //        }
    //    }

    //}


    public static Transform SplitGroundEffect(Transform root)
    {
        Transform result=root.Find("Ground");
        if (result != null)
        {
            result.parent = null;
        }
        return result;
    }


    public static Transform FindGroundEffect(Transform root)
    {
        Transform result = root.Find("Ground");
        return result;
    }

    public static List<Renderer> GetAllRenderer(Transform transform, List<Renderer> rendererList)
    {
        if (transform == null)
            return rendererList;

        Renderer tmpRenderer = transform.GetComponent<Renderer>();

        if (tmpRenderer != null)
        {
            rendererList.Add(tmpRenderer);
        }

        int transChildCount = transform.childCount;

        if (transChildCount != 0)
        {
            for (int i = 0; i < transChildCount; i++)
            {
                GetAllRenderer(transform.GetChild(i), rendererList);
            }
        }

        return rendererList;
    }


    public static List<SkinnedMeshRenderer> GetAllSkinnedMeshRenderer(Transform transform, List<SkinnedMeshRenderer> rendererList)
    {
        if (transform == null)
            return rendererList;

        SkinnedMeshRenderer tmpRenderer = transform.GetComponent<SkinnedMeshRenderer>();

        if (tmpRenderer != null)
        {
            rendererList.Add(tmpRenderer);
        }

        int transChildCount = transform.childCount;

        if (transChildCount != 0)
        {
            for (int i = 0; i < transChildCount; i++)
            {
                GetAllSkinnedMeshRenderer(transform.GetChild(i), rendererList);
            }
        }

        return rendererList;
    }


    public static List<Animator> GetAllAnimator(Transform transform, List<Animator> animatorList)
    {
        if (transform == null)
            return animatorList;

        Animator tmpAnimator = transform.GetComponent<Animator>();

        if (tmpAnimator != null)
        {
            animatorList.Add(tmpAnimator);
        }

        int transChildCount = transform.childCount;

        if (transChildCount != 0)
        {
            for (int i = 0; i < transChildCount; i++)
            {
                GetAllAnimator(transform.GetChild(i), animatorList);
            }
        }

        return animatorList;
    }

    public static List<Animation> GetAllAnimation(Transform transform, List<Animation> animationList)
    {
        if (transform == null)
            return animationList;

        Animation tmpAnimation = transform.GetComponent<Animation>();

        if (tmpAnimation != null)
        {
            animationList.Add(tmpAnimation);
        }

        int transChildCount = transform.childCount;

        if (transChildCount != 0)
        {
            for (int i = 0; i < transChildCount; i++)
            {
                GetAllAnimation(transform.GetChild(i), animationList);
            }
        }

        return animationList;
    }


    public static List<ParticleSystem> GetAllParticleSystem(Transform transform, List<ParticleSystem> particleSystemsList)
    {
        if (transform == null)
            return particleSystemsList;

        ParticleSystem tmpParticleSystem = transform.GetComponent<ParticleSystem>();

        if (tmpParticleSystem != null)
        {
            particleSystemsList.Add(tmpParticleSystem);
        }

        int transChildCount = transform.childCount;

        if (transChildCount != 0)
        {
            for (int i = 0; i < transChildCount; i++)
            {
                GetAllParticleSystem(transform.GetChild(i), particleSystemsList);
            }
        }

        return particleSystemsList;
    }


    //返回值为[1, iMax]
    private static int GetSimpleRandWithSeed(int uiSeed, int iMax = 100)
    {

#if false
          //http://stackoverflow.com/questions/167735/fast-pseudo-random-number-generator-for-procedural-content
          uiSeed = (uiSeed ^ 61) ^ (uiSeed >> 16);
          uiSeed = uiSeed + (uiSeed << 3);
          uiSeed = uiSeed ^ (uiSeed >> 4);
          uiSeed = uiSeed * 0x27d4eb2d;
          uiSeed = uiSeed ^ (uiSeed >> 15);
#else
        //http://burtleburtle.net/bob/hash/integer.html
        uiSeed -= (uiSeed << 6);
        uiSeed ^= (uiSeed >> 17);
        uiSeed -= (uiSeed << 9);
        uiSeed ^= (uiSeed << 4);
        uiSeed -= (uiSeed << 3);
        uiSeed ^= (uiSeed << 10);
        uiSeed ^= (uiSeed >> 15);
#endif
        return uiSeed % iMax + 1;
    }

    //本地种子，返回[1,iMax]
    public static int GetRandWithSeed(int seed, int iMax = 100)
    {
        int iResult;
        iResult = getRandon().Next(iMax);
        return iResult;

    }

    public static int GetRandWithMax(int max = 100)
    {
        int iResult;
        iResult = getRandon().Next(max);
        return iResult;       
    }

    public static int GetRandWithMinMax(int min, int max)
    {
        int iResult;
        iResult = getRandon().Next(min, max);
        return iResult;
    }

    private static Random _ran;
    private static Random getRandon()
    {
        if (_ran == null)
        {
            //int iSeed = 10;
            long tick = DateTime.Now.Ticks;
            _ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
        }
        return _ran;
    }

    public static string GetStreamingAssetsPath()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        return "file://" + Application.dataPath + "/StreamingAssets/";
#elif UNITY_ANDROID
        return "jar:file://" + Application.dataPath + "!/assets/";
#elif UNITY_IPHONE
        return Application.dataPath + "/Raw/";
#else
        return "";
#endif
    }

    public static string ReadFileText(string filePath)
    {
        var sr = new StreamReader(filePath);

        string text = sr.ReadToEnd();

        sr.Close();
        return text;
    }

    public static byte[] ReadFileBytes(string filePath)
    {
        FileStream fileStream = File.OpenRead(filePath);

        byte[] readData = new byte[fileStream.Length];

        fileStream.Read(readData, 0, (int)fileStream.Length);

        fileStream.Close();

        return readData;
    }

//     public static void SimulateParticle( Transform particleTransform , float simulateTime , AnimatorUpdateMode updateMode , float timeFromStart)
//     {
//         if (particleTransform == null)
//             return;
// 
//         //particle
//         ParticleSystem particle = particleTransform.particleSystem;
// 
//         if (particle != null)
//         {
//             //particle.enableEmission = true;
//             particle.randomSeed = 1;
//             if (updateMode == AnimatorUpdateMode.UnscaledTime)
//             {
//                 particle.Simulate(simulateTime, false, false);
//             }
//             else
//             {
//                 particle.Simulate(simulateTime, false, true);
//             }
//         }
// #if UNITY_EDITOR
//         //animator
//         Animator animator = particleTransform.GetComponent<Animator>();
// 
//         if (animator != null)
//         {
//             AnimationClip[] clips = AnimationUtility.GetAnimationClips(particleTransform.gameObject);
// 
//             if (clips != null &&
//                 clips.Length != 0)
//             {
//                 particleTransform.gameObject.SampleAnimation(clips[0], timeFromStart);
//             }
//         }
// 
//         Animation animation = particleTransform.gameObject.animation;
//         if (animation != null)
//         {
//             AnimationClip clip = animation.clip;
//             if (clip != null)
//             {
//                 particleTransform.gameObject.SampleAnimation(clip, timeFromStart);
//             }
//         }
// #endif
//         int transChildCount = particleTransform.childCount;
// 
//         if (transChildCount != 0)
//         {
//             for (int i = 0; i < transChildCount; i++)
//             {
//                 SimulateParticle(particleTransform.GetChild(i), simulateTime, updateMode, timeFromStart);
//             }
//         }
//     }
// 
// 
//     public static void PauseParticle(Transform particleTransform)
//     {
//         if (particleTransform == null)
//             return;
// 
//         if (particleTransform.renderer is MeshRenderer && particleTransform.gameObject.GetComponent<NcSpriteAnimation>()!=null)
//         {
//             particleTransform.gameObject.GetComponent<NcSpriteAnimation>().enabled = false;
//         }
// 
//         ParticleSystem particle = particleTransform.particleSystem;
// 
//         if (particle != null)
//         {
//             particle.Pause();
//         }
//         //animator
//         Animator animator = particleTransform.GetComponent<Animator>();
// 
//         if (animator != null)
//         {
//             animator.speed=0;
//         }
//         int transChildCount = particleTransform.childCount;
// 
//         if (transChildCount != 0)
//         {
//             for (int i = 0; i < transChildCount; i++)
//             {
//                 PauseParticle(particleTransform.GetChild(i));
//             }
//         }
//     }
// 
//     public static void PauseParticle(Transform particleTransform, List<Renderer> allRenderers,
//         List<Animator> allAnimators, List<ParticleSystem> allParticleSystems)
//     {
//         if (particleTransform == null)
//             return;
// 
//         if (allRenderers == null && allAnimators == null && allParticleSystems==null)
//         {
//             PauseParticle(particleTransform);
//             return;
//         }
// 
// 
//         if (allRenderers != null && allRenderers.Count > 0)
//         {
//             List<Renderer>.Enumerator rendererEnumerator = allRenderers.GetEnumerator();
//             while (rendererEnumerator.MoveNext())
//             {
//                 Renderer renderer = rendererEnumerator.Current;
// 
//                 if (renderer == null) continue;
// 
//                 if (renderer is MeshRenderer && renderer.gameObject.GetComponent<NcSpriteAnimation>() != null)
//                 {
//                     renderer.gameObject.GetComponent<NcSpriteAnimation>().enabled = false;
//                 }
//             }
//         }
// 
//         if (allAnimators != null && allAnimators.Count > 0)
//         {
//             List<Animator>.Enumerator animatorEnumerator = allAnimators.GetEnumerator();
//             while (animatorEnumerator.MoveNext())
//             {
//                 Animator animator = animatorEnumerator.Current;
// 
//                 if (animator == null) continue;
// 
//                 animator.speed = 0;
//             }
//         }
// 
//         if (allParticleSystems != null && allParticleSystems.Count > 0)
//         {
//             List<ParticleSystem>.Enumerator particleEnumerator = allParticleSystems.GetEnumerator();
//             while (particleEnumerator.MoveNext())
//             {
//                 ParticleSystem particle = particleEnumerator.Current;
// 
//                 if (particle == null) continue;
// 
//                 particle.Pause();
//             }
//         }
//     }
// 
//     public static void ResumeParticle(Transform particleTransform)
//     {
//         if (particleTransform == null)
//             return;
// 
// 
//         if (particleTransform.renderer is MeshRenderer && particleTransform.gameObject.GetComponent<NcSpriteAnimation>() != null)
//         {
//             particleTransform.gameObject.GetComponent<NcSpriteAnimation>().enabled = true;
//         }
// 
// 
//         //particle
//         ParticleSystem particle = particleTransform.particleSystem;
// 
//         if (particle != null)
//         {
//             particle.Play();
//         }
//         //animator
//         Animator animator = particleTransform.GetComponent<Animator>();
// 
//         if (animator != null)
//         {
//             animator.speed = 1;
//         }
//         int transChildCount = particleTransform.childCount;
// 
//         if (transChildCount != 0)
//         {
//             for (int i = 0; i < transChildCount; i++)
//             {
//                 ResumeParticle(particleTransform.GetChild(i));
//             }
//         }
//     }
// 
//     public static void ResumeParticle(Transform particleTransform, List<Renderer> allRenderers,
//     List<Animator> allAnimators, List<ParticleSystem> allParticleSystems)
//     {
//         if (particleTransform == null)
//             return;
// 
//         if (allRenderers == null && allAnimators == null && allParticleSystems == null)
//         {
//             ResumeParticle(particleTransform);
//             return;
//         }
// 
// 
//         if (allRenderers != null && allRenderers.Count > 0)
//         {
//             List<Renderer>.Enumerator rendererEnumerator = allRenderers.GetEnumerator();
//             while (rendererEnumerator.MoveNext())
//             {
//                 Renderer renderer = rendererEnumerator.Current;
// 
//                 if (renderer == null) continue;
// 
//                 if (renderer is MeshRenderer && renderer.gameObject.GetComponent<NcSpriteAnimation>() != null)
//                 {
//                     renderer.gameObject.GetComponent<NcSpriteAnimation>().enabled = true;
//                 }
//             }
//         }
// 
//         if (allAnimators != null && allAnimators.Count > 0)
//         {
//             List<Animator>.Enumerator animatorEnumerator = allAnimators.GetEnumerator();
//             while (animatorEnumerator.MoveNext())
//             {
//                 Animator animator = animatorEnumerator.Current;
// 
//                 if (animator == null) continue;
// 
//                 animator.speed = 1;
//             }
//         }
// 
//         if (allParticleSystems != null && allParticleSystems.Count > 0)
//         {
//             List<ParticleSystem>.Enumerator particleEnumerator = allParticleSystems.GetEnumerator();
//             while (particleEnumerator.MoveNext())
//             {
//                 ParticleSystem particle = particleEnumerator.Current;
// 
//                 if (particle == null) continue;
// 
//                 particle.Play();
//             }
//         }
//     }

    public static void SetParent(Transform selfTransform, Transform parentTransform)
    {
        selfTransform.parent = parentTransform;
        selfTransform.localPosition = Vector3.zero;
        selfTransform.localRotation = Quaternion.identity;
        selfTransform.localScale = Vector3.one;
    }

    // 根据名称查找子结点
    public static Transform FindBoneTransform(Transform root, string boneName)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name.Equals(boneName))
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform t = root.GetChild(i);

            t = FindBoneTransform(t, boneName);
            if (t != null)
            {
                return t;
            }
        }

        return null;
    }

    //子节点同步到父节点位置
    public static void ApplyParentPosition( Transform childTransform )
    {
        if (childTransform == null ||
            childTransform.parent == null)
        {
            return;
        }

        Transform parentTransform = childTransform.parent;

        ////swap and apply position
        //childTransform.parent = parentTransform.parent;
        //parentTransform.parent = childTransform;
        //parentTransform.localPosition = Vector3.zero;

        ////swape Back
        //parentTransform.parent = childTransform.parent;
        //childTransform.parent = parentTransform;
        Vector3 tmpPosition; 
        if (parentTransform.parent != null)
        {
            tmpPosition = parentTransform.parent.InverseTransformPoint(parentTransform.TransformPoint(childTransform.localPosition));
        }
        else
        {
            tmpPosition = parentTransform.TransformPoint(childTransform.localPosition);
        }
        parentTransform.localPosition = tmpPosition;
        childTransform.localPosition = Vector3.zero;

    }




    //获得一个枚举的描述信息
    public static string GetEnumDescription(Enum value)
    {
        FieldInfo fi = value.GetType().GetField(value.ToString());
        DescriptionAttribute[] attributes =
            (DescriptionAttribute[]) fi.GetCustomAttributes(
                typeof (DescriptionAttribute), false);
        return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
    }

    public static T GetCustomAttribute<T>(Enum source) where T : Attribute
    {
        Type sourceType = source.GetType();
        if (sourceType == null)
        {
            return null;
        }

        string sourceName = Enum.GetName(sourceType, source);
        if (sourceName == null)
        {
            return null;
        }

        FieldInfo field = sourceType.GetField(sourceName);
        object[] attributes = field.GetCustomAttributes(typeof (T), true);
        foreach (object attribute in attributes)
        {
            if (attribute is T)
            {
                return (T)attribute;
            }
        }

        return null;
    }

    public static T GetCustomAttribute<T>(Enum source, int index) where T : Attribute
    {
        Type type = source.GetType();
        string fieldName = Enum.GetName(type, source);
        System.Reflection.FieldInfo info = type.GetField(fieldName);
        object[] attributes = info.GetCustomAttributes(typeof(T), false);
        if (attributes != null && attributes.Length > index)
        {
            T attribute = attributes[index] as T;
            if (attribute != null)
            {
                return attribute;
            }
        }

        return null;
    }



    //是否为汉字
    public static bool IsChineseChar(char c)
    {
        return (int)c >= 0x4E00 && (int)c <= 0x9FA5;
    }

    //截取一定字节长度的字符串
    public static string GetSubString(string str, int n)
    {
        string temp = string.Empty;
        if (System.Text.Encoding.Default.GetByteCount(str) <= n)
        {
            return str;
        }
        else
        {
            int t = 0;
            char[] q = str.ToCharArray();
            for (int i = 0; i < q.Length; i++)
            {
                if ((int)q[i] >= 0x4E00 && (int)q[i] <= 0x9FA5)
                {
                    temp += q[i];
                    t += 2;
                }
                else
                {
                    temp += q[i];
                    t += 1;
                }
                if (t >= n)
                {
                    return temp;
                }
            }
            return temp;
        }
    }



    //sami 确保当前格子数和指定数一样多
    public static void EnsureGridCount(GameObject ObjSeed, List<GameObject> gridList, int num)
    {
        int i;
        int wantNum = num;
        int gridNum = gridList.Count;

        if (gridNum == wantNum)
        {
            return;
        }
        //格子数多了，要删除。。。
        if (gridNum > wantNum)
        {
            for (i = gridNum - 1; i >= wantNum; i--)
            {
                if (i == 0)
                {
                    gridList[i].SetActive(false);
                }
                else
                {
                    GameObject.Destroy(gridList[i]);
                }
                gridList.RemoveAt(i);
            }
        }
        //格子少了，要创建。。。
        if (gridNum < wantNum)
        {
            ObjSeed.SetActive(true);
            for (i = gridNum; i < wantNum; i++)
            {
                if (i == 0)
                {
                    gridList.Add(ObjSeed);
                }
                else
                {
                    GameObject obj = (GameObject)GameObject.Instantiate(ObjSeed);
                    obj.name = ObjSeed.name + i;
                    obj.transform.parent = ObjSeed.transform.parent;
                    obj.transform.localScale = ObjSeed.transform.localScale;
                    obj.transform.localPosition = ObjSeed.transform.localPosition;
                    gridList.Add(obj);
                }
            }
        }
    }

    //sami 一排图标布局函数
    public static void LayoutIcons(GameObject[] icons, Vector3 centPos, int showCount, int gap, bool bVertical = false)
    {
        if (null == icons || icons.Length <= 0 || icons.Length < showCount)
        {
            return;
        }
        Vector3 tempPos = centPos;
        //Hide All
        for (int i = 0; i < icons.Length; ++i)
        {
            icons[i].SetActive(false);
        }
        //layout
        for (int i = 0; i < showCount; i++)
        {
            if (bVertical)
            {
                icons[i].transform.localPosition = new Vector3(tempPos.x, tempPos.y + (i*2+1-showCount) * gap / 2.0f, tempPos.z);
            }
            else
            {
                icons[i].transform.localPosition = new Vector3(tempPos.x + (i*2+1-showCount) * gap / 2.0f, tempPos.y, tempPos.z);
            }
            icons[i].SetActive(true);
        }
    }

   




    //相当于序列化与反序列化，但是不用借助外部文件
    //1、struct转换为Byte[]
    public static Byte[] StructToBytes(Object structure)
    {
        Int32 size = Marshal.SizeOf(structure);
        IntPtr buffer = Marshal.AllocHGlobal(size);

        try
        {
            Marshal.StructureToPtr(structure, buffer, false);
            var bytes = new Byte[size];
            Marshal.Copy(buffer, bytes, 0, size);

            return bytes;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    //2、Byte[]转换为struct
    public static Object BytesToStruct(Byte[] bytes, Type strcutType)
    {
        Int32 size = Marshal.SizeOf(strcutType);
        IntPtr buffer = Marshal.AllocHGlobal(size);

        try
        {
            Marshal.Copy(bytes, 0, buffer, size);

            return Marshal.PtrToStructure(buffer, strcutType);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    // 整数到字节数组转换
    public static Byte[] int2bytes(int n)
    {
        var ab = new Byte[4];
        ab[0] = (Byte) (0xff & n);
        ab[1] = (Byte) ((0xff00 & n) >> 8);
        ab[2] = (Byte) ((0xff0000 & n) >> 16);
        ab[3] = (Byte) ((0xff000000 & n) >> 24);
        return ab;
    }

    // 字节数组到整数的转换
    public static int bytes2int(Byte[] b)
    {
        int s = 0;
        s = ((((b[0] & 0xff) << 8 | (b[1] & 0xff)) << 8) | (b[2] & 0xff)) << 8
            | (b[3] & 0xff);
        return s;
    }


    /* cp 在此是四个元素的数组: 
        cp[0] 为起点，或上图中的 P0 
        cp[1] 为第一控制点，或上图中的 P1 
        cp[2] 为第二控制点，或上图中的 P2 
        cp[3] 为结束点，或上图中的 P3 
        t 为参数值，0 <= t <= 1 */

    public static Vector3 PointOnCubicBezier(Vector3[] cp, float t)
    {
        float ax, bx, cx;
        float ay, by, cy;
        float tSquared, tCubed;
        Vector3 result = Vector3.zero;
        /* 计算多项式系数 */
        cx = 3.0f*(cp[1].x - cp[0].x);
        bx = 3.0f*(cp[2].x - cp[1].x) - cx;
        ax = cp[3].x - cp[0].x - cx - bx;
        cy = 3.0f*(cp[1].y - cp[0].y);
        by = 3.0f*(cp[2].y - cp[1].y) - cy;
        ay = cp[3].y - cp[0].y - cy - by;
        /* 计算t位置的点值 */
        tSquared = t*t;
        tCubed = tSquared*t;
        result.x = (ax*tCubed) + (bx*tSquared) + (cx*t) + cp[0].x;
        result.y = (ay*tCubed) + (by*tSquared) + (cy*t) + cp[0].y;
        return result;
    }

    public const float EPS = 1e-5f;

    public static int CompareFloat(float f1, float f2)
    {
        float delta = f1 - f2;
        if (Mathf.Abs(delta) < EPS)
            return 0;

        if (delta > 0)
            return 1;

        return -1;
    }

    public static int FloatCompare(float a, float b)
    {
        return FloatCompare(a, b, 1e-5f);
    }

    public static int FloatCompare(float a, float b, float e)
    {
        float delta = a - b;

        if (Mathf.Abs(delta) < e)
            return 0;

        if (delta > 0)
            return 1;

        return -1;
    }

    /// <summary>
    /// Instantiate an object and add it to the specified parent.
    /// </summary>

    public static GameObject AddChild(GameObject parent, GameObject prefab)
    {
        // 改为从对象池里面拿
 //       Transform goTransform = BattleSceneManager.GetInstance().CharacterObjectPool.Spawn(prefab.transform);

//         if (null == goTransform)
//         {
//             Log.E(ELogTag.Avatar, "CommonTools::AddChild SceneLogic.GetInstance().CharacterObjectPool we find null == goTransform.");
//         }

   //     GameObject go = goTransform.gameObject;
        GameObject go = GameObject.Instantiate(prefab) as GameObject;

#if UNITY_EDITOR && !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Object");
#endif

        if (go != null && parent != null)
        {
            Transform t = go.transform;
            t.parent = parent.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            go.layer = parent.layer;
        }
        return go;
    }

    /// <summary>
    /// split comma to list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="inputString"></param>
    /// <returns></returns>
    public static List<T> ParseCommaDelimitedString<T>(string inputString)
    {
        List<T> retList = new List<T>();

        if (string.IsNullOrEmpty(inputString))
        {
            return retList;
        }

        char[] separator = { ',', '，' };
        string[] splitStrings = inputString.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
        if (splitStrings != null)
        {

            for (int i = 0; i < splitStrings.Length; i++)
            {
                T item = TryParse<T>(splitStrings[i]);
                retList.Add(item);
            }
        }

        return retList;
    }

    public static T TryParse<T>(string inValue)
    {
        try
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));

            return (T)converter.ConvertFromString(null, CultureInfo.InvariantCulture, inValue);
        }
        catch (Exception ex)
        {
            Log.E(ELogTag.AI, "TryParse failed!{0}", ex.Message);
        }

        return default(T);
    }

    // 获取GameObject的全部路径
    public static string GetGameObjectFullPath(GameObject targetObj)
    {
        string targetObjFullPath = "";
        while (null != targetObj && null != targetObj.transform)
        {
            targetObjFullPath = targetObj.name + "/" + targetObjFullPath;

            if (null != targetObj.transform.parent)
            {
                targetObj = targetObj.transform.parent.gameObject;
            }
            else
            {
                targetObj = null;
            }

        }

        return targetObjFullPath;
    }





  


    public static bool IsRobot(UInt64 ullUid)
    {
        if (ullUid != 0 && ullUid < 100000)
        {
            return true;
        }

        return false;
    }


    private static int StringToNumber(String numberStr)
    {
        numberStr.Trim();

        int targetIndex = 0;
        char[] targetChars = new char[numberStr.Length];

        char[] numberChars = numberStr.ToCharArray();
        for (int i = 0; i < numberChars.Length; i++)
        {
            char ch = numberChars[i];
            if (char.IsNumber(ch))
            {
                targetChars[targetIndex++] = ch;
            }
        }

        return Convert.ToInt32(new string(targetChars, 0, targetIndex));
    }


}

[Serializable]
public abstract class SerializeObject
{
    protected string mFileName;
    protected Stream mStream;

    public string FileName
    {
        get { return mFileName; }
        set { mFileName = value; }
    }

    public Stream stream
    {
        get { return mStream; }
        set { mStream = value; }
    }

    public virtual void CloneFrom(SerializeObject obj)
    {
    }

    public abstract void Serialize();
    public abstract void DeSerialize();
    public abstract void DeSerialize(bool fromSteam);
}

[Serializable]
public abstract class BinarySerializeObject : SerializeObject
{
    public virtual void CloneFrom(BinarySerializeObject t)
    {
        //this.CloneFrom(t);
    }

    public override void Serialize()
    {
        string filePath = Path.GetDirectoryName(mFileName);
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        var fs = new FileStream(mFileName, FileMode.Create);
        var formatter = new BinaryFormatter();
        formatter.Serialize(fs, this);
        fs.Close();
        fs.Dispose();
    }

    public override void DeSerialize()
    {
        DeSerialize(true);
    }

    public override void DeSerialize(bool fromSteam)
    {
        Stream toRead = null;
        if (fromSteam)
            toRead = mStream;
        else
            toRead = new FileStream(mFileName, FileMode.Open);

        if (toRead == null)
            return;

        var formatter = new BinaryFormatter();
        var t = (BinarySerializeObject) formatter.Deserialize(toRead);
        CloneFrom(t);

        toRead.Close();
        toRead.Dispose();
    }
}

[Serializable]
public abstract class XmlSerializeObject : SerializeObject
{
    public override void Serialize()
    {
        string filePath = Path.GetDirectoryName(mFileName);
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);

            //DirectoryInfo dirInfo = Directory.CreateDirectory(filePath);
        }

        var fs = new FileStream(mFileName, FileMode.Create);
        var formatter = new XmlSerializer(typeof (XmlSerializeObject));
        formatter.Serialize(fs, this);

        fs.Close();
        fs.Dispose();
    }

    public override void DeSerialize()
    {
        DeSerialize(false);
    }

    public virtual void CloneFrom(XmlSerializeObject t)
    {
    }

    public override void DeSerialize(bool fromSteam)
    {
        Stream toRead = null;
        if (fromSteam)
            toRead = mStream;
        else
            toRead = new FileStream(mFileName, FileMode.Open);

        if (toRead == null)
            return;

        var formatter = new XmlSerializer(typeof (XmlSerializeObject));
        var t = (XmlSerializeObject) formatter.Deserialize(toRead);
        toRead.Close();
        toRead.Dispose();

        CloneFrom(t);
    }

    public static T GetCustomAttribute<T>(Enum source) where T : Attribute
    {
        Type sourceType = source.GetType();
        if (sourceType == null)
        {
            return null;
        }

        string sourceName = Enum.GetName(sourceType, source);
        if (sourceName == null)
        {
            return null;
        }

        FieldInfo field = sourceType.GetField(sourceName);
        object[] attributes = field.GetCustomAttributes(typeof(T), true);
        foreach (object attribute in attributes)
        {
            if (attribute is T)
            {
                return (T)attribute;
            }
        }

        return null;
    }


   
}

