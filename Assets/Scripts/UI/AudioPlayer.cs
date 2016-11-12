using UnityEngine;
using System.Collections;

public class AudioPlayer : MonoBehaviour
{

    public AudioSource ballaudio;


    public AudioSource directionaudio;


    public void PlayBall()
    {
        if (ballaudio.isPlaying)
        {
            ballaudio.Stop();
        }
        ballaudio.Play();
    }


    public void PlayDirection()
    {
        if (directionaudio.isPlaying)
        {
            directionaudio.Stop();
        }
        directionaudio.Play();
    }
}
