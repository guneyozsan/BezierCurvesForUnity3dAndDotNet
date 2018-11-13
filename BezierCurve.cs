// MIT License
// 
// Copyright (c) 2018 Guney Ozsan
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
// ---------------------------------------------------------------------
// 
// Reference for mathematical formulas: "Bézier curve" from https://en.wikipedia.org/wiki/B%C3%A9zier_curve

using System;
// Use this if .NET and comment out Unity 3D (using UnityEngine):
// using System.Numerics;
// Use this if Unity 3D and comment out .NET (using System.Numerics):
using UnityEngine;

// Notation follows the Mathematical convention.
public static class BezierCurve
{

    public static Vector2 Linear(Vector2 p0, Vector2 p1, float t)
    {
        // polynomial form (recursive)
        return p0 + t*(p1 - p0);
        // explicit form
        // return (1 - t)*p0 + t*p1;
    }


    public static Vector2 Quadratic(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        // polynomial form (recursive)
        return (1 - t)*(p0 + t*(p1 - p0)) + t*(p1 + t*(p2 - p1));
        // Alternative recursive function:
        // return (1 - t)*Linear(p0, p1, t) + t*Linear(p1, p2, t);
        // explicit form
        // return (1 - t)*(1 - t)*p0 + 2*(1 - t)*t*p1 + t*t*p2;
    }


    public static Vector2 Cubic(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        // polynomial form (recursive)
        return (1 - t)*((1 - t)*(p0 + t*(p1 - p0)) + t*(p1 + t*(p2 - p1))) + t*((1 - t)*(p1 + t*(p2 - p1)) + t*(p2 + t*(p3 - p2)));
        // Alternative recursive function:
        // return (1 - t)*Quadratic(p0, p1, p2, t) + t*Quadratic(p1, p2, p3, t);
        // explicit form
        // return (1 - t)*(1 - t)*(1 - t)*p0 + 3*(1 - t)*(1 - t)*t*p1 + 3*(1 - t)*t*t*p2 + t*t*t*p3;
    }


    // Recursive definition
    public static Vector2 Recursive(Vector2[] p, float t)
    {
        Vector2 bt = p[0];

        if (p.Length > 1)
        {
            Vector2[] p1pn = new Vector2[p.Length - 1];
            Array.Copy(p, 1, p1pn, 0, p.Length - 1);
            // The following should be like this but skipped for optimization
            // Vector2[] p0pnMinus1 = Array.Resize(ref p, p.Length - 1);
            // bt = (1 - t)*Recursive(p0pnMinus1 , t) + t*Recursive(p1pn, t);
            Array.Resize(ref p, p.Length - 1);
            bt = (1 - t)*Recursive(p, t) + t*Recursive(p1pn, t);
        }

        return bt;
    }


    // Explicit definition
    public static Vector2 General(Vector2[] p, float t)
    {
        Vector2 bt = Vector2.zero;
        int n = p.Length - 1;

        for (int i = 0; i <= n; i++)
        {
            bt += Combination(n, i)*Power(1 - t, n - i)*Power(t, i)*p[i];
        }

        return bt;
    }


    // Polynomial form (good for repetitive use).
    // Prior to using this, calculate polynomial coefficients using PolynomialCoefficients() method,
    // keep them in a Vector2 array c[] and pass the array here.
    // If you really need an efficient algorithm, you can make a dictionary of PolynomialCoefficients.
    public static Vector2 Polynomial(Vector2[] p, float t, Vector2[] c)
    {
        Vector2 bt = Vector2.zero;

        for (int j = 0; j < p.Length; j++)
        {
            bt += Power(t, j)*c[j];
        }

        return bt;
    }


    public static class FirstDerivative
    {

        public static Vector2 Linear(Vector2 p0, Vector2 p1, float t)
        {
            return p1 - p0;
        }


        public static Vector2 Quadratic(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            return 2*(1 - t)*(p1 - p0) + 2*t*(p2 - p1);
        }


        public static Vector2 Cubic(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            return 3*(1 - t)*(1 - t)*(p1 - p0) + 6*(1 - t)*t*(p2 - p1) + 3*t*t*(p3 - p2);
        }

    }


    public static class SecondDerivative
    {

        public static Vector2 Quadratic(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            return 2*(p2 - 2*p1 + p0);
        }


        public static Vector2 Cubic(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            return 6*(1 - t)*(p2 - 2*p1 + p0) + 6*t*(p3 - 2*p2 + p1);
        }

    }


    // Parametric derivative order version for flexible use, less optimal.
    public static class Derivative
    {
        public static Vector2 Linear(Vector2 p0, Vector2 p1, float t, int order)
        {
            switch (order)
            {
                case 1:
                    return p1 - p0;
                    break;

                default:
                    return Vector2.zero;
            }
        }


        public static Vector2 Quadratic(Vector2 p0, Vector2 p1, Vector2 p2, float t, int order)
        {
            switch (order)
            {
                case 2:
                    return 2*(p2 - 2*p1 + p0);
                    break;

                case 1:
                    return 2*(1 - t)*(p1 - p0) + 2*t*(p2 - p1);
                    break;

                default:
                    return Vector2.zero;
            }
        }


        public static Vector2 Cubic(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t, int order)
        {
            switch (order)
            {
                case 3:
                    return 6*(p3 - 3*p2 + 3*p1 - p0);
                    break;

                case 2:
                    return 6*(1 - t)*(p2 - 2*p1 + p0) + 6*t*(p3 - 2*p2 + p1);
                    break;

                case 1:
                    return 3*(1 - t)*(1 - t)*(p1 - p0) + 6*(1 - t)*t*(p2 - p1) + 3*t*t*(p3 - p2);
                    break;

                default:
                    return Vector2.zero;
            }
        }
    }


    // If you really need an efficient algorithm, you can make a dictionary of PolynomialCoefficients.
    public static Vector2[] PolynomialCoefficients(Vector2[] p)
    {
        Vector2[] c = new Vector2[p.Length];

        int n = p.Length - 1;

        for (int j = 0; j <= n; j++)
        {
            c[j] = Vector2.one;

            // Pi part
            int pi = 1;

            for (int m = 0; m <= j - 1; m++)
            {
                pi *= n - m;
            }

            // Sigma part
            Vector2 sigma = Vector2.zero;

            for (int i = 0; i <= j; i++)
            {
                sigma += (Power(-1, i + j)*p[i]) / (Factorial(i)*Factorial(j - i));
            }

            c[j] = pi*sigma;
        }

        return c;
    }


    public static int Combination(int n, int i)
    {
        return Factorial(n) / (Factorial(i)*Factorial(n - i));
    }


    public static int Factorial(int n)
    {
        int y = 1;

        for (int i = 1; i <= n; i++)
        {
            y *= i;
        }

        return y;
    }


    public static float Power(float b, int n)
    {
        if (n == 0)
        {
            return 1;
        }
        else
        {
            float y = b;

            for (int i = 1; i <= n - 1; i++)
            {
                y *= y;
            }

            return y;
        }
    }
}
