using UnityEngine;
using System.Collections;

public class ShipWall : MonoBehaviour {

    public GameObject part1GameObject;
    public Vector2 part1Pos = new Vector2();
    public Vector2 part1Vel = new Vector2();
    public GameObject part2GameObject;
    public Vector2 part2Pos = new Vector2();
    public Vector2 part2Vel = new Vector2();
    public GameObject part3GameObject;
    public Vector2 part3Pos = new Vector2();
    public Vector2 part3Vel = new Vector2();
    public GameObject destroyExplosionGameObject;
    public Vector2 explosion1Pos = new Vector2();
    public Vector2 explosion2Pos = new Vector2();
    public AudioClip destroySound;
    public AudioClip destroySound2;

    public void beDestroyed() {
        timeUser.timeDestroy();
        Vars.currentNodeData.eventHappen(AdventureEvent.Physical.DESTROYED_TUTORIAL_WALL);

        Rigidbody2D part1 = (GameObject.Instantiate(part1GameObject, transform.localPosition + new Vector3(part1Pos.x, part1Pos.y), Quaternion.identity) as GameObject).GetComponent<Rigidbody2D>();
        part1.velocity = part1Vel;
        Rigidbody2D part2 = (GameObject.Instantiate(part2GameObject, transform.localPosition + new Vector3(part2Pos.x, part2Pos.y), Quaternion.identity) as GameObject).GetComponent<Rigidbody2D>();
        part2.velocity = part2Vel;
        Rigidbody2D part3 = (GameObject.Instantiate(part3GameObject, transform.localPosition + new Vector3(part3Pos.x, part3Pos.y), Quaternion.identity) as GameObject).GetComponent<Rigidbody2D>();
        part3.velocity = part3Vel;

        // spawn explosions
        GameObject.Instantiate(destroyExplosionGameObject, transform.localPosition + new Vector3(explosion1Pos.x, explosion1Pos.y), Quaternion.identity);
        GameObject.Instantiate(destroyExplosionGameObject, transform.localPosition + new Vector3(explosion2Pos.x, explosion2Pos.y), Quaternion.identity);

        SoundManager.instance.playSFX(destroySound);
        SoundManager.instance.playSFX(destroySound2);

    }

	void Awake() {
        timeUser = GetComponent<TimeUser>();

        
	}
	
    void Start() {
        if (Vars.currentNodeData.eventHappened(AdventureEvent.Physical.DESTROYED_TUTORIAL_WALL)) {
            GameObject.Destroy(gameObject);
        }
    }

	void Update() {
		
	}

    TimeUser timeUser;
}
