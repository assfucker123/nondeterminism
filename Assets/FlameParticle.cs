using UnityEngine;
using System.Collections;

public class FlameParticle : MonoBehaviour {

    public float speed = 10;
    public float heading = 0;
    public Sprite[] possibleSprites;
    public float maxScale = 1.2f;
    public float minScale = .6f;

	void Awake() {
        timeUser = GetComponent<TimeUser>();
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
	}

    void Start() {
        spriteRenderer.sprite = possibleSprites[Mathf.FloorToInt(timeUser.randomValue() * possibleSprites.Length)];
        float scale = minScale + (maxScale - minScale) * timeUser.randomValue();
        transform.localScale = new Vector3(scale, scale, 1);
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        rb2d.velocity = new Vector2(speed * Mathf.Cos(heading * Mathf.PI / 180), speed * Mathf.Sin(heading * Mathf.PI / 180));

	}

    TimeUser timeUser;
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;
}
