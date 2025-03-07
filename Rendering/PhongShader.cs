﻿using System.Numerics;

namespace GK1_MeshEditor
{
    internal class PhongShader : IShader
    {
        private Vector3 _viewDirection { get; set; } = new Vector3(0, 0, 1);
        private Scene _scene;

        public PhongShader(Scene scene)
        {
            _scene = scene;
        }

        public Color CalculateColor(Vertex vertex)
        {
            var state = EditorViewModel.GetInstance().GetState();
            if (vertex.P.CloseTo(state.LightPosition, 1e-5)) return state.SurfaceColor;

            Color color = state.SurfaceColor;
            Vector3 normal = vertex.N;

            if (state.Texture != null)
                color = state.Texture.Sample(vertex.UV.X, vertex.UV.Y);

            if (state.NormalMap != null)
            {
                Vector3 offset = state.NormalMap.Sample(vertex.UV.X, vertex.UV.Y);
                Matrix4x4 matrix = Matrix4x4.Identity;
                Util.AssignVectorToMatrix(ref matrix, vertex.Pu, 0);
                Util.AssignVectorToMatrix(ref matrix, vertex.Pv, 1);
                Util.AssignVectorToMatrix(ref matrix, vertex.N, 2);

                normal = Vector3.Normalize(Vector3.TransformNormal(offset, Matrix4x4.Transpose(matrix)));
            }

            Vector3 lightIntensity = Vector3.Zero;
            Vector3 surfaceColor = new Vector3(color.R, color.G, color.B) / 255.0f;

            if (EditorViewModel.GetInstance().MainLightEnabled)
            {
                Vector3 mainLightColor = new Vector3(state.LightColor.R, state.LightColor.G, state.LightColor.B) / 255.0f;

                Vector3 lightDirection = Vector3.Normalize(state.LightPosition - vertex.P);
                Vector3 reflection = 2 * Vector3.Dot(normal, lightDirection) * normal - lightDirection;
                reflection = Vector3.Normalize(reflection);

                float cos1 = Math.Max(Vector3.Dot(normal, lightDirection), 0);
                float cos2 = Math.Max(Vector3.Dot(_viewDirection, reflection), 0);

                lightIntensity += (mainLightColor * surfaceColor) * (state.CoefKd * cos1 + state.CoefKs * MathF.Pow(cos2, state.CoefM));
            }

            if (EditorViewModel.GetInstance().ReflectorsEnabled)
            {
                foreach (Reflector r in _scene.LightSources)
                {
                    Vector3 rColor = new Vector3(r.Color.R, r.Color.G, r.Color.B) / 255.0f;
                    Vector3 L = Vector3.Normalize(r.Position - vertex.P);
                    Vector3 reflection = 2 * Vector3.Dot(normal, L) * normal - L;
                    reflection = Vector3.Normalize(reflection);

                    float cos1 = Math.Max(Vector3.Dot(normal, L), 0);
                    float cos2 = Math.Max(Vector3.Dot(_viewDirection, reflection), 0);

                    float rcos = Math.Max(Vector3.Dot(L, r.Direction), 0);
                    Vector3 Il = rColor * MathF.Pow(rcos, state.ReflectorsFocus);
                    lightIntensity += (Il * surfaceColor) * (state.CoefKd * cos1 + state.CoefKs * MathF.Pow(cos2, state.CoefM));
                }
            }

            Vector3 finalColor = Vector3.Min(lightIntensity, Vector3.One) * 255;
            return Color.FromArgb((int)finalColor.X, (int)finalColor.Y, (int)finalColor.Z);
        }
    }
}
