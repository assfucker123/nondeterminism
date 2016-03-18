using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Parallax))]
public class ChamberTri : MonoBehaviour {

    public float width = 50 / 16f;
    public float height = 42 / 16f;
    public bool up = true;

    public Vector2 targetPos = new Vector2();
    public Vector2 startPos = new Vector2();

    public float alpha {
        get { return _alpha; }
        set {
            _alpha = value;
            spriteRenderer.enabled = (_alpha > 0);
            Color c = spriteRenderer.color;
            c.a = _alpha;
            spriteRenderer.color = c;
        }
    }

    public SpriteRenderer spriteRenderer { get; private set; }
    public Parallax parallax { get; private set; }

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        parallax = GetComponent<Parallax>();
	}
	
	void Update() {

	}

    float _alpha = 0;
    
}
