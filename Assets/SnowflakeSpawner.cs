using UnityEngine;
using System.Collections;

/* Spawns Snowflakes.
 * Height of the BoxCollider2D does not matter.
 * Spawner position does not move. */

public class SnowflakeSpawner : MonoBehaviour {

    public float density = 20f;
    public float snowflakeDuration = 10.0f;
    public GameObject snowflakeGameObject;

	void Awake() {
        timeUser = GetComponent<TimeUser>();
	}

    void Start() {
        BoxCollider2D bc2d = GetComponent<BoxCollider2D>();
        left = bc2d.bounds.min.x;
        right = bc2d.bounds.max.x;
    }
	
	void Update() {
        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        float w = right - left;
        float dur = w / density;
        while (time >= dur) {
            //create Snowflake
            GameObject sfGO = GameObject.Instantiate(snowflakeGameObject,
                new Vector3(left + timeUser.randomValue() * w,
                    transform.localPosition.y, 0), Quaternion.identity) as GameObject;
            Snowflake sf = sfGO.GetComponent<Snowflake>();
            sf.swayTimeOffset = timeUser.randomValue() * sf.swayPeriod;
            sf.duration = snowflakeDuration;
            
            time -= dur;
        }
        

	}

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
    }
    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
    }
	
	// components
    TimeUser timeUser;

    float time = 0;
    float left = 0;
    float right = 0;
}
