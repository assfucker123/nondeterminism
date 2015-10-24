using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PauseScreen : MonoBehaviour {

    public static PauseScreen instance { get { return _instance; } }

    public enum Page {
        NONE, //meaning entire pause screen is hidden
        MAP,
        TIME_TREE,
        TALK,
        OPTIONS
    }

    public Page page { get { return _page; } }
    public bool paused { get { return _paused; } }

    public void pauseGame(Page page = Page.OPTIONS) {
        if (paused)
            return;
        if (page == Page.NONE)
            return;

        // stop time
        prevTimeScale = Time.timeScale;
        prevEffectsEnabled = CameraControl.instance.effectsEnabled;
        prevBloomIntensity = CameraControl.instance.bloomIntensity;
        prevColorCorrectionSaturation = CameraControl.instance.colorCorrectionSaturation;
        Time.timeScale = 0;
        CameraControl.instance.enableEffects(0, 0); //grayscale camera

        // show pause screen
        show(page);
        timePaused = 0;

        _paused = true;
    }
    public void unpauseGame() {
        if (!paused)
            return;

        // hide pause screen
        hide();

        // resume time
        Time.timeScale = prevTimeScale;
        if (prevEffectsEnabled) {
            CameraControl.instance.enableEffects(prevBloomIntensity, prevColorCorrectionSaturation);
        } else {
            CameraControl.instance.disableEffects();
        }

        _paused = false;
    }
    public void initialHide() {
        m_hide();
        tt_hide();
        t_hide();
        o_hide();
        hide();
    }


    public static Color DEFAULT_COLOR = new Color(203 / 255f, 136 / 255f, 177 / 255f);
    public static Color SELECTED_COLOR = new Color(45 / 255f, 0 / 255f, 27 / 255f);

    void show(Page page) {
        image.enabled = true;
        if (!Vars.arcadeMode) { // arcade mode only lets options window be shown
            mapPageText.enabled = true;
            timeTreePageText.enabled = true;
            talkPageText.enabled = true;
            switchPagesText.enabled = true;
        }
        optionsPageText.enabled = true;
        pageSelection.enabled = true;
        
        switchPage(page, true);
    }
    void hide() {
        switchPage(Page.NONE, true);

        image.enabled = false;
        mapPageText.color = DEFAULT_COLOR;
        mapPageText.enabled = false;
        timeTreePageText.color = DEFAULT_COLOR;
        timeTreePageText.enabled = false;
        talkPageText.color = DEFAULT_COLOR;
        talkPageText.enabled = false;
        optionsPageText.color = DEFAULT_COLOR;
        optionsPageText.enabled = false;
        pageSelection.enabled = false;
        switchPagesText.enabled = false;
    }
    void switchPage(Page pageTo, bool immediately=false) {
        if (page == pageTo) return;

        // hide original page
        Text option = null;
        switch (page) {
        case Page.MAP:
            option = mapPageText;
            m_hide();
            break;
        case Page.TIME_TREE:
            option = timeTreePageText;
            tt_hide();
            break;
        case Page.TALK:
            option = talkPageText;
            t_hide();
            break;
        case Page.OPTIONS:
            option = optionsPageText;
            o_hide();
            break;
        }
        if (option == null) {
            immediately = true;
        } else {
            option.color = DEFAULT_COLOR;
        }
        _page = pageTo;

        //show new page
        switch (page) {
        case Page.MAP:
            option = mapPageText;
            m_show();
            break;
        case Page.TIME_TREE:
            option = timeTreePageText;
            tt_show();
            break;
        case Page.TALK:
            option = talkPageText;
            t_show();
            break;
        case Page.OPTIONS:
            option = optionsPageText;
            o_show();
            break;
        }
        if (option != null) {
            option.color = SELECTED_COLOR;
            pageSelectionImageFinalPos.Set(option.rectTransform.localPosition.x, option.rectTransform.localPosition.y);
            pageSelectionImageFinalPos += new Vector2(0, 2);
        }
        
        // move page selection
        if (option == null) {
            pageSelection.enabled = false;
            pageSelectionImageTime = 9999;
        } else {
            pageSelection.enabled = true;
            pageSelectionImageTime = 0;
            pageSelectionImageInitialPos.Set(pageSelection.rectTransform.localPosition.x, pageSelection.rectTransform.localPosition.y);
            if (immediately) {
                pageSelectionImageInitialPos = pageSelectionImageFinalPos;
            }
        }

    }

	
	void Awake() {
        _instance = this;
        image = GetComponent<Image>();
		mapPageText = transform.Find("MapPageText").GetComponent<Text>();
        timeTreePageText = transform.Find("TimeTreePageText").GetComponent<Text>();
        talkPageText = transform.Find("TalkPageText").GetComponent<Text>();
        optionsPageText = transform.Find("OptionsPageText").GetComponent<Text>();
        pageSelection = transform.Find("PageSelection").GetComponent<Image>();
        switchPagesText = transform.Find("SwitchPagesText").GetComponent<Text>();

        optionsPage = transform.Find("OptionsPage").gameObject;
        o_selection = optionsPage.transform.Find("Selection").GetComponent<Image>();
        o_resumeText = optionsPage.transform.Find("ResumeText").GetComponent<Text>();
        o_restartText = optionsPage.transform.Find("RestartText").GetComponent<Text>();
        o_quitText = optionsPage.transform.Find("QuitText").GetComponent<Text>();
        o_quitSureText = optionsPage.transform.Find("QuitSureText").GetComponent<Text>();
        o_quitSureYesText = optionsPage.transform.Find("QuitSureYesText").GetComponent<Text>();
        o_quitSureNoText = optionsPage.transform.Find("QuitSureNoText").GetComponent<Text>();

	}
	
	void LateUpdate() {
        if (!paused)
            return;

        // use Time.unscaledDeltaTime;
        timePaused += Time.unscaledDeltaTime;

        // detect switching page
        if (!Vars.arcadeMode) { // can only be on options page in arcade mode
            if (Input.GetButtonDown("PageLeft")) {
                Page pageTo = page;
                switch (page) {
                case Page.MAP: pageTo = Page.OPTIONS; break;
                case Page.TIME_TREE: pageTo = Page.MAP; break;
                case Page.TALK: pageTo = Page.TIME_TREE; break;
                case Page.OPTIONS: pageTo = Page.TALK; break;
                }
                switchPage(pageTo);
            } else if (Input.GetButtonDown("PageRight")) {
                Page pageTo = page;
                switch (page) {
                case Page.MAP: pageTo = Page.TIME_TREE; break;
                case Page.TIME_TREE: pageTo = Page.TALK; break;
                case Page.TALK: pageTo = Page.OPTIONS; break;
                case Page.OPTIONS: pageTo = Page.MAP; break;
                }
                switchPage(pageTo);
            }
        }

        // move pageSelection
        float selectionTransDur = .1f;
        if (pageSelectionImageTime < selectionTransDur) {
            pageSelectionImageTime += Time.unscaledDeltaTime;
            pageSelection.rectTransform.localPosition = new Vector3(
                Utilities.easeOutQuadClamp(pageSelectionImageTime, pageSelectionImageInitialPos.x, pageSelectionImageFinalPos.x - pageSelectionImageInitialPos.x, selectionTransDur),
                Utilities.easeOutQuadClamp(pageSelectionImageTime, pageSelectionImageInitialPos.y, pageSelectionImageFinalPos.y - pageSelectionImageInitialPos.y, selectionTransDur));
        }

        // page update functions
        switch (page) {
        case Page.OPTIONS: o_update(); break;
        }
        
        // unpausing game
        if (timePaused > .1f && Input.GetButtonDown("Pause")) {
            unpauseGame();
        }

	}

    void OnDestroy() {
        _instance = null;
    }

    private static PauseScreen _instance;
    Page _page = Page.NONE;
    bool _paused = false;
    float timePaused = 0;
    float prevTimeScale = 1;
    bool prevEffectsEnabled = false;
    float prevBloomIntensity = 0;
    float prevColorCorrectionSaturation = 1;
    float pageSelectionImageTime = 9999;
    Vector2 pageSelectionImageInitialPos = new Vector2();
    Vector2 pageSelectionImageFinalPos = new Vector2();

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
            option.color = DEFAULT_COLOR;
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
        option.color = SELECTED_COLOR;
        selectionImageFinalPos = selectionImageOffset + new Vector2(option.rectTransform.localPosition.x, option.rectTransform.localPosition.y);
        if (immediately) {
            selectionImageInitialPos = selectionImageFinalPos;
        }
    }
    // call this during update to move the selection around
    void optionsUpdate() {
        // button presses to navigate options
        if (options.Count > 0) {
            if (Input.GetButtonDown("Down")) {
                setSelection(selectionIndex + 1);
            } else if (Input.GetButtonDown("Up")) {
                setSelection(selectionIndex - 1);
            }
        }
        // move selection image
        float selectionTransDur = .1f;
        if (selectionImageTime < selectionTransDur) {
            selectionImageTime += Time.unscaledDeltaTime;
            selectionImage.rectTransform.localPosition = new Vector3(
                Utilities.easeOutQuadClamp(selectionImageTime, selectionImageInitialPos.x, selectionImageFinalPos.x-selectionImageInitialPos.x, selectionTransDur),
                Utilities.easeOutQuadClamp(selectionImageTime, selectionImageInitialPos.y, selectionImageFinalPos.y-selectionImageInitialPos.y, selectionTransDur)
                );
        }
    }

	// main pause screen
    Image image;
    Text mapPageText;
    Text timeTreePageText;
    Text talkPageText;
    Text optionsPageText;
    Image pageSelection;
    Text switchPagesText;
    
    // map page
    void m_show() { }
    void m_hide() { }

    // time tree page
    void tt_show() { }
    void tt_hide() { }

    // talk page
    /* Talking should be able to be visible while displaying the map or time tree (for tutorials, etc.)
     * Remember the talk page will have secrets:
     *   - Holding FLASHBACK to rewind text
     *   - Tap __ to start a small minigame
     * */
    void t_show() { }
    void t_hide() { }

    // options page
    GameObject optionsPage;
    Image o_selection;
    Text o_resumeText;
    Text o_restartText; // only use in Arcade
    Text o_quitText;
    Text o_quitSureText;
    Text o_quitSureYesText;
    Text o_quitSureNoText;
    void o_show() {
        o_selection.enabled = true;
        o_resumeText.enabled = true;
        o_restartText.enabled = true;
        o_quitText.enabled = true;
        o_quitSureText.enabled = false;
        o_quitSureYesText.enabled = false;
        o_quitSureNoText.enabled = false;
        // set options helper
        selectionImage = o_selection;
        selectionImageOffset.Set(0, 2);
        options.Clear();
        options.Add(o_resumeText);
        options.Add(o_restartText);
        options.Add(o_quitText);
        setSelection(0, true);
    }
    void o_hide() {
        o_selection.enabled = false;
        o_resumeText.enabled = false;
        o_restartText.enabled = false;
        o_quitText.enabled = false;
        o_quitSureText.enabled = false;
        o_quitSureYesText.color = DEFAULT_COLOR;
        o_quitSureYesText.enabled = false;
        o_quitSureNoText.color = DEFAULT_COLOR;
        o_quitSureNoText.enabled = false;
    }
    void o_update() {

        optionsUpdate();

        bool madeSelection = false;
        Text option = null;
        if (Input.GetButtonDown("Jump")) {
            madeSelection = true;
            option = options[selectionIndex];
        }
        if (Input.GetButtonDown("Fire1")) {
            if (o_quitSureNoText.enabled) {
                madeSelection = true;
                option = o_quitSureNoText;
            }
        }
        if (madeSelection) { // pressed selection
            if (option == o_resumeText) {
                // resume game
                unpauseGame();
            } else if (option == o_restartText) {
                // restart game
                unpauseGame();
                Vars.restartLevel();
            } else if (option == o_quitText) {
                // go to quit sure mode
                // get rid of other options
                option.color = DEFAULT_COLOR;
                o_hide();
                o_selection.enabled = true;
                options.Clear();
                // set new options
                o_quitSureText.enabled = true;
                o_quitSureYesText.enabled = true;
                o_quitSureYesText.rectTransform.localPosition = o_restartText.rectTransform.localPosition;
                o_quitSureNoText.enabled = true;
                o_quitSureNoText.rectTransform.localPosition = o_quitText.rectTransform.localPosition;
                options.Add(o_quitSureYesText);
                options.Add(o_quitSureNoText);
                setSelection(1, true);
            } else if (option == o_quitSureNoText) {
                // cancel quitting
                o_show();
                setSelection(options.IndexOf(o_quitText), true);
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


}
