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
            Connect();
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

        //Thiết lập kết nối tới server
        private static void Connect()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress iPAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork); //Lấy ra IP dạng IPv4 từ host ở trên
            IPEndPoint serverEP = new IPEndPoint(iPAddress, 11000);

            ClientSocket.ConnectToServer(serverEP);
            //test
            //
        }

        private void BtnJoinGame_Click(object sender, EventArgs e)
        {
            var message = new Message(MessageType.CONNECT, new List<string> { Program.player.Name });
            ClientSocket.SendData(message);
            WaitingLobby waitingLobby = new WaitingLobby();
            waitingLobby.Show();
            this.Hide();
        }
    }
}