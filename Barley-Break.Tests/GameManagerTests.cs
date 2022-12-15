using Microsoft.VisualStudio.TestTools.UnitTesting;
using Barley_Break;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace Barley_Break.Tests
{
    [TestClass()]
    public class GameManagerTests
    {
        private GameManager gameManager;
        private GameField gameField;
        //public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            gameManager = new GameManager();
            gameField = gameManager.GameField;
            //gameField.StartNumbers = "7;9;12;3;6;4;14;13;2;10;11;15;1;8;5;";
            //List<int> numbers = gameField.StartNumbers.Split(';').Where(x => x != string.Empty).Select(x => Convert.ToInt32(x)).ToList();

            List<int> numbers = new List<int> { 7, 9, 12, 3, 6, 4, 14, 13, 2, 10, 11, 15, 1, 8, 5 };

            for (int i = 0; i < gameField.Size; i++)
            {
                for (int j = 0; j < gameField.Size; j++)
                {
                    gameField.Numbers[i, j] = new Label();
                    gameField.Cells[i, j] = new PictureBox();
                    gameField.Map[i, j] = new PictureBox();
                    if (i == gameField.Size - 1 && j == gameField.Size - 1) continue;
                    gameField.Numbers[i, j].Text = Convert.ToString(numbers[i * gameField.Size + j]);
                }
            }
        }

        [TestMethod()]
        public void ChangeColor_0and0_BasicColor()
        {
            // arrange
            int rowPos = 0;
            int colPos = 0;
            Color expected = gameField.BasicCellColor;

            // act
            gameManager.ChangeColor(rowPos, colPos);
            Color actual = gameField.Cells[rowPos, colPos].BackColor;

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ChangeColor_0and2_BasicColor()
        {
            // arrange
            int rowPos = 0;
            int colPos = 2;
            Color expected = gameField.BasicCellColor;

            // act
            gameManager.ChangeColor(rowPos, colPos);
            Color actual = gameField.Cells[rowPos, colPos].BackColor;

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ChangeColor_2and1_CorrectColor()
        {
            // arrange
            int rowPos = 2;
            int colPos = 1;
            Color expected = gameField.CorrectCellColor;

            // act
            gameManager.ChangeColor(rowPos, colPos);
            Color actual = gameField.Cells[rowPos, colPos].BackColor;

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ChangeColor_2and2_CorrectColor()
        {
            // arrange
            int rowPos = 2;
            int colPos = 2;
            Color expected = gameField.CorrectCellColor;

            // act
            gameManager.ChangeColor(rowPos, colPos);
            Color actual = gameField.Cells[rowPos, colPos].BackColor;

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void IsNameCorrect_Null_False()
        {
            // arrange
            string username = null;

            // act
            bool actual = gameManager.IsNameCorrect(username);

            // assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void IsNameCorrect_Empty_False()
        {
            // arrange
            string username = string.Empty;

            // act
            bool actual = gameManager.IsNameCorrect(username);

            // assert
            Assert.IsFalse(actual);
        }

        //[DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML",
        //    "TestNames.xml",
        //    "User",
        //    DataAccessMethod.Sequential)]
        [TestMethod()]
        public void IsNameCorrect_Contains_False()
        {
            // arrange
            string username = "ASdf:}";

            // act
            bool actual = gameManager.IsNameCorrect(username);

            // assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void IsNameCorrect_NotContains_True()
        {
            // arrange
            string username = "Username";

            // act
            bool actual = gameManager.IsNameCorrect(username);

            // assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void IsCorrect_minus1andminus3_False()
        {
            // arrange
            int rowPos = -1;
            int colPos = -3;

            // act
            bool actual = gameManager.IsCorrect(rowPos, colPos);

            // assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void IsCorrect_0and0_False()
        {
            // arrange
            int rowPos = 0;
            int colPos = 0;

            // act
            bool actual = gameManager.IsCorrect(rowPos, colPos);

            // assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void IsCorrect_0and2_False()
        {
            // arrange
            int rowPos = 0;
            int colPos = 2;

            // act
            bool actual = gameManager.IsCorrect(rowPos, colPos);

            // assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void IsCorrect_2and1_True()
        {
            // arrange
            int rowPos = 2;
            int colPos = 1;

            // act
            bool actual = gameManager.IsCorrect(rowPos, colPos);

            // assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void IsCorrect_2and2_True()
        {
            // arrange
            int rowPos = 2;
            int colPos = 2;

            // act
            bool actual = gameManager.IsCorrect(rowPos, colPos);

            // assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void Check_1and2andUnordered_False()
        {
            // arrange
            gameField.CurrentRowPos = 1;
            gameField.CurrentColPos = 2;

            // act
            bool actual = gameManager.Check();

            // assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void Check_3and3andUnordered_False()
        {
            // arrange
            gameField.CurrentRowPos = 3;
            gameField.CurrentColPos = 3;

            // act
            bool actual = gameManager.Check();

            // assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void Check_3and3andOrdered_True()
        {
            // arrange
            gameField.CurrentRowPos = 3;
            gameField.CurrentColPos = 3;

            for (int i = 0; i < gameField.Size; i++)
            {
                for (int j = 0; j < gameField.Size; j++)
                {
                    gameField.Numbers[i, j] = new Label();
                    if (i == gameField.Size - 1 && j == gameField.Size - 1) continue;
                    gameField.Numbers[i, j].Text = Convert.ToString(i * gameField.Size + j + 1);
                }
            }

            // act
            bool actual = gameManager.Check();

            // assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void IsInRange_minus1and0_False()
        {
            // arrange
            int rowPos = -1;
            int colPos = 0;

            // act
            bool actual = gameManager.IsInRange(rowPos, colPos);

            // assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void IsInRange_0and0_Tue()
        {
            // arrange
            int rowPos = 0;
            int colPos = 0;

            // act
            bool actual = gameManager.IsInRange(rowPos, colPos);

            // assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void IsInRange_2and1_True()
        {
            // arrange
            int rowPos = 2;
            int colPos = 1;

            // act
            bool actual = gameManager.IsInRange(rowPos, colPos);

            // assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void IsInRange_3and3_True()
        {
            // arrange
            int rowPos = 3;
            int colPos = 3;

            // act
            bool actual = gameManager.IsInRange(rowPos, colPos);

            // assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void IsInRange_5and3_False()
        {
            // arrange
            int rowPos = 5;
            int colPos = 3;

            // act
            bool actual = gameManager.IsInRange(rowPos, colPos);

            // assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void IsInCorner_2and3_False()
        {
            // arrange
            int rowPos = 2;
            int colPos = 3;

            // act
            bool actual = gameManager.IsInCorner(rowPos, colPos);

            // assert
            Assert.IsFalse(actual);
        }

        [TestMethod()]
        public void IsInCorner_3and3_True()
        {
            // arrange
            int rowPos = 3;
            int colPos = 3;

            // act
            bool actual = gameManager.IsInCorner(rowPos, colPos);

            // assert
            Assert.IsTrue(actual);
        }

        [TestMethod()]
        public void IsInCorner_5and6_False()
        {
            // arrange
            int rowPos = 5;
            int colPos = 6;

            // act
            bool actual = gameManager.IsInCorner(rowPos, colPos);

            // assert
            Assert.IsFalse(actual);
        }
    }
}