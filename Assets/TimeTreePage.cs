using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Extensions;

public class TimeTreePage : MonoBehaviour {

    #region Inspector Properties

    public float SECONDS_TO_PIXELS {
        get {
            //return 1000 / CountdownTimer.MELTDOWN_DURATION;
            return 2500 / CountdownTimer.MELTDOWN_DURATION;
        }
    }
    public int startingHealthUpgradeCount {
        get {
            return 2;
        }
    }
    public float VERTICAL_SPACING = 50;
    public float minPixelDistanceForVisibleLine = 20;
    public float intervalDuration = 5*60;
    public GameObject iconGameObject;
    public GameObject selectorGameObject;
    public GameObject lineRendererGameObject;
    public GameObject intervalGameObject;
    public AudioClip nodeSwitchSound;
    public TextAsset textAsset;

    #endregion

    public class TimeTreeNode {

        public static TimeTreeNode create(NodeData nodeData) {
            TimeTreeNode ttn;
            if (recycledTTNs.Count > 0) {
                ttn = recycledTTNs[recycledTTNs.Count - 1];
                recycledTTNs.RemoveAt(recycledTTNs.Count - 1);
            } else {
                ttn = new TimeTreeNode();
            }
            ttn.nodeData = nodeData;
            ttn.nodeData.timeTreeNode = ttn;
            ttn.nodeIcon = timeTreePageInstance.addNodeIcon(ttn.nodeData.chamberPositionCode,
                timeTreePageInstance.startingHealthUpgradeCount + ttn.nodeData.healthUpgrades.Count - ttn.nodeData.phaseReplacements, ttn.nodeData.phaseReplacements, ttn.nodeData.hasBooster, ttn.nodeData.temporary);
            ttn.toParentLineRenderer = timeTreePageInstance.drawLine(Vector2.zero, Vector2.one);
            timeTreeNodes.Add(ttn);
            return ttn;
        }

        public static void remove(TimeTreeNode timeTreeNode) {
            timeTreeNode.nodeData.timeTreeNode = null;
            timeTreeNode.nodeData = null;
            timeTreePageInstance.removeNodeIcon(timeTreeNode.nodeIcon);
            timeTreeNode.nodeIcon = null;
            timeTreePageInstance.removeLine(timeTreeNode.toParentLineRenderer);
            timeTreeNode.toParentLineRenderer = null;
            timeTreeNodes.Remove(timeTreeNode);
            recycledTTNs.Add(timeTreeNode);
        }

        public static void clearAll() {
            while (timeTreeNodes.Count > 0) {
                remove(timeTreeNodes[0]);
            }
        }

        public static void destroyAll() {
            clearAll();
            recycledTTNs.Clear();
        }

        public static TimeTreePage timeTreePageInstance = null;
        public static List<TimeTreeNode> timeTreeNodes = new List<TimeTreeNode>();


        public NodeData nodeData;
        public TimeTreeNodeIcon nodeIcon;
        public UILineRenderer toParentLineRenderer;

        public int spaceHeight = 0; // = sum of spaceHeights of children + number of children - 1, or 0 if no children
        public void setSpaceHeightFromNodeChildren() {
            spaceHeight = 0;
            if (numChildren > 0) {
                foreach (NodeData childNodeData in nodeData.children) {
                    if (childNodeData == null || childNodeData.timeTreeNode == null) continue;
                    spaceHeight += childNodeData.timeTreeNode.spaceHeight;
                }
                spaceHeight += numChildren - 1;
            }
        }

        public float x {
            get {
                if (nodeIcon == null) return 0;
                return nodeIcon.GetComponent<RectTransform>().localPosition.x;
            }
            set {
                if (nodeIcon == null) return;
                nodeIcon.GetComponent<RectTransform>().localPosition = new Vector2(value, y);
            }
        }
        public float y {
            get {
                if (nodeIcon == null) return 0;
                return nodeIcon.GetComponent<RectTransform>().localPosition.y;
            }
            set {
                if (nodeIcon == null) return;
                nodeIcon.GetComponent<RectTransform>().localPosition = new Vector2(x, value);
            }
        }
        public Vector2 pos {
            get {
                if (nodeIcon == null) return Vector2.zero;
                return new Vector2(x, y);
            }
            set {
                if (nodeIcon == null) return;
                nodeIcon.GetComponent<RectTransform>().localPosition = value;
            }
        }

