using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

    public static CameraControl instance { get { return _instance; } }

    public static float PIXEL_PER_UNIT = 32;
    public static float PIXEL_PER_UNIT_SCALE = 2;
    public static float ORTHOGRAPHIC_SIZE = 6; //screen height = 768
    //screen size: 1366px x 768px
    //half screen size: 683px x 384px
    //tile size: 16px x 16px
    //room tile size: 44 x 25
    //practical room tile size: 42 x 23 (edge tiles are obscured)
    //orthographic size = screen height / (pixel per unit * pixel per unit scale) / 2

    // Tiled map stuff
    public static Rect getMapBounds() {
        GameObject map = GameObject.FindWithTag("Map");
        Debug.Assert(map != null);
        Tiled2Unity.TiledMap tiledMap = map.GetComponent<Tiled2Unity.TiledMap>();
        Debug.Assert(tiledMap != null);
        return new Rect(
            map.transform.position.x,
            -map.transform.position.y,
            tiledMap.GetMapWidthInPixelsScaled(),
            tiledMap.GetMapHeightInPixelsScaled()
            );
    }

    //////////////////////
    // PUBLIC FUNCTIONS //
    //////////////////////

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

    public void shake(float magnitude = .5f, float duration = .5f) {
        shakeMagnitude = magnitude;
        shakeTime = 0;
        shakeDuration = duration;
    }
    public bool shaking { get { return shakeTime < shakeDuration; } }

    public void hitPause(float duration = .034f) {
        if (Time.timeScale == 0)
            return;
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
    private float hitPauseTime = 0;
    private float hitPauseDuration = 0;
    private float prevTimeScale = 1;

    public void enableEffects(float bloomIntensity, float colorCorrectionSaturation) {
        if (bloomOptimized != null) {
            if (!bloomOptimized.enabled)
                bloomOptimized.enabled = true;
            bloomOptimized.intensity = bloomIntensity;
        }
        if (colorCorrectionCurves != null) {
            if (!colorCorrectionCurves.enabled)
                colorCorrectionCurves.enabled = true;
            colorCorrectionCurves.saturation = colorCorrectionSaturation;
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

        //fullscreen
        if (Input.GetKeyDown(KeyCode.F10)) {
            Screen.fullScreen = !Screen.fullScreen;
        }

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

        if (timeUser.shouldNotUpdate)
            return;

        //handle shaking
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
    }
    void OnRevert(FrameInfo fi) {
        transform.localPosition = new Vector3(fi.floats["x"], fi.floats["y"], transform.localPosition.z);
        shakeMagnitude = fi.floats["sM"];
        shakeTime = fi.floats["sT"];
        shakeDuration = fi.floats["sD"];
        shakePos.x = fi.floats["sPX"];
        shakePos.y = fi.floats["sPY"];
        hitPauseTime = fi.floats["hPT"];
        hitPauseDuration = fi.floats["hPD"];
        prevTimeScale = fi.floats["pTS"];
    }

    void OnDestroy() {
        _instance = null;
    }

    private static CameraControl _instance = null;

    private float shakeMagnitude = 0;
    private float shakeTime = 9999;
    private float shakeDuration = 0;
    private Vector2 shakePos = new Vector2(); //true position = position + shakePos

    private Camera cameraComponent;
    private TimeUser timeUser;
    private UnityStandardAssets.ImageEffects.BloomOptimized bloomOptimized;
    private UnityStandardAssets.ImageEffects.ColorCorrectionCurves colorCorrectionCurves;


}
