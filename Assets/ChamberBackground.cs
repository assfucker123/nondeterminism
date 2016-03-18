using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChamberBackground : MonoBehaviour {

    public float widthUnits = 650 / 16f;
    public float triFlyInDelay = .02f;
    public float triFlyInDuration = .5f;
    public int maxLevel = 9;
    public float flyInStateDuration = 3;
    public float startDist = 10;
    public float startAngleOffset = 60;
    public GameObject upTriGameObject;
    public GameObject downTriGameObject;

    public bool isSetUp { get; private set; }

    public enum State {
        NONE,
        FLY_IN,
        IN
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
            CameraControl.getMapBounds().width / 2 - (widthInTris / 2) * triWidth,
            CameraControl.getMapBounds().height / 2 - (heightInTris / 2) * triHeight);

        //distOffset.Set(0, 0);

        numTris = 0;
        for (int l=0; l<maxLevel; l++) {
            numTris += 6 * (2 * l + 1);
        }
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
        state = State.FLY_IN;
        time = 0;
    }
	
	void Update() {

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            flyIn();
        }

        time += Time.unscaledDeltaTime;

        switch (state) {
        case State.NONE:
            break;
        case State.FLY_IN:
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
                t = time - triFlyInDelay * i;
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
                state = State.IN;
            }
            break;
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
    List<List<ChamberTri>> chamberTris = null;
    State state = State.NONE;
    float time = 0;
}
