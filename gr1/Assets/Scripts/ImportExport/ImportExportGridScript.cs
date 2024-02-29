using System.Collections.Generic;
using UnityEngine;

public class ImportExportGridScript : MonoBehaviour
{
    public string jsonExport;

    public void GridExport(GridConnectionNode grid)
    {
        GridObject obj = new GridObject(grid.GridSizeX, grid.GridSizeY);

        foreach (var cell in grid.cells)
        {
            if (cell.connectionNode.ItemType == ItemType.None)
            {
                continue;
            }
            if (cell.connectionNode.ItemType == ItemType.Cable || cell.connectionNode.ItemType == ItemType.Lightbulb)
            {
                print("Added cable/lightbulb to export");
                obj.cellObjects.Add(new GridObject.CellObject(cell.GridIndex_X, cell.GridIndex_Y, cell.connectionNode.ItemType));
            }
            else
            {
                if (cell.connectionNode.ItemType == ItemType.Battery && !cell.connectionNode.isPositive)
                {
                    continue;
                }
                print("Added battery/switch to export");
                obj.cellObjects.Add(new GridObject.CellObject(cell.GridIndex_X, cell.GridIndex_Y, cell.connectionNode.ItemType, cell.connectionNode.Rotation));
            }
        }

        // Serialize the entire GridObject
        jsonExport = JsonUtility.ToJson(obj);
        print("Grid exported successfully.");

        GUIUtility.systemCopyBuffer = jsonExport;
        print("Json copied on clipboard.");
    }

   /* public string GridImport()
    {

    }*/
}
