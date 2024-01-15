using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class GridElectricityFlow : MonoBehaviour
{
    public GridConnectionNode grid;

    // List of batteries -> First GC is for +charge, the second one is for -charge
    //public List<Tuple<GridCell, GridCell>> cellsWithBattery = new List<Tuple<GridCell, GridCell>>();
    public List<GridCell> cellsWithBattery = new List<GridCell>();

    public string _Console;

    public class TempCell
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


    public void GridCheck()
    {
        FindBatteries();

        if (cellsWithBattery.Count == 0)
        {
            _Console = "No batteries";
            return;
        }
        // first battery only
        var firstCable = FirstCableAfterBattery(cellsWithBattery[0]);
        TempCell batteryTC = new TempCell(cellsWithBattery[0].GridIndex_X, cellsWithBattery[0].GridIndex_Y);
        if (firstCable != null)
        {
            TempCell firstTC = new TempCell(firstCable.GridIndex_X, firstCable.GridIndex_Y);
            batteryTC.next.Add(firstTC);

            bool canConnect = CanConnect(firstTC, batteryTC, cellsWithBattery[0]);

            if (canConnect)
            {
                _Console = "connected";
            }
            else
            {
                _Console = "not connected";
            }
        }
        else
        {
            _Console = "not connected";
        }
    }
    public void FindBatteries()
    {
        cellsWithBattery = new List<GridCell>();
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
        }
    }
    public bool CanConnect(TempCell current, TempCell previous, GridCell battery)
    {
        current.prev.Add(previous);
        var surrCells = FindSurroundingCells(grid.cells[current.X, current.Y]);
        bool foundNext = false;
        for (int i = 0; i < 4; i++)
        {
            if (surrCells[i] == null
                || surrCells[i].connectionNode.ItemType == ItemType.None
                || current.prev
                    .Any(x => x.X == surrCells[i].GridIndex_X
                        && x.Y == surrCells[i].GridIndex_Y)
                || surrCells[i] == battery)
            {
                continue;
            }
            if (surrCells[i].connectionNode.batteryNode == battery.connectionNode)
            {
                var endCableToBattery = LastCableAfterBattery(battery);
                if (grid.cells[current.X, current.Y] == endCableToBattery)
                {
                    return true;
                }
            }
            else if (surrCells[i].connectionNode.ItemType == ItemType.Cable
                || surrCells[i].connectionNode.ItemType == ItemType.Lightbulb)
            {
                TempCell next = new TempCell(surrCells[i].GridIndex_X, surrCells[i].GridIndex_Y);
                bool alreadyHasConnection = AlreadyHasConnectionWithTC(current, next);
                if (alreadyHasConnection)
                {
                    continue;
                }
                current.next.Add(next);
                if (!foundNext)
                {
                    foundNext = CanConnect(next, current, battery);
                }
            }
        }
        if (foundNext)
        {
            return true;
        }
        return false;
    }

    public bool AlreadyHasConnectionWithTC(TempCell curr, TempCell cell)
    {
        foreach (var item in curr.next)
        {
            if (item == cell)
            {
                return true;
            }
            continue;
        }
        return false;
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

        /*for (int i = 0; i < 4; i++)
        {
            if (cells[i].connectionNode.ItemType == ItemType.None)
            {
                cells[i] = null;
            }
        }*/
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
