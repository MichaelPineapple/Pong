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
    public class Tyson
    {
        // PRIVATE CONSTANTS
        private const int WINDOW_STANDARD_WIDTH = 1500, WINDOW_STANDARD_HEIGHT = 845;
        private const double STANDARD_ORTH_X = 1920, STANDARD_ORTH_Y = 1080;

        private const int
            CMD_HUD_ALPHA_DEFAULT = 150,
            CMD_LINE_DUR_DEFAULT = 10000,
            CMD_TXT_ABSTRACT_SIZE_DEFAULT = 20;

        private static Color 
            CMD_TXT_HUE_SOLID_DEFAULT = Color.White, 
            CMD_HUE_SOLID_DEFAULT = Color.White, 
            CMD_TXT_HUE_GOOD_DEFAULT = Color.FromArgb(0, 129, 196), 
            CMD_TXT_HUE_BAD_DEFAULT = Color.Red;

        private const double CMD_SCALE_DEFAULT = 0.7;

        // PRIVATE VARS
        private static List<LineCMD> cmd_list = new List<LineCMD>();
        private static MouseState mStat;
        private static KeyboardState keyStat;
        private static PumpkinFont cmdFont, cursorFont;
        private static InputString cmdInStr = new InputString();
        private static TysonBitmap logoSplash;
        private static int winW, winH;
        private static bool cmdInput = false, exit = false;
        private static string inTxt = "", oldtxt = "";
        private static double cmdYshift = 0, mouseX = 0, mouseY = 0, orthR, orthL, orthU, orthD;
        private static bool[] toggle = new bool[10];

        // PRIVATE FUNCTIONS
        private static void PERFORM_COMMAND(string txt)
        {
            if (txt == "/exit") EXIT_GAME();
            else if (txt == "/resume") { PAUSE = false; ADD_CMD_LINE("logic resumed", CMD_TXT_HUE_GOOD); }
            else if (txt == "/pause") { PAUSE = true; ADD_CMD_LINE("logic paused", CMD_TXT_HUE_GOOD); }
            else if (txt == "/mouseon") { SHOW_MOUSE = true; ADD_CMD_LINE("mouse enabled", CMD_TXT_HUE_GOOD); }
            else if (txt == "/mouseoff") { SHOW_MOUSE = false; ADD_CMD_LINE("mouse disabled", CMD_TXT_HUE_GOOD); }
            else if (txt == "/cmdoff") { CMD_ON = false; PAUSE = false; }
            else if (txt == "/help")
            {
                ADD_CMD_LINE("/exit (exits program)", CMD_TXT_HUE_GOOD);
                ADD_CMD_LINE("/pause (pauses logic)", CMD_TXT_HUE_GOOD);
                ADD_CMD_LINE("/resume (resumes logic)", CMD_TXT_HUE_GOOD);
                ADD_CMD_LINE("/mouseon (activates mouse)", CMD_TXT_HUE_GOOD);
                ADD_CMD_LINE("/mouseoff (deactivates mouse)", CMD_TXT_HUE_GOOD);
                ADD_CMD_LINE("/cmdoff (deactivates the console)", CMD_TXT_HUE_GOOD);
            }
            else ADD_CMD_ERR("unrecognized command (" + txt + ")");
        }

      
        private static void emptyFunc(string lol) { }


        private static void setColors()
        {
            CMD_TXT_HUE_FADE = Color.FromArgb(CMD_HUD_ALPHA, CMD_TXT_HUE_SOLID);
            BUILD_TXT_HUE = Color.FromArgb(CMD_HUD_ALPHA, CMD_HUE_SOLID);
            SPIN_BOX_HUE = Color.FromArgb(CMD_HUD_ALPHA, CMD_HUE_SOLID);
        }

        // PUBLIC CONSTANTS
        public const double
            PI_OVER_FOUR = System.Math.PI / 4,
            THREE_PI_OVER_EIGHT = PI_OVER_FOUR * 3,
            PI_OVER_TWO = System.Math.PI / 2;

        public const string ENGINE_NAME = "Tyson 0.1.9";

        public static double getWinHeight() { return winH; }
        public static double getWinWith() { return winW; }
        public static double getOrthL() { return orthL; }
        public static double getOrthR() { return orthR; }
        public static double getOrthD() { return orthD; }
        public static double getOrthU() { return orthU; }

        // PUBLIC VARS (GLOBALS)
        public static bool SHOW_MOUSE = false, PAUSE = false, CMD_ON = true;

        public static int 
            CMD_HUD_ALPHA = CMD_HUD_ALPHA_DEFAULT, 
            CMD_LINE_DUR = CMD_LINE_DUR_DEFAULT; 

        public static Color 
            CMD_TXT_HUE_SOLID = CMD_TXT_HUE_SOLID_DEFAULT, 
            CMD_HUE_SOLID = CMD_HUE_SOLID_DEFAULT, 
            CMD_TXT_HUE_GOOD =  CMD_TXT_HUE_GOOD_DEFAULT, 
            CMD_TXT_HUE_BAD = CMD_TXT_HUE_BAD_DEFAULT;

        public static Color 
            CMD_TXT_HUE_FADE = Color.FromArgb(CMD_HUD_ALPHA, CMD_TXT_HUE_SOLID), 
            BUILD_TXT_HUE = Color.FromArgb(CMD_HUD_ALPHA, CMD_HUE_SOLID), 
            SPIN_BOX_HUE = Color.FromArgb(CMD_HUD_ALPHA, CMD_HUE_SOLID);

        public static double CMD_SCALE = CMD_SCALE_DEFAULT, FONT_SPACING_VERT = 75 * CMD_SCALE;
        
        // PUBLIC FUNCTIONS
        public static void RenderText(string txt, double X, double Y, double size, Color hue, PumpkinFont font)
        {
            double offset = 0;
            double offsetA = size * font.spacing;
            for (int i = 0; i < txt.Length; i++)
            {
                font.RenderLetter(txt[i].ToString(), X + (i * offsetA) - offset, Y, size * font.width, size * font.height, hue);

                switch (txt[i].ToString())
                {
                    case "f":
                    case "t":
                    case "r":
                    case "s":
                    case "k":
                        offset += (size * font.spacing) / 3;
                        break;
                    case "i":
                    case "l":
                        offset += (size * font.spacing) / 2;
                        break;
                    case "j":
                        offset += (size * font.spacing) / 3;
                        break;
                }

            }
        }

        public static void RenderCircle(double x, double y, double Scale, double inSmoothness, Color inColor)
        {
            GL.Begin(PrimitiveType.Polygon);
            GL.Color4(inColor);
            for (int a = 0; a < inSmoothness; a++) GL.Vertex2(x + Math.Cos(a) * Scale, y + Math.Sin(a) * Scale);
            GL.End();
        }

        public static void RenderPolygon(Color inColor, params Vector2[] inVerts)
        {
            GL.Begin(PrimitiveType.Polygon);
            GL.Color4(inColor);
            for (int a = 0; a < inVerts.Length; a++) GL.Vertex2(inVerts[a]);
            GL.End();
        }

        public static void RenderTriangle(double x, double y, double Scale, Color inColor)
        {
            GL.Begin(PrimitiveType.Triangles);
            GL.Color4(inColor);
            GL.Vertex2(x, y + Scale);
            GL.Vertex2(x - Scale, y - Scale);
            GL.Vertex2(x + Scale, y - Scale);
            GL.End();
        }

        public static void RenderBox(double x, double y, double W, double H, Color inColor)
        {
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(inColor);
            GL.Vertex2(x - W, y + H);
            GL.Vertex2(x - W, y - H);
            GL.Vertex2(x + W, y - H);
            GL.Vertex2(x + W, y + H);
            GL.End();
        }

        public static void RenderBoxTheta(double x, double y, double w_, double h_, Color c, double theta)
        {
            double
                ja = theta - PI_OVER_FOUR,
                jb = theta - THREE_PI_OVER_EIGHT,
                jc = theta + THREE_PI_OVER_EIGHT,
                jd = theta + PI_OVER_FOUR,
                w = w_ * 1.42, h = h_ * 1.42;

            GL.Begin(PrimitiveType.Quads);
            GL.Color4(c);
            GL.Vertex2(x + (Math.Cos(ja) * w), y + (Math.Sin(ja) * h));
            GL.Vertex2(x + (Math.Cos(jb) * w), y + (Math.Sin(jb) * h));
            GL.Vertex2(x + (Math.Cos(jc) * w), y + (Math.Sin(jc) * h));
            GL.Vertex2(x + (Math.Cos(jd) * w), y + (Math.Sin(jd) * h));
            GL.End();
        }

        public static void EXIT_GAME() { exit = true; }

        public static void ADD_CMD_LINE(string txt, Color hue, int fadeDur)
        {
            cmd_list.Add(new LineCMD(txt, hue, fadeDur));
            cmdYshift += FONT_SPACING_VERT;
            Console.WriteLine(txt);
        }
        public static void ADD_CMD_ERR(string txt) { ADD_CMD_LINE(txt, CMD_TXT_HUE_BAD); }
        public static void ADD_CMD_LINE(string txt) { ADD_CMD_LINE(txt, CMD_TXT_HUE_SOLID); }
        public static void ADD_CMD_LINE(string txt, Color hue) { ADD_CMD_LINE(txt, hue, CMD_LINE_DUR); }

        private static void REMOVE_CMD_LINE(int index)
        {
            cmd_list.RemoveAt(index);
            cmdYshift -= FONT_SPACING_VERT;
        }

        public static void ACTIVATE_CMD()
        {
            cmdInput = true;
            PAUSE = true;
        }
        public static void DEACTIVATE_CMD()
        {
            cmdInput = false;
            PAUSE = false;
        }

        












        // THE LAUNCH FUNCTION
       
        public static void LAUNCH(Action loadAction, Action<MouseState, KeyboardState, double, double> logicAction, Action renderAction, Action<string> cmdAction,
           int size, double orth, bool fullscreen, Color bgColor, string winName, int fps)
        {
            LAUNCH(loadAction, logicAction, renderAction, cmdAction, size, orth, fullscreen, bgColor, winName, fps, "lol idk");
        }
        public static void LAUNCH(Action loadAction, Action<MouseState, KeyboardState, double, double> logicAction, Action renderAction, bool fullscreen)
        {
            LAUNCH(loadAction, logicAction, renderAction, emptyFunc, 1, 1, fullscreen, Color.Black, "Game Window", 60, "lol idk");
        }
        public static void LAUNCH(Action loadAction, Action<MouseState, KeyboardState, double, double> logicAction, Action renderAction, Action<string> cmdAction,
            int size, double orth, bool fullscreen, Color bgColor, string winName, int fps, string fontDir)
        {
            Console.WriteLine("> launching...");
            orthR = 1920 * orth; orthL = -1920 * orth; orthU = 1080 * orth; orthD = -1080 * orth;
            winW = 1500 * size; winH = 845 * size;
            double cmdTxtX = orthL + 50, cmdTxtY = orthD + 100, cmdInputY = orthD + 50, boxX = orthR - 100, boxY = orthU - 100, boxAngle = 0;
            string buildName = ENGINE_NAME.ToLower() + " - " + winName.ToLower();
            int splashTimer = 0;

            using (var game = new GameWindow(winW, winH, OpenTK.Graphics.GraphicsMode.Default, winName + " - " + ENGINE_NAME))
            {
                game.Load += (sender, e) =>
                {
                    GL.Ortho(orthL, orthR, orthD, orthU, -1, 1);
                    if (fullscreen) game.WindowState = WindowState.Fullscreen;
                    GL.ClearColor(bgColor);
                    game.VSync = VSyncMode.Off;

                    try 
                    {
                        game.Icon = new Icon(fontDir + @"/icon.ico");
                        logoSplash = new TysonBitmap(fontDir + "/splash.png", true);
                        cmdFont = new PumpkinFont(fontDir, "#abcdefghijklmnopqrstuvwxyz1234567890().+-", 3*CMD_SCALE, 2*CMD_SCALE, 1.402*CMD_SCALE);
                        cursorFont = new PumpkinFont(fontDir, "^", 3.05 * CMD_SCALE, 2.5 * CMD_SCALE, 2 * CMD_SCALE, false); 
                    }
                    catch 
                    { 
                        Console.WriteLine("> Error: failed to load cmd font!");
                        Console.WriteLine("> cmd off");
                        CMD_ON = false;
                    }

                    loadAction();
                    Console.WriteLine("> initializing complete");
                    ADD_CMD_LINE("press / (forward slash) to open cmd");
                };

                game.Resize += (sender, e) => { GL.Viewport(0, 0, game.Width, game.Height); };

                game.UpdateFrame += (sender, e) =>
                {
                    mStat = Mouse.GetState();
                    keyStat = Keyboard.GetState();

                    if (exit || keyStat[Key.Escape]) game.Exit();

                    if (splashTimer >= 1000)
                    {
                        if (SHOW_MOUSE)
                        {
                            Mouse.SetPosition(500, 500);
                            System.Windows.Forms.Cursor.Hide();
                        }

                        mouseX = mStat.X;
                        mouseY = -mStat.Y;


                        if (CMD_ON)
                        {
                            if (keyStat[Key.Tilde])
                            {
                                if (!toggle[0])
                                {
                                    if (cmdInput) DEACTIVATE_CMD();
                                    else ACTIVATE_CMD();
                                }
                                toggle[0] = true;
                            }
                            else toggle[0] = false;

                            if (cmdInput)
                            {
                                if (keyStat[Key.Up]) cmdInStr.SetString(oldtxt);

                                if (inTxt.Length > 0)
                                {
                                    if (keyStat[Key.Enter])
                                    {
                                        if (!toggle[1])
                                        {
                                            ADD_CMD_LINE(inTxt);
                                            if (inTxt[0].ToString() == "/")
                                            {
                                                PERFORM_COMMAND(inTxt);
                                                cmdAction(inTxt);
                                            }
                                            oldtxt = inTxt;
                                            cmdInStr.Clear();
                                        }
                                        toggle[1] = true;
                                    }
                                    else toggle[1] = false;
                                }

                            }
                            else
                            {
                                if (keyStat[Key.Slash])
                                {
                                    ACTIVATE_CMD();
                                    cmdInStr.SetString("/");
                                }
                            }

                            if (!PAUSE) boxAngle += 0.1;
                        }

                        if (!PAUSE) logicAction(mStat, keyStat, mouseX, mouseY);
                    }
                };

                game.RenderFrame += (sender, e) =>
                {
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    if (splashTimer < 2000)
                    {
                        if (logoSplash != null) logoSplash.Render(0, 0, 500, 219);
                        else splashTimer = 3000;
                        splashTimer++;
                    }
                    else
                    {
                        renderAction();

                        if (CMD_ON)
                        {
                            RenderBoxTheta(boxX, boxY, 50, 50, SPIN_BOX_HUE, boxAngle);

                            for (int i = 0; i < cmd_list.Count; i++)
                            {
                                if (cmdInput) cmd_list[i].render(cmdTxtX, (cmdTxtY - (i * FONT_SPACING_VERT)) + cmdYshift, CMD_TXT_ABSTRACT_SIZE_DEFAULT, 255, cmdFont);
                                else cmd_list[i].render(cmdTxtX, (cmdTxtY - (i * FONT_SPACING_VERT)) + cmdYshift, CMD_TXT_ABSTRACT_SIZE_DEFAULT, CMD_HUD_ALPHA, cmdFont);
                                if (cmd_list[i].isDelete()) REMOVE_CMD_LINE(i);
                            }

                            if (cmdInput)
                            {
                                inTxt = cmdInStr.GetInput(keyStat);
                                RenderBox(0, orthD, orthL * 2, 100, Color.White);
                                RenderText(inTxt, cmdTxtX, cmdInputY, CMD_TXT_ABSTRACT_SIZE_DEFAULT, Color.Black, cmdFont);
                                RenderText(inTxt + "^", cmdTxtX, cmdInputY, CMD_TXT_ABSTRACT_SIZE_DEFAULT, Color.Red, cursorFont);
                            }

                            RenderText(buildName, cmdTxtX, boxY, 15, BUILD_TXT_HUE, cmdFont);
                        }

                        if (SHOW_MOUSE) RenderTriangle(mouseX, mouseY - CMD_TXT_ABSTRACT_SIZE_DEFAULT, CMD_TXT_ABSTRACT_SIZE_DEFAULT, Color.Red);

                    }

                    game.SwapBuffers();
                };
                Console.WriteLine("> done");
                game.Run(fps);
            }
        }







        internal class LineCMD
        {
            Color hue = Color.White;
            string txt = "";
            int timer = 0;
            int fade = 0;
            int duration = 10000;
            bool delete = false;

            public LineCMD(string _txt, Color _hue, int _dur)
            {
                hue = _hue;
                txt = _txt;
                fade = 0;
                duration = _dur;
            }

            public void render(double x, double y, double size, int alpha, PumpkinFont font)
            {
                int colorFadeVal = alpha - fade;

                if (colorFadeVal > 1) RenderText(txt, x, y, size, Color.FromArgb(colorFadeVal, hue), font);
                else delete = true;

                if (timer > duration) fade++;
                timer++;
            }

            public bool isDelete() { return delete; }
        }






    }


}
