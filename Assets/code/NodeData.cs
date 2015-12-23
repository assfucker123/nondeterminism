using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* Time-dependent save data.  */
public class NodeData {

    public NodeData(int id) {
        this.id = id;
    }

    public float time = 0;
    public string level = "";
    public Vector2 position = new Vector2();
    public List<PhysicalUpgrade.Orb> orbs = new List<PhysicalUpgrade.Orb>();
    public bool hasBooster = false;
    public List<PhysicalUpgrade.HealthUpgrade> healthUpgrades = new List<PhysicalUpgrade.HealthUpgrade>();
    public int phaseReplacements = 0; // number of health upgrades replaced with phase upgrades
    public List<AdventureEvent.Physical> physicalEvents = new List<AdventureEvent.Physical>();
    public void eventHappen(AdventureEvent.Physical eventID) {
        if (eventHappened(eventID)) return;
        physicalEvents.Add(eventID);
    }
    public bool eventHappened(AdventureEvent.Physical eventID) {
        return physicalEvents.IndexOf(eventID) != -1;
    }
    
    public int id = 1; // used to identify this NodeData.  Must be a positive integer
    public NodeData parent = null;
    public List<NodeData> children = new List<NodeData>();

    /* Copies info from the given nodeData to this (except for ID) */
    public void copyFrom(NodeData nodeData) {
        time = nodeData.time;
        level = nodeData.level;
        position = nodeData.position;
        orbs.Clear();
        orbs.AddRange(nodeData.orbs);
        hasBooster = nodeData.hasBooster;
        healthUpgrades.Clear();
        healthUpgrades.AddRange(nodeData.healthUpgrades);
        phaseReplacements = nodeData.phaseReplacements;
        physicalEvents.Clear();
        physicalEvents.AddRange(nodeData.physicalEvents);
        parent = nodeData.parent;
        children.Clear();
        children.AddRange(nodeData.children);
    }

    public void loadFromString(string str) {
        char[] delims = {'>'};
        char[] delims2 = {'?'};
        string[] strs = str.Split(delims);

        // id
        loadIDFromString(str);
        // level
        level = strs[1];
        // position
        position.Set(float.Parse(strs[2]), float.Parse(strs[3]));
        // time
        time = float.Parse(strs[4]);
        // orbs
        orbs.Clear();
        string[] orbStrs = strs[5].Split(delims2);
        for (int i=0; i<orbStrs.Length; i++) {
            if (orbStrs[i] == "") continue;
            orbs.Add((PhysicalUpgrade.Orb)int.Parse(orbStrs[i]));
        }
        // booster
        hasBooster = (strs[6] == "1");
        // health upgrades
        healthUpgrades.Clear();
        string[] healthStrs = strs[7].Split(delims2);
        for (int i = 0; i < healthStrs.Length; i++) {
            if (healthStrs[i] == "")continue;
            healthUpgrades.Add((PhysicalUpgrade.HealthUpgrade)int.Parse(healthStrs[i]));
        }
        // phase replacements
        phaseReplacements = int.Parse(strs[8]);
        // parent
        int parentID = int.Parse(strs[9]);
        parent = nodeDataFromID(parentID);
        // children
        children.Clear();
        string[] childrenStrs = strs[10].Split(delims2);
        for (int i=0; i<childrenStrs.Length; i++) {
            if (childrenStrs[i] == "") continue;
            children.Add(nodeDataFromID(int.Parse(childrenStrs[i])));
        }
        // physical events
        string[] physStrs = strs[11].Split(delims2);
        for (int i = 0; i < physStrs.Length; i++) {
            if (physStrs[i] == "") continue;
            physicalEvents.Add((AdventureEvent.Physical)int.Parse(physStrs[i]));
        }

    }

