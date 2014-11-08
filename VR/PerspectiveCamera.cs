using Microsoft.Xna.Framework;

namespace KinectLibrary.VR
{
    /// <summary>
    /// Creates a perspective off center camera.
    /// </summary>
    public class PerspectiveCamera : GameComponent
    {
        public PerspectiveCamera(Game game) : base(game)
        {
            // Default values
            NearPlane = 0.05f;
            FarPlane = 1000f;
            Position = new Vector3(0f, 0f, 1f);
        }

        public override void Update(GameTime gameTime)
        {
            float aspectRatio = Game.GraphicsDevice.Viewport.AspectRatio;
            Vector3 cameraTarget = new Vector3(Position.X, Position.Y, 0f);

            float left = NearPlane * (-0.5f * aspectRatio - Position.X) / Position.Z;
            float right = NearPlane * (0.5f * aspectRatio - Position.X) / Position.Z;
            float bottom = NearPlane * (-0.5f - Position.Y) / Position.Z;
            float top = NearPlane * (0.5f - Position.Y) / Position.Z;

            Projection = Matrix.CreatePerspectiveOffCenter(left, right, bottom, top, NearPlane, FarPlane);
            View = Matrix.CreateLookAt(Position, cameraTarget, Vector3.Up);

            base.Update(gameTime);
        }

        /// <summary>
        /// Gets the view matrix.
        /// </summary>
        public Matrix View { get; private set; }
        /// <summary>
        /// Gets the projection matrix.
        /// </summary>
        public Matrix Projection { get; private set; }

        /// <summary>
        /// Gets or sets the camera position. The screen coordinates goes from -1 to 1, where [0,0] is middle of the screen.
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Gets or sets the near plane.
        /// </summary>
        public float NearPlane { get; set; }
        /// <summary>
        /// Gets or sets the far plane.
        /// </summary>
        public float FarPlane { get; set; }
    }
}