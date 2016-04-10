using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Extensions;

public class TimeTreePage : MonoBehaviour {

    #region Inspector Properties
    
    public float VERTICAL_SPACING = 50;
    public float minPixelDistanceForVisibleLine = 20;
    public float intervalDuration = 5*60;
    public Color descripBoxFlashbackColor = new Color();
    public float nodeContainerScrollDuration = .2f;
    public float nodeContainerPadding = 25;
    public Color lowPhaseColor = new Color(255/255f, 190/255f, 0/255f);
    public GameObject iconGameObject;
    public GameObject selectorGameObject;
    public GameObject lineRendererGameObject;
    public GameObject intervalGameObject;
    public GameObject popupGameObject;
    public AudioClip nodeSwitchSound;
    public TextAsset textAsset;

    #endregion

    #region Other Public Properties

    public float SECONDS_TO_PIXELS {
        get {
            return 2500 / CountdownTimer.MELTDOWN_DURATION;
            //return 7500 / CountdownTimer.MELTDOWN_DURATION;
        }
    }
    public int startingHealthUpgradeCount {
        get {
            return 2;
        }
    }
    public float nodeDisplayWidth { get { return nodeMask.GetComponent<RectTransform>().sizeDelta.x; } }
    public Vector2 nodeContainerCenterOffset { get { return new Vector2(-nodeDisplayWidth / 2 + nodeContainerPadding, 0); } }
    public float nodeContainerScrollXMin {
        get {
            return -nodeContainerCenterOffset.x - SECONDS_TO_PIXELS * CountdownTimer.MELTDOWN_DURATION;
        }
    }
    public float nodeContainerScrollXMax {
        get {
            return nodeContainerCenterOffset.x;
        }
    }
    public bool playerCanChamberFlashback {
        get {
            if (Player.instance == null) return false;
            return Player.instance.phase >= Player.instance.visionsPhase;
        }
    }

    #endregion

