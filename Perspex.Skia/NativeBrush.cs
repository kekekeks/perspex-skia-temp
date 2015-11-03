using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Perspex.Media;

namespace Perspex.Skia
{
    internal enum NativeBrushType
    {
        Solid,
        LinearGradient,
        RadialGradient,
        Image
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NativeBrush
    {
        public const int MaxGradientStops = 1024;
        public NativeBrushType Type;
        public double Opacity;
        public uint Color;
        public bool Stroke;
        public float StrokeThickness;
        
        public int GradientStopCount;
        public GradientSpreadMethod GradientSpreadMethod;
        public SkiaPoint GradientStartPoint, GradientEndPoint;
        public float GradientRadius;
        public fixed uint GradientStopColors [MaxGradientStops];
        public fixed float GradientStops [MaxGradientStops];
        public void Reset()
        {
            Type = NativeBrushType.Solid;
            Opacity = 1f;
            Color = 0;
            Stroke = false;
            StrokeThickness = 1;
            GradientStopCount = 0;
        }
        
    }


    unsafe class NativeBrushContainer : IDisposable
    {
        private readonly NativeBrushPool _pool;
        public NativeBrush* Brush;

        public NativeBrushContainer(NativeBrushPool pool)
        {
            _pool = pool;
            Brush = (NativeBrush*) Marshal.AllocHGlobal(Marshal.SizeOf(typeof (NativeBrush))).ToPointer();
            Brush->Reset();
        }

        public void Dispose()
        {
            Brush->Reset();
            _pool?.Return(this);
        }
    }

    class NativeBrushPool
    {
        public static NativeBrushPool Instance { get; } = new NativeBrushPool();
        Stack<NativeBrushContainer> _pool = new Stack<NativeBrushContainer>();

        public void Return(NativeBrushContainer c)
        {
            _pool.Push(c);
        }

        public NativeBrushContainer Get()
        {
            if (_pool.Count == 0)
                return new NativeBrushContainer(this);
            return _pool.Pop();
        }
    }
}
