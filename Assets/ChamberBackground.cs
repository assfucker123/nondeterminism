using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChamberBackground : MonoBehaviour {

    public float widthUnits = 650 / 16f;
    public float triFlyInDelay = .02f;
    public float triFlyInDuration = .5f;
    public int maxLevel = 9;
    public float startDist = 10;
    public float startAngleOffset = 60;
    public float fadeAdditionalDuration = .4f;
    public Color triColor1 = new Color();
    public Color triColor2 = new Color();
    public float triColorPeriod = 2;
    public GameObject upTriGameObject;
    public GameObject downTriGameObject;
    public AudioClip flyInSound;
    public AudioClip flyOutSound;

    public State state { get; private set; }
    public bool isSetUp { get; private set; }
    public float fadeInter {
        get {
            switch (state) {
            case State.FLY_IN:
                return Mathf.Clamp(time / flyInStateDuration, 0, 1);
            case State.IN:
                return 1;
            case State.FLY_OUT:
                return 1 - Mathf.Clamp(time / flyInStateDuration, 0, 1);
            default:
                return 0;
            }
        }
    }

    public enum State {
        NONE,
        FLY_IN,
        IN,
        FLY_OUT,
        OUT
    }

	void Awake() {
        isSetUp = false;
	}

    public void setUp() {
        if (isSetUp) return;
        // calculations
        triWidth = upTriGameObject.GetComponent<ChamberTri>().width / 2;
        triHeight = upTriGameObject.GetComponent<ChamberTri>().height;
        //maxLevel = Mathf.CeilToInt((widthUnits / triWidth * 1 - 3) / 4); // is now set manually
        int widthInTris = 4*(maxLevel-1) + 3;
        int heightInTris = 2*(maxLevel-1) + 2;
        centerX = widthInTris / 2;
        centerY = heightInTris / 2;

        distOffset.Set(
            CameraControl.getMapBounds().width / 2 - ((widthInTris + 1) / 2) * triWidth,
            CameraControl.getMapBounds().height / 2 - (heightInTris / 2) * triHeight);

        //distOffset.Set(0, 0);

        numTris = 0;
        for (int l=0; l<maxLevel; l++) {
            numTris += 6 * (2 * l + 1);
        }
        flyInStateDuration = numTris * triFlyInDelay + triFlyInDuration + fadeAdditionalDuration;
        // create tris
        clear();
        chamberTris = new List<List<ChamberTri>>();
        Vector2 centerPos = new Vector2(
             (centerX+.5f) * triWidth + distOffset.x,
             (centerY+.5f) * triHeight + distOffset.y);
        float angle = 0;
        Vector2 diff = new Vector2();
        for (int x=0; x<=widthInTris; x++) {
            List<ChamberTri> col = new List<ChamberTri>();
            for (int y=0; y<=heightInTris; y++) {
                ChamberTri ct;
                if ((x + y) % 2 == 0) {
                    ct = (GameObject.Instantiate(upTriGameObject)).GetComponent<ChamberTri>();
                } else {
                    ct = (GameObject.Instantiate(downTriGameObject)).GetComponent<ChamberTri>();
                }
                ct.targetPos.Set(
                    x * triWidth + distOffset.x,
                    y * triHeight + distOffset.y);
                diff.Set(ct.targetPos.x - centerPos.x, ct.targetPos.y - centerPos.y);
                angle = Mathf.Atan2(diff.y, diff.x);
                angle += startAngleOffset * Mathf.Deg2Rad;
                ct.startPos.Set(
                    ct.targetPos.x + startDist * Mathf.Cos(angle),
                    ct.targetPos.y + startDist * Mathf.Sin(angle));
                ct.timeOffset = Random.value * triColorPeriod;
                ct.spriteRenderer.enabled = false;
                col.Add(ct);
            }
            chamberTris.Add(col);
        }
        isSetUp = true;
    }

    public void flyIn() {
        if (!isSetUp) {
            setUp();
        }
        moveFadingForegroundsToPlatformsLayer();
        state = State.FLY_IN;
        time = 0;
        SoundManager.instance.playSFXIgnoreVolumeScale(flyInSound);
    }

    public void flyOut() {
        if (!isSetUp) {
            setUp();
        }
        state = State.FLY_OUT;
        time = 0;
        SoundManager.instance.playSFXIgnoreVolumeScale(flyOutSound);
    }

    void Update() {
        
        time += Time.unscaledDeltaTime;

        switch (state) {
        case State.NONE:
            break;
        case State.FLY_IN:
        case State.FLY_OUT:
            int levelAt = maxLevel-1;
            int x = centerX - levelAt;
            int y = centerY - levelAt;
            int dir = 0; // [0, 5]
            int index = 0;
            int trisPerSide = 2 * levelAt + 1;
            float t = 0;
            float inter = 0; // from [0, 1]
            ChamberTri ct = null;
            for (int i=0; i<numTris; i++) {
                // set position and alpha of tri
                if (state == State.FLY_IN) {
                    t = time - triFlyInDelay * i;
                } else {
                    t = (flyInStateDuration - time) - triFlyInDelay * i;
                }
                inter = Utilities.easeOutQuadClamp(t, 0, 1, triFlyInDuration);
                ct = chamberTris[x][y];
                ct.parallax.position.Set(
                    ct.startPos.x + (ct.targetPos.x - ct.startPos.x) * inter,
                    ct.startPos.y + (ct.targetPos.y - ct.startPos.y) * inter);
                ct.alpha = Utilities.easeInQuadClamp(t, 0, 1, triFlyInDuration);
                // move to next tri (depends on direction dir)
                switch (dir) {
                case 0:
                    if (index % 2 == 0)
                        y++;
                    else
                        x--;
                    break;
                case 1:
                    if (index % 2 == 0)
                        x++;
                    else
                        y++;
                    break;
                case 2:
                    x++;
                    break;
                case 3:
                    if (index % 2 == 0)
                        y--;
                    else
                        x++;
                    break;
                case 4:
                    if (index % 2 == 0)
                        x--;
                    else
                        y--;
                    break;
                case 5:
                    x--;
                    break;
                }
                index++;
                // if at end of edge
                if (index >= trisPerSide) {
                    // switch direction
                    switch (dir) {
                    //case 0: dir = 2; break;
                    //case 2: dir = 4; break;
                    //case 4: dir = 1; break;
                    //case 1: dir = 3; break;
                    //case 3: dir = 5; break;
                    case 5:
                        // next level
                        levelAt--;
                        trisPerSide = 2 * levelAt + 1;
                        dir = 0;
                        x = centerX - levelAt;
                        y = centerY - levelAt;
                        break;
                    default:
                        dir++;
                        break;
                    }
                    index = 0;
                }
            } // ends for
            // end of fly in state
            if (time >= flyInStateDuration) {
                if (state == State.FLY_IN) {
                    state = State.IN;
                } else {
                    state = State.OUT;
                    moveFadingForegroundsToDefaultLayer();
                }
            }

            updateColors();
            break;
        case State.IN:
            updateColors();
            break;
        }


	}

    void updateColors() {
        colorTime += Time.unscaledDeltaTime;

        /*
        float curveDistBase = colorTime * triCurveSpeed;

        float centerDistX = chamberTris[centerX][0].targetPos.x;

        for (int y=0; y<chamberTris[0].Count; y++) {
            float curveCenter = curveDistBase + triCurveDistRowOffset * y;
            curveCenter = Utilities.fmod(curveCenter, triCurvePeriod);
            
            curveCenter += centerDistX - triCurvePeriod / 2;

            for (int x=0; x<chamberTris.Count; x++) {
                ChamberTri tri = chamberTris[x][y];
                // get interpolation value (higher the closer to curveCenter)
                float inter = 0;
                if (tri.targetPos.x < curveCenter) {
                    inter = Utilities.easeInOutQuadClamp(tri.targetPos.x - (curveCenter - triCurveWidth / 2), 0, 1, triCurveWidth/2);
                } else {
                    inter = Utilities.easeInOutQuadClamp(tri.targetPos.x - curveCenter, 1, -1, triCurveWidth/2);
                }
                // set color
                tri.spriteRenderer.color = Color.Lerp(triColor1, triColor2, inter);
            }

        }
        */

        for (int x=0; x<chamberTris.Count; x++) {
            for (int y=0; y<chamberTris[x].Count; y++) {
                ChamberTri ct = chamberTris[x][y];
                float time = colorTime + ct.timeOffset;
                time = Utilities.fmod(time, triColorPeriod) / triColorPeriod;
                float inter = (Mathf.Cos(time * Mathf.PI*2) + 1) / 2;
                ct.color = Color.Lerp(triColor1, triColor2, inter);
            }
        }

    }

    void moveFadingForegroundsToPlatformsLayer() {
        FadingForeground[] ffs = GameObject.FindObjectsOfType<FadingForeground>();
        foreach (FadingForeground ff in ffs) {
            ff.GetComponent<SpriteRenderer>().sortingLayerName = "Platforms";
        }
    }

    void moveFadingForegroundsToDefaultLayer() {
        FadingForeground[] ffs = GameObject.FindObjectsOfType<FadingForeground>();
        foreach (FadingForeground ff in ffs) {
            ff.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        }
    }

    void clear() {
        if (chamberTris != null) {
            foreach (List<ChamberTri> col in chamberTris) {
                foreach (ChamberTri ct in col) {
                    if (ct != null)
                        GameObject.Destroy(ct.gameObject);
                }
                col.Clear();
            }
            chamberTris.Clear();
        }
        isSetUp = false;
    }

    void OnDestroy() {
        clear();
    }

    int centerX = 0;
    int centerY = 0;
    Vector2 distOffset = new Vector2();
    float triWidth = 0;
    float triHeight = 0;
    int numTris = 0;
    float flyInStateDuration = 3;
    List<List<ChamberTri>> chamberTris = null;
    
    float time = 0;
    float colorTime = 0;
}
