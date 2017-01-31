using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridDrawer : MonoBehaviour {

    public GameObject gameManager;
    private GameManager gameManagerScript;

    private int tilePixelWidth;
    private int mapPixelWidth;
    private int mapPixelHeight;
    private float pixelUnitRatio;

    private float mapWidth;
    private float mapHeight;

    public GameObject linePrefab;

	// Use this for initialization
	void Start () {
        gameManagerScript = gameManager.GetComponent<GameManager>();
        tilePixelWidth = gameManagerScript.tilePixelWidth;
        mapPixelWidth = gameManagerScript.mapPixelWidth;
        mapPixelHeight = gameManagerScript.mapPixelHeight;

        mapWidth = gameManagerScript.GetMapWidth();
        mapHeight = gameManagerScript.GetMapHeight();

        pixelUnitRatio = mapPixelWidth / mapWidth;

        DrawLines();
	}
	
	// Update is called once per frame
	void Update () {

	}

    private void DrawLines()
    {
        //draw rows
        int rows = mapPixelHeight / tilePixelWidth;

        for (int i = 1; i < rows; i++)
        {
            float y = (tilePixelWidth / pixelUnitRatio) * i + -(mapHeight / 2);
            Vector3 pointA = new Vector3(-(mapWidth / 2), y, -1);
            Vector3 pointB = new Vector3(mapWidth / 2 , y, - 1);

            GameObject line = (GameObject)Instantiate(linePrefab);
            line.transform.parent = this.gameObject.transform;

            LineRenderer lineRend = line.GetComponent<LineRenderer>();
            lineRend.SetPosition(0, pointA);
            lineRend.SetPosition(1, pointB);
        }

        //draw columns
        int columns = mapPixelWidth / tilePixelWidth;

        for (int i = 1; i < columns; i++)
        {
            float x = (tilePixelWidth / pixelUnitRatio) * i + -(mapWidth / 2);
            Vector3 pointA = new Vector3(x, -(mapHeight / 2), -1);
            Vector3 pointB = new Vector3(x, mapHeight / 2, -1);

            GameObject line = (GameObject)Instantiate(linePrefab);
            line.transform.parent = this.gameObject.transform;

            LineRenderer lineRend = line.GetComponent<LineRenderer>();
            lineRend.SetPosition(0, pointA);
            lineRend.SetPosition(1, pointB);
        }
    }
}
