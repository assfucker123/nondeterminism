using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Aurora : MonoBehaviour {

    public float width = 4;
    public float sliverYDiffRange = 1.0f;
    public float waveAmplitude = 2;
    public float wavePeriod = .6f;
    public float waveOffset = 0;
    public float waveTimeMultiplier = 2;
    public float alphaMax = .5f;
    public float alphaFadeDist = 3;
    public float alphaPanSpeed = 2f;
    public float alphaPanSpread = 6;
    public float alphaPanDecay = .5f;
    public float alphaPanRepeatDelay = 1.0f;
    public float alphaPanTimeOffset = 0;
    public GameObject auroraSliverGameObject;

    float distUnit {  get {  return 4.0f / CameraControl.PIXEL_PER_UNIT / CameraControl.PIXEL_PER_UNIT_SCALE; } }
    int numSlivers {  get {  return Mathf.FloorToInt(width / distUnit); } }

	void Awake() {
        timeUser = GetComponent<TimeUser>();
	}

    void Start() {
        // create all slivers
        while (slivers.Count < numSlivers) {
            SpriteRenderer ssr = createSliver();
            ssr.enabled = true;
            slivers.Add(ssr);

            yOffsets.Add((timeUser.randomValue() - .5f) * sliverYDiffRange);
        }

        panTime += alphaPanTimeOffset;
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;
        panTime += Time.deltaTime;

        updateAurora();
	}

    void updateAurora() {

        for (int i=0; i<numSlivers; i++) {
            SpriteRenderer ssr = slivers[i];

            // find x position
            float x = i * distUnit;

            // y position in wave
            float y = Mathf.Sin((x + waveOffset + time * waveTimeMultiplier) / wavePeriod * Mathf.PI * 2) * waveAmplitude;
            y += yOffsets[i];

            // apply position
            ssr.transform.localPosition = new Vector3(x, y, 0);

            Color color = Color.white;

            // find alpha
            float a = 1;
            if (x < alphaFadeDist) {
                a = Utilities.easeLinearClamp(x, 0, 1, alphaFadeDist);
            }
            if (width - x < alphaFadeDist) {
                a = Utilities.easeLinearClamp(width - x, 0, 1, alphaFadeDist);
            }
            a *= alphaMax;

            // alpha pan
            float panX = panTime * alphaPanSpeed - alphaPanSpread / 2;
            if (panX >= width + alphaPanSpread / 2 + alphaPanRepeatDelay * alphaPanSpeed) {
                panTime = 0;
            }
            a *= Utilities.easeInOutQuadClamp(Mathf.Abs(panX - x), 1, -alphaPanDecay, alphaPanSpread / 2);


            color.a = a;
            ssr.color = color;

            
                // find color
                /*
                float tCol = Utilities.easeInOutQuadClamp(t, 0, .9999f, enabledDuration);
                int colIndex = Mathf.FloorToInt(tCol * (colors.Length - 1));
                float tIndex0 = Utilities.easeInOutQuadClamp(colIndex * 1.0f / (colors.Length - 1), 0, 1, 1);
                float tIndex1 = Utilities.easeInOutQuadClamp((colIndex+1) * 1.0f / (colors.Length - 1), 0, 1, 1);
                //Color col = Color.Lerp(colors[colIndex], colors[colIndex + 1], (tCol - tIndex0) / (tIndex1 - tIndex0));
                Color col = Color.white;
                */

                // find alpha

            /*
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

                */

                // apply color
                //ssr.color = col;

        
        }

        //this.transform.localPosition = transform.localPosition + new Vector3(-.5f, 0, 0) * Time.deltaTime;

    }

    

    
    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
        fi.floats["pt"] = panTime;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        panTime = fi.floats["pt"];
        updateAurora();
    }

    void OnDestroy() {
        foreach (SpriteRenderer sl in slivers) {
            GameObject.Destroy(sl);
        }
        slivers.Clear();
    }

    TimeUser timeUser;

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
    float panTime = 0;
}