    public string saveToString() {
        string ret = "";
        // id (0)
        ret += id + ">";
        // level (1)
        ret += level + ">";
        // position (2, 3)
        ret += position.x + ">" + position.y + ">";
        // time (4)
        ret += time + ">";
        // orbs (5)
        for (int i=0; i<orbs.Count; i++) {
            ret += (int)orbs[i];
            if (i < orbs.Count - 1) ret += "?";
        }
        ret += ">";
        // booster (6)
        if (hasBooster) ret += "1>";
        else ret += "0>";
        // health upgrades (7)
        for (int i = 0; i < healthUpgrades.Count; i++) {
            ret += (int)healthUpgrades[i];
            if (i < healthUpgrades.Count - 1) ret += "?";
        }
        ret += ">";
        // phase replacements (8)
        ret += phaseReplacements + ">";
        // parent (9)
        if (parent == null) ret += "0>";
        else ret += parent.id + ">";
        // children (10)
        for (int i = 0; i < children.Count; i++) {
            ret += children[i].id;
            if (i < children.Count - 1) ret += "?";
        }
        ret += ">";
        // physical events (11)
        for (int i = 0; i < physicalEvents.Count; i++) {
            ret += (int)physicalEvents[i];
            if (i < physicalEvents.Count - 1) ret += "?";
        }

        return ret;

    }

    public void loadIDFromString(string str) {
        char[] delims = {'>'};
        string[] strs = str.Split(delims);

        // id
        id = int.Parse(strs[0]);
    }
    
    ////////////
    // STATIC //
    ////////////

    public static List<NodeData> allNodes = new List<NodeData>();

    /* Creates a new NodeData, and automatically adds it to allNodes.
     * If parent is given, then new NodeData will have the parent's properties,
     * and parent/children data of both will be set accordingly */
    public static NodeData createNodeData(NodeData parent = null) {
        // pick random ID that does not match any ID in allNodes
        int id = 1;
        bool found = false;
        while (!found) {
            id = Random.Range(1, 999999);
            found = true;
            foreach (NodeData nd in allNodes) {
                if (nd.id == id) {
                    found = false;
                    break;
                }
            }
        }
        NodeData nodeData = new NodeData(id);
        allNodes.Add(nodeData);
        if (parent != null) {
            nodeData.copyFrom(parent);
            nodeData.parent = parent;
            nodeData.children.Clear();
            parent.children.Add(nodeData);
        }
        return nodeData;
    }

    /* finds children of all levels of the given nodeData */
    public static List<NodeData> getAllChildren(NodeData nodeData) {
        List<NodeData> ret = new List<NodeData>();
        foreach (NodeData nd in nodeData.children) {
            ret.Add(nd);
            ret.AddRange(getAllChildren(nd));
        }
        return ret;
    }

    /* finds the root of the nodes, starting with the given nodeData */
    public static NodeData getRoot(NodeData nodeData) {
        NodeData ret = nodeData;
        while (ret.parent != null) {
            ret = ret.parent;
        }
        return ret;
    }

    /* deletes a node, which also deletes all of its children.
     * returns the parent of the deleted nodeData. */
    public static NodeData deleteNode(NodeData nodeData) {
        NodeData parent = nodeData.parent;
        nodeData.parent = null; // removes reference to parent
        parent.children.Remove(nodeData); // removes child reference from parent
        while (nodeData.children.Count > 0) {
            deleteNode(nodeData.children[0]); // deleting all child nodes
        }
        allNodes.Remove(nodeData);
        return parent;
    }
    
    /* searched allNodes for NodeData with the matching id */
    public static NodeData nodeDataFromID(int id) {
        foreach (NodeData nd in allNodes) {
            if (nd.id == id)
                return nd;
        }
        return null;
    }

    public static void clearAllNodes() {
        allNodes.Clear();
    }

    public static void loadAllNodesFromString(string str) {
        clearAllNodes();
        char[] delims = {'<'};
        string[] nodeStrs = str.Split(delims);
        for (int i=0; i<nodeStrs.Length; i++) {
            if (nodeStrs[i] == "") continue;
            NodeData nd = new NodeData(1);
            nd.loadIDFromString(nodeStrs[i]);
            allNodes.Add(nd);
        }
        for (int i = 0; i < nodeStrs.Length; i++) {
            allNodes[i].loadFromString(nodeStrs[i]);
        }
    }

    public static string saveAllNodesToString() {
        string ret = "";
        for (int i=0; i<allNodes.Count; i++) {
            ret += allNodes[i].saveToString();
            if (i < allNodes.Count - 1) {
                ret += "<";
            }
        }
        return ret;
    }
    

}
