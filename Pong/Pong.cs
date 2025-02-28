using MclTech_1;
using MclTech_1.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Pong;

public class Pong
{
    MclWindow mcl;

    const float PADD_SPEED = 0.02f;
    const float PADD_HEIGHT = 0.1f;
    const float PADD_WIDTH = 0.01f;
    const float PADD_X = 0.75f;
    const float BALL_SIZE = 0.01f;

    Vector3 velBall = new Vector3(0.006f, 0.004f, 0.0f);

    int scoreLeft, scoreRigt;

    MclObject paddLeft, paddRigt, ball;
    
    Pong()
    {
        mcl = new MclWindow();
        mcl.onLoad = OnLoad;
        mcl.onUpdate = OnUpdate;
        mcl.Title = "Pong (MclTech_1)";
        mcl.UpdateFrequency = 120;
        mcl.Run();
        Console.WriteLine("GAME OVER");
        Console.WriteLine("Left:  "+scoreLeft);
        Console.WriteLine("Right: "+scoreRigt);
    }

    void OnLoad()
    {
        MclModel modelPadd = mcl.LoadModel(MclTech.RECTANGE_VERTICIES(PADD_WIDTH, PADD_HEIGHT));
        MclModel modelBall = mcl.LoadModel(MclTech.RECTANGE_VERTICIES(BALL_SIZE, BALL_SIZE));
        
        paddLeft = new MclObject(modelPadd);
        paddRigt = new MclObject(modelPadd);
        ball = new MclObject(modelBall);
        
        paddLeft.position.X = -PADD_X;
        paddRigt.position.X =  PADD_X;
        
        mcl.AddObject(paddLeft);
        mcl.AddObject(paddRigt);
        mcl.AddObject(ball);
    }

    void OnUpdate()
    {
        InputData inputDataLeft = new InputData();
        InputData inputDataRigt = new InputData();

        KeyboardState keyboard = mcl.KeyboardState;
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

        if (inputDataLeft.upp && paddLeft.position.Y <  1.0f) paddLeft.position.Y += PADD_SPEED;
        if (inputDataLeft.dwn && paddLeft.position.Y > -1.0f) paddLeft.position.Y -= PADD_SPEED;
        if (inputDataRigt.upp && paddRigt.position.Y <  1.0f) paddRigt.position.Y += PADD_SPEED;
        if (inputDataRigt.dwn && paddRigt.position.Y > -1.0f) paddRigt.position.Y -= PADD_SPEED;
        
        if (inputDataLeft.end || inputDataRigt.end) mcl.Close();
        
        HandleCollision();
        HandleScoring();
        
        ball.position += velBall;
    }
    
    void HandleCollision()
    {
        float[] ballBounds = new[]
        {
            ball.position.Y + BALL_SIZE,
            ball.position.Y - BALL_SIZE,
            ball.position.X + BALL_SIZE,
            ball.position.X - BALL_SIZE,
        };

        if (ballBounds[0] >= 1.0f || ballBounds[1] <= -1.0f)
        {
            velBall.Y = -velBall.Y;
            //AL.SourcePlay(idSoundBall);
        }
        
        HandlePaddleCollision(ballBounds, paddLeft.position);
        HandlePaddleCollision(ballBounds, paddRigt.position);
    }

    void HandlePaddleCollision(float[] _ball, Vector3 _pos)
    {
        if (_ball[0] >= (_pos.Y - PADD_HEIGHT) && _ball[1] <= (_pos.Y + PADD_HEIGHT))
        {
            if (_ball[2] >= (_pos.X - PADD_WIDTH) && _ball[3] <= (_pos.X + PADD_WIDTH))
            {
                velBall.X = -velBall.X;
                //AL.SourcePlay(idSoundBall);
            }
        }
    }

    void HandleScoring()
    {
        if (ball.position.X < -1.0f)
        {
            scoreRigt += 1;
            ball.position = Vector3.Zero;
        }
        
        if (ball.position.X > 1.0f)
        {
            scoreLeft += 1;
            ball.position = Vector3.Zero;
        }
    }
    
    struct InputData
    {
        public bool upp, dwn, end;

        public InputData()
        {
            upp = dwn = end = false;
        }
    }
    
    static void Main()
    {
        _ = new Pong();
    }
}