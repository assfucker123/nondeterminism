using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))] // more AudioSource components, more sounds can be played simultaneously
public class SoundManager : MonoBehaviour {

    public static SoundManager instance { get { return _instance; } }

    public float volumeScale = 1; // only change this for getting a game over, etc.
    public TextAsset musicMapper;

    public void playSFX(AudioClip clip, float volume = 1.0f) {
        playSFXIgnoreVolumeScale(clip, volume * volumeScale);
    }
    public void playSFXRandPitchBend(AudioClip clip, float pitchBendMagnitude = .05f, float volume = 1.0f) {
        playSFXRandPitchBendIgnoreVolumeScale(clip, pitchBendMagnitude, volume * volumeScale);
    }
    public void playSFXIfOnScreen(AudioClip clip, Vector2 position, float volume = 1.0f) {
        if (!CameraControl.pointContainedInScreen(position)) return;
        playSFX(clip, volume);
    }
    public void playSFXIfOnScreenRandPitchBend(AudioClip clip, Vector2 position, float pitchBendMagnitude = .05f, float volume = 1.0f) {
        if (!CameraControl.pointContainedInScreen(position)) return;
        playSFXRandPitchBend(clip, pitchBendMagnitude, volume);
    }
    public void playSFXIgnoreVolumeScale(AudioClip clip, float volume = 1.0f) {
        playSFXF(clip, volume, 1);
    }
    public void playSFXRandPitchBendIgnoreVolumeScale(AudioClip clip, float pitchBendMagnitude = .05f, float volume = 1.0f) {
        float pitch = 1 + (Random.value * 2 - 1) * pitchBendMagnitude;
        playSFXF(clip, volume, pitch);
    }
    
    public void stopSFX(AudioClip clip) {
        if (clip == null) return;
        foreach (AudioSource audS in sfxSources) {
            if (audS.clip == null) continue;
            if (audS.clip == clip) {
                audS.Stop();
                return;
            }
        }
    }

    public bool isSFXPlaying(AudioClip clip) {
        if (clip == null) return false;
        foreach (AudioSource audS in sfxSources) {
            if (audS.clip == clip && audS.isPlaying) {
                return true;
            }
        }
        return false;
    }

    public AudioSource getAudioSourcePlayingClip(AudioClip clip) {
        if (clip == null) return null;
        foreach (AudioSource audS in sfxSources) {
            if (audS.clip == clip && audS.isPlaying) {
                return audS;
            }
        }
        return null;
    }

    /////////////
    // PRIVATE //
    /////////////

