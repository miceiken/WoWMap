using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;

namespace WoWMapRenderer
{
    public class Camera
    {
        protected Vector3 _position = new Vector3(0, 100, 0);
        protected Vector3 _up = Vector3.UnitZ;
        protected Vector3 _direction;

        private float _viewportWidth;
        private float _viewportHeight;
        private float _farClip = 1000.0f;
        private float _nearClip = 0.01f;

        private const float _pitchLimit = 1.4f;

        private const float _speed = 3.5f;
        private const float _mouseSpeedX = 0.0045f;
        private const float _mouseSpeedY = 0.0025f;

        private const float _speedBoost = 5.0f;

        protected MouseState m_prevMouse;

        public delegate void MovementHandler();

        public event MovementHandler OnMovement;

        /// <summary>
        /// Creates the instance of the camera.
        /// </summary>
        public Camera(float viewportWidth, float Height)
        {
            _viewportWidth = viewportWidth;
            _viewportHeight = Height;
            // Create the direction vector and normalize it since it will be used for movement
            _direction = Vector3.Zero - _position;
            _direction.Normalize();

            // Create default camera matrices
            View = CreateLookAt();
            SetViewport(_viewportWidth, _viewportHeight);
        }

        public void SetViewport(float width, float height)
        {
            _viewportHeight = height;
            _viewportWidth = width;
            Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, width / height, _nearClip, _farClip);
        }

        public void SetFarClip(float farclip)
        {
            _farClip = farclip;
            Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, _viewportWidth / _viewportHeight, _nearClip, _farClip);
        }

        /// <summary>
        /// Creates the instance of the camera at the given location.
        /// </summary>
        /// <param name="position">Position of the camera.</param>
        /// <param name="target">The target towards which the camera is pointing.</param>
        public Camera(Vector3 position, Vector3 target)
        {
            _position = position;
            _direction = target - _position;
            _direction.Normalize();

            View = CreateLookAt();
            SetViewport(800, 600);
        }


        /// <summary>
        /// Handle the camera movement using user input.
        /// </summary>
        protected virtual void ProcessInput()
        {
            var mouse = Mouse.GetState();
            var keyboard = Keyboard.GetState();

            // Move camera with WASD keys
            if (keyboard.IsKeyDown(Key.W))
                _position += _direction * _speed * (keyboard.IsKeyDown(Key.LControl) ? _speedBoost : 1.0f);

            if (keyboard.IsKeyDown(Key.S))
                _position -= _direction * _speed * (keyboard.IsKeyDown(Key.LControl) ? _speedBoost : 1.0f); ;

            if (keyboard.IsKeyDown(Key.A))
                _position += Vector3.Cross(_up, _direction) * _speed * (keyboard.IsKeyDown(Key.LControl) ? _speedBoost : 1.0f); ;

            if (keyboard.IsKeyDown(Key.D))
                _position -= Vector3.Cross(_up, _direction) * _speed * (keyboard.IsKeyDown(Key.LControl) ? _speedBoost : 1.0f); ;

            if (keyboard.IsKeyDown(Key.Space))
                _position += _up * _speed * (keyboard.IsKeyDown(Key.LControl) ? _speedBoost : 1.0f); ;

            if (keyboard.IsKeyDown(Key.X))
                _position -= _up * _speed * (keyboard.IsKeyDown(Key.LControl) ? _speedBoost : 1.0f); ;


            if (mouse.IsButtonDown(MouseButton.Left))
            {
                _direction = Vector3.Transform(_direction,
                    Matrix4.CreateFromAxisAngle(_up,
                        - _mouseSpeedX * (mouse.X - m_prevMouse.X) * (keyboard.IsKeyDown(Key.LControl) ? _speedBoost : 1.0f)));

                var angle = _mouseSpeedY * (mouse.Y - m_prevMouse.Y) * (keyboard.IsKeyDown(Key.LControl) ? _speedBoost : 1.0f);
                if ((Pitch < _pitchLimit || angle > 0) && (Pitch > -_pitchLimit || angle < 0))
                {
                    _direction = Vector3.Transform(_direction, Matrix4.CreateFromAxisAngle(Vector3.Cross(_up, _direction), angle));
                }
            }

            m_prevMouse = mouse;
        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        public void Update()
        {
            // Handle camera movement
            ProcessInput();

            View = CreateLookAt();
            if (OnMovement != null)
                OnMovement();
        }


        /// <summary>
        /// Create a view (modelview) matrix using camera vectors.
        /// </summary>
        protected Matrix4 CreateLookAt()
        {
            return Matrix4.LookAt(_position, _position + _direction, _up);
        }


        /// <summary>
        /// Position vector.
        /// </summary>
        public Vector3 Position
        {
            get { return _position; }
        }

        /// <summary>
        /// Yaw of the camera in radians.
        /// </summary>
        public double Yaw
        {
            get { return Math.PI - Math.Atan2(_direction.X, _direction.Z); }
        }

        /// <summary>
        /// Pitch of the camera in radians.
        /// </summary>
        public double Pitch
        {
            get { return Math.Asin(_direction.Y); }
        }

        /// <summary>
        /// View (modelview) matrix accessor.
        /// </summary>
        public Matrix4 View { get; protected set; }

        /// <summary>
        /// Projection matrix accessor.
        /// </summary>
        public Matrix4 Projection { get; protected set; }

    }
}
