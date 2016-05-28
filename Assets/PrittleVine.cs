using UnityEngine;
using System.Collections;

public class PrittleVine : MonoBehaviour {

    public float maxLength = 9.85f;
    public float girth = .5f;

    public float length {
        get { return _length; }
        set {
            _length = Mathf.Clamp(value, 0, maxLength);
            spriteObject.transform.localPosition = new Vector2(_length - maxLength, 0);
        }
    }

    public float localRotation {
        get { return Utilities.get2DRot(transform.localRotation); }
        set { transform.localRotation = Utilities.setQuat(value); }
    }

    public float rotation {
        get { return Utilities.get2DRot(transform.rotation); }
        set { transform.rotation = Utilities.setQuat(value); }
    }

    public bool vision {
        get { return _vision; }
        set {
            if (_vision == value) return;
            _vision = value;
            if (_vision) {
                animator.Play("vision");
            } else {
                animator.Play("normal");
            }
        }
    }

    public float alpha {
        get { return spriteObject.GetComponent<SpriteRenderer>().color.a; }
        set {
            spriteObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, value);
        }
    }

    public GameObject spriteObject { get; private set; }

    public bool complete = false;

    public RaycastHit2D hitPlayer() {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        float radius = girth/2;
        Vector2 direction = new Vector2(Mathf.Cos(rotation * Mathf.Deg2Rad), Mathf.Sin(rotation * Mathf.Deg2Rad));
        RaycastHit2D rh2d = Physics2D.CircleCast(origin, radius, direction, length - radius, LayerMask.GetMask("Players"));
        return rh2d;
    }

	void Awake() {
        spriteObject = transform.Find("spriteObject").gameObject;
        animator = spriteObject.GetComponent<Animator>();

        // changing the orderInLayer of the masks prevents them from masking each other
        GetComponent<SpriteMask>().getRenderer().sortingOrder = orderInLayer;
        spriteObject.GetComponent<SpriteRenderer>().sortingOrder = orderInLayer + 1;
        orderInLayer -= 2;
	}
	
	void Update() {
		

	}
    
    float _length = 0;
    bool _vision = false;

    Animator animator;
    
    static int orderInLayer = -5;

}
