using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using TysonTK;
using OpenTK;
using OpenTK.Input;

namespace Pong
{
    public class PongB
    {
        public static void Main()
        {

            bool loop = true;
            while (loop)
            {
                Console.Write("P#: ");
                int input = 0;
                string inputStr = Console.ReadLine();

                try
                {
                    input = Int32.Parse(inputStr);

                    if (input >= 0 && input <= 4)
                    {

                        PongB x = new PongB(input);
                        Tyson.LAUNCH(x.Load, x.Logic, x.Render, CMD, 1, 1, false, Color.Black, "Pong", 60);
                        loop = false;
                    }

                }
                catch { }
                Console.WriteLine("Invalid Input! Try again...");
            }
        }

        static void CMD(string str)
        {

        }


        // ---------------------


        int numOfp = 2;

        Key p1Up = Key.W, p1Down = Key.S,
            p2Up = Key.Up, p2Down = Key.Down,
            p3Up = Key.T, p3Down = Key.G,
            p4Up = Key.KeypadSubtract, p4Down = Key.KeypadPlus;

        double paddleXpos = 1000;

        List<Paddle> paddleList = new List<Paddle>();
        List<Ball> ballsList = new List<Ball>();
        bool debug = false, pause = false, start = true, toggle = false;

        public PongB(int num)
        {
            numOfp = num;
        }

        public void Load()
        {
            reset(numOfp);
        }

        public void Logic(MouseState mstat, KeyboardState kStat, double mx, double my)
        {
            if (start)
            {
                if (kStat[Key.Tab])
                {
                    if (!toggle)
                    {
                        if (pause) pause = false;
                        else pause = true;
                    }
                    toggle = true;
                }
                else toggle = false;

                if (!pause)
                {
                    for (int i = 0; i < paddleList.Count; i++)
                    {
                        paddleList[i].run(kStat, ballsList);
                    }
                    for (int i = 0; i < ballsList.Count; i++)
                    {
                        ballsList[i].run(paddleList);
                    }

                    for (int i = 0; i < paddleList.Count; i++)
                    {
                        paddleList[i].calcVel();
                    }
                }
            }
            else
            {
                if (kStat[Key.Space])
                {
                    start = true;
                }
            }
        }

        public void Render()
        {
            for (int i = 0; i < ballsList.Count; i++)
            {
                ballsList[i].draw();
            }
            for (int i = 0; i < paddleList.Count; i++)
            {
                paddleList[i].draw(debug);
            }
            if (pause) drawPauseSymbol(-PONG_KIT.WIN_W + 100, -PONG_KIT.WIN_H + 100, 50, PONG_KIT.STANDARD_COLOR);
        }


        // -------


        Paddle makePaddle(double _x, double _y, int _aiVal, bool _ai, Color _c, Key _ku, Key _kd)
        {
            return new Paddle(_x, _y, 10, 100, 8, _c, _ku, _kd, PONG_KIT.WIN_W, PONG_KIT.WIN_H, _ai, _aiVal);
        }
        Paddle makePaddle(double _x, double _y, Color _c, Key _ku, Key _kd)
        {
            return makePaddle(_x, _y, 0, false, _c, _ku, _kd);
        }
        Paddle makePaddle(double _x, Color _c, Key _ku, Key _kd)
        {
            return makePaddle(_x, 0, 0, false, _c, _ku, _kd);
        }
        Paddle makePaddleAI(double _x, int aiVal, Color _c)
        {
            return makePaddle(_x, 0, aiVal, true, _c, Key.Z, Key.M);
        }
        void addBall(double x, double y, double xv, double yv)
        {
            ballsList.Add(new Ball(x, y, 10, 10, PONG_KIT.STANDARD_COLOR, PONG_KIT.WIN_W * 2, PONG_KIT.WIN_H, xv, yv));
        }
        int getRND(int v)
        {
            return new Random().Next(-v, v);
        }

        void reset(int playerNum)
        {
            paddleList.Clear();
            ballsList.Clear();

            switch (playerNum)
            {
                case 0:
                    paddleList.Add(makePaddleAI(-paddleXpos, 100, PONG_KIT.APERTURE_BLUE));
                    paddleList.Add(makePaddleAI(paddleXpos, 100, PONG_KIT.APERTURE_ORANGE));
                    addBall(0, 0, -10, getRND(3));
                    break;


                case 1:
                    paddleList.Add(makePaddleAI(-paddleXpos, 100, PONG_KIT.APERTURE_BLUE));
                    paddleList.Add(makePaddle(paddleXpos, PONG_KIT.APERTURE_ORANGE, p2Up, p2Down));
                    addBall(0, 0, 10, getRND(3));
                    break;

                case 2:
                    paddleList.Add(makePaddle(-paddleXpos, PONG_KIT.APERTURE_BLUE, p1Up, p1Down));
                    paddleList.Add(makePaddle(paddleXpos, PONG_KIT.APERTURE_ORANGE, p2Up, p2Down));
                    addBall(0, 0, -10, getRND(3));
                    break;
                case 3:
                    paddleList.Add(makePaddle(-paddleXpos, 200, PONG_KIT.APERTURE_PURPLE, p1Up, p1Down));
                    paddleList.Add(makePaddle(-paddleXpos - 20, -200, PONG_KIT.APERTURE_CYAN, p3Up, p3Down));
                    paddleList.Add(makePaddle(paddleXpos, PONG_KIT.APERTURE_ORANGE, p2Up, p2Down));
                    addBall(-20, 20, -10, getRND(3));
                    addBall(20, -20, 10, getRND(3));
                    break;
                case 4:
                    paddleList.Add(makePaddle(-paddleXpos - 20, 200, PONG_KIT.APERTURE_CYAN, p1Up, p1Down));
                    paddleList.Add(makePaddle(-paddleXpos, -200, PONG_KIT.APERTURE_PURPLE, p3Up, p3Down));
                    paddleList.Add(makePaddle(paddleXpos, 200, PONG_KIT.APERTURE_RED, p2Up, p2Down));
                    paddleList.Add(makePaddle(paddleXpos + 20, -200, PONG_KIT.APERTURE_YELLOW, p4Up, p4Down));
                    addBall(-20, 20, -10, getRND(3));
                    addBall(-20, -20, -10, -getRND(3));
                    addBall(20, -20, 10, getRND(3));
                    addBall(20, 20, 10, -getRND(3));

                    break;
            }
        }

        void drawPauseSymbol(double x, double y, double size, Color c)
        {
            double q = size / 4, j = size / 2;
            Tyson.RenderBox(x - j, y, q, size, c);
            Tyson.RenderBox(x + j, y, q, size, c);
        }


        
    }

   

}
