using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour {

    public Vector2 optionsPageOffset = new Vector2(0, -10);
    public AudioClip switchSound;
    public GameObject optionsPageGameObject;
    public TextAsset decryptorInfoTextAsset;
    public GameObject thingGameObject;
    public Color[] thingColors;
    public Sprite[] thingSprites;
    [Header("Things")]
    public float thingLoopSpan = 1000; // how far things will travel before looping
    public float thingYSpread = 600;
    public float thingMinSpeed = 100;
    public float thingMaxSpeed = 200;
    public float thingMinAmplitude = 100;
    public float thingMaxAmplitude = 200;
    public float thingMinPeriod = .3f;
    public float thingMaxPeriod = .5f;
    public int numThings = 20;


    class ThingThing {
        public GameObject gameObject;
        public float speed = 100;
        public float xOffset = 0;
        public float initialY = 0;
        public float loopSpan = 1000;
        public float amplitude = 100;
        public float period = .3f;
        public float periodOffset = 0;
        public float time = 0;
        public void updatePosition() {
            float x = speed * time + xOffset;
            int loops = Mathf.FloorToInt(x / loopSpan);
            x -= loops * loopSpan;
            float y = initialY + Mathf.Sin((time + periodOffset) * Mathf.PI * 2 / period) * amplitude;
            gameObject.GetComponent<RectTransform>().localPosition = new Vector3(x, y);
        }
    }
    List<ThingThing> things = new List<ThingThing>();
    void makeThing() {
        ThingThing tt = new ThingThing();
        tt.gameObject = GameObject.Instantiate(thingGameObject) as GameObject;
        Image image = tt.gameObject.GetComponent<Image>();
        image.sprite = thingSprites[Random.Range(0, thingSprites.Length)];
        image.color = thingColors[Random.Range(0, thingColors.Length)];
        tt.gameObject.transform.SetParent(thingsGO.transform);
        tt.speed = Random.Range(thingMinSpeed, thingMaxSpeed);
        tt.loopSpan = thingLoopSpan;
        tt.xOffset = Random.Range(0, tt.loopSpan);
        tt.initialY = Random.Range(-thingYSpread / 2, thingYSpread / 2);
        tt.amplitude = Random.Range(thingMinAmplitude, thingMaxAmplitude);
        tt.period = Random.Range(thingMinPeriod, thingMaxPeriod);
        tt.periodOffset = Random.Range(0, tt.period);
        tt.updatePosition();
        things.Add(tt);
    }

	void Awake() {
        clockText = transform.Find("ClockText").GetComponent<Text>();
        selection = transform.Find("Selection").GetComponent<Image>();
        playGameText = transform.Find("PlayGameText").GetComponent<Text>();
        optionsText = transform.Find("OptionsText").GetComponent<Text>();
        quitText = transform.Find("QuitText").GetComponent<Text>();
        controlsText = transform.Find("ControlsText").GetComponent<Text>();
        flashbackText = transform.Find("FlashbackText").GetComponent<Text>();
        thingsGO = transform.Find("Things").gameObject;

        GameObject optionsPageGO = GameObject.Instantiate(optionsPageGameObject);
        optionsPageGO.transform.SetParent(transform, false);
        optionsPageGO.GetComponent<RectTransform>().localPosition = optionsPageGO.GetComponent<RectTransform>().localPosition +
            new Vector3(optionsPageOffset.x, optionsPageOffset.y);
        optionsPage = optionsPageGO.GetComponent<OptionsPage>();
        optionsPage.GetComponent<RectTransform>().localScale = Vector3.one;

        if (!Decryptor.initialized) {
            Decryptor.initialize(new Properties(decryptorInfoTextAsset.text));
        }
	}

    void Start() {

        Vars.loadSettings();
        Vars.loadData(Vars.saveFileIndexLastUsed);

        optionsPage.hide();
        Time.timeScale = 1;

        for (int i = 0; i < numThings; i++) {
            makeThing();
        }

        // set options
        options.Add(playGameText);
        options.Add(optionsText);
        options.Add(quitText);
    }
	
	void Update() {

        menuUpdate();

        // timeUser stuff
        if (reverting) {
            revertingDiff += Time.unscaledDeltaTime * 2;
            if (!Keys.instance.flashbackHeld) {
                reverting = false;
            }
        } else {
            if (Keys.instance.flashbackHeld) {
                reverting = true;
            }
        }

        foreach (ThingThing tt in things) {
            if (reverting) {
                tt.time -= Time.unscaledDeltaTime;
            } else {
                tt.time += Time.unscaledDeltaTime;
            }
            tt.updatePosition();
        }

        updateClock(revertingDiff);

	}

    void menuUpdate() {

        if (optionsPageShown) {
            if (((optionsPage.onTopMenu() && Keys.instance.backPressed) ||
                (optionsPage.selectingBack() && Keys.instance.confirmPressed))) {
                optionsBack();
            }
            optionsPage.update();
            return;
        }

        int prevSelectionIndex = selectionIndex;
        bool newSelection = false;
        if (Keys.instance.upPressed) {
            selectionIndex--;
            if (selectionIndex < 0) selectionIndex = options.Count - 1;
            newSelection = true;
        } else if (Keys.instance.downPressed) {
            selectionIndex++;
            if (selectionIndex >= options.Count) selectionIndex = 0;
            newSelection = true;
        }
        if (newSelection) {
            options[prevSelectionIndex].color = PauseScreen.DEFAULT_COLOR;
            options[selectionIndex].color = PauseScreen.SELECTED_COLOR;
            selectionPos0.Set(selection.rectTransform.localPosition.x, selection.rectTransform.localPosition.y);
            selectionPos1.Set(options[selectionIndex].rectTransform.localPosition.x, options[selectionIndex].rectTransform.localPosition.y);
            selectionPos1 = selectionPos1 + selectionOffset;
            selectionTime = 0;
            SoundManager.instance.playSFX(switchSound);
        }

        float dur = .1f;
        if (selectionTime < dur) {
            selectionTime += Time.unscaledDeltaTime;
            float sx = Utilities.easeOutQuadClamp(selectionTime, selectionPos0.x, selectionPos1.x - selectionPos0.x, dur);
            float sy = Utilities.easeOutQuadClamp(selectionTime, selectionPos0.y, selectionPos1.y - selectionPos0.y, dur);
            selection.rectTransform.localPosition = new Vector3(sx, sy);
        }

        // confirming an option
        if (Keys.instance.confirmPressed) {
            Text selection = options[selectionIndex];

            if (selection == playGameText) {
                playGameSelected();
            } else if (selection == optionsText) {
                optionsSelected();
            } else if (selection == quitText) {
                quitGameSelected();
            }

        }

    }

    void updateClock(float timeDiff) {
        System.DateTime clock = System.DateTime.Now;
        clock = clock.AddMilliseconds((double)(timeDiff * -1000));
        clockText.text = clock.ToString("HH:mm:ss");
    }

    /// <summary>
    /// This is called by TitleScreen to begin the game.
    /// </summary>
    void playGameSelected() {

        // when debugging, can set specific start point
        /*
        Vars.currentNodeData.level = "first_ambush";
        Vars.currentNodeData.position.x = 76;
        Vars.currentNodeData.position.y = 9;
        */

        string levelName = Vars.currentNodeData.level;
        Vars.loadLevel(levelName);
    }

    void optionsSelected() {
        if (optionsPageShown) return;
        optionsPage.show(true);
        hideOptions();
        optionsPageShown = true;
    }
    void optionsBack() {
        if (!optionsPageShown) return;
        optionsPage.hide();
        showOptions(options.IndexOf(optionsText));
        optionsPageShown = false;
    }

    void quitGameSelected() {
        Vars.quitGame();
    }

    void hideOptions() {
        selection.enabled = false;
        playGameText.enabled = false;
        optionsText.enabled = false;
        quitText.enabled = false;

        controlsText.enabled = false;
        flashbackText.enabled = false;
    }
    void showOptions(int selectionIndex) {
        this.selectionIndex = selectionIndex;
        selection.rectTransform.localPosition = options[this.selectionIndex].rectTransform.localPosition +
            new Vector3(selectionOffset.x, selectionOffset.y, 0);
        selectionTime = 9999;

        selection.enabled = true;
        playGameText.enabled = true;
        optionsText.enabled = true;
        quitText.enabled = true;

        controlsText.enabled = true;
        flashbackText.enabled = true;
    }

    int selectionIndex = 0;
    float selectionTime = 9999f;
    Vector2 selectionPos0 = new Vector2();
    Vector2 selectionPos1 = new Vector2();
    List<Text> options = new List<Text>();
    bool optionsPageShown = false;

    Text clockText;
    Image selection;
    Vector2 selectionOffset = new Vector2(0, 2);
    Text playGameText;
    Text optionsText;
    Text quitText;
    OptionsPage optionsPage;
    GameObject thingsGO;

    bool reverting = false;
    float revertingDiff = 0;

    // get rid of?
    Text controlsText;
    Text flashbackText;

}
