using UnityEngine;
using System.Collections;

public class BulletFade : MonoBehaviour {

    /* BulletFade is a visual effect that is made when a bullet hits something.
     * It quickly dissappears.
     * */

    public float duration = .1f;

    void Awake() {
        timeUser = GetComponent<TimeUser>();
    }
    
    // Update is called once per frame
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
