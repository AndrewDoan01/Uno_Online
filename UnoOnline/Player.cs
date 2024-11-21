using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoOnline
{
    
    public class Player
    {
        public string Name { get; set; }
        public List<Card> Hand { get; set; }

        public bool IsTurn { get; set; }


        public Player() // Constructor mặc định 
        {
            Hand = new List<Card>();
        }

        public Player(string name)
        {
            Name = name;
            Hand = new List<Card>();
            IsTurn = false;
        }
    }
}
