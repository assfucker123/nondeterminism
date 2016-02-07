using UnityEngine;
using System.Collections;

public class NeedWeaponParticle : MonoBehaviour {

    public float duration = 1.0f;
    public float speed = 1;
    public float heading = 0;
    public int numParticles = 3;
    public bool playSound = true;
    public AudioClip sound;

	void Awake() {
        timeUser = GetComponent<TimeUser>();
	}

    void Start() {
        if (sound != null && playSound) {
            SoundManager.instance.playSFXRandPitchBend(sound);
        }
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        transform.localPosition += new Vector3(speed * Time.deltaTime * Mathf.Cos(heading * Mathf.PI / 180), speed * Time.deltaTime * Mathf.Sin(heading * Mathf.PI / 180));

        if (time >= duration) {
            timeUser.timeDestroy();
        }

	}

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
        fi.floats["x"] = transform.localPosition.x;
        fi.floats["y"] = transform.localPosition.y;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        transform.localPosition = new Vector3(fi.floats["x"], fi.floats["y"]);
    }

    TimeUser timeUser;

    float time = 0;

}
