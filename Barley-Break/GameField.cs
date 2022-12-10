using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barley_Break
{
    public class GameField
    {
        private Label[,] numbers;
        private PictureBox[,] cells;
        private PictureBox[,] map;
        private string startNumbers;

        private Color basicCellColor = Color.Orange;
        private Color correctCellColor = Color.ForestGreen;

        private int size = 4;

        private int randomSteps = 100;

        private int widthFormOffset = 50;
        private int heighFormtOffset = 50;

        private int widthTopScoreOffset = 10;
        private int heighTopScoretOffset = 10;

        private int widthCellOffset = 45;
        private int heighCelltOffset = 45;
        private int cellSize = 60;
        private int gapBetweenCells = 10;

        private int currentRowPos;
        private int currentColPos;

        public Label[,] Numbers { get => numbers; set => numbers = value; }
        public PictureBox[,] Cells { get => cells; set => cells = value; }
        public PictureBox[,] Map { get => map; set => map = value; }
        public string StartNumbers { get => startNumbers; set => startNumbers = value; }
        public Color BasicCellColor { get => basicCellColor; }
        public Color CorrectCellColor { get => correctCellColor; }
        public int Size { get => size; set => size = value; }
        public int RandomSteps { get => randomSteps; }
        public int WidthFormOffset { get => widthFormOffset; }
        public int HeighFormtOffset { get => heighFormtOffset; }
        public int WidthTopScoreOffset { get => widthTopScoreOffset; }
        public int HeighTopScoretOffset { get => heighTopScoretOffset; }
        public int WidthCellOffset { get => widthCellOffset; }
        public int HeighCelltOffset { get => heighCelltOffset; }
        public int CellSize { get => cellSize; }
        public int GapBetweenCells { get => gapBetweenCells; }
        public int CurrentRowPos { get => currentRowPos; set => currentRowPos = value; }
        public int CurrentColPos { get => currentColPos; set => currentColPos = value; }
    }
}
