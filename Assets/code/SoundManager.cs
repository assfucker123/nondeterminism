using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))] // more AudioSource components, more sounds can be played simultaneously
public class SoundManager : MonoBehaviour {

    public static SoundManager instance { get { return _instance; } }

    #region Inspector Properties

    public float volumeScale = 1; // only change this for getting a game over, etc.
    public float pausedMusicVolumeMultiplier = .5f;
    public float flashbackMusicVolumeMultiplier = .15f;
    public TextAsset musicMapper;

    #endregion

    #region SFX Functions (Public)

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

    public AudioSource getAudioSourcePlayingSFXClip(AudioClip clip) {
        if (clip == null) return null;
        foreach (AudioSource audS in sfxSources) {
            if (audS.clip == clip && audS.isPlaying) {
                return audS;
            }
        }
        return null;
    }

    #endregion

    #region Music Functions (Public)

    /// <summary>
    /// WORK HERE
    /// </summary>
    /// <param name="songName"></param>
    /// <param name="fadeDuration"></param>
    public void playMusic(string songName, float fadeDuration = .5f) {
        MusicElement me = mapper[songName];
        if (me == null) {
            Debug.LogError("ERROR: song " + songName + " does not exist");
            return;
        }
        if (currentMusicSource == 1) {
            musicElement2 = me;
            setMusicSourceF(musicSource2, me, 0, 0);
            currentMusicSource = 2;
            loopCounter2 = 0;
            // fade out source 1, fade in source 2
            fadeOutF(1, fadeDuration);
            fadeInF(2, fadeDuration);
        } else {
            musicElement1 = me;
            setMusicSourceF(musicSource1, me, 0, 0);
            currentMusicSource = 1;
            loopCounter1 = 0;
            // fade out source 2, fade in source 1
            fadeOutF(2, fadeDuration);
            fadeInF(1, fadeDuration);
        }
    }

    public void stopMusic() {
        musicSource1.Stop();
        musicElement1 = null;
        musicSource2.Stop();
        musicElement2 = null;
    }

    public string currentMusic {
        get {
            if (currentMusicSource == 1) {
                return musicElement1 == null ? "" : musicElement1.keyName;
            } else {
                return musicElement2 == null ? "" : musicElement2.keyName;
            }
        }
    }

    /// <summary>
    /// Maps the music names in musicMapper to the filenames of the songs.
    /// To designate a file to be the intro of a song, the end of its map name should be "-intro"
    /// </summary>
    public void mapMusicElements() {
        if (musicElementsMapped) return;
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
        for (int i = 0; i < keys.Count; i++) {
            mapper[keys[i]].intMapperIndex = i;
            intMapper.Add(keys[i]);
        }
        musicElementsMapped = true;
    }

    #endregion

    
    #region Event Functions

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
    
    void Start() {
        mapMusicElements();
    }

    void Update() {

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            playMusic("battle");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            stopMusic();
        }

        if (TimeUser.reverting) {
            // quieter music during a flashback
            musicSource1.pitch = -1;
            musicSource2.pitch = -1;
            musicSource1.volume = flashbackMusicVolumeMultiplier * Vars.musicVolume;
            musicSource2.volume = flashbackMusicVolumeMultiplier * Vars.musicVolume;
            // looping in reverse, detect going to intro (this doesn't work great hopefully nobody will notice)
            if (musicSource1.clip != null && musicSource1.loop && musicSource1.time < .03f) {
                if (loopCounter1 > 0) {
                    musicSource1.time = musicSource1.clip.length - .0001f;
                    loopCounter1--;
                } else if (musicElement1 != null && musicElement1.hasIntro) {
                    musicSource1.clip = musicElement1.intro;
                    musicSource1.loop = false;
                    musicSource1.Play();
                    musicSource1.time = musicSource1.clip.length - .0001f;
                    musicSource1.pitch = -1;
                }
            }
            if (musicSource2.clip != null && musicSource2.loop && musicSource2.time < .03f) {
                if (loopCounter2 > 0) {
                    musicSource2.time = musicSource2.clip.length - .0001f;
                    loopCounter2--;
                } else if (musicElement2 != null && musicElement2.hasIntro) {
                    musicSource2.clip = musicElement2.intro;
                    musicSource2.loop = false;
                    musicSource2.Play();
                    musicSource2.time = musicSource2.clip.length - .0001f;
                    musicSource2.pitch = -1;
                }
            }
            // flag
            if (!timeRevertingFlag) {
                timeRevertingFlag = true;
            }
            return;
        }

        float expectedVol = 1;
        if (PauseScreen.instance != null && PauseScreen.paused)
            expectedVol = pausedMusicVolumeMultiplier;

