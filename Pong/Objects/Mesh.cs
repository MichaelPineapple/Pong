using System.Reflection.Metadata;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Pong;

public class Mesh
{
    private int VAO;
    private int vertexCount;
    
    private const int typeSize = sizeof(float);
    private const int vertexSize = 8 * typeSize;
    
    public Mesh(float[] _verticies, Shader _shader)
    {
        loadVerticies(_verticies, _shader);
    }

    public Mesh(string _path, Shader _shader)
    {
        List<float> verticies = new List<float>();
        using (StreamReader sr = File.OpenText(_path))
        {
            string s = String.Empty;
            while ((s = sr.ReadLine()) != null)
            {
                if (s.Trim().Length > 0)
                {
                    string[] split = s.Split(',');
                    for (int i = 0; i < split.Length; i++)
                    {
                        string v = split[i].Trim();
                        if (v.Length > 0)
                        {
                            verticies.Add(float.Parse(v));
                        }
                    }
                }
            }
        }

        float[] vertArray = verticies.ToArray();
        loadVerticies(vertArray, _shader);
    }

    private void loadVerticies(float[] _verticies, Shader _shader)
    {
        vertexCount = _verticies.Length;
        
        int aVert = _shader.getAttribLocation("aVert");
        int aTex  = _shader.getAttribLocation("aTex");
        int aNorm = _shader.getAttribLocation("aNormal");
        
        VAO = GL.GenVertexArray();
        GL.BindVertexArray(VAO);
        int VBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, _verticies.Length * sizeof(float), _verticies, BufferUsageHint.DynamicDraw);
        
        GL.VertexAttribPointer(aVert, 3, VertexAttribPointerType.Float, false, vertexSize, 0);
        GL.VertexAttribPointer(aTex,  2, VertexAttribPointerType.Float, false, vertexSize, 3 * typeSize);
        GL.VertexAttribPointer(aNorm, 3, VertexAttribPointerType.Float, false, vertexSize, 5 * typeSize);
        
        GL.EnableVertexAttribArray(aVert);
        GL.EnableVertexAttribArray(aTex);
        GL.EnableVertexAttribArray(aNorm);
    }

    public void render(Shader _shader, Vector3 _position, Vector3 _rotation, float _scale)
    {
        Matrix4 model = Matrix4.Identity;
        model *= Matrix4.CreateScale(new Vector3(_scale, _scale, _scale));
        model *= Matrix4.CreateRotationX(_rotation.X);
        model *= Matrix4.CreateRotationY(_rotation.Y);
        model *= Matrix4.CreateRotationZ(_rotation.Z);
        model *= Matrix4.CreateTranslation(_position);
        _shader.setMatrix4("model", model);
        GL.BindVertexArray(VAO);
        GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
    }
}