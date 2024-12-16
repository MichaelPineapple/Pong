#version 330 core

in vec3 aVert;

uniform mat4 model;

void main()
{
    gl_Position = vec4(aVert, 1.0) * model;
}