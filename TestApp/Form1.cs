using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Perspex;
using Perspex.Media;
using Perspex.Media.Imaging;
using Perspex.Platform;
using Perspex.Skia;

namespace TestApp
{
    public partial class Form1 : Form
    {
        private IPlatformRenderInterface _render;
        private IRenderTarget _renderTarget;
        private RenderTargetBitmap _rbitmap;
        private Bitmap _bitmap;
        private StreamGeometry _geometry;
        public Form1()
        {
            InitializeComponent();

            _render = new PlatformRenderInterface();
            PerspexLocator.CurrentMutable.Bind<IPlatformRenderInterface>().ToConstant(_render);
            _geometry =  new StreamGeometry();
            _rbitmap = new RenderTargetBitmap(50, 50);
            _renderTarget = _render.CreateRenderer(new PlatformHandle(Handle, "HWND"), ClientSize.Width,
                ClientSize.Height);
            var timer = new Timer() {Interval = 20};
            timer.Tick += delegate { Invalidate(); };
            timer.Start();
            components.Add(timer);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            using (var ctx = _geometry.Open())
            {
                ctx.BeginFigure(new Point(10,10), true);
                ctx.LineTo(new Point(40,25));
                ctx.BezierTo(new Point(50, 45), new Point(43, 48), new Point(20, 90));
                ctx.LineTo(new Point(10, 60));
                ctx.EndFigure(true);
            }

            _text =
                new FormattedText(
                    "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum",
                    "Arial", 25, FontStyle.Normal, TextAlignment.Left, FontWeight.Normal);

            _text.Constraint = new Size(400, double.PositiveInfinity);

            using (var ctx = _rbitmap.CreateDrawingContext())
                ctx.DrawRectangle(new Pen(new SolidColorBrush(Colors.Aqua)), new Rect(10, 10, 30, 30), 5);

            _bitmap = new Bitmap(@"C:\Users\keks\Desktop\phoenix.png");
        }

        static Stopwatch Stopwatch = Stopwatch.StartNew();


        RadialGradientBrush CreateRadialGradient()
        {
            return new RadialGradientBrush
            {
                Center = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                Radius = 25,
                GradientStops =
                {
                    new GradientStop(Colors.Yellow, 0),
                    new GradientStop(Colors.Red, 0.5),
                    new GradientStop(Colors.Cyan, 1)
                },
                Opacity = 1,
            };
        }

        LinearGradientBrush CreateLinearGradient()
        {
            return new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Colors.Blue, 0),
                    new GradientStop(Colors.Red, 1)
                },
                SpreadMethod = GradientSpreadMethod.Reflect
            };
        }

        int _frames = 0;
        int _fps = 0;
        TimeSpan _lastFps;
        private FormattedText _text;
        string _textString;
        Point _textPos = new Point(40, 40);

        void UpdateTitle()
        {
            Text = "Highlighted: " + _textString + " FPS: " + _fps;
        }

        protected override void WndProc(ref Message m)
        {

            if (m.Msg == 15)
            {
                var now = Stopwatch.Elapsed;
                if ((now - _lastFps).Seconds >= 1)
                {
                    _fps = _frames;
                    _frames = 0;
                    _lastFps = now;
                    UpdateTitle();
                }
                _frames++;

                _renderTarget.Resize(ClientSize.Width, ClientSize.Height);
                using (var ctx = _renderTarget.CreateDrawingContext())
                {
                    ctx.FillRectangle(Brushes.White, new Rect(0, 0, Width, Height));
                    ctx.FillRectangle(Brushes.Green, new Rect(_textPos, _text.Measure()));
                    ctx.FillRectangle(Brushes.Blue, _textRect);
                    ctx.DrawText(Brushes.Red, _textPos, _text);


                    /*for(var x=0; x<Width; x+=40)
                        for (var y = 0; y < Height; y += 20)
                            ctx.FillRectangle(Brushes.Gray, new Rect(x + (y/20)%2*20, y, 20, 20));
*/
                    /*
                    _geometry.Transform = new MatrixTransform(GetTransform());
                    var rad = CreateRadialGradient();
                    rad.Opacity = 0.5;
                    ctx.DrawGeometry(rad, new Pen(CreateLinearGradient(), 4),
                        _geometry);*/
                    /*
                    using (ctx.PushPostTransform(GetTransform()))
                    {
                        ctx.DrawImage(_rbitmap, 1, new Rect(0, 0, 50, 50), new Rect(0, 0, 50, 50));
                        using(ctx.PushPostTransform(Matrix.CreateTranslation(200, 0)))
                        ctx.DrawImage(_bitmap, 1, new Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight),
                            new Rect(0, 0, 100, 100));
                    }*/
                }
                m.Result = new IntPtr(1);
                return;
            }
            base.WndProc(ref m);
        }

        Rect _textRect;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var res = _text.HitTestPoint(new Point(e.X, e.Y) - _textPos);
            if (res.IsInside)
            {
                _textRect = _text.HitTestTextPosition(res.TextPosition);
                _textRect = new Rect(_textRect.Position + _textPos, _textRect.Size);
                _textString = _text.Text.Substring(res.TextPosition, 1);
            }
            else
                _textRect = new Rect();
            UpdateTitle();
            base.OnMouseMove(e);
        }

        Matrix GetTransform()
        {
            double seed = Stopwatch.Elapsed.TotalMilliseconds;
            var rad = seed / 300;
            var scale = Math.Sin(rad) + 2;
            return  Matrix.CreateTranslation(-30, -30) * Matrix.CreateRotation(rad) *
                Matrix.CreateScale(scale, scale) *
                Matrix.CreateTranslation(30, 30) *
                Matrix.CreateTranslation(150, 100);
        }

        private void DrawRoundRect(DrawingContext ctx)
        {
            ctx.FillRectangle(new SolidColorBrush(Colors.White),
                new Rect(new Size(ClientSize.Width, ClientSize.Height)));
            var rc = new Rect(0, 0, 60, 60);


            var transform = GetTransform();

            using (ctx.PushPostTransform(transform))
            {
                ctx.FillRectangle(new SolidColorBrush(Colors.Aqua), rc, 20);
                ctx.DrawRectangle(new Pen(new SolidColorBrush(Colors.Magenta), 10), rc, 20);
            }
        }
    }
}