    // chamber flashback
    public void startChamberFlashback(NodeData nodeData) {
        if (nodeData == null) {
            Debug.LogError("ERROR: null nodeData");
            return;
        }
        if (Player.instance == null) {
            Debug.LogError("ERROR: player null");
        }
        // resume game
        PauseScreen.instance.unpauseGame();

        // get player to start chamber flashback cutscene
        Player.instance.chamberFlashback(nodeData);
    }

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
                timeTreePageInstance.setLineRenderer(toParentLineRenderer, new Vector2(x, y), new Vector2(parent.x, parent.y));
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
        setLineRenderer(lr, p0, p1);
        lines.Add(lr);
        return lr;
    }

    void setLineRenderer(UILineRenderer lineRenderer, Vector2 p0, Vector2 p1) {
        Vector2 diff = p1 - p0;
        lineRenderer.enabled = diff.SqrMagnitude() >= minPixelDistanceForVisibleLine * minPixelDistanceForVisibleLine;
        Rect bounds = new Rect(Mathf.Min(p0.x, p1.x), Mathf.Min(p0.y, p1.y), Mathf.Abs(diff.x), Mathf.Abs(diff.y));
        RectTransform rt = lineRenderer.GetComponent<RectTransform>();
        rt.localPosition = bounds.min;
        rt.sizeDelta = bounds.size;

        //float twoSegmentAngle = Mathf.Atan2(VERTICAL_SPACING, Mathf.Abs(diff.x));
        float twoSegmentAngle = Mathf.Atan2(Mathf.Abs(diff.y), VERTICAL_SPACING);
        float diffAngle = Mathf.Atan2(Mathf.Abs(diff.y), Mathf.Abs(diff.x));
        bool oneSegment = (diffAngle > twoSegmentAngle) || Mathf.Abs(diff.y) < .1f;
        if (!oneSegment) {
            // draw two segments
            Vector2 centerPoint = new Vector2();
            float seg1XDiff = Mathf.Abs(diff.y / Mathf.Tan(twoSegmentAngle));

            if (p0.x < p1.x) {
                centerPoint.x = p0.x + seg1XDiff;
                centerPoint.y = p1.y;
                if (Mathf.Abs(p1.x - centerPoint.x) < minPixelDistanceForVisibleLine) {
                    oneSegment = true;
                }
            } else {
                centerPoint.x = p1.x + seg1XDiff;
                centerPoint.y = p0.y;
                if (Mathf.Abs(p0.x - centerPoint.x) < minPixelDistanceForVisibleLine) {
                    oneSegment = true;
                }
            }
            
        }
        
        if (oneSegment/*diffAngle > twoSegmentAngle*/) {
            // draw segment as normal
            lineRenderer.Points[0] = p0 - bounds.min;
            lineRenderer.Points[1] = p1 - bounds.min;
            lineRenderer.Points[2] = p1 - bounds.min;
        } else {
            // draw two segments
            Vector2 centerPoint = new Vector2();
            float seg1XDiff = Mathf.Abs(diff.y / Mathf.Tan(twoSegmentAngle));
            
            if (p0.x < p1.x) {
                centerPoint.x = p0.x + seg1XDiff;
                centerPoint.y = p1.y;
            } else {
                centerPoint.x = p1.x + seg1XDiff;
                centerPoint.y = p0.y;
            }
            
            lineRenderer.Points[0] = p0 - bounds.min;
            lineRenderer.Points[1] = centerPoint - bounds.min;
            lineRenderer.Points[2] = p1 - bounds.min;
        }

        //lineRenderer.Points[0] = p0;
        //lineRenderer.Points[1] = p1;
        
        
        lineRenderer.SetVerticesDirty();
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
            TimeTreeInterval interv = GameObject.Instantiate(intervalGameObject).GetComponent<TimeTreeInterval>();
            interv.transform.SetParent(intervalsContainer.transform, false);
            intervals.Add(interv);
        }
        while (intervals.Count > numIntervals) {
            TimeTreeInterval interv = intervals[intervals.Count-1];
            intervals.RemoveAt(intervals.Count - 1);
            GameObject.Destroy(interv.gameObject);
        }
        // positioning and text
        for (int i=0; i<numIntervals; i++) {
            GameObject intervGO = intervals[i].gameObject;
            intervGO.GetComponent<RectTransform>().localPosition = new Vector2(
                0 + SECONDS_TO_PIXELS * intervalDuration * i,
                -nodeContainer.GetComponent<RectTransform>().localPosition.y);
            intervals[i].setTime(i * intervalDuration);
        }

    }
    void showIntervals() {
        foreach (TimeTreeInterval interv in intervals) {
            interv.show();
        }
    }
    void hideIntervals() {
        foreach (TimeTreeInterval interv in intervals) {
            interv.hide();
        }
    }


    #endregion

    #region Called from Pause Screen

    public void update() {
        
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

            if (Keys.instance.confirmPressed) {
                // trigger chamber flashback popup (if applicable)
                if (!mainSelectedNode.temporary && playerCanChamberFlashback && Player.instance != null) {
                    createPopup();
                    popup.show(
                        propAsset.getString("flashback_msg") + " " + mainSelectedNode.nodeData.chamberPositionCode + " " +
                        propAsset.getString("msg_at") + " " + getTimeStr(mainSelectedNode.nodeData.time) + propAsset.getString("msg_q"),
                        propAsset.getString("yes"), propAsset.getString("no"), descripBox.defaultStyle.color, true);
                    selectorInputEnabled = false;
                }
                
            }
        } else {

            // popup update (if running)
            if (popup != null && popup.visible) {
                if (popup.state == TimeTreePopup.State.YES_PRESSED) {
                    // do something
                    popup.hide();
                    selectorInputEnabled = true;
                    startChamberFlashback(mainSelectedNode.nodeData);
                } else if (popup.state == TimeTreePopup.State.NO_PRESSED) {
                    // back out of popup
                    popup.hide();
                    selectorInputEnabled = true;
                }
            }
        }

        // scrolling
        if (scrollTime < nodeContainerScrollDuration) {
            scrollTime += Time.unscaledDeltaTime;
            Vector2 center0;
            Vector2 center1 = mainSelectedNode.pos;
            //if (mainPrevSelectedNode == null) {
            //    center0 = center1;
            //} else {
            //    center0 = mainPrevSelectedNode.pos;
            //}
            center0 = prevPosition;
            // clamping x vals beforehand to try to make scrolling smoother
            center0.x = Mathf.Clamp(center0.x, -nodeContainerScrollXMax, -nodeContainerScrollXMin);
            center1.x = Mathf.Clamp(center1.x, -nodeContainerScrollXMax, -nodeContainerScrollXMin);
            center0.x = Mathf.Round(center0.x/2)*2; center0.y = Mathf.Round(center0.y/2)*2;
            center1.x = Mathf.Round(center1.x/2)*2; center1.y = Mathf.Round(center1.y/2)*2;

            Vector2 center = Utilities.easeOutQuadClamp(scrollTime, center0, center1-center0, nodeContainerScrollDuration);
            Vector2 scrollPos = -center;
            scrollPos.x = Mathf.Clamp(scrollPos.x, nodeContainerScrollXMin, nodeContainerScrollXMax);
            nodeContainer.GetComponent<RectTransform>().localPosition = scrollPos;

            // adjust y positions of intervals
            for (int i = 0; i < intervals.Count; i++) {
                GameObject intervGO = intervals[i].gameObject;
                intervGO.GetComponent<RectTransform>().localPosition = new Vector2(
                    0 + SECONDS_TO_PIXELS * intervalDuration * i,
                    -nodeContainer.GetComponent<RectTransform>().localPosition.y);
            }

            // scrolling map
            Vector2 gridPos0 = new Vector2();
            Vector2 gridPos1 = MapUI.instance.gridPositionFromWorldPosition(mainSelectedNode.nodeData.levelMapX, mainSelectedNode.nodeData.levelMapY, mainSelectedNode.nodeData.position);
            // more accurate grid position if node is in the current level
            if (mainSelectedNode.nodeData.levelMapX == Level.currentLoadedLevel.mapX && mainSelectedNode.nodeData.levelMapY == Level.currentLoadedLevel.mapY) {
                gridPos1.x = Mathf.Clamp(gridPos1.x, Level.currentLoadedLevel.mapX, Level.currentLoadedLevel.mapX + Level.currentLoadedLevel.mapWidth - 1);
                gridPos1.y = Mathf.Clamp(gridPos1.y, Level.currentLoadedLevel.mapY, Level.currentLoadedLevel.mapY + Level.currentLoadedLevel.mapHeight - 1);
            }
            if (mainPrevSelectedNode == null) {
                gridPos0 = gridPos1;
            } else {
                gridPos0 = MapUI.instance.gridPositionFromWorldPosition(mainPrevSelectedNode.nodeData.levelMapX, mainPrevSelectedNode.nodeData.levelMapY, mainPrevSelectedNode.nodeData.position);
                // more accurate grid position if node is in the current level
                if (mainPrevSelectedNode.nodeData.levelMapX == Level.currentLoadedLevel.mapX && mainPrevSelectedNode.nodeData.levelMapY == Level.currentLoadedLevel.mapY) {
                    gridPos0.x = Mathf.Clamp(gridPos0.x, Level.currentLoadedLevel.mapX, Level.currentLoadedLevel.mapX + Level.currentLoadedLevel.mapWidth - 1);
                    gridPos0.y = Mathf.Clamp(gridPos0.y, Level.currentLoadedLevel.mapY, Level.currentLoadedLevel.mapY + Level.currentLoadedLevel.mapHeight - 1);
                }
            }
            Vector2 gridPos = Utilities.easeOutQuadClamp(scrollTime, gridPos0, gridPos1-gridPos0, nodeContainerScrollDuration);
            MapUI.instance.setMapCenter(gridPos.x, gridPos.y);
        }

    }

    public void show() {
        nodeBG.enabled = true;
        mapBG.enabled = true;
        instrBox.makeAllCharsVisible();
        descripBox.makeAllCharsVisible();
        if (mainSelector != null) {
            mainSelector.GetComponent<Image>().enabled = true;
        }
        setIntervals();
        showIntervals();

        showMapUI();

        createNodes();


    }

    public void hide() {
        nodeBG.enabled = false;
        mapBG.enabled = false;
        instrBox.makeAllCharsInvisible();
        descripBox.makeAllCharsInvisible();
        if (mainSelector != null) {
            mainSelector.GetComponent<Image>().enabled = false;
        }
        hideIntervals();
        if (popup != null) {
            popup.hide();
        }

        clearNodes();

        hideMapUI();
    }

    #endregion

    void showMapUI() {

        MapUI.instance.showMap(true);
        MapUI.instance.inputEnabled = false;
        MapUI.instance.setTimeTreePagePosition(
            new Vector2(mapBG.GetComponent<RectTransform>().localPosition.x, mapBG.GetComponent<RectTransform>().localPosition.y),
            mapBG.GetComponent<RectTransform>().sizeDelta);

    }

    void hideMapUI() {
        if (MapUI.instance != null) {
            MapUI.instance.hideMap();
        }
        
    }

    // does nothing if popup was already made
    void createPopup() {
        if (popup != null) {
            return;
        }
        GameObject popupGO = GameObject.Instantiate(popupGameObject);
        popupGO.transform.SetParent(transform.parent.parent, false);
        popup = popupGO.GetComponent<TimeTreePopup>();
    }

    void selectNode(TimeTreeNode ttn, bool immediately) {
        if (mainSelector == null) {
            // make main selector
            mainSelector = GameObject.Instantiate(selectorGameObject);
            mainSelector.transform.SetParent(nodeContainer.transform, false);
        }

        mainPrevSelectedNode = mainSelectedNode;
        mainSelectedNode = ttn;

        prevPosition = mainSelector.GetComponent<RectTransform>().localPosition;
        mainSelector.GetComponent<RectTransform>().localPosition = ttn.nodeIcon.GetComponent<RectTransform>().localPosition;
        mainSelector.transform.SetAsLastSibling();

        setDescription(mainSelectedNode);

        // setting instr text
        if (ttn.temporary) {
            //instrBox.makeAllCharsInvisible();
            instrBox.setPlainText(propAsset.getString("current_position"));
            instrBox.setColor(PauseScreen.DEFAULT_COLOR);
        } else {
            if (playerCanChamberFlashback) {
                instrBox.setPlainText(propAsset.getString("flashback_instr") + " " + getTimeStr(mainSelectedNode.nodeData.time) +
                    " (" + mainSelectedNode.nodeData.chamberPositionCode + ")");
                instrBox.setColor(descripBox.defaultStyle.color);
            } else {
                instrBox.setPlainText(propAsset.getString("low_phase_instr"));
                instrBox.setColor(lowPhaseColor);
            }
        }

        // scrolling
        if (immediately) {
            scrollTime = nodeContainerScrollDuration - .0001f;
        } else {
            scrollTime = 0;
        }
        
        Vector2 gridPos = MapUI.instance.gridPositionFromWorldPosition(mainSelectedNode.nodeData.levelMapX, mainSelectedNode.nodeData.levelMapY, mainSelectedNode.nodeData.position);
        // more accurate grid position if node is in the current level
        if (mainSelectedNode.nodeData.levelMapX == Level.currentLoadedLevel.mapX && mainSelectedNode.nodeData.levelMapY == Level.currentLoadedLevel.mapY) {
            gridPos.x = Mathf.Clamp(gridPos.x, Level.currentLoadedLevel.mapX, Level.currentLoadedLevel.mapX + Level.currentLoadedLevel.mapWidth - 1);
            gridPos.y = Mathf.Clamp(gridPos.y, Level.currentLoadedLevel.mapY, Level.currentLoadedLevel.mapY + Level.currentLoadedLevel.mapHeight - 1);
        }
        MapUI.instance.setSlectorGreenPosition(Mathf.RoundToInt(gridPos.x), Mathf.RoundToInt(gridPos.y));

    }

    string getTimeStr(float time) {
        string timeStr = "";
        if (CountdownTimer.instance != null && CountdownTimer.instance.mode != CountdownTimer.Mode.NORMAL) {
            timeStr = CountdownTimer.timeToStr(time, CountdownTimer.Mode.MELTDOWN);
        } else {
            timeStr = CountdownTimer.timeToStr(time, CountdownTimer.Mode.NORMAL);
        }
        timeStr = timeStr.Replace("¦", "").Trim();
        return timeStr;
    }

    void setDescription(TimeTreeNode ttn) {
        string str = "";
        
        // time
        str += " " + getTimeStr(ttn.nodeData.time);

        // position code
        str += " ";
        if (ttn.temporary) {
            str += "--";
        } else {
            str += ttn.nodeData.chamberPositionCode;
        }

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

        // set text
        descripBox.setPlainText(str);

        // set color
        if (ttn.temporary) {
            descripBox.setColor(PauseScreen.DEFAULT_COLOR);
        } else {
            descripBox.setColor(descripBoxFlashbackColor);
        }
    }

    GameObject mainSelector = null;
    TimeTreeNode mainSelectedNode = null;
    TimeTreeNode mainPrevSelectedNode = null;
    Vector3 prevPosition = new Vector2();

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
        mapBG = transform.Find("MapBG").GetComponent<Image>();
        instrBox = transform.Find("InstrBox").GetComponent<GlyphBox>();
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
        foreach (TimeTreeInterval interv in intervals) {
            GameObject.Destroy(interv.gameObject);
        }
        intervals.Clear();
        if (popup != null) {
            GameObject.Destroy(popup.gameObject);
            popup = null;
        }
    }

    #endregion

    

    Properties propAsset;
    Image nodeBG;
    Image mapBG;
    GlyphBox instrBox;
    GlyphBox descripBox;
    GameObject nodeMask;
    GameObject nodeContainer;
    GameObject branchesContainer;
    GameObject intervalsContainer;

    TimeTreePopup popup = null;

    List<UILineRenderer> lines = new List<UILineRenderer>();
    List<TimeTreeNodeIcon> nodeIcons = new List<TimeTreeNodeIcon>();

    List<GameObject> recycledLines = new List<GameObject>();
    List<GameObject> recycledNodeIcons = new List<GameObject>();

    List<TimeTreeInterval> intervals = new List<TimeTreeInterval>();

    float scrollTime = 9999;

    bool selectorInputEnabled = true;

}
