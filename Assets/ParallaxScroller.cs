using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Parallax))]
[RequireComponent(typeof(TimeUser))]
public class ParallaxScroller : MonoBehaviour {

    public Direction direction = Direction.LEFT;
    public float speed = 10;
    public float distance = 100; 

    public enum Direction {
        LEFT,
        UP,
        RIGHT,
        DOWN
    }

	void Awake() {
        parallax = GetComponent<Parallax>();
        timeUser = GetComponent<TimeUser>();
	}

    void Start() {
        // setting initial position
        parallax.setInitialPositionFromTransform();
        initialPosition = parallax.position;
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;
        setPosition();
	}

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        setPosition();
    }

    void setPosition() {
        Vector2 pos = new Vector2();
        float dist = time*speed;
        dist -= distance * Mathf.Floor(dist / distance);
        switch (direction) {
        case Direction.LEFT:
            pos.x = -dist;
            break;
        case Direction.UP:
            pos.y = dist;
            break;
        case Direction.RIGHT:
            pos.x = dist;
            break;
        case Direction.DOWN:
            pos.y = -dist;
            break;
        }
        parallax.position = initialPosition + pos;
    }

    Vector2 initialPosition = new Vector2();
    float time = 0;

    Parallax parallax;
    TimeUser timeUser;

}
