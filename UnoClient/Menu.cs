using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UnoOnline
{
    public partial class Menu : Form
    {
        public List<Player> players = new List<Player>();

        public Menu()
        {
            InitializeComponent();
            ClientSocket.OnMessageReceived += ClientSocket_OnMessageReceived;
        }

        private void ClientSocket_OnMessageReceived(string message)
        {
            // Update the UI with the received message
            Invoke(new Action(() =>
            {
                // Display the message in a label or text box
                richTextBox1.Text = message;
            }));
        }

        private static void Connect()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress iPAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork); //Lấy ra IP dạng IPv4 từ host ở trên
            IPEndPoint serverEP = new IPEndPoint(iPAddress, 11000);

            ClientSocket.ConnectToServer(serverEP);

            var message = new Message(MessageType.CONNECT, new List<string> { Program.player.Name });
            ClientSocket.SendData(message);
        }

        private void BtnJoinGame_Click(object sender, EventArgs e)
        {
            Connect();
            WaitingLobby waitingLobby = new WaitingLobby();
            waitingLobby.Show();
            this.Hide();
        }

        private void Menu_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}