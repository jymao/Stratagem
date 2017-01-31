using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour {

    public int level;
    
    //tile base image is 16px, scaled 4x in game
    public int tilePixelWidth;
    public int mapPixelWidth;
    public int mapPixelHeight;
    private float pixelUnitRatio; //pixel to world space

    public GameObject map;
    private Bounds mapBounds;
    private float mapWidth;
    private float mapHeight;

    private float tileWidth; //converted to world space units
    private float originX; //origin is top left of map instead of middle
    private float originY;

    //enemy prefabs
    public GameObject enemyAssassin;
    public GameObject enemyKnight;
    public GameObject enemyMage;
    public GameObject enemySpear;
    public GameObject enemyCannon;
    public GameObject enemyAmbulance;
    //player prefabs
    public GameObject playerAssassin;
    public GameObject playerKnight;
    public GameObject playerMage;
    public GameObject playerSpear;
    public GameObject playerCannon;
    public GameObject playerAmbulance;
    //tile prefabs
    public GameObject enemyAttackTile;
    public GameObject playerMoveTile;

    public GameObject gridDrawer;
    private GridTile[,] grid;

    public GameObject phaseText;
    public GameObject chooseUnitUI;
    public GameObject actionUI;
    public GameObject unitInfoManager;
    public GameObject deployButton;

    private bool deployPhase = true;
    private bool playerPhase = false;
    private int playerUnits;
    private int deployed = 0;

    private Vector3 chosenLoc;

    private List<GameObject> enemies;
    private List<Vector2> moveTileCoords;

	// Use this for initialization
	void Start () {
        mapBounds = map.GetComponent<Renderer>().bounds;
        mapWidth = mapBounds.size.x;
        mapHeight = mapBounds.size.y;

        pixelUnitRatio = mapPixelWidth / mapWidth;

        tileWidth = tilePixelWidth / pixelUnitRatio;
        originX = -(mapWidth / 2);
        originY = (mapHeight / 2);
        
        enemies = new List<GameObject>();
        moveTileCoords = new List<Vector2>();
        ReadLevelFile();
        //DebugShowGrid();

        StartCoroutine(PhaseIntro("Deployment Phase"));
        //StartCoroutine(PhaseIntro("Player Turn"));
        //StartCoroutine(PhaseIntro("Enemy Turn"));
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButtonDown(0))
        {
            //DebugMouseClickToGrid();
            ClickToGrid();
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            RemoveTiles();
        }
	}

    //get map dimensions in world space units
    public float GetMapWidth()
    {
        return mapWidth;
    }

    public float GetMapHeight()
    {
        return mapHeight;
    }

    //Load level by reading in level data file, instantiate grid properly
    private void ReadLevelFile()
    {
        TextAsset leveldata = Resources.Load("level" + level) as TextAsset;
        char[] newline = { '\n' };
        char[] space = { ' ' };
        string[] lines = leveldata.text.Split(newline, StringSplitOptions.RemoveEmptyEntries);

        int index = 0;

        //grid dimensions
        string[] gridDimensions = lines[index].Split(space, StringSplitOptions.RemoveEmptyEntries);
        grid = new GridTile[Int32.Parse(gridDimensions[0]), Int32.Parse(gridDimensions[1])];

        for (int row = 0; row < grid.GetLength(0); row++)
        {
            for(int col = 0; col < grid.GetLength(1); col++)
            {
                grid[row, col] = new GridTile(row, col);
            }
        }

        //player spawn locations
        SetGridTile(lines, ref index, "Spawn", true);

        //enemy assassins
        SetGridTile(lines, ref index, "Assassin", false);

        //enemy knights
        SetGridTile(lines, ref index, "Knight", false);

        //enemy mages
        SetGridTile(lines, ref index, "Mage", false);

        //enemy spearwomen
        SetGridTile(lines, ref index, "Spearwoman", false);

        //enemy cannoneers
        SetGridTile(lines, ref index, "Cannoneer", false);

        //enemy ambulances
        SetGridTile(lines, ref index, "Ambulance", false);

        //obstacles
        SetGridTile(lines, ref index, "Obstacle", false);

        
    }

    //Fill out grid tiles with the proper entities
    private void SetGridTile(string[] lines, ref int index, string resident, bool isPlayer)
    {
        char[] space = { ' ' };
        string[] data = lines[++index].Split(space, StringSplitOptions.RemoveEmptyEntries);
        int amount = Int32.Parse(data[1]);

        string[] coords;
        for (int i = 0; i < amount; i++)
        {
            coords = lines[++index].Split(space, StringSplitOptions.RemoveEmptyEntries);
            GridTile tile = grid[Int32.Parse(coords[0]), Int32.Parse(coords[1])];
            tile.resident = resident;
            tile.isPlayer = isPlayer;

            if(!isPlayer && !resident.Equals("Obstacle"))
            {
                AddEnemy(Int32.Parse(coords[0]), Int32.Parse(coords[1]), resident);
            }

            if(isPlayer)
            {
                AddSpawn(Int32.Parse(coords[0]), Int32.Parse(coords[1]));
            }
        }
    }

    //Create an enemy in the scene
    private void AddEnemy(int row, int col, string unit)
    {
        float x = col * tileWidth + originX + (tileWidth / 2);
        float y = originY - (row * tileWidth) - (tileWidth / 2);

        switch (unit)
        {
            case "Assassin":
                grid[row, col].residentObject = (GameObject)Instantiate(enemyAssassin, new Vector3(x, y, 0), Quaternion.identity);
                break;
            case "Knight":
                grid[row, col].residentObject = (GameObject)Instantiate(enemyKnight, new Vector3(x, y, 0), Quaternion.identity);
                break;
            case "Mage":
                grid[row, col].residentObject = (GameObject)Instantiate(enemyMage, new Vector3(x, y, 0), Quaternion.identity);
                break;
            case "Spearwoman":
                grid[row, col].residentObject = (GameObject)Instantiate(enemySpear, new Vector3(x, y, 0), Quaternion.identity);
                break;
            case "Cannoneer":
                grid[row, col].residentObject = (GameObject)Instantiate(enemyCannon, new Vector3(x, y, 0), Quaternion.identity);
                break;
            case "Ambulance":
                grid[row, col].residentObject = (GameObject)Instantiate(enemyAmbulance, new Vector3(x, y, 0), Quaternion.identity);
                break;
            default:
                break;
        }

        enemies.Add(grid[row, col].residentObject);
    }

    //Create player spawns in the scene
    private void AddSpawn(int row, int col)
    {
        float x = col * tileWidth + originX + (tileWidth / 2);
        float y = originY - (row * tileWidth) - (tileWidth / 2);

        grid[row, col].residentObject = (GameObject)Instantiate(playerMoveTile, new Vector3(x, y, 0), Quaternion.identity);

        playerUnits++;
    }

    private void DebugShowGrid()
    {
        for (int row = 0; row < grid.GetLength(0); row++)
        {
            for (int col = 0; col < grid.GetLength(1); col++)
            {
                float x = col * tileWidth + originX + (tileWidth / 2);
                float y = originY - (row * tileWidth) - (tileWidth / 2);

                switch(grid[row, col].resident)
                {
                    /*
                   case "Assassin":
                       Instantiate(enemyAssassin, new Vector3(x, y, 0), Quaternion.identity);
                       break;
                   case "Knight":
                       Instantiate(enemyKnight, new Vector3(x, y, 0), Quaternion.identity);
                       break;
                   case "Mage":
                       Instantiate(enemyMage, new Vector3(x, y, 0), Quaternion.identity);
                       break;
                   case "Spearwoman":
                       Instantiate(enemySpear, new Vector3(x, y, 0), Quaternion.identity);
                       break;
                   case "Cannoneer":
                       Instantiate(enemyCannon, new Vector3(x, y, 0), Quaternion.identity);
                       break;
                   case "Ambulance":
                       Instantiate(enemyAmbulance, new Vector3(x, y, 0), Quaternion.identity);
                       break;
                   case "Spawn":
                       Instantiate(playerMoveTile, new Vector3(x, y, 0), Quaternion.identity);
                       break;
                     */
                    case "Obstacle":
                        Instantiate(enemyAttackTile, new Vector3(x, y, 0), Quaternion.identity);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    //Find out the grid location from a mouse click in the scene and apply appropriate actions
    private void ClickToGrid()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 point = ray.origin + (ray.direction * -Camera.main.transform.position.z);

        if (point.x > originX && point.x < -originX && point.y < originY && point.y > -originY)
        {
            float distX = point.x - originX;
            float distY = originY - point.y;
            int col = Mathf.FloorToInt(distX / tileWidth);
            int row = Mathf.FloorToInt(distY / tileWidth);

            //Debug.Log(row + ", " + col);

            if(deployPhase)
            {
                Deploy(row, col);
                ShowUnitInfo(row, col);
            }

            if(playerPhase)
            {
                ShowUnitInfo(row, col);
                ShowActionMenu(row, col);
            }
        }
        else
        {
            UnitInfoManager unitInfoScript = unitInfoManager.GetComponent<UnitInfoManager>();
            unitInfoScript.UnitInfoHide();
            
        }
        
    }

    //For clicking on a player spawn in deploy phase
    private void Deploy(int row, int col)
    {
        if (grid[row, col].isPlayer)
        {
            chosenLoc = grid[row, col].residentObject.transform.position;
            chooseUnitUI.SetActive(true);
        }
        else
        {
            chooseUnitUI.SetActive(false);
        }
    }

    //Create a player unit at the spawn location
    public void SpawnPlayer(string unit)
    {
        int row = GetRowFromWorldSpace(chosenLoc.y);
        int col = GetColFromWorldSpace(chosenLoc.x);

        if(grid[row, col].resident.Equals("Spawn"))
        {
            deployed++;
        }

        Destroy(grid[row, col].residentObject);

        grid[row, col].resident = unit;

        switch(unit)
        {
            case "Assassin":
                grid[row, col].residentObject = (GameObject)Instantiate(playerAssassin, chosenLoc, Quaternion.identity);
                break;
            case "Knight":
                grid[row, col].residentObject = (GameObject)Instantiate(playerKnight, chosenLoc, Quaternion.identity);
                break;
            case "Mage":
                grid[row, col].residentObject = (GameObject)Instantiate(playerMage, chosenLoc, Quaternion.identity);
                break;
            case "Spearwoman":
                grid[row, col].residentObject = (GameObject)Instantiate(playerSpear, chosenLoc, Quaternion.identity);
                break;
            case "Cannoneer":
                grid[row, col].residentObject = (GameObject)Instantiate(playerCannon, chosenLoc, Quaternion.identity);
                break;
            case "Ambulance":
                grid[row, col].residentObject = (GameObject)Instantiate(playerAmbulance, chosenLoc, Quaternion.identity);
                break;
            default:
                break;
        }

        chooseUnitUI.SetActive(false);

        if(deployed == playerUnits)
        {
            deployButton.SetActive(true);
        }
    }

    //Find grid cell from world space units
    private int GetRowFromWorldSpace(float y)
    {
        if(y < originY && y > -originY)
        {
            float distY = originY - y;
            int row = Mathf.FloorToInt(distY / tileWidth);
            return row;
        }
        else
        {
            return -1;
        }
    }

    private int GetColFromWorldSpace(float x)
    {
        if (x > originX && x < -originX)
        {
            float distX = x - originX;
            int col = Mathf.FloorToInt(distX / tileWidth);
            return col;
        }
        else
        {
            return -1;
        }
    }

    private void DebugMouseClickToGrid()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 point = ray.origin + (ray.direction * 10);

        Debug.Log(point);

        if(point.x > originX && point.x < -originX && point.y < originY && point.y > -originY)
        {
            float distX = point.x - originX;
            float distY = originY - point.y;
            int col = Mathf.FloorToInt(distX / tileWidth);
            int row = Mathf.FloorToInt(distY / tileWidth);

            Debug.Log(row + ", " + col);

            return;
        }

        Debug.Log("Outside grid");
    }

    //fade in and fade out for text of each phase (deploy, player and enemy turns)
    private IEnumerator PhaseIntro(string phase)
    {
        Text text = phaseText.GetComponent<Text>();
        text.text = phase;

        Color textColor = new Color(1, 1, 1, 0);

        if(phase.Equals("Player Turn"))
        {
            textColor = new Color(44f / 255f, 44f / 255f, 242f / 255f, 0);
        }
        else if(phase.Equals("Enemy Turn"))
        {
            textColor = new Color(242f / 255f, 44f / 255f, 44f / 255f, 0);
        }

        text.color = textColor;

        for (int i = 0; i < 50; i++)
        {
            yield return new WaitForSeconds(0.001f);
            textColor.a += 0.02f;
            text.color = textColor;
        }

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 50; i++)
        {
            yield return new WaitForSeconds(0.001f);
            textColor.a -= 0.02f;
            text.color = textColor;
        }
        
    }

    //End deploy phase and start game with player turn
    public void EndDeployPhase()
    {
        deployButton.SetActive(false);
        deployPhase = false;
        StartCoroutine(PhaseIntro("Player Turn"));
        playerPhase = true;
    }

    //Determine if entity in grid cell is a unit and display appropriate unit info
    private void ShowUnitInfo(int row, int col)
    {
        UnitInfoManager unitInfoScript = unitInfoManager.GetComponent<UnitInfoManager>();

        string resident = grid[row, col].resident;
        GameObject residentObject = grid[row, col].residentObject;

        if(!resident.Equals("Empty") && !resident.Equals("Obstacle"))
        {
            if(grid[row, col].isPlayer)
            {
                if(!resident.Equals("Spawn"))
                {
                    unitInfoScript.DisplayUnitInfo(residentObject, resident, false);
                }
                else
                {
                    unitInfoScript.UnitInfoHide();
                }
            }
            else
            {
                unitInfoScript.DisplayUnitInfo(residentObject, resident, true);
            }
        }
        else
        {
            unitInfoScript.UnitInfoHide();
        }
    }

    //For clicking on a player unit during player turn to show action menu
    private void ShowActionMenu(int row, int col)
    {
        if(grid[row, col].isPlayer)
        {
            chosenLoc = grid[row, col].residentObject.transform.position;
            actionUI.SetActive(true);
        }
        else
        {
            actionUI.SetActive(false);
        }
    }

    //Use BFS-style search to create tiles to show where the specific unit can move to based on the move stat
    public void CalcMoveTiles()
    {
        actionUI.SetActive(false);

        int row = GetRowFromWorldSpace(chosenLoc.y);
        int col = GetColFromWorldSpace(chosenLoc.x);   

        int move = grid[row, col].residentObject.GetComponent<Unit>().move;
        int level = 0;
        int nodesInLevel = 1;

        Queue<GridTile> queue = new Queue<GridTile>();
        queue.Enqueue(grid[row, col]);
        grid[row, col].visited = true;
   
        while(queue.Count > 0)
        {
            int nodesInNextLevel = 0;
            for(int i = 0; i < nodesInLevel; i++)
            {
                GridTile currTile = queue.Dequeue();
                int currRow = currTile.row;
                int currCol = currTile.col;

                if(level < move)
                {
                    if (!currTile.northTaken)
                    {
                        currTile.northTaken = true;
                        if (PosInGrid(currRow - 1, currCol))
                        {     
                            if (grid[currRow - 1, currCol].resident.Equals("Empty") || grid[currRow - 1, currCol].resident.Equals("MoveTile"))
                            {
                                grid[currRow - 1, currCol].southTaken = true;
                                queue.Enqueue(grid[currRow - 1, currCol]);
                                nodesInNextLevel++;
                                if (grid[currRow - 1, currCol].resident.Equals("Empty"))
                                {
                                    MakeTile(currRow - 1, currCol);
                                }
                            }
                        }
                    }
                    if (!currTile.westTaken)
                    {
                        currTile.westTaken = true;
                        if (PosInGrid(currRow, currCol - 1))
                        {
                            if (grid[currRow, currCol - 1].resident.Equals("Empty") || grid[currRow, currCol - 1].resident.Equals("MoveTile"))
                            {
                                grid[currRow, currCol - 1].eastTaken = true;
                                queue.Enqueue(grid[currRow, currCol - 1]);
                                nodesInNextLevel++;
                                if (grid[currRow, currCol - 1].resident.Equals("Empty"))
                                {
                                    MakeTile(currRow, currCol - 1);
                                }
                            }
                        }
                    }
                    if (!currTile.southTaken)
                    {
                        currTile.southTaken = true;
                        if (PosInGrid(currRow + 1, currCol))
                        {
                            if (grid[currRow + 1, currCol].resident.Equals("Empty") || grid[currRow + 1, currCol].resident.Equals("MoveTile"))
                            {
                                grid[currRow + 1, currCol].northTaken = true;
                                queue.Enqueue(grid[currRow + 1, currCol]);
                                nodesInNextLevel++;
                                if (grid[currRow + 1, currCol].resident.Equals("Empty"))
                                {
                                    MakeTile(currRow + 1, currCol);
                                }
                            }
                        }
                    }
                    if (!currTile.eastTaken)
                    {
                        currTile.eastTaken = true;
                        if (PosInGrid(currRow, currCol + 1))
                        {
                            if (grid[currRow, currCol + 1].resident.Equals("Empty") || grid[currRow, currCol + 1].resident.Equals("MoveTile"))
                            {
                                grid[currRow, currCol + 1].westTaken = true;
                                queue.Enqueue(grid[currRow, currCol + 1]);
                                nodesInNextLevel++;
                                if (grid[currRow, currCol + 1].resident.Equals("Empty"))
                                {
                                    MakeTile(currRow, currCol + 1);
                                }
                            }
                        }
                    }
                }
            }

            nodesInLevel = nodesInNextLevel;
            level++;        
        }

        grid[row, col].Reset();
    }

    //Check if position is valid in the grid
    private bool PosInGrid(int row, int col)
    {
        int numRows = grid.GetLength(0);
        int numCols = grid.GetLength(1);

        if(row >= 0 && row < numRows && col >= 0 && col < numCols)
        {
            return true;
        }

        return false;
    }

    private void MakeTile(int row, int col)
    {
        if(!grid[row, col].visited)
        {
            float x = col * tileWidth + originX + (tileWidth / 2);
            float y = originY - (row * tileWidth) - (tileWidth / 2);

            grid[row, col].resident = "MoveTile";
            grid[row, col].residentObject = (GameObject)Instantiate(playerMoveTile, new Vector3(x, y, 0), Quaternion.identity);
            moveTileCoords.Add(new Vector2(row, col));
            grid[row, col].visited = true;
        }
    }

    private void RemoveTiles()
    {
        for(int i = 0; i < moveTileCoords.Count; i++)
        {
            Vector2 coords = moveTileCoords[i];
            int row = (int)coords.x;
            int col = (int)coords.y;

            grid[row, col].resident = "Empty";
            grid[row, col].Reset();
            Destroy(grid[row, col].residentObject);
        }

        moveTileCoords = new List<Vector2>();
    }

    private void CalcAttackTiles()
    {

    }

    //Use A* to find shortest path between two grid cells
    private List<Vector2> CalcPath(Vector2 start, Vector2 end)
    {
        List<Vector2> result = new List<Vector2>(); //shortest path from start to end if it exists

        PriorityQueue openSet = new PriorityQueue(); //to retrieve node with smallest cost
        Dictionary<Vector2, Vector2> cameFrom = new Dictionary<Vector2, Vector2>(); //to keep track of paths
        Dictionary<Vector2, int> costSoFar = new Dictionary<Vector2, int>(); //cost from start to current node

        costSoFar[start] = 0;
        openSet.Insert(new Vector3(start.x, start.y, ManhattanDistance(start, end)));

        while(openSet.Count() > 0)
        {
            Vector3 currentNode = openSet.RemoveMin();
            int row = (int)currentNode.x;
            int col = (int)currentNode.y;
            Vector2 current = new Vector2(row, col);

            //if current node is the goal
            if(currentNode.x == end.x && currentNode.y == end.y)
            {
                result.Add(current);
                while(cameFrom.ContainsKey(current))
                {
                    current = cameFrom[current];
                    result.Add(current);
                }
                result.Reverse();
                return result;
            }

            //grid[row, col].visited = true;

            //north neighbor
            if(PosInGrid(row - 1, col))
            {
                //no obstacles
                if(grid[row - 1, col].resident.Equals("Empty"))
                {
                    int cost = costSoFar[current] + 1; //cost to current plus 1 cost to get to neighbor
                    Vector2 next = new Vector2(row - 1, col);
                    //not explored yet so no cost for neighbor found, or found before but this time cost is smaller
                    if(!costSoFar.ContainsKey(next) || cost < costSoFar[next])
                    {
                        costSoFar[next] = cost;
                        openSet.Insert(new Vector3(next.x, next.y, cost + ManhattanDistance(next, end)));
                        cameFrom[next] = current;
                    }
                }
            }
            //south neighbor
            if (PosInGrid(row + 1, col))
            {
                if (grid[row + 1, col].resident.Equals("Empty"))
                {
                    int cost = costSoFar[current] + 1;
                    Vector2 next = new Vector2(row + 1, col);
                    if (!costSoFar.ContainsKey(next) || cost < costSoFar[next])
                    {
                        costSoFar[next] = cost;
                        openSet.Insert(new Vector3(next.x, next.y, cost + ManhattanDistance(next, end)));
                        cameFrom[next] = current;
                    }
                }
            }
            //west neighbor
            if (PosInGrid(row, col - 1))
            {
                if (grid[row, col - 1].resident.Equals("Empty"))
                {
                    int cost = costSoFar[current] + 1;
                    Vector2 next = new Vector2(row, col - 1);
                    if (!costSoFar.ContainsKey(next) || cost < costSoFar[next])
                    {
                        costSoFar[next] = cost;
                        openSet.Insert(new Vector3(next.x, next.y, cost + ManhattanDistance(next, end)));
                        cameFrom[next] = current;
                    }
                }
            }
            //east neighbor
            if (PosInGrid(row, col + 1))
            {
                if (grid[row, col + 1].resident.Equals("Empty"))
                {
                    int cost = costSoFar[current] + 1;
                    Vector2 next = new Vector2(row, col + 1);
                    if (!costSoFar.ContainsKey(next) || cost < costSoFar[next])
                    {
                        costSoFar[next] = cost;
                        openSet.Insert(new Vector3(next.x, next.y, cost + ManhattanDistance(next, end)));
                        cameFrom[next] = current;
                    }
                }
            }
        }

        return result;
    }

    //heuristic for A*
    private int ManhattanDistance(Vector2 start, Vector2 end)
    {
        int startRow = (int)start.x;
        int startCol = (int)start.y;
        int endRow = (int)end.x;
        int endCol = (int)end.y;

        int distX = startCol - endCol;
        int distY = startRow - endRow;

        if (distX < 0)
            distX *= -1;
        if (distY < 0)
            distY *= -1;

        return distX + distY;
    }
}