using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovingPlatform : MonoBehaviour {

	// INITIALIZATION
	void Awake() {
		rb2d = this.GetComponent<Rigidbody2D>();
		Debug.Assert(rb2d);
	}
	
	// PUBLIC PROPERTIES

	public void clingBottom(GameObject gO){
		if (_gosClingBottom.IndexOf(gO) != -1)
			return;
		MovedByPlatforms mPlat = gO.GetComponent<MovedByPlatforms>();
		if (mPlat == null)
			return;
		_gosClingBottom.Add(gO);
		mPlat.platformClingedToBottom = this.gameObject;
	}
	public void disconnectBottom(GameObject gO){
		_gosClingBottom.Remove(gO);
		MovedByPlatforms mPlat = gO.GetComponent<MovedByPlatforms>();
		if (mPlat == null)
			return;
		mPlat.platformClingedToBottom = null;
	}
	
	// EVENT FUNCTIONS
	
	void Start() { }
	void OnEnable() { }
	void OnDisable() { }
	
	void Update() {

		moveClingedObjects();

	}
	
	// PRIVATE PROPERTIES
	private Rigidbody2D rb2d = null;
	private List<GameObject> _gosClingBottom = new List<GameObject>();


	void moveClingedObjects() {
		for (int i=0; i<_gosClingBottom.Count; i++){
			moveClingedObject(_gosClingBottom[i], Time.deltaTime);
		}
	}
	void moveClingedObject(GameObject go, float time){
		Rigidbody2D goRB2D = go.GetComponent<Rigidbody2D>();
		if (goRB2D == null)
			return;
		Vector2 pos0 = goRB2D.position;
		pos0 = pos0 - rb2d.position;
		Vector2 pos1 = new Vector2(pos0.x, pos0.y);

		//rotate around this
		float rot = rb2d.angularVelocity *Mathf.PI/180 * time;
		float c = Mathf.Cos(rot);
		float s = Mathf.Sin(rot);
		pos1.x = pos0.x*c - pos0.y*s;
		pos1.y = pos0.x*s + pos0.y*c;

		//translate
		Vector2 trans = rb2d.velocity * time;
		pos1 = pos1 + trans;

		//set position
		Vector2 diff = pos1 - pos0;
		diff.y = Mathf.Min(0, diff.y); //only for those hit bottom
		//PK_Entity pke = go.GetComponent<PK_Entity>();
		//pke.addVel = pke.addVel + diff / time    * 1;
		goRB2D.position = goRB2D.position + diff;

	}

	
}
