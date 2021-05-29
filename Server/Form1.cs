using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerSide
{
	public partial class Form1 : Form
	{
		TcpListener server;
		Byte[] ipbytes;
		IPAddress ipaddress;
		int portNo;
		Socket connection;
		NetworkStream nstream;
		BinaryReader reader;
		BinaryWriter writer;
		bool isServerRunning;
		public string word;
		public secretword h;
		string player;

		//string sentString;
		public TcpListener Server { get => server; set => server = value; }
		public Byte[] Ipbytes { get => ipbytes; set => ipbytes = value; }
		public IPAddress IpAddress { get => ipaddress; set => ipaddress = value; }
		public int PortNo { get => portNo; set => portNo = value; }
		public Socket Connection { get => connection; set => connection = value; }
		public string PlayerIP { get; set; }
		public ArrayList clients { get; set; }
		public static string wordcategory { get; set; }
		public static string worddifficulty { get; set; }
		public Form1()
		{
			InitializeComponent();
			Ipbytes = new Byte[] { 127,0,0,1};
			IpAddress = new IPAddress(Ipbytes);
			PortNo = 7777;
			Server = new TcpListener(IpAddress, PortNo);
			clients = new ArrayList();
			clients.Add(IpAddress.ToString());
			comboBox1.Items.Add("Fruits");
			comboBox1.Items.Add("Colors");
			comboBox1.Items.Add("Animals");
			comboBox2.Items.Add("Easy");
			comboBox2.Items.Add("Medium");
			comboBox2.Items.Add("Hard");
		}

		//connnect Async
		/*private async void connectAsync()
		{
			await Task.Run(async () =>
			{
				Server.Start();
				MessageBox.Show("Server Started..");
				Connection = Server.AcceptSocket();
				
		        MessageBox.Show(Connection.Connected.ToString());
				var clientIp = Connection.RemoteEndPoint as IPEndPoint;
				MessageBox.Show(clientIp.ToString());
				clients.Add(clientIp.Address);

				nstream = new NetworkStream(Connection);
				h = new Hangman(this);
				sendWord();
				h.ShowDialog();
				await RecieveAsync();

				//MessageBox.Show(Connection.Connected.ToString());


				isServerRunning = true;
				//MessageBox.Show("connection accepted");
				//await RecieveAsync();
			});
		}*/

		private void button3_ClickAsync(object sender, EventArgs e)
		{
			
			backgroundWorker2.RunWorkerAsync();
			wordcategory = comboBox1.SelectedItem.ToString();
			worddifficulty = comboBox2.SelectedItem.ToString();
			h = new secretword(this);
			///h.startGame();
			//connectAsync();

		}
		public void sendChar()
		{
			//MessageBox.Show(nstream.ToString());
			try
			{
				//nstream = new NetworkStream(Connection);
				writer = new BinaryWriter(nstream);

				if (nstream.CanWrite)
				{
					writer.Write(h.CharClicked);
					//MessageBox.Show(h.currentWord);
					//richTextBox1.Text = "";
					MessageBox.Show("Character Sent to client x");
					//writer.Close();
				}
			}
			catch (Exception) { MessageBox.Show("Client x is not exist."); }
		}
		public void sendPlayerIP()
		{
			try
			{
				//nstream = new NetworkStream(Connection);
				writer = new BinaryWriter(nstream);

				if (nstream.CanWrite)
				{
					writer.Write(PlayerIP.ToString());
					//MessageBox.Show(h.currentWord);
					//richTextBox1.Text = "";
					MessageBox.Show("IP Sent to client x");
					//writer.Close();
				}
			}
			catch (Exception) { MessageBox.Show("Player IP is not exist."); }

		}

		public void sendWord()
		{
			//MessageBox.Show(nstream.ToString());
			try
			{
				//nstream = new NetworkStream(Connection);
				writer = new BinaryWriter(nstream);

				if (nstream.CanWrite)
				{
					writer.Write($"{h.currentWord},{wordcategory},{worddifficulty}");
					//MessageBox.Show(h.currentWord);
					//richTextBox1.Text = "";
					MessageBox.Show("Message Sent to client x");
					//writer.Close();
				}
			}
			catch (Exception) { MessageBox.Show("Client x is not exist."); }
		}
		private void button1_Click(object sender, EventArgs e)
		{
			try
			{
				//nstream = new NetworkStream(Connection);
				writer = new BinaryWriter(nstream);

				if (nstream.CanWrite)
				{
					writer.Write(h.currentWord);
					//richTextBox1.Text = "";
					MessageBox.Show("Message Sent to client x");
					//writer.Close();
				}
			}
			catch (Exception) { MessageBox.Show("Client x is not exist."); }
		}
		public void RecieveClientIP()
		{

			if (isServerRunning && InvokeRequired)
			{
				reader = new BinaryReader(nstream);

				  player = reader.ReadString();

				Invalidate();
				MessageBox.Show(player);
				MessageBox.Show("IP Recieved from client x");
				clients.Add(player);
				foreach (string ip in clients)
				{
					MessageBox.Show("ip :" + ip);

				}
				MessageBox.Show(clients.Count.ToString());
			}
		}
		public void Recieve()
		{
			//richTextBox2.Text = "";

			//nstream = new NetworkStream(Connection);
			while (isServerRunning && InvokeRequired)
			{
			reader = new BinaryReader(nstream);

				word = reader.ReadString();
				 h.RecieveChar();
				
				Invalidate();
				//MessageBox.Show(word);
				MessageBox.Show("Message Recieved from client x");

			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			//Recieve();
			//reader.Close();
		}

		private void button4_Click(object sender, EventArgs e)
		{
			try
			{
				if (Connection.Connected)
				{
					//reader.Close();
					nstream.Close();
					Connection.Shutdown(SocketShutdown.Both);
					Connection.Close();
					MessageBox.Show("Connection is Closed..");
				}
			}
			catch (Exception exe) { MessageBox.Show(exe.ToString()); }
		}

		private void backgroundWorker2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			//Server Connection
			Server.Start();
			MessageBox.Show("Server Started..");
			Connection = Server.AcceptSocket();
			nstream = new NetworkStream(Connection);
			h = new secretword(this);
			sendWord();
			isServerRunning = true;
			//MessageBox.Show(Connection.Connected.ToString());
			//var clientIp = Connection.RemoteEndPoint as IPEndPoint;
			//MessageBox.Show(clientIp.ToString());
			//clients.Add(clientIp.Address);
			//form.clients.Add(form.IpAddress.ToString());
			
			reader = new BinaryReader(nstream);
			writer = new BinaryWriter(nstream);

			if (h.currentWord != string.Empty)
				backgroundWorker1.RunWorkerAsync();


			h.ShowDialog();
			//clients.Count


			//RecieveClientIP();
			//sendWord();
			//h.ShowDialog();


			//foreach (string ip in clients)
			//MessageBox.Show("IP: " + ip);
			//MessageBox.Show(Connection.Connected.ToString());





			/*Server.Start();
			MessageBox.Show("Server Started..");
			Connection = Server.AcceptSocket();
			nstream = new NetworkStream(Connection);
			isServerRunning = true;
			MessageBox.Show("connection accepted");
			backgroundWorker1.RunWorkerAsync();*/
		}
		/*	private async Task RecieveAsync()
			{
				await Task.Run(() => { Recieve();});
			}*/
		private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
	
			Recieve();

		}
		private async Task DoWorkAsync()
		{
			 await Task.Run(() => {
				Thread.Sleep(1000);
				 //return "Done with work!";
				 MessageBox.Show("Hello");
			});

		}
		private async void button5_Click(object sender, EventArgs e)
		{
			//this.Text = 
				await DoWorkAsync();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			CheckForIllegalCrossThreadCalls = false;

		}

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
