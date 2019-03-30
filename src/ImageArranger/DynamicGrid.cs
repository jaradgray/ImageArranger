using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ImageArranger
{
    struct Column
    {
        public double Width { get; set; }
        public Column(double width) { Width = width; }
    }

    struct Row
    {
        public double Height { get; set; }
        public Row(double height) { Height = height; }
    }

    struct Cell
    {
        public int Index { get; set; }
        public bool Occupied { get; set; }
        public Cell(int index, bool occupied)
        {
            Index = index;
            Occupied = occupied;
        }
        public Cell(Cell toCopy)
        {
            Index = toCopy.Index;
            Occupied = toCopy.Occupied;
        }

        override public string ToString()
        {
            string s = "";
            s += $"Index: {Index}\n";
            s += $"Occupied: {Occupied}";
            return s;
        }
    }

    class DynamicGrid
    {
        // Properties
        public Rect Bounds { get; set; }
        public int NumCols { get; set; }
        public int NumRows { get; set; }
        public int RectCount { get; set; }


        // Instance Variables
        private List<Column> cols;
        private List<Row> rows;
        private List<List<Cell>> cells;


        // Constructors

        public DynamicGrid(Rect bounds)
        {
            cols = new List<Column>();
            rows = new List<Row>();
            cells = new List<List<Cell>>();

            Bounds = bounds;
            NumCols = 1;
            NumRows = 1;
            RectCount = 0;

            cols.Add(new Column(bounds.Width));
            rows.Add(new Row(bounds.Height));

            Cell cell = new Cell(-1, false);
            cells = new List<List<Cell>>();
            cells.Insert(0, new List<Cell>());
            cells[0].Insert(0, cell);
        }


        // Methods

        public bool IsCellFree(int col, int row)
        {
            return !cells[col][row].Occupied;
        }

        public double GetRowHeight(int row)
        {
            if (row >= rows.Count) return -1.0;
            return rows[row].Height;
        }

        public double GetColWidth(int col)
        {
            if (col >= cols.Count) return -1.0;
            return cols[col].Width;
        }

        /// <summary>
        /// Given a Rect and column and row indexes, update this DynamicGrid's state
        /// to reflect placing the Rect at the given position and slicing the grid along
        /// the placed Rect's bottom and right edges.
        /// 
        /// Precondition: rect can be successfully placed at [col, row]
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="col"></param>
        /// <param name="row"></param>
        public void InsertRect(Rect rect, int col, int row)
        {
            // DETERMINE HOW MANY COLUMNS AND ROWS THE RECT TO BE PLACED WILL OCCUPY
            double accommodatedWidth = 0.0;
            int colSpan = 0;
            while (accommodatedWidth < rect.Width)
            {
                accommodatedWidth += cols[col + colSpan].Width;
                colSpan++;
            }
            double accommodatedHeight = 0.0;
            int rowSpan = 0;
            while (accommodatedHeight < rect.Height)
            {
                accommodatedHeight += rows[row + rowSpan].Height;
                rowSpan++;
            }

            // INSERT APPROPRIATE COLUMNS, ROWS, AND CELLS
            bool colAdded = false;
            bool rowAdded = false;
            if (Math.Abs(accommodatedWidth - rect.Width) > 1.0 && Math.Abs(accommodatedHeight - rect.Height) > 1.0)
            {
                // INSERT A NEW COLUMN AND A NEW ROW
                // cols and rows
                Column newCol = new Column(accommodatedWidth - rect.Width);
                cols[col + colSpan - 1] = new Column(cols[col + colSpan - 1].Width - newCol.Width);
                cols.Insert(col + colSpan, newCol);
                NumCols++;
                Row newRow = new Row(accommodatedHeight - rect.Height);
                rows[row + rowSpan - 1] = new Row(rows[row + rowSpan - 1].Height - newRow.Height);
                rows.Insert(row + rowSpan, newRow);
                NumRows++;
                // cells
                List<Cell> newCells = new List<Cell>();
                for (int i = 0; i < NumRows - 1; i++)
                {
                    newCells.Add(new Cell(-1, false));
                }
                cells.Insert(col + colSpan, newCells);
                for (int i = 0; i < NumCols; i++)
                {
                    cells[i].Insert(row + rowSpan, new Cell(-1, false));
                }
                colAdded = true;
                rowAdded = true;
            }
            else if (Math.Abs(accommodatedWidth - rect.Width) > 1.0)
            {
                // INSERT A NEW COLUMN
                // cells
                List<Cell> newCells = new List<Cell>();
                for (int i = 0; i < NumRows; i++)
                {
                    newCells.Add(new Cell(-1, false));
                }
                cells.Insert(col + colSpan, newCells);
                // cols
                Column newCol = new Column(accommodatedWidth - rect.Width);
                cols[col + colSpan - 1] = new Column(cols[col + colSpan - 1].Width - newCol.Width);
                cols.Insert(col + colSpan, newCol);
                NumCols++;
                colAdded = true;
            }
            else if (Math.Abs(accommodatedHeight - rect.Height) > 1.0)
            {
                // INSERT A NEW ROW
                // cells
                for (int i = 0; i < NumCols; i++)
                {
                    cells[i].Insert(row + rowSpan, new Cell(-1, false));
                }
                // rows
                Row newRow = new Row(accommodatedHeight - rect.Height);
                rows[row + rowSpan - 1] = new Row(rows[row + rowSpan - 1].Height - newRow.Height);
                rows.Insert(row + rowSpan, newRow);
                NumRows++;
                rowAdded = true;
            }

            // UPDATE PROPERTIES OF AFFECTED CELLS
            // newly-added Cells
            if (rowAdded)
            {
                for (int i = 0; i < cells.Count; i++)
                {
                    cells[i][row + rowSpan] = new Cell(cells[i][row + rowSpan - 1]);
                }
            }
            if (colAdded)
            {
                for (int i = 0; i < NumRows; i++)
                {
                    cells[col + colSpan][i] = new Cell(cells[col + colSpan - 1][i]);
                }
            }
            // Cells occupied by placed Rect
            for (int c = col; c < col + colSpan; c++)
            {
                for (int r = row; r < row + rowSpan; r++)
                {
                    cells[c][r] = new Cell(RectCount, true);
                }
            }
            RectCount++;
        }//end InsertRect()

        /// <summary>
        /// Resizes Bounds such that its shortest dimensions is reduced by 1px and its aspect ratio is maintained.
        /// </summary>
        /// <returns>True if Bounds was successfully resized, false otherwise</returns>
        public bool ShrinkBounds()
        {
            double newW, newH;
            // reduce the limiting dimension by 1
            if (Bounds.Width / Bounds.Height >= 1)
            {
                // reduce Height by 1.0, Width by more than 1.0
                newH = Bounds.Height - 1.0;
                newW = Bounds.Width * newH / Bounds.Height;
            }
            else
            {
                // reduce Width by 1.0, Height by more than 1.0
                newW = Bounds.Width - 1.0;
                newH = Bounds.Height * newW / Bounds.Width;
            }
            if (newW < 0 || newH < 0) return false;
            Bounds = new Rect(0.0, 0.0, newW, newH);
            return true;
        }//end ShrinkBounds()

        /// <summary>
        /// Given a Size, scales Bounds to as large as possible to fit within that
        /// Size, and scales each Column.Width and Row.Height by the same factor.
        /// </summary>
        /// <param name="size"></param>
        public void ScaleTo(Size size)
        {
            // set scale factor based on size's limiting dimension
            // E.g. smaller aspect (w/h) means narrower size, so Width is the limiting dimension
            double scaleFactor = ((size.Width / size.Height) <= (Bounds.Width / Bounds.Height)) ?
                size.Width / Bounds.Width :
                size.Height / Bounds.Height;

            Bounds = new Rect(0.0, 0.0, Bounds.Width * scaleFactor, Bounds.Height * scaleFactor);
            for (int i = 0; i < cols.Count; i++)
                cols[i] = new Column(cols[i].Width * scaleFactor);
            for (int i = 0; i < rows.Count; i++)
                rows[i] = new Row(rows[i].Height * scaleFactor);
        }

        /// <summary>
        /// Given the number of Rects represented in this DynamicGrid, returns a
        /// List of Rects whose properties reflect the grid's state, ordered by
        /// the order in which they were added to the grid.
        /// </summary>
        /// <returns></returns>
        public List<Rect> GetRects(int numRects)
        {
            List<Rect> result = new List<Rect>();
            for (int i = 0; i < numRects; i++)
                result.Add(new Rect());
            int index;
            List<int> visitedIndexes = new List<int>();
            for (int col = 0; col < cells.Count; col++)
            {
                for (int row = 0; row < cells[col].Count; row++)
                {
                    Cell cell = cells[col][row];
                    if (cell.Occupied && !visitedIndexes.Contains(cell.Index))
                    {
                        index = cell.Index;
                        visitedIndexes.Add(index);
                        // create the Rect who occupies this Cell and add it to result
                        // Note: a Rect can occupy more than one Cell
                        double x, y, w, h;
                        x = y = w = h = 0.0;
                        // the Rect's position
                        if (col > 0)
                        {
                            for (int i = 0; i < col; i++)
                                x += cols[i].Width;
                        }
                        if (row > 0)
                        {
                            for (int i = 0; i < row; i++)
                                y += rows[i].Height;
                        }
                        // the Rect's Width
                        w += cols[col].Width;
                        // check right neighbors
                        int nextCol = col + 1;
                        while (nextCol < cols.Count && cells[nextCol][row].Index == cell.Index)
                        {
                            w += cols[nextCol].Width;
                            nextCol++;
                        }
                        // the Rect's Height
                        h += rows[row].Height;
                        // check bottom neighbors
                        int nextRow = row + 1;
                        while (nextRow < rows.Count && cells[col][nextRow].Index == cell.Index)
                        {
                            h += rows[nextRow].Height;
                            nextRow++;
                        }
                        result[index] = new Rect(x, y, w, h);
                    }//end if
                }//end for row
            }//end for col
            return result;
        }//end GetRects()

        /// <summary>
        /// Returns a new DynamicGrid whose state is the same as this one's
        /// </summary>
        /// <returns></returns>
        public DynamicGrid GetCopy()
        {
            DynamicGrid result = new DynamicGrid(Bounds);
            result.cols.Clear();
            result.rows.Clear();
            result.cells.Clear();

            // NumCols, NumRows, RectCount
            result.NumCols = NumCols;
            result.NumRows = NumRows;
            result.RectCount = RectCount;

            // cols, rows, cells
            foreach (Column col in cols)
                result.cols.Add(new Column(col.Width));
            foreach (Row row in rows)
                result.rows.Add(new Row(row.Height));
            for (int col = 0; col < NumCols; col++)
            {
                List<Cell> newRow = new List<Cell>();
                for (int row = 0; row < NumRows; row++)
                {
                    newRow.Add(new Cell(cells[col][row]));
                }
                result.cells.Add(newRow);
            }

            return result;
        }

        override public string ToString()
        {
            string s = "\n";
            s += $"Bounds position: ({Bounds.X}, {Bounds.Y})\n";
            s += $"Bounds.Width: {Bounds.Width}\n";
            s += $"Bounds.Height: {Bounds.Height}\n";

            s += "COLUMNS:\n";
            for (int col = 0; col < NumCols; col++)
            {
                s += $"col {col} width: {cols[col].Width}\n";
            }

            s += "ROWS:\n";
            for (int row = 0; row < NumRows; row++)
            {
                s += $"row {row} height: {rows[row].Height}\n";
            }

            s += "CELLS:\n";
            for (int col = 0; col < NumCols; col++)
            {
                for (int row = 0; row < NumRows; row++)
                {
                    s += $"cells[{col}][{row}]:\n\t{cells[col][row].ToString()}\n";
                }
            }
            return s;
        }
    }
}