        if (timeRevertingFlag) { // if just stopped time reverting
            musicSource1.pitch = 1;
            musicSource2.pitch = 1;
            setMusicSourceF(musicSource1, musicElement1, musicTime1, musicVolMultiplier1);
            setMusicSourceF(musicSource2, musicElement2, musicTime2, musicVolMultiplier2);
            timeRevertingFlag = false;
        }

        // going from intro to main loop
        if (musicElement1 != null && musicElement1.hasIntro && musicSource1.clip == musicElement1.intro && (!musicSource1.isPlaying || musicSource1.time > musicSource1.clip.length - .02f)) {
            musicSource1.clip = musicElement1.main;
            musicSource1.loop = true;
            musicSource1.Play();
        } else if (musicSource1.clip != null && musicSource1.loop && musicSource1.time > musicSource1.clip.length - .02f) {
            // manual looping for counter
            musicSource1.time = 0;
            loopCounter1++;
        }
        if (musicElement2 != null && musicElement2.hasIntro && musicSource2.clip == musicElement2.intro && (!musicSource2.isPlaying || musicSource2.time > musicSource2.clip.length - .02f)) {
            musicSource2.clip = musicElement2.main;
            musicSource2.loop = true;
            musicSource2.Play();
        } else if (musicSource2.clip != null && musicSource2.loop && musicSource2.time > musicSource2.clip.length - .02f) {
            // manual looping for counter
            musicSource2.time = 0;
            loopCounter2++;
        }

        // fading
        switch (fadeMode1) {
        case FadeMode.FADE_IN:
            fadeTime1 += Time.unscaledDeltaTime;
            musicVolMultiplier1 = Utilities.easeLinearClamp(fadeTime1, 0, 1, fadeDuration1);
            musicSource1.volume = expectedVol * musicVolMultiplier1 * Vars.musicVolume;
            if (fadeTime1 >= fadeDuration1) {
                fadeMode1 = FadeMode.NOT_FADING;
            }
            break;
        case FadeMode.FADE_OUT:
            fadeTime1 += Time.unscaledDeltaTime;
            musicVolMultiplier1 = Utilities.easeLinearClamp(fadeTime1, 1, -1, fadeDuration1);
            musicSource1.volume = expectedVol * musicVolMultiplier1 * Vars.musicVolume;
            if (fadeTime1 >= fadeDuration1) {
                fadeMode1 = FadeMode.NOT_FADING;
            }
            break;
        case FadeMode.NOT_FADING:
            musicSource1.volume = expectedVol * musicVolMultiplier1 * Vars.musicVolume;
            break;
        }
        switch (fadeMode2) {
        case FadeMode.FADE_IN:
            fadeTime2 += Time.unscaledDeltaTime;
            musicVolMultiplier2 = Utilities.easeLinearClamp(fadeTime2, 0, 1, fadeDuration2);
            musicSource2.volume = expectedVol * musicVolMultiplier2 * Vars.musicVolume;
            if (fadeTime2 >= fadeDuration2) {
                fadeMode2 = FadeMode.NOT_FADING;
            }
            break;
        case FadeMode.FADE_OUT:
            fadeTime2 += Time.unscaledDeltaTime;
            musicVolMultiplier2 = Utilities.easeLinearClamp(fadeTime2, 1, -1, fadeDuration2);
            musicSource2.volume = expectedVol * musicVolMultiplier2 * Vars.musicVolume;
            if (fadeTime2 >= fadeDuration2) {
                fadeMode2 = FadeMode.NOT_FADING;
            }
            break;
        case FadeMode.NOT_FADING:
            musicSource2.volume = expectedVol * musicVolMultiplier2 * Vars.musicVolume;
            break;
        }

