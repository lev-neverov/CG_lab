using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace CG_lab3
{
    public partial class Form1 : Form
    {
        private GLControl glControl;
        private View view;

        public Form1()
        {
            InitializeComponent();
            InitializeGLControl();
        }

        private void InitializeGLControl()
        {
            glControl = new GLControl(new GraphicsMode(32, 24, 0, 4));
            glControl.Dock = DockStyle.Fill;
            Controls.Add(glControl);

            glControl.Load += GLControl_Load;
            glControl.Paint += GLControl_Paint;
            glControl.Resize += GLControl_Resize;
        }

        private void GLControl_Load(object sender, EventArgs e)
        {
            view = new View();
            view.Setup();
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        }

        private void GLControl_Paint(object sender, PaintEventArgs e)
        {
            if (view != null)
            {
                view.Render();
                glControl.SwapBuffers();
            }
        }

        private void GLControl_Resize(object sender, EventArgs e)
        {
            if (glControl.ClientSize.Height == 0)
                glControl.ClientSize = new System.Drawing.Size(glControl.ClientSize.Width, 1);

            GL.Viewport(0, 0, glControl.ClientSize.Width, glControl.ClientSize.Height);
            view?.OnResize(glControl.ClientSize.Width, glControl.ClientSize.Height);
            glControl.Invalidate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            view?.Dispose();
            glControl?.Dispose();
        }
    }
}