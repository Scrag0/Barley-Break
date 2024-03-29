﻿using Microsoft.VisualBasic.Devices;
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

        public GameField GameField { get => gameField; }

        public void RandomizeCells()
        {
            Random rand = new Random();
            int temp;
            int tempRowPos;
            int tempColPos;

            for (int i = 0; i < GameField.Size * GameField.RandomSteps; i++)
            {
                bool isMoved = false;
                tempRowPos = GameField.CurrentRowPos;
                tempColPos = GameField.CurrentColPos;

                while (!isMoved)
                {
                    temp = rand.Next(0, keyMoves.Length);

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

            GameField.Moves = 0;
            GameField.StartNumbers = string.Empty;
            for (int i = 0; i < GameField.Size; i++)
            {
                for (int j = 0; j < GameField.Size; j++)
                {
                    if (IsInCorner(i, j)) continue;
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
                            GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos] = GameField.Cells[GameField.CurrentRowPos + 1, GameField.CurrentColPos];
                            GameField.Numbers[GameField.CurrentRowPos, GameField.CurrentColPos] = GameField.Numbers[GameField.CurrentRowPos + 1, GameField.CurrentColPos];
                            ChangeColor(GameField.CurrentRowPos, GameField.CurrentColPos);
                            GameField.Cells[GameField.CurrentRowPos + 1, GameField.CurrentColPos].Location = new Point(GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos].Location.X, 
                                                                                                                       GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos].Location.Y - (GameField.CellSize + GameField.GapBetweenCells));
                            GameField.CurrentRowPos++;
                            GameField.Moves++;
                        }
                        break;
                    }
                case "Down":
                    {
                        if (IsInRange(GameField.CurrentRowPos - 1, GameField.CurrentColPos))
                        {
                            GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos] = GameField.Cells[GameField.CurrentRowPos - 1, GameField.CurrentColPos];
                            GameField.Numbers[GameField.CurrentRowPos, GameField.CurrentColPos] = GameField.Numbers[GameField.CurrentRowPos - 1, GameField.CurrentColPos];
                            ChangeColor(GameField.CurrentRowPos, GameField.CurrentColPos);
                            GameField.Cells[GameField.CurrentRowPos - 1, GameField.CurrentColPos].Location = new Point(GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos].Location.X, 
                                                                                                                       GameField.Cells[GameField.CurrentRowPos, GameField.CurrentColPos].Location.Y + (GameField.CellSize + GameField.GapBetweenCells));
                            GameField.CurrentRowPos--;
                            GameField.Moves++;
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
                            GameField.Moves++;
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
                            GameField.Moves++;
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
            if (!IsInRange(rowPos, colPos)) return false;
            int value = Convert.ToInt32(GameField.Numbers[rowPos, colPos].Text);
            if ((rowPos * GameField.Size + colPos) + 1 == value) return true;

            return false;
        }

        public bool Check()
        {
            if (!IsInCorner(GameField.CurrentRowPos, GameField.CurrentColPos)) return false;

            for (int i = 0; i < GameField.Size; i++)
            {
                for (int j = 0; j < GameField.Size; j++)
                {
                    if (IsInCorner(i, j)) return true;
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

        public bool IsInCorner(int rowPos, int colPos)
        {
            if (rowPos == GameField.Size - 1 && colPos == GameField.Size - 1) return true;
            return false;
        }

        private void DeleteAll()
        {
            foreach (var item in GameField.Map)
            {
                if (item != null) item.Dispose();
            }

            for (int i = 0; i < GameField.Size; i++)
            {
                for (int j = 0; j < GameField.Size; j++)
                {
                    if (GameField.CurrentRowPos == i && GameField.CurrentColPos == j) continue;

                    if (GameField.Cells[i, j] != null) GameField.Cells[i, j].Dispose();
                    if (GameField.Numbers[i, j] != null) GameField.Numbers[i, j].Dispose();
                }
            }
        }

        public void RecreateAll(int newGameFieldSize)
        {
            DeleteAll();

            GameField.Size = newGameFieldSize;

            GameField.Numbers = new Label[GameField.Size, GameField.Size];
            GameField.Cells = new PictureBox[GameField.Size, GameField.Size];
            GameField.Map = new PictureBox[GameField.Size, GameField.Size];

            GameField.CurrentRowPos = GameField.Size - 1;
            GameField.CurrentColPos = GameField.Size - 1;
        }
    }
}
