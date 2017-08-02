using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Harmonograph
{
    public class GraphWindow : GameWindow
    {
        private const double Tau = Math.PI * 2;
        private const double Pi = Math.PI;

        public HarmonographSolver Solver { get; } = new HarmonographSolver()
        {
            new PendulumSolver(Vector2d.UnitX * 1.5, 4.01, 0, 0.005),
            new PendulumSolver(Vector2d.UnitY, 5.01, 0, 0.005),
            new PendulumSolver(Vector2d.UnitX, 1.01, 0, 0.005),
            new PendulumSolver(Vector2d.UnitY / 2, -1.01, 1, 0.005),
        };

        public double ViewHeight { get; set; }
        public double DisplayRate { get; set; }
        public int Cycles { get; set; }

        public Color Back { get; }
        public Color Front { get; }

        private Vector2[] _path = new Vector2[0];
        private double _time = 0;

        private double _phaseRate = Math.PI / 4;
        private bool _phaseMode = false;

        public GraphWindow(string settingsFile)
            : base(2560, 1440, new GraphicsMode(32, 0, 0, 4))
        {
            if (settingsFile != null && File.Exists(settingsFile))
            {
                var jo = JObject.Parse(File.ReadAllText(settingsFile));
                Back = jo["back"]?.ToObject<Color>() ?? Color.Black;
                Front = jo["front"]?.ToObject<Color>() ?? Color.FromArgb(0x33, 0xff, 0xff, 0xff);

                ViewHeight = jo["viewHeight"]?.ToObject<int>() ?? 4;
                DisplayRate = jo["displayRate"]?.ToObject<double>() ?? 1;

                Cycles = jo["cycles"]?.ToObject<int>() ?? 100;

                if (jo["pendulums"] != null)
                {
                    Solver.Pendulums.Clear();
                    foreach (var t in jo["pendulums"])
                    {
                        var amp = t["amplitude"]?.ToObject<Vector2d>() ?? Vector2d.UnitX;
                        var period = t["period"]?.ToObject<double>() ?? 1;
                        var phase = t["phase"]?.ToObject<double>() ?? 0;
                        var friction = t["friction"]?.ToObject<double>() ?? 0.005;

                        Solver.Add(new PendulumSolver(amp, period, phase, friction));
                    }
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Width = 2560;
            Height = 1440;
            X = (DisplayDevice.Default.Width - Width) / 2;
            Y = (DisplayDevice.Default.Height - Height) / 2;

            RecalculatePath();
        }

        public void RecalculatePath()
        {
            var m = (int) (Cycles * 200 * Tau);
            var path = new Vector2[m];

            Parallel.For(0, m, i => path[i] = (Vector2) Solver.At(i / 20.0));

            _path = path;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            var proj = Matrix4d.CreateOrthographic(ViewHeight * Width / Height, ViewHeight, -1, 1);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref proj);

            GL.Viewport(ClientRectangle);
            GL.ClearColor(Back);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.LineWidth(1.5f);
            GL.Begin(PrimitiveType.LineStrip);
            GL.Color4(Front);
            for (var i = 0; i < _path.Length && i < _time; i++)
            {
                var p = _path[i];
                GL.Vertex2(p);
            }
            GL.End();

            SwapBuffers();
        }

        public void SaveScreenshot() => SaveScreenshot(DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png");

        public void SaveScreenshot(string file)
        {
            if (GraphicsContext.CurrentContext == null)
                throw new GraphicsContextMissingException();
            var w = ClientSize.Width;
            var h = ClientSize.Height;
            var bmp = new Bitmap(w, h);
            var data = bmp.LockBits(ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.ReadPixels(0, 0, w, h, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

            bmp.Save(file);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Space)
                SaveScreenshot();

            if (e.Key == Key.A)
                _phaseMode = !_phaseMode;

            if (e.Key == Key.R)
                _time = 0;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            Title = $"{1 / e.Time:00.00}";

            if (DisplayRate <= 0)
                _time = _path.Length;

            if (Focused)
                _time += e.Time * DisplayRate * 100;

            if (_phaseMode)
            {
                foreach (var p in Solver.Pendulums)
                    p.Phase += e.Time * _phaseRate;
                RecalculatePath();
            }
        }
    }
}