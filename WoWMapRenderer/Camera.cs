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
        protected Vector3 m_position = new Vector3(0, 100, 0);
        protected Vector3 m_up = Vector3.UnitZ;
        protected Vector3 m_direction;

        protected const float m_pitchLimit = 1.4f;

        protected const float m_speed = 3.5f;
        protected const float m_mouseSpeedX = 0.0045f;
        protected const float m_mouseSpeedY = 0.0025f;

        protected MouseState m_prevMouse;


        /// <summary>
        /// Creates the instance of the camera.
        /// </summary>
        public Camera(float Width, float Height)
        {
            // Create the direction vector and normalize it since it will be used for movement
            m_direction = Vector3.Zero - m_position;
            m_direction.Normalize();

            // Create default camera matrices
            View = CreateLookAt();
            SetViewport(Width, Height);
        }

        public void SetViewport(float Width, float Height)
        {
            Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, Width / Height, 0.01f, 1000);
        }

        /// <summary>
        /// Creates the instance of the camera at the given location.
        /// </summary>
        /// <param name="position">Position of the camera.</param>
        /// <param name="target">The target towards which the camera is pointing.</param>
        public Camera(Vector3 position, Vector3 target)
        {
            m_position = position;
            m_direction = target - m_position;
            m_direction.Normalize();

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
                // Move forward and backwards by adding m_position and m_direction vectors
                m_position += m_direction*m_speed;

            if (keyboard.IsKeyDown(Key.S))
                m_position -= m_direction*m_speed;

            if (keyboard.IsKeyDown(Key.A))
                // Strafe by adding a cross product of m_up and m_direction vectors
                m_position += Vector3.Cross(m_up, m_direction)*m_speed;

            if (keyboard.IsKeyDown(Key.D))
                m_position -= Vector3.Cross(m_up, m_direction)*m_speed;

            if (keyboard.IsKeyDown(Key.Space))
                m_position += m_up*m_speed;

            if (keyboard.IsKeyDown(Key.ControlLeft) || keyboard.IsKeyDown(Key.X))
                m_position -= m_up*m_speed;


            if (mouse.IsButtonDown(MouseButton.Left))
            {
                // Calculate yaw to look around with a mouse
                m_direction = Vector3.Transform(m_direction,
                    Matrix4.CreateFromAxisAngle(m_up, -m_mouseSpeedX*(mouse.X - m_prevMouse.X))
                    );

                // Pitch is limited to m_pitchLimit
                float angle = m_mouseSpeedY*(mouse.Y - m_prevMouse.Y);
                if ((Pitch < m_pitchLimit || angle > 0) && (Pitch > -m_pitchLimit || angle < 0))
                {
                    m_direction = Vector3.Transform(m_direction,
                        Matrix4.CreateFromAxisAngle(Vector3.Cross(m_up, m_direction), angle)
                        );
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
        }


        /// <summary>
        /// Create a view (modelview) matrix using camera vectors.
        /// </summary>
        protected Matrix4 CreateLookAt()
        {
            return Matrix4.LookAt(m_position, m_position + m_direction, m_up);
        }


        /// <summary>
        /// Position vector.
        /// </summary>
        public Vector3 Position
        {
            get { return m_position; }
        }

        /// <summary>
        /// Yaw of the camera in radians.
        /// </summary>
        public double Yaw
        {
            get { return Math.PI - Math.Atan2(m_direction.X, m_direction.Z); }
        }

        /// <summary>
        /// Pitch of the camera in radians.
        /// </summary>
        public double Pitch
        {
            get { return Math.Asin(m_direction.Y); }
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
