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
    public class PumpkinFont
    {
        List<PumpkinLetter> List_Letters = new List<PumpkinLetter>();

        public double spacing, height, width;

        public PumpkinFont(string fontDir, string seed, double _spacing, double _height, double _width)
        {
            for (int x = 0; x < seed.Length; x++)
            {
                try
                {
                    List_Letters.Add(new PumpkinLetter(fontDir + seed[x].ToString() + ".png", seed[x].ToString()));
                }
                catch { }
            }
            try
            {
                List_Letters.Add(new PumpkinLetter(fontDir + "_.png", " "));
                List_Letters.Add(new PumpkinLetter(fontDir + "[fs].png", "/"));
            }
            catch { }

            this.spacing = _spacing;
            this.height = _height;
            this.width = _width;
        }
        // gross
        public PumpkinFont(string fontDir, string seed, double _spacing, double _height, double _width, bool FS)
        {
            for (int x = 0; x < seed.Length; x++) try { List_Letters.Add(new PumpkinLetter(fontDir + seed[x].ToString() + ".png", seed[x].ToString())); }
                catch { }
            try { List_Letters.Add(new PumpkinLetter(fontDir + "_.png", " ")); if (FS) List_Letters.Add(new PumpkinLetter(fontDir + "[fs].png", "/")); }
                catch { }
            this.spacing = _spacing; this.height = _height; this.width = _width;
        }

        public void RenderLetter(string _character, double x, double y, double w, double h, Color c)
        {
            for (int q = 0; q < List_Letters.Count; q++)
            {
                if (_character == List_Letters[q].getChar())
                {
                    GL.Enable(EnableCap.Texture2D);
                    GL.BindTexture(TextureTarget.Texture2D, List_Letters[q].getID());
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
            }
        }




        private struct PumpkinLetter
        {
            string character;
            int image_id;

            public PumpkinLetter(string bitmapDir, string _character)
            {
                Bitmap image = new Bitmap(bitmapDir);
                GL.GenTextures(1, out image_id);
                BitmapData bitmapdata;
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                bitmapdata = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.BindTexture(TextureTarget.Texture2D, image_id);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, 
                    (OpenTK.Graphics.OpenGL.PixelFormat)(int)All.Bgra, PixelType.UnsignedByte, bitmapdata.Scan0);

                this.character = _character;
            }




            public string getChar()
            {
                return this.character;
            }

            public int getID()
            {
                return this.image_id;
            }
        }

    }
}
