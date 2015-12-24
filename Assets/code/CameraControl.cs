using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

    public static CameraControl instance { get { return _instance; } }

    public static float PIXEL_PER_UNIT = 16;
    public static float PIXEL_PER_UNIT_SCALE = 2;
    public static float ORTHOGRAPHIC_SIZE = 11.25f;
    public static int SCREEN_WIDTH = 1280;
    public static int SCREEN_HEIGHT = 720;
    public static float ROOM_UNIT_WIDTH { get { return SCREEN_WIDTH / PIXEL_PER_UNIT / PIXEL_PER_UNIT_SCALE; } } // = 40
    public static float ROOM_UNIT_HEIGHT { get { return SCREEN_HEIGHT / PIXEL_PER_UNIT / PIXEL_PER_UNIT_SCALE; } } // = 22.5
    //screen size: 1280px x 720px
    //half screen size: 640px x 360px
    //tile size: 16px x 16px
    //room tile size: 41 x 23
    //practical room tile size: 40 x 22 (edge tiles are obscured)
    //orthographic size = screen height / (pixel per unit * pixel per unit scale) / 2

    // Tiled map stuff
    public static Rect getMapBounds() {
        GameObject map = GameObject.FindWithTag("Map");
        Debug.Assert(map != null);
        Tiled2Unity.TiledMap tiledMap = map.GetComponent<Tiled2Unity.TiledMap>();
        Debug.Assert(tiledMap != null);
        return new Rect(
            map.transform.position.x,
            tiledMap.GetMapHeightInPixelsScaled() - map.transform.position.y,
            tiledMap.GetMapWidthInPixelsScaled(),
            tiledMap.GetMapHeightInPixelsScaled()
            );
    }

    //////////////////////
    // PUBLIC FUNCTIONS //
    //////////////////////

    /* Moves the camera to the target position.  Stays within the bounds if bounds are enabled.
     * Set duration to 0 to move there immediately. */
    public void moveToPosition(Vector2 targetPosition, float duration = 0) {
        movePos0 = position;
        movePos1 = targetPosition;
        if (duration < .0001f) {
            if (boundsEnabled) {
                position = fitPositionInBounds(movePos1, bounds, ROOM_UNIT_WIDTH / 2, ROOM_UNIT_HEIGHT / 2);
            } else {
                position = movePos1;
            }
            movePosTime = 0;
            movePosDuration = 0;
        } else {
            movePosTime = 0;
            movePosDuration = duration;
        }
    }
    public bool movingToPosition { get { return movePosTime < movePosDuration; } }
    /* The position chosen to move the camera to can be changed while moving there. */
    public Vector2 targetPosition {
        get { return movePos1; }
        set {
            movePos1 = value;
        }
    }
    
    /* Sets camera position directly, ignoring bounds etc. */
    public Vector2 position {
        get {
            Vector2 ret = new Vector2(transform.localPosition.x, transform.localPosition.y);
            return ret - shakePos;
        }
        set {
            Vector3 transPos = transform.localPosition;
            transPos.x = value.x + shakePos.x;
            transPos.y = value.y + shakePos.y;
            transform.localPosition = transPos;
        }
    }

    public void enableBounds() {
        if (boundsEnabled) return;
        _boundsEnabled = true;
        position = fitPositionInBounds(position, bounds, ROOM_UNIT_WIDTH / 2, ROOM_UNIT_HEIGHT / 2);
    }
    public void enableBounds(Rect bounds) {
        _bounds.xMin = bounds.xMin;
        _bounds.xMax = bounds.xMax;
        _bounds.yMin = bounds.yMin;
        _bounds.yMax = bounds.yMax;
        enableBounds();
    }
    public void disableBounds() {
        if (!boundsEnabled) return;
        _boundsEnabled = false;
    }

    public Rect bounds { get { return _bounds; } }
    public bool boundsEnabled { get { return _boundsEnabled; } }

    public void shake(float magnitude = .5f, float duration = .5f) {
        shakeMagnitude = magnitude;
        shakeTime = 0;
        shakeDuration = duration;
    }
    public bool shaking { get { return shakeTime < shakeDuration; } }

    public void hitPause(float duration = .034f) {
        if (Time.timeScale == 0) {
            hitPauseDuration = Mathf.Max(hitPauseDuration, duration);
            return;
        }
        if (TimeUser.reverting)
            return;
        bool prevHitPaused = hitPaused;
        if (!prevHitPaused) {
            prevTimeScale = Time.timeScale;
        }
        hitPauseTime = 0;
        hitPauseDuration = duration;
        Time.timeScale = 0;
    }
    public bool hitPaused { get { return hitPauseTime < hitPauseDuration; } }
    

    public void enableEffects(float bloomIntensity, float colorCorrectionSaturation, float inversion) {
        if (bloomOptimized != null) {
            if (!bloomOptimized.enabled)
                bloomOptimized.enabled = true;
            bloomOptimized.intensity = bloomIntensity;
        }
        if (colorCorrectionCurves != null) {
            if (!colorCorrectionCurves.enabled)
                colorCorrectionCurves.enabled = true;
            colorCorrectionCurves.saturation = colorCorrectionSaturation;

            // inverting colors
            _inversion = inversion;
            float v0 = Utilities.easeInCubicClamp(inversion, 0, 1, 1);
            float v1 = Utilities.easeOutQuadClamp(inversion, 1, -1, 1);

            colorCorrectionCurves.redChannel = AnimationCurve.Linear(0, v0, 1, v1);
            colorCorrectionCurves.greenChannel = AnimationCurve.Linear(0, v0, 1, v1);
            colorCorrectionCurves.blueChannel = AnimationCurve.Linear(0, v0, 1, v1);
            colorCorrectionCurves.UpdateParameters();
        }
    }
    public void disableEffects() {
        if (bloomOptimized != null)
            bloomOptimized.enabled = false;
        if (colorCorrectionCurves != null) {
            /* New rule: always have colorCorrectionCurves enabled (even if saturation is 1)
             * If the Distort shader is the only shader being used, it causes the distortion to be rendered incorrectly.
             * So always use colorCorrectionCurves. */
            colorCorrectionCurves.saturation = 1;
            //colorCorrectionCurves.enabled = false;

            // undo inverting colors
            _inversion = 0;
            float t = 0;
            colorCorrectionCurves.redChannel = AnimationCurve.Linear(0, 0, 1, 1);
            colorCorrectionCurves.greenChannel = AnimationCurve.Linear(0, 0, 1, 1);
            colorCorrectionCurves.blueChannel = AnimationCurve.Linear(0, 0, 1, 1);
            colorCorrectionCurves.UpdateParameters();
        }
    }
    public bool effectsEnabled { get {
        if (bloomOptimized != null) return bloomOptimized.enabled;
        if (colorCorrectionCurves != null) return colorCorrectionCurves.enabled;
        return false;
    } }
    public float bloomIntensity { get {
        if (bloomOptimized == null) return 0;
        return bloomOptimized.intensity;
    } }
    public float colorCorrectionSaturation { get {
        if (colorCorrectionCurves == null) return 1;
        return colorCorrectionCurves.saturation;
    } }
    public float inversion { get { return _inversion; } }

    //////////////////////
    // HELPER FUNCTIONS //
    //////////////////////

    public Rect viewport {
        get {
            return new Rect(position.x - ROOM_UNIT_WIDTH / 2, position.y - ROOM_UNIT_HEIGHT / 2, ROOM_UNIT_WIDTH, ROOM_UNIT_HEIGHT);
        }
    }
    public Vector2 fitPositionInBounds(Vector2 position, Rect bounds, float roomHalfWidth, float roomHalfHeight) {
        Vector2 pos = position;
        pos.x = Mathf.Clamp(pos.x, bounds.xMin + roomHalfWidth, bounds.xMax - roomHalfWidth);
        pos.y = Mathf.Clamp(pos.y, bounds.yMin + roomHalfHeight, bounds.yMax - roomHalfHeight);
        return pos;
    }

    /////////////
    // PRIVATE //
    /////////////
    
    void Awake() {
        _instance = this;
        cameraComponent = GetComponent<Camera>();
        timeUser = GetComponent<TimeUser>();
        bloomOptimized = GetComponent<UnityStandardAssets.ImageEffects.BloomOptimized>();
        colorCorrectionCurves = GetComponent<UnityStandardAssets.ImageEffects.ColorCorrectionCurves>();
	}

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        // update hitPause
        if (hitPaused) {
            hitPauseTime += Time.unscaledDeltaTime;
            if (!hitPaused) {
                Time.timeScale = prevTimeScale;
            }
        }

    }

    void LateUpdate() {

        if (timeUser.shouldNotUpdate) {
            updateParallaxObjects();
            return;
        }
        

        // move position
        if (movingToPosition) {
            movePosTime += Time.deltaTime;
            Vector2 pos = new Vector2(
                Utilities.easeInOutQuadClamp(movePosTime, movePos0.x, movePos1.x - movePos0.x, movePosDuration),
                Utilities.easeInOutQuadClamp(movePosTime, movePos0.y, movePos1.y - movePos0.y, movePosDuration));
            if (boundsEnabled) {
                position = fitPositionInBounds(pos, bounds, ROOM_UNIT_WIDTH / 2, ROOM_UNIT_HEIGHT / 2);
            } else {
                position = pos;
            }
        }

        // handle shaking
        transform.localPosition -= new Vector3(shakePos.x, shakePos.y, 0);
        if (shaking) {
            shakeTime += Time.deltaTime;
            if (shakeTime >= shakeDuration){
                shakePos.Set(0, 0);
            } else {
                float radius = Utilities.easeOutQuad(shakeTime, shakeMagnitude, -shakeMagnitude, shakeDuration);
                float angle = 0;
                if (shakePos.sqrMagnitude < .0001f) {
                    angle = timeUser.randomValue() * Mathf.PI * 2;
                } else {
                    angle = Mathf.Atan2(shakePos.y, shakePos.x);
                    angle += Mathf.PI + (timeUser.randomValue() * 2 - 1) * Mathf.PI / 3;
                }
                shakePos.x = radius * Mathf.Cos(angle);
                shakePos.y = radius * Mathf.Sin(angle);
            }
        }
        transform.localPosition += new Vector3(shakePos.x, shakePos.y, 0);

        updateParallaxObjects();

    }

    void updateParallaxObjects() {
        // affect parallax objects
        Vector2 camDiff = new Vector2(transform.localPosition.x, transform.localPosition.y)-getMapBounds().center;
        foreach (Parallax parallax in Parallax.parallaxs) {
            parallax.updateTransform(camDiff);
        }
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["x"] = transform.localPosition.x;
        fi.floats["y"] = transform.localPosition.y;
        fi.floats["sM"] = shakeMagnitude;
        fi.floats["sT"] = shakeTime;
        fi.floats["sD"] = shakeDuration;
        fi.floats["sPX"] = shakePos.x;
        fi.floats["sPY"] = shakePos.y;
        fi.floats["hPT"] = hitPauseTime;
        fi.floats["hPD"] = hitPauseDuration;
        fi.floats["pTS"] = prevTimeScale;

        fi.floats["bX"] = bounds.x;
        fi.floats["bW"] = bounds.width;
        fi.floats["bY"] = bounds.y;
        fi.floats["bH"] = bounds.height;
        fi.bools["bEnabled"] = boundsEnabled;

        fi.floats["mP0x"] = movePos0.x;
        fi.floats["mP0y"] = movePos0.y;
        fi.floats["mP1x"] = movePos1.x;
        fi.floats["mP1y"] = movePos1.y;
        fi.floats["mPt"] = movePosTime;
        fi.floats["mPd"] = movePosDuration;
    }
    void OnRevert(FrameInfo fi) {

        bool prevHitPaused = hitPaused;
        float prevPrevTimeScale = prevTimeScale;
        
        transform.localPosition = new Vector3(fi.floats["x"], fi.floats["y"], transform.localPosition.z);
        shakeMagnitude = fi.floats["sM"];
        shakeTime = fi.floats["sT"];
        shakeDuration = fi.floats["sD"];
        shakePos.x = fi.floats["sPX"];
        shakePos.y = fi.floats["sPY"];
        hitPauseTime = fi.floats["hPT"];
        hitPauseDuration = fi.floats["hPD"];
        prevTimeScale = fi.floats["pTS"];
        if (prevHitPaused && !hitPaused) {
            Time.timeScale = prevPrevTimeScale;
        }
        if (fi.bools["bEnabled"]) {
            enableBounds(new Rect(fi.floats["bX"], fi.floats["bY"], fi.floats["bW"], fi.floats["bH"]));
        } else {
            disableBounds();
        }
        movePos0.Set(fi.floats["mP0x"], fi.floats["mP0y"]);
        movePos1.Set(fi.floats["mP1x"], fi.floats["mP1y"]);
        movePosTime = fi.floats["mPt"];
        movePosDuration = fi.floats["mPd"];
    }

    void OnDestroy() {
        _instance = null;
    }

    private static CameraControl _instance = null;

    private float shakeMagnitude = 0;
    private float shakeTime = 9999;
    private float shakeDuration = 0;
    private Vector2 shakePos = new Vector2(); //true position = position + shakePos

    private float hitPauseTime = 0;
    private float hitPauseDuration = 0;
    private float prevTimeScale = 1;

    private Rect _bounds = new Rect();
    private bool _boundsEnabled = false;
    private Vector2 movePos0 = new Vector2();
    private Vector2 movePos1 = new Vector2();
    private float movePosTime = 0;
    private float movePosDuration = 0;

    private Camera cameraComponent;
    private TimeUser timeUser;
    private UnityStandardAssets.ImageEffects.BloomOptimized bloomOptimized;
    private UnityStandardAssets.ImageEffects.ColorCorrectionCurves colorCorrectionCurves;
    private float _inversion = 0;


}
