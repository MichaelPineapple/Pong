#define GLAD_GL_IMPLEMENTATION
#define GLFW_INCLUDE_NONE
#include <glad/gl.h>
#include <GLFW/glfw3.h>
#include "linmath.h"
#include <cstdlib>
#include <cstdio>
#include <thread>

const float PADDLE_SPEED = 0.001f;
const float PADDLE_HEIGHT = 0.1f;
const float PADDLE_WIDTH = 0.01f;
const float PADDLE_X = 0.5f;
const float BALL_SIZE = 0.01f;
const float BALL_SPEED = 0.0005f;

const float verticesPadd[12] =
{
    -PADDLE_WIDTH,  PADDLE_HEIGHT,
     PADDLE_WIDTH, -PADDLE_HEIGHT,
    -PADDLE_WIDTH, -PADDLE_HEIGHT,

    -PADDLE_WIDTH,  PADDLE_HEIGHT,
     PADDLE_WIDTH,  PADDLE_HEIGHT,
     PADDLE_WIDTH, -PADDLE_HEIGHT,
};

const float verticesBall[12] =
{
    -BALL_SIZE,  BALL_SIZE,
     BALL_SIZE, -BALL_SIZE,
    -BALL_SIZE, -BALL_SIZE,
    -BALL_SIZE,  BALL_SIZE,
     BALL_SIZE,  BALL_SIZE,
     BALL_SIZE, -BALL_SIZE,
};

bool leftUpp, leftDwn = false;
bool rigtUpp, rigtDwn = false;

vec2 posLeft = {-PADDLE_X, 0.0f};
vec2 posRigt = {PADDLE_X, 0.0f};
vec2 velBall = {BALL_SPEED*2.0f, BALL_SPEED};
vec2 posBall = {0.0f, 0.0f};

int scoreLeft;
int scoreRigt;

GLuint shader;
GLuint vaoPadd;
GLuint vaoBall;

GLint ulModel;
GLint ulProjj;

bool close = false;

GLuint createShader()
{
    const char* shaderSrcVert =
    "#version 330\n"
    "in vec2 vPos;"
    "uniform mat4 model;"
    "uniform mat4 projj;"
    "void main()"
    "{"
    "    gl_Position = model * projj * vec4(vPos, 0.0, 1.0);"
    "}";

    const char* shaderSrcFrag =
    "#version 330\n"
    "out vec4 fragment;"
    "void main()"
    "{"
    "    fragment = vec4(1.0, 1.0, 1.0, 1.0);"
    "}";

    const GLuint vertex_shader = glCreateShader(GL_VERTEX_SHADER);
    glShaderSource(vertex_shader, 1, &shaderSrcVert, NULL);
    glCompileShader(vertex_shader);

    const GLuint fragment_shader = glCreateShader(GL_FRAGMENT_SHADER);
    glShaderSource(fragment_shader, 1, &shaderSrcFrag, NULL);
    glCompileShader(fragment_shader);

    const GLuint program = glCreateProgram();
    glAttachShader(program, vertex_shader);
    glAttachShader(program, fragment_shader);
    glLinkProgram(program);

    return program;
}

GLuint createVAO(GLuint _shader, const float _vertices[], int _len)
{
    const GLsizeiptr valSize = sizeof(float);
    const GLint aVert = glGetAttribLocation(_shader, "vPos");

    GLuint vertex_buffer;
    glGenBuffers(1, &vertex_buffer);
    glBindBuffer(GL_ARRAY_BUFFER, vertex_buffer);
    glBufferData(GL_ARRAY_BUFFER, valSize * _len, _vertices, GL_DYNAMIC_DRAW);

    GLuint vao;
    glGenVertexArrays(1, &vao);
    glBindVertexArray(vao);
    glEnableVertexAttribArray(aVert);
    glVertexAttribPointer(aVert, 2, GL_FLOAT, GL_FALSE, 2 * valSize, 0);
    return vao;
}

void onLoad()
{
    shader = createShader();

    vaoPadd = createVAO(shader, verticesPadd, 12);
    vaoBall = createVAO(shader, verticesBall, 12);

    ulModel = glGetUniformLocation(shader, "model");
    ulProjj = glGetUniformLocation(shader, "projj");
}

void handlePaddleCollision(const float _ball[], const vec2 _pos)
{
    if (_ball[0] >= (_pos[1] - PADDLE_HEIGHT) && _ball[1] <= (_pos[1] + PADDLE_HEIGHT))
    {
        if (_ball[2] >= (_pos[0] - PADDLE_WIDTH) && _ball[3] <= (_pos[0] + PADDLE_WIDTH))
        {
            velBall[0] = -velBall[0];
        }
    }
}

