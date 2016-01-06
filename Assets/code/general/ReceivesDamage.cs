using UnityEngine;
using System.Collections;

public class ReceivesDamage : MonoBehaviour {

    public static float HIT_FLASH_DURATION = .11f;
    public static Color HIT_FLASH_COLOR = new Color(.89f, 0, 0);

    public static float MERCY_FLASH_PERIOD = .2f;
    public static Color MERCY_FLASH_COLOR = Color.red;

    public int health = 10;
    public bool autoHitFlash = true; //if set to true, spriteRenderer will briefly flash after being hit
    public bool autoActivateDefaultDeath = true; //if set to true, will call activate() from the DefaultDeath component (if available)
    
    /* Granting mercyInvincibility does not change any events and does not alter AttackInfo at any point.
     * Modification to health lost is only done in the calculation, and is not reflected in PreDamage()
     * nor OnDamage(). */
    public void mercyInvincibility(float duration, int damageDecrease=int.MaxValue) {
        if (duration > mercyInvincibilityDuration - mercyInvincibilityTime) {
            _mercyInvincibilityTime = 0;
            mercyInvincibilityDuration = duration;
        }
    }
    public bool isMercyInvincible {
        get { return mercyInvincibilityTime < mercyInvincibilityDuration; }
    }
    public float mercyInvincibilityTime { get { return _mercyInvincibilityTime; } }


    ///////////////////
    // SEND MESSAGES //
    ///////////////////

    /* Will be called when dealDamage is called, just before damage is subtracted from health.
     * Use PreDamage to change some info of the attack. */
    // void PreDamage(AttackInfo ai);

    /* Will be called when dealDamage is called, just after damage is subtracted from health.
     * Use OnDamage to affect the GameObject as a result of taking damage. */
    // void OnDamage(AttackInfo ai);

    //////////////////////
    // PUBLIC FUNCTIONS //
    //////////////////////

    public void dealDamage(int damage) {
        dealDamage(damage, true);
    }
    public void dealDamage(int damage, bool toRight) {
        AttackInfo ai = new AttackInfo();
        ai.damage = damage;
        if (toRight) {
            ai.impactHeading = 0;
        } else {
            ai.impactHeading = -180;
        }
        ai.impactMagnitude = 1;
        dealDamage(ai);
    }
    /* Calling dealDamage(attackInfo) will not change the attackInfo parameter */
    public AttackInfo dealDamage(AttackInfo attackInfo) {

        //calls PreDamage to possibly change attackInfo before it's applied
        AttackInfo ai = attackInfo.clone();
        SendMessage("PreDamage", ai, SendMessageOptions.DontRequireReceiver);

        //decrease health
        bool sendOnDamage = true;
        if (isMercyInvincible && !ai.ignoreMercyInvincibility && ai.damage > 0) {
            int dam = ai.damage;
            dam -= mercyInvincibilityDamageDecrease;
            if (dam > 0) {
                health -= dam;
            } else {
                sendOnDamage = false;
            }
        } else {
            health -= ai.damage;
            if (ai.damage < 0) {
                sendOnDamage = false;
            }
        }

        if (sendOnDamage) {
            
            if (health <= 0 && autoActivateDefaultDeath) {
                if (defaultDeath != null) {
                    if (!defaultDeath.activated) {
                        defaultDeath.activate(attackInfo.impactToRight());
                    }
                }
            }
            if (autoHitFlash && spriteRenderer != null &&
                (defaultDeath == null || !defaultDeath.activated)) {
                hitFlashTime = 0;
                spriteRenderer.color = HIT_FLASH_COLOR;
            }
            SendMessage("OnDamage", ai, SendMessageOptions.DontRequireReceiver);
        }
        return ai;

    }


    float _mercyInvincibilityTime = 0;
    float mercyInvincibilityDuration = 0;
    int mercyInvincibilityDamageDecrease = int.MaxValue;

    void Awake() {
        Transform sot = this.transform.Find("spriteObject");
        GameObject spriteObject;
        if (sot == null) {
            spriteObject = gameObject;
            spriteRenderer = this.GetComponent<SpriteRenderer>();
        } else {
            spriteObject = sot.gameObject;
            spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
        }
        defaultDeath = GetComponent<DefaultDeath>();
    }
    void Update() {
        _mercyInvincibilityTime += Time.deltaTime;

        if (autoHitFlash && hitFlashTime < HIT_FLASH_DURATION) {
            hitFlashTime += Time.deltaTime;
            if (hitFlashTime >= HIT_FLASH_DURATION && spriteRenderer != null ) {
                spriteRenderer.color = Color.white;
            }
        }
    }

    // TimeUser stuff
    void OnSaveFrame(FrameInfo fi) {
        fi.ints["health"] = health;
        fi.floats["miTime"] = mercyInvincibilityTime;
        fi.floats["miDuration"] = mercyInvincibilityDuration;
        fi.ints["miDD"] = mercyInvincibilityDamageDecrease;
    }
    void OnRevert(FrameInfo fi) {
        health = fi.ints["health"];
        _mercyInvincibilityTime = fi.floats["miTime"];
        mercyInvincibilityDuration = fi.floats["miDuration"];
        mercyInvincibilityDamageDecrease = fi.ints["miDD"];
    }

    private SpriteRenderer spriteRenderer;
    private DefaultDeath defaultDeath;
    private float hitFlashTime = 9999;

}
