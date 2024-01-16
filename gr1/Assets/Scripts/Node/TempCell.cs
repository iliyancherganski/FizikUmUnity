using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempCell : MonoBehaviour
{
    public int X;
    public int Y;

    public List<TempCell> prev = new List<TempCell>();
    public List<TempCell> next = new List<TempCell>();
    public TempCell()
    {
    }
    public TempCell(int x, int y)
    {
        X = x;
        Y = y;
    }
}
