using UnityEngine;
using System.Collections;

public class VisualEffect : MonoBehaviour {

    public float duration = .1f;

    void Awake() {
        timeUser = GetComponent<TimeUser>();
        Debug.Assert(timeUser != null && GetComponent<VisionUser>() != null);
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;
        if (time >= duration) {
            timeUser.timeDestroy();
        }
    }

    TimeUser timeUser;
    float time = 0;

}
