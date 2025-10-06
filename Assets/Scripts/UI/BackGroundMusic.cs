using UnityEngine;

public class BackGroundMusic : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public AudioClip BackgroundClip;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = BackgroundClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    
}
