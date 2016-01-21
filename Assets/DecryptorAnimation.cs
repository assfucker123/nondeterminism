using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DecryptorAnimation : MonoBehaviour {

    [System.Serializable]
    public class Ring {
        public int numNumbers = 6;
        public float radius = 2;
        public float revolvePeriod = 4.0f;
        public float growDuration = 1.0f;
        public float collapseDelay = .5f;
        public float collapseDuration = .5f;
    }

    public Ring[] rings;
    public float moveCenterToPlayerDuration = .7f;
    public float circleDuration = 2.0f;

    public GameObject number0GameObject;
    public GameObject number1GameObject;

    [HideInInspector]
    public int randSeed = 0; // set by DecryptorPickup
    [HideInInspector]
    public Vector2 startPos = new Vector2(); // set by DecryptorPickup

    public class Number {
        public float angleOffset = 0; // radians
        public int ringIndex = 0;
        public GameObject gameObject;
    }

    public int numRings {  get { return rings.Length; } }

    public enum State {
        GROW,
        CIRCLE,
        COLLAPSE,
        ENDING,
    }

    void Awake() {
        timeUser = GetComponent<TimeUser>();
	}

    void Start() {

        // create numbers
        for (int i=0; i < rings.Length; i++) {
            Ring ring = rings[i];
            for (int j=0; j<ring.numNumbers; j++) {
                Number num = new Number();
                num.angleOffset = j * 1.0f / ring.numNumbers * Mathf.PI * 2;
                num.ringIndex = i;
                if (timeUser.randomValue() < .5f) {
                    num.gameObject = GameObject.Instantiate(number0GameObject);
                } else {
                    num.gameObject = GameObject.Instantiate(number1GameObject);
                }
                num.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "FG";
                num.gameObject.transform.localPosition = new Vector3(startPos.x, startPos.y);
                numbers.Add(num);
            }
            growDuration = Mathf.Max(growDuration, ring.growDuration);
            collapseDuration = Mathf.Max(collapseDuration, ring.collapseDelay + ring.collapseDuration);
        }

        // stop player
        Player.instance.receivePlayerInput = false;
        CutsceneKeys.allFalse();

    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;
        
        time += Time.deltaTime;

        setPositions();

	}

    void setPositions() {

        if (Player.instance == null)
            return;
        Vector2 playerPos = Player.instance.rb2d.position;
        Ring ring;
        float rad = 0;
        float angle = 0;

        Vector3 center = startPos;
        if (state == State.GROW) {
            // move center towards the player
            center.x = Utilities.easeInOutQuadClamp(time, center.x, playerPos.x - center.x, moveCenterToPlayerDuration);
            center.y = Utilities.easeInOutQuadClamp(time, center.y, playerPos.y - center.y, moveCenterToPlayerDuration);
        } else {
            // set center to player
            center.x = playerPos.x;
            center.y = playerPos.y;
        }


        switch (state) {
        case State.GROW:
            foreach (Number num in numbers) {
                ring = rings[num.ringIndex];
                rad = Utilities.easeInOutQuadClamp(time, 0, ring.radius, ring.growDuration);
                angle = num.angleOffset + time * Mathf.PI * 2 / ring.revolvePeriod;
                setPosition(num, rad, angle, center);
            }
            if (time >= growDuration) {
                state = State.CIRCLE;
                time -= growDuration;
            }
            break;
        case State.CIRCLE:
            foreach (Number num in numbers) {
                ring = rings[num.ringIndex];
                rad = ring.radius;
                angle = num.angleOffset + (time + growDuration) * Mathf.PI * 2 / ring.revolvePeriod;
                setPosition(num, rad, angle, center);
            }
            if (time >= circleDuration) {
                state = State.COLLAPSE;
                time -= circleDuration;
            }
            break;
        case State.COLLAPSE:
            foreach (Number num in numbers) {
                ring = rings[num.ringIndex];
                rad = Utilities.easeInOutQuadClamp(time - ring.collapseDelay, ring.radius, -ring.radius, ring.collapseDuration);
                angle = num.angleOffset + (time + growDuration + circleDuration) * Mathf.PI * 2 / ring.revolvePeriod;
                setPosition(num, rad, angle, center);
                if (time - ring.collapseDelay >= ring.collapseDuration) {
                    num.gameObject.GetComponent<SpriteRenderer>().color = Color.clear;
                } else {
                    num.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                }
            }
            // play "shing" sound effect when a ring collapses
            foreach (Ring ring2 in rings) {
                if (time - Time.deltaTime < ring2.collapseDelay + ring2.collapseDuration &&
                    time >= ring2.collapseDelay + ring2.collapseDuration) {
                    Debug.Log("Shing! sound effect");
                }
            }
            if (time >= collapseDuration) {
                state = State.ENDING;
                time -= collapseDuration;
                foreach (Number num in numbers) {
                    num.gameObject.GetComponent<SpriteRenderer>().color = Color.clear;
                }
                Debug.Log("ending state");
                // should do this later:
                Player.instance.receivePlayerInput = true;
            }
            break;
        case State.ENDING:
            // todo put something here

            break;
        }

    }
    void setPosition(Number num, float rad, float angle, Vector3 center) {
        num.gameObject.transform.localPosition = new Vector3(center.x + rad * Mathf.Cos(angle), center.y + rad * Mathf.Sin(angle));
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["t"] = time;
        fi.bools["rs"] = ranScript;
    }

    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["t"];
        ranScript = fi.bools["rs"];
        setPositions();
    }

    void OnRevertExist() {
        foreach (Number num in numbers) {
            num.gameObject.GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    void OnTimeDestroy() {
        foreach (Number num in numbers) {
            num.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    void OnDestroy() {
        foreach (Number num in numbers) {
            GameObject.Destroy(num.gameObject);
        }
        numbers.Clear();
    }

    TimeUser timeUser;
    float time = 0;
    State state = State.GROW;
    bool ranScript = false;
    float growDuration = 0; // duration for all rings to grow
    float collapseDuration = 0; // duration for all rings to collapse

    List<Number> numbers = new List<Number>();

}
