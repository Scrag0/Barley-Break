using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Permissions;
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
        private string currentTime;

        private delegate void printer(string data, bool check);
        private delegate void cleaner();
        private delegate void timer();
        private delegate void layout(string data);
        private printer Printer;
        private cleaner Cleaner;
        private timer Timer;
        private layout Layouts;

        private string userName = string.Empty;
        private readonly string host = "127.0.0.1";
        private readonly string port = "6462";

        private Thread clientThread;
        private Thread timeThread;

        public GameField GameField { get => gameField; }
        public Client Client { get => client; }
        public GameManager GameManager { get => gameManager; }

        public DateTime StartDateTime { get => startDateTime; set => startDateTime = value; }
        public string CurrentTime { get => currentTime; set => currentTime = value; }

        public string UserName { get => userName; set => userName = value; }
        public string Host { get => host; }
        public string Port { get => port; }

        public Thread ClientThread { get => clientThread; set => clientThread = value; }
        public Thread TimeThread { get => timeThread; set => timeThread = value; }

        public Form1()
        {
            InitializeComponent();
            Printer = new printer(Print);
            Cleaner = new cleaner(ClearTopScores);
            Timer = new timer(GetTime);
            Layouts = new layout(UpdateLayouts);

            this.KeyDown += new KeyEventHandler(OnKeyboardPressedMove);

            Client.Connect(Host, Port);

            CreateSizes(5);

            ClientThread = new Thread(ClientListener);
            ClientThread.IsBackground = true;
            ClientThread.Start();

            gameManager = new GameManager();
            gameField = GameManager.GameField;

            ResizeElements();
            CreateMap();
        }

        private void ClientListener()
        {
            while (Client.IsConnected)
            { 
                string data = Client.GetData();

                if (data.Contains("#updateLayouts"))
                {
                    UpdateLayouts(data.Split('&')[1]);
                }

                if (data.Contains("#updateTopScores"))
                {
                    UpdateTopScores(data.Split('&')[1]);
                }

                data = Client.GetClientException();
                if (data.Contains("#clientException"))
                {
                    UpdateExceptions(data.Split('&')[1]);
                }
            }
        }

        private void TimeListener()
        {
            while (!btnPlay.Enabled)
            {
                GetTime();
            }
        }

        // Without TryCatch was System.ObjectDisposedException: "Cannot access a disposed object.ObjectDisposed_ObjectName_Name"
        private void GetTime()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(Timer);
                    return;
                }

                CurrentTime = DateTime.Now.Subtract(StartDateTime).ToString().Split('.')[0];
                if (string.IsNullOrEmpty(CurrentTime)) CurrentTime = TimeOnly.MinValue.ToString();
                lblTime.Text = "Time: " + CurrentTime;
            }
            catch
            {
                return;
            }
        }

        private void ClearTopScores()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(Cleaner);
                    return;
                }
                rTxtBTopScores.Clear();
            }
            catch
            {
                return;
            }
        }

        private void UpdateTopScores(string data)
        {
            var Fields = data.Split('|');
            int countFields = Fields.Length;

            if (countFields <= 0) return;

            if (Fields[0].Split('$')[0] == GameField.StartNumbers)
            {
                ClearTopScores();
            }

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
                        if (playerData.Split('~').Length < 4) continue;
                        Print(playerData.Split('~')[0] + ". Moves:" + playerData.Split('~')[1] + " Time:" + playerData.Split('~')[2], bool.Parse(playerData.Split('~')[3]));
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }

        private void UpdateLayouts(string data)
        {
            if (data.Contains('|'))
            {
                string[] Layouts = data.Split('|');
                foreach (var layout in Layouts)
                {
                    if (string.IsNullOrEmpty(layout)) continue;
                    CreateMenuItem(layoutsToolStripMenuItem, layout, changeLayoutToolStripMenuItem_Click);
                }
                return;
            }

            if (string.IsNullOrEmpty(data)) return;

            if (layoutsToolStripMenuItem.DropDownItems.Count == 10)
            {
                layoutsToolStripMenuItem.DropDownItems.RemoveAt(0);
            }

            if (layoutsToolStripMenuItem.DropDownItems.Count <= 0)
            {
                CreateMenuItem(layoutsToolStripMenuItem, data, changeLayoutToolStripMenuItem_Click);
                return;
            }

            bool IsPresent = false;
            foreach (ToolStripMenuItem children in layoutsToolStripMenuItem.DropDownItems)
            {
                if (children.Text == data)
                {
                    IsPresent = true;
                    break;
                }
            }

            if (!IsPresent) CreateMenuItem(layoutsToolStripMenuItem, data, changeLayoutToolStripMenuItem_Click);
        }

        private void UpdateExceptions(string data)
        {
            ClearTopScores();
            if (string.IsNullOrEmpty(data)) return;
            Print(data);
        }

        private void CreateMenuItem(ToolStripMenuItem menuItem, string data, EventHandler eventHandler)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(Layouts, data);
                return;
            }

            var item = new ToolStripMenuItem(data, null, eventHandler);
            menuItem.DropDownItems.Add(item);
        }

        private void CreateSizes(int size)
        {
            if (size < 3) size = 3;

            if (size > 7) size = 7;

            for (int i = 3; i <= size; i++)
            {
                CreateMenuItem(sizeToolStripMenuItem, $"{i}x{i}", sizeChangeToolStripMenuItem_Click);
            }
        }

        private void Print(string data, bool isFinished = false)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(Printer, data, isFinished);
                    return;
                }

                data = data ?? string.Empty;

                if (rTxtBTopScores.Text.Length == 0)
                    rTxtBTopScores.AppendText(data);
                else
                    rTxtBTopScores.AppendText(Environment.NewLine + data);

                rTxtBTopScores.Select(rTxtBTopScores.TextLength - data.Length, data.Length);
                rTxtBTopScores.SelectionColor = isFinished ? Color.Blue : Color.Red;
            }
            catch
            {
                return;
            }
        }

        private void OnKeyboardPressedMove(object sender,KeyEventArgs e)
        {
            // TmpMoves can be deleted by using GameManager.keyMoves
            GameManager.MoveCell(e.KeyCode.ToString());

            if (GameField.TmpMoves != GameField.Moves)
            {
                lblMoves.Text = "Moves: " + GameField.Moves;
                Client.SendPlayerData(GameField.StartNumbers, UserName, GameField.Moves, CurrentTime, GameField.IsFinished.ToString());
                GameField.TmpMoves = GameField.Moves;
            }

            if (GameManager.Check())
            {
                Stop();
                Client.SendPlayerData(GameField.StartNumbers, UserName, GameField.Moves, CurrentTime, GameField.IsFinished.ToString());
                MessageBox.Show($"Finish.\nYour result: {GameField.Moves}\nTime: {CurrentTime}");
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            Start();

            if (!btnPlay.Enabled)
            {
                GameManager.RecreateAll(GameField.Size);
                ResizeElements();
                CreateMap();
                GenerateGameField();
                GameManager.RandomizeCells();
                Client.SendPlayerData(GameField.StartNumbers, UserName, GameField.Moves, CurrentTime, GameField.IsFinished.ToString());
            }
        }

        private void Start()
        {
            ClearTopScores();

            if (!GameManager.IsNameCorrect(txtBName.Text))
            {
                Print("Write correct username");
                return;
            }

            userName = txtBName.Text;

            txtBName.Enabled = false;
            btnPlay.Visible = false;
            btnPlay.Enabled = false;

            if (TimeThread == null || !TimeThread.IsAlive)
            {
                TimeThread = new Thread(TimeListener);
                TimeThread.IsBackground = true;
                TimeThread.Start();
            }

            GameField.Moves = 0;
            GameField.TmpMoves = 0;
            lblMoves.Text = "Moves: " + GameField.Moves;
            GameField.IsFinished = false;

            StartDateTime = DateTime.Now;
        }

        private void Restart()
        {
            ClearTopScores();

            txtBName.Enabled = true;
            btnPlay.Visible = true;
            btnPlay.Enabled = true;
            GameField.Moves = 0;
            GameField.TmpMoves = 0;
            StartDateTime = DateTime.Now;

            GameManager.RecreateAll(GameField.Size);
            ResizeElements();
            CreateMap();

            lblTime.Text = "Time: 0";
            lblMoves.Text = "Moves: " + GameField.Moves;
        }

        private void Stop()
        {
            txtBName.Enabled = true;
            btnPlay.Visible = true;
            btnPlay.Enabled = true;
            GameField.IsFinished = true;
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
                ClearTopScores();
                GameManager.RandomizeCells();

                GameField.Moves = 0;
                GameField.TmpMoves = 0;
                StartDateTime = DateTime.Now;
                lblMoves.Text = "Moves: " + GameField.Moves;
                Client.SendPlayerData(GameField.StartNumbers, UserName, GameField.Moves, CurrentTime, GameField.IsFinished.ToString());
            }
        }

        private void sizeChangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem == null) return;

            Restart();
            GameManager.RecreateAll(int.Parse(menuItem.Text.Split('x')[0]));
            ResizeElements();
            CreateMap();
        }

        private void changeLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem == null) return;

            Start();

            if (btnPlay.Enabled) return;

            GameField.StartNumbers = menuItem.Text;
            Client.SendPlayerData(GameField.StartNumbers, UserName, GameField.Moves, CurrentTime, GameField.IsFinished.ToString());

            List<int> numbers = menuItem.Text.Split(';').Where(x => x != string.Empty).Select(x => Convert.ToInt32(x)).ToList();

            GameManager.RecreateAll((int) Math.Sqrt(numbers.Count + 1));
            ResizeElements();
            CreateMap();

            for (int i = 0; i < GameField.Size; i++)
            {
                for (int j = 0; j < GameField.Size; j++)
                {
                    if (GameManager.IsInCorner(i, j)) continue;

                    GenerateCell(i, j, numbers[i * GameField.Size + j]);
                }
            }
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
            for (int i = 0; i < GameField.Size; i++)
            {
                for (int j = 0; j < GameField.Size; j++)
                {
                    if (GameManager.IsInCorner(i, j)) continue;

                    GenerateCell(i, j, i * GameField.Size + j + 1);
                }
            }
        }

        public void GenerateCell(int rowPos, int colPos, int value)
        {
            GameField.Cells[rowPos, colPos] = new PictureBox();
            GameField.Numbers[rowPos, colPos] = new Label();
            GameField.Numbers[rowPos, colPos].Text = Convert.ToString(value);
            GameField.Numbers[rowPos, colPos].Size = new Size(GameField.CellSize, GameField.CellSize);
            GameField.Numbers[rowPos, colPos].TextAlign = ContentAlignment.MiddleCenter;
            GameField.Numbers[rowPos, colPos].Font = new Font(new FontFamily("Microsoft Sans Serif"), 15);
            GameField.Cells[rowPos, colPos].Controls.Add(GameField.Numbers[rowPos, colPos]);
            GameField.Cells[rowPos, colPos].Location = new Point(pnlGameField.Location.X + GameField.WidthCellOffset + (GameField.CellSize + GameField.GapBetweenCells) * colPos,
                                                 pnlGameField.Location.Y + GameField.HeighCelltOffset + (GameField.CellSize + GameField.GapBetweenCells) * rowPos);
            GameField.Cells[rowPos, colPos].Size = new Size(GameField.CellSize, GameField.CellSize);
            GameField.Cells[rowPos, colPos].BackColor = GameField.BasicCellColor;
            this.Controls.Add(GameField.Cells[rowPos, colPos]);
            GameField.Cells[rowPos, colPos].BringToFront();
            GameManager.ChangeColor(rowPos, colPos);
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