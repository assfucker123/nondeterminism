using UnityEngine;
using System.Collections;

/* Attach to objects that can only be destroyed by a certain weapon.
 * If object is immune to weapon, AttackInfo ai is set to 0 in PreDamage, and an effect happens. */
public class DamageBarrier : MonoBehaviour {

    public enum Type {
        NONE,
        CHARGE_SHOT,
        GRENADE,
        MISSILE,
        FLARE_DIVE
    }

    public Type type = Type.NONE;
    public bool autoDestroyAnimation = true;
    public GameObject needChargeShotGameObject;
    public float destroyDuration = .6f;
    public float destroyFlashPeriod = .04f;
    public AudioClip destroySound;

    void Awake() {
        timeUser = GetComponent<TimeUser>();
        receivesDamage = GetComponent<ReceivesDamage>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void PreDamage(AttackInfo ai) {

        switch (type) {
        case Type.CHARGE_SHOT:
            if (!ai.breaksChargeShotBarriers) {
                ai.damage = 0;
                spawnParticles(needChargeShotGameObject, ai.impactPoint);
            }
            break;
        case Type.GRENADE:
            if (!ai.breaksGrenadeBarriers) {
                ai.damage = 0;
                Debug.Log("Spawn 'need grenade' effect here");
            }
            break;
        case Type.MISSILE:
            if (!ai.breaksGrenadeBarriers) {
                ai.damage = 0;
                Debug.Log("Spawn 'need missile' effect here");
            }
            break;
        case Type.FLARE_DIVE:
            if (!ai.breaksGrenadeBarriers) {
                ai.damage = 0;
                Debug.Log("Spawn 'need flare dive' effect here");
            }
            break;
        }
    }

    void spawnParticles(GameObject particleGameObject, Vector2 position) {
        int numParticles = particleGameObject.GetComponent<NeedWeaponParticle>().numParticles;
        float offset = 0;
        if (GetComponent<TimeUser>() != null) {
            offset = GetComponent<TimeUser>().randomValue();
        } else {
            offset = Random.value;
        }
        offset *= 360;

        for (int i=0; i<numParticles; i++) {
            float heading = offset + (360f * i / numParticles);
            NeedWeaponParticle nwp = (GameObject.Instantiate(particleGameObject, position, Quaternion.identity) as GameObject).GetComponent<NeedWeaponParticle>();
            nwp.heading = heading;
            nwp.playSound = (i == 0);
        }
    }

    void OnDamage(AttackInfo ai) {
        if (timeUser != null && timeUser.shouldNotUpdate)
            return;
        if (receivesDamage.health <= 0) {
            if (autoDestroyAnimation) {
                destroy();
            }
        }
    }

    void Update() {

        if (timeUser != null && timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;
        setColor();

        if (destroying && time >= destroyDuration) {
            if (timeUser == null) {
                GameObject.Destroy(gameObject);
            } else {
                timeUser.timeDestroy();
            }
        }

    }

    void setColor() {

        Color color = spriteRenderer.color;

        if (destroying && time < destroyDuration) {
            bool visible = (Utilities.fmod(time, destroyFlashPeriod * 2) < destroyFlashPeriod);
            float alpha = 1;
            if (visible) {
                alpha = Utilities.easeLinearClamp(time, 1, -1, destroyDuration);
            } else {
                alpha = 0;
            }
            color.a = alpha;

        } else {
            color.a = 1;
        }

        spriteRenderer.color = color;

    }

    void destroy() {
        if (destroying) return;

        destroying = true;
        time = 0;
        gameObject.layer = LayerMask.NameToLayer("HitNothing");
        if (destroySound != null) {
            SoundManager.instance.playSFX(destroySound);
        }
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.ints["lay"] = gameObject.layer;
        fi.bools["d"] = destroying;
        fi.floats["t"] = time;
    }

    void OnRevert(FrameInfo fi) {
        gameObject.layer = fi.ints["lay"];
        destroying = fi.bools["d"];
        time = fi.floats["t"];
        setColor();
    }

    TimeUser timeUser;
    ReceivesDamage receivesDamage;
    SpriteRenderer spriteRenderer;

    bool destroying = false;
    float time = 0;


}
