using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Pong;

public class Pong : GameWindow
{
    private const float PADDLE_SPEED = 0.001f;
    private const float PADDLE_HEIGHT = 0.1f;
    private const float PADDLE_WIDTH = 0.01f;
    private const float PADDLE_X = 0.75f;
    private const float BALL_SIZE = 0.01f;
    
    private readonly float[] verticesPadd = new []
    {
        -PADDLE_WIDTH,  PADDLE_HEIGHT,
         PADDLE_WIDTH, -PADDLE_HEIGHT,
        -PADDLE_WIDTH, -PADDLE_HEIGHT,
        
        -PADDLE_WIDTH,  PADDLE_HEIGHT,
         PADDLE_WIDTH,  PADDLE_HEIGHT,
         PADDLE_WIDTH, -PADDLE_HEIGHT,
    };
    
    private readonly float[] verticesBall = new []
    {
        -BALL_SIZE,  BALL_SIZE,
         BALL_SIZE, -BALL_SIZE,
        -BALL_SIZE, -BALL_SIZE,
        
        -BALL_SIZE,  BALL_SIZE,
         BALL_SIZE,  BALL_SIZE,
         BALL_SIZE, -BALL_SIZE,
    };

    private int shaderDefault;

    private int vaoBall;
    private int vaoPadd;
    
    private int ulModel;
    private int ulProjj;
    
    private Vector3 posLeft = new Vector3(-PADDLE_X, 0.0f, 0.0f);
    private Vector3 posRigt = new Vector3(PADDLE_X, 0.0f, 0.0f);
    private Vector3 posBall = Vector3.Zero;
    private Vector3 velBall = new Vector3(0.0003f, 0.0002f, 0.0f);

    private int scoreLeft;
    private int scoreRigt;

    public Pong() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        ClientSize = (1000, 1000);
        Title = "Pong";
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        
        CreateShader();
        vaoBall = CreateVAO(verticesBall);
        vaoPadd = CreateVAO(verticesPadd);
        
        ulModel = GL.GetUniformLocation(shaderDefault, "model");
        ulProjj = GL.GetUniformLocation(shaderDefault, "projj");
    }

    private void CreateShader()
    {
        const string shaderSourceVertex = 
            "#version 330 core \n" +
            "in vec3 vert;" +
            "uniform mat4 model;" +
            "uniform mat4 projj;" +
            "void main(){" +
            "gl_Position = vec4(vert, 1.0) * model * projj;" +
            "}";
        
        const string shaderSourceFragmt = 
            "#version 330 core \n" +
            "out vec4 FragColor;" +
            "void main(){" +
            "FragColor = vec4(1.0f, 1.0f, 1.0f, 1.0f);" +
            "}";
        
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        int fragmtShader = GL.CreateShader(ShaderType.FragmentShader);
        
        GL.ShaderSource(vertexShader, shaderSourceVertex);
        GL.ShaderSource(fragmtShader, shaderSourceFragmt);
        
        GL.CompileShader(vertexShader);
        GL.CompileShader(fragmtShader);
        
        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int succ1);
        GL.GetShader(fragmtShader, ShaderParameter.CompileStatus, out int succ2);
        
        if (succ1 == 0) Console.WriteLine(GL.GetShaderInfoLog(vertexShader));
        if (succ2 == 0) Console.WriteLine(GL.GetShaderInfoLog(fragmtShader));
        
        shaderDefault = GL.CreateProgram();

        GL.AttachShader(shaderDefault, vertexShader);
        GL.AttachShader(shaderDefault, fragmtShader);
        
        GL.LinkProgram(shaderDefault);
        GL.GetProgram(shaderDefault, GetProgramParameterName.LinkStatus, out int succ3);
        
        if (succ3 == 0) Console.WriteLine(GL.GetProgramInfoLog(shaderDefault));
        
        GL.DetachShader(shaderDefault, vertexShader);
        GL.DetachShader(shaderDefault, fragmtShader);
        
        GL.DeleteShader(fragmtShader);
        GL.DeleteShader(vertexShader);
    }
    
    private int CreateVAO(float[] _verticies)
    {
        const int typeSize = sizeof(float); 
        int VAO = GL.GenVertexArray();
        GL.BindVertexArray(VAO);
        int VBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, _verticies.Length * typeSize, _verticies, BufferUsageHint.DynamicDraw);
        int aVert = GL.GetAttribLocation(shaderDefault, "vert");
        GL.VertexAttribPointer(aVert, 2, VertexAttribPointerType.Float, false, 2 * typeSize, 0);
        GL.EnableVertexAttribArray(aVert);
        return VAO;
    }
    
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);    
        
        if (KeyboardState.IsKeyDown(Keys.Escape)) Close();

        if (KeyboardState.IsKeyDown(Keys.W) && posLeft.Y < 1.0f) posLeft.Y += PADDLE_SPEED;
        if (KeyboardState.IsKeyDown(Keys.S) && posLeft.Y > -1.0f) posLeft.Y -= PADDLE_SPEED;
        if (KeyboardState.IsKeyDown(Keys.Up) && posRigt.Y < 1.0f) posRigt.Y += PADDLE_SPEED;
        if (KeyboardState.IsKeyDown(Keys.Down) && posRigt.Y > -1.0f) posRigt.Y -= PADDLE_SPEED;

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
        
        if (ball[0] >= 1.0f || ball[1] <= -1.0f) velBall.Y = -velBall.Y;
        
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
    
    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.UseProgram(shaderDefault);
        
        float aspectRatio = Size.X / (float)Size.Y;
        Matrix4 proj = Matrix4.CreateOrthographicOffCenter(-aspectRatio, aspectRatio, -1.0f, 1.0f, 1.0f, -1.0f);
        GL.UniformMatrix4(ulProjj, true, ref proj);
        
        RenderVAO(vaoBall, posBall);
        RenderVAO(vaoPadd, posLeft);
        RenderVAO(vaoPadd, posRigt);
        
        SwapBuffers();
    }

    private void RenderVAO(int _VAO, Vector3 _pos)
    {
        Matrix4 model = Matrix4.CreateTranslation(_pos);
        GL.UniformMatrix4(ulModel, true, ref model);
        GL.BindVertexArray(_VAO);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }
    
    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }
    
    protected override void OnUnload()
    {
        GL.DeleteProgram(shaderDefault);
        Console.WriteLine("Left:  "+scoreLeft);
        Console.WriteLine("Right: "+scoreRigt);
    }
    
    public static void Main(String[] args)
    {
        new Pong().Run();
    }
}