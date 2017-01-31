using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile
{
    public int row;
    public int col;

    public bool isPlayer = false;
    public string resident = "Empty";
    public GameObject residentObject;

    public bool visited = false;
    public bool northTaken = false;
    public bool westTaken = false;
    public bool southTaken = false;
    public bool eastTaken = false;

    public GridTile(int row, int col)
    {
        this.row = row;
        this.col = col;
    }

    public void Reset()
    {
        visited = false;
        northTaken = false;
        westTaken = false;
        southTaken = false;
        eastTaken = false;
    }
}
