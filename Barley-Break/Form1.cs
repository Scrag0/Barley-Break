using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace Barley_Break
{
    public partial class Form1 : Form
    {
        private Client client = new Client();
        private GameManager gameManager;
        private GameField gameField;

        private DateTime startDateTime;
        private DateTime currentDateTime;
        string currentTime;

        private delegate void printer(string data);
        private delegate void cleaner();
        printer Printer;
        cleaner Cleaner;

        private string userName = string.Empty;
        private readonly string host = "127.0.0.1";
        private readonly string port = "6462";

        private Thread clientThread;

        public GameField GameField { get => gameField; }

        public Form1()
        {
            InitializeComponent();
            Printer = new printer(Print);
            Cleaner = new cleaner(ClearTopScores);

            this.KeyDown += new KeyEventHandler(OnKeyboardPressedMove);

            gameManager = new GameManager(this.Controls);
            gameField = gameManager.GameField;
            ResizeElements();
            RecreateAll();
        }

        private void Listener()
        {
            while (client.IsConnected)
            {
                //string data = GetScores();
                GetTime();
                if (gameManager.TmpMoves == gameManager.Moves)
                {
                    //UpdateTopScores(data);
                }
            }
        }

        private void GetTime()
        {
            currentDateTime = DateTime.Now;
            currentTime = currentDateTime.Subtract(startDateTime).ToString().Split('.')[0];
            lblTime.Text = "Time: " + currentTime;
        }

        //метод, що очищує TopScores
        private void ClearTopScores()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(Cleaner);
                return;
            }
            rTxtBTopScores.Clear();
        }

        //метод, що оновлює TopScores
        private void UpdateTopScores(string data)
        {
            ClearTopScores();

            var Players = data.Split('|');
            int countPlayers = Players.Length;

            if (countPlayers <= 0) return;

            for (int i = 0; i < countPlayers; i++)
            {
                try
                {
                    if (string.IsNullOrEmpty(Players[i])) continue;
                    Print(String.Format(Players[i]));
                }
                catch
                {
                    continue;
                }
            }
        }

        //метод, що виводить текст в richTextBox на Form1
        private void Print(string data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(Printer, data);
                return;
            }

            data = data ?? string.Empty;

            if (rTxtBTopScores.Text.Length == 0)
                rTxtBTopScores.AppendText(data);
            else
                rTxtBTopScores.AppendText(Environment.NewLine + data);
        }

        private void OnKeyboardPressedMove(object sender,KeyEventArgs e)
        {
            gameManager.MoveCell(e.KeyCode.ToString());

            if (gameManager.TmpMoves != gameManager.Moves)
            {
                lblMoves.Text = "Moves: " + gameManager.Moves;
                client.SendPlayerData(gameManager.GameField.Size, userName, gameManager.Moves, currentTime);
                gameManager.TmpMoves = gameManager.Moves;
            }

            if (gameManager.Check())
            {
                MessageBox.Show($"Finish.\n Your result : {gameManager.Moves}\nTime: {currentTime}");
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (!gameManager.IsNameCorrect(txtBName.Text))
            {
                ClearTopScores();
                Print("Write correct username");
            }
            else
            {
                ClearTopScores();

                userName = txtBName.Text;

                txtBName.Enabled = false;
                btnPlay.Visible = false;
                btnPlay.Enabled = false;

                client.TcpClient = new TcpClient();
                client.Connect(host, port);

                clientThread = new Thread(Listener);
                clientThread.IsBackground = true;
                clientThread.Start();

                GenerateGameField();
                gameManager.RandomizeCells();

                startDateTime = DateTime.Now;
            }
        }

        private void Restart()
        {
            ClearTopScores();

            txtBName.Enabled = true;
            btnPlay.Visible = true;
            btnPlay.Enabled = true;
            gameManager.Moves = 0;
            gameManager.TmpMoves = -1;

            RecreateAll();

            lblTime.Text = "Time: " + 0;
            lblMoves.Text = "Moves: " + gameManager.Moves;
            //Disconnect();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.Disconnect();
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Restart();
        }

        private void changeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!btnPlay.Enabled)
            {
                gameManager.RandomizeCells();
            }
        }

        private void x3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Restart();
            RecreateAll(3);
        }

        private void x4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Restart();
            RecreateAll(4);
        }

        private void x5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Restart();
            RecreateAll(5);
        }

        public void RecreateAll(int newGameFieldSize = 4)
        {
            gameManager.DeleteAll();

            GameField.Size = newGameFieldSize;


            GameField.Numbers = new Label[GameField.Size, GameField.Size];
            GameField.Cells = new PictureBox[GameField.Size, GameField.Size];
            GameField.Map = new PictureBox[GameField.Size, GameField.Size];

            ResizeElements();

            GameField.CurrentRowPos = GameField.Size - 1;
            GameField.CurrentColPos = GameField.Size - 1;

            CreateMap();
        }

        public void CreateMap()
        {
            for (int i = 0; i < GameField.Size; i++)
            {
                for (int j = 0; j < GameField.Size; j++)
                {
                    GameField.Map[i, j] = new PictureBox();
                    GameField.Map[i, j].Location = new Point(pnlGameField.Location.X + GameField.WidthCellOffset + (GameField.CellSize + GameField.GapBetweenCells) * j,
                                                    pnlGameField.Location.Y + GameField.HeighCelltOffset + (GameField.CellSize + GameField.GapBetweenCells) * i);
                    GameField.Map[i, j].Size = new Size(GameField.CellSize, GameField.CellSize);
                    GameField.Map[i, j].BackColor = Color.Gray;
                    this.Controls.Add(GameField.Map[i, j]);
                    GameField.Map[i, j].BringToFront();
                }
            }
        }

        public void GenerateGameField()
        {
            int n = 1;
            for (int i = 0; i < GameField.Size; i++)
            {
                for (int j = 0; j < GameField.Size; j++)
                {
                    if (n == GameField.Size * GameField.Size)
                    {
                        break;
                    }

                    GameField.Cells[i, j] = new PictureBox();
                    GameField.Numbers[i, j] = new Label();
                    GameField.Numbers[i, j].Text = Convert.ToString(n++);
                    GameField.Numbers[i, j].Size = new Size(GameField.CellSize, GameField.CellSize);
                    GameField.Numbers[i, j].TextAlign = ContentAlignment.MiddleCenter;
                    GameField.Numbers[i, j].Font = new Font(new FontFamily("Microsoft Sans Serif"), 15);
                    GameField.Cells[i, j].Controls.Add(GameField.Numbers[i, j]);
                    GameField.Cells[i, j].Location = new Point(pnlGameField.Location.X + GameField.WidthCellOffset + (GameField.CellSize + GameField.GapBetweenCells) * j, pnlGameField.Location.Y + GameField.HeighCelltOffset + (GameField.CellSize + GameField.GapBetweenCells) * i);
                    GameField.Cells[i, j].Size = new Size(GameField.CellSize, GameField.CellSize);
                    GameField.Cells[i, j].BackColor = GameField.BasicCellColor;
                    this.Controls.Add(GameField.Cells[i, j]);
                    GameField.Cells[i, j].BringToFront();
                    gameManager.ChangeColor(i, j);
                }
            }
        }

        public void ResizeElements()
        {
            pnlGameField.Size = new Size(GameField.WidthCellOffset * 2 + (GameField.CellSize + GameField.GapBetweenCells) * GameField.Size - GameField.GapBetweenCells,
                                    GameField.HeighCelltOffset * 2 + (GameField.CellSize + GameField.GapBetweenCells) * GameField.Size - GameField.GapBetweenCells);
            this.Size = new Size(pnlGameField.Width + pnlInfo.Width + GameField.WidthFormOffset, pnlGameField.Height + menuStrip1.Height + GameField.HeighFormtOffset);
            rTxtBTopScores.Size = new Size(pnlInfo.Width - rTxtBTopScores.Location.X - GameField.WidthTopScoreOffset, pnlInfo.Height - rTxtBTopScores.Location.Y - GameField.HeighTopScoretOffset);
        }
    }
}