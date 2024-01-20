using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Transactions;
using System.Xml;
using UnityEngine;

public class GridElectricityFlow : MonoBehaviour
{
    public GridConnectionNode grid;

    // List of batteries -> First GC is for +charge, the second one is for -charge
    //public List<Tuple<GridCell, GridCell>> cellsWithBattery = new List<Tuple<GridCell, GridCell>>();
    public List<GridCell> cellsWithBattery = new List<GridCell>();
    public List<GridCell> cellsWithLightbulbs = new List<GridCell>();

    List<GridCell> batteryFirstCables = new List<GridCell>();
    List<GridCell> batteryLastCables = new List<GridCell>();

    public string _Console;

    public void GridCheck()
    {
        FindInGrid();
        ResetAllTC();

        if (cellsWithBattery.Count == 0)
        {
            _Console = "No batteries";
            return;
        }
        foreach (var battery in cellsWithBattery)
        {
            battery.tempCell.Reset(battery.GridIndex_X, battery.GridIndex_Y);
            var c1 = FirstCableAfterBattery(battery);
            if (c1 != null)
            {
                c1.tempCell.Reset(c1.GridIndex_X, c1.GridIndex_Y);
            }
            batteryFirstCables.Add(c1);
            var c2 = LastCableAfterBattery(battery);
            if (c2 != null)
            {
                c2.tempCell.Reset(c2.GridIndex_X, c2.GridIndex_Y);
            }
            batteryLastCables.Add(c2);
        }

        // FIRST BATTERY ONLY
        var listOfCells = new List<GridCell>()
        {
            batteryFirstCables[0]
        };

        bool isConnected = CanConnect(batteryFirstCables[0], cellsWithBattery[0], listOfCells, cellsWithBattery[0]);

        if (isConnected)
        {
            _Console = "connected";
        }
        else
        {
            _Console = "not connected";
        }

        foreach (var lightbulb in cellsWithLightbulbs)
        {
            if (lightbulb.tempCell == null)
            {
                continue;
            }
            if (lightbulb.tempCell.prev.Count > 0
                && lightbulb.tempCell.next.Count > 0)
            {
                lightbulb.connectionNode.InstantiateLightbulb_ON_or_OFF(true);
            }
            else
            {
                lightbulb.connectionNode.InstantiateLightbulb_ON_or_OFF(false);
            }
        }
    }

    public bool CanConnect(GridCell current, GridCell previous, List<GridCell> cellsWithMultipleConnections, GridCell battery)
    {
        if (current == null || previous == null)
        {
            return false;
        }
/*        if (current.tempCell == null)
            current.tempCell.Reset(current.GridIndex_X, current.GridIndex_Y);

        if (previous.tempCell == null)
            previous.tempCell.Reset(previous.GridIndex_X, previous.GridIndex_Y);*/
        
        if(!previous.tempCell.next.Contains(current.tempCell))
            previous.tempCell.next.Add(current.tempCell);

        if(!current.tempCell.next.Contains(previous.tempCell))
            current.tempCell.prev.Add(previous.tempCell);

        if (current == LastCableAfterBattery(battery))
        {
            return true;
        }

        var SurroundingCells = FindSurroundingCells(current)
            .Where(x => x != null
                     && x != previous
                     && x.connectionNode.ItemType != ItemType.None).ToList();

        if (SurroundingCells.Count == 0)
        {
            return false;
        }
        /*else if(SurroundingCells.Count == 1)
        {
            return CanConnect(SurroundingCells.FirstOrDefault(), current, cellsWithMultipleConnections, battery);
        }*/

        bool hasConnection = false;

        var connections = new List<GridCell>();
        foreach (var c in cellsWithMultipleConnections)
        {
            connections.Add(c);
        }
        connections.Add(current);

        foreach (var cell in SurroundingCells)
        {
            bool currentIsConnected = false;

            if (connections.Contains(cell))
            {
                continue;
            }
            currentIsConnected = CanConnect(cell, current, connections, battery);
            if (currentIsConnected)
            {
                hasConnection = true;
            }
            else
            {
                current.tempCell.next.Remove(cell.tempCell);
                cell.tempCell.prev.Remove(current.tempCell);
            }
        }
        return hasConnection;

    }

