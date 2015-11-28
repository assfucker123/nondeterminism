using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class OptionsPage : MonoBehaviour {

    public AudioClip switchSound;
    public TextAsset textAsset;

    public bool locked {
        get {
            return lockedTime < lockedDuration;
        }
    }
    
    public void update() {

        if (locked) {
            lockedTime += Time.unscaledDeltaTime;
            if (!locked) {
                if (setFullscreenWhenLockOver) {
                    if (QualitySettings.antiAliasing == 0) {
                        lockedTime -= Time.unscaledDeltaTime;
                        return;
                    }
                    Screen.fullScreen = true;
                    setFullscreenWhenLockOver = false;
                }
                if (setGoodQualityWhenLockOver) {
                    QualitySettings.SetQualityLevel(3, true);
                    setGoodQualityWhenLockOver = false;
                    lockedTime = 0;
                }
            }
            return;
        }

        optionsUpdate();

        bool madeSelection = false;
        GlyphBox option = null;
        if (settingSFX || settingMusic) {
            if (Keys.instance.leftPressed) {
                if (settingSFX) {
                    Vars.sfxVolume = Mathf.Max(0, Vars.sfxVolume - .2f);
                    setVolumeText(true);
                } else {
                    Vars.musicVolume = Mathf.Max(0, Vars.musicVolume - .2f);
                    setVolumeText(false);
                }
                SoundManager.instance.playSFXIgnoreVolumeScale(switchSound);
            } else if (Keys.instance.rightPressed) {
                if (settingSFX) {
                    Vars.sfxVolume = Mathf.Min(1, Vars.sfxVolume + .2f);
                    setVolumeText(true);
                } else {
                    Vars.musicVolume = Mathf.Min(1, Vars.musicVolume + .2f);
                    setVolumeText(false);
                }
                SoundManager.instance.playSFXIgnoreVolumeScale(switchSound);
            }
            if (Keys.instance.confirmPressed || Keys.instance.backPressed) {
                // exit out of setting SFX/Music
                bool temp = settingSFX;
                show(titleMode);
                if (temp)
                    setSelection(options.IndexOf(sfxVolumeText), true);
                else
                    setSelection(options.IndexOf(musicVolumeText), true);
            }
        } else {
            if (Keys.instance.confirmPressed) {
                madeSelection = true;
                option = options[selectionIndex];
            }
            if (Keys.instance.backPressed) {
                if (quitSureNoText.hasVisibleChars) {
                    madeSelection = true;
                    option = quitSureNoText;
                }
            }
        }
        if (madeSelection) { // pressed selection
            if (option == resumeText) {
                // resume game
                PauseScreen.instance.unpauseGame();
            } else if (option == restartText) {
                // restart game
                PauseScreen.instance.unpauseGame();
                Vars.restartLevel();
            } else if (option == fullscreenText) {
                // toggle fullscreen

                if (Screen.fullScreen) {
                    
                    //Screen.fullScreen = false;
                    lockedTime = 0;
                    lockedDuration = 2;
                    Screen.fullScreen = false;
                    setGoodQualityWhenLockOver = true;

                } else {

                    //Screen.fullScreen = true;
                    lockedTime = 0;
                    lockedDuration = .5f;
                    QualitySettings.SetQualityLevel(6, true);
                    setFullscreenWhenLockOver = true;

                }
                
            } else if (option == sfxVolumeText) {
                // toggle sfx volue
                option.setColor(PauseScreen.DEFAULT_COLOR);
                hide();
                volumeText.makeAllCharsVisible();
                setVolumeText(true);
                options.Clear();
                settingSFX = true;

            } else if (option == musicVolumeText) {
                // toggle music volue
                option.setColor(PauseScreen.DEFAULT_COLOR);
                hide();
                volumeText.makeAllCharsVisible();
                setVolumeText(false);
                options.Clear();
                settingMusic = true;

            } else if (option == quitText) {
                if (titleMode) { // when quitText actually says BACK
                    // do nothing.  TitleScreen will detect backing out of the options page
                } else {
                    // go to quit sure mode
                    // get rid of other options
                    option.setColor(PauseScreen.DEFAULT_COLOR);
                    hide();
                    selection.enabled = true;
                    quitSureYesText.rectTransform.localPosition = options[1].rectTransform.localPosition;
                    quitSureNoText.rectTransform.localPosition = options[2].rectTransform.localPosition;
                    options.Clear();
                    // set new options
                    quitSureText.makeAllCharsVisible();
                    quitSureYesText.makeAllCharsVisible();
                    quitSureNoText.makeAllCharsVisible();
                    options.Add(quitSureYesText);
                    options.Add(quitSureNoText);
                    setSelection(1, true);
                }
            } else if (option == quitSureNoText) {
                // cancel quitting
                show(titleMode);
                setSelection(options.IndexOf(quitText), true);
            } else {
                // quit game (should quit to title screen)
                Vars.goToTitleScreen();
            }

        }

    }
	
	void Awake() {
        selection = transform.Find("Selection").GetComponent<Image>();
        resumeText = transform.Find("ResumeText").GetComponent<GlyphBox>();
        restartText = transform.Find("RestartText").GetComponent<GlyphBox>();
        fullscreenText = transform.Find("FullscreenText").GetComponent<GlyphBox>();
        sfxVolumeText = transform.Find("sfxVolumeText").GetComponent<GlyphBox>();
        musicVolumeText = transform.Find("MusicVolumeText").GetComponent<GlyphBox>();
        volumeText = transform.Find("VolumeText").GetComponent<GlyphBox>();
        quitText = transform.Find("QuitText").GetComponent<GlyphBox>();
        quitSureText = transform.Find("QuitSureText").GetComponent<GlyphBox>();
        quitSureYesText = transform.Find("QuitSureYesText").GetComponent<GlyphBox>();
        quitSureNoText = transform.Find("QuitSureNoText").GetComponent<GlyphBox>();
        propAsset = new Properties(textAsset.text);
	}

    void Start() {
        resumeText.setPlainText(propAsset.getString("resume"));
        restartText.setPlainText(propAsset.getString("restart"));
        fullscreenText.setPlainText(propAsset.getString("fullscreen"));
        sfxVolumeText.setPlainText(propAsset.getString("sfx_volume"));
        musicVolumeText.setPlainText(propAsset.getString("music_volume"));
        quitText.setPlainText(propAsset.getString("quit"));
        quitSureText.setPlainText(propAsset.getString("quit_sure"));
        quitSureYesText.setPlainText(propAsset.getString("quit_sure_yes"));
        quitSureNoText.setPlainText(propAsset.getString("quit_sure_no"));
        hide();
    }
	
	void Update() {
		
	}

    public bool selectingBack() {
        if (!titleMode) return false;
        if (!onTopMenu()) return false;
        if (options.IndexOf(quitText) == -1) return false;
        return selectionIndex == options.IndexOf(quitText);
    }
    public bool onTopMenu() {
        return quitText.hasVisibleChars;
    }

    public void show(bool titleMode) {
        this.titleMode = titleMode;
        selection.enabled = true;
        if (titleMode) {
            resumeText.makeAllCharsInvisible();
            restartText.makeAllCharsInvisible();
        } else {
            resumeText.makeAllCharsVisible();
            restartText.makeAllCharsVisible();
        }
        fullscreenText.makeAllCharsVisible();
        sfxVolumeText.makeAllCharsVisible();
        musicVolumeText.makeAllCharsVisible();
        volumeText.makeAllCharsInvisible();
        quitText.makeAllCharsVisible();
        quitSureText.makeAllCharsInvisible();
        quitSureYesText.makeAllCharsInvisible();
        quitSureNoText.makeAllCharsInvisible();
        settingSFX = false;
        settingMusic = false;
        // set options helper
        selectionImage = selection;
        selectionImageOffset.Set(0, 0);
        options.Clear();
        if (titleMode) {
            quitText.setPlainText(propAsset.getString("quit_back"));
        } else {
            quitText.setPlainText(propAsset.getString("quit"));
            options.Add(resumeText);
            options.Add(restartText);
        }
        options.Add(fullscreenText);
        options.Add(sfxVolumeText);
        options.Add(musicVolumeText);
        options.Add(quitText);
        setSelection(0, true);
    }

    public void hide() {
        selection.enabled = false;
        resumeText.makeAllCharsInvisible();
        restartText.makeAllCharsInvisible();
        fullscreenText.makeAllCharsInvisible();
        sfxVolumeText.makeAllCharsInvisible();
        musicVolumeText.makeAllCharsInvisible();
        volumeText.makeAllCharsInvisible();
        quitText.makeAllCharsInvisible();
        quitSureText.makeAllCharsInvisible();
        quitSureYesText.setColor(PauseScreen.DEFAULT_COLOR);
        quitSureYesText.makeAllCharsInvisible();
        quitSureNoText.setColor(PauseScreen.DEFAULT_COLOR);
        quitSureNoText.makeAllCharsInvisible();

        // save options
        Vars.saveSettings();
    }

    // call this during update to move the selection around
    void optionsUpdate() {
        // button presses to navigate options
        if (options.Count > 0) {
            if (Keys.instance.downPressed) {
                setSelection(selectionIndex + 1);
                SoundManager.instance.playSFXIgnoreVolumeScale(switchSound);
            } else if (Keys.instance.upPressed) {
                setSelection(selectionIndex - 1);
                SoundManager.instance.playSFXIgnoreVolumeScale(switchSound);
            }
        }
        // move selection image
        float selectionTransDur = .1f;
        if (selectionImageTime < selectionTransDur) {
            selectionImageTime += Time.unscaledDeltaTime;
            selectionImage.rectTransform.localPosition = new Vector3(
                Utilities.easeOutQuadClamp(selectionImageTime, selectionImageInitialPos.x, selectionImageFinalPos.x - selectionImageInitialPos.x, selectionTransDur),
                Utilities.easeOutQuadClamp(selectionImageTime, selectionImageInitialPos.y, selectionImageFinalPos.y - selectionImageInitialPos.y, selectionTransDur)
                );
        }
    }

    void setVolumeText(bool sfx) {
        float val = 0;
        if (sfx) val = Vars.sfxVolume;
        else val = Vars.musicVolume;
        string left = "<";
        if (val <= .0001f) left = " ";
        string right = ">";
        if (val >= .9999f) right = " ";
        int ones = Mathf.FloorToInt(val);
        string str = "" + ones;
        val -= ones;
        str += "." + Mathf.FloorToInt(val * 10 + .01f);
        if (sfx) {
            volumeText.setPlainText(propAsset.getString("sfx_volume_switch") + " " + left + " " + str + " " + right);
        } else {
            volumeText.setPlainText(propAsset.getString("music_volume_switch") + " " + left + " " + str + " " + right);
        }
    }

    // options helpers
    /* Set options, selectionImage, selectionImageOffset then call setSelection() */
    List<GlyphBox> options = new List<GlyphBox>();
    Image selectionImage = null;
    Vector2 selectionImageOffset = new Vector2();
    int selectionIndex = -1;
    float selectionImageTime = 9999;
    Vector2 selectionImageInitialPos = new Vector2();
    Vector2 selectionImageFinalPos = new Vector2();
    void setSelection(int index, bool immediately = false) {

        GlyphBox option;
        if (selectionIndex >= 0 && selectionIndex < options.Count) {
            // unselect
            option = options[selectionIndex];
            option.setColor(PauseScreen.DEFAULT_COLOR);
        }

        // select
        if (index < 0)
            index = options.Count - 1;
        else if (index >= options.Count)
            index = 0;
        if (selectionIndex >= 0 && selectionIndex < options.Count) {
            option = options[selectionIndex]; //previously selected
            selectionImageInitialPos.Set(selectionImage.rectTransform.localPosition.x, selectionImage.rectTransform.localPosition.y);
        } else {
            immediately = true;
        }
        selectionImageTime = 0;
        selectionIndex = index;
        option = options[selectionIndex];
        option.setColor(PauseScreen.SELECTED_COLOR);
        selectionImageFinalPos = selectionImageOffset + new Vector2(option.rectTransform.localPosition.x, option.rectTransform.localPosition.y);
        if (immediately) {
            selectionImageInitialPos = selectionImageFinalPos;
            selectionImage.rectTransform.localPosition = new Vector3(selectionImageFinalPos.x, selectionImageFinalPos.y);
        }
    }

    Properties propAsset;
    Image selection;
    GlyphBox resumeText;
    GlyphBox restartText; // only use in Arcade
    GlyphBox fullscreenText;
    GlyphBox sfxVolumeText;
    GlyphBox musicVolumeText;
    GlyphBox volumeText;

    GlyphBox quitText; // renamed to BACK when in title mode
    GlyphBox quitSureText;
    GlyphBox quitSureYesText;
    GlyphBox quitSureNoText;
    bool settingSFX = false;
    bool settingMusic = false;
    bool titleMode = false;

    private float lockedTime = 0;
    private float lockedDuration = 0;
    private bool setFullscreenWhenLockOver = false;
    private bool setGoodQualityWhenLockOver = false;

}
