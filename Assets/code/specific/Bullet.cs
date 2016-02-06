using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

    /* The layer determines what the Bullet will hit.
     * PlayerAttacks: will only hit enemies
     * EnemyAttacks: will only hit players
     * MiscAttacks: will only hit players and enemies
     * */

    // PROPERTIES
    public float speed = 100;
    public int damage = 2;
    public float heading = 0;
    public float radius = 0; // set to 0 for raycast, positive number for circlecast
    public float maxDistance = -1; //set to negative number to have bullet travel "forever"
    public bool breaksChargeShotBarriers = false;
    public bool shotByOracle = false;
    public GameObject bulletFadeGameObject;
    public GameObject bulletExplosionGameObject;
    public AudioClip hitSound; // can be null for no sound

    public float distTravelled { get { return _distTravelled; } }

    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        timeUser = GetComponent<TimeUser>();
        visionUser = GetComponent<VisionUser>();
        Debug.Assert(timeUser != null && visionUser != null);
    }

	void Start() {
	    
	}
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        // collision
        float distance = speed * Time.deltaTime;
        bool destroyBullet = false;
        Vector2 destroyPoint = new Vector2();
        // check if exceeds max distance
        if (maxDistance > 0) {
            if (distTravelled + distance > maxDistance) {
                distance = maxDistance - distTravelled;
                destroyBullet = true;
            }
        }
        _distTravelled += distance;
        // perform raycast
        Vector2 direction = new Vector2(Mathf.Cos(heading * Mathf.PI / 180), Mathf.Sin(heading * Mathf.PI / 180));
        int layerMask = ColFinder.getLayerCollisionMask(gameObject.layer);
        RaycastHit2D rh2d;
        if (radius <= .0001f) {
            rh2d = Physics2D.Raycast(
                rb2d.position,
                direction,
                distance,
                layerMask);
        } else {
            rh2d = Physics2D.CircleCast(
                rb2d.position,
                radius,
                direction,
                distance,
                layerMask);
        }
        if (rh2d.collider == null) {
            //hit nothing
            rb2d.position = rb2d.position + direction * distance;
            aliveTime += Time.deltaTime;
            if (destroyBullet) {
                destroyPoint = rb2d.position;
            }
        } else {
            //hit thing
            
            ReceivesDamage rd = rh2d.collider.gameObject.GetComponent<ReceivesDamage>();
            if (rd != null && rd.health > 0) {
                AttackInfo ai = new AttackInfo();
                ai.damage = damage;
                if (shotByOracle) {
                    EnemyInfo ei = rd.gameObject.GetComponent<EnemyInfo>();
                    if (ei != null) {
                        // check to see if damage is boosted by creature card
                        if (Vars.creatureCardFound(ei.creatureID)) {
                            if (breaksChargeShotBarriers) {
                                ai.damage += CreatureCard.CHARGE_SHOT_DAMAGE_INCREASE;
                            } else {
                                ai.damage += CreatureCard.STANDARD_DAMAGE_INCREASE;
                            }
                        }
                    }
                }
                ai.impactHeading = heading;
                ai.impactPoint = rh2d.point;
                ai.breaksChargeShotBarriers = breaksChargeShotBarriers;
                ai = rd.dealDamage(ai);
                if (ai.damage > 0) {
                    if (hitSound != null)
                        SoundManager.instance.playSFXRandPitchBend(hitSound);
                } else {
                    Debug.Log("Bullet dealt 0 damage");
                }
                
            }

            rb2d.position = rh2d.point;
            destroyBullet = true;
            destroyPoint = rh2d.point;
        }

        // leaving map
        bool leftMap = false;
        Rect rect = CameraControl.getMapBounds();
        rect.xMin -= 10;
        rect.xMax += 10;
        rect.yMin -= 10;
        rect.yMax += 10;
        if (!rect.Contains(rb2d.position)) {
            leftMap = true;
            destroyBullet = true;
            destroyPoint = rb2d.position;
        }

        if (destroyBullet) {

            //create bulletFade at point of hit
            bool createBulletFade = aliveTime > 0 && !leftMap;
            if (bulletFadeGameObject != null && createBulletFade) {
                GameObject bF = GameObject.Instantiate(bulletFadeGameObject,
                    destroyPoint,
                    gameObject.transform.rotation) as GameObject;
                if (visionUser.isVision) {
                    bF.GetComponent<VisionUser>().becomeVisionNow(visionUser.duration - visionUser.time, visionUser);
                }
            }

            if (!visionUser.isVision && !leftMap) {
                //create bulletExplosion at point of hit
                if (bulletExplosionGameObject != null) {
                    GameObject bE = GameObject.Instantiate(bulletExplosionGameObject,
                        destroyPoint,
                        gameObject.transform.rotation) as GameObject;
                    if (visionUser.isVision) {
                        bE.GetComponent<VisionUser>().becomeVisionNow(visionUser.duration - visionUser.time, visionUser);
                    }
                }
            }

            timeUser.timeDestroy();

        }

	}

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["aliveTime"] = aliveTime;
        fi.floats["distTravelled"] = distTravelled;
    }
    void OnRevert(FrameInfo fi) {
        aliveTime = fi.floats["aliveTime"];
        _distTravelled = fi.floats["distTravelled"];
    }

    private Rigidbody2D rb2d;
    private TimeUser timeUser;
    private VisionUser visionUser;
    private float aliveTime = 0;
    private float _distTravelled = 0;

}
