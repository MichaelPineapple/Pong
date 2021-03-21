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

namespace TysonTK.test
{
    internal class testClass
    {
        static int a = 0;
        static Color hue;

        static void load()
        {
            hue = Color.FromArgb(238, 127, 27);
        }

        static void logic(MouseState ms, KeyboardState ks, double mx, double my)
        {
            if (a > 500) a = 0;
            else a++;
        }

        static void render()
        {
            Tyson.RenderCircle(0, 0, a, a, hue);
        }

        static void cmdhandle(string arg)
        {
            if (arg == "lol") Tyson.ADD_CMD_LINE("wow ok");
        }

        public static void Main()
        {
            Tyson.LAUNCH(load, logic, render, false);
        }
    }
}
