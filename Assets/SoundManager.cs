using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))] // more AudioSource components, more sounds can be played simultaneously
public class SoundManager : MonoBehaviour {

    public static SoundManager instance { get { return _instance; } }

    public void playSFX(AudioClip clip, float volume = 1.0f) {
        playSFXF(clip, volume, 1);
    }
    public void playSFXRandPitchBend(AudioClip clip, float pitchBendMagnitude = .05f, float volume = 1.0f) {
        float pitch = 1 + (Random.value * 2 - 1) * pitchBendMagnitude;
        playSFXF(clip, volume, pitch);
    }

	void Awake() {
        // make SoundManager a singleton
        if (instance == null)
            _instance = this;
        else if (instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        AudioSource[] ass = GetComponents<AudioSource>();

        musicSource = ass[0];
        for (int i = 1; i < ass.Length; i++) {
            sfxSources.Add(ass[i]);
        }

	}
	
	void Update() {
		
	}

    void OnDestroy() {
        foreach (AudioSource audS in sfxSources) {
            audS.clip = null;
        }
        sfxSources.Clear();
        musicSource.clip = null;
    }

    void playSFXF(AudioClip clip, float volume, float pitch) {
        if (clip == null) {
            Debug.Log("ERROR: AudioClip is null");
            return;
        }
        volume *= Vars.sfxVolume;
        AudioSource source = assignSource(clip);
        source.volume = volume;
        source.pitch = pitch;
        source.Play();
    }

    /* Finds an AudioSource from sfxSources to play the clip on.
     * AudioSources that aren't currently playing anything are prioritized. */
    AudioSource assignSource(AudioClip clip) {
        AudioSource source = sfxSources[0];
        foreach (AudioSource audS in sfxSources) {
            if (audS.clip == null || !audS.isPlaying) {
                source = audS;
                break;
            }
        }
        if (source.clip != null && !source.isPlaying) {
            source.Stop();
        }
        source.clip = clip;
        return source;
    }

    static SoundManager _instance = null;

    List<AudioSource> sfxSources = new List<AudioSource>();
    AudioSource musicSource; 

}
