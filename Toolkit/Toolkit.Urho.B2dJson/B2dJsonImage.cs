using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Urho.Urho2D;

namespace Toolkit.UrhoSharp.B2dJson
{


    /// <summary>
    /// A 3-by-3 matrix. Stored in column-major order.
    /// </summary>
    internal struct _b2Mat33
    {
        public Vector3 Ex, Ey, Ez;

        public _b2Mat33(Vector3 c1, Vector3 c2, Vector3 c3)
        {
            Ex = c1;
            Ey = c2;
            Ez = c3;
        }

        /// <summary>
        /// Set this matrix to all zeros.
        /// </summary>
        public void SetZero()
        {
            Ex.X = Ex.Y = Ex.Z = 0.0f;
            Ey.X = Ey.Y = Ey.Z = 0.0f;
            Ez.X = Ez.Y = Ez.Z = 0.0f;
        }

    };


    public enum B2dJsonImagefilterType
    {
        FT_NEAREST,
        FT_LINEAR,

        FT_MAX
    };



    /// <summary>
    /// An axis aligned bounding box.
    /// </summary>
    public struct AABB
    {

        /// <summary>
        /// the lower vertex
        /// </summary>
        public Vector2 LowerBound { get; set; }
        /// <summary>
        /// the upper vertex
        /// </summary>
        public Vector2 UpperBound { get; set; }

        /// <summary>
        /// Get the center of the AABB.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetCenter()
        {
            return 0.5f * (LowerBound + UpperBound);
        }

        /// <summary>
        /// Get the extents of the AABB (half-widths).
        /// </summary>
        /// <returns></returns>
        public Vector2 GetExtents()
        {
            return 0.5f * (UpperBound - LowerBound);
        }

        /// <summary>
        /// Get the perimeter length
        /// </summary>
        /// <returns></returns>
        public float GetPerimeter()
        {
            float wx = UpperBound.X - LowerBound.X;
            float wy = UpperBound.Y - LowerBound.Y;
            return 2.0f * (wx + wy);
        }

        /// <summary>
        /// Combine an AABB into this one.
        /// </summary>
        public void Combine(AABB aabb)
        {
            LowerBound = Vector2.Min(LowerBound, aabb.LowerBound);
            UpperBound = Vector2.Max(UpperBound, aabb.UpperBound);
        }

        /// <summary>
        /// Combine two AABBs into this one.
        /// </summary>
        public void Combine(AABB aabb1, AABB aabb2)
        {

            LowerBound = Vector2.Min(aabb1.LowerBound, aabb2.LowerBound);
            UpperBound = Vector2.Max(aabb1.UpperBound, aabb2.UpperBound);
        }

        /// <summary>
        /// Does this aabb contain the provided AABB.
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <returns></returns>
        public bool Contains(AABB aabb)
        {
            bool result = true;
            result = result && LowerBound.X <= aabb.LowerBound.X;
            result = result && LowerBound.Y <= aabb.LowerBound.Y;
            result = result && aabb.UpperBound.X <= UpperBound.X;
            result = result && aabb.UpperBound.Y <= UpperBound.Y;
            return result;
        }
    };




    public class B2dJsonImage
    {

        public B2dJsonImage()
        {
            Body = null;
            Center = new Vector2();
            Angle = 0;
            Scale = 1;
            AspectScale = 1;
            Flip = false;
            Filter = B2dJsonImagefilterType.FT_LINEAR;
            Opacity = 1;
            RenderOrder = 0;
            for (int i = 0; i < 4; i++) ColorTint[i] = 255;

            NumPoints = 0;
            Points = null;
            UvCoords = null;
            NumIndices = 0;
            Indices = null;
        }


        public string Name { get; set; }
        public string File { get; set; }
        public string Path { get; set; }
        public RigidBody2D Body { get; set; }
        public Vector2 Center { get; set; }
        public float Angle { get; set; }
        public float Scale { get; set; }
        public float AspectScale { get; set; }
        public bool Flip { get; set; }
        public float Opacity { get; set; }
        /// <summary>0 = nearest, 1 = linear</summary>
        public B2dJsonImagefilterType Filter { get; set; }
        public float RenderOrder { get; set; }
        public int[] ColorTint { get; set; } = new int[4];

        public Vector2[] Corners { get; set; } = new Vector2[4];

        public int NumPoints { get; set; }
        public float[] Points { get; set; }
        public float[] UvCoords { get; set; }
        public int NumIndices { get; set; }
        public ushort[] Indices { get; set; }


        public void UpdateCorners(float aspect)
        {
            float hx = 0.5f * aspect;
            float hy = 0.5f;

            Corners[0].X = -hx; Corners[0].Y = -hy;
            Corners[1].X = hx; Corners[1].Y = -hy;
            Corners[2].X = hx; Corners[2].Y = hy;
            Corners[3].X = -hx; Corners[3].Y = hy;

            _b2Mat33 r = new _b2Mat33(), s = new _b2Mat33();
            _setMat33Rotation(r, Angle);
            _setMat33Scale(s, Scale, Scale);
            _b2Mat33 m = _b2Mul(r, s);

            for (int i = 0; i < 4; i++)
            {
                Corners[i] = _b2Mul(m, Corners[i]);
                Corners[i] += Center;
            }
        }

