﻿// | -----------------------------------------------------------------
// | idx3d III is (c)1999/2000 by Peter Walser
// | -----------------------------------------------------------------
// | idx3d is a 3d engine written in 100% pure Java (1.1 compatible)
// | and provides a fast and flexible API for software 3d rendering
// | on the Java platform.
// |
// | Feel free to use the idx3d API / classes / source code for
// | non-commercial purposes (of course on your own risk).
// | If you intend to use idx3d for commercial purposes, please
// | contact me with an e-mail [proxima@active.ch].
// |
// | Thanx & greetinx go to:
// | * Wilfred L. Guerin, 	for testing, bug report, and tons 
// |			of brilliant suggestions
// | * Sandy McArthur,	for reverse loops
// | * Dr. Douglas Lyons,	for mentioning idx3d1 in his book
// | * Hugo Elias,		for maintaining his great page
// | * the comp.graphics.algorithms people, 
// | 			for scientific concerns
// | * Tobias Hill,		for inspiration and awakening my
// |			interest in java gfx coding
// | * Kai Krause,		for inspiration and hope
// | * Incarom & Parisienne,	for keeping me awake during the 
// |			long coding nights
// | * Doris Langhard,	for being the sweetest girl on earth
// | * Etnica, Infinity Project, X-Dream and "Space Night"@BR3
// | 			for great sound while coding
// | and all coderz & scenerz out there (keep up the good work, ppl :)
// |
// | Peter Walser
// | proxima@active.ch
// | http://www2.active.ch/~proxima
// | "On the eigth day, God started debugging"
// | -----------------------------------------------------------------

using System;
using System.Text;

namespace IDx3DSharp
{
    /// <summary>
    /// Defines a 3D matrix.
    /// </summary>
    public struct Matrix : ICloneable
    {
        // M A T R I X   D A T A

        public float m00, m01, m02, m03;
        public float m10, m11, m12, m13;
        public float m20, m21, m22, m23;
        public float m30, m31, m32, m33;

        // C O N S T R U C T O R S
        public Matrix(bool init) //dummy arg to deal with c# not supporting parameterless default ctors
        {
            m01 = m02 = m03 = m10 = m12 = m13 = m20 = m21 = m23 = m30 = m31 = m32 = 0;
            m00 = m11 = m22 = m33 = 1;
        }
        
        public Matrix(Vector right, Vector up, Vector forward)
        {
            m00 = right.X;
            m10 = right.Y;
            m20 = right.Z;
            m01 = up.X;
            m11 = up.Y;
            m21 = up.Z;
            m02 = forward.X;
            m12 = forward.Y;
            m22 = forward.Z;
            m03 = 0;
            m13 = 0;
            m23 = 0;
            m30 = 0;
            m31 = 0;
            m32 = 0;
            m33 = 1;
        }

        public void ImportFromArray(float[,] data)
        {
            if (data.Length < 4) return;
            if (data.GetLength(1) < 4)
                return;

            m00 = data[0, 0]; m01 = data[0, 1]; m02 = data[0, 2]; m03 = data[0, 3];
            m10 = data[1, 0]; m11 = data[1, 1]; m12 = data[1, 2]; m13 = data[1, 3];
            m20 = data[2, 0]; m21 = data[2, 1]; m22 = data[2, 2]; m23 = data[2, 3];
            m30 = data[3, 0]; m31 = data[3, 1]; m32 = data[3, 2]; m33 = data[3, 3];
        }

        public float[,] ExportToArray()
        {
            var data = new float[4, 4];
            data[0, 0] = m00; data[0, 1] = m01; data[0, 2] = m02; data[0, 3] = m03;
            data[1, 0] = m10; data[1, 1] = m11; data[1, 2] = m12; data[1, 3] = m13;
            data[2, 0] = m20; data[2, 1] = m21; data[2, 2] = m22; data[2, 3] = m23;
            data[3, 0] = m30; data[3, 1] = m31; data[3, 2] = m32; data[3, 3] = m33;
            return data;
        }



        // F A C T O R Y  M E T H O D S

        public static Matrix shiftMatrix(float dx, float dy, float dz)
        // matrix for shifting
        {
            var m = new Matrix(true);
            m.m03 = dx;
            m.m13 = dy;
            m.m23 = dz;
            return m;
        }

        public static Matrix scaleMatrix(float dx, float dy, float dz)
        // matrix for scaling
        {
            var m = new Matrix(true);
            m.m00 = dx;
            m.m11 = dy;
            m.m22 = dz;
            return m;
        }

        public static Matrix scaleMatrix(float d)
        // matrix for scaling
        {
            return scaleMatrix(d, d, d);
        }

