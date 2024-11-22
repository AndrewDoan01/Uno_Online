using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UnoOnline
{
    public partial class Menu : Form
    {
        ClientSocket clientSocket = new ClientSocket();
        
        public List<Player> players = new List<Player>();
        public Menu()
        {
            InitializeComponent();
            Connect();
        }
        //Thiết lập kết nối tới server
        private static void Connect()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress iPAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork); //Lấy ra IP dạng IPv4 từ host ở trên
            IPEndPoint serverEP = new IPEndPoint(iPAddress, 11000);

            ClientSocket.ConnectToServer(serverEP);
            //test
            ClientSocket.SendData("Hello from client");
        }
        private void BtnJoinGame_Click(object sender, EventArgs e)
        {
            ClientSocket.SendData("CONNECT;" + Program.player.Name);
            WaitingLobby waitingLobby = new WaitingLobby();
            waitingLobby.Show();
        }
    }
}
