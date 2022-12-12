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
        private DateTime currentDateTime;
        private string currentTime;

        private delegate void printer(string data);
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
        public GameManager GameManager { get => gameManager; set => gameManager = value; }

        public DateTime StartDateTime { get => startDateTime; set => startDateTime = value; }
        public DateTime CurrentDateTime { get => currentDateTime; set => currentDateTime = value; }
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

            Client.TcpClient = new TcpClient();
            Client.Connect(Host, Port);

            CreateSizes(5);

            ClientThread = new Thread(ClientListener);
            ClientThread.IsBackground = true;
            ClientThread.Start();

            GameManager = new GameManager(this.Controls);
            gameField = GameManager.GameField;
            ResizeElements();
            RecreateAll(GameField.Size);
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
            }
        }

        private void TimeListener()
        {
            while (!btnPlay.Enabled)
            {
                GetTime();
            }
        }

        // Exeption of disposed object?
        private void GetTime()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(Timer);
                return;
            }

            CurrentDateTime = DateTime.Now;
            CurrentTime = CurrentDateTime.Subtract(StartDateTime).ToString().Split('.')[0];
            if (string.IsNullOrEmpty(CurrentTime)) CurrentTime = TimeOnly.MinValue.ToString();
            lblTime.Text = "Time: " + CurrentTime;
        }

        private void ClearTopScores()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(Cleaner);
                return;
            }
            rTxtBTopScores.Clear();
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
                        Print(playerData.Split('~')[0] + ". Moves:" + playerData.Split('~')[1] + " Time:" + playerData.Split('~')[2]);
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

            try
            {
                //if (isExeption)
                //{
                //    rTxtBChat.Select(rTxtBChat.TextLength - msg.Length, msg.Length);
                //    rTxtBChat.SelectionColor = Color.BlueViolet;

                //    isExeption = false;
                //    return;
                //}

                // Works, but change colors
                //rTxtBTopScores.Select(rTxtBTopScores.TextLength - data.Length, data.Split(".")[0].Length);
                //if (data.Split(".")[0] == userName)
                //{
                //    rTxtBTopScores.SelectionColor = Color.Blue;
                //}
                //else
                //{
                //    rTxtBTopScores.SelectionColor = Color.Red;
                //}

                //rTxtBTopScores.Select(rTxtBTopScores.TextLength - data.Length + data.Split(":")[0].Length + 1, data.Split(":")[1].Length - 5);
                //if (data.Split(".")[0] == userName)
                //{
                //    rTxtBTopScores.SelectionColor = Color.Red;
                //}

                //rTxtBTopScores.Select(rTxtBTopScores.TextLength - data.Length + data.Split("Time:")[0].Length + 5, data.Split("Time:")[1].Length);
                //if (data.Split(".")[0] == userName)
                //{
                //    rTxtBTopScores.SelectionColor = Color.LawnGreen;
                //}
            }
            catch (Exception ex)
            {
                //isExeption = true;
                //Print(ex.Message);
            }
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
                Stop();
                MessageBox.Show($"Finish.\nYour result : {GameManager.Moves}\nTime: {CurrentTime}");
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            Start();

            if (!btnPlay.Enabled)
            {
                RecreateAll(GameField.Size);
                GenerateGameField();
                GameManager.RandomizeCells();
                Client.SendPlayerData(GameField.StartNumbers, UserName, GameManager.Moves, CurrentTime);
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

            GameManager.Moves = 0;
            GameManager.TmpMoves = -1;
            lblMoves.Text = "Moves: " + GameManager.Moves;

            StartDateTime = DateTime.Now;
        }

        private void Restart()
        {
            ClearTopScores();

            txtBName.Enabled = true;
            btnPlay.Visible = true;
            btnPlay.Enabled = true;
            GameManager.Moves = 0;
            GameManager.TmpMoves = -1;
            StartDateTime = DateTime.Now;

            RecreateAll(GameField.Size);

            lblTime.Text = "Time: 0";
            lblMoves.Text = "Moves: " + GameManager.Moves;
        }

        private void Stop()
        {
            txtBName.Enabled = true;
            btnPlay.Visible = true;
            btnPlay.Enabled = true;
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

                GameManager.Moves = 0;
                GameManager.TmpMoves = -1;
                StartDateTime = DateTime.Now;
                lblMoves.Text = "Moves: " + GameManager.Moves;
                Client.SendPlayerData(GameField.StartNumbers, UserName, GameManager.Moves, CurrentTime);
            }
        }

        private void sizeChangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem == null) return;

            Restart();
            RecreateAll(int.Parse(menuItem.Text.Split('x')[0]));
        }

        private void changeLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Call function in gameManager to change layout
            var menuItem = sender as ToolStripMenuItem;

            if (menuItem == null) return;

            Start();

            if (btnPlay.Enabled) return;

            GameField.StartNumbers = menuItem.Text;
            Client.SendPlayerData(GameField.StartNumbers, UserName, GameManager.Moves, CurrentTime);

            List<int> numbers = menuItem.Text.Split(';').Where(x => x != string.Empty).Select(x => Convert.ToInt32(x)).ToList();
            numbers.Add(0);

            RecreateAll((int) Math.Sqrt(numbers.Count));

            for (int i = 0; i < GameField.Size; i++)
            {
                for (int j = 0; j < GameField.Size; j++)
                {
                    if (GameManager.IsInCorner(i, j)) continue;

                    GenerateCell(i, j, numbers[i * GameField.Size + j]);
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

            ResizeElements();

            GameField.CurrentRowPos = GameField.Size - 1;
            GameField.CurrentColPos = GameField.Size - 1;

            CreateMap();
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