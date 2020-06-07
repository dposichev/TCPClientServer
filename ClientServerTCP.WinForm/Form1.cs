using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientServerTCP.WinForm
{
    public partial class Form1 : Form
    {
        Server2 _server;
        Client2 _client2;

        public Form1()
        {
            InitializeComponent();

            _server = new Server2();

            _server.OnClientConnect += Server_OnClientConnect;
            _server.OnGetMessage += Server_OnGetMessage;

            _client2 = new Client2();
            _client2.OnConnected += _client2_OnConnected;
            _client2.OnGetMessage += _client2_OnGetMessage;

            

            //Task.Factory.StartNew(InitialServer);
        }

        private void _client2_OnGetMessage(byte[] obj)
        {
            listBox2.Invoke(new Action(() => listBox2.Items.Add($"Recieve message: {Encoding.ASCII.GetString(obj)}")));
        }

        private void _client2_OnConnected()
        {
            listBox2.Invoke(new Action(() => listBox2.Items.Add("Connected")));
        }

        private void Server_OnGetMessage(byte[] obj)
        {
            listBox1.Invoke(new Action(() => listBox1.Items.Add($"Recieve message: {Encoding.ASCII.GetString(obj)}")));
        }

        private void Server_OnClientConnect()
        {
            listBox1.Invoke(new Action(()=>listBox1.Items.Add("Connect client")));
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            _server.InitialServer(Int32.Parse(textBox1.Text));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _server.SendData(Encoding.ASCII.GetBytes(textBox2.Text));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _client2.SendData(Encoding.ASCII.GetBytes(textBox3.Text));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _client2.InitialClient(textBox4.Text, Int32.Parse(textBox5.Text));
        }
    }
}
