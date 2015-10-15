using UnityEngine;
using System.Collections;

public class BulletMuzzle : MonoBehaviour {

    public float duration = .2f;

	void Awake () {
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
