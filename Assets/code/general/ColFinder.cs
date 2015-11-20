using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* ColFinder uses the actual collision events to determine if something
 * collided with this GameObject, then updates hitBottom, hitLeft, hitRight,
 * and hitTop accordingly.
 * 
 * Only works if objects are moving towards each other (includes influence by gravity)
 * Does not work if objects are stationary next to each other.
 * 
 * in Start(), all connected Rigidbody2D's sleep modes are set to never sleep
 * */

public class ColFinder : MonoBehaviour {

	/* If hitting surface whose normal angle is larger than this,
	 * than it will count as hitting the bottom.  Otherwise it
	 * counts as hitting left or right.  Value is in [0, 90] */
	public float bottomAngleThreshold = 35f;

	/* How far down to check when attempting to reposition object
	 * down on a bottom surface */
	public float raycastDownDistance = 1.0f;

	/* Helps adjust velocity when repositioning with raycast down.
	 * Should be the same as the actual gravity */
	public float raycastDownGravity = 70f;

	//getters
    public bool hit {
        get { return hitBottom || hitTop || hitLeft || hitRight; }
    }
	public bool hitBottom {
		get { return _hitBottom; }
	}
	public float normalBottom { //will be around PI/2 if hit
		get { return _normalBottom; }
	}
	public GameObject[] gameObjectsHitBottom {
		get { return _gosHitBottom.ToArray(); }
	}
	public bool hitLeft {
		get { return _hitLeft; }
	}
	public float normalLeft { //will be around 0 if hit
		get { return _normalLeft; }
	}
	public GameObject[] gameObjectsHitLeft {
		get { return _gosHitLeft.ToArray(); }
	}
	public bool hitTop {
		get { return _hitTop; }
	}
	public float normalTop { //will be around -PI/2 if hit
		get { return _normalTop; }
	}
	public GameObject[] gameObjectsHitTop {
		get { return _gosHitTop.ToArray(); }
	}
	public bool hitRight {
		get { return _hitRight; }
	}
	public float normalRight { //will be around PI if hit
		get { return _normalRight; }
	}
	public GameObject[] gameObjectsHitRight {
		get { return _gosHitRight.ToArray(); }
	}

    public Rect boundsOfColliders(Collider2D[] colliders) {
        if (colliders.Length == 0)
            return new Rect(0, 0, 0, 0);
        Rect ret = new Rect(colliders[0].bounds.min.x, colliders[0].bounds.min.y,
            colliders[0].bounds.extents.x * 2, colliders[0].bounds.extents.y * 2);
        foreach (Collider2D coldr in colliders) {
            ret.xMin = Mathf.Min(ret.xMin, coldr.bounds.min.x);
            ret.yMin = Mathf.Min(ret.yMin, coldr.bounds.min.y);
            ret.xMax = Mathf.Max(ret.xMax, coldr.bounds.max.x);
            ret.yMax = Mathf.Max(ret.yMax, coldr.bounds.max.y);
        }
        return ret;
    }

    /* Gets an int that acts as a filter to detect Colliders only on the layers objects
     * on layerName would collide with.  */
    public static int getLayerCollisionMask(int layer) {
        int allLayers = Physics2D.AllLayers;
        int mask = 0;
        for (int otherLayer = 0; otherLayer < 32; otherLayer++) {
            int shiftIndex = 1 << otherLayer;
            if ((shiftIndex & allLayers) == 0)
                continue;
            if (!Physics2D.GetIgnoreLayerCollision(layer, otherLayer))
                mask |= shiftIndex;
        }
        return mask;
    }

    /* Call to check if the object is about to walk off a platform (flat platforms only) */
    public bool onRightEdge() {
        //find bottom right of this gameObject
        Rect bounds = boundsOfColliders(GetComponents<Collider2D>());
        Vector2 pt = new Vector2(bounds.xMax, bounds.yMin);
        float raycastDistance = raycastDownDistance;
        RaycastHit2D rh2d = Physics2D.Raycast(
            pt, //origin
            new Vector2(0, -1), //direction
            raycastDistance,  //distance
            getLayerCollisionMask(gameObject.layer));
        return (rh2d.collider == null);
    }
    public bool onLeftEdge() {
        //find bottom left of this gameObject
        Rect bounds = boundsOfColliders(GetComponents<Collider2D>());
        Vector2 pt = new Vector2(bounds.xMin, bounds.yMin);
        float raycastDistance = raycastDownDistance;
        RaycastHit2D rh2d = Physics2D.Raycast(
            pt, //origin
            new Vector2(0, -1), //direction
            raycastDistance,  //distance
            getLayerCollisionMask(gameObject.layer));
        return (rh2d.collider == null);
    }

