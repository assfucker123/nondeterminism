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
    public float heading = 0;
    public float maxDistance = -1; //set to negative number to have bullet travel "forever"
    public GameObject bulletFadeGameObject;
    public GameObject bulletExplosionGameObject;

    public float distTravelled { get { return _distTravelled; } }

    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        timeUser = GetComponent<TimeUser>();
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
        RaycastHit2D rh2d = Physics2D.Raycast(
            rb2d.position,
            direction,
            distance,
            layerMask);
        if (rh2d.collider == null) {
            //hit nothing
            rb2d.position = rb2d.position + direction * distance;
            aliveTime += Time.deltaTime;
            if (destroyBullet) {
                destroyPoint = rb2d.position;
            }
        } else {
            //hit thing
            
            //todo: affect object hit

            rb2d.position = rh2d.point;
            destroyBullet = true;
            destroyPoint = rh2d.point;
        }

        // leaving map
        Rect rect = CameraConfig.getMapBounds();
        if (!rect.Contains(rb2d.position)) {
            destroyBullet = true;
            destroyPoint = rb2d.position;
        }

        if (destroyBullet) {

            //create bulletFade at point of hit
            bool createBulletFade = aliveTime > 0;
            if (bulletFadeGameObject != null && createBulletFade) {
                GameObject.Instantiate(bulletFadeGameObject,
                    destroyPoint,
                    gameObject.transform.rotation);
            }
            //create bulletExplosion at point of hit
            if (bulletExplosionGameObject != null) {
                GameObject.Instantiate(bulletExplosionGameObject,
                    destroyPoint,
                    gameObject.transform.rotation);
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
    private float aliveTime = 0;
    private float _distTravelled = 0;

}
