using System.IO;
using UnityEngine;

namespace BMRP.Runtime.Core
{
    public class Util
    {
        public static string PrettifyName (string text)
        {
            text = text.Replace(c => c.IsCapital(), c => $" {c}").Replace(' ', '_', '.', '-').Trim();
            text = text[0].ToString().ToUpper() + text.Substring(1, text.Length - 1);
            return text;
        }

        public static bool IsChildDirectoryOf (string child, string parent)
        {
            child = Path.GetFullPath(child);
            parent = Path.GetFullPath(parent);

            if (parent.Length > child.Length) return false;

            for (var i = 0; i < parent.Length; i++)
            {
                if (child[i] != parent[i]) return false;
            }

            return true;
        }

        public static string SimplifyName(string text) => SimplifyName(ref text);
        public static string SimplifyName(ref string text) => string.IsNullOrEmpty(text) ? string.Empty : text.Trim().Replace(" ", "").ToLower();
        
        public static bool CompareNames(string a, string b)
        {
            a = SimplifyName(a);
            b = SimplifyName(b);
            
            if (string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(b)) return true;
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return false;
            
            return a == b;
        }

        public delegate float InterpolationMethod(float a, float b, float t);

        public static Vector2 Interp(Vector2 a, Vector2 b, float t) => Interp(a, b, t, Mathf.Lerp);
        public static Vector2 Interp(Vector2 a, Vector2 b, float t, InterpolationMethod lerp)
        {
            return new Vector2
            {
                x = lerp(a.x, b.x, t),
                y = lerp(a.y, b.y, t),
            };
        }

        public static Vector3 Interp(Vector3 a, Vector3 b, float t) => Interp(a, b, t, Mathf.Lerp);
        public static Vector3 Interp(Vector3 a, Vector3 b, float t, InterpolationMethod lerp)
        {
            return new Vector3
            {
                x = lerp(a.x, b.x, t),
                y = lerp(a.y, b.y, t),
                z = lerp(a.z, b.z, t),
            };
        }

        public static Color WhiteBalance (Color col, float temperature, float tint)
        {
            var t1 = temperature * 10 / 6;
            var t2 = tint * 10 / 6;

            // Get the CIE xy chromaticity of the reference white point.
            // Note: 0.31271 = x value on the D65 white point
            var x = 0.31271f - t1 * (t1 < 0 ? 0.1f : 0.05f);
            var standardIlluminantY = 2.87f * x - 3 * x * x - 0.27509507f;
            var y = standardIlluminantY + t2 * 0.05f;

            // Calculate the coefficients in the LMS space.
            var w1 = new Vector3(0.949237f, 1.03542f, 1.08728f); // D65 white point

            // CIExyToLMS
            const float Y = 1;
            var X = Y * x / y;
            var Z = Y * (1 - x - y) / y;
            var L = 0.7328f * X + 0.4296f * Y - 0.1624f * Z;
            var M = -0.7036f * X + 1.6975f * Y + 0.0061f * Z;
            var S = 0.0030f * X + 0.0136f * Y + 0.9834f * Z;
            var w2 = new Vector3(L, M, S);

            var balance = new Vector3(w1.x / w2.x, w1.y / w2.y, w1.z / w2.z);

            var LIN_2_LMS_MAT = new Matrix4x4(
                new Vector4(3.90405e-1f, 5.49941e-1f, 8.92632e-3f),
                new Vector4(7.08416e-2f, 9.63172e-1f, 1.35775e-3f),
                new Vector4(2.31082e-2f, 1.28021e-1f, 9.36245e-1f),
                Vector4.zero
            );

            var LMS_2_LIN_MAT = new Matrix4x4(
                new Vector4(2.85847e+0f, -1.62879e+0f, -2.48910e-2f),
                new Vector4(-2.10182e-1f,  1.15820e+0f,  3.24281e-4f),
                new Vector4(-4.18120e-2f, -1.18169e-1f,  1.06867e+0f),
                Vector4.zero
            );

            var lms = (Vector3)(LIN_2_LMS_MAT * col);
            lms = Vector3.Scale(lms, balance);
            var output = (Color)(LMS_2_LIN_MAT * lms);
            output.a = 1.0f;
            return output;
        }
    }
}
