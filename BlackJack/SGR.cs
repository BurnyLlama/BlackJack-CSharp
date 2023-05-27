using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack
{
    /// <summary>
    /// SGR means Set Graphics Rendition and is a way to set different rendering options
    /// for text printed to the console via escape characters and codes.
    /// </summary>
    internal static class SGR
    {
        public static string Reset = "\x1b[0m";

        /*
            THESE ARE NICER, BUT LAGGY!
            Like... takes many milliseconds to render :c
         */
        //public static string Black = "\x1b[38;2;15;15;36m";
        //public static string Red = "\x1b[38;2;224;79;83m";
        //public static string Orange = "\x1b[38;2;245;163;0m";
        //public static string Yellow = "\x1b[38;2;240;217;17m";
        //public static string Green = "\x1b[38;2;100;219;79m";
        //public static string Cyan = "\x1b[38;2;47;235;235m";
        //public static string Blue = "\x1b[38;2;68;166;252m";
        //public static string Magenta = "\x1b[38;2;217;67;217m";
        //public static string White = "\x1b[38;2;206;206;227";
        //public static string BrightBlack = "\x1b[38;2;59;59;94m";
        //public static string BrightRed = "\x1b[38;2;250;152;154m";
        //public static string BrightOrange = "\x1b[38;2;252;205;111m";
        //public static string BrightYellow = "\x1b[38;2;255;242;125m";
        //public static string BrightGreen = "\x1b[38;2;177;240;165m";
        //public static string BrightCyan = "\x1b[38;2;148;247;246m";
        //public static string BrightBlue = "\x1b[38;2;157;208;252m";
        //public static string BrightMagenta = "\x1b[38;2;252;162;252m";
        //public static string BrightWhite = "\x1b[38;2;250;250;255m";

        public static string Black      = "\x1b[30m";
        public static string Red        = "\x1b[31m";
        public static string Green      = "\x1b[32m";
        public static string Yellow     = "\x1b[33m";
        public static string Blue       = "\x1b[34m";
        public static string Magenta    = "\x1b[35m";
        public static string Cyan       = "\x1b[36m";
        public static string White      = "\x1b[37m";

        public static string BrightBlack    = "\x1b[90m";
        public static string BrightRed      = "\x1b[91m";
        public static string BrightGreen    = "\x1b[92m";
        public static string BrightYellow   = "\x1b[93m";
        public static string BrightBlue     = "\x1b[94m";
        public static string BrightMagenta  = "\x1b[95m";
        public static string BrightCyan     = "\x1b[96m";
        public static string BrightWhite    = "\x1b[97m";

        public static string BG_Black   = "\x1b[40m";
        public static string BG_Red     = "\x1b[41m";
        public static string BG_Green   = "\x1b[42m";
        public static string BG_Yellow  = "\x1b[43m";
        public static string BG_Blue    = "\x1b[44m";
        public static string BG_Magenta = "\x1b[45m";
        public static string BG_Cyan    = "\x1b[46m";
        public static string BG_White   = "\x1b[47m";

        public static string BG_BrightBlack   = "\x1b[100m";
        public static string BG_BrightRed     = "\x1b[101m";
        public static string BG_BrightGreen   = "\x1b[102m";
        public static string BG_BrightYellow  = "\x1b[103m";
        public static string BG_BrightBlue    = "\x1b[104m";
        public static string BG_BrightMagenta = "\x1b[105m";
        public static string BG_BrightCyan    = "\x1b[106m";
        public static string BG_BrightWhite   = "\x1b[107m";

        public static string Bold       = "\x1b[1m";
        public static string Dim        = "\x1b[2m";
        public static string Italic     = "\x1b[3m";
        public static string Underline  = "\x1b[4m";

    }
}
