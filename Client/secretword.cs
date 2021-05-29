using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ClientSide
{
    public partial class secretword : Form
    {
        TcpClient client;
        byte[] ipbytes;
        IPAddress ipaddress;
        int portNo;
        bool isConnected;
        Socket connection;
        NetworkStream nstream;
        BinaryReader reader;
        BinaryWriter writer;

        enum States
        {

            None,
            Piller,
            Rope,
            Head,CurrentStates,
            body,
            LeftHand,
            RightHand,
            LeftLeg,
            RightLeg
        }
		public string CharClicked { get; set; }
		public string stringClicked { get; set; }
		// Holds currnent word characters
		List<Label> labels = new List<Label>();
        // Word under consideration
        public static string currentWord { get; set; }
        public static string NewWord { get; set; }
        public static string temp { get; set; }
        // Default character for hidden word letters
        public string DefaultChar { get { return "__"; } }
        // Current States, used specially to repaint panel grphics
        private States CurrentStates = States.None;
        // States enum size
        public int StatesSize { get { return (Enum.GetValues(typeof(States)).Length - 1); } }
        public int Sscore { get; set; }
        // Global graphics data
        
        ClientForm form;
        int Flag =0;
        public secretword(ClientForm f)
        {
            InitializeComponent();
            AddButtons();
            ipbytes = new byte[] { 127, 0, 0, 1 };
            ipaddress = new IPAddress(ipbytes);
            portNo = 7777;
            form = f;
            score();
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
                b.BackColor = Color.AliceBlue;                
                b.Click += b_Click; // Event hook-up
            }

            // Disabling buttons
            flowLayoutPanel1.Enabled = false;
        }
        /* private void RecieveAsync()
         {

                 if (isConnected && InvokeRequired)
                 {
                     reader = new BinaryReader(nstream);
                     MessageBox.Show(reader.ReadString());
                     //richTextBox1.Text = reader.ReadString();
                     Invalidate();
                     MessageBox.Show("Message Recieved from server");
                 }




         }

         private async void connectAsync()
         {
             try
             {
                 client = new TcpClient();
                 client.Connect(ipaddress, portNo);
                 isConnected = true;
                 nstream = client.GetStream();
                 reader = new BinaryReader(nstream);
                 writer = new BinaryWriter(nstream);
                  RecieveAsync();
                 MessageBox.Show("Connected..");
             }
             catch (Exception exe) { MessageBox.Show(exe.ToString()); }
         }*/
        public void RecieveChar()
        {
            
            if (NewWord != currentWord && NewWord != null)
            {
                temp = currentWord;
                currentWord = NewWord;
            }
            NewWord = currentWord;
            SelectWord();
            var btns= flowLayoutPanel1.Controls.OfType<Button>();
            if (flowLayoutPanel2.Visible == false)
            {
                btns = flowLayoutPanel1.Controls.OfType<Button>();
            }
            else { btns = flowLayoutPanel2.Controls.OfType<Button>();
                flowLayoutPanel1.Visible = false;
            }
            foreach (Button b in btns)
            {
                if (b.Text == stringClicked)
                {
                    b.Enabled = false;
                    //MessageBox.Show(b.Text);
                }
            }
            if (Flag >= 1)
            {
                currentWord = temp;
                
            }
                if ((currentWord = currentWord.ToUpper()).Contains(stringClicked))
            {
                flowLayoutPanel1.Enabled = false;
                flowLayoutPanel2.Enabled = false;
                // char is there (right guess)
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
                    lblInfo.ForeColor = Color.Red;
                    lblInfo.Text = "You Lose";
                    flowLayoutPanel1.Enabled = false;
                    flowLayoutPanel2.Enabled = false;
                    MessageBox.Show($"your score={Sscore}");

                    flowLayoutPanel1.Controls.Clear();
                    flowLayoutPanel2.Controls.Clear();
                    playAgain();
                    for (int i = (int)'A'; i <= (int)'Z'; i++)
                    {
                        Button b = new Button();
                        b.Text = ((char)i).ToString();
                        b.Parent = flowLayoutPanel2;
                        b.Font = new Font(FontFamily.GenericSansSerif, 20, FontStyle.Bold);
                        b.Size = new Size(40, 40);
                        b.BackColor = Color.AliceBlue;
                        b.Click += b_Click; // Event hook-up
                    }

                    // Disabling buttons
                    flowLayoutPanel1.Enabled = false;
                    flowLayoutPanel2.Enabled = false;


                    CurrentStates = States.None;
                    txtWrongguesses.Clear();
                    lblInfo.Text = "";

                    SelectWord();
                    currentWord = temp;
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
                        l.Parent = groupBox2;
                        l.BringToFront();
                        labels.Add(l);
                    }

                    // Writting text boxes 
                    txtWordLen.Text = len.ToString();
                    txtGuessesLeft.Text = StatesSize.ToString();
                    flowLayoutPanel2.Visible = true;
                    groupBox2.Visible = true;
                    Flag = Flag + 1;






                }
            } 
            else
            {
                // Wrong guess
                lblInfo.Text = "Your Turn";
                lblInfo.ForeColor = Color.Brown;
                if (CurrentStates != States.RightLeg)
                    CurrentStates++;
                txtGuessesLeft.Text = (StatesSize - (int)CurrentStates).ToString();
                txtWrongguesses.Text += string.IsNullOrWhiteSpace(txtWrongguesses.Text) ? stringClicked.ToString() : "," + stringClicked;
               



                if (CurrentStates == States.RightLeg)
                {
                    lblInfo.Text = "You lose!";
                    lblInfo.ForeColor = Color.Red;
                    flowLayoutPanel1.Enabled = false;
                    flowLayoutPanel2.Enabled = false;

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
                    ResetControls();

                    SelectWord();
                    AddLabels();
                }
                flowLayoutPanel1.Enabled = true;
                flowLayoutPanel2.Enabled = true;
            }

		}
        public void checkTurn()
        {

            if (form.ip == form.currentPlayerIP)
            {
                flowLayoutPanel1.Enabled = true;
            }
            else
            {
                flowLayoutPanel1.Enabled = false;
            }

        }
        void b_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            char charClicked = b.Text.ToCharArray()[0];
            CharClicked = charClicked.ToString();
            //MessageBox.Show(currentWord);

            if ((currentWord = currentWord.ToUpper()).Contains(charClicked))
            {
                flowLayoutPanel1.Enabled = true;
                flowLayoutPanel2.Enabled = true;
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
                    lblInfo.Text = "You win";
                    flowLayoutPanel1.Enabled = false;
                    flowLayoutPanel2.Enabled = false;
                    win();
                    MessageBox.Show($"your score={Sscore }");
                    playAgain();
                    ResetControls();

                    SelectWord();
                    AddLabels();
                }
            }
            else
            {
                // Wrong guess
                lblInfo.Text = "Worng Guess!";
                lblInfo.ForeColor = Color.Brown;
                if (CurrentStates != States.RightLeg)
                    CurrentStates++;
                txtGuessesLeft.Text = (StatesSize - (int)CurrentStates).ToString();
                txtWrongguesses.Text += string.IsNullOrWhiteSpace(txtWrongguesses.Text) ? charClicked.ToString() : "," + charClicked;
 
                if (CurrentStates == States.RightLeg)
                {
                    lblInfo.Text = "You lose!";
                    lblInfo.ForeColor = Color.Red;
                    flowLayoutPanel1.Enabled = false;
                    flowLayoutPanel2.Enabled = false;
                    MessageBox.Show($"your score={Sscore}");
                    playAgain();
                    ResetControls();

                    SelectWord();
                    AddLabels();
                }
                b.Enabled = false;
                flowLayoutPanel1.Enabled = false;
                flowLayoutPanel2.Enabled = false;
            }
            form.sendChar();
        }        
    

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        public void startGame()
        { 
            if (flowLayoutPanel1.Enabled)
                if (MessageBox.Show("Game in progress, wanna start again?", "Game in progress", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.Cancel)
                    return;

            ResetControls();
            form.Recieve();
            SelectWord();
            AddLabels();
        }
        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (flowLayoutPanel1.Enabled)
                if (MessageBox.Show("Game in progress, wanna start again?", "Game in progress", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.Cancel)
                    return;

            ResetControls();
           
            SelectWord();
            AddLabels();
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
            flowLayoutPanel1.Controls.Clear();
            AddButtons();
            CurrentStates = States.None;
            txtWrongguesses.Clear();
            lblInfo.Text = "";
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
        private void SelectWord()
        {
			try { 
            if (form.word.Length > 1)
                stringClicked = form.word;
            else stringClicked = form.word;


            }catch(Exception ex) { MessageBox.Show(ex.Message); }
        }


        public void playAgain()
        {
            DialogResult d = (MessageBox.Show("DO YOU Want to play again?", "New Game", MessageBoxButtons.OKCancel));
            if (d == System.Windows.Forms.DialogResult.OK)
            {

            }
            else
            {
                this.Close();
            }


        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Your score is {Sscore}", "Client Score", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
