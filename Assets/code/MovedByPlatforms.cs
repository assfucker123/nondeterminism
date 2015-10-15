using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovedByPlatforms : MonoBehaviour {

	// INITIALIZATION
	void Awake() {
		rb2d = this.GetComponent<Rigidbody2D>();
		colFinder = this.GetComponent<ColFinder>();
		Debug.Assert(rb2d);
		Debug.Assert(colFinder);
	}
	
	// PUBLIC PROPERTIES
	public bool clingsBottom {
		get { return _clingsBottom; }
		set {
			if (value == _clingsBottom) return;
			_clingsBottom = value;
			if (!_clingsBottom){

			}
		}
	}
	public bool clingsLeft {
		get { return _clingsLeft; }
		set {
			if (value == _clingsLeft) return;
			_clingsLeft = value;
			if (!_clingsLeft){
				
			}
		}
	}
	public bool clingsRight {
		get { return _clingsRight; }
		set {
			if (value == _clingsRight) return;
			_clingsRight = value;
			if (!_clingsRight){
				
			}
		}
	}
	public bool clingsTop {
		get { return _clingsTop; }
		set {
			if (value == _clingsTop) return;
			_clingsTop = value;
			if (!_clingsTop){
				
			}
		}
	}

	/* Do not set directly! */
	public GameObject platformClingedToBottom;
	
	// EVENT FUNCTIONS
	
	void Start() { }
	void OnEnable() { }
	void OnDisable() { }
	
	void Update() {

		if (platformClingedToBottom != null){
			bool hit = false;
			for (int i=0; i<colFinder.gameObjectsHitBottom.Length; i++){
				if (colFinder.gameObjectsHitBottom[i] == platformClingedToBottom)
					hit = true;
			}
			if (!hit){
				platformClingedToBottom.GetComponent<MovingPlatform>().disconnectBottom(this.gameObject);
			}
		}

	}


	void OnCollisionStay2D(Collision2D col) {

		float bottomAngleThreshold = colFinder.bottomAngleThreshold;
		//check if should cling to MovingPlatform hit
		GameObject gO = col.gameObject;
		MovingPlatform mPlat = gO.GetComponent<MovingPlatform>();
		if (mPlat == null)
			return;
		
		foreach (ContactPoint2D cp in col.contacts){
			float contactAngle = Mathf.Atan2(cp.normal.y, cp.normal.x); //assuming range is in [-PI, PI]
			float caDeg = contactAngle *180/Mathf.PI;
			
			if (bottomAngleThreshold < caDeg &&
			    caDeg <= 180-bottomAngleThreshold){
				//normal pointing up; hit bottom
				if (clingsBottom){
					mPlat.clingBottom(this.gameObject);
				}

			} else if (-bottomAngleThreshold < caDeg &&
			           caDeg <= bottomAngleThreshold){
				//normal pointing right; hit left
				if (clingsLeft){
				}

			} else if (-180+bottomAngleThreshold < caDeg &&
			           caDeg <= -bottomAngleThreshold){
				//normal pointing down; hit top
				if (clingsTop){
				}
				
			} else {
				//normal pointing left; hit right
				if (clingsRight){
				}
				
			}
			
		}
		
	}


	void OnCollisionExit2D(Collision2D col) {
		
		float bottomAngleThreshold = colFinder.bottomAngleThreshold;
		//check if should cling to MovingPlatform hit
		GameObject gO = col.gameObject;
		MovingPlatform mPlat = gO.GetComponent<MovingPlatform>();
		if (mPlat == null)
			return;
		
		foreach (ContactPoint2D cp in col.contacts){
			float contactAngle = Mathf.Atan2(cp.normal.y, cp.normal.x); //assuming range is in [-PI, PI]
			float caDeg = contactAngle *180/Mathf.PI;
			
			if (bottomAngleThreshold < caDeg &&
			    caDeg <= 180-bottomAngleThreshold){
				//normal pointing up; hit bottom
				if (clingsBottom){
					mPlat.disconnectBottom(this.gameObject);
				}
				
			} else if (-bottomAngleThreshold < caDeg &&
			           caDeg <= bottomAngleThreshold){
				//normal pointing right; hit left
				if (clingsLeft){
				}
				
			} else if (-180+bottomAngleThreshold < caDeg &&
			           caDeg <= -bottomAngleThreshold){
				//normal pointing down; hit top
				if (clingsTop){
				}
				
			} else {
				//normal pointing left; hit right
				if (clingsRight){
				}
				
			}
			
		}
		
	}

	void OnDestroy() {
		if (platformClingedToBottom != null){
			platformClingedToBottom.GetComponent<MovingPlatform>().disconnectBottom(this.gameObject);
		}
	}


	
	// PRIVATE PROPERTIES
	private Rigidbody2D rb2d = null;
	private ColFinder colFinder = null;

	private bool _clingsBottom = true;
	private bool _clingsLeft = false;
	private bool _clingsRight = false;
	private bool _clingsTop = false;

}
