using UnityEngine;
using System.Collections;

public class TitleBossShadows : MonoBehaviour {

    public enum ShadowID {
        NONE,
        SHERIVICE
    }

    public static ShadowID shadowID {
        get { return _shadowID; }
        set {
            if (_shadowID == value) return;
            _shadowID = value;
            if (instance == null) return;

            if (_shadowID == ShadowID.NONE) {
                instance.spriteRenderer.enabled = false;
                return;
            }
            instance.spriteRenderer.enabled = true;
            Sprite sprite = instance.spriteRenderer.sprite;
            switch (_shadowID) {
            case ShadowID.SHERIVICE:
                sprite = instance.sheriviceSprite;
                break;
            }
            instance.spriteRenderer.sprite = sprite;
        }
    }
    
    public Sprite sheriviceSprite;

    public Color darkColor = new Color(21/255f,24/255f,28/255f);
    public Color brightColor = new Color(62/255f, 68/255f, 61/255f);
    public float delayMin = 2;
    public float delayMax = 6;
    public float flashOnDuration = .1f;
    public float flashOffDuration = .1f;
    public int numFlashes = 2;

	void Awake() {
        instance = this;
        spriteRenderer = GetComponent<SpriteRenderer>();
        bg = transform.Find("BG").GetComponent<SpriteRenderer>();
        spriteRenderer.color = darkColor;
        bg.color = darkColor;
    }

    void Start() {
        ShadowID temp = shadowID;
        _shadowID = ShadowID.NONE;
        shadowID = temp;
        shadowID = ShadowID.SHERIVICE;

        time = 0;
        duration = Random.Range(delayMin, delayMax);
        state = State.DELAY;
        bg.color = darkColor;
    }

    enum State {
        DELAY,
        FLASH_ON,
        FLASH_OFF
    }
	
	void Update() {

        time += Time.unscaledDeltaTime;

        switch (state) {
        case State.DELAY:
            if (time >= duration) {
                time -= duration;
                bg.color = brightColor;
                state = State.FLASH_ON;
                count = 0;
            }
            break;
        case State.FLASH_ON:
            if (time >= flashOnDuration) {
                time -= flashOnDuration;
                bg.color = darkColor;
                count++;
                if (count >= numFlashes) {
                    state = State.DELAY;
                    duration = Random.Range(delayMin, delayMax);
                } else {
                    state = State.FLASH_OFF;
                }
            }
            break;
        case State.FLASH_OFF:
            if (time >= flashOffDuration) {
                time -= flashOffDuration;
                bg.color = brightColor;
                state = State.FLASH_ON;
            }
            break;
        }

	}

    SpriteRenderer spriteRenderer;
    SpriteRenderer bg;
    float time = 0;
    float duration = 0;
    int count = 0;
    State state = 0;

    static TitleBossShadows instance;
    static ShadowID _shadowID = ShadowID.NONE;
}
