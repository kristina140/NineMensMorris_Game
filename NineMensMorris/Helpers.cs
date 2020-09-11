using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NineMensMorris
{
    public enum GamePhase
    {
        Placing = 0,
        Moving = 1,
        Flying = 2,
        Discarding = 3,
        END = 4
    }

    public static class GameText
    {
        public static string GetGamePhase(GamePhase gamePhase)
        {
            var text = string.Empty;

            switch (gamePhase)
            {
                case GamePhase.Placing:
                    text = "Phase 1: Place your pieces";
                    break;
                case GamePhase.Moving:
                    text = "Phase 2: Move your pieces";
                    break;
                case GamePhase.Flying:
                    text = "Phase 3: Flying";
                    break;
                case GamePhase.Discarding:
                    text = "DISCARD phase: discard opponent's token";
                    break;
                case GamePhase.END:
                    text = "GAME OVER";
                    break;
                default:
                    break;
            }

            return text;
        }
    }
}