        public void setXFromNodeTime() {
            if (nodeData == null) return;
            x = timeTreePageInstance.SECONDS_TO_PIXELS * nodeData.time;
        }

        public void drawToParentLine() {
            if (parent == null) {
                toParentLineRenderer.enabled = false;
            } else {
                toParentLineRenderer.Points[0] = new Vector2(x, y);
                toParentLineRenderer.Points[1] = new Vector2(parent.x, parent.y);
                toParentLineRenderer.enabled = (toParentLineRenderer.Points[0] - toParentLineRenderer.Points[1]).SqrMagnitude() >=
                    timeTreePageInstance.minPixelDistanceForVisibleLine * timeTreePageInstance.minPixelDistanceForVisibleLine;
                toParentLineRenderer.SetVerticesDirty();
            }
        }

        public TimeTreeNode parent {
            get {
                if (nodeData == null) return null;
                if (nodeData.parent == null) return null;
                return nodeData.parent.timeTreeNode;
            }
        }

        public bool temporary {
            get {
                if (nodeData == null) return false;
                return nodeData.temporary;
            }
        }

        public int numChildren {
            get {
                if (nodeData == null) return 0;
                if (nodeData.children == null) return 0;
                return nodeData.children.Count;
            }
        }
        public TimeTreeNode child(int index) {
            return nodeData.children[index].timeTreeNode;
        }

        private static List<TimeTreeNode> recycledTTNs = new List<TimeTreeNode>();
    }

    #region Creating and Clearing Nodes

    void createNodes() {
        if (Vars.currentNodeData == null) return;
        clearNodes();

        // construct nodes in post-order
        NodeData node = NodeData.getRoot(Vars.currentNodeData);
        TimeTreeNode ttn;
        TimeTreeNode theTemporaryNode = null;
        Stack<int> childIndices = new Stack<int>();
        childIndices.Push(0);
        int failsafeCount = 0;
        while (node != null && failsafeCount < 1000) {
            if (childIndices.Peek() < node.children.Count) {
                // create child first
                childIndices.Push(childIndices.Pop() + 1); // increment child index for this level
                if (node.children[childIndices.Peek() - 1] == null) {
                    Debug.Log("THIS SHOULDN'T HAPPEN"); // but it's good to be safe
                } else {
                    node = node.children[childIndices.Peek() - 1]; // go to next level
                    childIndices.Push(0); // go to next level
                }
            } else {
                // exhauseted children (or didn't have children)
                // create node
                ttn = TimeTreeNode.create(node);
                ttn.setXFromNodeTime();
                ttn.setSpaceHeightFromNodeChildren();
                if (ttn.temporary) {
                    theTemporaryNode = ttn;
                }
                // go back to parent
                node = node.parent;
                childIndices.Pop();
            }

            failsafeCount++;
        }
        if (failsafeCount >= 1000) {
            Debug.LogError("Failsafe FAILED.  Infinite loop");
        }

        // bring temporary icon to front
        if (theTemporaryNode != null) {
            theTemporaryNode.nodeIcon.transform.SetAsLastSibling();
        }

        // get y position of nodes in pre-order
        TimeTreeNode rootNode = NodeData.getRoot(Vars.currentNodeData).timeTreeNode;
        rootNode.y = 0; // root at 0
        setYPositionsHelper(rootNode);
        
        // draw lines of nodes
        foreach (TimeTreeNode ttn2 in TimeTreeNode.timeTreeNodes) {
            ttn2.drawToParentLine();
        }

        // default first selection
        if (theTemporaryNode == null) {
            if (rootNode != null) {
                selectNode(rootNode, true);
            }
        } else {
            selectNode(theTemporaryNode, true);
        }
        selectorInputEnabled = true;

    }
    void setYPositionsHelper(TimeTreeNode ttn) {
        // set y positions of children, based on current node's y position
        float currentSpaceHeight = ttn.spaceHeight / 2.0f; // go from up to down
        for (int i = 0; i < ttn.numChildren; i++) {
            ttn.child(i).y = ttn.y + (currentSpaceHeight - ttn.child(i).spaceHeight / 2.0f) * VERTICAL_SPACING;
            setYPositionsHelper(ttn.child(i)); // do the same for this child's children
            currentSpaceHeight -= ttn.child(i).spaceHeight + 1;
        }
    }