	/* Call this to attempt to snap an object down to a platform
	 * if it's close enough.
	 * returns if the raycast hit something */
	public bool raycastDownCorrection(){
		//find bottom center of this gameObject
		Collider2D[] colliders = this.GetComponents<Collider2D>();
		if (colliders.Length == 0)
			return false;
		Vector2 botPoint = new Vector2(
			colliders[0].bounds.center.x,
			colliders[0].bounds.min.y);
		foreach (Collider2D coldr in colliders){
			if (coldr.bounds.min.y < botPoint.y){
				botPoint.x = coldr.bounds.center.x;
				botPoint.y = coldr.bounds.min.y;
			}
		}
		botPoint.y -= .01f;
		
		float raycastDistance = raycastDownDistance + rb2d.velocity.y*Time.deltaTime;
		RaycastHit2D rh2d = Physics2D.Raycast(
			botPoint, //origin
			new Vector2(0, -1), //direction
			raycastDistance,
            getLayerCollisionMask(gameObject.layer)); //distance
		
		if (rh2d.collider == null){
			//raycast hit nothing
			return false;
		}
		Vector2 normal = rh2d.normal;
		float contactAngle = Mathf.Atan2(normal.y, normal.x); //assuming range is in [-PI, PI]
		
		//don't apply if slope is too steep
		float caDeg = contactAngle *180/Mathf.PI;
		if (caDeg <= bottomAngleThreshold ||
		    180-bottomAngleThreshold <= caDeg){
			return false;
		}
		
		Vector2 hitPoint = rh2d.point;
		//reposition this gameObject
		rb2d.position = new Vector2(
			rb2d.position.x,
			rb2d.position.y + (hitPoint.y - botPoint.y));
		//reset y velocity to match slope
		float a = contactAngle - Mathf.PI/2;
		float vy = rb2d.velocity.x * Mathf.Atan(a) * 1.4f;
		float g = raycastDownGravity*Time.deltaTime;
		vy -= g;
		rb2d.velocity = new Vector2(
			rb2d.velocity.x,
			vy);
		return true;
		
	}

	// initialization
	void Awake() {
		Debug.Assert(0 <= bottomAngleThreshold && bottomAngleThreshold <= 90);
		rb2d = this.GetComponent<Rigidbody2D>();
	}

	void Start() {
		Rigidbody2D[] rbs = this.GetComponents<Rigidbody2D>();
		foreach (Rigidbody2D rb in rbs){
			rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
		}
	}

    // called when a collision happens
	// note this will be called before the frame's Update function
	void OnCollisionStay2D(Collision2D col) {

		GameObject gO = col.gameObject;
        
		foreach (ContactPoint2D cp in col.contacts){
            
			float contactAngle = Mathf.Atan2(cp.normal.y, cp.normal.x); //assuming range is in [-PI, PI]
			float caDeg = contactAngle *180/Mathf.PI;

			if (bottomAngleThreshold < caDeg &&
			    caDeg <= 180-bottomAngleThreshold){
				//normal pointing up; hit bottom
				_hitBottom = true;
				_normalBottom = contactAngle;
				_gosHitBottom.Add(gO);

			} else if (-bottomAngleThreshold < caDeg &&
			           caDeg <= bottomAngleThreshold){
				//normal pointing right; hit left
				_hitLeft = true;
				_normalLeft = contactAngle;
				_gosHitLeft.Add(gO);
                
			} else if (-180+bottomAngleThreshold < caDeg &&
			           caDeg <= -bottomAngleThreshold){
				//normal pointing down; hit top
				_hitTop = true;
				_normalTop = contactAngle;
				_gosHitTop.Add(gO);

			} else {
				//normal pointing left; hit right
				_hitRight = true;
				_normalRight = contactAngle;
				_gosHitRight.Add(gO);

			}

		}

        /* Doing a dumb additional check for when object is hit from the
         * side and bottom of the same object, and hitting the bottom is
         * not detected. */
        if ((_hitLeft || _hitRight) &&
            col.contacts.Length == 1 && !_hitBottom) {
            //find bottom center of this gameObject
            Collider2D[] colliders = this.GetComponents<Collider2D>();
            Vector2 botPoint = new Vector2(
                colliders[0].bounds.center.x,
                colliders[0].bounds.min.y);
            foreach (Collider2D coldr in colliders) {
                if (coldr.bounds.min.y < botPoint.y) {
                    botPoint.x = coldr.bounds.center.x;
                    botPoint.y = coldr.bounds.min.y;
                }
            }
            botPoint.y -= .01f;

            float raycastDistance = .5f;
            RaycastHit2D rh2d = Physics2D.Raycast(
                botPoint, //origin
                new Vector2(0, -1), //direction
                raycastDistance, //distance
                getLayerCollisionMask(gameObject.layer));

            if (rh2d.collider != null) { //raycaster hit something
                Vector2 normal = rh2d.normal;
                float contactAngle = Mathf.Atan2(normal.y, normal.x); //assuming range is in [-PI, PI]
                //only apply if slope isn't too deep
                float caDeg = contactAngle * 180 / Mathf.PI;
                if (bottomAngleThreshold < caDeg &&
			        caDeg <= 180-bottomAngleThreshold) {
                    _hitBottom = true;
                    _normalBottom = contactAngle;
                    _gosHitBottom.Add(gO);

                    //raycastDownCorrection();
                }
            }
        }

	}

	//called right before the internal physics update
	void FixedUpdate() {
		//reset variables
		_hitBottom = false;
		_hitLeft = false;
		_hitTop = false;
		_hitRight = false;
		_gosHitBottom.Clear();
		_gosHitLeft.Clear();
		_gosHitRight.Clear();
		_gosHitTop.Clear();
	}

	private bool _hitBottom = false;
	private bool _hitLeft = false;
	private bool _hitRight = false;
	private bool _hitTop = false;
	private float _normalBottom = Mathf.PI/2;
	private float _normalLeft = 0;
	private float _normalTop = -Mathf.PI/2;
	private float _normalRight = Mathf.PI;
	private List<GameObject> _gosHitBottom = new List<GameObject>();
	private List<GameObject> _gosHitLeft = new List<GameObject>();
	private List<GameObject> _gosHitRight = new List<GameObject>();
	private List<GameObject> _gosHitTop = new List<GameObject>();
	private Rigidbody2D rb2d;

}
