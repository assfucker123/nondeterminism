using UnityEngine;
using System.Collections;

/* Displays a glyph for a sprite font.  How to use:
 * 1. Put characters in a .png file, ready to be split into tiles.  Order of characters:

 !"#$%&'()*+,-./0123456789:;<=>?
@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_
`abcdefghijklmnopqrstuvwxyz{|}~ 
 ¡¢£¤¥¦§¨©ª«¬­ ®¯°±²³´µ¶·¸¹º»¼½¾¿

(char 32-63)
(char 64-95)
(char 96-127)
(char 160-191)

 * 2. Put a temporary solid background behind the characters.  This is to fool Unity's sprite slicer
 *      into making spaces their own tile instead of ignoring them.
 * 3. Import the sprite into Unity and slice the sprites.  Replace the original image with one without
 *      a solid background.
 * 4. Make an animator featuring all the characters as its frames
 * 5. Rename the animation in the animator to "default"
 * 6. Add this script to the created GameObject.
 * */

public class GlyphSprite : MonoBehaviour {

    public int numChars = 128;
    public int pixelWidth = 9;
    public int pixelHeight = 16;
    public float horizSpacing = 0;
    public float vertSpacing = 0;

    public char character {
        get { return _character; }
        set {
            if (character == value) return;
            _character = value;
            int cInt = (int)_character;
            if (32 <= cInt && cInt <= 127) {
                setAnimationFrame(cInt - 32);
            } else if (160 <= cInt && cInt <= 191) {
                setAnimationFrame(96 + cInt - 160);
            } else { // no frame found for this character
                setAnimationFrame(0);
            }
        }
    }
    public bool uiMode { get { return gameObject.layer == LayerMask.NameToLayer("UI"); } }
    public Color color {
        get {
            if (uiMode)
                return image.color;
            else
                return spriteRenderer.color;
        }
        set {
            if (uiMode)
                image.color = value;
            else
                spriteRenderer.color = value;
        }
    }
    public bool visible {
        get {
            if (uiMode)
                return image.enabled;
            else
                return spriteRenderer.enabled;
        }
        set {
            if (uiMode)
                image.enabled = value;
            else
                spriteRenderer.enabled = value;
        }
    }

    public Vector2 position {
        get {
            if (uiMode)
                return new Vector2(rectTransform.localPosition.x, rectTransform.localPosition.y);
            else
                return new Vector2(transform.localPosition.x, transform.localPosition.y);
        }
        set {
            if (uiMode)
                rectTransform.localPosition = value;
            else
                transform.localPosition = value;
        }
    }

    public void setAnimatorController() {
        
    }

	void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        image = GetComponent<UnityEngine.UI.Image>();
        rectTransform = GetComponent<RectTransform>();
        animator = GetComponent<Animator>();
        setAnimationFrame(0);
	}
    void Start() {
        
    }
	
	void Update() {

	}

    void setAnimationFrame(int frame) {
        animator.Play("default", 0, frame * 1.0f / numChars + .0001f);
        animator.speed = 0;
    }

    SpriteRenderer spriteRenderer;
    UnityEngine.UI.Image image;
    RectTransform rectTransform;
    Animator animator;

    char _character = ' ';
}