    void clearNodes() {
        TimeTreeNode.clearAll();
        clearLines();
        clearNodeIcons();
    }

    UILineRenderer drawLine(Vector2 p0, Vector2 p1) {
        GameObject lrGO;
        if (recycledLines.Count > 0) {
            lrGO = recycledLines[recycledLines.Count - 1];
            recycledLines.RemoveAt(recycledLines.Count - 1);
        } else {
            lrGO = GameObject.Instantiate(lineRendererGameObject);
        }
        lrGO.transform.SetParent(branchesContainer.transform, false);
        UILineRenderer lr = lrGO.GetComponent<UILineRenderer>();
        lr.enabled = (p0 - p1).SqrMagnitude() >= minPixelDistanceForVisibleLine*minPixelDistanceForVisibleLine;
        lr.Points[0] = p0;
        lr.Points[1] = p1;
        lr.SetVerticesDirty();
        lines.Add(lr);
        return lr;
    }

    void removeLine(UILineRenderer line) {
        if (line == null) return;
        lines.Remove(line);
        line.enabled = false;
        recycledLines.Add(line.gameObject);
    }

    // recycles lines, doesn't delete them
    void clearLines() {
        while (lines.Count > 0) {
            removeLine(lines[0]);
        }
    }

    TimeTreeNodeIcon addNodeIcon(string chars, int healthTokens, int phaseTokens, bool booster, bool temporary) {
        GameObject niGO;
        if (recycledNodeIcons.Count > 0) {
            niGO = recycledNodeIcons[recycledNodeIcons.Count - 1];
            recycledNodeIcons.RemoveAt(recycledNodeIcons.Count - 1);
        } else {
            niGO = GameObject.Instantiate(iconGameObject);
        }
        niGO.transform.SetParent(nodeContainer.transform, false);
        TimeTreeNodeIcon ni = niGO.GetComponent<TimeTreeNodeIcon>();
        ni.showing = true;
        ni.chars = chars;
        ni.setTokens(healthTokens, phaseTokens);
        ni.booster = booster;
        ni.temporary = temporary;
        nodeIcons.Add(ni);
        return ni;
    }

    void removeNodeIcon(TimeTreeNodeIcon nodeIcon) {
        if (nodeIcon == null) return;
        nodeIcons.Remove(nodeIcon);
        nodeIcon.showing = false;
        recycledNodeIcons.Add(nodeIcon.gameObject);
    }

    // recycles node icons, doesn't delete them
    void clearNodeIcons() {
        while (nodeIcons.Count > 0) {
            removeNodeIcon(nodeIcons[0]);
        }
    }

    void setIntervals() {
        // ensure correct amount
        int numIntervals = Mathf.RoundToInt(CountdownTimer.MELTDOWN_DURATION / intervalDuration) + 1;
        while (intervals.Count < numIntervals) {
            Image interv = GameObject.Instantiate(intervalGameObject).GetComponent<Image>();
            interv.transform.SetParent(intervalsContainer.transform, false);
            intervals.Add(interv);
        }
        while (intervals.Count > numIntervals) {
            Image interv = intervals[intervals.Count-1];
            intervals.RemoveAt(intervals.Count - 1);
            GameObject.Destroy(interv.gameObject);
        }
        // positioning
        for (int i=0; i<numIntervals; i++) {
            GameObject intervGO = intervals[i].gameObject;
            intervGO.GetComponent<RectTransform>().localPosition = new Vector2(
                0 + SECONDS_TO_PIXELS * intervalDuration * i,
                -nodeContainer.GetComponent<RectTransform>().localPosition.y);
        }
    }
    void showIntervals() {
        foreach (Image interv in intervals) {
            interv.enabled = true;
        }
    }
    void hideIntervals() {
        foreach (Image interv in intervals) {
            interv.enabled = false;
        }
    }


