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

    public bool activated { get { return _activated; } }

    public void makeTree(float time) {
        time = Utilities.easeOutCubicClamp(time, 0, treeMaxDuration, treeMaxDuration);

        // create nodes
        clearTree(rootNode);
        rootNode = GOSNode.createNode();
        rootNode.y = startY;
        Random.seed = randSeed;
        float randRange = Random.Range(-1.0f, 1.0f);
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
        text.enabled = false;
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

    /////////////
    // PRIVATE //
    /////////////

	void Awake() {
        text = GetComponent<UnityEngine.UI.Text>();
        timeUser = GetComponent<TimeUser>();
	}

    void Start() {

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
        t = Utilities.easeOutQuadClamp(t, 0, 1, .8f);


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

            makeTree(treeTime);
            setBlackScreen();
        }
        
        
	}

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["tt"] = treeTime;
        fi.bools["activated"] = activated;
    }

    void OnRevert(FrameInfo fi) {
        bool prevActivated = activated;
        treeTime = fi.floats["tt"];
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
        foreach (UnityEngine.UI.Extensions.UILineRenderer lr in lineRenderers) {
            GameObject.Destroy(lr.gameObject);
        }
        lineRenderers.Clear();
        GOSNode.clearAllRecycledNodes();
    }

    void setBlackScreen() {
        if (!activated) return;
        float t = treeTime / treeMaxDuration;
        t = Utilities.easeOutCubicClamp(t, 0, 1, 1);
        HUD.instance.blackScreen.color = Color.Lerp(Color.clear, Color.white, t);
        SoundManager.instance.volumeScale = Mathf.Max(0, 1 - t);
    }


    bool _activated = false;
    List<UnityEngine.UI.Extensions.UILineRenderer> lineRenderers = new List<UnityEngine.UI.Extensions.UILineRenderer>();
    int randSeed = 0;
    float treeTime = 0;

    GOSNode rootNode = null;
    int numBranches = 0;
    int branchIndex = 0;

    // components
    UnityEngine.UI.Text text;
    TimeUser timeUser;
	
}
