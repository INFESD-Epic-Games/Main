using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace SpellFall.Collision
{

    public class LinePieceCollider : Collider, IEquatable<LinePieceCollider>
    {

        public Vector2 Start;
        public Vector2 End;

        /// <summary>
        /// The length of the LinePiece, changing the length moves the end vector to adjust the length.
        /// </summary>
        public float Length 
        { 
            get { 
                return (End - Start).Length(); 
            } 
            set {
                End = Start + GetDirection() * value; 
            }
        }

        /// <summary>
        /// The A component from the standard line formula Ax + By + C = 0
        /// </summary>
        public float StandardA
        {
            get
            {
                return Start.Y - End.Y;
            }
        }

        /// <summary>
        /// The B component from the standard line formula Ax + By + C = 0
        /// </summary>
        public float StandardB
        {
            get
            {
                return End.X - Start.X;
            }
        }

        /// <summary>
        /// The C component from the standard line formula Ax + By + C = 0
        /// </summary>
        public float StandardC
        {
            get
            {
                return Start.X * End.Y - End.X * Start.Y;
            }
        }

        public LinePieceCollider(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }
        
        public LinePieceCollider(Vector2 start, Vector2 direction, float length)
        {
            Start = start;
            End = start + direction * length;
        }

        /// <summary>
        /// Should return the angle between a given direction and the up vector.
        /// </summary>
        /// <param name="direction">The Vector2 pointing out from (0,0) to calculate the angle to.</param>
        /// <returns> The angle in radians between the the up vector and the direction to the cursor.</returns>
        public static float GetAngle(Vector2 direction)
        {
            return (float)Math.Atan2(direction.X, -direction.Y);
        }


        /// <summary>
        /// Calculates the normalized vector pointing from point1 to point2
        /// </summary>
        /// <returns> A Vector2 containing the direction from point1 to point2. </returns>
        public static Vector2 GetDirection(Vector2 point1, Vector2 point2)
        {
            Vector2 direction = point1 - point2;
            return -Vector2.Normalize(direction);
        }


        /// <summary>
        /// Calculates the normalized vector pointing from the start to the end of the line.
        /// </summary>
        /// <param name="point">The point to calculate the distance to.</param>
        /// <returns></returns>
        public float StandardLineForm(Vector2 point)
        {
            return StandardA * point.X + StandardB * point.Y + StandardC;
        }


        /// <summary>
        /// Gets whether or not the Line intersects another Line
        /// </summary>
        /// <param name="other">The Line to check for intersection</param>
        /// <returns>true there is any overlap between the Circle and the Line.</returns>
        public override bool Intersects(LinePieceCollider other)
        {
            // TODO Implement.
            return false;
        }


        /// <summary>
        /// Gets whether or not the line intersects a Circle.
        /// </summary>
        /// <param name="other">The Circle to check for intersection.</param>
        /// <returns>true there is any overlap between the two Circles.</returns>
        public override bool Intersects(CircleCollider other)
        {
            Vector2 nearestPoint = NearestPointOnLine(other.Center);
            Vector2 toCircle = other.Center - nearestPoint;
            return toCircle.Length() <= other.Radius;
        }

        /// <summary>
        /// Gets whether or not the Line intersects the Rectangle.
        /// </summary>
        /// <param name="other">The Rectangle to check for intersection.</param>
        /// <returns>true there is any overlap between the Circle and the Rectangle.</returns>
        public override bool Intersects(RectangleCollider other)
        {
            var box = other.GetBoundingBox();

            float[] results =
            {
                StandardLineForm(new Vector2(box.Left,  box.Top)),
                StandardLineForm(new Vector2(box.Right, box.Top)),
                StandardLineForm(new Vector2(box.Left,  box.Bottom)),
                StandardLineForm(new Vector2(box.Right, box.Bottom))
            };

            return !(results.All(r => r > 0) || results.All(r => r < 0));
        }

        /// <summary>
        /// Calculates the intersection point between 2 lines.
        /// </summary>
        /// <param name="Other">The line to intersect with</param>
        /// <returns>A Vector2 with the point of intersection.</returns>
        public Vector2 GetIntersection(LinePieceCollider Other)
        {
            // TODO Implement
            return Vector2.Zero;
        }

        /// <summary>
        /// Finds the nearest point on a line to a given vector, taking into account if the line is .
        /// </summary>
        /// <param name="other">The Vector you want to find the nearest point to.</param>
        /// <returns>The nearest point on the line.</returns>
        public Vector2 NearestPointOnLine(Vector2 other)
        {
            Vector2 direction = End - Start;
            float length = direction.Length();
            if (length == 0)
            {
                return Start;
            }
            direction.Normalize();
            Vector2 toOther = other - Start;
            float dot = Vector2.Dot(toOther, direction);
            float clampedDot = MathHelper.Clamp(dot, 0, length);
            Vector2 nearest = Start + clampedDot * direction;
            return nearest;
        }

        /// <summary>
        /// Returns the enclosing Axis Aligned Bounding Box containing the control points for the line.
        /// As an unbound line has infinite length, the returned bounding box assumes the line to be bound.
        /// </summary>
        /// <returns></returns>
        public override Rectangle GetBoundingBox()
        {
            Point topLeft = new Point((int)Math.Min(Start.X, End.X), (int)Math.Min(Start.Y, End.Y));
            Point size = new Point((int)Math.Max(Start.X, End.X), (int)Math.Max(Start.Y, End.Y)) - topLeft;
            return new Rectangle(topLeft,size);
        }


        /// <summary>
        /// Gets whether or not the provided coordinates lie on the line.
        /// </summary>
        /// <param name="coordinates">The coordinates to check.</param>
        /// <returns>true if the coordinates are within the circle.</returns>
        public override bool Contains(Vector2 coordinates)
        {
            // TODO: Implement

            return false;
        }

        public bool Equals(LinePieceCollider other)
        {
            return other.Start == this.Start && other.End == this.End;
        }

        /// <summary>
        /// Calculates the normalized vector pointing from point1 to point2
        /// </summary>
        /// <returns> A Vector2 containing the direction from point1 to point2. </returns>
        public static Vector2 GetDirection(Point point1, Point point2)
        {
            return GetDirection(point1.ToVector2(), point2.ToVector2());
        }


        /// <summary>
        /// Calculates the normalized vector pointing from point1 to point2
        /// </summary>
        /// <returns> A Vector2 containing the direction from point1 to point2. </returns>
        public Vector2 GetDirection()
        {
            return GetDirection(Start, End);
        }


        /// <summary>
        /// Should return the angle between a given direction and the up vector.
        /// </summary>
        /// <param name="direction">The Vector2 pointing out from (0,0) to calculate the angle to.</param>
        /// <returns> The angle in radians between the the up vector and the direction to the cursor.</returns>
        public float GetAngle()
        {
            return GetAngle(GetDirection());
        }
    }
}