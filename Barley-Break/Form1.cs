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
        private delegate void timer();
        private printer Printer;
        private cleaner Cleaner;
        private timer Timer;

        private string userName = string.Empty;
        private readonly string host = "127.0.0.1";
        private readonly string port = "6462";

        private Thread clientThread;

        public GameField GameField { get => gameField; }
        public Client Client { get => client; }
        public GameManager GameManager { get => gameManager; set => gameManager = value; }

        public DateTime StartDateTime { get => startDateTime; set => startDateTime = value; }
        public DateTime CurrentDateTime { get => currentDateTime; set => currentDateTime = value; }
        public string CurrentTime { get => currentTime; set => currentTime = value; }

        public string UserName { get => userName; set => userName = value; }

        public string Host { get => host; }

        public string Port { get => port; }

        public Thread ClientThread { get => clientThread; set => clientThread = value; }

        public Form1()
        {
            InitializeComponent();
            Printer = new printer(Print);
            Cleaner = new cleaner(ClearTopScores);
            Timer = new timer(GetTime);

            this.KeyDown += new KeyEventHandler(OnKeyboardPressedMove);

            Client.TcpClient = new TcpClient();
            Client.Connect(Host, Port);

            GameManager = new GameManager(this.Controls);
            gameField = GameManager.GameField;
            ResizeElements();
            RecreateAll(GameField.Size);
        }

        private void Listener()
        {
            while (Client.IsConnected)
            {
                string data = Client.GetScores();
                GetTime();
                if (GameManager.TmpMoves == GameManager.Moves)
                {
                    UpdateTopScores(data);
                }
            }
        }

        // Exeption of disposed object
        private void GetTime()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(Timer);
                return;
            }

            CurrentDateTime = DateTime.Now;
            CurrentTime = CurrentDateTime.Subtract(StartDateTime).ToString().Split('.')[0];
            lblTime.Text = "Time: " + CurrentTime;
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

            var Fields = data.Split('|');
            int countFields = Fields.Length;

            if (countFields <= 0) return;

            for (int i = 0; i < countFields; i++)
            {
                var split = Fields[i].Split('$');
                if (split.Length == 1) continue;
                string Field = split[0];
                string playerData = split[1];

                if (Field == GameField.StartNumbers)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(playerData)) continue;
                        Print(playerData.Split('~')[0] + ". Moves:" + playerData.Split('~')[1] + " Time:" + playerData.Split('~')[2]);
                    }
                    catch
                    {
                        continue;
                    }
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
            GameManager.MoveCell(e.KeyCode.ToString());

            if (GameManager.TmpMoves != GameManager.Moves)
            {
                lblMoves.Text = "Moves: " + GameManager.Moves;
                Client.SendPlayerData(GameField.StartNumbers, UserName, GameManager.Moves, CurrentTime);
                GameManager.TmpMoves = GameManager.Moves;
            }

            if (GameManager.Check())
            {
                MessageBox.Show($"Finish.\nYour result : {GameManager.Moves}\nTime: {CurrentTime}");
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (!GameManager.IsNameCorrect(txtBName.Text))
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

                ClientThread = new Thread(Listener);
                ClientThread.IsBackground = true;
                ClientThread.Start();

                GenerateGameField();
                GameManager.RandomizeCells();

                StartDateTime = DateTime.Now;
            }
        }

        private void Restart()
        {
            ClearTopScores();

            txtBName.Enabled = true;
            btnPlay.Visible = true;
            btnPlay.Enabled = true;
            GameManager.Moves = 0;
            GameManager.TmpMoves = -1;

            RecreateAll(GameField.Size);

            lblTime.Text = "Time: " + 0;
            lblMoves.Text = "Moves: " + GameManager.Moves;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Client.Disconnect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Restart();
        }

        private void changeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!btnPlay.Enabled)
            {
                GameManager.RandomizeCells();
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

        public void RecreateAll(int newGameFieldSize)
        {
            GameManager.DeleteAll();

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
                    GameField.Cells[i, j].Location = new Point(pnlGameField.Location.X + GameField.WidthCellOffset + (GameField.CellSize + GameField.GapBetweenCells) * j,
                                                         pnlGameField.Location.Y + GameField.HeighCelltOffset + (GameField.CellSize + GameField.GapBetweenCells) * i);
                    GameField.Cells[i, j].Size = new Size(GameField.CellSize, GameField.CellSize);
                    GameField.Cells[i, j].BackColor = GameField.BasicCellColor;
                    this.Controls.Add(GameField.Cells[i, j]);
                    GameField.Cells[i, j].BringToFront();
                    GameManager.ChangeColor(i, j);
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