	void Awake() {
        // make SoundManager a singleton
        if (instance == null)
            _instance = this;
        else if (instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        timeUser = GetComponent<TimeUser>();

        AudioSource[] ass = GetComponents<AudioSource>();

        musicSource1 = ass[0];
        musicSource2 = ass[1];
        for (int i = 2; i < ass.Length; i++) {
            sfxSources.Add(ass[i]);
        }

	}
    
    

    void OnDestroy() {
        foreach (AudioSource audS in sfxSources) {
            audS.clip = null;
        }
        sfxSources.Clear();
        musicSource1.clip = null;
        musicSource2.clip = null;
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
        source.loop = false;
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
    AudioSource musicSource1;
    MusicElement musicElement1;
    float musicTime1 = 0;
    float musicVolMultiplier1 = 1;
    AudioSource musicSource2;
    MusicElement musicElement2;
    float musicTime2 = 0;
    float musicVolMultiplier2 = 1;
    TimeUser timeUser;
    

    void OnSaveFrame(FrameInfo fi) {
        fi.ints["me1"] = musicElement1 == null ? -1 : musicElement1.intMapperIndex;
        fi.floats["mt1"] = musicTime1;
        fi.floats["mvm1"] = musicVolMultiplier1;
        fi.ints["me2"] = musicElement2 == null ? -1 : musicElement2.intMapperIndex;
        fi.floats["mt2"] = musicTime2;
        fi.floats["mvm2"] = musicVolMultiplier2;
    }

    void OnRevert(FrameInfo fi) {
        musicTime1 = fi.floats["mt1"];
        musicVolMultiplier1 = fi.floats["mvm1"];
        int index = fi.ints["me1"];
        if (index == -1) {
            musicElement1 = null;
            if (musicSource1.isPlaying) {
                musicSource1.Stop();
            }
        } else {
            musicElement1 = mapper[intMapper[index]];
            // new idea: do this in Update, right when TimeUser stops reverting
            //setMusicSourceF(musicSource1, musicElement1, musicTime1, musicVolMultiplier1);
        }
        musicTime2 = fi.floats["mt2"];
        musicVolMultiplier2 = fi.floats["mvm2"];
        index = fi.ints["me2"];
        if (index == -1) {
            musicElement2 = null;
            if (musicSource2.isPlaying) {
                musicSource2.Stop();
            }
        } else {
            musicElement2 = mapper[intMapper[index]];
            // new idea: do this in Update, right when TimeUser stops reverting
            //setMusicSourceF(musicSource2, musicElement2, musicTime2, musicVolMultiplier2);
        }
    }

    bool timeRevertingFlag = false;

    void Update() {

        if (timeUser.shouldNotUpdate) {
            if (TimeUser.reverting) {
                timeRevertingFlag = true;
            }
            return;
        }

        if (timeRevertingFlag) { // if just stopped time reverting
            setMusicSourceF(musicSource1, musicElement1, musicTime1, musicVolMultiplier1);
            setMusicSourceF(musicSource2, musicElement2, musicTime2, musicVolMultiplier2);
            timeRevertingFlag = false;
        }
        
        // going from intro to main loop
        if (musicElement1 != null && musicElement1.hasIntro && !musicSource1.isPlaying) {
            musicSource1.clip = musicElement1.main;
            musicSource1.loop = true;
            musicSource1.Play();
        }
        if (musicElement2 != null && musicElement2.hasIntro && !musicSource2.isPlaying) {
            musicSource2.clip = musicElement2.main;
            musicSource2.loop = true;
            musicSource2.Play();
        }

        // update time values
        musicTime1 = musicSource1.time;
        if (musicElement1 != null && musicElement1.hasIntro && musicSource1.clip == musicElement1.main)
            musicTime1 += musicElement1.intro.length;
        musicTime2 = musicSource2.time;
        if (musicElement2 != null && musicElement2.hasIntro && musicSource2.clip == musicElement2.main)
            musicTime2 += musicElement2.intro.length;

    }

    void setMusicSourceF(AudioSource musicSource, MusicElement musicElement, float time, float volumeMultiplier) {
        volumeMultiplier *= Vars.musicVolume;
        if (musicElement == null) {
            musicSource.Stop();
            musicSource.volume = volumeMultiplier;
            musicSource.clip = null;
            return;
        }
        if (!musicElement.loaded) {
            Debug.LogError("ERROR: MusicElement not loaded");
            return;
        }
        bool onIntro = musicElement.onIntro(time);
        AudioClip clip = onIntro ? musicElement.intro : musicElement.main;
        if (musicSource.clip != clip) {
            musicSource.clip = clip;
            musicSource.Play();
        }
        musicSource.volume = volumeMultiplier;
        musicSource.time = musicElement.getTime(time);
        musicSource.loop = !onIntro;
        if (!musicSource.isPlaying) {
            musicSource.Play();
            musicSource.time = musicElement.getTime(time);
        }
    }

    /// <summary>
    /// Maps the music names in musicMapper to the filenames of the songs.
    /// To designate a file to be the intro of a song, the end of its map name should be "-intro"
    /// </summary>
    public void mapMusicElements() {
        mapper.Clear();
        intMapper.Clear();
        Properties prop = new Properties(musicMapper.text);
        List<string> keys = prop.getKeys();
        foreach (string key in keys) {
            if (key == "") continue;
            int index = key.LastIndexOf("-intro");
            if (index != -1 && index == key.Length - 6) continue; // has -intro, so don't count as song

            MusicElement me;

            // check if intro exists
            string introFileName = prop.getString(key + "-intro");
            if (introFileName == "") { // no intro exists
                me = new MusicElement(key, prop.getString(key));
            } else { // intro exists
                me = new MusicElement(key, introFileName, prop.getString(key));
            }
            mapper.Add(key, me);
        }
        // give each music element an integer representation
        keys = new List<string>(mapper.Keys);
        for (int i=0; i<keys.Count; i++) {
            mapper[keys[i]].intMapperIndex = i;
            intMapper.Add(keys[i]);
        }
    }
    Dictionary<string, MusicElement> mapper = new Dictionary<string, MusicElement>();
    List<string> intMapper = new List<string>(); // maps int (index) to a string that can be used in mapper

    class MusicElement {

        public MusicElement(string keyName, string mainFileName) {
            this.keyName = keyName;
            this.mainFileName = mainFileName;
            setClips();
        }
        public MusicElement(string keyName, string introFileName, string mainFileName) {
            this.keyName = keyName;
            this.introFileName = introFileName;
            this.mainFileName = mainFileName;
            setClips();
        }
        
        public bool hasIntro {  get { return introFileName != ""; } }
        public bool loaded {
            get {
                if (main == null) return false;
                if (hasIntro) {
                    if (intro == null) return false;
                    if (intro.loadState != AudioDataLoadState.Loaded) return false;
                }
                return main.loadState == AudioDataLoadState.Loaded;
            }
        }
        /// <summary>
        /// if the song has been playing for timeSinceMusicStarted seconds, would it be on the intro or in the main?
        /// </summary>
        public bool onIntro(float timeSinceMusicStarted) {
            if (!hasIntro) return false;
            if (intro == null) return false;
            if (timeSinceMusicStarted < 0) return false;
            return timeSinceMusicStarted < intro.length;
        }
        /// <summary>
        /// if the song has been playing for timeSinceMusicStarted seconds, what should the time property of the AudioSource be?
        /// </summary>
        public float getTime(float timeSinceMusicStarted) {
            if (onIntro(timeSinceMusicStarted)) {
                return timeSinceMusicStarted;
            }
            if (main == null) return 0;
            float loopT;
            if (hasIntro && timeSinceMusicStarted >= 0) {
                loopT = timeSinceMusicStarted - intro.length;
            } else {
                loopT = timeSinceMusicStarted;
            }
            return Utilities.fmod(loopT, main.length);
        }

        public string keyName = ""; // key name is what's used when a call is made to play music
        public string introFileName = "";
        public string mainFileName = "";
        public int intMapperIndex = 0; // each MusicElement gets its own, so it acts as an ID
        public AudioClip intro;
        public AudioClip main;

        /// <summary>
        /// uses Resources.Load to set the info and main clips from their filenames
        /// </summary>
        void setClips() {
            if (intro != null) {
                intro = null;
            }
            if (main != null) {
                main = null;
            }
            if (introFileName != "") {
                intro = Resources.Load<AudioClip>(introFileName);
            }
            main = Resources.Load<AudioClip>(mainFileName);
        }

    }

}
