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
    public int recursionCycling;


    public string _Console;

    public void GridCheck()
    {
        //print("entered");
        FindInGrid();
        ResetAllTC();

        if (grid.cellsWithBattery.Count == 0)
        {
            _Console = "No batteries";
            return;
        }
        grid.batteryFirstCables = new List<GridCell>();
        grid.batteryLastCables = new List<GridCell>();
        foreach (var battery in grid.cellsWithBattery)
        {
            battery.tempCell.Reset(battery.GridIndex_X, battery.GridIndex_Y);
            var c1 = grid.FirstCableAfterBattery(battery);
            if (c1 != null)
            {
                c1.tempCell.Reset(c1.GridIndex_X, c1.GridIndex_Y);
            }
            if (!grid.batteryFirstCables.Contains(c1))
            {
                grid.batteryFirstCables.Add(c1);
            }
            var c2 = grid.LastCableAfterBattery(battery);
            if (c2 != null)
            {
                c2.tempCell.Reset(c2.GridIndex_X, c2.GridIndex_Y);
            }
            if (!grid.batteryLastCables.Contains(c2))
            {
                grid.batteryLastCables.Add(c2);
            }
        }

        // FIRST BATTERY ONLY
        var listOfCells = new List<GridCell>()
        {
            grid.batteryFirstCables[0]
        };

        bool isConnected = CanConnect(grid.batteryFirstCables[0], grid.cellsWithBattery[0], listOfCells, grid.cellsWithBattery[0]);

        if (isConnected)
        {
            _Console = "connected";
        }
        else
        {
            _Console = "not connected";
        }

        RemoveDublicatedPrevsAndNexts();

        bool hasShortCurcuit = HasShortCircuit(grid.batteryFirstCables[0], new List<GridCell>(), grid.cellsWithBattery[0]);

        if (hasShortCurcuit)
        {
            print("HAS SHORT CIRCUT");
        }

        foreach (var lightbulb in grid.cellsWithLightbulbs)
        {
            if (lightbulb.tempCell == null)
            {
                continue;
            }
            if (lightbulb.tempCell.prev.Count > 0
                && lightbulb.tempCell.next.Count > 0
                && !hasShortCurcuit)
            {
                lightbulb.connectionNode.SetLightbulb_ON_or_OFF(true);
            }
            else
            {
                lightbulb.connectionNode.SetLightbulb_ON_or_OFF(false);
            }
        }

        foreach (var sw in grid.cellsWithSwitches)
        {
            if (sw.tempCell == null)
            {
                continue;
            }
            if (sw.tempCell.prev.Count > 0
                && sw.tempCell.next.Count > 0
                && !hasShortCurcuit)
            {
                sw.IfSwitchAndOn_TurnOnLight(true);
            }
            else
            {
                sw.IfSwitchAndOn_TurnOnLight(false);
            }
        }


    }

    public bool CanConnect(GridCell current, GridCell previous, List<GridCell> cellsWithMultipleConnections, GridCell battery)
    {
        if (current == null
            || previous == null
            || current.IsATurnedOffSwitch())
        {
            return false;
        }

        if (!previous.tempCell.next.Contains(current.tempCell))
            previous.tempCell.next.Add(current.tempCell);

        if (!current.tempCell.prev.Contains(previous.tempCell))
            current.tempCell.prev.Add(previous.tempCell);

        if (current == grid.LastCableAfterBattery(battery))
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

    public bool HasShortCircuit(GridCell current, List<GridCell> allPrevious, GridCell battery)
    {
        var connections = new List<GridCell>();
        foreach (var c in allPrevious)
        {
            connections.Add(c);
        }
        connections.Add(current);
        bool hasSC = false;
        if (current == grid.LastCableAfterBattery(battery))
        {
            if (connections
                .Where(x => x.connectionNode.ItemType == ItemType.Lightbulb
                || x.IsATurnedOffSwitch()).ToList().Count() == 0)
            {
                hasSC = true;
            }
        }
        foreach (var cell in current.tempCell.next)
        {
            if (HasShortCircuit(grid.cells[cell.X, cell.Y], connections, battery))
            {
                hasSC = true;
            }
        }
        return hasSC;
    }

    public void RemoveDublicatedPrevsAndNexts()
    {
        foreach (var cell in grid.cells)
        {
            if (cell.tempCell != null)
            {
                cell.tempCell.prev = cell.tempCell.prev.Distinct().ToList();
                cell.tempCell.next = cell.tempCell.next.Distinct().ToList();
            }
        }
    }

    public void FindInGrid()
    {
        grid.cellsWithBattery = new List<GridCell>();
        grid.cellsWithLightbulbs = new List<GridCell>();
        grid.cellsWithSwitches = new List<GridCell>();
        foreach (var cell in grid.cells)
        {
            if (cell.connectionNode.ItemType == ItemType.Battery
                && cell.connectionNode.isPositive)
            {
                grid.cellsWithBattery.Add(cell);
            }
            else if (grid.cellsWithBattery.Contains(cell))
            {
                grid.cellsWithBattery.Remove(cell);
            }
            else if (cell.connectionNode.ItemType == ItemType.Lightbulb)
            {
                grid.cellsWithLightbulbs.Add(cell);
            }
            else if (cell.connectionNode.ItemType == ItemType.Switch)
            {
                grid.cellsWithSwitches.Add(cell);
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
}
