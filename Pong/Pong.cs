using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Pong;

public class Pong : GameWindow
{
    private const float SPEED_PADDLE = 0.001f;
    private const float PADDLE_HEIGHT = 0.1f;
    private const float PADDLE_WIDTH = 0.01f;
    private const float PADDLE_X = 0.75f;
    private const float SPEED_BALL = 0.0002f;
    private const float BALL_SIZE = 0.01f;
    private float[] verticiesPaddle = new []
    {
        -PADDLE_WIDTH,  PADDLE_HEIGHT, 0.0f,    0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
         PADDLE_WIDTH, -PADDLE_HEIGHT, 0.0f,    0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 
        -PADDLE_WIDTH, -PADDLE_HEIGHT, 0.0f,    0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
        
        -PADDLE_WIDTH,  PADDLE_HEIGHT, 0.0f,    0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
         PADDLE_WIDTH,  PADDLE_HEIGHT, 0.0f,    0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
         PADDLE_WIDTH, -PADDLE_HEIGHT, 0.0f,    0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
    };
    
    private float[] verticiesBall = new []
    {
        -BALL_SIZE,  BALL_SIZE, 0.0f,    0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
         BALL_SIZE, -BALL_SIZE, 0.0f,    0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 
        -BALL_SIZE, -BALL_SIZE, 0.0f,    0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
        
        -BALL_SIZE,  BALL_SIZE, 0.0f,    0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
         BALL_SIZE,  BALL_SIZE, 0.0f,    0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
         BALL_SIZE, -BALL_SIZE, 0.0f,    0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
    };
    
    private Shader shaderDefault;
    private Mesh meshPaddle;
    private Mesh meshBall;
    
    private Vector3 leftPos = new Vector3(-PADDLE_X, 0.0f, 0.0f);
    private Vector3 rightPos = new Vector3(PADDLE_X, 0.0f, 0.0f);
    private Vector3 ballPos = Vector3.Zero;
    private Vector3 ballVel = new Vector3(SPEED_BALL, SPEED_BALL*1.1f, 0.0f);

    private int leftScore = 0;
    private int rightScore = 0;

    public Pong() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        ClientSize = (1000, 1000);
        Title = "Pong";
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        
        string pathShaders = "../../../shaders/";
        shaderDefault = new Shader(pathShaders + "Default.vert", pathShaders + "Default.frag");
        shaderDefault.Use();

        meshPaddle = new Mesh(verticiesPaddle, shaderDefault);
        meshBall = new Mesh(verticiesBall, shaderDefault);
    }
    
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);    
        
        if (KeyboardState.IsKeyDown(Keys.Escape)) Close();

        if (KeyboardState.IsKeyDown(Keys.W) && leftPos.Y < 1.0f) leftPos.Y += SPEED_PADDLE;
        if (KeyboardState.IsKeyDown(Keys.S) && leftPos.Y > -1.0f) leftPos.Y -= SPEED_PADDLE;
        if (KeyboardState.IsKeyDown(Keys.Up) && rightPos.Y < 1.0f) rightPos.Y += SPEED_PADDLE;
        if (KeyboardState.IsKeyDown(Keys.Down) && rightPos.Y > -1.0f) rightPos.Y -= SPEED_PADDLE;

        HandleCollision();
        HandleScoring();
        
        ballPos += ballVel;
    }

    private void HandleCollision()
    {
        float[] ball = new[]
        {
            ballPos.Y + BALL_SIZE,
            ballPos.Y - BALL_SIZE,
            ballPos.X + BALL_SIZE,
            ballPos.X - BALL_SIZE,
        };
        
        if (ball[0] >= 1.0f || ball[1] <= -1.0f) ballVel.Y = -ballVel.Y;
        
        HandlePaddleCollision(ball, leftPos);
        HandlePaddleCollision(ball, rightPos);
    }

    private void HandlePaddleCollision(float[] _ball, Vector3 _pos)
    {
        if (_ball[0] >= (_pos.Y - PADDLE_HEIGHT) && _ball[1] <= (_pos.Y + PADDLE_HEIGHT))
        {
            if (_ball[2] >= (_pos.X - PADDLE_WIDTH) && _ball[3] <= (_pos.X + PADDLE_WIDTH))
            {
                ballVel.X = -ballVel.X;
            }
        }
    }

    private void HandleScoring()
    {
        if (ballPos.X < -1.0f)
        {
            rightScore += 1;
            ballPos = Vector3.Zero;
        }
        
        if (ballPos.X > 1.0f)
        {
            leftScore += 1;
            ballPos = Vector3.Zero;
        }
    }
    
    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        shaderDefault.Use();
        
        int uHudColor = shaderDefault.getUniformLocation("color");
        GL.Uniform3(uHudColor, Vector3.One);
        meshPaddle.render(shaderDefault, leftPos, Vector3.Zero, 1.0f);
        meshPaddle.render(shaderDefault, rightPos, Vector3.Zero, 1.0f);
        meshBall.render(shaderDefault, ballPos, Vector3.Zero, 1.0f);
        
        SwapBuffers();
    }
    
    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }
    
    protected override void OnUnload()
    {
        shaderDefault.dispose();
    }
}