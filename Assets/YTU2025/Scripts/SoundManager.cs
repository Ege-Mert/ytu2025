using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class SoundManager : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        public bool loop = false;
        [Range(0f, 1f)]
        public float spatialBlend = 0f; // 0 = 2D, 1 = 3D
        
        [HideInInspector]
        public AudioSource source;
    }
    
    [Header("Sounds")]
    public Sound[] sounds;
    
    [Header("Time Scale Independence")]
    [SerializeField] private bool makeTimeScaleIndependent = true;
    
    public static SoundManager instance;
    
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Create AudioSource components for each sound
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.spatialBlend = s.spatialBlend;
            
            // Make independent of time scale if needed
            if (makeTimeScaleIndependent)
            {
                s.source.ignoreListenerPause = true;
            }
        }
    }
    
    public void Play(string name)
    {
        // Find the sound by name
        Sound s = System.Array.Find(sounds, sound => sound.name == name);
        
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        
        // Play the sound - with pitch variation for more natural sound
        s.source.pitch = s.pitch * (1f + Random.Range(-0.1f, 0.1f));
        s.source.Play();
    }
    
    public void PlayDelayed(string name, float delay)
    {
        // Find the sound by name
        Sound s = System.Array.Find(sounds, sound => sound.name == name);
        
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        
        // Play the sound with delay
        s.source.pitch = s.pitch * (1f + Random.Range(-0.1f, 0.1f));
        s.source.PlayDelayed(delay);
    }
    
    public void Stop(string name)
    {
        // Find the sound by name
        Sound s = System.Array.Find(sounds, sound => sound.name == name);
        
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        
        // Stop the sound
        s.source.Stop();
    }
    
    public void FadeOut(string name, float duration)
    {
        // Find the sound by name
        Sound s = System.Array.Find(sounds, sound => sound.name == name);
        
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        
        // Fade out the sound
        float initialVolume = s.source.volume;
        DOTween.To(() => s.source.volume, x => s.source.volume = x, 0f, duration)
            .SetUpdate(true) // Make it independent of time scale
            .OnComplete(() => {
                s.source.Stop();
                s.source.volume = initialVolume; // Reset volume for next play
            });
    }
    
    public bool IsPlaying(string name)
    {
        // Find the sound by name
        Sound s = System.Array.Find(sounds, sound => sound.name == name);
        
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return false;
        }
        
        // Return if the sound is playing
        return s.source.isPlaying;
    }
}
