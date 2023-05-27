using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack
{
    internal class Card
    {
        public enum EHouses { Spades, Hearts, Clubs, Diamonds }
        // This is pretty nasty, but an enum value cannot be a number...
        public enum EValues { A, V2, V3, V4, V5, V6, V7, V8, V9, V10, J, Q, K }

        public readonly EHouses House;
        public readonly EValues Value;

        public readonly int Points;

        public Card(EHouses house, EValues value)
        {
            House = house;
            Value = value;
            Points = _pointsFromValue(value);
        }

        private static int _pointsFromValue(EValues value)
        {
            // This goes through all possible values, so you can't reach a default of 0.
            switch (value)
            {
                case EValues.A: return 1;
                case EValues.V2: return 2;
                case EValues.V3: return 3;
                case EValues.V4: return 4;
                case EValues.V5: return 5;
                case EValues.V6: return 6;
                case EValues.V7: return 7;
                case EValues.V8: return 8;
                case EValues.V9: return 9;
                case EValues.V10:
                case EValues.J:
                case EValues.Q:
                case EValues.K:
                    return 10;
                default: return 0;
            }
        }

        /// <summary>
        /// Create a properly/nicely formatted string out of the card.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string house;
            switch (House)
            {
                case EHouses.Spades: 
                    house = $"{SGR.Black}>";
                    break;
                case EHouses.Hearts:
                    house = $"{SGR.Red}<";
                    break;
                case EHouses.Clubs:
                    house = $"{SGR.Black}%";
                    break;
                case EHouses.Diamonds:
                    house = $"{SGR.Red}+";
                    break;
                default:
                    house = "-";
                    break;
            }

            string value;
            switch (Value)
            {
                case EValues.A: 
                    value = "1";
                    break;
                case EValues.V2:
                    value = "2";
                    break;
                case EValues.V3:
                    value = "3";
                    break;
                case EValues.V4:
                    value = "4";
                    break;
                case EValues.V5:
                    value = "5";
                    break;
                case EValues.V6:
                    value = "6";
                    break;
                case EValues.V7:
                    value = "7";
                    break;
                case EValues.V8:
                    value = "8";
                    break;
                case EValues.V9:
                    value = "9";
                    break;
                case EValues.V10:
                    value = "10";
                    break;
                case EValues.J:
                    value = "J";
                    break;
                case EValues.Q:
                    value = "Q";
                    break;
                case EValues.K:
                    value = "K";
                    break;
                default: 
                    value = "-";
                    break;
            }

            return $"{SGR.BG_BrightWhite}{SGR.Black}[{house}{value}{SGR.Black}]{SGR.Reset}";
        }
    }
}
