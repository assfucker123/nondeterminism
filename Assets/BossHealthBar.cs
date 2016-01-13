using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossHealthBar : MonoBehaviour {

    public int maxHealth = 50;
    public int health = 50;
    public float fadeInDuration = 1.0f;
    public float healthScaleSpeed = .2f; // scale per second
    public float healthScale = 1;
    public float shadowScale = 1;
    public float shadowDelayDuration = 1.0f;
    public float shadowScaleSpeed = .6f;
    public AudioClip destroySound;
    public float pieceSpawnDelayInterval = .2f;
    public GameObject pieceGameObject;
    public PieceSpawn[] pieceSpawns;
    

    [System.Serializable]
    public class PieceSpawn {
        public Sprite sprite;
        public Vector2 location = new Vector2();
        public Vector2 velocity = new Vector2();
        public float angularVelocity = 0;
    }

    public enum State {
        FADING_IN,
        IDLE,
        DESTROYED
    }
    public State state {  get { return _state; } }
    public bool shadowDelayed { get { return shadowDelay < shadowDelayDuration; } }

    public void setHealth(int newHealth, bool immediately = false) {

        health = newHealth;

        if (immediately) {
            healthScale = getScale(health, maxHealth);
            shadowScale = healthScale;
            shadowDelay = 9999;
        } else {
            shadowDelay = 0;
        }
        
    }
    
    public bool hidden {  get { return _hidden; } }

    public void hide() {
        if (hidden)
            return;

        redMask.enabled = false;
        red.enabled = false;
        shadowMask.enabled = false;
        shadow.enabled = false;
        BG.enabled = false;
        border.enabled = false;
        _hidden = true;
    }

    public void show() {
        if (!hidden)
            return;
        
        redMask.enabled = true;
        red.enabled = true;
        shadowMask.enabled = true;
        shadow.enabled = true;
        BG.enabled = true;
        border.enabled = true;
        _hidden = false;
    }

    public void fadeIn() {
        if (state == State.FADING_IN)
            return;

        show();
        healthScale = 0;
        shadowScale = 0;
        setHealth(maxHealth);

        time = 0;
        _state = State.FADING_IN;
    }

    public void destroy() {
        if (state == State.DESTROYED)
            return;

        for (int i=0; i<pieceSpawns.Length; i++) {
            PieceSpawn ps = pieceSpawns[i];

            GameObject psGO = GameObject.Instantiate(pieceGameObject);
            psGO.transform.SetParent(transform, false);
            BossHealthBarPiece bhbp = psGO.GetComponent<BossHealthBarPiece>();
            bhbp.velocity = ps.velocity;
            bhbp.angularVelocity = ps.angularVelocity;
            bhbp.delay = pieceSpawnDelayInterval * i;
            Image bhbpImage = psGO.GetComponent<Image>();
            bhbpImage.sprite = ps.sprite;
            bhbpImage.GetComponent<RectTransform>().sizeDelta = new Vector2(ps.sprite.rect.width*2, ps.sprite.rect.height*2);
            psGO.GetComponent<RectTransform>().localPosition = ps.location;

        }

        SoundManager.instance.playSFX(destroySound);

        hide();

        _state = State.DESTROYED;
    }

    /////////////
    // PRIVATE //
    /////////////

    float getScale(int health, int maxHealth) {
        return health * 1.0f / maxHealth;
    }

    void updateHealth() {
        if (hidden)
            return;

        // update scale over time
        float targetScale = getScale(health, maxHealth);
        if (healthScale < targetScale) {
            healthScale = Mathf.Min(targetScale, healthScale + healthScaleSpeed * Time.deltaTime);
        } else {
            healthScale = Mathf.Max(targetScale, healthScale - healthScaleSpeed * Time.deltaTime);
        }

        redMask.GetComponent<RectTransform>().localScale = new Vector3(1, healthScale, 1);

        // update shadow scale over time (if not delayed)
        shadowScale = Mathf.Max(shadowScale, healthScale);
        if (!shadowDelayed) {
            targetScale = healthScale;
            shadowScale = Mathf.Max(targetScale, shadowScale - shadowScaleSpeed * Time.deltaTime);
        }

        shadowMask.GetComponent<RectTransform>().localScale = new Vector3(1, shadowScale, 1);

        // update alpha for fading in
        float alpha = 1;
        if (state == State.FADING_IN) {
            alpha = Utilities.easeLinearClamp(time, 0, 1, fadeInDuration);
        }
        redMask.color = new Color(1, 1, 1, alpha);
        red.color = new Color(1, 1, 1, alpha);
        BG.color = new Color(1, 1, 1, alpha);
        border.color = new Color(1, 1, 1, alpha);

    }

    void Awake() {
        timeUser = GetComponent<TimeUser>();
        redMask = transform.Find("RedMask").GetComponent<Image>();
        red = redMask.transform.Find("Red").GetComponent<Image>();
        shadowMask = transform.Find("ShadowMask").GetComponent<Image>();
        shadow = shadowMask.transform.Find("Shadow").GetComponent<Image>();
        BG = transform.Find("BG").GetComponent<Image>();
        border = transform.Find("Border").GetComponent<Image>();
    }

    void Start() {
        
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;
        shadowDelay += Time.deltaTime;

        switch (state) {
        case State.IDLE:
            break;
        case State.FADING_IN:
            if (time >= fadeInDuration) {
                _state = State.IDLE;
            }
            break;
        }

        updateHealth();

    }

    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["t"] = time;
        fi.ints["h"] = health;
        fi.ints["mh"] = maxHealth;
        fi.ints["hee"] = healthEaseEnd;
        fi.floats["hs"] = healthScale;

        fi.floats["ss"] = shadowScale;
        fi.floats["sd"] = shadowDelay;

        fi.bools["h"] = hidden;
    }

    void OnRevert(FrameInfo fi) {
        _state = (State)fi.state;
        time = fi.floats["t"];
        health = fi.ints["h"];
        maxHealth = fi.ints["mh"];
        healthEaseEnd = fi.ints["hee"];
        healthScale = fi.floats["hs"];

        shadowScale = fi.floats["ss"];
        shadowDelay = fi.floats["sd"];

        bool prevHidden = hidden;
        bool nowHidden = fi.bools["h"];
        if (nowHidden != prevHidden) {
            if (nowHidden)
                hide();
            else
                show();
        }

        updateHealth();
    }

    TimeUser timeUser;

    Image redMask;
    Image red;
    Image shadowMask;
    Image shadow;
    Image BG;
    Image border;

    bool _hidden = false;
    float time = 0;
    State _state = State.IDLE;
    int healthEaseEnd = 0;

    float shadowDelay = 9999;
    
    
    
}
