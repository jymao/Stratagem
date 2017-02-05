using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

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

    public GameObject explosion;

    public GameObject gridDrawer;
    private GridTile[,] grid;

    public GameObject phaseText;
    public GameObject chooseUnitUI;
    public GameObject actionUI;
    public GameObject unitInfoManager;
    public GameObject deployButton;
    public GameObject gameOverUI;
    public GameObject instructionText;

    private bool deployPhase = true;
    private bool playerPhase = false;
    private bool attacking = false;
    private bool healing = false;
    private bool clickingAllowed = true;
    private int playerUnits;

    private Vector3 chosenLoc;

    private List<GameObject> enemies;
    private List<GameObject> players;
    private List<Vector2> moveTileCoords;
    private Dictionary<Vector2, GameObject> attackTiles;
    private Dictionary<Vector2, GameObject> healTiles;

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
        players = new List<GameObject>();
        moveTileCoords = new List<Vector2>();
        attackTiles = new Dictionary<Vector2, GameObject>();
        healTiles = new Dictionary<Vector2, GameObject>();
        
        ReadLevelFile();
        //DebugShowGrid();

        StartCoroutine(PhaseIntro("Deployment Phase"));
        //StartCoroutine(PhaseIntro("Player Turn"));
        //StartCoroutine(PhaseIntro("Enemy Turn"));
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButtonDown(0) && clickingAllowed)
        {
            //DebugMouseClickToGrid();
            ClickToGrid();

            //Debug.Log(chosenLoc);
            
        }

        if(players.Count == 0 && !deployPhase)
        {
            clickingAllowed = false;
            StartCoroutine(GameOver(false));
        }

        if (enemies.Count == 0 && !deployPhase)
        {
            clickingAllowed = false;
            StartCoroutine(GameOver(true));
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
        float x = GetWorldXFromCol(col);
        float y = GetWorldYFromRow(row);

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
        float x = GetWorldXFromCol(col);
        float y = GetWorldYFromRow(row);

        grid[row, col].residentObject = (GameObject)Instantiate(playerMoveTile, new Vector3(x, y, 0), Quaternion.identity);

        playerUnits++;
    }

    private void DebugShowGrid()
    {
        for (int row = 0; row < grid.GetLength(0); row++)
        {
            for (int col = 0; col < grid.GetLength(1); col++)
            {
                float x = GetWorldXFromCol(col);
                float y = GetWorldYFromRow(row);

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
                if (!attacking && !healing)
                {
                    ShowUnitInfo(row, col);
                }
                ShowActionMenu(row, col);
                CheckMove(row, col);
                CheckAttack(row, col);
                CheckHeal(row, col);
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

        for (int i = 0; i < players.Count; i++)
        {
            if(players[i] == grid[row, col].residentObject)
            {
                players.RemoveAt(i);
                break;
            }
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

        players.Add(grid[row, col].residentObject);
        chooseUnitUI.SetActive(false);

        if(players.Count == playerUnits)
        {
            deployButton.SetActive(true);
            instructionText.SetActive(false);
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

    private float GetWorldXFromCol(int col)
    {
        return col * tileWidth + originX + (tileWidth / 2);
    }

    private float GetWorldYFromRow(int row)
    {
        return originY - (row * tileWidth) - (tileWidth / 2);
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

        if(!resident.Equals("Empty") && !resident.Equals("Obstacle") && !resident.Equals("MoveTile") && !resident.Equals("AttackTile"))
        {
            if(grid[row, col].isPlayer)
            {
                if(!resident.Equals("Spawn"))
                {
                    unitInfoScript.DisplayUnitInfo(residentObject, resident);
                }
                else
                {
                    unitInfoScript.UnitInfoHide();
                }
            }
            else
            {
                unitInfoScript.DisplayUnitInfo(residentObject, resident);
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
        //return previous chosen player to normal color
        int prevRow = GetRowFromWorldSpace(chosenLoc.y);
        int prevCol = GetColFromWorldSpace(chosenLoc.x);

        //check if previous character has died
        if (grid[prevRow, prevCol].isPlayer)
        {
            Color normal = new Color(1, 1, 1, 1);
            grid[prevRow, prevCol].residentObject.GetComponent<SpriteRenderer>().color = normal;
        }

        if(grid[row, col].isPlayer && !healing)
        {
            GameObject unit = grid[row, col].residentObject;
            chosenLoc = unit.transform.position;
            actionUI.SetActive(true);

            if(grid[row, col].resident.Equals("Ambulance"))
            {
                actionUI.transform.GetChild(2).gameObject.SetActive(true);
            }
            else
            {
                actionUI.transform.GetChild(2).gameObject.SetActive(false);
            }

            //check if unit has moved and/or acted and set those buttons as uninteractable
            Unit unitScript = unit.GetComponent<Unit>();
            if(unitScript.GetMoved())
            {
                actionUI.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = false;
            }
            else
            {
                actionUI.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = true;
            }

            if(unitScript.GetActed())
            {
                actionUI.transform.GetChild(1).gameObject.GetComponent<Button>().interactable = false;
                if (grid[row, col].resident.Equals("Ambulance"))
                {
                    GameObject ambulanceActions = actionUI.transform.GetChild(2).gameObject;
                    ambulanceActions.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = false;
                    //ambulanceActions.transform.GetChild(1).gameObject.GetComponent<Button>().interactable = false;
                    //ambulanceActions.transform.GetChild(2).gameObject.GetComponent<Button>().interactable = false;
                }
            }
            else
            {
                actionUI.transform.GetChild(1).gameObject.GetComponent<Button>().interactable = true;
                if (grid[row, col].resident.Equals("Ambulance"))
                {
                    actionUI.transform.GetChild(1).gameObject.GetComponent<Button>().interactable = false;

                    GameObject ambulanceActions = actionUI.transform.GetChild(2).gameObject;
                    ambulanceActions.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = true;
                    //ambulanceActions.transform.GetChild(1).gameObject.GetComponent<Button>().interactable = true;
                    /*
                    if (players.Count != playerUnits)
                    {
                        ambulanceActions.transform.GetChild(2).gameObject.GetComponent<Button>().interactable = true;
                    }
                    else
                    {
                        ambulanceActions.transform.GetChild(2).gameObject.GetComponent<Button>().interactable = false;
                    }
                    */
                }
            }

            //highlight chosen player
            Color highlight = new Color(1, 1, 0, 1f);
            unit.GetComponent<SpriteRenderer>().color = highlight;
        }
        else
        {
            actionUI.SetActive(false);
            actionUI.transform.GetChild(2).gameObject.SetActive(false);
        }
    }

    //check if clicked a valid move tile to move there, else cancel move operation
    private void CheckMove(int row, int col)
    {
        if(grid[row, col].resident.Equals("MoveTile"))
        {
            RemoveTiles(true);

            int prevRow = GetRowFromWorldSpace(chosenLoc.y);
            int prevCol = GetColFromWorldSpace(chosenLoc.x);

            List<Vector2> path = CalcPath(new Vector2(prevRow, prevCol), new Vector2(row, col));
            StartCoroutine(FollowPath(path, prevRow, prevCol));

            float x = GetWorldXFromCol(col);
            float y = GetWorldYFromRow(row);
            chosenLoc = new Vector3(x, y, 0);    
        }

        gridDrawer.SetActive(false);
        RemoveTiles(true);
    }

    //animate unit moving along path
    private IEnumerator FollowPath(List<Vector2> path, int row, int col)
    {
        clickingAllowed = false;
        GameObject unit = grid[row, col].residentObject;
        grid[row, col].residentObject = null;
        grid[row, col].isPlayer = false;

        //start from 1 since the cell unit is on is included
        for (int i = 1; i < path.Count; i++)
        {
            Vector2 endCoord = path[i];
            float x = GetWorldXFromCol((int)endCoord.y);
            float y = GetWorldYFromRow((int)endCoord.x);
            Vector3 start = unit.transform.position;
            Vector3 end = new Vector3(x, y, 0);

            Vector3 interval = (end - start) * (0.1f);
            for (int j = 0; j < 10; j++)
            {
                unit.transform.position = unit.transform.position + interval;
                yield return new WaitForSeconds(0.001f);
            }

        }

        int endRow = (int)path[path.Count - 1].x;
        int endCol = (int)path[path.Count - 1].y;

        grid[endRow, endCol].resident = grid[row, col].resident;
        grid[row, col].resident = "Empty";
        grid[endRow, endCol].residentObject = unit;
        grid[endRow, endCol].residentObject.GetComponent<Unit>().SetMoved(true);

        if (playerPhase)
        {
            grid[endRow, endCol].isPlayer = true;
            clickingAllowed = true;
        }
    }

    //Use BFS-style search to create tiles to show where the specific unit can move/attack based on the move/range stat
    public void CalcTiles(bool isMoveTile)
    {
        actionUI.SetActive(false);
        gridDrawer.SetActive(true);

        int row = GetRowFromWorldSpace(chosenLoc.y);
        int col = GetColFromWorldSpace(chosenLoc.x);

        int distance;
        if(isMoveTile)
        {
            distance = grid[row, col].residentObject.GetComponent<Unit>().move;
        }
        else
        {
            attacking = true;
            distance = grid[row, col].residentObject.GetComponent<Unit>().range;
        }
        
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

                if(level < distance)
                {
                    if (!currTile.northTaken)
                    {
                        currTile.northTaken = true;
                        if (PosInGrid(currRow - 1, currCol))
                        {     
                            if (grid[currRow - 1, currCol].resident.Equals("Empty") || grid[currRow - 1, currCol].resident.Equals("MoveTile") || !isMoveTile)
                            {
                                grid[currRow - 1, currCol].southTaken = true;
                                queue.Enqueue(grid[currRow - 1, currCol]);
                                nodesInNextLevel++;
                                if (grid[currRow - 1, currCol].resident.Equals("Empty") || !isMoveTile)
                                {
                                    MakeTile(currRow - 1, currCol, isMoveTile);
                                }
                            }
                        }
                    }
                    if (!currTile.westTaken)
                    {
                        currTile.westTaken = true;
                        if (PosInGrid(currRow, currCol - 1))
                        {
                            if (grid[currRow, currCol - 1].resident.Equals("Empty") || grid[currRow, currCol - 1].resident.Equals("MoveTile") || !isMoveTile)
                            {
                                grid[currRow, currCol - 1].eastTaken = true;
                                queue.Enqueue(grid[currRow, currCol - 1]);
                                nodesInNextLevel++;
                                if (grid[currRow, currCol - 1].resident.Equals("Empty") || !isMoveTile)
                                {
                                    MakeTile(currRow, currCol - 1, isMoveTile);
                                }
                            }
                        }
                    }
                    if (!currTile.southTaken)
                    {
                        currTile.southTaken = true;
                        if (PosInGrid(currRow + 1, currCol))
                        {
                            if (grid[currRow + 1, currCol].resident.Equals("Empty") || grid[currRow + 1, currCol].resident.Equals("MoveTile") || !isMoveTile)
                            {
                                grid[currRow + 1, currCol].northTaken = true;
                                queue.Enqueue(grid[currRow + 1, currCol]);
                                nodesInNextLevel++;
                                if (grid[currRow + 1, currCol].resident.Equals("Empty") || !isMoveTile)
                                {
                                    MakeTile(currRow + 1, currCol, isMoveTile);
                                }
                            }
                        }
                    }
                    if (!currTile.eastTaken)
                    {
                        currTile.eastTaken = true;
                        if (PosInGrid(currRow, currCol + 1))
                        {
                            if (grid[currRow, currCol + 1].resident.Equals("Empty") || grid[currRow, currCol + 1].resident.Equals("MoveTile") || !isMoveTile)
                            {
                                grid[currRow, currCol + 1].westTaken = true;
                                queue.Enqueue(grid[currRow, currCol + 1]);
                                nodesInNextLevel++;
                                if (grid[currRow, currCol + 1].resident.Equals("Empty") || !isMoveTile)
                                {
                                    MakeTile(currRow, currCol + 1, isMoveTile);
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

    private void MakeTile(int row, int col, bool isMoveTile)
    {
        if(!grid[row, col].visited)
        {
            float x = GetWorldXFromCol(col);
            float y = GetWorldYFromRow(row);

            if (isMoveTile)
            {
                grid[row, col].resident = "MoveTile";
                grid[row, col].residentObject = (GameObject)Instantiate(playerMoveTile, new Vector3(x, y, 0), Quaternion.identity);
                moveTileCoords.Add(new Vector2(row, col));
            }
            else
            {
                attackTiles[new Vector2(GetRowFromWorldSpace(y), GetColFromWorldSpace(x))] = 
                    (GameObject)Instantiate(enemyAttackTile, new Vector3(x, y, 0), Quaternion.identity);
            }
            grid[row, col].visited = true;
        }
    }

    private void RemoveTiles(bool isMoveTile)
    {
        if (isMoveTile)
        {
            for (int i = 0; i < moveTileCoords.Count; i++)
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
        else
        {
            ICollection keys = attackTiles.Keys;
            foreach(Vector2 position in keys)
            {
                int row = (int)position.x;
                int col = (int)position.y;
                grid[row, col].Reset();
            }

            ICollection values = attackTiles.Values;
            foreach (GameObject tile in values)
            {
                Destroy(tile);
            }
            attackTiles = new Dictionary<Vector2, GameObject>();
        }
                
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
                //no obstacles, unless its the target
                if (grid[row - 1, col].resident.Equals("Empty") || (row - 1 == (int)end.x && col == (int)end.y))
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
                if (grid[row + 1, col].resident.Equals("Empty") || (row + 1 == (int)end.x && col == (int)end.y))
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
                if (grid[row, col - 1].resident.Equals("Empty") || (row == (int)end.x && col - 1 == (int)end.y))
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
                if (grid[row, col + 1].resident.Equals("Empty") || (row == (int)end.x && col + 1 == (int)end.y))
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

    private void CheckAttack(int row, int col)
    {
        //is a unit
        if (!grid[row, col].resident.Equals("Empty") && !grid[row, col].resident.Equals("MoveTile") && !grid[row, col].resident.Equals("Obstacle"))
        {
            //is an enemy
            if (!grid[row, col].isPlayer)
            {
                //enemy within range
                if(attackTiles.ContainsKey(new Vector2(row, col)))
                {
                    int playerRow = GetRowFromWorldSpace(chosenLoc.y);
                    int playerCol = GetColFromWorldSpace(chosenLoc.x);

                    Battle(playerRow, playerCol, row, col);

                    if (grid[playerRow, playerCol].isPlayer)
                    {
                        grid[playerRow, playerCol].residentObject.GetComponent<Unit>().SetActed(true);
                    }
                }
            }
        }

        attacking = false;
        gridDrawer.SetActive(false);
        RemoveTiles(false);
    }

    private void Battle(int playerRow, int playerCol, int enemyRow, int enemyCol)
    {
        int range = ManhattanDistance(new Vector2(playerRow, playerCol), new Vector2(enemyRow, enemyCol));
        GameObject player = grid[playerRow, playerCol].residentObject;
        GameObject enemy = grid[enemyRow, enemyCol].residentObject;
        Unit playerScript = player.GetComponent<Unit>();
        Unit enemyScript = enemy.GetComponent<Unit>();

        bool playerProjectile = grid[playerRow, playerCol].resident.Equals("Mage") || grid[playerRow, playerCol].resident.Equals("Cannoneer");
        bool enemyProjectile = grid[enemyRow, enemyCol].resident.Equals("Mage") || grid[enemyRow, enemyCol].resident.Equals("Cannoneer");
        bool playerMagic = playerScript.magic > playerScript.attack;
        bool enemyMagic = enemyScript.magic > enemyScript.attack;

        //enemy can counterattack
        if(enemyScript.range >= range)
        {
            //one with higher speed attacks first, if equal, attacking party goes first
            if(playerScript.speed >= enemyScript.speed)
            {
                //player turn
                StartCoroutine(Attack(player, enemy, playerProjectile, false));

                //player deal damage to enemy
                if (playerMagic)
                {
                    enemyScript.Damage(playerScript.magic, playerMagic);
                }
                else
                {
                    enemyScript.Damage(playerScript.attack, playerMagic);
                }

                //enemy turn
                if (enemyScript.GetCurrHealth() > 0)
                {
                    StartCoroutine(Attack(enemy, player, enemyProjectile, true));

                    //enemy deal damage to player
                    if (enemyMagic)
                    {
                        playerScript.Damage(enemyScript.magic, enemyMagic);
                    }
                    else
                    {
                        playerScript.Damage(enemyScript.attack, enemyMagic);
                    }

                    if(playerScript.GetCurrHealth() == 0)
                    {
                        UnitDeath(playerRow, playerCol);
                    }
                }
                else
                {
                    UnitDeath(enemyRow, enemyCol);
                }
                
            }
            //enemy attacking first
            else
            {
                //enemy turn
                StartCoroutine(Attack(enemy, player, enemyProjectile, false));

                //enemy deal damage to player
                if (enemyMagic)
                {
                    playerScript.Damage(enemyScript.magic, enemyMagic);
                }
                else
                {
                    playerScript.Damage(enemyScript.attack, enemyMagic);
                }

                //player turn
                if (playerScript.GetCurrHealth() > 0)
                {
                    StartCoroutine(Attack(player, enemy, playerProjectile, true));

                    //player deal damage to enemy
                    if (playerMagic)
                    {
                        enemyScript.Damage(playerScript.magic, playerMagic);
                    }
                    else
                    {
                        enemyScript.Damage(playerScript.attack, playerMagic);
                    }

                    if (enemyScript.GetCurrHealth() == 0)
                    {
                        UnitDeath(enemyRow, enemyCol);
                    }
                }
                else
                {
                    UnitDeath(playerRow, playerCol);
                }
            }
        }
        //no counterattacks
        else
        {
            StartCoroutine(Attack(player, enemy, playerProjectile, false));

            //player deal damage to enemy
            if (playerMagic)
            {
                enemyScript.Damage(playerScript.magic, playerMagic);
            }
            else
            {
                enemyScript.Damage(playerScript.attack, playerMagic);
            }

            if (enemyScript.GetCurrHealth() == 0)
            {
                UnitDeath(enemyRow, enemyCol);
            }
        }
    }

    private IEnumerator Attack(GameObject attacking, GameObject defending, bool projectile, bool isSecond)
    {
        clickingAllowed = false;

        if(isSecond)
        {
            yield return new WaitForSeconds(1f);
        }

        if(projectile)
        {
            Instantiate(explosion, defending.transform.position, Quaternion.identity);
        }
        else
        {
            Vector3 original = attacking.transform.position;
            Vector3 interval = (defending.transform.position - attacking.transform.position) * (0.1f);
            for (int i = 0; i < 10; i++)
            {
                attacking.transform.position = attacking.transform.position + interval;
                yield return new WaitForSeconds(0.001f);
            }
            for (int i = 0; i < 10; i++)
            {
                attacking.transform.position = attacking.transform.position - interval;
                yield return new WaitForSeconds(0.001f);
            }

            
        }

        yield return new WaitForSeconds(1f);

        if (playerPhase)
        {
            clickingAllowed = true;
        }
    }

    private void UnitDeath(int row, int col)
    {
        grid[row, col].resident = "Empty";

        if(grid[row, col].isPlayer)
        {
            for(int i = 0; i < players.Count; i++)
            {
                if(grid[row, col].residentObject == players[i])
                {
                    players.RemoveAt(i);
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (grid[row, col].residentObject == enemies[i])
                {
                    enemies.RemoveAt(i);
                    break;
                }
            }
        }

        grid[row, col].isPlayer = false;
        StartCoroutine(grid[row, col].residentObject.GetComponent<Unit>().Death());
        grid[row, col].residentObject = null;
    }

    private void CheckHeal(int row, int col)
    {
        int prevRow = GetRowFromWorldSpace(chosenLoc.y);
        int prevCol = GetColFromWorldSpace(chosenLoc.x);

        if (grid[prevRow, prevCol].resident.Equals("Ambulance"))
        {
            //is a unit
            if (!grid[row, col].resident.Equals("Empty") && !grid[row, col].resident.Equals("MoveTile") && !grid[row, col].resident.Equals("Obstacle"))
            {
                //is an ally
                if (grid[row, col].isPlayer)
                {
                    //ally within range
                    if (healTiles.ContainsKey(new Vector2(row, col)))
                    {
                        StartCoroutine(grid[row, col].residentObject.GetComponent<Unit>().Heal(15));

                        grid[prevRow, prevCol].residentObject.GetComponent<Unit>().SetActed(true);
                    }
                }
            }
        }

        healing = false;
        gridDrawer.SetActive(false);

        ICollection values = healTiles.Values;
        foreach(GameObject tile in values)
        {
            Destroy(tile);
        }
        healTiles = new Dictionary<Vector2, GameObject>();
    }

    public void HealTiles()
    {
        int row = GetRowFromWorldSpace(chosenLoc.y);
        int col = GetColFromWorldSpace(chosenLoc.x);

        gridDrawer.SetActive(true);
        healing = true;

        if(PosInGrid(row - 1, col))
        {
            float x = GetWorldXFromCol(col);
            float y = GetWorldYFromRow(row - 1);

            healTiles[new Vector2(row - 1, col)] = (GameObject)Instantiate(enemyAttackTile, new Vector3(x, y, 0), Quaternion.identity);
        }
        if (PosInGrid(row + 1, col))
        {
            float x = GetWorldXFromCol(col);
            float y = GetWorldYFromRow(row + 1);

            healTiles[new Vector2(row + 1, col)] = (GameObject)Instantiate(enemyAttackTile, new Vector3(x, y, 0), Quaternion.identity);
        }
        if (PosInGrid(row, col - 1))
        {
            float x = GetWorldXFromCol(col - 1);
            float y = GetWorldYFromRow(row);

            healTiles[new Vector2(row, col - 1)] = (GameObject)Instantiate(enemyAttackTile, new Vector3(x, y, 0), Quaternion.identity);
        }
        if (PosInGrid(row, col + 1))
        {
            float x = GetWorldXFromCol(col + 1);
            float y = GetWorldYFromRow(row);

            healTiles[new Vector2(row, col + 1)] = (GameObject)Instantiate(enemyAttackTile, new Vector3(x, y, 0), Quaternion.identity);
        }
    }

    public void EndPlayerTurn()
    {
        actionUI.SetActive(false);
        actionUI.transform.GetChild(2).gameObject.SetActive(false);

        //return previous chosen player to normal color
        int prevRow = GetRowFromWorldSpace(chosenLoc.y);
        int prevCol = GetColFromWorldSpace(chosenLoc.x);

        //check if previous character has died
        if (grid[prevRow, prevCol].isPlayer)
        {
            Color normal = new Color(1, 1, 1, 1);
            grid[prevRow, prevCol].residentObject.GetComponent<SpriteRenderer>().color = normal;
        }

        for (int i = 0; i < players.Count; i++)
        {
            Unit unitScript = players[i].GetComponent<Unit>();
            unitScript.SetActed(false);
            unitScript.SetMoved(false);
        }

        clickingAllowed = false;
        StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        playerPhase = false;
        StartCoroutine(PhaseIntro("Enemy Turn"));
        yield return new WaitForSeconds(2f);

        //go through all enemy actions, go backwards because an enemy might die and be removed from list
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            int enemyRow = GetRowFromWorldSpace(enemies[i].transform.position.y);
            int enemyCol = GetColFromWorldSpace(enemies[i].transform.position.x);

            //if ambulance unit, check if other enemies are injured, if so heal, else do nothing
            if(grid[enemyRow, enemyCol].resident.Equals("Ambulance"))
            {
                StartCoroutine(EnemyAmbulanceAI(enemyRow, enemyCol, i));
            }
            //otherwise, check each player position and go towards nearest one to attack
            else
            {
                StartCoroutine(EnemyAttackerAI(enemyRow, enemyCol));
            }

            yield return new WaitForSeconds(2f); //time between each enemy move
        }

        if (players.Count > 0)
        {
            StartCoroutine(PhaseIntro("Player Turn"));
            yield return new WaitForSeconds(2f);
            clickingAllowed = true;
            playerPhase = true;
        }
    }

    //priority is balance between distance away and amount of health missing
    //zero priority is ally not missing health
    private IEnumerator EnemyAmbulanceAI(int ambRow, int ambCol, int ambIndex)
    {
        PriorityQueue queue = new PriorityQueue();

        //figure out which allies to consider
        for(int i = 0; i < enemies.Count; i++)
        {
            if(i != ambIndex)
            {
                Unit unit = enemies[i].GetComponent<Unit>();
                int missingHealth = unit.health - unit.GetCurrHealth();

                //don't bother with those that don't need healing
                if(missingHealth == 0)
                {
                    continue;
                }

                int row = GetRowFromWorldSpace(enemies[i].transform.position.y);
                int col = GetColFromWorldSpace(enemies[i].transform.position.x);

                List<Vector2> path = CalcPath(new Vector2(ambRow, ambCol), new Vector2(row, col));

                int cost = path.Count - missingHealth; //distance away (cost) - missing health (urgency)

                //unreachable
                if (path.Count == 0)
                {
                    cost += 1000;
                }

                queue.Insert(new Vector3(row, col, cost));
            }
        }

        //if there are any allies that need healing
        if(queue.Count() > 0)
        {
            Vector3 chosenAlly = queue.RemoveMin();
            List<Vector2> path = CalcPath(new Vector2(ambRow, ambCol), new Vector2(chosenAlly.x, chosenAlly.y));

            Unit ambulance = grid[ambRow, ambCol].residentObject.GetComponent<Unit>();
            int move = ambulance.move;
            int range = ambulance.range;

            //unreachable
            if(path.Count == 0)
            {
                yield break;
            }

            //path is longer than move + range
            //subtract 1 since path includes start position
            if(path.Count - 1 > move + range)
            {
                //number of elements to remove from path, note that we subtract one to exclude starting position since FollowPath accounts for it
                int toRemove = path.Count - move - 1;
                path.RemoveRange(move + 1, toRemove);

                StartCoroutine(FollowPath(path, ambRow, ambCol));
            }
            //path is shorter than or equal to move + range
            else
            {
                //don't move on top of ally
                path.RemoveAt(path.Count - 1);
                StartCoroutine(FollowPath(path, ambRow, ambCol));

                //wait to move into position before healing
                yield return new WaitForSeconds(1f);

                int row = (int)chosenAlly.x;
                int col = (int)chosenAlly.y;
                StartCoroutine(grid[row, col].residentObject.GetComponent<Unit>().Heal(15));
            }
        }
    }

    private IEnumerator EnemyAttackerAI(int enemyRow, int enemyCol)
    {
        PriorityQueue queue = new PriorityQueue();

        for (int i = 0; i < players.Count; i++)
        {
            Unit unit = players[i].GetComponent<Unit>();
            int row = GetRowFromWorldSpace(players[i].transform.position.y);
            int col = GetColFromWorldSpace(players[i].transform.position.x);

            List<Vector2> path = CalcPath(new Vector2(enemyRow, enemyCol), new Vector2(row, col));

            int cost = (path.Count * 2) + unit.GetCurrHealth(); //distance away and how much health remaining to defeat. prioritize closer enemies

            //unreachable
            if(path.Count == 0)
            {
                cost += 1000;
            }

            queue.Insert(new Vector3(row, col, cost));
        }

        if(queue.Count() > 0)
        {
            Vector3 chosenTarget = queue.RemoveMin();
            List<Vector2> path = CalcPath(new Vector2(enemyRow, enemyCol), new Vector2(chosenTarget.x, chosenTarget.y));

            //unreachable
            if (path.Count == 0)
            {
                yield break;
            }

            Unit enemy = grid[enemyRow, enemyCol].residentObject.GetComponent<Unit>();
            int move = enemy.move;
            int range = enemy.range;

            //path is longer than move + range
            //subtract 1 since path includes start position
            if (path.Count - 1 > move + range)
            {
                //number of elements to remove from path, note that we subtract one to exclude starting position since FollowPath accounts for it
                int toRemove = path.Count - move - 1;
                path.RemoveRange(move + 1, toRemove);

                StartCoroutine(FollowPath(path, enemyRow, enemyCol));
            }
            //path is greater than range, need to move to attack
            else if(path.Count - 1 > range)
            {
                //attack as far away as possible
                path.RemoveRange(path.Count - range, range);

                StartCoroutine(FollowPath(path, enemyRow, enemyCol));

                //wait to move into position before atttack
                yield return new WaitForSeconds(1f);

                int newEnemyRow = (int)path[path.Count - 1].x;
                int newEnemyCol = (int)path[path.Count - 1].y;
                int row = (int)chosenTarget.x;
                int col = (int)chosenTarget.y;
                Battle(newEnemyRow, newEnemyCol, row, col);
            }
            //path is equal to or less than range, no need to move to attack
            else
            {
                int row = (int)chosenTarget.x;
                int col = (int)chosenTarget.y;
                Battle(enemyRow, enemyCol, row, col);
            }
        }
    }

    private IEnumerator GameOver(bool win)
    {
        yield return new WaitForSeconds(2f);

        gameOverUI.SetActive(true);

        if(win)
        {
            gameOverUI.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Victory";
        }
        else
        {
            gameOverUI.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Defeat";
        }
    }

    public void ReturnToTitle()
    {
        SceneManager.LoadScene(0);
    }
}