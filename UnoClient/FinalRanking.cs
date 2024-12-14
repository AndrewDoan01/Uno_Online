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
    public partial class FinalRanking : Form
    {
        public FinalRanking()
        {
            InitializeComponent();
        }
        public static void DisplayRanking(List<Player> players)
        {
            // Sắp xếp danh sách người chơi theo điểm số giảm dần
            players.Sort((x, y) => y.Points.CompareTo(x.Points));
            // Hiển thị danh sách người chơi top 1 2 3 3 

        }
        private void FinalRanking_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
    }
}
