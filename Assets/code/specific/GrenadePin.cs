using UnityEngine;
using System.Collections;

public class GrenadePin : MonoBehaviour {

    public Vector2 spawnPos = new Vector2(0, 0);
    public float spawnRot = 0;
    public float duration = .5f;

    void Awake() {
        timeUser = GetComponent<TimeUser>();
    }

    void Start() {

    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        if (time >= duration) {
            timeUser.timeDestroy();
        }

    }

    /* called at the end of a frame to record information */
    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
    }

    float time = 0;

    // components
    TimeUser timeUser;

}
