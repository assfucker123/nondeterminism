using UnityEngine;
using System.Collections;

public class Snowflake : MonoBehaviour {

    public float fallSpeed = 10f;
    public float horizSpeed = 5f;
    public float swayMagnitude = 2f;
    public float swayPeriod = 1.0f;
    public float swayTimeOffset = 0; //set by SnowflakeSpawner
    public float time = 0;
    public float duration = 1.5f;
    public Sprite[] sprites;

	void Awake() {
		spriteRenderer = GetComponent<SpriteRenderer>();
        timeUser = GetComponent<TimeUser>();
	}

    void Start() {
        //randomize sprite
        spriteRenderer.sprite = sprites[Mathf.FloorToInt(timeUser.randomValue() * sprites.Length)];
        int rot = Mathf.FloorToInt(timeUser.randomValue() * 4) * 90;
        transform.localRotation = Utilities.setQuat(rot);

        startPos = transform.localPosition;
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;
        float y = startPos.y - fallSpeed * time;
        float x = startPos.x + horizSpeed * time;
        float xOff = swayMagnitude * Mathf.Sin((time + swayTimeOffset) / swayPeriod * Mathf.PI * 2);
        x += xOff;
        transform.localPosition = new Vector3(x, y, transform.localPosition.z);

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
        transform.localPosition = new Vector3(fi.floats["x"], fi.floats["y"], transform.localPosition.z);
    }

	// components
    SpriteRenderer spriteRenderer;
    TimeUser timeUser;

    private Vector3 startPos = new Vector3();
}
