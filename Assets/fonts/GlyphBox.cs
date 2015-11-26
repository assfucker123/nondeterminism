using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* NEW PLAN: CANNOT USE FLASHBACK WHEN THE GAME IS PAUSED (E.G. IN PAUSE MENU)
 * BUT IT CAN STILL BE USED WHEN TEXT IS DISPLAYED IN-GAME (E.G. GAMEPLAY OR CUTSCENES)
 * */

public class GlyphBox : MonoBehaviour {

    [System.Serializable]
    public class GlyphStyle {
        
        public Color color = Color.white;
        public void apply(GlyphSprite glyphSprite) {
            glyphSprite.color = color;
        }
        public string toString() {
            string ret = "";
            ret += TimeUser.colorToString(color);
            return ret;
        }
        public void setFromString(string str) {
            char[] delims = { ';' };
            string[] strs = str.Split(delims);
            color = TimeUser.stringToColor(strs[0]);
        }

        public static GlyphStyle secretTextStyle {
            get {
                GlyphStyle ret = new GlyphStyle();
                ret.color = new Color(.5f, .5f, .5f, .5f);
                return ret;
            }
        }

    }

    public enum Alignment {
        LEFT,
        CENTER,
        RIGHT
    }

    ////////////////
    // PROPERTIES //
    ////////////////

    /* Represents all the "fonts" that can be used. */
    public GameObject[] glyphGameObjects;
    public int width = 10; // kept constant once Start is called
    public int height = 2; // kept constant once Start is called
    public string initialText = "";
    public Alignment initialAlignment = Alignment.LEFT;
    public GlyphStyle defaultStyle;
    public bool useTimeUser = true;

    public bool uiMode { get { return gameObject.layer == LayerMask.NameToLayer("UI"); } }
    public int visibleChars {
        get { return _visibleChars; }
        set {
            if (visibleChars == value) return;
            _visibleChars = value;
            updateGlyphs(false);
        }
    }
    public int visibleSecretChars {
        get { return _visibleSecretChars; }
        set {
            if (visibleSecretChars == value) return;
            _visibleSecretChars = value;
            updateGlyphs(false);
        }
    }

    public Alignment alignment {
        get { return _alignment; }
        set {
            if (alignment == value) return;
            if (glyphs.Count < 1 || glyphs[0].Count < 1) {
                _alignment = value;
                return;
            }
            Vector2 pos0 = getNormalPostion(glyphs[0][0], 0, 0);
            _alignment = value;
            Vector2 pos1 = getNormalPostion(glyphs[0][0], 0, 0);
            Vector2 diff = pos1 - pos0;
            foreach (List<GlyphSprite> row in glyphs) {
                foreach (GlyphSprite gs in row) {
                    gs.position += diff;
                }
            }
        }
    }

    Alignment _alignment = Alignment.LEFT;

    ///////////////
    // FUNCTIONS //
    ///////////////

    /* Sets the text without applying any style. */
    public void setPlainText(string text) {
        setPlainText(text, 99999);
        _visibleChars = 0;
        for (int i = 0; i < height; i++) {
            _visibleChars += lines[i].Length;
        }
    }
    public void setPlainText(string text, int visibleChars) {
        List<string> lines = textToLines(text);
        for (int i = 0; i < this.lines.Count; i++) {
            if (i >= lines.Count)
                this.lines[i] = "";
            else
                this.lines[i] = lines[i];
            if (i >= secretLines.Count)
                this.secretLines[i] = "";
            else
                this.secretLines[i] = secretLines[i];
        }
        _visibleChars = visibleChars;
        updateGlyphs(false);
    }

    /* Adds secret text (revealed when visibleSecretChars > visibleChars) */
    public void insertSecretText(string text, int startLine, int startCharIndex) {
        if (startLine < 0 || startLine >= height) return;
        if (startCharIndex < 0 || startCharIndex >= width) return;
        string line = secretLines[startLine];
        while (line.Length < startCharIndex + text.Length) {
            line = line + " ";
        }
        char[] cArr = line.ToCharArray();
        for (int i = 0; i < text.Length; i++) {
            cArr[startCharIndex + i] = text[i];
        }
        secretLines[startLine] = new string(cArr);
    }

    /* Clears all text and resets visibleChars to 0 */
    public void clearText() {
        for (int i = 0; i < height; i++) {
            lines[i] = "";
            secretLines[i] = "";
        }
        _visibleChars = 0;
        _visibleSecretChars = 0;
    }

