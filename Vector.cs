using System;

namespace KinectLibrary
{
    public class Vector
    {
        public static readonly Vector Zero = new Vector(0d,0d,0d);
        public static readonly Vector Thousand = new Vector(1000, 1000, 1000);


        public Vector()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public double Length { get { return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2)); } }

        public override bool Equals(object obj)
        {
            Vector compare = obj as Vector;
            return compare != null && X == compare.X && Y == compare.Y && Z == compare.Z;
        }

        public override int GetHashCode()
        {
            return (X + Y + Z).GetHashCode();
        }

        /// <summary>
        /// Get the dot product between two vectors.
        /// </summary>
        public static double Dot(Vector vectorA, Vector vectorB)
        {
            return vectorA.X * vectorB.X + vectorA.Y * vectorB.Y + vectorA.Z * vectorB.Z;
        }

        /// <summary>
        /// Get the angle, in radians, between two vectors.
        /// </summary>
        public static double Theta(Vector vectorA, Vector vectorB)
        {
            return Math.Acos(Dot(vectorA, vectorB)/(vectorA.Length*vectorB.Length));
        }

        public static Vector Bisect(Vector vectorA, Vector vectorB)
        {
            return (vectorA.ToUnit() + vectorB.ToUnit())/2;
        }

        /// <summary>
        /// Converts the vector to a unit vector.
        /// </summary>
        public Vector ToUnit()
        {
            return new Vector(X/Length, Y/Length, 0d);
        }

        public static Vector Add(Vector vectorA, Vector vectorB)
        {
            return new Vector(vectorA.X + vectorB.X, vectorA.Y + vectorB.Y, vectorA.Z + vectorB.Z);
        }

        public Vector Add(Vector vector)
        {
            return new Vector(X + vector.X, Y + vector.Y, Z + vector.Z);
        }

        public static Vector Subtract(Vector vectorA, Vector vectorB)
        {
            return new Vector(vectorA.X - vectorB.X, vectorA.Y - vectorB.Y, vectorA.Z - vectorB.Z);
        }

        public static Vector operator /(Vector vectorA, int i)
        {
            return new Vector(vectorA.X / i, vectorA.Y / i, vectorA.Z / i);
        }

        public static Vector operator *(Vector vector, int i)
        {
            return new Vector(vector.X * i, vector.Y * i, vector.Z * i);
        }

        public static Vector operator *(double i, Vector vector)
        {
            return new Vector(vector.X * i, vector.Y * i, vector.Z * i);
        }

        public static Vector operator +(Vector vectorA,Vector vectorB)
        {
            return Add(vectorA, vectorB);
        }

        public static Vector operator -(Vector vectorA, Vector vectorB)
        {
            return Subtract(vectorA, vectorB);
        }

        public static bool operator ==(Vector vectorA, Vector vectorB)
        {
            return Equals(vectorA, vectorB);
        }

        public static bool operator !=(Vector vectorA, Vector vectorB)
        {
            return !Equals(vectorA, vectorB);
        }
    }
}