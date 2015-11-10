using UnityEngine;
using System.Collections;

public class PhaseMeter : MonoBehaviour {

    public Vector2 position = new Vector2(0, -42);
    public float increaseDuration = .4f;
    public float warningThreshold = .2f;
    public float pulsePeriod = .4f;
    public float lowSoundStart = 25;
    public Sprite fullMeterSprite;
    public Sprite fullBarSprite;
    public Sprite emptyMeterSprite;
    public Sprite emptyBarSprite;
    public AudioClip phaseLowSound;
    public AudioClip phaseEmptySound;
    public Color increaseColor = new Color(1, 1, 0);
    public Color pulseColor = new Color(0, 0, 1);

    /* Called by HUD after being instantiated. */
    public void setUp() {
        rt.anchorMin = new Vector2(.5f, 1);
        rt.anchorMax = new Vector2(.5f, 1);
        rt.anchoredPosition = position;
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
        pulseTime = 99999;
        if (phase == 0) {
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
        image = GetComponent<UnityEngine.UI.Image>();
        rt = GetComponent<RectTransform>();
        phaseBar = transform.Find("PhaseBar").gameObject;
        pbImage = phaseBar.GetComponent<UnityEngine.UI.Image>();
        pbRT = phaseBar.GetComponent<RectTransform>();
	}
	
	void Update() {

        if (increasing) { //increasing

            increaseTime += Time.deltaTime;

            //change color
            float t = Mathf.Min(1, increaseTime / increaseDuration);
            if (t < .5f) {
                image.color = Color.Lerp(Color.white, increaseColor, t * 2);
            } else {
                image.color = Color.Lerp(increaseColor, Color.white, (t - .5f) * 2);
            }
            pbImage.color = image.color;

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
                image.color = Color.Lerp(Color.white, pulseColor, t * 2);
            } else {
                image.color = Color.Lerp(pulseColor, Color.white, (t - .5f) * 2);
            }
            pbImage.color = image.color;

            displayPhase = phase;

        } else {
            displayPhase = phase;
            if (image.color != Color.white){
                image.color = Color.white;
                pbImage.color = image.color;
            }
            
        }

        pbRT.transform.localScale = new Vector3(displayPhase / maxPhase, pbRT.transform.localScale.y);

        
        if (displayPhase == 0) {
            // switch to empty sprite when phase is at 0
            if (image.sprite != emptyMeterSprite) {
                image.sprite = emptyMeterSprite;
                pbImage.sprite = emptyBarSprite;
            }
        } else if (!increasing && !pulsing && displayPhase / maxPhase < warningThreshold) {
            // blink warning when phase is getting low
            warningTime += Time.deltaTime;
            if (warningTime > .2f) {
                if (image.sprite == fullMeterSprite) {
                    image.sprite = emptyMeterSprite;
                    pbImage.sprite = emptyBarSprite;
                } else {
                    image.sprite = fullMeterSprite;
                    pbImage.sprite = fullBarSprite;
                }
                warningTime -= .2f;
            }
        } else {
            warningTime = 0;
            if (image.sprite != fullMeterSprite) {
                image.sprite = fullMeterSprite;
                pbImage.sprite = fullBarSprite;
            }
        }

	}

    private float _maxPhase = 1;
    private float _phase = 0;
    private float warningTime = 0;
	
	// components
    UnityEngine.UI.Image image;
    RectTransform rt;
    GameObject phaseBar;
    UnityEngine.UI.Image pbImage;
    RectTransform pbRT;
}
