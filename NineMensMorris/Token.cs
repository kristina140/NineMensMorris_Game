using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NineMensMorris
{
    public class Token
    {
        public bool IsPlaced { get; private set; }
        public bool IsDiscarded { get; private set; }

        public Player Owner { get; private set; }

        public Token(Player owner)
        {
            IsPlaced = false;
            IsDiscarded = false;

            Owner = owner ?? throw new ArgumentNullException("Owner must be defined.");
        }

        public void Discard()
        {
            if (!IsPlaced || IsDiscarded)
            {
                throw new InvalidOperationException("This token can not be discarded");
            }

            IsDiscarded = true;
            IsPlaced = false;
        }

        public void Place()
        {
            if (IsPlaced || IsDiscarded)
            {
                throw new InvalidOperationException("Token can not be placed on board");
            }

            IsPlaced = true;
        }

        public void Reset()
        {
            IsPlaced = false;
            IsDiscarded = false;
        }

    }
}
