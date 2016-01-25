using UnityEngine;
using System.Collections;

public class Pouch : MonoBehaviour {

    public PickupSpawner.BurstSize burstSize = PickupSpawner.BurstSize.SMALL;
    public Vector2 burstPosition = new Vector2();
    public AudioClip burstSound;

    public void burst() {
        if (bursted) return;

        Vector2 pos = Utilities.rotateAroundPoint(burstPosition, Vector2.zero, rb2d.rotation * Mathf.PI / 180);
        pickupSpawner.burstSpawn(rb2d.position + pos, burstSize);
        animator.Play("open");
        gameObject.layer = LayerMask.NameToLayer("HitNothing");
        SoundManager.instance.playSFXRandPitchBend(burstSound);
        _bursted = true;
    }
    public bool bursted {  get { return _bursted; } }

    void Awake() {
        animator = GetComponent<Animator>();
        receivesDamage = GetComponent<ReceivesDamage>();
        rb2d = GetComponent<Rigidbody2D>();
        pickupSpawner = GetComponent<PickupSpawner>();
        timeUser = GetComponent<TimeUser>();
	}
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;
        
        time += Time.deltaTime;

	}

    void OnDamage(AttackInfo ai) {
        if (receivesDamage.health <= 0) {
            burst();
        }
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
        fi.bools["bur"] = bursted;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        bool prevBursted = bursted;
        _bursted = fi.bools["bur"];
        if (prevBursted && !bursted) {
            gameObject.layer = LayerMask.NameToLayer("Enemies");
        }
    }
    
    Animator animator;
    ReceivesDamage receivesDamage;
    Rigidbody2D rb2d;
    PickupSpawner pickupSpawner;
    TimeUser timeUser;

    bool _bursted = false;
    float time = 0;

}
