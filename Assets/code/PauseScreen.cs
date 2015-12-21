using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PauseScreen : MonoBehaviour {

    public static PauseScreen instance { get { return _instance; } }
    public static bool paused {
        get {
            if (instance == null) return false;
            return instance._paused;
        }
    }
    public static Page lastPageOpened = Page.OPTIONS;

    public AudioClip switchSound;
    public GameObject mapPageGameObject;
    public GameObject optionsPageGameObject;
    public TextAsset propAsset;
    float openAnimationDuration = .00001f; //can't be 0

    public static Color DEFAULT_COLOR = new Color(203 / 255f, 136 / 255f, 177 / 255f); // CB88B1
    public static Color SELECTED_COLOR = new Color(45 / 255f, 0 / 255f, 27 / 255f); // 2D001B

    public enum Page {
        NONE, //meaning entire pause screen is hidden
        MAP,
        TIME_TREE,
        TALK,
        PROGRESS,
        OPTIONS
    }

    public Page page { get { return _page; } }

    // pauses the game, doing an animation before the player can choose stuff
    public void pauseGame(Page page = Page.OPTIONS) {
        if (paused) return;
        if (page == Page.NONE) return;

        stopTime();
        if (!Vars.screenshotMode) {
            CameraControl.instance.enableEffects(0, 0, 0); //grayscale camera
        }

        // do open animation first
        lastPageOpened = page;
        openAnimationTime = 0;

        _paused = true;
    }
    public bool doingOpenAnimation { get { return paused && openAnimationTime < openAnimationDuration; } }
    public void unpauseGame() {
        if (!paused)
            return;
        
        hide();
        resumeTime();
        _paused = false;

        // destroy the pause screen
        GameObject.Destroy(gameObject);
    }
    public void initialHide() {
        mapPage.hide();
        //timeTreePage.hide();
        //talkPage.hide();
        //progressPage.hide();
        optionsPage.hide();
        hide();
    }


    /////////////
    // PRIVATE //
    /////////////

    void stopTime() {
        prevTimeScale = Time.timeScale;
        prevEffectsEnabled = CameraControl.instance.effectsEnabled;
        prevBloomIntensity = CameraControl.instance.bloomIntensity;
        prevColorCorrectionSaturation = CameraControl.instance.colorCorrectionSaturation;
        prevInversion = CameraControl.instance.inversion;
        Time.timeScale = 0;
    }
    void resumeTime() {
        Time.timeScale = prevTimeScale;
        if (prevEffectsEnabled) {
            CameraControl.instance.enableEffects(prevBloomIntensity, prevColorCorrectionSaturation, prevInversion);
        } else {
            CameraControl.instance.disableEffects();
        }
    }

    void show(Page page) {
        image.enabled = true;
        if (!Vars.arcadeMode) { // arcade mode only lets options window be shown
            mapPageText.makeAllCharsVisible();
            timeTreePageText.makeAllCharsVisible();
            talkPageText.makeAllCharsVisible();
            progressPageText.makeAllCharsVisible();
            switchPagesText.makeAllCharsVisible();
        }
        optionsPageText.makeAllCharsVisible();
        pageSelection.enabled = true;
        
        switchPage(page, true);
    }
    void hide() {
        switchPage(Page.NONE, true);

        image.enabled = false;
        mapPageText.setColor(DEFAULT_COLOR);
        mapPageText.makeAllCharsInvisible();
        timeTreePageText.setColor(DEFAULT_COLOR);
        timeTreePageText.makeAllCharsInvisible();
        talkPageText.setColor(DEFAULT_COLOR);
        talkPageText.makeAllCharsInvisible();
        progressPageText.setColor(DEFAULT_COLOR);
        progressPageText.makeAllCharsInvisible();
        optionsPageText.setColor(DEFAULT_COLOR);
        optionsPageText.makeAllCharsInvisible();
        pageSelection.enabled = false;
        switchPagesText.makeAllCharsInvisible();
    }
    void switchPage(Page pageTo, bool immediately=false) {
        if (doingOpenAnimation) return;
        if (page == pageTo) return;
        
        // hide original page
        GlyphBox option = null;
        switch (page) {
        case Page.MAP:
            option = mapPageText;
            mapPage.hide();
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
            option.setColor(DEFAULT_COLOR);
        }
        _page = pageTo;
        if (_page != Page.NONE)
            lastPageOpened = _page;

        //show new page
        switch (page) {
        case Page.MAP:
            option = mapPageText;
            mapPage.show();
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
            optionsPage.show(false);
            break;
        }
        if (option != null) {
            option.setColor(SELECTED_COLOR);
            pageSelectionImageFinalPos.Set(option.rectTransform.localPosition.x, option.rectTransform.localPosition.y);
            pageSelectionImageFinalPos += new Vector2(0, 0);
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
        prop = new Properties(propAsset.text);
        image = GetComponent<Image>();
		mapPageText = transform.Find("MapPageText").GetComponent<GlyphBox>();
        timeTreePageText = transform.Find("TimeTreePageText").GetComponent<GlyphBox>();
        talkPageText = transform.Find("TalkPageText").GetComponent<GlyphBox>();
        progressPageText = transform.Find("ProgressPageText").GetComponent<GlyphBox>();
        optionsPageText = transform.Find("OptionsPageText").GetComponent<GlyphBox>();
        pageSelection = transform.Find("PageSelection").GetComponent<Image>();
        switchPagesText = transform.Find("SwitchPagesText").GetComponent<GlyphBox>();

        // map page
        GameObject mapPageGO = GameObject.Instantiate(mapPageGameObject);
        mapPageGO.transform.SetParent(transform, false);
        mapPage = mapPageGO.GetComponent<MapPage>();
        mapPage.GetComponent<RectTransform>().localScale = Vector3.one;

        // options page
        GameObject optionsPageGO = GameObject.Instantiate(optionsPageGameObject);
        optionsPageGO.transform.SetParent(transform, false);
        optionsPage = optionsPageGO.GetComponent<OptionsPage>();
        optionsPage.GetComponent<RectTransform>().localScale = Vector3.one;
        
	}

    void Start() {
        hide();
    }

    void Update() {
        if (doingOpenAnimation) {
            openAnimationTime += Time.unscaledDeltaTime;
            if (openAnimationTime >= openAnimationDuration) {

                // creating the pause screen

                // display stuff
                mapPageText.setPlainText(prop.getString("map"));
                timeTreePageText.setPlainText(prop.getString("time_tree"));
                talkPageText.setPlainText(prop.getString("talk"));
                progressPageText.setPlainText(prop.getString("progress"));
                optionsPageText.setPlainText(prop.getString("options"));
                switchPagesText.setPlainText(prop.getString("switch_pages"));
                image.enabled = true;

                // display more stuff
                if (!Vars.screenshotMode) {
                    show(lastPageOpened);
                }
                timePaused = 0;

            }
        }
    }
    
    void LateUpdate() {
        if (!paused)
            return;
        if (doingOpenAnimation)
            return;


        // use Time.unscaledDeltaTime;
        timePaused += Time.unscaledDeltaTime;

        if (Vars.screenshotMode) {
            // unpausing game
            if (timePaused > .1f && Keys.instance.startPressed) {
                unpauseGame();
            }
            return;
        }

        // detect switching page
        if (!Vars.arcadeMode) { // can only be on options page in arcade mode
            if (Keys.instance.pageLeftPressed) {
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
            } else if (Keys.instance.pageRightPressed) {
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
        case Page.MAP:
            mapPage.update();
            break;
        case Page.TIME_TREE:
            //timeTreePage.update();
            break;
        case Page.TALK:
            //talkPage.update();
            break;
        case Page.PROGRESS:
            //progressPage.update();
            break;
        case Page.OPTIONS:
            optionsPage.update();
            break;
        }
        
        // unpausing game
        if (timePaused > .1f && Keys.instance.startPressed) {
            unpauseGame();
        }

	}

    void OnDestroy() {
        hide();
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
    float prevInversion = 0;
    float pageSelectionImageTime = 9999;
    Vector2 pageSelectionImageInitialPos = new Vector2();
    Vector2 pageSelectionImageFinalPos = new Vector2();
    float openAnimationTime = 999999;
    

    // main pause screen
    Image image;
    GlyphBox mapPageText;
    GlyphBox timeTreePageText;
    GlyphBox talkPageText;
    GlyphBox progressPageText;
    GlyphBox optionsPageText;
    Image pageSelection;
    GlyphBox switchPagesText;
    Properties prop;
    
    // talk page
    /* Talking should be able to be visible while displaying the map or time tree (for tutorials, etc.)
     * Remember the talk page will have secrets:
     *   - Tap __ to start a small minigame
     * */
    
    // pages
    MapPage mapPage;
    //TimeTreePage timeTreePage
    //TalkPage talkPage;
    //ProgressPage progressPage;
    OptionsPage optionsPage;
    
    
    
    
    


}
