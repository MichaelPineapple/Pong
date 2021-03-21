using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace TysonTK
{

    public class InputString
    {
        string txt = "", latestChar = "";
        bool toggle = false;

        public string GetInput(KeyboardState _k)
        {
            if (checkInput(_k) == latestChar || latestChar == "")
            {
                if (toggle) doCalcInput(_k);
                if (checkInput(_k) == "") toggle = true;
                else toggle = false;
            }
            else doCalcInput(_k);
            return txt;
        }

        void doCalcInput(KeyboardState kek)
        {
            latestChar = checkInput(kek);
            Calc(latestChar);
        }

        string checkInput(KeyboardState k)
        {
            string exp = "";
            if (k[Key.LShift])
            {
                if (k[Key.Tilde]) exp = "~";
                else if (k[Key.Number3]) exp = "#";
                else if (k[Key.Plus]) exp = "=";
            }
            else
            {
                if (k[Key.Number1]) exp = "1";
                else if (k[Key.Number2]) exp = "2";
                else if (k[Key.Number3]) exp = "3";
                else if (k[Key.Number4]) exp = "4";
                else if (k[Key.Number5]) exp = "5";
                else if (k[Key.Number6]) exp = "6";
                else if (k[Key.Number7]) exp = "7";
                else if (k[Key.Number8]) exp = "8";
                else if (k[Key.Number9]) exp = "9";
                else if (k[Key.Number0]) exp = "0";
                else if (k[Key.Minus]) exp = "-";
                else if (k[Key.Plus]) exp = "+";
                else if (k[Key.Period]) exp = ".";
                else if (k[Key.Slash]) exp = "/";
            }

            if (k[Key.A]) exp = "A";
            else if (k[Key.B]) exp = "B";
            else if (k[Key.C]) exp = "C";
            else if (k[Key.D]) exp = "D";
            else if (k[Key.E]) exp = "E";
            else if (k[Key.F]) exp = "F";
            else if (k[Key.G]) exp = "G";
            else if (k[Key.H]) exp = "H";
            else if (k[Key.I]) exp = "I";
            else if (k[Key.J]) exp = "J";
            else if (k[Key.K]) exp = "K";
            else if (k[Key.L]) exp = "L";
            else if (k[Key.M]) exp = "M";
            else if (k[Key.N]) exp = "N";
            else if (k[Key.O]) exp = "O";
            else if (k[Key.P]) exp = "P";
            else if (k[Key.Q]) exp = "Q";
            else if (k[Key.R]) exp = "R";
            else if (k[Key.S]) exp = "S";
            else if (k[Key.T]) exp = "T";
            else if (k[Key.U]) exp = "U";
            else if (k[Key.V]) exp = "V";
            else if (k[Key.W]) exp = "W";
            else if (k[Key.X]) exp = "X";
            else if (k[Key.Y]) exp = "Y";
            else if (k[Key.Z]) exp = "Z";
            else if (k[Key.Space]) exp = "_";
            else if (k[Key.BackSpace]) exp = "[back]";

            return exp;
        }

        void Calc(string inString)
        {
            if (inString == "[back]") Backspace();
            else AddLetter(inString.ToLower());
        }

        public void SetString(string str)
        {
            txt = str;
        }
        public void AddLetter(string inLetter)
        {
            txt += inLetter;
        }
        public void Backspace()
        {
            if (txt != "" && txt != null) txt = txt.Remove(txt.Length - 1, 1);
        }
        public void Clear()
        {
            txt = "";
        }
    }

}
