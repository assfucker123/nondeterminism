using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChamberPlatform : MonoBehaviour {

    public GameObject rayGameObject;
    public Vector2 rayCenterPos = new Vector2();
    public float raySpread = 4 / 16f;
    public int rayCount = 8;
    public float rayEdgeScaleMultiplier = .5f;
    public float rayPeriodMin = 1.0f;
    public float rayPeriodMax = 1.5f;
    public float rayScaleCenter = 1;
    public float rayScaleRange = .5f;
    public AudioClip standSound;
    public float standSoundPlayerOffDuration = 1.0f;
    public GameObject chamberScreenGameObject;

    public static ChamberPlatform instance { get; private set; }

    public static string positionCodeFromMapPosition(int x, int y) {
        char xChar = (char)('A' + (x / 2));
        char[] xChars = { xChar };
        int yInt = (y + 1) / 5 + 1;
        return new string(xChars) + yInt;
    }

    public State state { get; private set; }
    public string positionCode {
        get {
            if (MapUI.instance == null) {
                return "";
            } else {
                Vector2 gridPos = MapUI.instance.gridPositionFromWorldPosition(Level.currentLoadedLevel.mapX, Level.currentLoadedLevel.mapY,
                new Vector2(transform.localPosition.x, transform.localPosition.y));
                return positionCodeFromMapPosition(Mathf.RoundToInt(gridPos.x), Mathf.RoundToInt(gridPos.y));
            }
        }
    }

    public enum State {
        IDLE,
        PAUSED
    }

    public void toChamberScreen() {
        if (state != State.IDLE) return;

        Player.instance.toKneel();

        // going to chamber screen requires game to be paused
        Time.timeScale = 0;

        chamberBackground.flyIn();

        state = State.PAUSED;
    }

    public void resumePlay() {

        // get rid of chamber screen
        if (chamberScreenRef != null) {
            GameObject.Destroy(chamberScreenRef.gameObject);
        }
        chamberScreenRef = null;
        screenUp = false;

        Time.timeScale = 0;
        state = State.PAUSED;
        
        // change player animation
        Player.instance.outOfKneel();
        // fly out
        chamberBackground.flyOut();

    }

    /// <summary>
    /// Returns where the player's position should be when using this chamber platform to save
    /// </summary>
    public Vector2 playerSavePosition() {
        return new Vector2(transform.localPosition.x, transform.localPosition.y + 1.6f);
    }


    public bool playerIsOnPlatform {
        get {
            if (Player.instance == null) return false;
            if (!Player.instance.GetComponent<ColFinder>().hitBottom) return false;
            Vector2 plrPos = Player.instance.rb2d.position;
            Bounds bounds = pc2d.bounds;
            if (plrPos.y < bounds.max.y || plrPos.y > bounds.max.y + 2) return false;
            return (bounds.min.x < plrPos.x && plrPos.x < bounds.max.x);
        }
    }

	void Awake() {
        instance = this;
        pc2d = GetComponent<PolygonCollider2D>();
        timeUser = GetComponent<TimeUser>();
        chamberBackground = GetComponent<ChamberBackground>();
        createRays();
	}

    void Start() {
        // add chamber icon to map
        if (MapUI.instance != null) {
            Vector2 gridPos = MapUI.instance.gridPositionFromWorldPosition(Level.currentLoadedLevel.mapX, Level.currentLoadedLevel.mapY,
                new Vector2(transform.localPosition.x, transform.localPosition.y));
            int gridX = Mathf.RoundToInt(gridPos.x);
            int gridY = Mathf.RoundToInt(gridPos.y);
            if (!MapUI.instance.iconInPosition(MapUI.Icon.CHAMBER, gridX, gridY)) {
                MapUI.instance.addIcon(MapUI.Icon.CHAMBER, gridX, gridY);
            }
        }
    }

    void createRays() {
        for (int i=0; i<rayCount; i++) {
            GameObject rGO = GameObject.Instantiate(rayGameObject);
            rGO.transform.SetParent(this.transform, true);
            rGO.transform.localPosition = new Vector2((i - (rayCount - 1) / 2f) * raySpread, 0) + rayCenterPos;
            Ray ray = new Ray();
            ray.gameObject = rGO;
            ray.period = timeUser.randomRange(rayPeriodMin, rayPeriodMax);
            ray.timeOffset = ray.period * timeUser.randomValue();
            float scaleMult = Utilities.easeLinear(Mathf.Abs(i - (rayCount - 1) / 2f), 1, rayEdgeScaleMultiplier - 1, (rayCount - 1) / 2f);
            ray.scaleCenter = rayScaleCenter * scaleMult;
            ray.scaleRange = rayScaleRange * scaleMult;
            rays.Add(ray);
        }
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;
        updateRays();
        
        switch (state) {
        case State.IDLE:

            // detect if platform should refill phase
            if (playerIsOnPlatform) {
                if (Player.instance.phase < Player.instance.maxPhase) {
                    // refill phase
                    phaseUsed = Player.instance.phasePickup(Player.instance.maxPhase);
                }
                if (Player.instance.health < Player.instance.maxHealth) {
                    Player.instance.healthPickup(Player.instance.maxHealth);
                }

                if (timeSincePlayerOnPlatform > standSoundPlayerOffDuration) {
                    SoundManager.instance.playSFX(standSound);
                }
                timeSincePlayerOnPlatform = 0;
            }
            playerHasMaxPhase = (Player.instance != null && Player.instance.phase >= Player.instance.maxPhase);

            timeSincePlayerOnPlatform += Time.deltaTime;

            // detect when going to chamber platform
            if (playerIsOnPlatform &&
                (Keys.instance.upPressed || Keys.instance.downPressed)) {
                toChamberScreen();

                // take down tutorial message on how to activate these
                if (ControlsMessageSpawner.instance != null) {
                    ControlsMessageSpawner.instance.takeDownMessage(ControlsMessage.Control.ACTIVATE_PLATFORMS, true);
                }
                
            }

            break;
        case State.PAUSED:

            // bring up menu when chamberBackground is done with the animation
            if (screenUp) {
                // detect quitting
                if (chamberScreenRef != null && chamberScreenRef.quitNow) {
                    resumePlay();
                }

            } else {
                if (chamberBackground.state == ChamberBackground.State.IN) {
                    // bring up chamber screen
                    GameObject canvasGO = GameObject.FindGameObjectWithTag("Canvas");
                    GameObject csGO = GameObject.Instantiate(chamberScreenGameObject);
                    csGO.transform.SetParent(canvasGO.transform, false);
                    chamberScreenRef = csGO.GetComponent<ChamberScreen>();
                    chamberScreenRef.positionCode = positionCode;
                    Vars.currentNodeData.chamberPositionCode = positionCode;
                    screenUp = true;
                } else if (chamberBackground.state == ChamberBackground.State.OUT) {
                    // chamber background has flown out, resume game
                    Time.timeScale = 1;
                    Player.instance.resumeFromKneel();
                    state = State.IDLE;
                }
            }

            break;
        }

	}

    void updateRays() {

        foreach (Ray ray in rays) {
            float t = time + ray.timeOffset;
            float scale = ray.scaleCenter + Mathf.Sin(t / ray.period * Mathf.PI*2) * ray.scaleRange;
            ray.gameObject.transform.localScale = new Vector3(1, scale, 1);
            Color color = ray.gameObject.GetComponent<SpriteRenderer>().color;
            color.a = 1 - chamberBackground.fadeInter;
            ray.gameObject.GetComponent<SpriteRenderer>().color = color;
        }

    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
        fi.floats["pu"] = phaseUsed;
        fi.bools["phmp"] = playerHasMaxPhase;
        fi.floats["tspop"] = timeSincePlayerOnPlatform;
    }

    void OnRevert(FrameInfo fi) {
        bool prevPlayerHasMaxPhase = playerHasMaxPhase;
        playerHasMaxPhase = fi.bools["phmp"];
        if (prevPlayerHasMaxPhase && !playerHasMaxPhase) {
            // take phase from player
            Player.instance.revertBeforePhasePickup(phaseUsed);
        }
        phaseUsed = fi.floats["pu"];

        time = fi.floats["t"];
        timeSincePlayerOnPlatform = fi.floats["tspop"];
        updateRays();
    }

    

    /// <summary>
    /// Called by a script once to do some hacky stuff with save position issues
    /// </summary>
    void BaseLandingReceiver() {
        if (Vars.currentNodeData != null) {
            Vars.currentNodeData.position = playerSavePosition();
            if (Vars.currentNodeData.parent != null) {
                Vars.currentNodeData.parent.position = playerSavePosition();
            }
        }
    }

    class Ray {
        public GameObject gameObject;
        public float timeOffset = 0;
        public float period = 1;
        public float scaleCenter = 1;
        public float scaleRange = .5f;
    }

    void OnDestroy() {
        if (instance == this)
            instance = null;
        foreach (Ray ray in rays) {
            GameObject.Destroy(ray.gameObject);
        }
        rays.Clear();
    }

    List<Ray> rays = new List<Ray>();

    PolygonCollider2D pc2d;
    TimeUser timeUser;
    ChamberBackground chamberBackground;
    float time = 0;
    float phaseUsed = 0;
    bool playerHasMaxPhase = false;
    float timeSincePlayerOnPlatform = 9999;
    
    bool screenUp = false;
    ChamberScreen chamberScreenRef = null;
    
    
}
