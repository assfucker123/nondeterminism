using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class OptionsPage : MonoBehaviour {

    public AudioClip switchSound;

    public void update() {

        optionsUpdate();

        bool madeSelection = false;
        Text option = null;
        if (settingSFX || settingMusic) {
            if (Input.GetButtonDown("Left")) {
                if (settingSFX) {
                    Vars.sfxVolume = Mathf.Max(0, Vars.sfxVolume - .2f);
                    setVolumeText(true);
                } else {
                    Vars.musicVolume = Mathf.Max(0, Vars.musicVolume - .2f);
                    setVolumeText(false);
                }
                SoundManager.instance.playSFXIgnoreVolumeScale(switchSound);
            } else if (Input.GetButtonDown("Right")) {
                if (settingSFX) {
                    Vars.sfxVolume = Mathf.Min(1, Vars.sfxVolume + .2f);
                    setVolumeText(true);
                } else {
                    Vars.musicVolume = Mathf.Min(1, Vars.musicVolume + .2f);
                    setVolumeText(false);
                }
                SoundManager.instance.playSFXIgnoreVolumeScale(switchSound);
            }
            if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Fire1")) {
                // exit out of setting SFX/Music
                bool temp = settingSFX;
                show();
                if (temp)
                    setSelection(options.IndexOf(sfxVolumeText), true);
                else
                    setSelection(options.IndexOf(musicVolumeText), true);
            }
        } else {
            if (Input.GetButtonDown("Jump")) {
                madeSelection = true;
                option = options[selectionIndex];
            }
            if (Input.GetButtonDown("Fire1")) {
                if (quitSureNoText.enabled) {
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
            } else if (option == sfxVolumeText) {
                // toggle sfx volue
                option.color = PauseScreen.DEFAULT_COLOR;
                hide();
                volumeText.enabled = true;
                setVolumeText(true);
                options.Clear();
                settingSFX = true;

            } else if (option == musicVolumeText) {
                // toggle music volue
                option.color = PauseScreen.DEFAULT_COLOR;
                hide();
                volumeText.enabled = true;
                setVolumeText(false);
                options.Clear();
                settingMusic = true;

            } else if (option == quitText) {
                // go to quit sure mode
                // get rid of other options
                option.color = PauseScreen.DEFAULT_COLOR;
                hide();
                selection.enabled = true;
                quitSureYesText.rectTransform.localPosition = options[1].rectTransform.localPosition;
                quitSureNoText.rectTransform.localPosition = options[2].rectTransform.localPosition;
                options.Clear();
                // set new options
                quitSureText.enabled = true;
                quitSureYesText.enabled = true;
                quitSureNoText.enabled = true;
                options.Add(quitSureYesText);
                options.Add(quitSureNoText);
                setSelection(1, true);
            } else if (option == quitSureNoText) {
                // cancel quitting
                show();
                setSelection(options.IndexOf(quitText), true);
            } else {
                // quit game (should quit to title screen)
                #if UNITY_EDITOR
                // set the PlayMode to stop
                #else
                Application.Quit();
                #endif
            }

        }

    }
	
	void Awake() {
        selection = transform.Find("Selection").GetComponent<Image>();
        resumeText = transform.Find("ResumeText").GetComponent<Text>();
        restartText = transform.Find("RestartText").GetComponent<Text>();
        sfxVolumeText = transform.Find("sfxVolumeText").GetComponent<Text>();
        musicVolumeText = transform.Find("MusicVolumeText").GetComponent<Text>();
        volumeText = transform.Find("VolumeText").GetComponent<Text>();
        quitText = transform.Find("QuitText").GetComponent<Text>();
        quitSureText = transform.Find("QuitSureText").GetComponent<Text>();
        quitSureYesText = transform.Find("QuitSureYesText").GetComponent<Text>();
        quitSureNoText = transform.Find("QuitSureNoText").GetComponent<Text>();
	}
	
	void Update() {
		
	}


    public void show() {
        selection.enabled = true;
        resumeText.enabled = true;
        restartText.enabled = true;
        sfxVolumeText.enabled = true;
        musicVolumeText.enabled = true;
        volumeText.enabled = false;
        quitText.enabled = true;
        quitSureText.enabled = false;
        quitSureYesText.enabled = false;
        quitSureNoText.enabled = false;
        settingSFX = false;
        settingMusic = false;
        // set options helper
        selectionImage = selection;
        selectionImageOffset.Set(0, 2);
        options.Clear();
        options.Add(resumeText);
        options.Add(restartText);
        options.Add(sfxVolumeText);
        options.Add(musicVolumeText);
        options.Add(quitText);
        setSelection(0, true);
    }

    public void hide() {
        selection.enabled = false;
        resumeText.enabled = false;
        restartText.enabled = false;
        sfxVolumeText.enabled = false;
        musicVolumeText.enabled = false;
        volumeText.enabled = false;
        quitText.enabled = false;
        quitSureText.enabled = false;
        quitSureYesText.color = PauseScreen.DEFAULT_COLOR;
        quitSureYesText.enabled = false;
        quitSureNoText.color = PauseScreen.DEFAULT_COLOR;
        quitSureNoText.enabled = false;
    }

    // call this during update to move the selection around
    void optionsUpdate() {
        // button presses to navigate options
        if (options.Count > 0) {
            if (Input.GetButtonDown("Down")) {
                setSelection(selectionIndex + 1);
                SoundManager.instance.playSFXIgnoreVolumeScale(switchSound);
            } else if (Input.GetButtonDown("Up")) {
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
            volumeText.text = "SFX Volume: " + left + " " + str + " " + right;
        } else {
            volumeText.text = "Music Volume: " + left + " " + str + " " + right;
        }
    }

    // options helpers
    /* Set options, selectionImage, selectionImageOffset then call setSelection() */
    List<Text> options = new List<Text>();
    Image selectionImage = null;
    Vector2 selectionImageOffset = new Vector2();
    int selectionIndex = -1;
    float selectionImageTime = 9999;
    Vector2 selectionImageInitialPos = new Vector2();
    Vector2 selectionImageFinalPos = new Vector2();
    void setSelection(int index, bool immediately = false) {

        Text option;
        if (selectionIndex >= 0 && selectionIndex < options.Count) {
            // unselect
            option = options[selectionIndex];
            option.color = PauseScreen.DEFAULT_COLOR;
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
        option.color = PauseScreen.SELECTED_COLOR;
        selectionImageFinalPos = selectionImageOffset + new Vector2(option.rectTransform.localPosition.x, option.rectTransform.localPosition.y);
        if (immediately) {
            selectionImageInitialPos = selectionImageFinalPos;
        }
    }

    Image selection;
    Text resumeText;
    Text restartText; // only use in Arcade

    Text sfxVolumeText;
    Text musicVolumeText;
    Text volumeText;

    Text quitText;
    Text quitSureText;
    Text quitSureYesText;
    Text quitSureNoText;
    bool settingSFX = false;
    bool settingMusic = false;

}
