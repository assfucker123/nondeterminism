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
    public Color flashColor = new Color();
    public float flashInitialDuration = .7f;
    public float flashMiddleTimingOffset = -.2f;
    public float flashMiddleDuration = .4f;
    public float flashLastDuration = .8f;
    public float waitToTextDuration = 2.0f;

    public GameObject number0GameObject;
    public GameObject number1GameObject;
    public GameObject decryptorTextGameObject;

    public AudioClip getSound;
    public AudioClip flashMediumSound;
    public AudioClip flashFinalSound;

    [HideInInspector]
    public Decryptor.ID decryptor = Decryptor.ID.NONE; // set by DecryptorPickup
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
        WAIT_TO_TEXT,
        ENDING,
    }

    void Awake() {
        
	}

    void Start() {

        // create numbers
        Random.seed = randSeed;
        for (int i=0; i < rings.Length; i++) {
            Ring ring = rings[i];
            for (int j=0; j<ring.numNumbers; j++) {
                Number num = new Number();
                num.angleOffset = j * 1.0f / ring.numNumbers * Mathf.PI * 2;
                num.ringIndex = i;

                if (Random.Range(0, 2) == 0) {
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

        // pause game
        HUD.instance.createPauseScreenLight();
        PauseScreen.instance.pauseGameDecryptor();

        // flash screen
        HUD.instance.speedLines.flash(Color.clear, flashColor, flashInitialDuration);
        SoundManager.instance.playSFXIgnoreVolumeScale(getSound);

    }
	
	void Update() {

        time += Time.unscaledDeltaTime;

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
                rad = Utilities.easeInQuadClamp(time - ring.collapseDelay, ring.radius, -ring.radius, ring.collapseDuration);
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
                if (time - Time.unscaledDeltaTime - flashMiddleTimingOffset < ring2.collapseDelay + ring2.collapseDuration &&
                    time - flashMiddleTimingOffset >= ring2.collapseDelay + ring2.collapseDuration) {
                    if (time - flashMiddleTimingOffset >= collapseDuration) {
                        HUD.instance.speedLines.flash(Color.clear, flashColor, flashLastDuration);
                        //HUD.instance.speedLines.flash(Color.clear, flashColor, flashMiddleDuration);
                    } else {
                        HUD.instance.speedLines.flash(Color.clear, flashColor, flashMiddleDuration);
                    }

                    if (Mathf.Abs(ring2.collapseDelay + ring2.collapseDuration - collapseDuration) < .01f) {
                        SoundManager.instance.playSFXIgnoreVolumeScale(flashFinalSound);
                    } else {
                        SoundManager.instance.playSFXIgnoreVolumeScale(flashMediumSound);
                    }
                }
            }
            if (time >= collapseDuration) {
                state = State.WAIT_TO_TEXT;
                time -= collapseDuration;
                foreach (Number num in numbers) {
                    num.gameObject.GetComponent<SpriteRenderer>().color = Color.clear;
                }
                
            }
            break;
        case State.WAIT_TO_TEXT:
            if (time >= waitToTextDuration) {
                state = State.ENDING;
                time -= waitToTextDuration;

                // make text explaining what decryptor does
                GameObject dtGO = GameObject.Instantiate(decryptorTextGameObject);
                dtGO.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
                createdDecryptorText = dtGO.GetComponent<DecryptorText>();
                createdDecryptorText.display(decryptor);
            }
            break;
        case State.ENDING:
            // wait until decryptorText closes
            if (createdDecryptorText == null || createdDecryptorText.closed) {
                // add decryptor to save file
                Vars.collectDecryptor(decryptor);
                // unpause game
                PauseScreen.instance.unpauseGame();
                GameObject.Destroy(createdDecryptorText.gameObject);
                GameObject.Destroy(gameObject);
            }
            break;
        }

    }
    void setPosition(Number num, float rad, float angle, Vector3 center) {
        num.gameObject.transform.localPosition = new Vector3(center.x + rad * Mathf.Cos(angle), center.y + rad * Mathf.Sin(angle));
    }
    
    void OnDestroy() {
        foreach (Number num in numbers) {
            GameObject.Destroy(num.gameObject);
        }
        numbers.Clear();
    }

    float time = 0;
    State state = State.GROW;
    bool ranScript = false;
    float growDuration = 0; // duration for all rings to grow
    float collapseDuration = 0; // duration for all rings to collapse
    DecryptorText createdDecryptorText = null;

    List<Number> numbers = new List<Number>();

}
