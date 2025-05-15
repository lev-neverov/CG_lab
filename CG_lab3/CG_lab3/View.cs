using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CG_lab3
{
    public class View
    {
        private int vbo_position;
        private int BasicProgramID;
        private int BasicVertexShader;
        private int BasicFragmentShader; 
        private int attribute_vpos;
        private int uniform_pos;
        private int uniform_aspect;
        private Vector3 campos = new Vector3(0, 0, -8);
        private float aspect = 1.0f;

        public void Setup()
        {
            // Инициализация шейдеров
            InitShaders();

            // Настройка буферов
            InitBuffers();
        }

        private void InitShaders()
        {
            BasicProgramID = GL.CreateProgram();

            // Загрузка шейдеров
            loadShader("raytracing.vert", ShaderType.VertexShader, BasicProgramID, out BasicVertexShader);
            loadShader("raytracing.frag", ShaderType.FragmentShader, BasicProgramID, out BasicFragmentShader);

            GL.LinkProgram(BasicProgramID);

            // Проверка успешности компоновки
            GL.GetProgram(BasicProgramID, GetProgramParameterName.LinkStatus, out int status);
            Console.WriteLine(GL.GetProgramInfoLog(BasicProgramID));

            // Получаем расположение атрибутов и uniform-переменных
            attribute_vpos = GL.GetAttribLocation(BasicProgramID, "vPosition");
            uniform_pos = GL.GetUniformLocation(BasicProgramID, "campos");
            uniform_aspect = GL.GetUniformLocation(BasicProgramID, "aspect");
        }

        private void loadShader(string filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);

            try
            {
                string shaderSource = File.ReadAllText(filename);
                GL.ShaderSource(address, shaderSource);
                GL.CompileShader(address);

                // Проверка ошибок
                string log = GL.GetShaderInfoLog(address);
                if (!string.IsNullOrEmpty(log))
                    MessageBox.Show($"Shader error ({filename}):\n{log}");

                GL.GetShader(address, ShaderParameter.CompileStatus, out int status);
                if (status == 0)
                    throw new Exception($"Shader compilation failed: {filename}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading shader {filename}:\n{ex.Message}");
                throw;
            }

            GL.AttachShader(program, address);
        }

        private void InitBuffers()
        {
            Vector3[] vertdata = new Vector3[] {
                new Vector3(-1f, -1f, 0f),
                new Vector3(1f, -1f, 0f),
                new Vector3(1f, 1f, 0f),
                new Vector3(-1f, 1f, 0f)
            };

            GL.GenBuffers(1, out vbo_position);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.BufferData(BufferTarget.ArrayBuffer, vertdata.Length * Vector3.SizeInBytes,
                          vertdata, BufferUsageHint.StaticDraw);
        }

        public void Draw()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(BasicProgramID);

            // Передаем параметры в шейдер
            GL.Uniform3(uniform_pos, campos);
            GL.Uniform1(uniform_aspect, aspect);

            // Включаем атрибут позиции
            GL.EnableVertexAttribArray(attribute_vpos);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 0, 0);

            // Рисуем квадрат
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            // Отключаем атрибут
            GL.DisableVertexAttribArray(attribute_vpos);
        }

        public void Render()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(BasicProgramID);

            // Активируем буфер и атрибуты
            GL.EnableVertexAttribArray(attribute_vpos);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 0, 0);

            // Рисуем полноэкранный квад
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            // Отключаем атрибуты
            GL.DisableVertexAttribArray(attribute_vpos);
        }

        public void OnResize(int width, int height)
        {
            GL.Viewport(0, 0, width, height);
            aspect = width / (float)height;
        }

        public void Dispose()
        {
            GL.DeleteShader(BasicVertexShader);
            GL.DeleteShader(BasicFragmentShader);
            GL.DeleteProgram(BasicProgramID);

            GL.DeleteBuffer(vbo_position);
        }
    }
}