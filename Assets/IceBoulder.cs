using UnityEngine;
using System.Collections;

public class IceBoulder : MonoBehaviour {

    public GameObject[] shardGameObjects;
    public PickupSpawner.BurstSize pickupsBurstSize = PickupSpawner.BurstSize.MEDIUM;
    public float pickupsProbability = .5f;
    public AudioClip iceShatterSound;

	void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        receivesDamage = GetComponent<ReceivesDamage>();
        rb2d = GetComponent<Rigidbody2D>();
        timeUser = GetComponent<TimeUser>();
        visionUser = GetComponent<VisionUser>();
        pickupSpawner = GetComponent<PickupSpawner>();
	}
	
    void Start() {

    }

	void Update() {
		
	}

    /* Detect being destroyed, spawning shards */
    void OnDamage(AttackInfo ai) {
        if (receivesDamage.health <= 0) {
            // be destroyed
            SoundManager.instance.playSFXRandPitchBend(iceShatterSound);
            if (timeUser.randomValue() < pickupsProbability) {
                pickupSpawner.burstSpawn(rb2d.position, pickupsBurstSize);
            }
            spawnShards();
            timeUser.timeDestroy();
        }
    }

    void spawnShards() {
        
        foreach (GameObject shardGameObject in shardGameObjects) {
            IceBoulderShard ibs = shardGameObject.GetComponent<IceBoulderShard>();
            Vector2 relPos = Utilities.rotateAroundPoint(ibs.startPos, Vector2.zero, rb2d.rotation * Mathf.PI / 180);

            GameObject.Instantiate(shardGameObject, transform.localPosition + new Vector3(relPos.x, relPos.y), Utilities.setQuat(rb2d.rotation));
        }
    }

    SpriteRenderer spriteRenderer;
    ReceivesDamage receivesDamage;
    Rigidbody2D rb2d;
    TimeUser timeUser;
    VisionUser visionUser;
    PickupSpawner pickupSpawner;
}