    #endregion

    #region Called from Pause Screen

    public void update() {

        /*
        if (Keys.instance.confirmPressed) {
            madeSelection = true;
            option = options[selectionIndex];
        }
        if (Keys.instance.backPressed) {
            if (quitSureNoText.hasVisibleChars) {
                madeSelection = true;
                option = quitSureNoText;
            }
        }
        */

        if (selectorInputEnabled) {
            TimeTreeNode nextTTN = null;
            if (Keys.instance.upPressed) {
                nextTTN = getNextTimeTreeNode(mainSelectedNode, Direction.UP);
            } else if (Keys.instance.rightPressed) {
                nextTTN = getNextTimeTreeNode(mainSelectedNode, Direction.RIGHT);
            } else if (Keys.instance.downPressed) {
                nextTTN = getNextTimeTreeNode(mainSelectedNode, Direction.DOWN);
            } else if (Keys.instance.leftPressed) {
                nextTTN = getNextTimeTreeNode(mainSelectedNode, Direction.LEFT);
            }
            if (nextTTN != null) {
                selectNode(nextTTN, false);
                SoundManager.instance.playSFXIgnoreVolumeScale(nodeSwitchSound);
            }
        }

    }

    public void show() {
        nodeBG.enabled = true;
        glyphBox.makeAllCharsVisible();
        descripBox.makeAllCharsVisible();
        if (mainSelector != null) {
            mainSelector.GetComponent<Image>().enabled = true;
        }
        setIntervals();
        showIntervals();

        createNodes();
    }

    public void hide() {
        nodeBG.enabled = false;
        glyphBox.makeAllCharsInvisible();
        descripBox.makeAllCharsInvisible();
        if (mainSelector != null) {
            mainSelector.GetComponent<Image>().enabled = false;
        }
        hideIntervals();

        clearNodes();
    }

    #endregion


    void selectNode(TimeTreeNode ttn, bool immediately) {
        if (mainSelector == null) {
            // make main selector
            mainSelector = GameObject.Instantiate(selectorGameObject);
            mainSelector.transform.SetParent(nodeContainer.transform, false);
        }

        mainPrevSelectedNode = mainSelectedNode;
        mainSelectedNode = ttn;
        mainSelector.GetComponent<RectTransform>().localPosition = ttn.nodeIcon.GetComponent<RectTransform>().localPosition;
        mainSelector.transform.SetAsLastSibling();

        setDescription(mainSelectedNode);

    }

    void setDescription(TimeTreeNode ttn) {
        // position code
        string str = "";
        //if (ttn.temporary) {
        //    str += "--";
        //} else {
        //    str += ttn.nodeData.chamberPositionCode;
        //}
        // time
        string timeStr = "";
        if (CountdownTimer.instance != null && CountdownTimer.instance.mode != CountdownTimer.Mode.NORMAL) {
            timeStr = CountdownTimer.timeToStr(ttn.nodeData.time, CountdownTimer.Mode.MELTDOWN);
        } else {
            timeStr = CountdownTimer.timeToStr(ttn.nodeData.time, CountdownTimer.Mode.NORMAL);
        }
        timeStr = timeStr.Replace("¦", "").Trim();
        str += " " + timeStr;
        // hearts and phase upgrades
        str += " ";
        int numHearts = startingHealthUpgradeCount + ttn.nodeData.healthUpgrades.Count;
        for (int i=0; i<numHearts; i++) {
            str += "¼";
        }
        int numPhase = ttn.nodeData.phaseReplacements;
        for (int i = 0; i < numPhase; i++) {
            str += "½";
        }
        // booster
        if (ttn.nodeData.hasBooster) {
            str += " ¾";
        }

        descripBox.setPlainText(str);
    }

    GameObject mainSelector = null;
    TimeTreeNode mainSelectedNode = null;
    TimeTreeNode mainPrevSelectedNode = null;

    enum Direction {
        LEFT, UP, RIGHT, DOWN
    }

