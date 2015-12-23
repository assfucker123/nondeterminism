using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FlashbackArtifacts : MonoBehaviour {

    public float alpha = .1f;
    public float alphaDuration = 1.0f; // how long it takes for alpha to go from 0 to 1
    public Vector2 line1Point0 = new Vector2();
    public Vector2 line1Point1 = new Vector2();
    public int line1NumArtifacts = 6;
    public float line1Speed = 300;
    public Vector2 line2Point0 = new Vector2();
    public Vector2 line2Point1 = new Vector2();
    public int line2NumArtifacts = 6;
    public float line2Speed = 300;

    public enum State {
        STOPPED,
        RUNNING
    }

    /* begin flashing artifacts */
    public void begin() {
        time = 0;
        state = State.RUNNING;
    }

    /* stops flashing artifacts, they all disappear */
    public void stop() {
        if (state == State.STOPPED) return;
        foreach (Image art in line1Artifacts) {
            art.enabled = false;
        }
        foreach (Image art in line2Artifacts) {
            art.enabled = false;
        }
        state = State.STOPPED;
    }

    public void setUp() {
        stop();
    }

	void Awake() {
		// get all artifact images
        foreach (Transform childT in transform) {
            Image art = childT.GetComponent<Image>();
            if (art == null) continue;
            art.enabled = false;
            if (art.gameObject.name == "Art1") {
                artifact1 = art.gameObject;
            }
            if (art.gameObject.name == "Art2") {
                artifact2 = art.gameObject;
            }
        }
	}

    void Start() {
        // create lines
        for (int i = 0; i < line1NumArtifacts; i++) {
            GameObject GO = null;
            if (i % 2 == 0) {
                GO = GameObject.Instantiate(artifact1);
            } else {
                GO = GameObject.Instantiate(artifact2);
            }
            GO.transform.SetParent(transform, false);
            Image image = GO.GetComponent<Image>();
            image.enabled = false;
            line1Artifacts.Add(image);
        }
        for (int i = 0; i < line2NumArtifacts; i++) {
            GameObject GO = null;
            if (i % 2 == 0) {
                GO = GameObject.Instantiate(artifact1);
            } else {
                GO = GameObject.Instantiate(artifact2);
            }
            GO.transform.SetParent(transform, false);
            Image image = GO.GetComponent<Image>();
            image.enabled = false;
            line2Artifacts.Add(image);
        }
    }
	
	void Update() {
        if (state == State.RUNNING) {
            time += Time.deltaTime;

            positionLine(line1Artifacts, line1Point0, line1Point1, line1Speed, time);
            positionLine(line2Artifacts, line2Point0, line2Point1, line2Speed, time);

        }
	}

    void OnDestroy() {
        foreach (Image art in line1Artifacts) {
            GameObject.Destroy(art.gameObject);
        }
        foreach (Image art in line2Artifacts) {
            GameObject.Destroy(art.gameObject);
        }
        line1Artifacts.Clear();
    }

    void positionLine(List<Image> line, Vector2 p0, Vector2 p1, float speed, float time) {

        int numArtifacts = line.Count;
        float distance = Vector2.Distance(p0, p1);
        float spacing = distance / numArtifacts;
        float diff = speed * time;
        diff -= Mathf.Floor(diff / distance) * distance;

        for (int i = 0; i < line.Count; i++) {
            float dist = diff + i * spacing;
            dist -= Mathf.Floor(dist / distance) * distance;
            float interpolate = dist / distance;
            Vector2 pos = p0 + (p1 - p0) * interpolate;
            line[i].rectTransform.localPosition = pos;

            // alpha
            float a = Utilities.easeLinearClamp(time, 0, alpha, alphaDuration);
            line[i].enabled = true;
            line[i].color = new Color(1, 1, 1, a);
        }

    }

    GameObject artifact1 = null;
    GameObject artifact2 = null;
    List<Image> line1Artifacts = new List<Image>();
    List<Image> line2Artifacts = new List<Image>();

    State state = State.STOPPED;
    float time = 0;
	
}
