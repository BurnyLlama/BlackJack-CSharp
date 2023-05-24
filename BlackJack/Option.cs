using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack
{
    /// <summary>
    /// An option is a way to store an action the player can take.
    /// It requires the key that the player needs to press,
    /// a name, or more-so description, of what the action does,
    /// and lastly an action to actually execute if the options is selected.
    /// </summary>
    internal class Option
    {
        public readonly char KeyToPress;
        public readonly string Name;
        public readonly Action Func;

        public Option(char keyToPress, string name, Action func)
        {
            KeyToPress = keyToPress;
            Name = name;
            Func = func;
        }
    }
}
