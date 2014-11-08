using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KinectLibrary.VR
{
    public class GridBox : DrawableGameComponent
    {
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private VertexPositionColor[] vertices;
        private short[] indices;
        private BasicEffect basicEffect;

        public GridBox(Game game) : base(game) { }

        protected override void LoadContent()
        {
            basicEffect = new BasicEffect(GraphicsDevice);
            CreateGrid(out vertices, out indices, out vertexBuffer, out indexBuffer);
            base.LoadContent();
        }

        private void CreateGrid(out VertexPositionColor[] vertices, out short[] indices, out VertexBuffer vertexBuffer, out IndexBuffer indexBuffer)
        {
            int vertexCount;
            const int gridLines = 10;
            var vertexColor = Color.Red;
            int screenWidth = GraphicsDevice.Viewport.Width;

            vertices = CreateVertices(gridLines, screenWidth, vertexColor, out vertexCount);
            indices = CreateIndices();

            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), vertexCount, BufferUsage.None);
            vertexBuffer.SetData(this.vertices);

            indexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), indices.Length, BufferUsage.None);
            indexBuffer.SetData(indices);
        }

        private VertexPositionColor[] CreateVertices(int gridLines, int screenWidth, Color vertexColor, out int vertexCount)
        {
            var pointList = new List<VertexPositionColor>();
            double step = screenWidth / (double)gridLines;

            // Points across the X axis
            for (int i = 0; i <= gridLines * 2; i += 2)
            {
                float positionX = (float)Math.Round((i * step / 2.0d) / screenWidth, 1);

                pointList.Add(CreateVertexPosition(new Vector3(positionX, 0.0f, 0.0f), vertexColor));
                pointList.Add(CreateVertexPosition(new Vector3(positionX, 1.0f, 0.0f), vertexColor));
            }

            // Points across the Y axis
            for (int i = 0; i <= gridLines * 2; i += 2)
            {
                float positionY = (float)Math.Round((i * step / 2.0d) / screenWidth, 1);

                pointList.Add(CreateVertexPosition(new Vector3(0.0f, positionY, 0.0f), vertexColor));
                pointList.Add(CreateVertexPosition(new Vector3(1.0f, positionY, 0.0f), vertexColor));
            }

            vertexCount = pointList.Count;
            return pointList.ToArray();
        }

        private VertexPositionColor CreateVertexPosition(Vector3 postion, Color color)
        {
            return new VertexPositionColor { Position = postion, Color = color };
        }

        private short[] CreateIndices()
        {
            short[] lineListIndices = new short[45];

            lineListIndices[0] = 0; // Start point, top left corner
            for (short i = 0; i < 22; i += 4) // Lines in Y direction
            {
                lineListIndices[i + 1] = (short)(i + 1);
                lineListIndices[i + 2] = (short)(i + 3);
                lineListIndices[i + 3] = (short)(i + 2);
                lineListIndices[i + 4] = (short)(i + 4);
            }

            // Reset position to top-left corner
            lineListIndices[22] = 1;
            lineListIndices[23] = 0;

            const int offset = 1; // Array offset
            for (short i = 22; i < 39; i += 4) // Lines in X direction
            {
                lineListIndices[i + 1 + offset] = (short)(i + 1);
                lineListIndices[i + 2 + offset] = (short)(i + 3);
                lineListIndices[i + 3 + offset] = (short)(i + 2);
                lineListIndices[i + 4 + offset] = (short)(i + 4);
            }

            lineListIndices[44] = 43; // End point, bottom right corner

            return lineListIndices;
        }

        /// <summary>
        /// Draw a grid box with the camera looking through one end and into the box. The ends are open.
        /// </summary>
        /// <param name="view">The view matrix.</param>
        /// <param name="projection">The projection matrix.</param>
        /// <param name="boxDepth">The positive depth of the box.</param>
        public void DrawGridBox(Matrix view, Matrix projection, int boxDepth)
        {
            Matrix world;
            float aspectRatio = GraphicsDevice.Viewport.AspectRatio;
            
            // Left
            world = Matrix.CreateTranslation(-.5f, -.5f, 0f) * 
                    Matrix.CreateScale(1 * boxDepth / 2f, 1f, 3f) *
                    Matrix.CreateRotationY(MathHelper.PiOver2) *
                    Matrix.CreateTranslation(0.5f * aspectRatio, 0f, -.5f * boxDepth / 2f);
            DrawGrid(world, view, projection);

            // Right
            world *= Matrix.CreateTranslation(-1f * aspectRatio, 0, 0);
            DrawGrid(world, view, projection);

            // Floor
            world = Matrix.CreateTranslation(-.5f, -.5f, 0f) *
                    Matrix.CreateScale(aspectRatio, 1 * boxDepth / 2f, 1f) *
                    Matrix.CreateRotationX(MathHelper.PiOver2) *
                    Matrix.CreateTranslation(0f, 0.5f, -.5f * boxDepth / 2f);
            DrawGrid(world, view, projection);

            // Ceiling
            world *= Matrix.CreateTranslation(0, -1f, 0f);
            DrawGrid(world, view, projection);
        }

        /// <summary>
        /// Draw a 2D grid plane.
        /// </summary>
        /// <param name="world">The world matrix.</param>
        /// <param name="view">The view matrix.</param>
        /// <param name="projection">The projection matrix.</param>
        private void DrawGrid(Matrix world, Matrix view, Matrix projection)
        {
            var graphicsDevice = basicEffect.GraphicsDevice;
            basicEffect.World = world;
            basicEffect.View = view;
            basicEffect.Projection = projection;

            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;

            foreach (EffectPass effectPass in basicEffect.CurrentTechnique.Passes)
            {
                effectPass.Apply();

                GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.LineStrip,
                                                                              vertices,
                                                                              0,
                                                                              vertices.Length,
                                                                              indices,
                                                                              0,
                                                                              indices.Length - 1
                                                                             );
            }
        }

    }
}