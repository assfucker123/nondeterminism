using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class TimeTreeNodeIcon : MonoBehaviour {

    public Color glyphNormalColor = new Color();
    public Color glyphBoosterColor = new Color();
    public Color glyphGrayedColor = new Color();
    public Sprite normalSprite;
    public Sprite boosterSprite;
    public Sprite grayedSprite;
    public Sprite normalTemporarySprite;
    public Sprite boosterTemporarySprite;
    public Sprite grayedTemporarySprite;
    public Vector2 tokenCenter = new Vector2(0, -1);
    public int tokenMaxNumColumns = 4;
    public float tokenHorizSpacing = 2;
    public float tokenVertSpacing = 2;
    public float glyphInitialY = 3;
    public GameObject tokenGameObject;

    public string chars {
        get {
            char[] cs = {char1, char2};
            return new string(cs);
        }
        set {
            if (value.Length > 0) {
                char1 = value[0];
                if (value.Length > 1)
                    char2 = value[1];
                else
                    char2 = ' ';
            } else {
                char1 = ' ';
            }
        }
    }

    public char char1 {
        get { return glyph1.character; }
        set { glyph1.character = value; }
    }
    public char char2 {
        get { return glyph2.character; }
        set { glyph2.character = value; }
    }

    public bool booster {
        get { return _booster; }
        set {
            if (_booster == value) return;
            _booster = value;
            glyphColor = booster ? glyphBoosterColor : glyphNormalColor;
            updateSprite();
        }
    }

    public bool temporary {
        get { return _temporary; }
        set {
            if (_temporary == value) return;
            _temporary = value;
            if (temporary) {
                glyph1.visible = false;
                glyph2.visible = false;
                foreach (GameObject tok in upgradeTokens) {
                    tok.GetComponent<Image>().enabled = false;
                }
            } else {
                glyph1.visible = true;
                glyph2.visible = true;
                foreach (GameObject tok in upgradeTokens) {
                    tok.GetComponent<Image>().enabled = true;
                }
            }
            updateSprite();
        }
    }

    void updateSprite() {
        if (temporary) {
            if (grayed) {
                image.sprite = grayedTemporarySprite;
            } else {
                image.sprite = booster ? boosterTemporarySprite : normalTemporarySprite;
            }
        } else {
            if (grayed) {
                image.sprite = grayedSprite;
            } else {
                image.sprite = booster ? boosterSprite : normalSprite;
            }
        }
    }

    public bool grayed {
        get { return _grayed; }
        set {
            if (_grayed == value) return;
            _grayed = value;
            if (grayed) {
                glyph1.color = glyphGrayedColor;
                glyph2.color = glyphGrayedColor;
            } else {
                glyph1.color = _glyphColor;
                glyph2.color = _glyphColor;
            }
            foreach (GameObject tokenGO in upgradeTokens) {
                tokenGO.GetComponent<TimeTreeNodeUpgradeToken>().grayed = grayed;
            }
            updateSprite();
        }
    }

    public Color glyphColor {
        get {
            return _glyphColor;
        }
        set {
            _glyphColor = value;
            if (!grayed) {
                glyph1.color = value;
                glyph2.color = value;
            }
        }
    }

    public void setTokens(int healthTokens, int phaseTokens) {
        clearTokens();

        int totalTokens = healthTokens + phaseTokens;
        if (totalTokens == 0) return;
        int numRows = Mathf.CeilToInt(totalTokens * 1.0f / tokenMaxNumColumns);
        for (int i=0; i<totalTokens; i++) {
            int rowIndex = i / tokenMaxNumColumns;
            int colIndex = i % tokenMaxNumColumns;
            int numColumns = tokenMaxNumColumns;
            if (rowIndex == numRows - 1)
                numColumns = totalTokens - (numRows - 1) * tokenMaxNumColumns;
            Vector2 pos = tokenCenter;
            pos.x += Utilities.centeredSpacing(tokenHorizSpacing, colIndex, numColumns);
            pos.y -= Utilities.centeredSpacing(tokenVertSpacing, rowIndex, numRows);
            GameObject tokenGO = GameObject.Instantiate(tokenGameObject);
            tokenGO.transform.SetParent(transform, false);
            tokenGO.GetComponent<RectTransform>().localPosition = pos;
            if (i < healthTokens) {
                tokenGO.GetComponent<TimeTreeNodeUpgradeToken>().state = TimeTreeNodeUpgradeToken.State.HEALTH;
            } else {
                tokenGO.GetComponent<TimeTreeNodeUpgradeToken>().state = TimeTreeNodeUpgradeToken.State.PHASE;
            }
            upgradeTokens.Add(tokenGO);
        }
        // also adjust glyph position
        glyph1.gameObject.GetComponent<RectTransform>().localPosition = new Vector2(
            glyph1.gameObject.GetComponent<RectTransform>().localPosition.x, glyphInitialY + (numRows - 1) * (tokenVertSpacing / 2));
        glyph2.gameObject.GetComponent<RectTransform>().localPosition = new Vector2(
            glyph2.gameObject.GetComponent<RectTransform>().localPosition.x, glyph1.gameObject.GetComponent<RectTransform>().localPosition.y);

        if (temporary) {
            foreach (GameObject tok in upgradeTokens) {
                tok.GetComponent<Image>().enabled = false;
            }
        } else {
            foreach (GameObject tok in upgradeTokens) {
                tok.GetComponent<Image>().enabled = true;
            }
        }
    }

    public void clearTokens() {
        foreach (GameObject tok in upgradeTokens) {
            GameObject.Destroy(tok);
        }
        upgradeTokens.Clear();
    }

    public bool showing {
        get { return image.enabled; }
        set {
            if (value == showing) return;
            image.enabled = value;
            if (!temporary) {
                glyph1.visible = value;
                glyph2.visible = value;
                foreach (GameObject tok in upgradeTokens) {
                    tok.GetComponent<Image>().enabled = value;
                }
            }
        }
    }

    void Awake() {
        image = GetComponent<Image>();
        glyph1 = transform.Find("Glyph1").GetComponent<GlyphSprite>();
        glyph2 = transform.Find("Glyph2").GetComponent<GlyphSprite>();
	}

    void Start() {
        glyphColor = glyphNormalColor;
    }
	
	void Update() {
		
	}

    void OnDestroy() {
        clearTokens();
    }

    Image image;
    GlyphSprite glyph1;
    GlyphSprite glyph2;
    bool _booster = false;
    bool _grayed = false;
    bool _temporary = false;
    Color _glyphColor = new Color();
    List<GameObject> upgradeTokens = new List<GameObject>();

}
