using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhaseMeter : MonoBehaviour {

    public Vector2 position = new Vector2(0, -42);
    public float increaseDuration = .4f;
    public float warningThreshold = .2f;
    public float pulsePeriod = .4f;
    public float lowSoundStart = 25;

    public Vector2 raisedPositionOffset = new Vector2(0, -40);

    public float fullStandardPhase = 80;
    public float middleBarPhase = 20;
    public float extendStandardPhase = 28;
    public float bgExtendScaleMultiplier = 1.12f;
    public Sprite[] digitSprites;
    public Sprite messageSpriteVisionsOK;
    public Sprite messageSpriteNoVisions;
    public Sprite messageSpriteEmpty;
    public AudioClip phaseLowSound;
    public AudioClip phaseEmptySound;
    public Color increaseColor = new Color(1, 1, 0);
    public Color pulseColor = new Color(0, 0, 1);
    public Color visionsOKColor = new Color(1, 1, 1, 1);
    public Color noVisionsColor = new Color(1, 1, 1, 1);
    public Color emptyColor = new Color(1, 1, 1, 1);

    /* Called by HUD after being instantiated. */
    public void setUp() {
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(.5f, 1);
        rt.anchorMax = new Vector2(.5f, 1);
        rt.anchoredPosition = position;
    }

    /// <summary>
    /// Called by CutsceneBars.  Puts phaseMeter in position based on position of the cutscene bars.
    /// </summary>
    /// <param name="inter">in [0, 1]</param>
    public void setRaisedPosition(float inter) {
        GetComponent<RectTransform>().anchoredPosition = Utilities.easeLinearClamp(inter, position, raisedPositionOffset, 1);
    }

    public float maxPhase { get { return _maxPhase; } }
    public float phase { get { return _phase; } }
    public bool increasing { get { return increaseTime < increaseDuration; } }
    public bool pulsing { get { return pulseTime < pulsePeriod; } }

    private float increaseTime = 99999;
    private float increaseInitialPhase = 0;
    private float displayPhase = 0; //directly correlates to the scale of the meter
    private float pulseTime = 99999;

    public void setMaxPhase(float maxPhase) {
        if (_maxPhase == maxPhase) return;
        _maxPhase = maxPhase;

        // set scales of bgExtends
        float scale = (maxPhase - (fullStandardPhase - extendStandardPhase)) / extendStandardPhase;
        scale = 1 + (scale - 1) * bgExtendScaleMultiplier;
        scale = Mathf.Max(0, scale);
        bgExtendLeft.rectTransform.localScale = new Vector3(scale, 1, 1);
        bgExtendRight.rectTransform.localScale = new Vector3(scale, 1, 1);
    }
    public float setPhase(float phase) {
        if (_phase == phase) return phase;
        _phase = Mathf.Max(0, Mathf.Min(maxPhase, phase));
        return _phase;
    }

    public void beginPulse() {
        pulseTime = 0;
        // low sound
        if (displayPhase < lowSoundStart) {
            float volume = Utilities.easeLinearClamp(displayPhase, 1, -1, lowSoundStart);
            SoundManager.instance.playSFX(phaseLowSound, volume);
        }
    }
    public void endPulse() {
        if (!pulsing)
            return;
        pulseTime = 99999;
        if (phase == 0) {
            Utilities.debugLogCallStack();
            SoundManager.instance.playSFX(phaseEmptySound);
        }
    }
    public void playPhaseEmptySound() {
        SoundManager.instance.playSFX(phaseEmptySound);
    }

    /* Increases phase by given amount, and goes through increase animation. */
    public float increasePhase(float phaseIncrease) {
        if (phaseIncrease <= 0) return phase;
        
        increaseInitialPhase = displayPhase;
        _phase = Mathf.Min(maxPhase, phase + phaseIncrease);
        increaseTime = 0;

        return phase;
    }

    

	void Awake() {

        bgExtendLeft = transform.Find("BGExtendLeft").GetComponent<Image>();
        bgExtendRight = transform.Find("BGExtendRight").GetComponent<Image>();
        barRight = transform.Find("BarRight").GetComponent<Image>();
        barLeft = transform.Find("BarLeft").GetComponent<Image>();
        barMiddle = transform.Find("BarMiddle").GetComponent<Image>();
        message = transform.Find("Message").GetComponent<Image>();
        percent = transform.Find("Percent").GetComponent<Image>();
        digit1 = transform.Find("Digit1").GetComponent<Image>();
        digit10 = transform.Find("Digit10").GetComponent<Image>();
        digit100 = transform.Find("Digit100").GetComponent<Image>();
        
	}
	
	void Update() {

        if (increasing) { //increasing

            increaseTime += Time.deltaTime;

            //change color
            float t = Mathf.Min(1, increaseTime / increaseDuration);
            if (t < .5f) {
                barLeft.color = Color.Lerp(Color.white, increaseColor, t * 2);
            } else {
                barLeft.color = Color.Lerp(increaseColor, Color.white, (t - .5f) * 2);
            }
            
            displayPhase = Mathf.Min(maxPhase, Utilities.easeOutQuadClamp(
                increaseTime,
                increaseInitialPhase,
                phase - increaseInitialPhase,
                increaseDuration));

        } else if (pulsing) { //pulsing

            pulseTime += Time.deltaTime;
            if (pulseTime >= pulsePeriod){
                pulseTime -= pulsePeriod;
                // low sound
                if (displayPhase < lowSoundStart) {
                    float volume = Utilities.easeLinearClamp(displayPhase, 1, -1, lowSoundStart);
                    if (TimeUser.reverting) {
                        SoundManager.instance.playSFX(phaseLowSound, volume);
                    }
                }
            }

            //change color
            float t = Mathf.Min(1, pulseTime / pulsePeriod);
            if (t < .5f) {
                barLeft.color = Color.Lerp(Color.white, pulseColor, t * 2);
            } else {
                barLeft.color = Color.Lerp(pulseColor, Color.white, (t - .5f) * 2);
            }

            displayPhase = phase;

        } else {
            displayPhase = phase;
            if (barLeft.color != Color.white){
                barLeft.color = Color.white;
            }
            
        }
        barRight.color = barLeft.color;
        barMiddle.color = barLeft.color;

        // set scales
        if (displayPhase <= middleBarPhase) {
            barMiddle.rectTransform.localScale = new Vector3(displayPhase / middleBarPhase, 1, 1);
            barLeft.rectTransform.localScale = new Vector3(0, 1, 1);
            barRight.rectTransform.localScale = new Vector3(0, 1, 1);
        } else {
            barMiddle.rectTransform.localScale = new Vector3(1, 1, 1);
            barLeft.rectTransform.localScale = new Vector3((displayPhase - middleBarPhase) / (fullStandardPhase - middleBarPhase), 1, 1);
            barRight.rectTransform.localScale = barLeft.rectTransform.localScale;
        }

        // set digits
        int percent = Mathf.CeilToInt(100 * displayPhase / fullStandardPhase - .01f);
        int hundreds = percent / 100;
        percent -= hundreds * 100;
        if (hundreds > 10) hundreds = 9;
        int tens = percent / 10;
        percent -= tens * 10;
        int ones = percent;
        digit100.sprite = digitSprites[hundreds];
        digit100.enabled = (hundreds > 0);
        digit10.sprite = digitSprites[tens];
        digit1.sprite = digitSprites[ones];

        // set message and color
        if (displayPhase <= .1f) {
            message.sprite = messageSpriteEmpty;
            message.color = emptyColor;
        } else if (VisionUser.abilityActive) {
            message.sprite = messageSpriteVisionsOK;
            message.color = visionsOKColor;
        } else {
            message.sprite = messageSpriteNoVisions;
            message.color = noVisionsColor;
        }
        this.percent.color = message.color;
        digit1.color = message.color;
        digit10.color = message.color;
        digit100.color = message.color;

	}

    private float _maxPhase = 1;
    private float _phase = 0;

    // components
    Image bgExtendLeft;
    Image bgExtendRight;
    Image barRight;
    Image barLeft;
    Image barMiddle;
    Image message;
    Image percent;
    Image digit1;
    Image digit10;
    Image digit100;
}
