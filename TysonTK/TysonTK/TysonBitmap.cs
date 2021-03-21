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
    public class TysonBitmap
    {

        Frame picture;

        bool anime = false;
        List<Frame> list_frames = new List<Frame>();
        int frameTimer;
        int frameIndex;


        public TysonBitmap(string ImageDirectory)
        {
            picture = new Frame(ImageDirectory, false);
        }
        public TysonBitmap(string ImageDirectory, bool UseSmoothing)
        {
            picture = new Frame(ImageDirectory, UseSmoothing);
        }
        public TysonBitmap(string ImageDirectory, bool UseSmoothing, int FrameCount)
        {
            for (int q = 0; q < FrameCount; q++)
            {
                list_frames.Add(new Frame(ImageDirectory + "/" + q.ToString() + ".png", UseSmoothing));
            }
            anime = true;
            picture = list_frames[0];
        }


        public void Render(double X, double Y, double W, double H)
        {
            picture.Render(X, Y, W, H, Color.White);
        }
        public void Render(double X, double Y, double W, double H, Color Hue)
        {
            picture.Render(X, Y, W, H, Hue);
        }
        public void Render(double X, double Y, double W, double H, double Angle, Color Hue)
        {
            picture.RenderFrameTheta(X, Y, W, H, Hue, Angle);
        }


        public void Render(double X, double Y, double W, double H, Color Hue, int Speed)
        {
            if (anime)
            {
                list_frames[frameIndex].Render(X, Y, W, H, Hue);

                if (frameTimer < 1)
                {
                    if (frameIndex == list_frames.Count - 1) frameIndex = -1;
                    frameIndex++;
                    frameTimer = Speed;
                }

                frameTimer--;
            }
        }
        public void Render(double X, double Y, double W, double H, Color Hue, double Angle, int Speed)
        {
            if (anime)
            {
                list_frames[frameIndex].RenderFrameTheta(X, Y, W, H, Hue, Angle);

                if (frameTimer < 1)
                {
                    if (frameIndex == list_frames.Count - 1) frameIndex = -1;
                    frameIndex++;
                    frameTimer = Speed;
                }

                frameTimer--;
            }
        }





        public class Frame
        {
            int image_id = 0;

            public Frame(string dir, bool _smooth)
            {
                if (_smooth)
                {
                    Bitmap image = new Bitmap(dir);
                    GL.GenTextures(1, out image_id);
                    BitmapData bitmapdata;
                    Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                    bitmapdata = image.LockBits(rect,
                        System.Drawing.Imaging.ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    GL.BindTexture(TextureTarget.Texture2D, image_id);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                        (OpenTK.Graphics.OpenGL.PixelFormat)(int)All.Bgra, PixelType.UnsignedByte, bitmapdata.Scan0);
                }
                else
                {
                    Bitmap image = new Bitmap(dir);
                    GL.GenTextures(1, out image_id);
                    BitmapData bitmapdata;
                    Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                    bitmapdata = image.LockBits(rect,
                        System.Drawing.Imaging.ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    GL.BindTexture(TextureTarget.Texture2D, image_id);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                        (OpenTK.Graphics.OpenGL.PixelFormat)(int)All.Bgra, PixelType.UnsignedByte, bitmapdata.Scan0);
                }
            }


            public void Render(double x, double y, double w, double h, Color c)
            {
                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, this.image_id);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                GL.Begin(PrimitiveType.Quads);
                GL.Color4(c);
                GL.TexCoord2(0, 1); GL.Vertex2(x - w, y - h);
                GL.TexCoord2(1, 1); GL.Vertex2(x + w, y - h);
                GL.TexCoord2(1, 0); GL.Vertex2(x + w, y + h);
                GL.TexCoord2(0, 0); GL.Vertex2(x - w, y + h);
                GL.End();
                GL.Disable(EnableCap.Texture2D);
                GL.Flush();
            }
            public void RenderFrameTheta(double x, double y, double w_, double h_, Color c, double theta)
            {
                double ja = theta - Tyson.PI_OVER_FOUR;
                double jb = theta - Tyson.THREE_PI_OVER_EIGHT;
                double jc = theta + Tyson.THREE_PI_OVER_EIGHT;
                double jd = theta + Tyson.PI_OVER_FOUR;
                double w = w_ * 1.42;
                double h = h_ * 1.42;

                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, this.image_id);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                GL.Begin(PrimitiveType.Quads);
                GL.Color4(c);
                GL.TexCoord2(1, 1); GL.Vertex2(x + (Math.Cos(ja) * w), y + (Math.Sin(ja) * h));
                GL.TexCoord2(0, 1); GL.Vertex2(x + (Math.Cos(jb) * w), y + (Math.Sin(jb) * h));
                GL.TexCoord2(0, 0); GL.Vertex2(x + (Math.Cos(jc) * w), y + (Math.Sin(jc) * h));
                GL.TexCoord2(1, 0); GL.Vertex2(x + (Math.Cos(jd) * w), y + (Math.Sin(jd) * h));
                GL.End();
                GL.Disable(EnableCap.Texture2D);
                GL.Flush();
            }

            public int getID()
            {
                return this.image_id;
            }
        }


    }
}
