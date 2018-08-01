﻿using System;
using System.Numerics;

namespace FxMania.Graphics
{
    public enum CameraKind
    {
        PerspectiveFoV,
        Orthographic,
    }

    public class BasicCamera
    {
        private Cached<Transform> view, projection;

        private Vector3 position;
        private Quaternion rotation;

        private float fieldOfView, aspectRatio;
        private float viewWidth, viewHeight;
        private float near, far;

        public CameraKind Kind { get; private set; }
        
        public Transform ViewMatrix => view.EnsureValid() ? view.Value : view.Refresh(ComputeViewMatrix);
        public Transform ProjectionMatrix => projection.EnsureValid() ? projection.Value : projection.Refresh(ComputeProjectionMatrix);

        public Vector3 Position
        {
            get => position;
            set
            {
                if (value == position)
                    return;
                position = value;
                view.Invalidate();
            }
        }

        public Quaternion Rotation
        {
            get => rotation;
            set
            {
                if (value == rotation)
                    return;
                rotation = value;
                view.Invalidate();
            }
        }

        public float FieldOfView
        {
            get => fieldOfView;
            set
            {
                if (value == fieldOfView)
                    return;
                fieldOfView = value;
                projection.Invalidate();
            }
        }

        public float AspectRatio
        {
            get => aspectRatio;
            set
            {
                if (value == aspectRatio)
                    return;
                aspectRatio = value;
                projection.Invalidate();
            }
        }

        public float ViewportWidth
        {
            get => viewWidth;
            set
            {
                if (value == viewWidth)
                    return;
                viewWidth = value;
                projection.Invalidate();
            }
        }

        public float ViewportHeight
        {
            get => viewHeight;
            set
            {
                if (value == viewHeight)
                    return;
                viewHeight = value;
                projection.Invalidate();
            }
        }

        public float NearDistance
        {
            get => near;
            set
            {
                if (value == near)
                    return;
                near = value;
                projection.Invalidate();
            }
        }

        public float FarDistance
        {
            get => far;
            set
            {
                if (value == far)
                    return;
                far = value;
                projection.Invalidate();
            }
        }

        public void SetPerspectiveFoV(float fov, float aspect, float near, float far)
        {
            projection.Invalidate();
            Kind = CameraKind.PerspectiveFoV;

            fieldOfView = fov;
            aspectRatio = aspect;
            this.near = near;
            this.far = far;
        }

        public void SetOrthographic(float width, float height, float near, float far)
        {
            projection.Invalidate();
            Kind = CameraKind.Orthographic;

            viewWidth = width;
            viewHeight = height;
            this.near = near;
            this.far = far;
        }

        public void LookAt(Vector3 point, Vector3? up = null)
        {
            Rotation = Quaternion.CreateFromRotationMatrix(Matrix4x4.CreateLookAt(Position, point, up ?? Vector3.UnitY));
        }

        private Transform ComputeViewMatrix()
        {
            var rotConj = Quaternion.Conjugate(Rotation);
            return (Transform)(Matrix4x4.CreateFromQuaternion(rotConj) *
                               Matrix4x4.CreateTranslation(-Position));
        }

        private Transform ComputeProjectionMatrix()
        {
            switch (Kind)
            {
                case CameraKind.PerspectiveFoV:
                    return (Transform)Matrix4x4.CreatePerspectiveFieldOfView(Mathf.ToRadians(fieldOfView), aspectRatio, near, far);

                case CameraKind.Orthographic:
                    return (Transform)Matrix4x4.CreateOrthographic(viewWidth, viewHeight, near, far);

                default: throw new NotImplementedException();
            }
        }
    }
}
