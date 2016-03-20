using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HUD : MonoBehaviour {

    public static HUD instance { get { return _instance; } }

    public GameObject blackScreenGameObject;
    public GameObject healthHeartGameObject;
    public GameObject phaseMeterGameObject;
    public GameObject countdownTimerGameObject;
    public GameObject pauseScreenGameObject;
    public GameObject mapUIGameObject;
    public GameObject speedLinesGameObject;
    public GameObject gameOverScreenObject;
    public GameObject textBoxGameObject;
    public GameObject cutsceneBarsGameObject;
    public GameObject controlsMessageSpawnerGameObject;
    public GameObject notificationGameObject;
    public GameObject bossHealthBarGameObject;

    public GameObject flashbackArtifactsGameObject;

    public int health { get { return _health; } }
    public int maxHealth { get { return _maxHealth; } }
    public bool canPause {
        get {
            return (
                !PauseScreen.paused &&
                !TimeUser.reverting &&
                !(GameOverScreen.instance != null && GameOverScreen.instance.activated) &&
                !ScriptRunner.scriptsPreventPausing &&
                (ChamberPlatform.instance == null || ChamberPlatform.instance.state != ChamberPlatform.State.PAUSED));
        }
    }
    public UnityEngine.UI.Image blackScreen { get { return _blackScreen; } }
    public PhaseMeter phaseMeter { get { return _phaseMeter; } }
    public CountdownTimer countdownTimer { get { return _countdownTimer; } }
    public SpeedLines speedLines { get { return _speedLines; } }
    public PauseScreen pauseScreen { get { return _pauseScreen; } }
    public MapUI mapUI { get { return _mapUI; } }
    public GameOverScreen gameOverScreen { get { return _gameOverScreen; } }
    public TextBox textBox {  get { return _textBox; } }
    public CutsceneBars cutsceneBars {  get { return _cutsceneBars; } }
    public BossHealthBar bossHealthBar {  get { return _bossHealthBar; } }
    public ControlsMessageSpawner controlsMessageSpawner {  get { return _controlsMessageSpawner; } }
    public Notification notification {  get { return _notification; } }
    
    public FlashbackArtifacts flashbackArtifacts { get { return _flashbackArtifacts; } }

    public void setMaxHealth(int maxHealth){
        if (_maxHealth == maxHealth)
            return;

        _maxHealth = maxHealth;
        int numHH = maxHealth / 2;
        while (healthHearts.Count > numHH) {
            HealthHeart hh = healthHearts[healthHearts.Count - 1];
            GameObject.Destroy(hh.gameObject);
            healthHearts.RemoveAt(healthHearts.Count - 1);
        }
        while (healthHearts.Count < numHH) {
            GameObject hhGO = GameObject.Instantiate(healthHeartGameObject) as GameObject;
            hhGO.transform.SetParent(canvas.transform, false);
            hhGO.transform.SetAsFirstSibling();
            if (cutsceneBars != null) {
                cutsceneBars.transform.SetAsFirstSibling();
            }
            HealthHeart hh = hhGO.GetComponent<HealthHeart>();
            int index = healthHearts.Count;
            healthHearts.Add(hh);

            hh.setPosition(new Vector2(hh.startPosition.x + hh.spacing * index, hh.startPosition.y));
        }
        
    }

    public void setHealth(int health) {
        if (_health == health)
            return;

        if (health < 0) health = 0;
        if (health > maxHealth) health = maxHealth;

        if (health < this.health) {
            //jostle hearts
            foreach (HealthHeart hh in healthHearts) {
                hh.jostle();
            }
        } else if (health > this.health) {
            //shine hearts
            foreach (HealthHeart hh in healthHearts) {
                hh.shine();
            }
        }

        _health = health;

        if (health < 0 || health > healthHearts.Count*2)
            return;
        for (int i = 0; i < healthHearts.Count; i++) {
            HealthHeart.State state = HealthHeart.State.EMPTY;
            if (health >= (i+1)*2) {
                state = HealthHeart.State.FULL;
            }
            if (health == (i+1)*2-1) {
                state = HealthHeart.State.HALF;
            }
            healthHearts[i].state = state;
        }

    }

    public void onUnloadLevel() {
        foreach (ControlsMessage cm in ControlsMessage.allMessages) {
            GameObject.Destroy(cm.gameObject);
        }
        ControlsMessage.allMessages.Clear();
        foreach (HaltScreen hs in HaltScreen.allScreens) {
            GameObject.Destroy(hs.gameObject);
        }
        HaltScreen.allScreens.Clear();
        if (bossHealthBar != null) {
            bossHealthBar.hide();
        }
        if (notification != null) {
            notification.clearAll();
        }
    }
	
	void Awake() {
        _instance = this;
        canvas = GetComponent<Canvas>();

        //create Speed Lines
        GameObject slGO = GameObject.Instantiate(speedLinesGameObject) as GameObject;
        slGO.transform.SetParent(canvas.transform, false);
        _speedLines = slGO.GetComponent<SpeedLines>();
        _speedLines.setUp();
        //create Flashback Artifacts
        GameObject faGO = GameObject.Instantiate(flashbackArtifactsGameObject) as GameObject;
        faGO.transform.SetParent(canvas.transform, false);
        _flashbackArtifacts = faGO.GetComponent<FlashbackArtifacts>();
        _flashbackArtifacts.setUp();
        // create Cutscene Bars
        GameObject cbGO = GameObject.Instantiate(cutsceneBarsGameObject) as GameObject;
        cbGO.transform.SetParent(canvas.transform, false);
        _cutsceneBars = cbGO.GetComponent<CutsceneBars>();
        _cutsceneBars.moveOffImmediately();

        // creates boss health bar
        GameObject bhbGO = GameObject.Instantiate(bossHealthBarGameObject) as GameObject;
        bhbGO.transform.SetParent(canvas.transform, false);
        _bossHealthBar = bhbGO.GetComponent<BossHealthBar>();
        _bossHealthBar.hide();

        // create controls message spawner
        GameObject cmsGO = GameObject.Instantiate(controlsMessageSpawnerGameObject) as GameObject;
        cmsGO.transform.SetParent(canvas.transform, false);
        _controlsMessageSpawner = cmsGO.GetComponent<ControlsMessageSpawner>();

        //create Phase Meter
        GameObject pmGO = GameObject.Instantiate(phaseMeterGameObject) as GameObject;
        pmGO.transform.SetParent(canvas.transform, false);
        _phaseMeter = pmGO.GetComponent<PhaseMeter>();
        _phaseMeter.setUp();

        // create Notification
        GameObject nGO = GameObject.Instantiate(notificationGameObject) as GameObject;
        nGO.transform.SetParent(canvas.transform, false);
        _notification = nGO.GetComponent<Notification>();

        //create Countdown Timer
        GameObject ctGO = GameObject.Instantiate(countdownTimerGameObject) as GameObject;
        ctGO.transform.SetParent(canvas.transform, false);
        _countdownTimer = ctGO.GetComponent<CountdownTimer>();
        _countdownTimer.setUp();

        //create Black Screen
        GameObject bsGO = GameObject.Instantiate(blackScreenGameObject) as GameObject;
        bsGO.transform.SetParent(canvas.transform, false);
        _blackScreen = bsGO.GetComponent<UnityEngine.UI.Image>();

        // (not creating pause screen until needed, too much bottleneck)

        //create Map UI
        GameObject muGO = GameObject.Instantiate(mapUIGameObject) as GameObject;
        muGO.transform.SetParent(canvas.transform, false);
        _mapUI = muGO.GetComponent<MapUI>();
        _mapUI.hideMap();
        
        // (not creating Game Over screen until needed)
        
        // create Text Box
        GameObject tbGO = GameObject.Instantiate(textBoxGameObject) as GameObject;
        tbGO.transform.SetParent(canvas.transform, false);
        _textBox = tbGO.GetComponent<TextBox>();
        _textBox.closeImmediately();

	}

    void Start() {
        blackScreen.color = Color.clear;
    }

    void OnLevelWasLoaded(int level) {

        // fading out black screen once level is loaded
        blackScreenFadeTime = 0;
        blackScreen.color = new Color(0, 0, 0, 1);

    }

    public void createPauseScreen() {
        if (PauseScreen.instance != null) return;
        GameObject psGO = GameObject.Instantiate(pauseScreenGameObject) as GameObject;
        psGO.transform.SetParent(canvas.transform, false);
        psGO.transform.SetSiblingIndex(_mapUI.transform.GetSiblingIndex()); // put pause screen behind mapUI
        _pauseScreen = psGO.GetComponent<PauseScreen>();
        _pauseScreen.initialHide();
    }
    public void createPauseScreenLight() {
        if (PauseScreen.instance != null) return;
        PauseScreen.lightFlag = true;
        createPauseScreen();
    }

    void destroyPauseScreen() {
        GameObject.Destroy(_pauseScreen.gameObject);
        _pauseScreen = null;
    }

    public void createGameOverScreen() {
        if (GameOverScreen.instance != null) return;
        GameObject gosGO = GameObject.Instantiate(gameOverScreenObject) as GameObject;
        gosGO.transform.SetParent(canvas.transform, false);
        gosGO.transform.SetSiblingIndex(_textBox.transform.GetSiblingIndex()); // put pause screen behind textBox
        _gameOverScreen = gosGO.GetComponent<GameOverScreen>();
        _gameOverScreen.initialHide();
    }
	
    public void destroyGameOverScreen() {
        GameObject.Destroy(_gameOverScreen.gameObject);
        _gameOverScreen = null;
    }

	void Update() {

        // blackscreen fading
        if (blackScreenFadeTime < blackScreenFadeDuration) {
            blackScreenFadeTime += Time.fixedDeltaTime;
            blackScreen.color = new Color(0, 0, 0, Mathf.Max(0, 1 - blackScreenFadeTime / blackScreenFadeDuration));
        }

        // detect screenshots
        if (Vars.screenshotMode) {
            if (Input.GetKeyDown(KeyCode.F8)) {
                Vars.takeScreenshot();
            }
        }

        // detect pausing the game
        if (canPause &&
            (Keys.instance.startPressed || Keys.instance.escapePressed)) {
            createPauseScreen();
            pauseScreen.pauseGame(PauseScreen.lastPageOpened);
        }

        // incrementing playtime
        if (!PauseScreen.paused) {
            Vars.playTime += Time.unscaledDeltaTime;
        }
        
	}

    void LateUpdate() {

        

    }

    void OnDestroy() {
        _instance = null;
        foreach (HealthHeart hh in healthHearts) {
            GameObject.Destroy(hh.gameObject);
        }
        healthHearts.Clear();
    }

    private static HUD _instance;

    private int _maxHealth = -1;
    private int _health = -1;

    float blackScreenFadeTime = 9999;
    float blackScreenFadeDuration = .3f;

    private List<HealthHeart> healthHearts = new List<HealthHeart>();
    private UnityEngine.UI.Image _blackScreen;
    private PhaseMeter _phaseMeter;
    private CountdownTimer _countdownTimer;
    private SpeedLines _speedLines;
    private PauseScreen _pauseScreen;
    private MapUI _mapUI;
    private GameOverScreen _gameOverScreen;
    private TextBox _textBox;
    private Notification _notification;
    private CutsceneBars _cutsceneBars;
    private BossHealthBar _bossHealthBar;
    private ControlsMessageSpawner _controlsMessageSpawner;
    private FlashbackArtifacts _flashbackArtifacts;
	
	// components
    Canvas canvas;
}
