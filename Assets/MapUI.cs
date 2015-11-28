using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MapUI : MonoBehaviour {

    public static int GRID_WIDTH = 60; // width in cells
    public static int GRID_HEIGHT = 50; // height in cells
    private static int[,] grid = new int[GRID_HEIGHT, GRID_WIDTH];

    public static int gridGetCell(int x, int y) {
        return grid[y, x];
    }
    public static void gridSetCell(int x, int y, int cell) {
        grid[y, x] = cell;
    }
    public static string gridToString() {
        string str = "";
        for (int i = 0; i < grid.Length; i++) {
            str = str + grid[i / GRID_HEIGHT, i % GRID_HEIGHT] + ",";
        }
        return str;
    }
    public static void gridFromString(string str) {
        char[] delims = {','};
        string[] cells = str.Split(delims);
        for (int i = 0; i < grid.Length; i++) {
            grid[i / GRID_HEIGHT, i % GRID_HEIGHT] = int.Parse(cells[i]);
        }
    }

    public static void gridAddRoom(
        int x,
        int y,
        int roomWidth,
        int roomHeight,
        int[,] cells) {
        for (int iy = 0; iy < roomHeight; iy++) {
            for (int ix = 0; ix < roomWidth; ix++) {
                gridSetCell(x + ix, y + iy, cells[iy, ix]);
            }
        }
    }
    public static void gridAddRoom(
        int x,
        int y,
        int roomWidth,
        int roomHeight,
        bool[] openLeftEdges,
        bool[] openTopEdges,
        bool[] openRightEdges,
        bool[] openBottomEdges) {
        if (roomWidth < 1 || roomHeight < 1) {
            Debug.LogError("Room invalid dimensions");
            return;
        }
        int[,] cells = new int[roomHeight, roomWidth];
        for (int iy = 0; iy < roomHeight; iy++) {
            for (int ix = 0; ix < roomWidth; ix++) {
                Edge leftEdge = Edge.NO_WALL;
                Edge topEdge = Edge.NO_WALL;
                Edge rightEdge = Edge.NO_WALL;
                Edge bottomEdge = Edge.NO_WALL;
                if (ix == 0) {
                    if (iy < openLeftEdges.Length && openLeftEdges[iy])
                        leftEdge = Edge.OPEN;
                    else leftEdge = Edge.WALL;
                }
                if (iy == 0) {
                    if (ix < openTopEdges.Length && openTopEdges[ix])
                        topEdge = Edge.OPEN;
                    else topEdge = Edge.WALL;
                }
                if (ix == roomWidth - 1) {
                    if (iy < openRightEdges.Length && openRightEdges[iy])
                        rightEdge = Edge.OPEN;
                    else rightEdge = Edge.WALL;
                }
                if (iy == roomHeight - 1) {
                    if (ix < openBottomEdges.Length && openBottomEdges[ix])
                        bottomEdge = Edge.OPEN;
                    else bottomEdge = Edge.WALL;
                }
                cells[iy, ix] = makeCell(leftEdge, topEdge, rightEdge, bottomEdge);
            }
        }
        gridAddRoom(x, y, roomWidth, roomHeight, cells);
    }


    public GameObject leftWallGameObject;
    public GameObject leftWallOpenGameObject;
    public GameObject topWallGameObject;
    public GameObject topWallOpenGameObject;
    public GameObject rightWallGameObject;
    public GameObject rightWallOpenGameObject;
    public GameObject bottomWallGameObject;
    public GameObject bottomWallOpenGameObject;
    public float cellWidth = 9;
    public float cellHeight = 5;

    public enum Edge:int {
        NO_WALL = 0,
        OPEN = 1,
        WALL = 2
    }

    public static int makeCell(Edge leftEdge, Edge topEdge, Edge rightEdge, Edge bottomEdge) {
        int ret = 0;
        if (leftEdge != Edge.NO_WALL) {
            ret |= 1 << ((int)leftEdge - 1);
        }
        if (topEdge != Edge.NO_WALL) {
            ret |= 1 << 2 + ((int)topEdge - 1);
        }
        if (rightEdge != Edge.NO_WALL) {
            ret |= 1 << 4 + ((int)rightEdge - 1);
        }
        if (bottomEdge != Edge.NO_WALL) {
            ret |= 1 << 6 + ((int)bottomEdge - 1);
        }
        return ret;
    }
    public static Edge leftEdge(int cell) {
        if ((cell & 1) != 0) return Edge.OPEN;
        if ((cell & 2) != 0) return Edge.WALL;
        return Edge.NO_WALL;
    }
    public static Edge topEdge(int cell) {
        if ((cell & 4) != 0) return Edge.OPEN;
        if ((cell & 8) != 0) return Edge.WALL;
        return Edge.NO_WALL;
    }
    public static Edge rightEdge(int cell) {
        if ((cell & 16) != 0) return Edge.OPEN;
        if ((cell & 32) != 0) return Edge.WALL;
        return Edge.NO_WALL;
    }
    public static Edge bottomEdge(int cell) {
        if ((cell & 64) != 0) return Edge.OPEN;
        if ((cell & 128) != 0) return Edge.WALL;
        return Edge.NO_WALL;
    }
	

	void Awake() {
        
	}

    void Start() {

        gridAddRoom(30, 30, 3, 2,
            new bool[] { true, false},
            new bool[] { false, false, false },
            new bool[] { false, true },
            new bool[] { false, false, false });

        gridAddRoom(33, 31, 1, 1,
            new bool[] { true },
            new bool[] { false },
            new bool[] { true },
            new bool[] { true });

        gridAddRoom(33, 32, 4, 1,
            new bool[] { false },
            new bool[] { true, false, false, false },
            new bool[] { false },
            new bool[] { false, false, true, false });

        makeMap();

    }
	
	void Update() {
		
	}


    void makeMap() {
        for (int y = 0; y < GRID_HEIGHT; y++) {
            for (int x = 0; x < GRID_WIDTH; x++) {
                if (grid[y, x] == 0) continue;

                int cell = grid[y, x];
                Image edge;
                if (leftEdge(cell) == Edge.WALL) {
                    edge = GameObject.Instantiate(leftWallGameObject).GetComponent<Image>();
                    edge.transform.SetParent(transform, false);
                    edge.rectTransform.localPosition = new Vector2(x * 2 * cellWidth, y * -2 * cellHeight);
                    edgeImages.Add(edge);
                } else if (leftEdge(cell) == Edge.OPEN) {
                    edge = GameObject.Instantiate(leftWallOpenGameObject).GetComponent<Image>();
                    edge.transform.SetParent(transform, false);
                    edge.rectTransform.localPosition = new Vector2(x * 2 * cellWidth, y * -2 * cellHeight);
                    edgeImages.Add(edge);
                }
                if (topEdge(cell) == Edge.WALL) {
                    edge = GameObject.Instantiate(topWallGameObject).GetComponent<Image>();
                    edge.transform.SetParent(transform, false);
                    edge.rectTransform.localPosition = new Vector2(x * 2 * cellWidth, y * -2 * cellHeight);
                    edgeImages.Add(edge);
                } else if (topEdge(cell) == Edge.OPEN) {
                    edge = GameObject.Instantiate(topWallOpenGameObject).GetComponent<Image>();
                    edge.transform.SetParent(transform, false);
                    edge.rectTransform.localPosition = new Vector2(x * 2 * cellWidth, y * -2 * cellHeight);
                    edgeImages.Add(edge);
                }
                if (rightEdge(cell) == Edge.WALL) {
                    edge = GameObject.Instantiate(rightWallGameObject).GetComponent<Image>();
                    edge.transform.SetParent(transform, false);
                    edge.rectTransform.localPosition = new Vector2((x+1) * 2 * cellWidth, y * -2 * cellHeight);
                    edgeImages.Add(edge);
                } else if (rightEdge(cell) == Edge.OPEN) {
                    edge = GameObject.Instantiate(rightWallOpenGameObject).GetComponent<Image>();
                    edge.transform.SetParent(transform, false);
                    edge.rectTransform.localPosition = new Vector2((x+1) * 2 * cellWidth, y * -2 * cellHeight);
                    edgeImages.Add(edge);
                }
                if (bottomEdge(cell) == Edge.WALL) {
                    edge = GameObject.Instantiate(bottomWallGameObject).GetComponent<Image>();
                    edge.transform.SetParent(transform, false);
                    edge.rectTransform.localPosition = new Vector2(x * 2 * cellWidth, (y+1) * -2 * cellHeight);
                    edgeImages.Add(edge);
                } else if (bottomEdge(cell) == Edge.OPEN) {
                    edge = GameObject.Instantiate(bottomWallOpenGameObject).GetComponent<Image>();
                    edge.transform.SetParent(transform, false);
                    edge.rectTransform.localPosition = new Vector2(x * 2 * cellWidth, (y+1) * -2 * cellHeight);
                    edgeImages.Add(edge);
                }

            }
        }
    }

    List<Image> edgeImages = new List<Image>();

}