        // update time values
        musicTime1 = musicSource1.time;
        if (musicElement1 != null && musicElement1.hasIntro && musicSource1.clip == musicElement1.main)
            musicTime1 += musicElement1.intro.length;
        musicTime2 = musicSource2.time;
        if (musicElement2 != null && musicElement2.hasIntro && musicSource2.clip == musicElement2.main)
            musicTime2 += musicElement2.intro.length;

    }

    void OnSaveFrame(FrameInfo fi) {
        fi.ints["me1"] = musicElement1 == null ? -1 : musicElement1.intMapperIndex;
        fi.floats["mt1"] = musicTime1;
        fi.floats["mvm1"] = musicVolMultiplier1;
        fi.ints["fm1"] = (int)fadeMode1;
        fi.floats["ft1"] = fadeTime1;
        fi.floats["fd1"] = fadeDuration1;
        fi.ints["lc1"] = loopCounter1;
        fi.ints["me2"] = musicElement2 == null ? -1 : musicElement2.intMapperIndex;
        fi.floats["mt2"] = musicTime2;
        fi.floats["mvm2"] = musicVolMultiplier2;
        fi.ints["fm2"] = (int)fadeMode2;
        fi.floats["ft2"] = fadeTime2;
        fi.floats["fd2"] = fadeDuration2;
        fi.ints["lc2"] = loopCounter2;
    }

    void OnRevert(FrameInfo fi) {
        fadeMode1 = (FadeMode)fi.ints["fm1"];
        fadeTime1 = fi.floats["ft1"];
        fadeDuration1 = fi.floats["fd1"];
        musicTime1 = fi.floats["mt1"];
        musicVolMultiplier1 = fi.floats["mvm1"];
        loopCounter1 = fi.ints["lc1"];
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
        fadeMode2 = (FadeMode)fi.ints["fm2"];
        fadeTime2 = fi.floats["ft2"];
        fadeDuration2 = fi.floats["fd2"];
        musicTime2 = fi.floats["mt2"];
        musicVolMultiplier2 = fi.floats["mvm2"];
        loopCounter2 = fi.ints["lc2"];
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

    void OnDestroy() {
        foreach (AudioSource audS in sfxSources) {
            audS.clip = null;
        }
        sfxSources.Clear();
        musicSource1.clip = null;
        musicSource2.clip = null;
    }

    #endregion

    #region Helper SFX Functions

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

    #endregion

    #region Helper Music Functions

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
            musicSource.time = 0;
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

    void fadeInF(int musicSource, float duration) {
        if (musicSource == 1) {
            if (duration <= .001f) {
                fadeMode1 = FadeMode.NOT_FADING;
                musicVolMultiplier1 = 1;
                musicSource1.volume = musicVolMultiplier1 * Vars.musicVolume;
            } else if (fadeMode1 != FadeMode.FADE_IN) {
                fadeMode1 = FadeMode.FADE_IN;
                fadeTime1 = 0;
                fadeDuration1 = duration;
                musicVolMultiplier1 = 0;
                musicSource1.volume = 0;
            }
        } else {
            if (duration <= .001f) {
                fadeMode2 = FadeMode.NOT_FADING;
                musicVolMultiplier2 = 1;
                musicSource2.volume = musicVolMultiplier2 * Vars.musicVolume;
            } else if (fadeMode2 != FadeMode.FADE_IN) {
                fadeMode2 = FadeMode.FADE_IN;
                fadeTime2 = 0;
                fadeDuration2 = duration;
                musicVolMultiplier2 = 0;
                musicSource2.volume = 0;
            }
        }
    }

    void fadeOutF(int musicSource, float duration) {
        if (musicSource == 1) {
            if (duration <= .001f) {
                fadeMode1 = FadeMode.NOT_FADING;
                musicVolMultiplier1 = 0;
                musicSource1.volume = 0;
            } else if (fadeMode1 != FadeMode.FADE_OUT) {
                fadeMode1 = FadeMode.FADE_OUT;
                fadeTime1 = 0;
                fadeDuration1 = duration;
                musicVolMultiplier1 = 1;
                musicSource1.volume = musicVolMultiplier1 * Vars.musicVolume;
            }
        } else {
            if (duration <= .001f) {
                fadeMode2 = FadeMode.NOT_FADING;
                musicVolMultiplier2 = 0;
                musicSource2.volume = 0;
            } else if (fadeMode2 != FadeMode.FADE_OUT) {
                fadeMode2 = FadeMode.FADE_OUT;
                fadeTime2 = 0;
                fadeDuration2 = duration;
                musicVolMultiplier2 = 1;
                musicSource2.volume = musicVolMultiplier2 * Vars.musicVolume;
            }
        }
    }

    #endregion

    #region Properties

    static SoundManager _instance = null;

    enum FadeMode {
        NOT_FADING,
        FADE_IN,
        FADE_OUT
    }

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

        public bool hasIntro { get { return introFileName != ""; } }
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

    List<AudioSource> sfxSources = new List<AudioSource>();
    AudioSource musicSource1;
    MusicElement musicElement1;
    float musicTime1 = 0;
    float musicVolMultiplier1 = 1;
    FadeMode fadeMode1 = FadeMode.NOT_FADING;
    float fadeTime1 = 0;
    float fadeDuration1 = 0;
    int loopCounter1 = 0;
    AudioSource musicSource2;
    MusicElement musicElement2;
    float musicTime2 = 0;
    float musicVolMultiplier2 = 1;
    FadeMode fadeMode2 = FadeMode.NOT_FADING;
    float fadeTime2 = 0;
    float fadeDuration2 = 0;
    int loopCounter2 = 0;

    bool timeRevertingFlag = false;
    int currentMusicSource = 1;

    TimeUser timeUser;
    
    Dictionary<string, MusicElement> mapper = new Dictionary<string, MusicElement>();
    List<string> intMapper = new List<string>(); // maps int (index) to a string that can be used in mapper
    bool musicElementsMapped = false;

    #endregion


}
