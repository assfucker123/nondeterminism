using UnityEngine;
using System.Collections;

public class SnowParticle : MonoBehaviour {

    public Color color1 = new Color();
    public Color color2 = new Color();
    public Sprite[] particles;
    [HideInInspector]
    public Vector2 startPos = new Vector2();
    public float heading {
        get { return _heading; }
        set {
            _heading = value;
        }
    }
    [HideInInspector]
    public float speed = 30;
    [HideInInspector]
    public float waveTimeOffset = 0;
    public float waveMagnitude = 2;
    public float wavePeriod = 2;


    public void setSprite() {
        setSprite(Random.Range(0, particles.Length), Color.Lerp(color1, color2, Random.value));
    }
    public void setSprite(int particleIndex, Color color) {
        spriteRenderer.sprite = particles[particleIndex];
        spriteRenderer.color = color;
    }

    /// <summary>
    /// Calculates position over time.
    /// </summary>
    public Vector2 getPosition(float time) {
        Vector2 pos = new Vector2();
        pos.x = speed * time;
        pos.y = waveMagnitude * Mathf.Sin((time + waveTimeOffset) * Mathf.PI * 2 / wavePeriod);
        pos = Utilities.rotateAroundPoint(pos, Vector2.zero, heading * Mathf.Deg2Rad);
        pos += startPos;
        return pos;
    }

	void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
	}
	
    SpriteRenderer spriteRenderer;
    float _heading = 0;
}
