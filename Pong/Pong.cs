using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Audio.OpenAL;
using System.Net;
using System.Net.Sockets;

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

    private int idSoundBall;
    
    private Vector3 posLeft = new Vector3(-PADDLE_X, 0.0f, 0.0f);
    private Vector3 posRigt = new Vector3(PADDLE_X, 0.0f, 0.0f);
    private Vector3 posBall = Vector3.Zero;
    private Vector3 velBall = new Vector3(0.0003f, 0.0002f, 0.0f);

    private int scoreLeft;
    private int scoreRigt;

    private bool host;
    private bool net;
    private Socket? sock;
    private Stream? stream;
    
    public Pong(Socket? _sock, Stream? _stream, bool _host, bool _net) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        ClientSize = (1000, 1000);
        Title = "Pong";
        UpdateFrequency = 60.0;
        sock = _sock;
        stream = _stream;
        host = _host;
        net = _net;
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
        
        InitializeAudio();

        var soundData = CreateBallSound();
        idSoundBall = LoadSound(soundData.Item1, soundData.Item2);
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

        InputData inputDataLeft = new InputData();
        InputData inputDataRigt = new InputData();
        
        inputDataLeft.upp = KeyboardState.IsKeyDown(Keys.W);
        inputDataLeft.dwn = KeyboardState.IsKeyDown(Keys.S);
        inputDataRigt.upp = KeyboardState.IsKeyDown(Keys.Up);
        inputDataRigt.dwn = KeyboardState.IsKeyDown(Keys.Down);

        if (net)
        {
            if (host) inputDataRigt = NetRecieveInput(inputDataLeft);
            else inputDataLeft = NetTransmitInput(inputDataRigt);
        }

        if (inputDataLeft.upp && posLeft.Y <  1.0f) posLeft.Y += PADDLE_SPEED;
        if (inputDataLeft.dwn && posLeft.Y > -1.0f) posLeft.Y -= PADDLE_SPEED;
        if (inputDataRigt.upp && posRigt.Y <  1.0f) posRigt.Y += PADDLE_SPEED;
        if (inputDataRigt.dwn && posRigt.Y > -1.0f) posRigt.Y -= PADDLE_SPEED;
        
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
        Console.WriteLine("Left:  "+scoreLeft);
        Console.WriteLine("Right: "+scoreRigt);
        GL.DeleteProgram(shaderDefault);
        AL.DeleteSource(idSoundBall);
        ShutdownAudio();
    }
    
    // *** AUDIO ***
    
    private int LoadSound(short[] _data, int _freq)
    {
        int buffer = AL.GenBuffer();
        int source = AL.GenSource();
        AL.BufferData(buffer, ALFormat.Mono16, ref _data[0], _data.Length * sizeof(short), _freq);
        AL.Source(source, ALSourcei.Buffer, buffer);
        AL.Source(source, ALSourcef.Gain, 1.0f);
        AL.DeleteBuffer(buffer);
        return source;
    }

    private (short[], int) CreateBallSound()
    {
        int freq0 = 220;
        int sampleRate = 44100;
        short[] data = new short[4410];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (short)(MathF.Sin((i * freq0 * MathF.PI * 2) / sampleRate) * short.MaxValue);
        }
        return (data, 44100);
    }
    
    private void InitializeAudio()
    {
        ALDevice device = ALC.OpenDevice(null);
        ALContext context = ALC.CreateContext(device, (int[])null);
        ALC.MakeContextCurrent(context);
    }
    
    private void ShutdownAudio()
    {
        ALContext context = ALC.GetCurrentContext();
        ALDevice device = ALC.GetContextsDevice(context);
        ALC.MakeContextCurrent(ALContext.Null);
        ALC.DestroyContext(context);
        ALC.CloseDevice(device);
    }
    
    private byte NetTransmit(byte _transData)
    {
        if (stream == null) return 255;
        stream.Write(new [] { _transData }, 0, 1);
        byte[] reply = new byte[1];
        int k = stream.Read(reply, 0, 1);
        return reply[0];
    }

    private byte NetRecieve(byte _replyData)
    {
        if (sock == null) return 255;
        byte[] transData = new byte[1];
        sock.Receive(transData);
        sock.Send(new [] { _replyData });
        return transData[0];
    }
    
    private byte Encode(InputData _data)
    {
        byte b = 0;
        if (_data.upp) b += 2;
        if (_data.dwn) b += 1;
        return b;
    }

    private InputData Decode(byte _data)
    {
        InputData output = new InputData();
        output.upp = (_data & 2) != 0;
        output.dwn = (_data & 1) != 0;
        return output;
    }
    
    private InputData NetTransmitInput(InputData _clientInput)
    {
        byte b = Encode(_clientInput);
        byte serverReply = NetTransmit(b);
        InputData serverInput = Decode(serverReply);
        return serverInput;
    }

    private InputData NetRecieveInput(InputData _serverInput)
    {
        byte b = Encode(_serverInput);
        byte clientTrans = NetRecieve(b);
        InputData clientInput = Decode(clientTrans);
        return clientInput;
    }
    
    private struct InputData
    {
        public bool upp, dwn;

        public InputData()
        {
            upp = dwn = false;
        }
    }
    
    public static void Main(String[] args)
    {
        Console.WriteLine("Pong!");
        if (Prompt("Use Network?"))
        {
            Console.WriteLine("IP:");
            string? ip = Console.ReadLine();
            if (ip == null) return;
            LaunchNetworkMode(ip, 8001, Prompt("Host?"));
        }
        else new Pong(null, null, false, false).Run();
    }

    private static bool Prompt(string _txt)
    {
        Console.WriteLine(_txt);
        return (Console.ReadLine() == "y");
    }

    private static void LaunchNetworkMode(string _ip, int _port, bool _host)
    {
        try
        {
            if (_host)
            {
                TcpListener server = new TcpListener(IPAddress.Parse(_ip), _port);
                server.Start();
                Console.WriteLine("Waiting for connection...");
                Socket sock = server.AcceptSocket();
                new Pong(sock, null, true, true).Run();
                sock.Close();
                server.Stop();
            }
            else
            {
                TcpClient client = new TcpClient();
                Console.WriteLine("Connecting...");
                client.Connect(_ip, _port);
                Stream stream = client.GetStream();
                new Pong(null, stream, false, true).Run();
                client.Close();
            }
        }
        catch (Exception e) { Console.WriteLine("Error: " + e.StackTrace); }
    }
}