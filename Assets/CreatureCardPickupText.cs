using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CreatureCardPickupText : MonoBehaviour {

    public Vector2 startPosition = new Vector2(0, 112);
    public float openDuration = .1f;
    public float closeDuration = .1f;
    public float textSpeed = 10;
    public float textDisplayDelay = .2f;
    public float openedMinDuration = 4.0f;
    public float openedMaxDuration = 10.0f;
    public int creatureID = -1;
    public bool firstTime = false;
    public TextAsset textAsset;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip textSound;
    public float textSoundPeriod = .08f;

    public bool closed { get { return state == State.CLOSED; } }

    public enum State {
        OPEN,
        OPENED,
        CLOSE,
        CLOSED
    }

    public void display(string creature) {
        display(CreatureCard.getIDFromCardName(creature));
    }
    public void display(int creatureID) {
        if (state != State.CLOSED) return;
        this.creatureID = creatureID;
        state = State.OPEN;
        time = 0;
        hide();
        image.enabled = true;
        animator.Play("open");
        SoundManager.instance.playSFXIgnoreVolumeScale(openSound);

        numLines = prop.getInt("numLines");
        lineIndex = 0;
    }
    
    void Awake() {
        image = GetComponent<Image>();
        animator = GetComponent<Animator>();
        description = transform.Find("Description").GetComponent<GlyphBox>();
        prop = new Properties(textAsset.text);
    }

    void Start() {
        GetComponent<RectTransform>().localPosition = new Vector3(startPosition.x, startPosition.y);
        if (state == State.CLOSED) {
            hide();
        }
    }

    void show() {
        image.enabled = true;
        description.makeAllCharsVisible();
    }

    void hide() {
        image.enabled = false;
        description.makeAllCharsInvisible();
    }

    void Update() {

        time += Time.unscaledDeltaTime;

        switch (state) {
        case State.OPEN:
            if (time >= openDuration) {
                time -= openDuration;
                state = State.OPENED;
                // set text
                if (firstTime) {
                    lineIndex = 0;
                    description.setText(prop.getString("line" + lineIndex), 0);
                } else {
                    description.setText(CreatureCard.getCardDescription(creatureID), 0);
                }
                
            }
            break;
        case State.OPENED:
            // scroll text
            description.visibleChars = Mathf.Clamp(Mathf.FloorToInt((time - textDisplayDelay) * textSpeed), 0, description.totalChars);
            bool incrementTextSoundTime = true;
            if (description.visibleChars >= description.totalChars || description.visibleChars <= 0) {
                incrementTextSoundTime = false;
            }
            if (incrementTextSoundTime) {
                textSoundTime += Time.unscaledDeltaTime;
            }
            if (textSoundTime >= textSoundPeriod) {
                SoundManager.instance.playSFXIgnoreVolumeScale(textSound);
                textSoundTime = 0;
            }

            // detect closing
            bool toCloseState = false;
            if (description.visibleChars >= description.totalChars) {
                if (Keys.instance.confirmPressed || Keys.instance.backPressed || time >= openedMaxDuration) {
                    toCloseState = true;
                }
            }
            if (toCloseState) {
                if (firstTime && lineIndex < numLines) {
                    lineIndex++;
                    if (lineIndex == numLines) {
                        description.setText(CreatureCard.getCardDescription(creatureID), 0);
                    } else {
                        description.setText(prop.getString("line" + lineIndex), 0);
                    }
                    time = 0;
                } else {
                    state = State.CLOSE;
                    time = 0;
                    animator.Play("close");
                    SoundManager.instance.playSFXIgnoreVolumeScale(closeSound);
                    description.makeAllCharsInvisible();
                }
            }
            break;
        case State.CLOSE:
            if (time >= closeDuration) {
                image.enabled = false;
                state = State.CLOSED;
                time = 0;
            }
            break;
        }

    }

    Image image;
    Animator animator;
    GlyphBox description;

    int numLines = 0;
    int lineIndex = 0;
    Properties prop;

    State state = State.CLOSED;
    float time = 0;
    float textSoundTime = 0;

}
