using UnityEditor.iOS;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public Transform BgmRoot;

    public Transform AudioRoot;


    private const string AudioFilesPath = "/RTP/Audio/";

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


    public void PlayAudioClip(string name, float volume = 1f)
    {
        AudioClip clip = Resources.Load(AudioFilesPath + name) as AudioClip;

        GameObject audio = new GameObject(name);
        AudioSource source = audio.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;

        audio.transform.parent = AudioRoot;
    }


    public void PlayBGM(string name, float volume=1f)
    {
        AudioClip clip=Resources.Load(AudioFilesPath + name) as AudioClip;
        // fade in out
        if (BgmRoot.childCount== 1)
        {
            
        }
        else
        {
            GameObject bgm=new GameObject(name);
            AudioSource source = bgm.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            bgm.transform.parent = BgmRoot;
        }

    }

}
