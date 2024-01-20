using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempCell : MonoBehaviour
{
    public int X;
    public int Y;

    public List<TempCell> prev = new List<TempCell>();
    public List<TempCell> next = new List<TempCell>();
    public void Reset(int x, int y)
    {
        prev = new List<TempCell>();

        next = new List<TempCell>();
        X = x;
        Y = y;
    }
}
