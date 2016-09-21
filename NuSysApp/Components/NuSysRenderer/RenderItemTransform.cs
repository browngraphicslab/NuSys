using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class RenderItemTransform : I2dTransformable
    {
        protected bool HasChanged = true;

        public RenderItemTransform Parent { get; private set; }

        public Matrix3x2 LocalMatrix { get; private set; } = Matrix3x2.Identity;

        private Matrix3x2 _parentLocalToScreenTransform = Matrix3x2.Identity;
        private Matrix3x2 _localToScreenTransform = Matrix3x2.Identity;
        private Matrix3x2 _screenToLocalTransform = Matrix3x2.Identity;
        public Matrix3x2 T { get; private set; } = Matrix3x2.Identity;
        public Matrix3x2 S { get; private set; } = Matrix3x2.Identity;
        public Matrix3x2 C { get; private set; } = Matrix3x2.Identity;

        public Matrix3x2 LocalToScreenMatrix
        {
            get
            {
                UpdateTransforms();
                HasChanged = false;
                return _localToScreenTransform;
            }
        }

        public Matrix3x2 ScreenToLocalMatrix
        {
            get
            {
                UpdateTransforms();
                HasChanged = false;
                return _screenToLocalTransform;
            }
        }

        public void Translate(Vector2 translation)
        {
            Translate(translation.X, translation.Y);
            HasChanged = true;
        }

        public void Translate(float x, float y)
        {
            T = Matrix3x2.CreateTranslation(T.M31 + x, T.M32 + y);
            HasChanged = true;
        }

        public Vector2 LocalScale { get { return new Vector2(S.M11, S.M22); } set { S = Matrix3x2.CreateScale(value); HasChanged = true; } }
        public Vector2 LocalPosition { get { return new Vector2(T.M31, T.M32); } set { T = Matrix3x2.CreateTranslation(value); HasChanged = true; } }
        public Vector2 LocalScaleCenter { get { return new Vector2(C.M31, C.M32); } set { C = Matrix3x2.CreateTranslation(value); HasChanged = true; } }


        public Size Size { get; set; }
        public Point Position { get; set; }
        public Vector2 CameraTranslation { get; set; }
        public Vector2 CameraCenter { get; set; }
        public float CameraScale { get; set; }

        public void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _parentLocalToScreenTransform = parentLocalToScreenTransform;
            HasChanged = true;
        }

        private void UpdateTransforms()
        {
            LocalMatrix = Win2dUtil.Invert(C) * S * C * T;
            _localToScreenTransform = LocalMatrix * _parentLocalToScreenTransform;
            _screenToLocalTransform = Win2dUtil.Invert(_localToScreenTransform);
        }

        public void SetParent(RenderItemTransform parentTransform)
        {
            Parent = parentTransform;
        }
    }

}
