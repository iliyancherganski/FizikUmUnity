using System.Collections.Generic;

public class GridObject
{
    public int gridSize_x;
    public int gridSize_y;
    public List<CellObject> cellObjects;

    public GridObject(int gridSize_x, int gridSize_y)
    {
        this.gridSize_x = gridSize_x;
        this.gridSize_y = gridSize_y;
        cellObjects = new List<CellObject>();
    }

    [System.Serializable]
    public class CellObject
    {
        public int X, Y;

        public ItemType type;
        public int rotation = 0;
        public int interacted = 0;

        public CellObject(int x, int y, ItemType type)
        {
            X = x;
            Y = y;
            this.type = type;
        }

        public CellObject(int x, int y, ItemType type, int rotation)
        {
            X = x;
            Y = y;
            this.type = type;
            this.rotation = rotation;
        }

        public CellObject(int x, int y, ItemType type, int rotation, int interacted)
        {
            X = x;
            Y = y;
            this.type = type;
            this.rotation = rotation;
            this.interacted = interacted;
        }
    }
}