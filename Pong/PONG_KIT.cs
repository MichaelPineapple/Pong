using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TysonTK;
using System.Drawing;
using OpenTK;
using OpenTK.Input;

namespace Pong
{
    public static class PONG_KIT
    {
        public static Color
            STANDARD_COLOR = Color.White,
            APERTURE_BLUE = Color.FromArgb(0, 129, 196),
            APERTURE_ORANGE = Color.FromArgb(238, 127, 27),
            APERTURE_PURPLE = Color.FromArgb(36, 1, 121),
            APERTURE_CYAN = Color.FromArgb(104, 204, 255),
            APERTURE_YELLOW = Color.FromArgb(240, 206, 89),
            APERTURE_RED = Color.FromArgb(138, 24, 24);

        public const double WIN_W = 1920, WIN_H = 1080;

        public static double trig_f(double c, Ball b)
        {
            double m = (b.yVel / b.xVel);
            return m * c + (b.y - m * b.x);
        }
    }

    public class Ball
    {
        public double x, y, size, xVel, yVel, speed, screenW, screenH;
        Color hue;

        public Ball(double _x, double _y, double _scale, double _s, Color _c, double _screenW, double _screenH, double _xv, double _yv)
        {
            x = _x;
            y = _y;
            speed = _s;
            hue = _c;
            size = _scale;
            xVel = _xv;
            yVel = _yv;
            screenW = _screenW;
            screenH = _screenH;
        }

        public void run(List<Paddle> list_paddles)
        {
            x += xVel;
            y += yVel;

            if (y + size > screenH || y - size < -screenH) bounceY();
            if (x + size > screenW || x - size < -screenW) bounceX();

            for (int i = 0; i < list_paddles.Count; i++)
            {
                if (y < (list_paddles[i].y + list_paddles[i].height) && y > (list_paddles[i].y - list_paddles[i].height))
                {
                    if (Math.Abs(x - list_paddles[i].x) < (list_paddles[i].width + size + 1))
                    {
                        if (Math.Abs(x - list_paddles[i].x) > (list_paddles[i].width + size - 1))
                        {
                            bounceX();
                            if (Math.Abs(list_paddles[i].yVel) > 0) yVel = list_paddles[i].yVel;
                            list_paddles[i].calcOffset();
                        }
                        else
                        {
                            if (x > list_paddles[i].x) x += size * 2;
                            else x -= size * 2;
                        }
                    }
                }
            }

        }

        public void draw()
        {
            Tyson.RenderBox(x, y, size, size, hue);
        }

        public void bounceX()
        {
            xVel = -xVel;
        }
        public void bounceY()
        {
            yVel = -yVel;
        }
    }

    public class Paddle
    {
        public double x, y, yVel, width, height, speed, oldY, screenH, screenW, aiRNDoffset;
        Color hue;
        Key upKey, downKey;
        bool AI = false;
        int aiOffset;
        Random rnd = new Random();

        public Paddle(double _x, double _y, double _w, double _h, double _s, Color _c, Key _up, Key _down, double _sw, double _sh, bool _ai, int _aiOffset)
        {
            x = _x;
            y = _y;
            yVel = 0;
            width = _w;
            height = _h + 20;
            speed = _s;
            hue = _c;
            upKey = _up;
            downKey = _down;
            screenH = _sh;
            screenW = _sw;
            AI = _ai;
            aiOffset = _aiOffset;
            calcOffset();
        }

        public void run(KeyboardState k, List<Ball> iBalls)
        {
            if (AI)
            {
                for (int i = 0; i < iBalls.Count; i++)
                {
                    double
                        intercept = PONG_KIT.trig_f(x, iBalls[i]),
                        offAbs = Math.Abs(aiRNDoffset),
                        diff = Math.Abs(iBalls[i].x - x);

                    if (diff < 1500 - offAbs && diff > 500 - offAbs)
                    {
                        setObjective(intercept + aiRNDoffset * 3);
                    }
                    else if (diff < 500 - offAbs)
                    {
                        setObjective(intercept);
                    }
                    else
                    {
                        setObjective(aiRNDoffset);
                    }
                }
            }
            else
            {
                if (k[upKey]) moveUp();
                if (k[downKey]) moveDown();
            }

        }

        public void draw(bool debug)
        {
            if (debug) Tyson.RenderBox(x, y, width, aiOffset * 2, Color.Green);
            Tyson.RenderBox(x, y, width, height - 20, hue);
            if (debug) Tyson.RenderBox(x, aiRNDoffset, 5, 5, Color.Orange);
        }

        void setObjective(double var)
        {
            if (y > var) moveDown();
            if (y < var) moveUp();
        }

        public void calcVel()
        {
            yVel = y - oldY;
            oldY = y;
        }

        public void calcOffset()
        {
            aiRNDoffset = new Random().Next(-aiOffset, aiOffset);
        }

        void moveUp()
        {
            if (y + height < screenH) y += speed;
        }
        void moveDown()
        {
            if (y - height > -screenH) y -= speed;
        }

    }

}
