using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientSide
{
	public partial class ClientForm : Form
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
		public secretword h;
		public string word;
		public string currentPlayerIP;
		public string ip;

		public static string Category { get; set; }
		public static string Difficulty { get; set; }
		public static string Data { get; set; }
		public static string ClientName { get; set; }
		public TcpClient Client { get=> client; set=> client = value; }
		public byte[] Ipbytes { get=> ipbytes; set=> ipbytes = value; }
		public IPAddress IpAddress { get=> ipaddress; set=> ipaddress = value; }
		public int PortNo { get=> portNo; set=> portNo = value; }
		public Socket Connection { get=> connection; set=> connection = value; }
		[Obsolete]
		public ClientForm()
		{
			InitializeComponent();
			Ipbytes = new byte[] { 127,0,0,1 };
			IpAddress = new IPAddress(Ipbytes);
			PortNo = 7777;
			 ip = Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString();

			h = new secretword(this);
		}


		private void button3_Click(object sender, EventArgs e)
		{
			connectionWorker1.RunWorkerAsync();
		}
		public void CloseConnection()
		{
			isConnected = false;
				reader.Close();
				writer.Close();
				Client.Close();
				MessageBox.Show("Connection is Closed..");
		}
		private void button4_Click(object sender, EventArgs e)
		{
			try {
				CloseConnection();
			

			}
			catch (Exception exe)
			{
				MessageBox.Show(exe.ToString());
			}
		}
		public void sendClientIP()
		{
			try
			{
				writer = new BinaryWriter(nstream);

				if (nstream.CanWrite)
				{
					writer.Write(ip);
					MessageBox.Show("IP Sent to server");
				}
			}
			catch (Exception) { MessageBox.Show("Server is not exist."); }
		}
		public void sendChar()
		{
			try
			{
				writer = new BinaryWriter(nstream);

				if (nstream.CanWrite)
				{
					writer.Write(h.CharClicked);
					MessageBox.Show("Character Sent to server");
				}
			}
			catch (Exception) { MessageBox.Show("Server is not exist."); }
		}

		private void button2_Click(object sender, EventArgs e)
		{
			try {
			if (nstream.CanWrite)
			{
				MessageBox.Show("Message Sent to server");

				}
				else
			{
				MessageBox.Show("Sorry.You cannot write to this NetworkStream.");
			}
			}catch(Exception exe) { MessageBox.Show("Server is disconnected."); }
		}
		
		public void Recieve()
		{
			while (isConnected)
			{
				reader = new BinaryReader(nstream);
				Data = reader.ReadString();

				string[] arr = Data.Split(',');
				word = arr[0];
				if (arr.Length != 1)
				{
					Category = arr[1];
					Difficulty = arr[2];
					ask();
				}

				if (word.Length == 1) h.RecieveChar();
				else secretword.currentWord = word;
				Invalidate();
				//MessageBox.Show(word);
				MessageBox.Show("Message Recieved from server");

			}
		}
		public void ask()
		{
			DialogResult d = MessageBox.Show($"Difficulty: {Difficulty} and Category: {Category} \n Do You agree?", "Game Rule", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (d == DialogResult.Yes)
			{
				MessageBox.Show("Connected..");
			}
			else
			{
				CloseConnection();
				Close();
				h.Close();
			}
		}
		public void RecievePlayerIP()
		{
	
			if (isConnected && InvokeRequired)
			{
				reader = new BinaryReader(nstream);
				currentPlayerIP = reader.ReadString();
				
				Invalidate();
				MessageBox.Show(currentPlayerIP);
				MessageBox.Show("IP Recieved from server");
			}
		}

		
		private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				Client = new TcpClient();
				Client.Connect(IpAddress, PortNo);
				isConnected = true;
				nstream = Client.GetStream();
				reader = new BinaryReader(nstream);
				writer = new BinaryWriter(nstream);
				RecieveWorker1.RunWorkerAsync();
				h.ShowDialog();
				MessageBox.Show("Connected..");

			}
			catch (Exception exe) { MessageBox.Show(exe.ToString()); }
		}

		private void RecieveWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
				Recieve();
		}

		private void ClientForm_Load(object sender, EventArgs e)
		{
			CheckForIllegalCrossThreadCalls = false;

		}

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
