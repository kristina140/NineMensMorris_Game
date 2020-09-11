using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NineMensMorris
{
    public class Player
    {
        public string Name { get; set; }
        public Color Color { get; set; }
        public int Id { get; private set; }

        public List<Token> Tokens { get; private set; }
        public int NotPlacedTokens => Tokens.Where(_ => !_.IsPlaced && !_.IsDiscarded).Count();
        public int PlacedTokens => Tokens.Where(_ => _.IsPlaced && !_.IsDiscarded).Count();
        public int DiscardedTokens => Tokens.Where(_ => _.IsDiscarded).Count();

        public Player(int id, Color color)
        {
            Id = id;
            Name = "Player " + id.ToString();
            Color = color;

            Tokens = new List<Token>();
            for (int i = 0; i < 9; i++)
            {
                Tokens.Add(new Token(this));
            }
        }

        public Token GetToken()
        {
            return Tokens.FirstOrDefault(_ => !_.IsPlaced && !_.IsDiscarded);
        }

        public void Reset()
        {
            Tokens.ForEach(_ => _.Reset());
        }
    }
}