    /* Sets the GlyphStyle for all the Glyphs */
    public void setStyle(GlyphStyle style, bool updateGlyphs = true) {
        string styleStr = style.toString();
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                glyphStyles[y][x].setFromString(styleStr);
            }
        }
        if (updateGlyphs)
            this.updateGlyphs(true);
    }
    /* Sets the GlyphStyle for some Glyphs */
    public void setStyle(GlyphStyle style, int lineIndex, int startCharIndex, int endCharIndex, bool updateGlyphs = true) {
        string styleStr = style.toString();
        if (lineIndex < 0 || lineIndex >= height) return;
        if (startCharIndex < 0 || startCharIndex >= width) return;
        for (int x = startCharIndex; x < Mathf.Min(endCharIndex, width); x++) {
            glyphStyles[lineIndex][x].setFromString(styleStr);
        }
        if (updateGlyphs)
            this.updateGlyphs(true);
    }

    public GameObject getGlyphGameObjectByName(string name) {
        for (int i = 0; i < glyphGameObjects.Length; i++) {
            GameObject gGO = glyphGameObjects[i];
            GlyphSprite gs = gGO.GetComponent<GlyphSprite>();
            if (gs.name == name)
                return gGO;
        }
        return null;
    }

    /* gets expected position for a glyphSprite if nothing was done to it */
    public Vector2 getNormalPostion(GlyphSprite glyphSprite, int x, int y) {
        Vector2 ret = new Vector2();
        float width = 0;
        float height = 0;
        if (uiMode) {
            width = rectTransform.rect.width;
            height = rectTransform.rect.height;
        }

        float lineWidth = 0;
        if (y < lines.Count)
            lineWidth = lines[y].Length * glyphSprite.pixelWidth + (lines[y].Length - 1) * glyphSprite.horizSpacing;
        float alignOffset = 0;
        switch (alignment) {
        case Alignment.LEFT: alignOffset = 0; break;
        case Alignment.CENTER: alignOffset = (width - lineWidth) / 2; break;
        case Alignment.RIGHT: alignOffset = width - lineWidth; break;
        }

        ret.x = alignOffset + x * glyphSprite.pixelWidth + Mathf.Max(0, x - 1) * glyphSprite.horizSpacing;
        ret.y = y * glyphSprite.pixelHeight + Mathf.Max(0, y - 1) * glyphSprite.vertSpacing;
        ret.y *= -1;
        return ret;
    }
    
    ////////////////////
    // EVENT MESSAGES //
    ////////////////////

	void Awake() {
        rectTransform = GetComponent<RectTransform>();
		timeUser = GetComponent<TimeUser>();
	}

    void Start() {
        if (!validateFonts())
            return;
        createGlyphs(glyphGameObjects[0]);
        for (int i = 0; i < height; i++) {
            lines.Add("");
            secretLines.Add("");
        }
        setPlainText(initialText);
        alignment = initialAlignment;

        //test
        insertSecretText("star key", 0, 2);
        visibleSecretChars = 12;
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        //testing
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            visibleChars--;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            visibleChars++;
        }

    }

    void OnSaveFrame(FrameInfo fi) {
        if (!useTimeUser) return;

        fi.ints["vC"] = visibleChars;
        fi.ints["vSC"] = visibleSecretChars;
        // convert lines and secretLines into strings
        string linesStr = "";
        for (int i = 0; i < lines.Count; i++) {
            linesStr = linesStr + lines[i];
            if (i != lines.Count - 1) linesStr += '\n';
        }
        fi.strings["lines"] = linesStr;
        linesStr = "";
        for (int i = 0; i < secretLines.Count; i++) {
            linesStr = linesStr + secretLines[i];
            if (i != secretLines.Count - 1) linesStr += '\n';
        }
        fi.strings["sLines"] = linesStr;

        fi.strings["gStyles"] = glyphStylesToString();
    }

    void OnRevert(FrameInfo fi) {
        if (!useTimeUser) return;

        _visibleChars = fi.ints["vC"];
        _visibleSecretChars = fi.ints["vSC"];
        // parse lines and secretLines from strings
        char[] delims = { '\n' };
        string linesStr = fi.strings["lines"];
        string[] strs = linesStr.Split(delims);
        for (int i = 0; i < height; i++) {
            if (i < strs.Length)
                lines[i] = strs[i];
            else
                lines[i] = "";
        }
        linesStr = fi.strings["sLines"];
        string[] sStrs = linesStr.Split(delims);
        for (int i = 0; i < height; i++) {
            if (i < sStrs.Length)
                secretLines[i] = sStrs[i];
            else
                secretLines[i] = "";
        }

        glyphStylesFromString(fi.strings["gStyles"]);

        updateGlyphs(true);
    }

    ///////////////////////
    // PRIVATE FUNCTIONS //
    ///////////////////////

    /* Creates the array of glyphs from the weight and width properties */
    void createGlyphs(GameObject glyphGameObject) {
        clearGlyphs();
        for (int y = 0; y < height; y++) {
            List<GlyphSprite> row = new List<GlyphSprite>();
            List<GlyphStyle> styleRow = new List<GlyphStyle>();
            for (int x = 0; x < width; x++) {
                GlyphStyle gStyle = new GlyphStyle();
                gStyle.setFromString(defaultStyle.toString());
                styleRow.Add(gStyle);

                GameObject gGO = GameObject.Instantiate(glyphGameObject) as GameObject;
                gGO.transform.SetParent(transform, false);
                GlyphSprite gs = gGO.GetComponent<GlyphSprite>();
                gs.position = getNormalPostion(gs, x, y);
                gStyle.apply(gs);
                row.Add(gs);
            }
            glyphs.Add(row);
            glyphStyles.Add(styleRow);
        }
    }

    void clearGlyphs() {
        foreach (List<GlyphSprite> row in glyphs) {
            foreach (GlyphSprite gs in row) {
                GameObject.Destroy(gs);
            }
            row.Clear();
        }
        glyphs.Clear();
        foreach (List<GlyphStyle> row in glyphStyles) {
            row.Clear();
        }
        glyphStyles.Clear();
    }

    /* Updates GlyphSprites based on lines, visibleChars, glyphStyles, etc. */
    void updateGlyphs(bool updateStyle) {
        int charCount = 0;
        for (int y = 0; y < height; y++) {
            string line = "";
            string secretLine = "";
            if (y < lines.Count)
                line = lines[y];
            if (y < secretLines.Count)
                secretLine = secretLines[y];
            for (int x = 0; x < width; x++) {
                char character = ' ';
                bool applySecretStyle = false;
                if (x < line.Length) {
                    charCount++;
                    if (charCount <= visibleChars) {
                        character = line[x];
                    } else if (charCount <= visibleSecretChars && x < secretLine.Length) {
                        character = secretLine[x];
                        applySecretStyle = true;
                    }
                }
                glyphs[y][x].character = character;
                if (applySecretStyle) {
                    GlyphStyle.secretTextStyle.apply(glyphs[y][x]);
                } else if (updateStyle || visibleChars < visibleSecretChars) {
                    glyphStyles[y][x].apply(glyphs[y][x]);
                }
            }
        }
    }

    /* Converts the given plain text into lines, taking width into account */
    List<string> textToLines(string text) {
        List<string> ret = new List<string>();
        string str = text.Replace("\r", "");
        string word = "";
        string line = "";
        for (int i = 0; i < str.Length; i++) {
            char c = str[i];
            bool endOfStr = (i == str.Length - 1);
            bool wordComplete =
                (c == ' ' || c == '\n' || endOfStr);

            // adding new word
            if (wordComplete) {
                if (c != ' ' && c != '\n')
                    word = word + c;
                if (line.Length + 1 + word.Length > width) {
                    // too big to fit on current line, so line complete
                    ret.Add(line);
                    // add word to next line
                    line = word;
                } else {
                    // word fits on currnt line, so add it
                    if (line.Length == 0) {
                        line = word;
                    } else {
                        line = line + " " + word;
                    }
                }
                word = "";
            } else {
                // word not complete, add to word
                word = word + c;
            }

            // creating new line from '\n'
            if (c == '\n' && !endOfStr) {
                ret.Add(line);
                line = "";
            }
            // adding last line made
            if (endOfStr && line.Length > 0) {
                ret.Add(line);
            }

        }

        return ret;
    }
	
    /* Ensures UI glyphs for UI mode, normal glyphs for normal mode */
    bool validateFonts() {
        if (glyphGameObjects.Length == 0) {
            Debug.LogError("Error: glyphGameObjects is empty.");
            return false;
        }
        for (int i = 0; i < glyphGameObjects.Length; i++) {
            GameObject gGO = glyphGameObjects[i];
            GlyphSprite gs = gGO.GetComponent<GlyphSprite>();
            if (gs == null) {
                Debug.LogError("Error: GO in glyphGameObjects needs GlyphSprite script.");
                return false;
            }
            if (gs.uiMode != uiMode) {
                Debug.Log(uiMode);
                Debug.LogError("Error: ui modes for GlyphBox and GlyphSprite do not match.");
                return false;
            }
        }
        return true;
    }

    string glyphStylesToString() {
        string str = "";
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                string glyphStr = glyphStyles[y][x].toString();
                str = str + glyphStr;
                if (x != width - 1 || y != height - 1) {
                    str += "|";
                }
            }
        }
        return str;
    }
    void glyphStylesFromString(string str) {
        char[] delims = {'|'};
        string[] strs = str.Split(delims);
        for (int i = 0; i < width*height; i++) {
            int x = i % width;
            int y = i / width;
            glyphStyles[y][x].setFromString(strs[i]);
        }
    }

    List<List<GlyphSprite>> glyphs = new List<List<GlyphSprite>>();
    List<List<GlyphStyle>> glyphStyles = new List<List<GlyphStyle>>();
    int _visibleChars = 0;
    int _visibleSecretChars = 0;
    List<string> lines = new List<string>(); // hold text data
    List<string> secretLines = new List<string>(); // hold secret text data

    RectTransform rectTransform;
    TimeUser timeUser;
	
}