        public static Matrix rotateMatrix(float dx, float dy, float dz)
        // matrix for rotation
        {
            var result = new Matrix(true);
            float SIN;
            float COS;

            if (dx != 0)
            {
                var m = new Matrix(true);
                SIN = MathUtility.Sin(dx);
                COS = MathUtility.Cos(dx);
                m.m11 = COS;
                m.m12 = SIN;
                m.m21 = -SIN;
                m.m22 = COS;
                result.transform(m);
            }
            if (dy != 0)
            {
                var m = new Matrix(true);
                SIN = MathUtility.Sin(dy);
                COS = MathUtility.Cos(dy);
                m.m00 = COS;
                m.m02 = SIN;
                m.m20 = -SIN;
                m.m22 = COS;
                result.transform(m);
            }
            if (dz != 0)
            {
                var m = new Matrix(true);
                SIN = MathUtility.Sin(dz);
                COS = MathUtility.Cos(dz);
                m.m00 = COS;
                m.m01 = SIN;
                m.m10 = -SIN;
                m.m11 = COS;
                result.transform(m);
            }
            return result;
        }


        // P U B L I C   M E T H O D S

        public void shift(float dx, float dy, float dz)
        {
            transform(shiftMatrix(dx, dy, dz));
        }

        public void scale(float dx, float dy, float dz)
        {
            transform(scaleMatrix(dx, dy, dz));
        }

        public void scale(float d)
        {
            transform(scaleMatrix(d));
        }

        public void rotate(float dx, float dy, float dz)
        {
            transform(rotateMatrix(dx, dy, dz));
        }

        public void scaleSelf(float dx, float dy, float dz)
        {
            preTransform(scaleMatrix(dx, dy, dz));
        }

        public void scaleSelf(float d)
        {
            preTransform(scaleMatrix(d));
        }

        public void rotateSelf(float dx, float dy, float dz)
        {
            preTransform(rotateMatrix(dx, dy, dz));
        }

        public void transform(Matrix n)
        // transforms this matrix by matrix n from left (this=n x this)
        {
            var m = Clone();

            m00 = n.m00 * m.m00 + n.m01 * m.m10 + n.m02 * m.m20;
            m01 = n.m00 * m.m01 + n.m01 * m.m11 + n.m02 * m.m21;
            m02 = n.m00 * m.m02 + n.m01 * m.m12 + n.m02 * m.m22;
            m03 = n.m00 * m.m03 + n.m01 * m.m13 + n.m02 * m.m23 + n.m03;
            m10 = n.m10 * m.m00 + n.m11 * m.m10 + n.m12 * m.m20;
            m11 = n.m10 * m.m01 + n.m11 * m.m11 + n.m12 * m.m21;
            m12 = n.m10 * m.m02 + n.m11 * m.m12 + n.m12 * m.m22;
            m13 = n.m10 * m.m03 + n.m11 * m.m13 + n.m12 * m.m23 + n.m13;
            m20 = n.m20 * m.m00 + n.m21 * m.m10 + n.m22 * m.m20;
            m21 = n.m20 * m.m01 + n.m21 * m.m11 + n.m22 * m.m21;
            m22 = n.m20 * m.m02 + n.m21 * m.m12 + n.m22 * m.m22;
            m23 = n.m20 * m.m03 + n.m21 * m.m13 + n.m22 * m.m23 + n.m23;
        }

        public void preTransform(Matrix n)
        // transforms this matrix by matrix n from right (this=this x n)
        {
            var m = Clone();

            m00 = m.m00 * n.m00 + m.m01 * n.m10 + m.m02 * n.m20;
            m01 = m.m00 * n.m01 + m.m01 * n.m11 + m.m02 * n.m21;
            m02 = m.m00 * n.m02 + m.m01 * n.m12 + m.m02 * n.m22;
            m03 = m.m00 * n.m03 + m.m01 * n.m13 + m.m02 * n.m23 + m.m03;
            m10 = m.m10 * n.m00 + m.m11 * n.m10 + m.m12 * n.m20;
            m11 = m.m10 * n.m01 + m.m11 * n.m11 + m.m12 * n.m21;
            m12 = m.m10 * n.m02 + m.m11 * n.m12 + m.m12 * n.m22;
            m13 = m.m10 * n.m03 + m.m11 * n.m13 + m.m12 * n.m23 + m.m13;
            m20 = m.m20 * n.m00 + m.m21 * n.m10 + m.m22 * n.m20;
            m21 = m.m20 * n.m01 + m.m21 * n.m11 + m.m22 * n.m21;
            m22 = m.m20 * n.m02 + m.m21 * n.m12 + m.m22 * n.m22;
            m23 = m.m20 * n.m03 + m.m21 * n.m13 + m.m22 * n.m23 + m.m23;
        }