    TimeTreeNode getNextTimeTreeNode(TimeTreeNode fromNode, Direction direction) {
        // get all nodes that can be accessed
        List<TimeTreeNode> ttns = new List<TimeTreeNode>();
        foreach (TimeTreeNode ttn in TimeTreeNode.timeTreeNodes) {
            if (ttn == fromNode) continue;

            ttns.Add(ttn);
        }
        if (ttns.Count == 0)
            return null;

        Vector2 startPos = fromNode.pos;
        TimeTreeNode ret = null;
        float dist = 99999;
        float d;

        switch (direction) {
        case Direction.LEFT:
            foreach (TimeTreeNode ttn in ttns) {
                if (Utilities.pointInSector(ttn.pos, startPos, 99999, Mathf.PI, Mathf.PI / 2)) {
                    d = startPos.x - ttn.x;
                    if (d < dist) {
                        dist = d;
                        ret = ttn;
                    }
                }
            }
            break;
        case Direction.DOWN:
            foreach (TimeTreeNode ttn in ttns) {
                if (Utilities.pointInSector(ttn.pos, startPos, 99999, -Mathf.PI / 2, Mathf.PI / 2)) {
                    d = startPos.y - ttn.y;
                    if (d < dist) {
                        dist = d;
                        ret = ttn;
                    }
                }
            }
            break;
        case Direction.RIGHT:
            foreach (TimeTreeNode ttn in ttns) {
                if (Utilities.pointInSector(ttn.pos, startPos, 99999, 0, Mathf.PI / 2)) {
                    d = ttn.x - startPos.x;
                    if (d < dist) {
                        dist = d;
                        ret = ttn;
                    }
                }
            }
            break;
        case Direction.UP:
            foreach (TimeTreeNode ttn in ttns) {
                if (Utilities.pointInSector(ttn.pos, startPos, 99999, Mathf.PI / 2, Mathf.PI / 2)) {
                    d = ttn.y - startPos.y;
                    if (d < dist) {
                        dist = d;
                        ret = ttn;
                    }
                }
            }
            break;
        }

        return ret;
    }


    #region Event Functions

    void Awake() {
        nodeBG = transform.Find("NodeBG").GetComponent<Image>();
        glyphBox = transform.Find("GlyphBox").GetComponent<GlyphBox>();
        descripBox = transform.Find("DescripBox").GetComponent<GlyphBox>();
        propAsset = new Properties(textAsset.text);
        nodeMask = transform.Find("NodeMask").gameObject;
        nodeContainer = nodeMask.transform.Find("NodeContainer").gameObject;
        branchesContainer = nodeContainer.transform.Find("Branches").gameObject;
        intervalsContainer = nodeContainer.transform.Find("Intervals").gameObject;

        TimeTreeNode.timeTreePageInstance = this;
    }

    void Start() {
        hide();
    }

    void Update() { }
    
    void OnDestroy() {
        // remove all lines and node icons for good
        clearLines();
        foreach (GameObject lrGO in recycledLines) {
            GameObject.Destroy(lrGO);
        }
        recycledLines.Clear();
        clearNodeIcons();
        foreach (GameObject niGO in recycledNodeIcons) {
            GameObject.Destroy(niGO);
        }
        recycledNodeIcons.Clear();
        foreach (Image interv in intervals) {
            GameObject.Destroy(interv.gameObject);
        }
        intervals.Clear();
    }

    #endregion

    

    Properties propAsset;
    Image nodeBG;
    GlyphBox glyphBox; // ?? why is this here??
    GlyphBox descripBox;
    GameObject nodeMask;
    GameObject nodeContainer;
    GameObject branchesContainer;
    GameObject intervalsContainer;

    List<UILineRenderer> lines = new List<UILineRenderer>();
    List<TimeTreeNodeIcon> nodeIcons = new List<TimeTreeNodeIcon>();

    List<GameObject> recycledLines = new List<GameObject>();
    List<GameObject> recycledNodeIcons = new List<GameObject>();

    List<Image> intervals = new List<Image>();

    bool selectorInputEnabled = true;

}
