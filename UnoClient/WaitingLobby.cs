using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace UnoOnline
{
    public partial class WaitingLobby : Form
    {
        public WaitingLobby()
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

        private void btnJoinGame_Click(object sender, EventArgs e)
        {
            var message = new Message(MessageType.START, new List<string> { Program.player.Name });
            ClientSocket.SendData(message);
        }

        private void WaitingLobby_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Send disconnect message to the server
                var message = new Message(MessageType.DISCONNECT, new List<string> { Program.player.Name });
                ClientSocket.SendData(message);

                // Close the client socket
                ClientSocket.Disconnect();
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                MessageBox.Show($"Error during disconnect: {ex.Message}");
            }
        }
    }
}