    public void FindInGrid()
    {
        cellsWithBattery = new List<GridCell>();
        cellsWithLightbulbs = new List<GridCell>();
        foreach (var cell in grid.cells)
        {
            if (cell.connectionNode.ItemType == ItemType.Battery
                && cell.connectionNode.isPositive)
            {
                cellsWithBattery.Add(cell);
            }
            else if (cellsWithBattery.Contains(cell))
            {
                cellsWithBattery.Remove(cell);
            }
            else if (cell.connectionNode.ItemType == ItemType.Lightbulb)
            {
                cellsWithLightbulbs.Add(cell);
            }
        }
    }
    public void ResetAllTC()
    {
        foreach (var cell in grid.cells)
        {
            cell.tempCell.Reset(cell.GridIndex_X, cell.GridIndex_Y);
        }
    }
    public bool AlreadyHasConnectionWithTC(TempCell curr, TempCell cell)
    {
        bool foundAny = false;
        foreach (var item in curr.next)
        {
            if (item == cell)
            {
                return true;
            }
            /*else
            {
                foundAny = AlreadyHasConnectionWithTC(item, cell);
                if (foundAny)
                {
                    return true;
                }
            }*/
        }
        return foundAny;
    }

    public GridCell[] FindSurroundingCells(GridCell initial)
    {
        int x = initial.GridIndex_X;
        int y = initial.GridIndex_Y;

        GridCell[] cells = new GridCell[4];
        /*
                  0
                3 x 1
                  2
        */
        // UP
        if (x - 1 >= 0)
            cells[0] = grid.cells[x - 1, y];
        else
            cells[0] = null;

        // LEFT
        if (y - 1 >= 0)
            cells[1] = grid.cells[x, y - 1];
        else
            cells[1] = null;

        // DOWN
        if (x + 1 < grid.cells.GetLength(0))
            cells[2] = grid.cells[x + 1, y];
        else
            cells[2] = null;

        // RIGHT
        if (y + 1 < grid.cells.GetLength(1))
            cells[3] = grid.cells[x, y + 1];
        else
            cells[3] = null;

        return cells;
    }
    public GridCell FirstCableAfterBattery(GridCell battery)
    {
        //    1st   2nd
        //r0: x-1   x+2
        //r1: y-1   y+2
        //r2: x+1   x-2
        //r3: y+1   y-2
        GridCell cell = null;
        switch (grid.PlacementModeRotation)
        {
            case 0:
                cell = grid.cells[battery.GridIndex_X - 1, battery.GridIndex_Y];
                break;
            case 1:
                cell = grid.cells[battery.GridIndex_X, battery.GridIndex_Y - 1];
                break;
            case 2:
                cell = grid.cells[battery.GridIndex_X + 1, battery.GridIndex_Y];
                break;
            case 3:
                cell = grid.cells[battery.GridIndex_X, battery.GridIndex_Y + 1];
                break;
        }
        if (cell != null && cell.connectionNode.ItemType == ItemType.Cable)
        {
            return cell;
        }
        return null;
    }
    public GridCell LastCableAfterBattery(GridCell battery)
    {
        //    1st   2nd
        //r0: x-1   x+2
        //r1: y-1   y+2
        //r2: x+1   x-2
        //r3: y+1   y-2
        GridCell cell = null;
        switch (grid.PlacementModeRotation)
        {
            case 0:
                cell = grid.cells[battery.GridIndex_X + 2, battery.GridIndex_Y];
                break;
            case 1:
                cell = grid.cells[battery.GridIndex_X, battery.GridIndex_Y + 2];
                break;
            case 2:
                cell = grid.cells[battery.GridIndex_X - 2, battery.GridIndex_Y];
                break;
            case 3:
                cell = grid.cells[battery.GridIndex_X, battery.GridIndex_Y - 2];
                break;
        }
        if (cell != null && cell.connectionNode.ItemType == ItemType.Cable)
        {
            return cell;
        }
        return null;
    }

}