        public static Matrix multiply(Matrix m1, Matrix m2)
        // returns m1 x m2
        {
            var m = new Matrix(true);

            m.m00 = m1.m00 * m2.m00 + m1.m01 * m2.m10 + m1.m02 * m2.m20;
            m.m01 = m1.m00 * m2.m01 + m1.m01 * m2.m11 + m1.m02 * m2.m21;
            m.m02 = m1.m00 * m2.m02 + m1.m01 * m2.m12 + m1.m02 * m2.m22;
            m.m03 = m1.m00 * m2.m03 + m1.m01 * m2.m13 + m1.m02 * m2.m23 + m1.m03;
            m.m10 = m1.m10 * m2.m00 + m1.m11 * m2.m10 + m1.m12 * m2.m20;
            m.m11 = m1.m10 * m2.m01 + m1.m11 * m2.m11 + m1.m12 * m2.m21;
            m.m12 = m1.m10 * m2.m02 + m1.m11 * m2.m12 + m1.m12 * m2.m22;
            m.m13 = m1.m10 * m2.m03 + m1.m11 * m2.m13 + m1.m12 * m2.m23 + m1.m13;
            m.m20 = m1.m20 * m2.m00 + m1.m21 * m2.m10 + m1.m22 * m2.m20;
            m.m21 = m1.m20 * m2.m01 + m1.m21 * m2.m11 + m1.m22 * m2.m21;
            m.m22 = m1.m20 * m2.m02 + m1.m21 * m2.m12 + m1.m22 * m2.m22;
            m.m23 = m1.m20 * m2.m03 + m1.m21 * m2.m13 + m1.m22 * m2.m23 + m1.m23;
            return m;
        }

        public override string ToString()
        {
            var result = new StringBuilder("<Matrix: \r\n");
            result.Append(m00 + "," + m01 + "," + m02 + "," + m03 + ",\r\n");
            result.Append(m10 + "," + m11 + "," + m12 + "," + m13 + ",\r\n");
            result.Append(m20 + "," + m21 + "," + m22 + "," + m23 + ",\r\n");
            result.Append(m30 + "," + m31 + "," + m32 + "," + m33 + ">\r\n");
            return result.ToString();
        }

        public Matrix Clone()
        {
            //var m = new Matrix();
            //m.m00 = m00; m.m01 = m01; m.m02 = m02; m.m03 = m03;
            //m.m10 = m10; m.m11 = m11; m.m12 = m12; m.m13 = m13;
            //m.m20 = m20; m.m21 = m21; m.m22 = m22; m.m23 = m23;
            //m.m30 = m30; m.m31 = m31; m.m32 = m32; m.m33 = m33;
            return this;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public Matrix inverse()
        // Returns the inverse of this matrix
        // Code generated with MapleV and handoptimized
        {
            var m = new Matrix(true);

            var q1 = m12; var q6 = m10 * m01; var q7 = m10 * m21; var q8 = m02;
            var q13 = m20 * m01; var q14 = m20 * m11; var q21 = m02 * m21; var q22 = m03 * m21;
            var q25 = m01 * m12; var q26 = m01 * m13; var q27 = m02 * m11; var q28 = m03 * m11;
            var q29 = m10 * m22; var q30 = m10 * m23; var q31 = m20 * m12; var q32 = m20 * m13;
            var q35 = m00 * m22; var q36 = m00 * m23; var q37 = m20 * m02; var q38 = m20 * m03;
            var q41 = m00 * m12; var q42 = m00 * m13; var q43 = m10 * m02; var q44 = m10 * m03;
            var q45 = m00 * m11; var q48 = m00 * m21;
            var q49 = q45 * m22 - q48 * q1 - q6 * m22 + q7 * q8;
            var q50 = q13 * q1 - q14 * q8;
            var q51 = 1 / (q49 + q50);

            m.m00 = (m11 * m22 * m33 - m11 * m23 * m32 - m21 * m12 * m33 + m21 * m13 * m32 + m31 * m12 * m23 - m31 * m13 * m22) * q51;
            m.m01 = -(m01 * m22 * m33 - m01 * m23 * m32 - q21 * m33 + q22 * m32) * q51;
            m.m02 = (q25 * m33 - q26 * m32 - q27 * m33 + q28 * m32) * q51;
            m.m03 = -(q25 * m23 - q26 * m22 - q27 * m23 + q28 * m22 + q21 * m13 - q22 * m12) * q51;
            m.m10 = -(q29 * m33 - q30 * m32 - q31 * m33 + q32 * m32) * q51;
            m.m11 = (q35 * m33 - q36 * m32 - q37 * m33 + q38 * m32) * q51;
            m.m12 = -(q41 * m33 - q42 * m32 - q43 * m33 + q44 * m32) * q51;
            m.m13 = (q41 * m23 - q42 * m22 - q43 * m23 + q44 * m22 + q37 * m13 - q38 * m12) * q51;
            m.m20 = (q7 * m33 - q30 * m31 - q14 * m33 + q32 * m31) * q51;
            m.m21 = -(q48 * m33 - q36 * m31 - q13 * m33 + q38 * m31) * q51;
            m.m22 = (q45 * m33 - q42 * m31 - q6 * m33 + q44 * m31) * q51;
            m.m23 = -(q45 * m23 - q42 * m21 - q6 * m23 + q44 * m21 + q13 * m13 - q38 * m11) * q51;

            return m;
        }

        public void reset()
        // Resets the matrix
        {
            m00 = 1;
            m01 = 0;
            m02 = 0;
            m03 = 0;
            m10 = 0;
            m11 = 1;
            m12 = 0;
            m13 = 0;
            m20 = 0;
            m21 = 0;
            m22 = 1;
            m23 = 0;
            m30 = 0;
            m31 = 0;
            m32 = 0;
            m33 = 1;
        }
    }
}