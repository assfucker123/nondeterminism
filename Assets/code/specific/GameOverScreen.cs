using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameOverScreen : MonoBehaviour {

    public class GOSNode {
        public float x = 0;
        public float y = 0;
        public float angle = 0;
        public bool oneBranch = false;
        public GOSNode left = null; //is the first branch
        public GOSNode right = null;

        public void destroy() {
            x = 0;
            y = 0;
            angle = 0;
            oneBranch = false;
            left = null;
            right = null;
            recycledNodes.Add(this);
        }

        public static GOSNode createNode() {
            GOSNode ret = null;
            if (recycledNodes.Count > 0) {
                ret = recycledNodes[recycledNodes.Count - 1];
                recycledNodes.RemoveAt(recycledNodes.Count - 1);
            } else {
                ret = new GOSNode();
            }
            return ret;
        }
        public static void clearAllRecycledNodes() {
            recycledNodes.Clear();
        }
        private static List<GOSNode> recycledNodes = new List<GOSNode>();

    }

    public float startY = 12;
    public float startX = 0;
    public float startXRange = 5;
    public float startXDeadRange = 1; // won't appear in this range in the middle
    public float startAngleRange = 30;
    public float distMin = .7f;
    public float distMax = 1.3f;
    public float angleMin = 20;
    public float angleMax = 50;
    public float timePerBranch = .4f;
    public Color startGradientColor = Color.red;
    public Color endGradientColor = Color.red;
    public float endGradientY = -12;
    public float startLineWidth = .13f;
    public float endLineWidth = .12f;
    public float treeMaxDuration = 3.0f;
    public GameObject gameOverLineRendererGameObject;
    public AudioClip crackSound;
    public GameObject gameOverGlyphGameObject;
    public Vector2 glyphStartPosition = new Vector2(-500,100);
    public float glyphSpacing = 90;
    public string glyphText = "ORACLE  DIES";
    public float glyphAppearInitialDelay = .2f;
    public float glyphAppearDuration = 2.0f;
    public float glyphDisappearDuration = .3f;
    public AudioClip glyphSound;
    public GameObject gameOverTextGameObject;
    public Vector2 gameOverTextPosition = new Vector2(0, 130);
    public float textAppearDuration = .5f;
    public float uiAppearsDuration = .5f; //after text appears
    public AudioClip humSound;
    public AudioClip uiSwitchSound;
    public TextAsset textAsset;

    public bool activated { get { return _activated; } }
    public bool cannotRevert { get {
        if (!activated) return false;
        return (treeTime >= treeMaxDuration);
    } }

    public void makeTree(float time) {
        time = Utilities.easeOutCubicClamp(time, 0, treeMaxDuration, treeMaxDuration);

        // create nodes
        clearTree(rootNode);
        rootNode = GOSNode.createNode();
        rootNode.y = startY;
        Random.seed = randSeed;

        float deadRatio = startXDeadRange / startXRange;
        float randRange = Random.Range(-1.0f, 1.0f);
        if (randRange > 0) {
            randRange = Utilities.easeLinear(randRange, deadRatio, 1 - deadRatio, 1);
        } else {
            randRange = Utilities.easeLinear(-randRange, -deadRatio, -1 + deadRatio, 1);
        }

        rootNode.x = startX + randRange * startXRange;
        rootNode.angle = 270 - randRange * startAngleRange;
        rootNode.oneBranch = true;
        makeTreeNodeHelper(rootNode, Random.seed, time);

        // draw nodes
        while (lineRenderers.Count > numBranches) {
            UnityEngine.UI.Extensions.UILineRenderer lr = lineRenderers[lineRenderers.Count - 1];
            GameObject.Destroy(lr.gameObject);
            lineRenderers.RemoveAt(lineRenderers.Count - 1);
        }
        while (lineRenderers.Count < numBranches) {
            GameObject lrGO = GameObject.Instantiate(gameOverLineRendererGameObject) as GameObject;
            lrGO.transform.SetParent(transform, false);
            lineRenderers.Add(lrGO.GetComponent<UnityEngine.UI.Extensions.UILineRenderer>());
        }
        branchIndex = 0;
        drawTreeNodeHelper(rootNode, time);

    }

    

    public void initialHide() {
        GameObject gotGO = GameObject.Instantiate(gameOverTextGameObject) as GameObject;
        gotGO.transform.SetParent(HUD.instance.GetComponent<Canvas>().transform, false);
        gotGO.GetComponent<RectTransform>().localPosition = new Vector3(gameOverTextPosition.x, gameOverTextPosition.y);
        text = gotGO.GetComponent<UnityEngine.UI.Image>();
        selection = gotGO.transform.Find("Selection").GetComponent<UnityEngine.UI.Image>();
        continueText = gotGO.transform.Find("ContinueText").GetComponent<GlyphBox>();
        quitText = gotGO.transform.Find("QuitText").GetComponent<GlyphBox>();
        
        text.enabled = false;
        selection.enabled = false;
        continueText.makeAllCharsInvisible();
        quitText.makeAllCharsInvisible();
    }

    public void activate() {
        if (activated) return;

        randSeed = (int)(timeUser.randomValue() * int.MaxValue);
        treeTime = 0;
        _activated = true;
    }

    public void deactivate() {
        if (!activated) return;

        clearTree(rootNode);
        while (lineRenderers.Count > 0) {
            UnityEngine.UI.Extensions.UILineRenderer lr = lineRenderers[lineRenderers.Count - 1];
            GameObject.Destroy(lr.gameObject);
            lineRenderers.RemoveAt(lineRenderers.Count - 1);
        }
        numBranches = 0;
        rootNode = null;
        HUD.instance.blackScreen.color = Color.clear;
        SoundManager.instance.volumeScale = 1;
        _activated = false;
    }

    public static GameOverScreen instance {  get { return _instance; } }

    /////////////
    // PRIVATE //
    /////////////

	void Awake() {
        if (_instance != null) {
            GameObject.Destroy(_instance.gameObject);
        }
        _instance = this;

        text = GetComponent<UnityEngine.UI.Image>();
        timeUser = GetComponent<TimeUser>();
        propAsset = new Properties(textAsset.text);
	}

    void Start() {
        glyphText = propAsset.getString("oracle_dies");
    }

    private void clearTree(GOSNode root) {
        if (root == null)
            return;
        if (root.left != null) {
            clearTree(root.left);
            root.left = null;
        }
        if (root.right != null) {
            clearTree(root.right);
            root.right = null;
        }
        root.destroy();
        numBranches = 0;
    }

    private void drawTreeNodeHelper(GOSNode root, float time) {
        if (time < 0)
            return;
        float t = 1;
        if (time < timePerBranch) {
            t = time / timePerBranch;
        }
        t = Utilities.easeOutQuadClamp(t, 0, 1, .9f);


        if (root.left != null) {
            setLineRenderer(lineRenderers[branchIndex], root.x, root.y,
                root.x + (root.left.x - root.x) * t,
                root.y + (root.left.y - root.y) * t);
            branchIndex++;
        }
        if (branchIndex >= lineRenderers.Count)
            return;
        if (root.right != null) {
            setLineRenderer(lineRenderers[branchIndex], root.x, root.y,
                root.x + (root.right.x - root.x) * t,
                root.y + (root.right.y - root.y) * t);
            branchIndex++;
        }
        if (branchIndex >= lineRenderers.Count)
            return;

        if (root.left != null) {
            drawTreeNodeHelper(root.left, time - timePerBranch);
        }
        if (root.right != null) {
            drawTreeNodeHelper(root.right, time - timePerBranch);
        }
    }

    private void setLineRenderer(UnityEngine.UI.Extensions.UILineRenderer lr, float x0, float y0, float x1, float y1) {

        Color color0 = Color.Lerp(startGradientColor, endGradientColor, (y0 - startY) / (endGradientY - startY));
        
        float width0 = Utilities.easeLinearClamp(startY - y0, startLineWidth, endLineWidth - startLineWidth, startY - endGradientY);
        
        float dist2 = (x0 - x1) * (x0 - x1) + (y0 - y1) * (y0 - y1);
        lr.Points[0].Set(x0, y0);
        lr.Points[1].Set(x1, y1);
        if (dist2 < 15 * 15) {
            lr.color = Color.clear;
        } else {
            lr.color = color0;
        }
        lr.LineThickness = width0;
        lr.SetVerticesDirty();
    }

    private void makeTreeNodeHelper(GOSNode root, int randSeed, float time) {
        if (time < 0)
            return;
        Random.seed = randSeed;
        if (root.oneBranch) {
            root.left = GOSNode.createNode();
            root.left.angle = root.angle;
            root.left.oneBranch = false;
            numBranches++;
        } else {
            // make two branches
            root.left = GOSNode.createNode();
            root.right = GOSNode.createNode();
            float angleSpread = Random.Range(angleMin, angleMax);
            root.left.angle = root.angle - angleSpread / 2;
            root.right.angle = root.angle + angleSpread / 2;
            root.left.oneBranch = (Random.value < .5f);
            root.right.oneBranch = !root.left.oneBranch;
            numBranches += 2;
        }
        // set positions for nodes and
        // make child nodes
        float distL = Random.Range(distMin, distMax);
        float distR = Random.Range(distMin, distMax);
        int seed1 = (int)(Random.value * int.MaxValue);
        int seed2 = (int)(Random.value * int.MaxValue);
        if (root.left != null) {
            root.left.x = root.x + distL * Mathf.Cos(root.left.angle * Mathf.PI / 180);
            root.left.y = root.y + distL * Mathf.Sin(root.left.angle * Mathf.PI / 180);
            makeTreeNodeHelper(root.left, seed1, time - timePerBranch);
        }
        if (root.right != null) {
            root.right.x = root.x + distR * Mathf.Cos(root.right.angle * Mathf.PI / 180);
            root.right.y = root.y + distR * Mathf.Sin(root.right.angle * Mathf.PI / 180);
            makeTreeNodeHelper(root.right, seed2, time - timePerBranch);
        }
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        if (activated) {
            float prevTime = treeTime;
            treeTime += Time.deltaTime;

            // .06, .13, .22, .32, .47

            if (prevTime < treeMaxDuration * .01f && treeTime >= treeMaxDuration * .01f) {
                SoundManager.instance.playSFXRandPitchBendIgnoreVolumeScale(crackSound, .05f, 1.0f);
            }
            if (prevTime < treeMaxDuration * .06f && treeTime >= treeMaxDuration * .06f) {
                SoundManager.instance.playSFXRandPitchBendIgnoreVolumeScale(crackSound, .05f, .1f);
            }
            if (prevTime < treeMaxDuration * .13f && treeTime >= treeMaxDuration * .13f) {
                SoundManager.instance.playSFXRandPitchBendIgnoreVolumeScale(crackSound, .05f, .7f);
            }
            if (prevTime < treeMaxDuration * .22f && treeTime >= treeMaxDuration * .22f) {
                SoundManager.instance.playSFXRandPitchBendIgnoreVolumeScale(crackSound, .05f, .1f);
            }
            if (prevTime < treeMaxDuration * .32f && treeTime >= treeMaxDuration * .32f) {
                SoundManager.instance.playSFXRandPitchBendIgnoreVolumeScale(crackSound, .05f, .4f);
            }
            if (prevTime < treeMaxDuration * .47f && treeTime >= treeMaxDuration * .47f) {
                SoundManager.instance.playSFXRandPitchBendIgnoreVolumeScale(crackSound, .05f, .1f);
            }

            if (callUIUpdate) {
                uiUpdate();
            } else {
                if (treeTime < treeMaxDuration) {
                    makeTree(treeTime);
                }
                setBlackScreen();
            }

            updateGlyphs();

        }
        
        
	}

    void updateGlyphs() {

        glyphTime += Time.deltaTime;

        int numGlyphs = glyphText.Length;
        float durEach = glyphAppearDuration / numGlyphs;

        if (glyphTime - glyphAppearInitialDelay > durEach * glyphSpawnIndex && glyphSpawnIndex < numGlyphs) {
            // spawn glyph
            GameObject gogGO = GameObject.Instantiate(gameOverGlyphGameObject);
            if (glyphText[glyphSpawnIndex] != ' ') {
                SoundManager.instance.playSFX(glyphSound);
            }
            //gogGO.transform.SetParent(transform, false);
            gogGO.transform.SetParent(text.transform, false);
            GameOverGlyphUI gog = gogGO.GetComponent<GameOverGlyphUI>();
            gog.rectTransform.localPosition = new Vector3(glyphStartPosition.x + glyphSpawnIndex * glyphSpacing, glyphStartPosition.y, 0);
            gog.glyphSprite.character = glyphText[glyphSpawnIndex];
            gog.existDuration = treeMaxDuration - glyphTime - (glyphDisappearDuration * (numGlyphs - glyphSpawnIndex) / numGlyphs);
            glyphSpawnIndex++;
        }

    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["tt"] = treeTime;
        fi.bools["activated"] = activated;
        fi.floats["gt"] = glyphTime;
        fi.ints["gsi"] = glyphSpawnIndex;
    }

    void OnRevert(FrameInfo fi) {
        bool prevActivated = activated;
        treeTime = fi.floats["tt"];
        glyphTime = fi.floats["gt"];
        glyphSpawnIndex = fi.ints["gsi"];
        bool act = fi.bools["activated"];
        if (!act && prevActivated) {
            deactivate();
        } else if (act && !prevActivated) {
            activate();
        } else if (act) {
            makeTree(treeTime);
        }
        setBlackScreen();
    }

    void OnDestroy() {
        if (instance == this) {
            _instance = null;
        }
        foreach (UnityEngine.UI.Extensions.UILineRenderer lr in lineRenderers) {
            GameObject.Destroy(lr.gameObject);
        }
        lineRenderers.Clear();
        GOSNode.clearAllRecycledNodes();
        GameObject.Destroy(text.gameObject);
        text = null;
    }

    void setBlackScreen() {
        if (!activated) return;
        float t = treeTime / treeMaxDuration;
        t = Utilities.easeOutCubicClamp(t, 0, 1, 1);
        HUD.instance.blackScreen.color = Color.Lerp(Color.clear, Color.white, t);
        SoundManager.instance.volumeScale = Mathf.Max(0, 1 - t);

        if (treeTime >= treeMaxDuration) {
            if (!text.enabled) {
                text.enabled = true;
                //play appear sound effect here
                SoundManager.instance.playSFXIgnoreVolumeScale(humSound);
            }
            text.color = Color.Lerp(Color.clear, Color.white, (treeTime - treeMaxDuration) / textAppearDuration);
            if (treeTime >= treeMaxDuration + textAppearDuration + uiAppearsDuration) {
                showUI();
            }
        } else {
            if (text.enabled)
                text.enabled = false;
        }
    }

    void showUI() {
        if (callUIUpdate) return;
        Time.timeScale = 0;

        selection.enabled = true;
        selectionIndex = 0;
        continueText.makeAllCharsVisible();
        continueText.setPlainText(propAsset.getString("continue"));
        quitText.makeAllCharsVisible();
        quitText.setPlainText(propAsset.getString("quit"));

        // select restartText first
        selection.rectTransform.localPosition = new Vector3(
            continueText.rectTransform.localPosition.x + selectionImageOffset.x,
            continueText.rectTransform.localPosition.y + selectionImageOffset.y);
        continueText.setColor(PauseScreen.SELECTED_COLOR, true);
        quitText.setColor(PauseScreen.DEFAULT_COLOR, true);

        callUIUpdate = true;
    }
    void uiUpdate() {

        if (Keys.instance.downPressed || Keys.instance.upPressed) {
            //setSelection(selectionIndex + 1);
            SoundManager.instance.playSFXIgnoreVolumeScale(uiSwitchSound);

            GlyphBox textFrom;
            GlyphBox textTo;
            if (selectionIndex == 0) {
                selectionIndex = 1;
                textFrom = continueText;
                textTo = quitText;
            } else {
                selectionIndex = 0;
                textFrom = quitText;
                textTo = continueText;
            }
            textFrom.setColor(PauseScreen.DEFAULT_COLOR, true);
            textTo.setColor(PauseScreen.SELECTED_COLOR, true);
            selectionPos0.Set(selection.rectTransform.localPosition.x, selection.rectTransform.localPosition.y);
            selectionPos1 = new Vector2(textTo.rectTransform.localPosition.x, textTo.rectTransform.localPosition.y) +
                selectionImageOffset;
            selectionImageTime = 0;
        }

        float selectionTransDur = .1f;
        if (selectionImageTime < selectionTransDur) {
            selectionImageTime += Time.unscaledDeltaTime;
            selection.rectTransform.localPosition = new Vector3(
                Utilities.easeOutQuadClamp(selectionImageTime, selectionPos0.x, selectionPos1.x - selectionPos0.x, selectionTransDur),
                Utilities.easeOutQuadClamp(selectionImageTime, selectionPos0.y, selectionPos1.y - selectionPos0.y, selectionTransDur)
                );
        }

        if (Keys.instance.confirmPressed || Keys.instance.startPressed) {
            HUD.instance.destroyGameOverScreen();

            if (selectionIndex == 0) { // continue

                // when restarting, refill Phase for mercy
                HUD.instance.phaseMeter.setPhase(Player.instance.maxPhase);

                Vars.restartLevel();
            } else if (selectionIndex == 1) { // quit game
                Vars.goToTitleScreen();
            }
        }

    }


    bool _activated = false;
    List<UnityEngine.UI.Extensions.UILineRenderer> lineRenderers = new List<UnityEngine.UI.Extensions.UILineRenderer>();
    int randSeed = 0;
    float treeTime = 0;
    bool callUIUpdate = false;

    GOSNode rootNode = null;
    int numBranches = 0;
    int branchIndex = 0;

    int selectionIndex = 0;
    Vector2 selectionImageOffset = new Vector2(100, -17);
    Vector2 selectionPos0 = new Vector2();
    Vector2 selectionPos1 = new Vector2();
    float selectionImageTime = 9999;

    float glyphTime = 0;
    int glyphSpawnIndex = 0;


    private static GameOverScreen _instance = null;

    // components
    UnityEngine.UI.Image text;
    UnityEngine.UI.Image selection;
    GlyphBox continueText;
    GlyphBox quitText;
    TimeUser timeUser;
    Properties propAsset;
	
}
