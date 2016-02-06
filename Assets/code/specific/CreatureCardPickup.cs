using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreatureCardPickup : MonoBehaviour {

    public string creature = "";
    public float circlePeriod = 1.0f;
    public float circleRadius = .2f;
    public Material defaultMaterial;
    public Material grayscaleMaterial;
    public GameObject hudCardGameObject;
    public Color flash1Color = new Color(1, 1, 1, .6f);
    public float flash1ColorDuration = 2f;
    public Color flash2Color = new Color(1, 1, 1, .6f);
    public float flash2ColorDuration = 2f;
    public Vector2 cardStartPos = new Vector2(0, -300);
    public Vector2 cardFinalPos = new Vector2(0, 0);
    public float cardMoveDuration = 1.0f;
    public float midDelayDuration = .3f;
    public float flipDuration = .4f;
    public GameObject particleGameObject;
    public int numRows = 4;
    public float particlesRowSpacing = 60;
    public int particlesPerRow = 10;
    public float particleYRange = 380;
    public float particleFallSpeed = 100;
    public float particleFadeDuration = .4f;
    public float particleExplodeSpeed = 300;
    public float frontShownDelay = 1.0f;
    public GameObject textBoxGameObject;
    public AudioClip firstSound;
    public AudioClip finalSound;

    public enum State {
        NOT_FOUND,
        FOUND,
        COLLECTED
    }

    public State state {
        get {
            return _state;
        }
        set {
            if (value == _state) return;

            if (creatureID == -1)
                creatureID = CreatureCard.getIDFromCardName(creature);

            if (value == State.NOT_FOUND) {
                animator.Play("default");
            }
            if (_state == State.NOT_FOUND) {
                animator.Play("found");
            }
            if (value == State.COLLECTED) {
                spriteRenderer.material = grayscaleMaterial;
                spriteRenderer.enabled = false;
                Vars.creatureCardFind(creatureID);
                Vars.currentNodeData.creatureCardCollect(creatureID);
            }
            if (_state == State.COLLECTED) {
                spriteRenderer.material = defaultMaterial;
                spriteRenderer.enabled = true;
                Vars.currentNodeData.creatureCardCollectUndo(creatureID);
            }

            _state = value;
        }
    }

    public enum AnimationState {
        NO_ANIMATION,
        START,
        MID_DELAY,
        FLIP1,
        FLIP2,
        FRONT_SHOWN,
        TEXT_BOX
    }
    public AnimationState animationState {
        get {
            return _animationState;
        }
        set {
            if (value == _animationState) return;

            _animationState = value;
        }
    }

    public class Particle {
        public GameObject gameObject;
        public Vector2 velocity = new Vector2();
    }

    public bool obtained {  get { return state == State.COLLECTED; } }

	void Awake() {
        timeUser = GetComponent<TimeUser>();
        timeUser.updateAnimatorInfo = false;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
	}

    void Start() {
        startPos = transform.localPosition;
        creatureID = CreatureCard.getIDFromCardName(creature);

        if (Vars.currentNodeData.creatureCardCollected(creatureID)) {
            state = State.COLLECTED;
        } else if (Vars.creatureCardFound(creatureID)) {
            state = State.FOUND;
        } else {
            state = State.NOT_FOUND;
        }

        canvas = GameObject.FindGameObjectWithTag("Canvas");
    }
	
	void Update() {

        // do animation for collecting card
        if (animationState != AnimationState.NO_ANIMATION) {
            animationTime += Time.unscaledDeltaTime;

            switch (animationState) {
            case AnimationState.START:
                creatureCard.GetComponent<RectTransform>().localPosition = new Vector3(
                    Utilities.easeOutQuadClamp(animationTime, cardStartPos.x, cardFinalPos.x - cardStartPos.x, cardMoveDuration),
                    Utilities.easeOutQuadClamp(animationTime, cardStartPos.y, cardFinalPos.y - cardStartPos.y, cardMoveDuration));

                if (animationTime >= cardMoveDuration) {
                    animationState = AnimationState.MID_DELAY;
                    animationTime -= cardMoveDuration;
                }
                break;
            case AnimationState.MID_DELAY:
                if (animationTime >= midDelayDuration) {
                    animationTime -= midDelayDuration;
                    animationState = AnimationState.FLIP1;
                }
                break;
            case AnimationState.FLIP1:
                float scale = Utilities.easeInCircClamp(animationTime, 1, -1, flipDuration/2);
                creatureCard.GetComponent<RectTransform>().localScale = new Vector3(scale, 1, 1);
                if (animationTime >= flipDuration/2) {
                    animationTime -= flipDuration/2;
                    // show front
                    creatureCard.showFront();
                    creatureCard.setCard(creatureID);
                    animationState = AnimationState.FLIP2;
                    // flash screen
                    HUD.instance.speedLines.flash(Color.clear, flash2Color, flash2ColorDuration);
                    // make particles explode
                    foreach (Particle part in particles) {
                        Vector3 diff = part.gameObject.GetComponent<RectTransform>().localPosition - new Vector3(cardFinalPos.x, cardFinalPos.y);
                        part.velocity.Set(diff.x, diff.y);
                        part.velocity.Normalize();
                        part.velocity *= particleExplodeSpeed;
                    }
                    // sound
                    SoundManager.instance.playSFX(finalSound);
                }
                break;
            case AnimationState.FLIP2:
                scale = Utilities.easeInCircClamp(animationTime, 0, 1, flipDuration/2);
                creatureCard.GetComponent<RectTransform>().localScale = new Vector3(scale, 1, 1);
                if (animationTime >= flipDuration / 2) {
                    animationTime -= flipDuration / 2;
                    animationState = AnimationState.FRONT_SHOWN;
                }
                break;
            case AnimationState.FRONT_SHOWN:
                if (animationTime >= frontShownDelay) {
                    animationTime -= frontShownDelay;
                    // make decryptor text
                    GameObject textBoxGO = GameObject.Instantiate(textBoxGameObject);
                    textBoxGO.transform.SetParent(canvas.transform, false);
                    textBox = textBoxGO.GetComponent<CreatureCardPickupText>();
                    if (!Vars.eventHappened(AdventureEvent.Info.FOUND_CREATURE_CARD)) {
                        Vars.eventHappen(AdventureEvent.Info.FOUND_CREATURE_CARD);
                        textBox.firstTime = true;
                    } else {
                        textBox.firstTime = false;
                    }
                    textBox.display(creatureID);
                    animationState = AnimationState.TEXT_BOX;
                }
                break;
            case AnimationState.TEXT_BOX:
                if (textBox.closed) {
                    animationState = AnimationState.NO_ANIMATION;
                    foreach (Particle part in particles) {
                        GameObject.Destroy(part.gameObject);
                    }
                    particles.Clear();
                    GameObject.Destroy(creatureCard.gameObject);
                    creatureCard = null;
                    GameObject.Destroy(textBox.gameObject);
                    textBox = null;

                    PauseScreen.instance.unpauseGame();
                }
                break;
            }

            // animate particles
            foreach (Particle part in particles) {
                Vector3 pPos = part.gameObject.GetComponent<RectTransform>().localPosition;
                pPos.x += part.velocity.x * Time.unscaledDeltaTime;
                pPos.y += part.velocity.y * Time.unscaledDeltaTime;
                // loop particles
                if (animationState != AnimationState.FLIP2 && animationState != AnimationState.FRONT_SHOWN && animationState != AnimationState.TEXT_BOX) {
                    if (pPos.y > particleYRange) {
                        pPos.y -= particleYRange * 2;
                    }
                    if (pPos.y < -particleYRange) {
                        pPos.y += particleYRange * 2;
                    }
                }
                part.gameObject.GetComponent<RectTransform>().localPosition = pPos;
                if (animationState == AnimationState.START) {
                    float a = Utilities.easeLinearClamp(animationTime, 0, 1, particleFadeDuration);
                    part.gameObject.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, a);
                }
            }


        }
        

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        setPosition();
	}

    void setPosition() {
        if (!timeUser.exists) return;
        float angle = time / circlePeriod * Mathf.PI*2;
        Vector3 diff = new Vector2(Mathf.Cos(angle) * 0, Mathf.Sin(angle));
        diff *= circleRadius;
        transform.localPosition = startPos + diff;
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["t"] = time;
    }

    void OnRevert(FrameInfo fi) {
        State nextState = (State)fi.state;
        if (state == State.COLLECTED && nextState != State.COLLECTED) {
            spriteRenderer.material = defaultMaterial;
        }
        if (nextState == State.NOT_FOUND && Vars.creatureCardFound(creatureID)) {
            state = State.FOUND;
        } else {
            state = nextState;
        }

        time = fi.floats["t"];
        setPosition();
    }

    void OnTriggerEnter2D(Collider2D c2d) {
        if (timeUser.shouldNotUpdate)
            return;
        if (c2d.gameObject != Player.instance.gameObject)
            return;
        if (Vars.currentNodeData == null || Vars.currentNodeData.creatureCardCollected(creatureID))
            return;

        obtain();
        
    }

    void obtain() {
        if (obtained)
            return;

        state = State.COLLECTED; // also "finds" and "collects" card
        
        // begin collect card animation
        HUD.instance.createPauseScreenLight();
        PauseScreen.instance.pauseGameCreatureCard();
        animationState = AnimationState.START;
        HUD.instance.speedLines.flash(Color.clear, flash1Color, flash1ColorDuration);
        animationTime = 0;
        // make particles
        for (int i=0; i<numRows; i++) {
            //1,0 2,-.5 3,-1 4,-1.5 5,-2
            float x = (i + (numRows - 1) / -2f) * particlesRowSpacing;
            for (int j=0; j<particlesPerRow; j++) {
                float y = -particleYRange + particleYRange*2 * j / particlesPerRow;

                Particle part = new Particle();
                part.gameObject = GameObject.Instantiate(particleGameObject);
                part.gameObject.transform.SetParent(canvas.transform, false);
                part.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(x, y);
                if (i % 2 == 0) {
                    part.velocity.Set(0, particleFallSpeed);
                } else {
                    part.velocity.Set(0, -particleFallSpeed);
                }
                part.gameObject.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 0);
                particles.Add(part);
            }
        }
        
        // create creature card animation
        creatureCard = GameObject.Instantiate(hudCardGameObject).GetComponent<CreatureCard>();
        creatureCard.transform.SetParent(canvas.transform, false);
        creatureCard.GetComponent<RectTransform>().localPosition = new Vector3(cardStartPos.x, cardStartPos.y);
        creatureCard.showBack();

        SoundManager.instance.playSFX(firstSound);
    }

    void OnDestroy() {
        foreach (Particle part in particles) {
            GameObject.Destroy(part.gameObject);
        }
        particles.Clear();
        if (creatureCard != null) {
            GameObject.Destroy(creatureCard.gameObject);
            creatureCard = null;
        }
        if (textBox != null) {
            GameObject.Destroy(textBox.gameObject);
            textBox = null;
        }
    }

    Vector3 startPos = new Vector3();

    float time = 0;
    int creatureID = -1;
    State _state = State.NOT_FOUND;
    AnimationState _animationState = AnimationState.NO_ANIMATION;
    float animationTime = 0;
    List<Particle> particles = new List<Particle>();

    TimeUser timeUser;
    Animator animator;
    SpriteRenderer spriteRenderer;

    CreatureCard creatureCard = null; // will be created for animation
    CreatureCardPickupText textBox = null; // will be created for animation
    GameObject canvas; // reference to Canvas

}
