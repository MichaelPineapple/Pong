using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Audio.OpenAL;
using MclTK.Audio;
using MclTK.Graphics;
using MclTK.Graphics.Shaders;
using MclTK.Window;

namespace Pong;

public class Pong
{
    private MclWindow win;
    
    private const float PADDLE_SPEED = 0.02f;
    private const float PADDLE_HEIGHT = 0.1f;
    private const float PADDLE_WIDTH = 0.01f;
    private const float PADDLE_X = 0.75f;
    private const float BALL_SIZE = 0.01f;

    private MclShaderDefault2D shaderDefault;

    private int vaoBall;
    private int vaoPadd;

    private int idSoundBall;
    
    private Vector3 posLeft = new Vector3(-PADDLE_X, 0.0f, 0.0f);
    private Vector3 posRigt = new Vector3(PADDLE_X, 0.0f, 0.0f);
    private Vector3 posBall = Vector3.Zero;
    private Vector3 velBall = new Vector3(0.006f, 0.004f, 0.0f);

    private int scoreLeft;
    private int scoreRigt;
    
    public Pong()
    {
        win = new MclWindow();
        win.ClientSize = (500, 500);
        win.Title = "Pong";
        win.UpdateFrequency = 120.0;
        win.FuncLoad = OnLoad;
        win.FuncUpdate = OnUpdate;
        win.FuncRender = OnRender;
    }

    public void Run()
    {
        win.Run();
        Console.WriteLine("GAME OVER");
        Console.WriteLine("Left:  "+scoreLeft);
        Console.WriteLine("Right: "+scoreRigt);
    }
    
    private void OnLoad()
    {
        shaderDefault = new MclShaderDefault2D();
        shaderDefault.Use();
        shaderDefault.SetColor(Vector3.One);

        float[] meshBall = MclGL.MakeRectangleMesh(BALL_SIZE, BALL_SIZE);
        float[] meshPadd = MclGL.MakeRectangleMesh(PADDLE_WIDTH, PADDLE_HEIGHT);

        vaoBall = MclGL.CreateVAO(meshBall, shaderDefault.handle);
        vaoPadd = MclGL.CreateVAO(meshPadd, shaderDefault.handle);

        AudioData audioData = CreateBallSound();
        idSoundBall = MclAL.CreateSource(audioData);
    }
    
    private void OnUpdate(double _dt)
    {
        KeyboardState keyboard = win.KeyboardState;
        
        InputData inputDataLeft = new InputData();
        InputData inputDataRigt = new InputData();
        
        inputDataLeft.upp = keyboard.IsKeyDown(Keys.W);
        inputDataLeft.dwn = keyboard.IsKeyDown(Keys.S);
        inputDataRigt.upp = keyboard.IsKeyDown(Keys.Up);
        inputDataRigt.dwn = keyboard.IsKeyDown(Keys.Down);
        inputDataLeft.end = inputDataRigt.end = keyboard.IsKeyDown(Keys.Escape);

        // if (net)
        // {
        //     if (host) inputDataRigt = NetRecieveInput(inputDataLeft);
        //     else inputDataLeft = NetTransmitInput(inputDataRigt);
        // }

        if (inputDataLeft.upp && posLeft.Y <  1.0f) posLeft.Y += PADDLE_SPEED;
        if (inputDataLeft.dwn && posLeft.Y > -1.0f) posLeft.Y -= PADDLE_SPEED;
        if (inputDataRigt.upp && posRigt.Y <  1.0f) posRigt.Y += PADDLE_SPEED;
        if (inputDataRigt.dwn && posRigt.Y > -1.0f) posRigt.Y -= PADDLE_SPEED;
        
        if (inputDataLeft.end || inputDataRigt.end) win.Close();
        
        HandleCollision();
        HandleScoring();
        
        posBall += velBall;
    }

    private void HandleCollision()
    {
        float[] ball = new[]
        {
            posBall.Y + BALL_SIZE,
            posBall.Y - BALL_SIZE,
            posBall.X + BALL_SIZE,
            posBall.X - BALL_SIZE,
        };

        if (ball[0] >= 1.0f || ball[1] <= -1.0f)
        {
            velBall.Y = -velBall.Y;
            AL.SourcePlay(idSoundBall);
        }
        
        HandlePaddleCollision(ball, posLeft);
        HandlePaddleCollision(ball, posRigt);
    }

    private void HandlePaddleCollision(float[] _ball, Vector3 _pos)
    {
        if (_ball[0] >= (_pos.Y - PADDLE_HEIGHT) && _ball[1] <= (_pos.Y + PADDLE_HEIGHT))
        {
            if (_ball[2] >= (_pos.X - PADDLE_WIDTH) && _ball[3] <= (_pos.X + PADDLE_WIDTH))
            {
                velBall.X = -velBall.X;
                AL.SourcePlay(idSoundBall);
            }
        }
    }

    private void HandleScoring()
    {
        if (posBall.X < -1.0f)
        {
            scoreRigt += 1;
            posBall = Vector3.Zero;
        }
        
        if (posBall.X > 1.0f)
        {
            scoreLeft += 1;
            posBall = Vector3.Zero;
        }
    }
    
    private void OnRender()
    {
        float aspectRatio = win.MclAspectRatio;
        Matrix4 proj = Matrix4.CreateOrthographicOffCenter(-aspectRatio, aspectRatio, -1.0f, 1.0f, 1.0f, -1.0f);
        shaderDefault.SetProjj(proj);
        
        GL.BindVertexArray(vaoBall);
        RenderVAO(posBall);
        
        GL.BindVertexArray(vaoPadd);
        RenderVAO(posLeft);
        RenderVAO(posRigt);
    }

    private void RenderVAO(Vector3 _pos)
    {
        Matrix4 model = Matrix4.CreateTranslation(_pos);
        shaderDefault.SetModel(model);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }
    
    private AudioData CreateBallSound()
    {
        int freq0 = 220;
        int sampleRate = 44100;
        short[] data = new short[4410];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (short)(MathF.Sin((i * freq0 * MathF.PI * 2) / sampleRate) * short.MaxValue);
        }

        AudioData output = new AudioData();
        output.dataShort = data;
        output.sampleRate = sampleRate;
        output.format = ALFormat.Mono16;
        output.useShort = true;

        return output;
    }
    
    private struct InputData
    {
        public bool upp, dwn, end;

        public InputData()
        {
            upp = dwn = end = false;
        }
    }
    
    public static void Main(String[] args)
    {
        Console.WriteLine("Pong!");
        new Pong().Run();
    }
}