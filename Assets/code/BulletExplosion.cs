using UnityEngine;
using System.Collections;

public class BulletExplosion : MonoBehaviour {

    public float duration = .1f;

    void Awake() {
        timeUser = GetComponent<TimeUser>();
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