void handleCollision()
{
    float ball[4] =
    {
        posBall[1] + BALL_SIZE,
        posBall[1] - BALL_SIZE,
        posBall[0] + BALL_SIZE,
        posBall[0] - BALL_SIZE,
    };

    if (ball[0] >= 1.0f || ball[1] <= -1.0f) velBall[1] = -velBall[1];

    handlePaddleCollision(ball, posLeft);
    handlePaddleCollision(ball, posRigt);
}

void handleScoring()
{
    bool resetBall = false;

    if (posBall[0] < -1.0f)
    {
        scoreRigt += 1;
        resetBall = true;
    }

    if (posBall[0] > 1.0f)
    {
        scoreLeft += 1;
        resetBall = true;
    }

    if (resetBall)
    {
        posBall[0] = 0.0f;
        posBall[1] = 0.0f;
    }
}

void onUpdate()
{
    float speed = PADDLE_SPEED;
    if (leftUpp && posLeft[1] <  1.0f) posLeft[1] += speed;
    if (leftDwn && posLeft[1] > -1.0f) posLeft[1] -= speed;
    if (rigtUpp && posRigt[1] <  1.0f) posRigt[1] += speed;
    if (rigtDwn && posRigt[1] > -1.0f) posRigt[1] -= speed;

    handleCollision();
    handleScoring();

    posBall[0] += velBall[0];
    posBall[1] += velBall[1];
}

void renderVAO(GLuint _vao, vec2 _pos)
{
    mat4x4 model;
    mat4x4_translate(model, _pos[0], _pos[1], 0.0f);
    glUniformMatrix4fv(ulModel, 1, GL_FALSE, (const GLfloat*) &model);
    glBindVertexArray(_vao);
    glDrawArrays(GL_TRIANGLES, 0, 6);
}

void onRender(GLFWwindow* _window)
{
    int width, height;
    glfwGetFramebufferSize(_window, &width, &height);
    const float aspectRatio = width / (float)height;

    glViewport(0, 0, width, height);
    glClear(GL_COLOR_BUFFER_BIT);
    glUseProgram(shader);

    mat4x4 proj;
    mat4x4_ortho(proj, -aspectRatio, aspectRatio, -1.0f, 1.0f, 1.0f, -1.0f);
    glUniformMatrix4fv(ulProjj, 1, GL_FALSE, (const GLfloat*) &proj);

    renderVAO(vaoBall, posBall);
    renderVAO(vaoPadd, posLeft);
    renderVAO(vaoPadd, posRigt);

    glfwSwapBuffers(_window);
}

void error_callback(int error, const char* description)
{
    fprintf(stderr, "Error: %s\n", description);
}

void key_callback(GLFWwindow* window, int key, int scancode, int action, int mods)
{
    if (key == GLFW_KEY_ESCAPE && action == GLFW_PRESS) glfwSetWindowShouldClose(window, GLFW_TRUE);

    if (key == GLFW_KEY_W)
    {
        if (action == GLFW_PRESS) leftUpp = true;
        if (action == GLFW_RELEASE) leftUpp = false;
    }

    if (key == GLFW_KEY_S)
    {
        if (action == GLFW_PRESS) leftDwn = true;
        if (action == GLFW_RELEASE) leftDwn = false;
    }

    if (key == GLFW_KEY_UP)
    {
        if (action == GLFW_PRESS) rigtUpp = true;
        if (action == GLFW_RELEASE) rigtUpp = false;
    }

    if (key == GLFW_KEY_DOWN)
    {
        if (action == GLFW_PRESS) rigtDwn = true;
        if (action == GLFW_RELEASE) rigtDwn = false;
    }
}

void threadFunc()
{
    while (!close)
    {
        onUpdate();
        std::this_thread::sleep_for(std::chrono::milliseconds(1));
    }
}

int main()
{
    glfwSetErrorCallback(error_callback);

    if (!glfwInit()) exit(EXIT_FAILURE);

    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

    GLFWwindow* window = glfwCreateWindow(1000, 1000, "Pong", NULL, NULL);

    if (!window)
    {
        glfwTerminate();
        exit(EXIT_FAILURE);
    }

    glfwMakeContextCurrent(window);
    gladLoadGL(glfwGetProcAddress);
    glfwSwapInterval(1);
    glfwSetKeyCallback(window, key_callback);

    onLoad();

    const double updateTime = 0.001f;
    double prevUpdateTime = 0;

    std::thread testThread(threadFunc);

    while (!glfwWindowShouldClose(window))
    {
        // double now = glfwGetTime();
        // double deltaTime = now - prevUpdateTime;
        // if (deltaTime >= updateTime)
        // {
        //
        //     prevUpdateTime = now;
        //     //printf("\n%f", deltaTime);
        // }

        //double t0 = glfwGetTime();
        onRender(window);
        glfwPollEvents();
        //double t1 = glfwGetTime();
        //double dt = t1 - t0;
        //printf("\n%f", dt);

        //onUpdate((float)dt);
    }

    close = true;

    testThread.join();

    glfwDestroyWindow(window);

    glfwTerminate();
    exit(EXIT_SUCCESS);
}