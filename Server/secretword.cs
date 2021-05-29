using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerSide
{
    public partial class secretword : Form
    {
        TcpListener server;
        Byte[] ipbytes;
        IPAddress ipaddress;
        int portNo;
        Socket socket;
        NetworkStream nstream;
        BinaryReader reader;
        BinaryWriter writer;
        static int turn;
        public int Sscore { get; set; }
        enum States
        {
            None,
            Piller,
            Rope,
            Head,
            body,
            LeftHand,
            RightHand,
            LeftLeg,
            RightLeg
        }

        // Holds currnent word characters
        List<Label> labels = new List<Label>();
        // Word under consideration
        public string currentWord { get; set; }
        // Default character for hidden word letters
        public string DefaultChar { get { return "__"; } }
        // Current States, used specially to repaint panel grphics
        private States CurrentStates = States.None;
        // States enum size
        public int StatesSize { get { return (Enum.GetValues(typeof(States)).Length - 1); } }
        public string stringClicked { get; set; }
        public string CharClicked { get; set; }


    
        string dif;
        string cat;
        Form1 form;
        public secretword(Form1 f)
        {
            InitializeComponent();
            turn = 0;
            AddButtons();
            form = f;
            ipbytes = new Byte[] { 127, 0, 0, 1 };
            ipaddress = new IPAddress(ipbytes);
            portNo = 7777;
            cat = Form1.wordcategory;
            dif = Form1.worddifficulty;
            server = new TcpListener(ipaddress, portNo);
            score();
            SelectWord();
        }

        /// <summary>
        /// Adds buttons
        /// </summary>
        private void AddButtons()
        {
            for (int i = (int)'A'; i <= (int)'Z'; i++)
            {
                Button b = new Button();
                b.Text = ((char)i).ToString();
                b.Parent = flowLayoutPanel1;
                b.Font = new Font(FontFamily.GenericSansSerif, 20, FontStyle.Bold);
                b.Size = new Size(40, 40);
                b.BackColor = Color.Cornsilk;
                b.Click += b_Click; // Event hook-up
            }

            // Disabling buttons
            flowLayoutPanel1.Enabled = false;
        }
        //private async void connectAsync()
        //{
        //    await Task.Run(async () =>
        //    {
        //        server.Start();
        //        MessageBox.Show("Server Started..");
        //        socket = server.AcceptSocket();

        //        nstream = new NetworkStream(socket);

        //        MessageBox.Show("connection accepted");
        //        await RecieveAsync();
        //    });
        //}
        //     private async Task RecieveAsync()
        //     {
        //         await Task.Run(() => { Recieve(); });
        //     }
        //     public void Recieve()
        //     {
        //         /*//richTextBox2.Text = "";

        ////nstream = new NetworkStream(Connection);*/
        //         while (InvokeRequired)
        //{
        //reader = new BinaryReader(nstream);

        ////richTextBox2.Text = reader.ReadString();
        //Invalidate();
        //MessageBox.Show("Message Recieved from client x");

        //}
        //     }
        //     void sendChar(char letter) {
        //         try
        //         {
        //             writer = new BinaryWriter(nstream);

        //             if (nstream.CanWrite)
        //             {
        //                 writer.Write(letter);
        //                 //richTextBox1.Text = "";
        //                 MessageBox.Show("Message Sent to client x");
        //                 //writer.Close();
        //             }
        //         }
        //         catch (Exception ex) {MessageBox.Show(ex.Message);} 
        // }
        public void checkTurn()
        {
            try
            {
                //turn ++;
                //if (turn == form.clients.Count)
                //{
                //    turn = 0;
                //}
                //MessageBox.Show(form.IpAddress.ToString());
                //MessageBox.Show(form.PlayerIP.ToString());
                if (form.IpAddress.ToString() == form.PlayerIP)
                {
                    flowLayoutPanel1.Enabled = true;
                }
                else
                {
                    flowLayoutPanel1.Enabled = false;
                }
            }
            catch (Exception exe) { MessageBox.Show(exe.Message); }
        }





        public void b_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            char charClicked = b.Text.ToCharArray()[0];
            CharClicked = charClicked.ToString();



            if ((currentWord = currentWord.ToUpper()).Contains(charClicked))
            {
                flowLayoutPanel1.Enabled = true;
                b.Enabled = false;
                // char is there (right guess)
                lblInfo.Text = "Awesome!";
                lblInfo.ForeColor = Color.Green;
                char[] charArray = currentWord.ToCharArray();
                for (int i = 0; i < currentWord.Length; i++)
                {
                    if (charArray[i] == charClicked)
                        labels[i].Text = charClicked.ToString();
                }

                // Winning condition               
                if (!labels.Where(x => x.Text.Equals(DefaultChar)).Any())
                {

                    lblInfo.ForeColor = Color.Green;
                    lblInfo.Text = "You win.";
                    flowLayoutPanel1.Enabled = false;
                    win();
                    MessageBox.Show($"your score={Sscore }");
                    playAgain();
                }
            }
            else
            {
                // Wrong guess
                lblInfo.Text = "Wrong Guess!";
                lblInfo.ForeColor = Color.Brown;
                if (CurrentStates != States.RightLeg)
                    CurrentStates++;
                txtGuessesLeft.Text = (StatesSize - (int)CurrentStates).ToString();
                txtWrongguesses.Text += string.IsNullOrWhiteSpace(txtWrongguesses.Text) ? charClicked.ToString() : "," + charClicked;
                //panel1.Invalidate();

                turn++;
                if (turn == form.clients.Count)
                {
                    turn = 0;
                }

                form.PlayerIP = (string)form.clients.ToArray()[turn];
                //form.sendPlayerIP();
                checkTurn();


                if (CurrentStates == States.RightLeg)
                {
                    lblInfo.Text = "You lose!";
                    lblInfo.ForeColor = Color.Red;
                    flowLayoutPanel1.Enabled = false;

                    // Reveal the word
                    for (int i = 0; i < currentWord.Length; i++)
                    {
                        if (labels[i].Text.Equals(DefaultChar))
                        {
                            labels[i].Text = currentWord[i].ToString();
                            labels[i].ForeColor = Color.Blue;
                        }
                    }
                    MessageBox.Show($"your score={Sscore}");
                    playAgain();
                }

                b.Enabled = false;
                flowLayoutPanel1.Enabled = false;

            }
            form.sendChar();
        }

        public void RecieveChar()
        {
            //SelectWord();
            stringClicked = form.word;
            Regex ipReg = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            if (!ipReg.IsMatch(stringClicked))
            {
                var btns = flowLayoutPanel1.Controls.OfType<Button>();
                foreach (Button b in btns)
                {
                    if (b.Text == stringClicked)
                    {
                        b.Enabled = false;
                        //MessageBox.Show(b.Text);
                    }
                }
                if ((currentWord = currentWord.ToUpper()).Contains(stringClicked))
                {
                    // char is there (right guess)
                    flowLayoutPanel1.Enabled = false;
                    lblInfo.Text = "Your Opponent's Turn!";
                    lblInfo.ForeColor = Color.Blue;
                    char[] charArray = currentWord.ToCharArray();
                    for (int i = 0; i < currentWord.Length; i++)
                    {
                        if (charArray[i] == stringClicked[0])
                            labels[i].Text = stringClicked[0].ToString();
                    }
                    // Winning condition               
                    if (!labels.Where(x => x.Text.Equals(DefaultChar)).Any())
                    {
                        //if (labels.Where(x => x.Text.Equals(DefaultChar)).Count() == 1)
                        //    MessageBox.Show("Last Character");
                        lblInfo.ForeColor = Color.Red;
                        lblInfo.Text = "You Lose";
                        flowLayoutPanel1.Enabled = false;
                        MessageBox.Show($"your score={Sscore}");
                        playAgain();
                    }
                }
                else
                {
                    flowLayoutPanel1.Enabled = true;
                    // Wrong guess
                    lblInfo.Text = "Your Turn";
                    lblInfo.ForeColor = Color.Brown;
                    if (CurrentStates != States.RightLeg)
                        CurrentStates++;
                    txtGuessesLeft.Text = (StatesSize - (int)CurrentStates).ToString();
                    txtWrongguesses.Text += string.IsNullOrWhiteSpace(txtWrongguesses.Text) ? stringClicked.ToString() : "," + stringClicked;
                   

                    // turn++;
                    // if (turn == form.clients.Count)
                    // {
                    //     turn = 0;
                    // }
                    // form.PlayerIP = (string)form.clients.ToArray()[turn];
                    //// form.sendPlayerIP();
                    // checkTurn();

                    if (CurrentStates == States.RightLeg)
                    {
                        lblInfo.Text = "You lose!";
                        lblInfo.ForeColor = Color.Red;
                        flowLayoutPanel1.Enabled = false;

                        // Reveal the word
                        for (int i = 0; i < currentWord.Length; i++)
                        {
                            if (labels[i].Text.Equals(DefaultChar))
                            {
                                labels[i].Text = currentWord[i].ToString();
                                labels[i].ForeColor = Color.Blue;
                            }
                        }
                        MessageBox.Show($"your score={Sscore}");
                        playAgain();
                    }
                }
            }
        }



        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        public void startGame()
        {
            //connectAsync();

            if (flowLayoutPanel1.Enabled)
                if (MessageBox.Show("Game in progress, wanna start again?", "Game in progress", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.Cancel)
                    return;

            ResetControls();
            //SelectWord();
            AddLabels();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //connectAsync();

            if (flowLayoutPanel1.Enabled)
                if (MessageBox.Show("Game in progress, wanna start again?", "Game in progress", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.Cancel)
                    return;

            ResetControls();
            //SelectWord();
            AddLabels();

        }


        public void playAgain()
        {
            //connectAsync();


            DialogResult d = (MessageBox.Show("DO YOU Want to play again?", "New Game", MessageBoxButtons.OKCancel));
            if (d == System.Windows.Forms.DialogResult.OK)
            {
                ResetControls();
                SelectWord();
                form.sendWord();
                AddLabels();
            }
            else
            {
                try
                {
                    if (form.Connection.Connected)
                    {
                        //reader.Close();
                        nstream.Close();
                        form.Connection.Shutdown(SocketShutdown.Both);
                        form.Connection.Close();
                        MessageBox.Show("Connection is Closed..");
                    }
                }
                catch (Exception exe) { MessageBox.Show(exe.ToString()); }
            }


        }

        private void AddLabels()
        {
            // Adding word to labels and labels to group Box
            groupBox1.Controls.Clear();
            labels.Clear();
            char[] wordChars = currentWord.ToCharArray();
            int len = wordChars.Length;
            int refer = groupBox1.Width / len;

            for (int i = 0; i < len; i++)
            {
                Label l = new Label();
                l.Text = DefaultChar;
                l.Location = new Point(10 + i * refer, groupBox1.Height - 30);
                l.Parent = groupBox1;
                l.BringToFront();
                labels.Add(l);
            }

            // Writting text boxes 
            txtWordLen.Text = len.ToString();
            txtGuessesLeft.Text = StatesSize.ToString();
        }

        private void ResetControls()
        {
            // Resetting things
            flowLayoutPanel1.Controls.Clear();
            AddButtons();
            CurrentStates = States.None;
            txtWrongguesses.Clear();
            lblInfo.Text = "";
            flowLayoutPanel1.Enabled = true;
        }

        /// <summary>
        /// Randomizes a word reading text file (Words.txt) from current directory (exe location)
        /// </summary>
        private void SelectWord()
        {
            string filePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), $"{cat}{dif}.txt");
            using (TextReader tr = new StreamReader(filePath, Encoding.ASCII))
            {
                Random r = new Random();
                var allWords = tr.ReadToEnd().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                currentWord = allWords[r.Next(0, allWords.Length - 1)];
            }

            //stringClicked = form.word;

        }
        public void win()

        {
            string path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "score.txt"); ;
            File.WriteAllText(path, String.Empty);
            TextWriter tw = new StreamWriter(path, true);
            Sscore = Sscore + 1;
            tw.WriteLine(Sscore);
            tw.Close();

        }

        public void score()
        {
            string filePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "score.txt");
            using (TextReader tr = new StreamReader(filePath, Encoding.ASCII))
            {

                var allWords = tr.ReadToEnd();
                Sscore = int.Parse(allWords);


            }
        }

        private void secretword_Load(object sender, EventArgs e)
        {
            cat = Form1.wordcategory;
            dif = Form1.worddifficulty;
        }

        private void scoreToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Your score is {Sscore}","Server Score",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

    }
}
