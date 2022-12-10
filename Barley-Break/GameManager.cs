using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Barley_Break
{
    public class GameManager
    {
        private string[] keyMoves = { "Up", "Down", "Left", "Right" };
        private string specialSymbols = " !\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

        private GameField gameField = new GameField();
        private int moves = 0;
        private int tmpMoves = -1;

        public GameField GameField { get => gameField; }
        public int Moves { get => moves; set => moves = value; }
        public int TmpMoves { get => tmpMoves; set => tmpMoves = value; }

        private Control.ControlCollection Controls;

        public GameManager(Control.ControlCollection Controls)
        {
            this.Controls = Controls;
        }

        public void RandomizeCells()
        {
            Random rand = new Random();
            int temp;
            int tempRowPos;
            int tempColPos;

            for (int i = 0; i < GameField.Size * GameField.RandomSteps; i++)
            {
                bool isMoved = false;

                while (!isMoved)
                {
                    temp = rand.Next(0, keyMoves.Length);
                    tempRowPos = GameField.CurrentRowPos;
                    tempColPos = GameField.CurrentColPos;

                    MoveCell(keyMoves[temp]);

                    if (tempRowPos != GameField.CurrentRowPos || tempColPos != GameField.CurrentColPos) isMoved = true;
                }
            }

            while (GameField.CurrentRowPos != GameField.Size - 1)
            {
                MoveCell(keyMoves[0]);
            }

            while (GameField.CurrentColPos != GameField.Size - 1)
            {
                MoveCell(keyMoves[2]);
            }

            Moves = 0;

            for (int i = 0; i < GameField.Size; i++)
            {
                for (int j = 0; j < GameField.Size; j++)
                {
                    if (i == GameField.Size - 1 && j == GameField.Size - 1) break;
                    GameField.StartNumbers += GameField.Numbers[i,j].Text + ";";
                }
            }
        }

        public void ChangeColor(int rowPos, int colPos)
        {
            GameField.Cells[rowPos, colPos].BackColor = (IsCorrect(rowPos, colPos)) ? GameField.CorrectCellColor : GameField.BasicCellColor;
        }

        public void MoveCell(string moveTo)
        {
            switch (moveTo)
            {
                case "Up":
                    {
                        if (IsInRange(GameField.CurrentRowPos + 1, GameField.CurrentColPos))
                        {
                            //  Alternative
                            //pics[GameField.CurrentRowPos, GameField.CurrentColPos] = pics[GameField.CurrentRowPos - 1, GameField.CurrentColPos];
                            //labels[GameField.CurrentRowPos, GameField.CurrentColPos] = labels[GameField.CurrentRowPos - 1, GameField.CurrentColPos];
                            //pics[GameField.CurrentRowPos - 1, GameField.CurrentColPos].Location = new Point(pics[GameField.CurrentRowPos - 1, GameField.CurrentColPos].Location.X, pics[GameField.CurrentRowPos - 1, GameField.CurrentColPos].Location.Y + (GameField.Cellsize + GameField.GapBetweenCells));
                            //GameField.CurrentRowPos--;

                            GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos] = GameField.Cells[GameField.CurrentRowPos + 1, GameField.CurrentColPos];
                            GameField.Numbers[GameField.CurrentRowPos, GameField.CurrentColPos] = GameField.Numbers[GameField.CurrentRowPos + 1, GameField.CurrentColPos];
                            ChangeColor(GameField.CurrentRowPos, GameField.CurrentColPos);
                            GameField.Cells[GameField.CurrentRowPos + 1, GameField.CurrentColPos].Location = new Point(GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos].Location.X, 
                                                                                                                       GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos].Location.Y - (GameField.CellSize + GameField.GapBetweenCells));
                            GameField.CurrentRowPos++;
                            Moves++;
                        }
                        break;
                    }
                case "Down":
                    {
                        if (IsInRange(GameField.CurrentRowPos - 1, GameField.CurrentColPos))
                        {
                            //pics[GameField.CurrentRowPos, GameField.CurrentColPos] = pics[GameField.CurrentRowPos + 1, GameField.CurrentColPos];
                            //labels[GameField.CurrentRowPos, GameField.CurrentColPos] = labels[GameField.CurrentRowPos + 1, GameField.CurrentColPos];
                            //pics[GameField.CurrentRowPos + 1, GameField.CurrentColPos].Location = new Point(pics[GameField.CurrentRowPos + 1, GameField.CurrentColPos].Location.X, pics[GameField.CurrentRowPos + 1, GameField.CurrentColPos].Location.Y - (GameField.Cellsize + GameField.GapBetweenCells));
                            //GameField.CurrentRowPos++;

                            GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos] = GameField.Cells[GameField.CurrentRowPos - 1, GameField.CurrentColPos];
                            GameField.Numbers[GameField.CurrentRowPos, GameField.CurrentColPos] = GameField.Numbers[GameField.CurrentRowPos - 1, GameField.CurrentColPos];
                            ChangeColor(GameField.CurrentRowPos, GameField.CurrentColPos);
                            GameField.Cells[GameField.CurrentRowPos - 1, GameField.CurrentColPos].Location = new Point(GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos].Location.X, 
                                                                                                                       GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos].Location.Y + (GameField.CellSize + GameField.GapBetweenCells));
                            GameField.CurrentRowPos--;
                            Moves++;
                        }
                        break;
                    }
                case "Left":
                    {
                        if (IsInRange(GameField.CurrentRowPos, GameField.CurrentColPos + 1))
                        {
                            GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos] = GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos + 1];
                            GameField.Numbers[GameField.CurrentRowPos, GameField.CurrentColPos] = GameField.Numbers[GameField.CurrentRowPos, GameField.CurrentColPos + 1];
                            ChangeColor(GameField.CurrentRowPos, GameField.CurrentColPos);
                            GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos + 1].Location = new Point(GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos].Location.X - (GameField.CellSize + GameField.GapBetweenCells), 
                                                                                                                       GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos].Location.Y);
                            GameField.CurrentColPos++;
                            Moves++;
                        }
                        break;
                    }
                case "Right":
                    {
                        if (IsInRange(GameField.CurrentRowPos, GameField.CurrentColPos - 1))
                        {
                            GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos] = GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos - 1];
                            GameField.Numbers[GameField.CurrentRowPos, GameField.CurrentColPos] = GameField.Numbers[GameField.CurrentRowPos, GameField.CurrentColPos - 1];
                            ChangeColor(GameField.CurrentRowPos, GameField.CurrentColPos);
                            GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos - 1].Location = new Point(GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos].Location.X + (GameField.CellSize + GameField.GapBetweenCells), 
                                                                                                                       GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos].Location.Y);
                            GameField.CurrentColPos--;
                            Moves++;
                        }
                        break;
                    }
            }
        }

        public bool IsNameCorrect(string username)
        {
            if (username == null) return false;
            if (username.Trim() == string.Empty) return false;

            foreach (var c in specialSymbols)
            {
                if (username.Contains(c)) return false;
            }

            return true;
        }

        public bool IsCorrect(int rowPos, int colPos)
        {
            int value = Convert.ToInt32(GameField.Numbers[rowPos, colPos].Text);
            if ((rowPos * GameField.Size + colPos) + 1 == value) return true;

            return false;
        }

        public bool Check()
        {
            if (GameField.CurrentRowPos != GameField.Size - 1 || GameField.CurrentColPos != GameField.Size - 1) return false;

            for (int i = 0; i < GameField.Size; i++)
            {
                for (int j = 0; j < GameField.Size; j++)
                {
                    //if (IsCorrect(i, j) || (i == GameField.Size - 1 && j == GameField.Size - 1)) continue;
                    //return false;
                    if (i == GameField.Size - 1 && j == GameField.Size - 1) return true;
                    if (!IsCorrect(i, j)) return false;
                }
            }

            return true;
        }

        public bool IsInRange(int rowPos, int colPos)
        {
            if ((0 <= rowPos && 0 <= colPos) && (rowPos < GameField.Size && colPos < GameField.Size)) return true;

            return false;
        }

        public void DeleteAll()
        {
            if (GameField.Map == null) return;

            foreach (var item in GameField.Map)
            {
                if (item != null) Controls.Remove(item);
            }

            if (GameField.Cells == null) return;
            if (GameField.Numbers == null) return;

            for (int i = 0; i < GameField.Size; i++)
            {
                for (int j = 0; j < GameField.Size; j++)
                {
                    if (GameField.CurrentRowPos == i && GameField.CurrentColPos == j) continue;

                    if (GameField.Cells[i, j] != null && GameField.Numbers[i, j] != null)
                    {
                        Controls.Remove(GameField.Cells[i, j]);
                        Controls.Remove(GameField.Numbers[i, j]);
                    }
                }
            }
        }
    }
}
