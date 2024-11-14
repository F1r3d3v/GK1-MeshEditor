﻿using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;

namespace GK1_MeshEditor
{
    internal class Texture
    {
        private DirectBitmap _texture;

        public Texture(string path)
        {
            Bitmap m = new Bitmap(path);
            _texture = new DirectBitmap(m.Width, m.Height);
            Graphics g = Graphics.FromImage(_texture.Bitmap);
            g.DrawImage(m, 0, 0, m.Width, m.Height);
        }

        public Color Sample(float u, float v)
        {
            u = Math.Clamp(u, 0f, 1f);
            v = Math.Clamp(1.0f - v, 0f, 1f);
            int x = (int)Math.Round(u * (_texture.Width - 1));
            int y = (int)Math.Round(v * (_texture.Height - 1));
            return _texture.GetPixel(x, y);
        }

        public UtilAVX2.AVX2Color SampleSimd(Vector256<float> u, Vector256<float> v)
        {
            u = Vector256.Min(Vector256.Max(u, Vector256.Create(0f)), Vector256.Create(1f));
            v = Vector256.Min(Vector256.Max(Vector256.Create(1f) - v, Vector256.Create(0f)), Vector256.Create(1f));

            Vector256<int> x = Avx.ConvertToVector256Int32WithTruncation(u * (_texture.Width - 1));
            Vector256<int> y = Avx.ConvertToVector256Int32WithTruncation(v * (_texture.Height - 1));

            Vector256<int> r = Vector256<int>.Zero;
            Vector256<int> g = Vector256<int>.Zero;
            Vector256<int> b = Vector256<int>.Zero;

            for (int i = 0; i < Vector256<int>.Count; i++)
            {
                int xi = x.GetElement(i);
                int yi = y.GetElement(i);
                Color color = _texture.GetPixel(xi, yi);
                r = r.WithElement(i, color.R);
                g = g.WithElement(i, color.G);
                b = b.WithElement(i, color.B);
            }

            return new UtilAVX2.AVX2Color(r, g, b);
        }
    }

}
