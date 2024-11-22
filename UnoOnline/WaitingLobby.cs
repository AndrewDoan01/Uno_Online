using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UnoOnline
{
    public partial class WaitingLobby : Form
    {
        public WaitingLobby()
        {
            InitializeComponent();
        }

        private void btnJoinGame_Click(object sender, EventArgs e)
        {
            ClientSocket.SendData("START;" + Program.player.Name);
        }
    }
}
