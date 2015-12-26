using UnityEngine;
using System.Collections;

public class ShipFlameSpawner : MonoBehaviour {

    public float ySpread = 4;
    public float spawnRate = 10; // per second
    public float flameSpeedMin = 5;
    public float flameSpeedMax = 10;
    public float flameSpeedPeriod = 2.0f;

    public GameObject shipFlameGameObject;

	void Awake() {
        timeUser = GetComponent<TimeUser>();
	}
	
	void Update() {
        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;
        speedTime += Time.deltaTime;

        while (time > 1.0f / spawnRate) {

            Vector2 pos = new Vector2();
            pos.y = (timeUser.randomValue() * 2 - 1) * ySpread / 2;
            float speed = Mathf.Sin(speedTime / flameSpeedPeriod * Mathf.PI * 2) * (flameSpeedMax - flameSpeedMin) / 2 + (flameSpeedMin + flameSpeedMax) / 2;

            GameObject shipFlameGO = GameObject.Instantiate(shipFlameGameObject, transform.localPosition + new Vector3(pos.x, pos.y), Quaternion.identity) as GameObject;
            Rigidbody2D shipFlameRB2D = shipFlameGO.GetComponent<Rigidbody2D>();
            shipFlameRB2D.velocity = new Vector2(-speed, 0);
            
            time -= 1.0f / spawnRate;
        }

	}

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
        fi.floats["st"] = speedTime;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        speedTime = fi.floats["st"];
    }

    float time = 0;
    float speedTime = 0;

    TimeUser timeUser;
}
