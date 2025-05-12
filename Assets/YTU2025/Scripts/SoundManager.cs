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
    
    [Header("Ambient Settings")]
    [SerializeField] private string ambientSoundName = "forestAmbient";
    [SerializeField] private float ambientFadeInTime = 2f;
    [SerializeField] private float ambientVolume = 0.5f;
    
    [Header("Time Scale Independence")]
    [SerializeField] private bool makeTimeScaleIndependent = true;
    
    [Header("Default Sounds")]
    [SerializeField] private AudioClip defaultGunshot;
    [SerializeField] private AudioClip defaultReload;
    [SerializeField] private AudioClip defaultRabbitDeath;
    [SerializeField] private AudioClip defaultScream;
    [SerializeField] private AudioClip defaultSiren;
    [SerializeField] private AudioClip defaultForestAmbient;
    [SerializeField] private AudioClip defaultFootstep;
    
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
        
        // Add default sounds if needed
        AddDefaultSoundsIfMissing();
        
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
    
    void Start()
    {
        // Start playing ambient sound with fade in
        if (!string.IsNullOrEmpty(ambientSoundName))
        {
            Sound ambientSound = System.Array.Find(sounds, sound => sound.name == ambientSoundName);
            if (ambientSound != null)
            {
                ambientSound.source.volume = 0f;
                ambientSound.source.Play();
                
                // Fade in the ambient sound
                DOTween.To(() => ambientSound.source.volume, x => ambientSound.source.volume = x, ambientVolume, ambientFadeInTime)
                    .SetUpdate(true);
            }
        }
    }
    
    private void AddDefaultSoundsIfMissing()
    {
        // Create a list from existing sounds array
        List<Sound> soundsList = new List<Sound>(sounds != null ? sounds : new Sound[0]);
        
        // Check and add default gunshot sound
        AddDefaultSoundIfMissing(soundsList, "gunshot", defaultGunshot, 0.7f, 1f, false);
        
        // Check and add default reload sound
        AddDefaultSoundIfMissing(soundsList, "reload", defaultReload, 0.6f, 1f, false);
        
        // Check and add default rabbit death sound
        AddDefaultSoundIfMissing(soundsList, "rabbitDeath", defaultRabbitDeath, 0.8f, 1f, false);
        
        // Check and add default scream sound
        AddDefaultSoundIfMissing(soundsList, "scream", defaultScream, 1f, 1f, false);
        
        // Check and add default siren sound
        AddDefaultSoundIfMissing(soundsList, "siren", defaultSiren, 0.7f, 1f, true);
        
        // Check and add default forest ambient sound
        AddDefaultSoundIfMissing(soundsList, "forestAmbient", defaultForestAmbient, ambientVolume, 1f, true);
        
        // Check and add default footstep sound
        AddDefaultSoundIfMissing(soundsList, "footstep", defaultFootstep, 0.5f, 1f, false);
        
        // Update the sounds array
        sounds = soundsList.ToArray();
    }
    
    private void AddDefaultSoundIfMissing(List<Sound> soundsList, string name, AudioClip defaultClip, float volume, float pitch, bool loop)
    {
        // Check if sound already exists
        bool soundExists = System.Array.Exists(sounds, sound => sound.name == name);
        
        // If sound doesn't exist and we have a default clip, add it
        if (!soundExists && defaultClip != null)
        {
            Sound newSound = new Sound
            {
                name = name,
                clip = defaultClip,
                volume = volume,
                pitch = pitch,
                loop = loop
            };
            
            soundsList.Add(newSound);
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
    
    public void PlayFootstep()
    {
        // Play footstep sound with more randomization for natural feel
        Sound s = System.Array.Find(sounds, sound => sound.name == "footstep");
        
        if (s == null)
        {
            Debug.LogWarning("Footstep sound not found!");
            return;
        }
        
        // More variation for footsteps
        s.source.pitch = s.pitch * (1f + Random.Range(-0.2f, 0.2f));
        s.source.volume = s.volume * (1f + Random.Range(-0.15f, 0.15f));
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
