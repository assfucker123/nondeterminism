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

    public int health { get { return _health; } }
    public int maxHealth { get { return _maxHealth; } }
    public bool canPause {
        get {
            return (
                !PauseScreen.paused &&
                !TimeUser.reverting &&
                !HUD.instance.gameOverScreen.activated);
        }
    }
    public UnityEngine.UI.Image blackScreen { get { return _blackScreen; } }
    public PhaseMeter phaseMeter { get { return _phaseMeter; } }
    public CountdownTimer countdownTimer { get { return _countdownTimer; } }
    public SpeedLines speedLines { get { return _speedLines; } }
    public PauseScreen pauseScreen { get { return _pauseScreen; } }
    public MapUI mapUI { get { return _mapUI; } }
    public GameOverScreen gameOverScreen { get { return _gameOverScreen; } }

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
	
	void Awake() {
        _instance = this;
        canvas = GetComponent<Canvas>();

        //create Speed Lines
        GameObject slGO = GameObject.Instantiate(speedLinesGameObject) as GameObject;
        slGO.transform.SetParent(canvas.transform, false);
        _speedLines = slGO.GetComponent<SpeedLines>();
        _speedLines.setUp();
        //create Phase Meter
        GameObject pmGO = GameObject.Instantiate(phaseMeterGameObject) as GameObject;
        pmGO.transform.SetParent(canvas.transform, false);
        _phaseMeter = pmGO.GetComponent<PhaseMeter>();
        _phaseMeter.setUp();
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
        //create Countdown Timer
        GameObject ctGO = GameObject.Instantiate(countdownTimerGameObject) as GameObject;
        ctGO.transform.SetParent(canvas.transform, false);
        _countdownTimer = ctGO.GetComponent<CountdownTimer>();
        _countdownTimer.setUp();
        //create Game Over Screen
        GameObject gosGO = GameObject.Instantiate(gameOverScreenObject) as GameObject;
        gosGO.transform.SetParent(canvas.transform, false);
        _gameOverScreen = gosGO.GetComponent<GameOverScreen>();
        _gameOverScreen.initialHide();
        
	}

    void Start() {
        blackScreen.color = Color.clear;
    }

    void createPauseScreen() {
        if (PauseScreen.instance != null) return;
        GameObject psGO = GameObject.Instantiate(pauseScreenGameObject) as GameObject;
        psGO.transform.SetParent(canvas.transform, false);
        psGO.transform.SetSiblingIndex(_mapUI.transform.GetSiblingIndex()); // put pause screen behind mapUI
        _pauseScreen = psGO.GetComponent<PauseScreen>();
        _pauseScreen.initialHide();
    }

    void destroyPauseScreen() {
        GameObject.Destroy(_pauseScreen.gameObject);
        _pauseScreen = null;
    }
	
	void Update() {

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

    private List<HealthHeart> healthHearts = new List<HealthHeart>();
    private UnityEngine.UI.Image _blackScreen;
    private PhaseMeter _phaseMeter;
    private CountdownTimer _countdownTimer;
    private SpeedLines _speedLines;
    private PauseScreen _pauseScreen;
    private MapUI _mapUI;
    private GameOverScreen _gameOverScreen;
	
	// components
    Canvas canvas;
}
