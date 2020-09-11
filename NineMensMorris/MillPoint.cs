using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NineMensMorris
{
    //
    // Summary:
    //     Describes a point/intersection in game.
    public class MillPoint
    {
        public Point Id { get; private set; }

        public Point Center { get; private set; }
        public readonly int Radius = 33;
        public Rectangle AcceptanceArea { get; private set; }

        public bool IsOccupied { get; private set; }
        public Token Token { get; private set; }

        public MillPoint NeighbourUp { get; private set; }
        public MillPoint NeighbourRight { get; private set; }
        public MillPoint NeighbourDown { get; private set; }
        public MillPoint NeighbourLeft { get; private set; }

        public MillPoint(Point id, int centerX, int centerY)
        {
            Id = id;
            Center = new Point(centerX, centerY);
            AcceptanceArea = new Rectangle(centerX - Radius, centerY - Radius, Radius*2, Radius*2);
            Token = null;
            IsOccupied = false;
        }

        public void SetNeighbours(MillPoint neighbourUp, MillPoint neighbourRight, MillPoint neighbourDown, MillPoint neighbourLeft)
        {
            NeighbourUp = neighbourUp;
            NeighbourRight = neighbourRight;
            NeighbourDown = neighbourDown;
            NeighbourLeft = neighbourLeft;
        }

        public bool Contains(Point point)
        {
            return AcceptanceArea.Contains(point);
        }

        public void PlaceToken(Token token)
        {
            if (token != null && Token == null)
            {
                Token = token;
                IsOccupied = true;
                token.Place();
            }
        }
        public void DiscardToken()
        {
            if (Token != null)
            {
                Token.Discard();
                Token = null;
            }
            IsOccupied = false;
        }
        public void RemoveToken()
        {
            if (Token != null)
            {
                Token.Reset();
                Token = null;
            }
            IsOccupied = false;
        }


        //Returns number of mills this point is a part of.
        public int GetNumberOfMills()
        {
            if (!IsOccupied) return 0;
            var playerId = Token.Owner.Id;

            //horizontal count
            var horizontalTokens = 1;

            //to the left
            var nextMillPoint = NeighbourLeft;
            while (nextMillPoint != null)
            {
                if (nextMillPoint.IsOccupied && nextMillPoint.Token.Owner.Id == playerId)
                    horizontalTokens += 1;
                nextMillPoint = nextMillPoint.NeighbourLeft;
            }

            //to the right
            nextMillPoint = NeighbourRight;
            while (nextMillPoint != null)
            {
                if (nextMillPoint.IsOccupied && nextMillPoint.Token.Owner.Id == playerId)
                    horizontalTokens += 1;
                nextMillPoint = nextMillPoint.NeighbourRight;
            }

            //vertical count
            var verticalTokens = 1;

            //up
            nextMillPoint = NeighbourUp;
            while (nextMillPoint != null)
            {
                if (nextMillPoint.IsOccupied && nextMillPoint.Token.Owner.Id == playerId)
                    verticalTokens += 1;
                nextMillPoint = nextMillPoint.NeighbourUp;
            }

            //down
            nextMillPoint = NeighbourDown;
            while (nextMillPoint != null)
            {
                if (nextMillPoint.IsOccupied && nextMillPoint.Token.Owner.Id == playerId)
                    verticalTokens += 1;
                nextMillPoint = nextMillPoint.NeighbourDown;
            }

            //total
            var numberOfMills = 0;
            if (horizontalTokens == 3) numberOfMills += 1;
            if (verticalTokens == 3) numberOfMills += 1;

            return numberOfMills;
        }

        public bool CanMove()
        {
            var canMoveLeft = NeighbourLeft != null && !NeighbourLeft.IsOccupied;
            var canMoveRight = NeighbourRight != null && !NeighbourRight.IsOccupied;
            var canMoveUp = NeighbourUp != null && !NeighbourUp.IsOccupied;
            var canMoveDown = NeighbourDown != null && !NeighbourDown.IsOccupied;

            return canMoveLeft || canMoveRight || canMoveUp || canMoveDown;
        }
    }
}
