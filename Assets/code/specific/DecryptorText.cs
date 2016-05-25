using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DecryptorText : MonoBehaviour {

    public Vector2 startPosition = new Vector2(0, 112);
    public float openDuration = .1f;
    public float closeDuration = .1f;
    public float textSpeed = 10;
    public float textDisplayDelay = .2f;
    public float openedMinDuration = 4.0f;
    public float openedMaxDuration = 10.0f;
    public Decryptor.ID decryptor = Decryptor.ID.NONE;
    public TextAsset textAsset;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip textSound;
    public float textSoundPeriod = .08f;

    public bool closed {  get { return state == State.CLOSED; } }

    public enum State {
        OPEN,
        OPENED,
        CLOSE,
        CLOSED
    }

    public void display(Decryptor.ID decryptor) {
        if (state != State.CLOSED) return;
        this.decryptor = decryptor;
        state = State.OPEN;
        time = 0;
        hide();
        image.enabled = true;
        animator.Play("open");
        SoundManager.instance.playSFXIgnoreVolumeScale(openSound);
    }

	void Awake() {
        image = GetComponent<Image>();
        animator = GetComponent<Animator>();
        ability = transform.Find("Ability").GetComponent<GlyphBox>();
        instructions = transform.Find("Instructions").GetComponent<GlyphBox>();
	}

    void Start() {
        GetComponent<RectTransform>().localPosition = new Vector3(startPosition.x, startPosition.y);
        if (state == State.CLOSED) {
            hide();
        }
    }

    void show() {
        image.enabled = true;
        ability.makeAllCharsVisible();
        instructions.makeAllCharsVisible();
    }

    void hide() {
        image.enabled = false;
        ability.makeAllCharsInvisible();
        instructions.makeAllCharsInvisible();
    }
	
	void Update() {

        time += Time.unscaledDeltaTime;

        switch (state) {
        case State.OPEN:
            if (time >= openDuration) {
                time -= openDuration;
                state = State.OPENED;
                // set text
                Properties prop = new Properties(textAsset.text);
                ability.makeAllCharsVisible();

                bool hasPrerequisites = false;
                System.Collections.Generic.List<Decryptor.ID> prereqs = Decryptor.prerequisiteDecryptors(decryptor);
                if (prereqs.Count == 0) {
                    hasPrerequisites = true;
                } else {
                    if (Decryptor.canUse(decryptor, true, Vars.decryptors)) {
                        hasPrerequisites = true;
                    }
                }

                if (Decryptor.requiresBooster(decryptor) && (Vars.currentNodeData == null || !Vars.currentNodeData.hasBooster)) {
                    // requires booster message
                    ability.setText(prop.getString("decryptor_found") + " " + Decryptor.getName(decryptor));
                    instructions.setText(Decryptor.getName(decryptor) + " " + prop.getString("requires_booster"), 0);
                } else if (hasPrerequisites) {
                    // can use now, standard unlock message
                    //ability.setText(prop.getString("decryptor_get") + " |" + Decryptor.getName(decryptor) + "|");
                    ability.setText(prop.getString("decryptor_left") + Decryptor.getCode(decryptor) + prop.getString("decryptor_right") + " |" + Decryptor.getName(decryptor) + "|");
                    instructions.setPlainText(Decryptor.getDescription(decryptor), 0);
                } else {
                    // requires prerequisites message
                    ability.setText(prop.getString("decryptor_found") + " " + Decryptor.getName(decryptor));
                    // only include missing prereqs
                    for (int i = 0; i < prereqs.Count; i++) {
                        if (Vars.decryptors.IndexOf(prereqs[i]) != -1) {
                            prereqs.RemoveAt(i);
                            i--;
                        }
                    }
                    if (prereqs.Count == 1) {
                        instructions.setText(prop.getString("requires_prerequisite") + " |" + Decryptor.getName(prereqs[0]) + "| " + prop.getString("requires_prerequisite_2"), 0);
                    } else {
                        string prereqText = prop.getString("requires_prerequisites") + " ";
                        for (int i=0; i<prereqs.Count; i++) {
                            prereqText += Decryptor.getName(prereqs[i]);
                            if (i != prereqs.Count - 1) {
                                prereqText += ", ";
                            }
                        }
                        prereqText += ".";
                        instructions.setPlainText(prereqText, 0);
                    }
                    
                }

                
            }
            break;
        case State.OPENED:
            // scroll text
            instructions.visibleChars = Mathf.Clamp(Mathf.FloorToInt((time - textDisplayDelay) * textSpeed), 0, instructions.totalChars);
            bool incrementTextSoundTime = true;
            if (instructions.visibleChars >= instructions.totalChars || instructions.visibleChars <= 0) {
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
            if (time >= openedMinDuration && instructions.visibleChars >= instructions.totalChars) {
                if (Keys.instance.confirmPressed || Keys.instance.backPressed || time >= openedMaxDuration) {
                    toCloseState = true;
                }
            }
            if (toCloseState) {
                state = State.CLOSE;
                time = 0;
                animator.Play("close");
                SoundManager.instance.playSFXIgnoreVolumeScale(closeSound);
                ability.makeAllCharsInvisible();
                instructions.makeAllCharsInvisible();
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
    GlyphBox ability;
    GlyphBox instructions;

    State state = State.CLOSED;
    float time = 0;
    float textSoundTime = 0;

}
