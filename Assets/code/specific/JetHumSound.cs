using UnityEngine;
using System.Collections;

public class JetHumSound : MonoBehaviour {

    public float baseVolume = 1;

    void Awake() {
        source = GetComponent<AudioSource>();
	}
	
	void Update() {
		
        if (source.isPlaying) {

            source.volume = baseVolume * SoundManager.instance.volumeScale * Vars.sfxVolume;

            if (TimeUser.reverting ||
                Time.timeScale == 0) {
                source.Stop();
            }
        } else {
            if (!TimeUser.reverting &&
                Time.timeScale > 0) {
                source.Play();
            }
        }
        

	}

    AudioSource source;
    
}