        public void UpdateUVs(float aspect)
        {
            //set up vertices

            float hx = 0.5f * aspect;
            float hy = 0.5f;

            Vector2[] verts = new Vector2[4];
            verts[0].X = -hx; verts[0].Y = -hy;
            verts[1].X = hx; verts[1].Y = -hy;
            verts[2].X = hx; verts[2].Y = hy;
            verts[3].X = -hx; verts[3].Y = hy;

            _b2Mat33 r = new _b2Mat33(), s = new _b2Mat33();
            _setMat33Rotation(r, Angle);
            _setMat33Scale(s, Scale, Scale);
            _b2Mat33 m = _b2Mul(r, s);

            for (int i = 0; i < 4; i++)
            {
                verts[i] = _b2Mul(m, verts[i]);
                verts[i] += Center;
            }

            //set up uvs

            Vector2[] uvs = new Vector2[4];
            verts[0].X = 0; verts[0].Y = 0;
            verts[1].X = 1; verts[1].Y = 0;
            verts[2].X = 1; verts[2].Y = 1;
            verts[3].X = 0; verts[3].Y = 1;

            //set up arrays for rendering

            NumPoints = 4;
            NumIndices = 6;

            Points = new float[2 * NumPoints];
            UvCoords = new float[2 * NumPoints];
            Indices = new ushort[NumIndices];

            for (int i = 0; i < NumPoints; i++)
            {
                Points[2 * i + 0] = verts[i].X;
                Points[2 * i + 1] = verts[i].Y;
                UvCoords[2 * i + 0] = uvs[i].X;
                UvCoords[2 * i + 1] = uvs[i].Y;
            }
            Indices[0] = 0;
            Indices[1] = 1;
            Indices[2] = 2;
            Indices[3] = 2;
            Indices[4] = 3;
            Indices[5] = 0;
        }

        public AABB GetAABB()
        {
            _b2Mat33 r = new _b2Mat33(), t = new _b2Mat33(), m = new _b2Mat33();
            if (null != Body)
            {
                _setMat33Rotation(r, Body.Node.Rotation2D);
                _setMat33Translation(t, Body.Node.Position2D);
                m = _b2Mul(r, t);
            }
            else
                m = new _b2Mat33(new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1)); //identity matrix

            AABB aabb = new AABB();
            aabb.LowerBound = new Vector2(float.MaxValue, float.MaxValue);
            aabb.UpperBound = new Vector2(-float.MaxValue, -float.MaxValue);

            for (int i = 0; i < 4; i++)
            {
                aabb.LowerBound = Vector2.Min(aabb.LowerBound, _b2Mul(m, Corners[i]));
                aabb.UpperBound = Vector2.Max(aabb.UpperBound, _b2Mul(m, Corners[i]));
            }
            return aabb;
        }

        public virtual bool LoadImage() { return false; }
        public virtual void Render() { }








        private void _setMat33Translation(_b2Mat33 mat, Vector2 t)
        {
            mat.SetZero();
            mat.Ex.X = 1;
            mat.Ey.Y = 1;
            mat.Ez.X = t.X;
            mat.Ez.Y = t.Y;
            mat.Ez.Z = 1;
        }

        private void _setMat33Rotation(_b2Mat33 mat, float angle)
        {
            mat.SetZero();
            float c = (float)Math.Cos(angle), s = (float)Math.Sin(angle);
            mat.Ex.X = c; mat.Ey.X = -s;
            mat.Ex.Y = s; mat.Ey.Y = c;
            mat.Ez.Z = 1;
        }

        private void _setMat33Scale(_b2Mat33 mat, float xfactor, float yfactor)
        {
            mat.SetZero();
            mat.Ex.X = xfactor;
            mat.Ey.Y = yfactor;
            mat.Ez.Z = 1;
        }

        private Vector2 _b2Mul(_b2Mat33 A, Vector2 v2)
        {
            Vector3 v = new Vector3(v2.X, v2.Y, 1);
            Vector3 r = v.X * A.Ex + v.Y * A.Ey + v.Z * A.Ez;
            return new Vector2(r.X, r.Y);
        }

        private _b2Mat33 _b2Mul(_b2Mat33 B, _b2Mat33 A)
        {
            Vector3 b2Mul(_b2Mat33 _a, Vector3 _v) { return _v.X * _a.Ex + _v.Y * _a.Ey + _v.Z * _a.Ez; }
            return new _b2Mat33(b2Mul(A, B.Ex), b2Mul(A, B.Ey), b2Mul(A, B.Ez));
        }


    }

}
