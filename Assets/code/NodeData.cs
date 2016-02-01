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
    public int levelMapX = 0;
    public int levelMapY = 0;
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
    public void eventUndo(AdventureEvent.Physical eventID) {
        if (eventID == AdventureEvent.Physical.NONE)
            return;
        physicalEvents.Remove(eventID);
    }
    public bool eventHappened(AdventureEvent.Physical eventID) {
        return physicalEvents.IndexOf(eventID) != -1;
    }
    public List<string> levelsAmbushesDefeated = new List<string>(); // rooms whose ambushes have been defeated
    public void defeatAmbush(string levelName) {
        if (ambushDefeated(levelName)) return;
        levelsAmbushesDefeated.Add(levelName);
    }
    public void defeatAmbushUndo(string levelName) {
        levelsAmbushesDefeated.Remove(levelName);
    }
    public bool ambushDefeated(string levelName) {
        return levelsAmbushesDefeated.IndexOf(levelName) != -1;
    }
    public List<string> damageBarriersDestroyed = new List<string>();
    public void destroyDamageBarrier(string levelName, string objectName) {
        if (damageBarrierDestroyed(levelName, objectName)) return;
        damageBarriersDestroyed.Add(damageBarrierCode(levelName, objectName));
    }
    public void destroyDamageBarrierUndo(string levelName, string objectName) {
        damageBarriersDestroyed.Remove(damageBarrierCode(levelName, objectName));
    }
    public bool damageBarrierDestroyed(string levelName, string objectName) {
        return (damageBarriersDestroyed.IndexOf(damageBarrierCode(levelName, objectName)) != -1);
    }
    public List<int> creatureCards = new List<int>();
    public bool creatureCardCollected(string creatureName) {
        return creatureCardCollected(CreatureCard.getIDFromCardName(creatureName));
    }
    public bool creatureCardCollected(int creatureID) {
        return creatureCards.IndexOf(creatureID) != -1;
    }
    public void creatureCardCollect(string creatureName) {
        creatureCardCollect(CreatureCard.getIDFromCardName(creatureName));
    }
    public void creatureCardCollect(int creatureID) {
        if (creatureCardCollected(creatureID)) return;
        creatureCards.Add(creatureID);
    }
    
    public int id = 1; // used to identify this NodeData.  Must be a positive integer
    public NodeData parent = null;
    public List<NodeData> children = new List<NodeData>();
    public bool temporary = false; // temporary nodes are not saved

    /* Copies info from the given nodeData to this (except for ID) */
    public void copyFrom(NodeData nodeData) {
        if (nodeData == null)
            return;
        time = nodeData.time;
        level = nodeData.level;
        levelMapX = nodeData.levelMapX;
        levelMapY = nodeData.levelMapY;
        position = nodeData.position;
        orbs.Clear();
        orbs.AddRange(nodeData.orbs);
        hasBooster = nodeData.hasBooster;
        healthUpgrades.Clear();
        healthUpgrades.AddRange(nodeData.healthUpgrades);
        phaseReplacements = nodeData.phaseReplacements;
        physicalEvents.Clear();
        physicalEvents.AddRange(nodeData.physicalEvents);
        levelsAmbushesDefeated.Clear();
        levelsAmbushesDefeated.AddRange(nodeData.levelsAmbushesDefeated);
        damageBarriersDestroyed.Clear();
        damageBarriersDestroyed.AddRange(nodeData.damageBarriersDestroyed);
        parent = nodeData.parent;
        children.Clear();
        children.AddRange(nodeData.children);
        temporary = nodeData.temporary;
    }

    /* Gets if this and the given nodeData are exactly the same,
     * except for id, time, and position.  Doesn't check for children but it should. */
    public bool redundant(NodeData nodeData) {
        if (level != nodeData.level) return false;
        // this is a really inefficient implementation
        foreach (PhysicalUpgrade.Orb orb in orbs) {
            if (nodeData.orbs.IndexOf(orb) == -1) return false;
        }
        foreach (PhysicalUpgrade.Orb orb in nodeData.orbs) {
            if (orbs.IndexOf(orb) == -1) return false;
        }
        // compare booster
        if (hasBooster != nodeData.hasBooster) return false;
        // compare health upgrades
        foreach (PhysicalUpgrade.HealthUpgrade hu in healthUpgrades) {
            if (nodeData.healthUpgrades.IndexOf(hu) == -1) return false;
        }
        foreach (PhysicalUpgrade.HealthUpgrade hu in nodeData.healthUpgrades) {
            if (healthUpgrades.IndexOf(hu) == -1) return false;
        }
        // compare phase replacements
        if (phaseReplacements != nodeData.phaseReplacements) return false;
        // compare physical events
        foreach (AdventureEvent.Physical pe in physicalEvents) {
            if (nodeData.physicalEvents.IndexOf(pe) == -1) return false;
        }
        foreach (AdventureEvent.Physical pe in nodeData.physicalEvents) {
            if (physicalEvents.IndexOf(pe) == -1) return false;
        }
        // compare ambushes defeated
        foreach (string levelName in levelsAmbushesDefeated) {
            if (nodeData.levelsAmbushesDefeated.IndexOf(levelName) == -1) return false;
        }
        foreach (string levelName in nodeData.levelsAmbushesDefeated) {
            if (levelsAmbushesDefeated.IndexOf(levelName) == -1) return false;
        }
        // compare damage barriers
        foreach (string db in damageBarriersDestroyed) {
            if (nodeData.damageBarriersDestroyed.IndexOf(db) == -1) return false;
        }
        foreach (string db in nodeData.damageBarriersDestroyed) {
            if (damageBarriersDestroyed.IndexOf(db) == -1) return false;
        }

        return true;
    }

    public void loadFromString(string str) {
        char[] delims = {'>'};
        char[] delims2 = {'?'};
        string[] strs = str.Split(delims);

        // id
        loadIDFromString(str);
        // level
        level = strs[1];
        // level map x
        levelMapX = int.Parse(strs[2]);
        // level map y
        levelMapY = int.Parse(strs[3]);
        // position
        position.Set(float.Parse(strs[4]), float.Parse(strs[5]));
        // time
        time = float.Parse(strs[6]);
        // orbs
        orbs.Clear();
        string[] orbStrs = strs[7].Split(delims2);
        for (int i=0; i<orbStrs.Length; i++) {
            if (orbStrs[i] == "") continue;
            orbs.Add((PhysicalUpgrade.Orb)int.Parse(orbStrs[i]));
        }
        // booster
        hasBooster = (strs[8] == "1");
        // health upgrades
        healthUpgrades.Clear();
        string[] healthStrs = strs[9].Split(delims2);
        for (int i = 0; i < healthStrs.Length; i++) {
            if (healthStrs[i] == "")continue;
            healthUpgrades.Add((PhysicalUpgrade.HealthUpgrade)int.Parse(healthStrs[i]));
        }
        // phase replacements
        phaseReplacements = int.Parse(strs[10]);
        // parent
        int parentID = int.Parse(strs[11]);
        parent = nodeDataFromID(parentID);
        // children
        children.Clear();
        string[] childrenStrs = strs[12].Split(delims2);
        for (int i=0; i<childrenStrs.Length; i++) {
            if (childrenStrs[i] == "") continue;
            children.Add(nodeDataFromID(int.Parse(childrenStrs[i])));
        }
        // physical events
        string[] physStrs = strs[13].Split(delims2);
        for (int i = 0; i < physStrs.Length; i++) {
            if (physStrs[i] == "") continue;
            physicalEvents.Add((AdventureEvent.Physical)int.Parse(physStrs[i]));
        }
        // ambushes defeated
        string[] ambushStrs = strs[14].Split(delims2);
        for (int i = 0; i < ambushStrs.Length; i++) {
            if (ambushStrs[i] == "") continue;
            levelsAmbushesDefeated.Add(ambushStrs[i]);
        }
        // damage barriers destroyed
        string[] dbStrs = strs[15].Split(delims2);
        for (int i=0; i<dbStrs.Length; i++) {
            if (dbStrs[i] == "") continue;
            damageBarriersDestroyed.Add(dbStrs[i]);
        }

    }

    public string saveToString() {
        string ret = "";
        // id (0)
        ret += id + ">";
        // level (1)
        ret += level + ">";
        // level map x (2)
        ret += levelMapX + ">";
        // level map y (3)
        ret += levelMapY + ">";
        // position (4, 5)
        ret += position.x + ">" + position.y + ">";
        // time (6)
        ret += time + ">";
        // orbs (7)
        for (int i=0; i<orbs.Count; i++) {
            ret += (int)orbs[i];
            if (i < orbs.Count - 1) ret += "?";
        }
        ret += ">";
        // booster (8)
        if (hasBooster) ret += "1>";
        else ret += "0>";
        // health upgrades (9)
        for (int i = 0; i < healthUpgrades.Count; i++) {
            ret += (int)healthUpgrades[i];
            if (i < healthUpgrades.Count - 1) ret += "?";
        }
        ret += ">";
        // phase replacements (10)
        ret += phaseReplacements + ">";
        // parent (11)
        if (parent == null) ret += "0>";
        else ret += parent.id + ">";
        // children (12)
        for (int i = 0; i < children.Count; i++) {
            ret += children[i].id;
            if (i < children.Count - 1) ret += "?";
        }
        ret += ">";
        // physical events (13)
        for (int i = 0; i < physicalEvents.Count; i++) {
            ret += (int)physicalEvents[i];
            if (i < physicalEvents.Count - 1) ret += "?";
        }
        ret += ">";
        // ambushes defeated (14)
        for (int i = 0; i < levelsAmbushesDefeated.Count; i++) {
            ret += levelsAmbushesDefeated[i];
            if (i < levelsAmbushesDefeated.Count - 1) ret += "?";
        }
        ret += ">";
        // damage barriers destroyed (15)
        for (int i = 0; i < damageBarriersDestroyed.Count; i++) {
            ret += damageBarriersDestroyed[i];
            if (i < damageBarriersDestroyed.Count - 1) ret += "?";
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
    public static NodeData createNodeData(NodeData parent, bool temporary) {
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
        nodeData.temporary = temporary;
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
        if (nodeData == null) return null;
        NodeData parent = nodeData.parent;
        nodeData.parent = null; // removes reference to parent
        if (parent != null) {
            parent.children.Remove(nodeData); // removes child reference from parent
        }
        while (nodeData.children.Count > 0) {
            NodeData childNode = nodeData.children[0];
            if (childNode != nodeData) { // this shouldn't happen
                deleteNode(childNode);
            }
            nodeData.children.Remove(childNode);
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
            if (allNodes[i].temporary)
                continue;
            ret += allNodes[i].saveToString();
            if (i < allNodes.Count - 1) {
                ret += "<";
            }
        }
        return ret;
    }

    /////////////
    // PRIVATE //
    /////////////

    string damageBarrierCode(string levelName, string objectName) {
        return levelName + "." + objectName;
    }

}
