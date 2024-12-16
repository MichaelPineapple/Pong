using System.Diagnostics.Contracts;
using Microsoft.VisualBasic.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Pong;

public class Shader
{
    private int handle;
    private bool disposedValue;
    
    public Shader(string vertexPath, string fragmentPath)
    {
        string vertexShaderSource = File.ReadAllText(vertexPath);
        string fragmentShaderSource = File.ReadAllText(fragmentPath);
        
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        
        GL.ShaderSource(vertexShader, vertexShaderSource);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        
        GL.CompileShader(vertexShader);
        GL.CompileShader(fragmentShader);
        
        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int succ1);
        GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out int succ2);
        
        if (succ1 == 0)
        {
            string infoLog = GL.GetShaderInfoLog(vertexShader);
            Console.WriteLine(infoLog);
        }
        if (succ2 == 0)
        {
            string infoLog = GL.GetShaderInfoLog(fragmentShader);
            Console.WriteLine(infoLog);
        }
        
        handle = GL.CreateProgram();

        GL.AttachShader(handle, vertexShader);
        GL.AttachShader(handle, fragmentShader);
        
        GL.LinkProgram(handle);

        GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out int succ3);
        if (succ3 == 0)
        {
            string infoLog = GL.GetProgramInfoLog(handle);
            Console.WriteLine(infoLog);
        }
        
        GL.DetachShader(handle, vertexShader);
        GL.DetachShader(handle, fragmentShader);
        
        GL.DeleteShader(fragmentShader);
        GL.DeleteShader(vertexShader);
    }
    
    public int getAttribLocation(string name)
    {
        return GL.GetAttribLocation(handle, name);
    }

    public int getUniformLocation(string name)
    {
        return GL.GetUniformLocation(handle, name);
    }
    
    public void setInt(string name, int value)
    {
        int location = GL.GetUniformLocation(handle, name);
        GL.Uniform1(location, value);
    }
    
    public void setMatrix4(string name, Matrix4 value)
    {
        int location = GL.GetUniformLocation(handle, name);
        GL.UniformMatrix4(location, true, ref value);
    }
    
    public void Use()
    {
        GL.UseProgram(handle);
    }
    
    protected virtual void dispose(bool disposing)
    {
        if (!disposedValue)
        {
            GL.DeleteProgram(handle);
            disposedValue = true;
        }
    }

    ~Shader()
    {
        if (disposedValue == false)
        {
            Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
        }
    }
    
    public void dispose()
    {
        dispose(true);
        GC.SuppressFinalize(this);
    }
}