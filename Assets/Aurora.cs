using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Aurora : MonoBehaviour {

    public Color[] colors;
    public float fullDuration = 7.0f;
    public float auroraSpeed = 2.0f;
    public float sliverAliveDuration = 2.0f;
    public float sliverFadeInDuration = .5f;
    public float sliverFadeOutDuration = .5f;
    public float sliverYDiffRange = 1.0f;
    public float waveAmplitude = 2;
    public float wavePeriod = .6f;
    public GameObject auroraSliverGameObject;

    float distUnit {  get {  return 4.0f / CameraControl.PIXEL_PER_UNIT / CameraControl.PIXEL_PER_UNIT_SCALE; } }
    float createRate {  get { return distUnit / auroraSpeed; } }
    int numSlivers {  get {  return Mathf.FloorToInt(sliverAliveDuration / createRate); } }

	void Awake() {
        timeUser = GetComponent<TimeUser>();
	}

    void Start() {
        startPosition = transform.localPosition;
        // create all slivers
        while (slivers.Count < numSlivers) {
            SpriteRenderer ssr = createSliver();
            ssr.enabled = false;
            slivers.Add(ssr);

            yOffsets.Add((timeUser.randomValue() - .5f) * sliverYDiffRange);
        }
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        updateAurora();
	}

    void updateAurora() {

        float enabledDuration = fullDuration - sliverAliveDuration;

        for (int i=0; i<numSlivers; i++) {
            SpriteRenderer ssr = slivers[i];
            float t = time - i * createRate;

            if (t < 0) {
                ssr.enabled = false;
            } else if (t < enabledDuration) {
                ssr.enabled = true;

                // find x position
                float x = i * distUnit;

                // find y position from wave
                float y = Mathf.Sin(t / wavePeriod * Mathf.PI * 2) * waveAmplitude;
                y += yOffsets[i];

                // apply position
                ssr.transform.localPosition = new Vector3(x, y, 0);

                // find color
                float tCol = Utilities.easeInOutQuadClamp(t, 0, .9999f, enabledDuration);
                int colIndex = Mathf.FloorToInt(tCol * (colors.Length - 1));
                float tIndex0 = Utilities.easeInOutQuadClamp(colIndex * 1.0f / (colors.Length - 1), 0, 1, 1);
                float tIndex1 = Utilities.easeInOutQuadClamp((colIndex+1) * 1.0f / (colors.Length - 1), 0, 1, 1);
                //Color col = Color.Lerp(colors[colIndex], colors[colIndex + 1], (tCol - tIndex0) / (tIndex1 - tIndex0));
                Color col = Color.white;

                // find alpha

                float a1;
                if (t < sliverFadeInDuration) {
                    a1 = Utilities.easeLinearClamp(t, 0, 1, sliverFadeInDuration);
                } else if (t < enabledDuration - sliverFadeOutDuration) {
                    a1 = 1;
                } else {
                    a1 = Utilities.easeLinearClamp(enabledDuration - t, 0, 1, sliverFadeOutDuration);
                }

                float a2;
                if (i < numSlivers / 2) {
                    a2 = Utilities.easeLinearClamp(i, 0, 1, numSlivers / 4.0f);
                } else {
                    a2 = Utilities.easeLinearClamp(numSlivers - i, 0, 1, numSlivers / 4.0f);
                }
                col.a = Mathf.Min(a1, a2) * .5f;

                

                // apply color
                ssr.color = col;


            } else {
                ssr.enabled = false;
            }
        }

        //this.transform.localPosition = transform.localPosition + new Vector3(-.5f, 0, 0) * Time.deltaTime;

    }

    

    
    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        updateAurora();
    }

    void OnDestroy() {
        foreach (SpriteRenderer sl in slivers) {
            GameObject.Destroy(sl);
        }
        slivers.Clear();
    }

    TimeUser timeUser;
    Vector3 startPosition = new Vector3();

    SpriteRenderer createSliver() {
        SpriteRenderer ret;
        ret = GameObject.Instantiate(auroraSliverGameObject).GetComponent<SpriteRenderer>();
        
        ret.transform.SetParent(this.transform, false);
        ret.enabled = false;
        return ret;
    }
    List<SpriteRenderer> slivers = new List<SpriteRenderer>();
    List<float> yOffsets = new List<float>();

    float time = 0;
}
