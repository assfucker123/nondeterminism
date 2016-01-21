using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DecryptorPickup : MonoBehaviour {

    public Decryptor.ID decryptor = Decryptor.ID.NONE;
    public float bobPeriod = 2.0f;
    public float bobDist = .3f;
    public int numNumbers = 6;
    public float numberXDist = 1;
    public float numberXDistVariation = .2f;
    public float numberXPeriod = .4f;
    public float numberXPeriodVariation = .1f;
    public float numberYDist = .5f;
    public float numberYDistVariation = .1f;
    public float numberYPeriod = 1.2f;
    public float numberYPeriodVariation = .3f;
    public int randSeed = 132549871;

    public GameObject number0GameObject;
    public GameObject number1GameObject;
    public GameObject animationGameObject;
    public Material grayscaleMaterial;

    public class Number {
        public float xTimeOffset = 0;
        public float xDist = 0;
        public float xPeriod = 1;
        public float yTimeOffset = 0;
        public float yDist = 0;
        public float yPeriod = 1;
        public bool reversed = false;
        public GameObject gameObject;
    }

	void Awake() {
        timeUser = GetComponent<TimeUser>();
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
	}

    void Start() {
        startPos = rb2d.position;
        timeUser.setRandSeed(randSeed);
        // make numbers
        for (int i=0; i<numNumbers; i++) {
            Number num = new Number();
            num.xPeriod = numberXPeriod + (timeUser.randomValue() * 2 - 1) * numberXPeriodVariation;
            num.xTimeOffset = timeUser.randomValue() * num.xPeriod;
            num.xDist = numberXDist + (timeUser.randomValue() * 2 - 1) * numberXDistVariation;

            num.yPeriod = numberYPeriod + (timeUser.randomValue() * 2 - 1) * numberYPeriodVariation;
            num.yTimeOffset = timeUser.randomValue() * num.yPeriod;
            num.yDist = numberYDist + (timeUser.randomValue() * 2 - 1) * numberYDistVariation;

            num.reversed = (timeUser.randomValue() > .5f);
            if (i % 2 == 0) {
                num.gameObject = GameObject.Instantiate(number0GameObject);
            } else {
                num.gameObject = GameObject.Instantiate(number1GameObject);
            }
            num.gameObject.transform.SetParent(transform, false);

            num.gameObject.GetComponent<Animator>().Play("idle", 0, timeUser.randomValue());

            if (Vars.abilityKnown(decryptor)) {
                num.gameObject.GetComponent<SpriteRenderer>().material = grayscaleMaterial;
            }

            numbers.Add(num);
        }
        if (Vars.abilityKnown(decryptor)) {
            spriteRenderer.material = grayscaleMaterial;
        }
    }
	
	void FixedUpdate() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        setPositions();
        
	}

    void setPositions() {

        Vector2 pos = startPos;
        pos.y += Mathf.Sin(time / bobPeriod * Mathf.PI * 2) * bobDist;
        if (timeUser.shouldNotUpdate)
            rb2d.position = pos;
        else
            rb2d.MovePosition(pos);

        for (int i=0; i<numbers.Count; i++) {
            Number num = numbers[i];

            Vector3 numPos = num.gameObject.transform.localPosition;
            float t = time + num.xTimeOffset;
            bool under = (Utilities.fmod(t, num.xPeriod) < num.xPeriod/2);
            if (num.reversed)
                under = !under;
            numPos.x = Mathf.Cos(t / num.xPeriod * Mathf.PI*2) * num.xDist;
            t = time + num.yTimeOffset;
            numPos.y = Mathf.Sin(t / num.yPeriod * Mathf.PI * 2) * num.yDist;
            num.gameObject.transform.localPosition = numPos;
            
            if (under) {
                num.gameObject.GetComponent<SpriteRenderer>().sortingOrder = spriteRenderer.sortingOrder - 1;
            } else {
                num.gameObject.GetComponent<SpriteRenderer>().sortingOrder = spriteRenderer.sortingOrder + 1;
            }
            
        }

    }

    void obtain() {
        if (obtained)
            return;

        // create animation
        DecryptorAnimation dAnim = GameObject.Instantiate(animationGameObject).GetComponent<DecryptorAnimation>();
        dAnim.decryptor = decryptor;
        dAnim.randSeed = randSeed;
        dAnim.startPos = rb2d.position;

        timeUser.timeDestroy();
        obtained = true;

        HUD.instance.createPauseScreenLight();
        PauseScreen.instance.pauseGameDecryptor();
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
        fi.bools["o"] = obtained;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        obtained = fi.bools["o"];
        setPositions();
    }

    void OnRevertExist() {
        foreach (Number num in numbers) {
            num.gameObject.GetComponent<SpriteRenderer>().enabled = true;
        }
        if (Vars.abilityKnown(decryptor)) {
            spriteRenderer.material = grayscaleMaterial;
            foreach (Number num in numbers) {
                num.gameObject.GetComponent<SpriteRenderer>().material = grayscaleMaterial;
            }
        }
    }

    void OnTimeDestroy() {
        foreach (Number num in numbers) {
            num.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    void OnTriggerEnter2D(Collider2D c2d) {
        if (timeUser.shouldNotUpdate)
            return;
        if (c2d.gameObject != Player.instance.gameObject)
            return;
        if (Vars.abilityKnown(decryptor))
            return;

        obtain();
    }

    void OnDestroy() {
        foreach (Number num in numbers) {
            GameObject.Destroy(num.gameObject);
        }
        numbers.Clear();
    }

    TimeUser timeUser;
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;

    Vector2 startPos = new Vector2();
    float time = 0;
    bool obtained = false;

    List<Number> numbers = new List<Number>();
}
