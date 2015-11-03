﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PauseScreen : MonoBehaviour {

    public static PauseScreen instance { get { return _instance; } }

    public AudioClip switchSound;

    public static Color DEFAULT_COLOR = new Color(203 / 255f, 136 / 255f, 177 / 255f);
    public static Color SELECTED_COLOR = new Color(45 / 255f, 0 / 255f, 27 / 255f);

    public enum Page {
        NONE, //meaning entire pause screen is hidden
        MAP,
        TIME_TREE,
        TALK,
        PROGRESS,
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
        //mapPage.hide();
        //timeTreePage.hide();
        //talkPage.hide();
        //progressPage.hide();
        optionsPage.hide();
        hide();
    }

    // good for ui, since can't get buttonDown for axises
    public bool leftButtonDown { get { return _leftButtonDown; } }
    public bool rightButtonDown { get { return _rightButtonDown; } }
    public bool upButtonDown { get { return _upButtonDown; } }
    public bool downButtonDown { get { return _downButtonDown; } }

    /////////////
    // PRIVATE //
    /////////////

    void show(Page page) {
        image.enabled = true;
        if (!Vars.arcadeMode) { // arcade mode only lets options window be shown
            mapPageText.enabled = true;
            timeTreePageText.enabled = true;
            talkPageText.enabled = true;
            progressPageText.enabled = true;
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
        progressPageText.color = DEFAULT_COLOR;
        progressPageText.enabled = false;
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
            //mapPage.hide();
            break;
        case Page.TIME_TREE:
            option = timeTreePageText;
            //timeTreePage.hide();
            break;
        case Page.TALK:
            option = talkPageText;
            //talkPage.hide();
            break;
        case Page.PROGRESS:
            option = progressPageText;
            //progressPage.hide();
            break;
        case Page.OPTIONS:
            option = optionsPageText;
            optionsPage.hide();
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
            //mapPage.show();
            break;
        case Page.TIME_TREE:
            option = timeTreePageText;
            //timeTreePage.show();
            break;
        case Page.TALK:
            option = talkPageText;
            //talkPage.show();
            break;
        case Page.PROGRESS:
            option = progressPageText;
            //progressPage.show();
            break;
        case Page.OPTIONS:
            option = optionsPageText;
            optionsPage.show();
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
        progressPageText = transform.Find("ProgressPageText").GetComponent<Text>();
        optionsPageText = transform.Find("OptionsPageText").GetComponent<Text>();
        pageSelection = transform.Find("PageSelection").GetComponent<Image>();
        switchPagesText = transform.Find("SwitchPagesText").GetComponent<Text>();

        // options page
        optionsPage = transform.Find("OptionsPage").GetComponent<OptionsPage>();

	}

    // controls
    void Update() {

        float horiz = Input.GetAxisRaw("Horizontal");
        float vert = Input.GetAxisRaw("Vertical");
        bool prevLeftButton = leftButton;
        leftButton = horiz < -.5f;
        _leftButtonDown = !prevLeftButton && leftButton;
        bool prevRightButton = rightButton;
        rightButton = horiz > .5f;
        _rightButtonDown = !prevRightButton && rightButton;
        bool prevUpButton = upButton;
        upButton = vert < -.5f;
        _upButtonDown = !prevUpButton && upButton;
        bool prevDownButton = downButton;
        downButton = vert > .5f;
        _downButtonDown = !prevDownButton && downButton;

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
                bool immediately = false;
                switch (page) {
                case Page.MAP:
                    pageTo = Page.OPTIONS;
                    immediately = true; //wrapping around looks awkward
                    break;
                case Page.TIME_TREE: pageTo = Page.MAP; break;
                case Page.TALK: pageTo = Page.TIME_TREE; break;
                case Page.PROGRESS: pageTo = Page.TALK; break;
                case Page.OPTIONS: pageTo = Page.PROGRESS; break;
                }
                switchPage(pageTo, immediately);
            } else if (Input.GetButtonDown("PageRight")) {
                Page pageTo = page;
                bool immediately = false;
                switch (page) {
                case Page.MAP: pageTo = Page.TIME_TREE; break;
                case Page.TIME_TREE: pageTo = Page.TALK; break;
                case Page.TALK: pageTo = Page.PROGRESS; break;
                case Page.PROGRESS: pageTo = Page.OPTIONS; break;
                case Page.OPTIONS:
                    pageTo = Page.MAP;
                    immediately = true; //wrapping around looks weird
                    break;
                }
                switchPage(pageTo, immediately);
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
        case Page.OPTIONS:
            optionsPage.update();
            break;
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

    bool _leftButtonDown = false;
    bool leftButton = false;
    bool _rightButtonDown = false;
    bool rightButton = false;
    bool _upButtonDown = false;
    bool upButton = false;
    bool _downButtonDown = false;
    bool downButton = false;

    // main pause screen
    Image image;
    Text mapPageText;
    Text timeTreePageText;
    Text talkPageText;
    Text progressPageText;
    Text optionsPageText;
    Image pageSelection;
    Text switchPagesText;
    
    // talk page
    /* Talking should be able to be visible while displaying the map or time tree (for tutorials, etc.)
     * Remember the talk page will have secrets:
     *   - Holding FLASHBACK to rewind text
     *   - Tap __ to start a small minigame
     * */
    
    // pages
    //MapPage mapPage;
    //TimeTreePage timeTreePage
    //TalkPage talkPage;
    //ProgressPage progressPage;
    OptionsPage optionsPage;
    
    
    
    
    


}
