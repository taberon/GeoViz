using System;

namespace MetalHelix.Geometry
{
   /// <summary>
   /// 3 component floating point vector structure.
   /// </summary>
   public struct Vector3 : IEquatable<Vector3>
   {
      public float X;
      public float Y;
      public float Z;

      public Vector3( float x, float y, float z )
      {
         this.X = x;
         this.Y = y;
         this.Z = z;
      }

      public float Length
      {
         get { return (float)Math.Sqrt( this.X * this.X + this.Y * this.Y + this.Z * this.Z ); }
      }

      public void Normalize()
      {
         float len = this.Length;
         this.X /= len;
         this.Y /= len;
         this.Z /= len;
      }

      public bool IsEmpty()
      {
         return this.X == 0f && this.Y == 0f && this.Z == 0f;
      }

      public float Dot( Vector3 source )
      {
         return this.X * source.X + this.Y * source.Y + this.Z * source.Z;
      }

      public Vector3 Cross( Vector3 source )
      {
         Vector3 vec = new Vector3();
         vec.X = this.Y * source.Z - this.Z * source.Y;
         vec.Y = this.Z * source.X - this.X * source.Z;
         vec.Z = this.X * source.Y - this.Y * source.X;
         return vec;
      }

      public void Scale( float size )
      {
         this.X *= size;
         this.Y *= size;
         this.Z *= size;
      }

      public void Scale( float x, float y, float z )
      {
         this.X *= x;
         this.Y *= y;
         this.Z *= z;
      }

      public void Translate( float x, float y, float z )
      {
         this.X += x;
         this.Y += y;
         this.Z += z;
      }

      public void Translate( Vector3 vector )
      {
         this.X += vector.X;
         this.Y += vector.Y;
         this.Z += vector.Z;
      }

      public void RotateX( float angle )
      {
         this = RotateX( this, angle );
      }

      public void RotateY( float angle )
      {
         this = RotateY( this, angle );
      }

      public void RotateZ( float angle )
      {
         this = RotateZ( this, angle );
      }

      public void Rotate( Vector3 axis, float angle )
      {
         this = Rotate( axis, this, angle );
      }

      public override string ToString()
      {
         return string.Format( "{0}, {1}, {2}", this.X, this.Y, this.Z );
      }


      public static Vector3 RotateX( Vector3 point, float angle )
      {
         Vector3 vec = point;
         vec.Y = (float)( point.Y * Math.Cos( angle ) + point.Z * Math.Sin( angle ) );
         vec.Z = (float)( point.Z * Math.Cos( angle ) - point.Y * Math.Sin( angle ) );
         return vec;
      }

      public static Vector3 RotateY( Vector3 point, float angle )
      {
         Vector3 vec = point;
         vec.X = (float)( point.X * Math.Cos( angle ) - point.Z * Math.Sin( angle ) );
         vec.Z = (float)( point.Z * Math.Cos( angle ) + point.X * Math.Sin( angle ) );
         return vec;
      }

      public static Vector3 RotateZ( Vector3 point, float angle )
      {
         Vector3 vec = point;
         vec.X = (float)( point.X * Math.Cos( angle ) + point.Y * Math.Sin( angle ) );
         vec.Y = (float)( point.Y * Math.Cos( angle ) - point.X * Math.Sin( angle ) );
         return vec;
      }

      public static Vector3 Rotate( Vector3 axis, Vector3 point, float angle )
      {
         axis.Normalize();
         Vector3 vec = new Vector3();
         double cos = Math.Cos( angle );
         double sin = Math.Sin( angle );
         vec.X = (float)( ( point.X * ( axis.X * axis.X * ( 1 - cos ) + cos ) ) + ( point.Y * ( axis.X * axis.Y * ( 1 - cos ) + axis.Z * sin ) ) + ( point.Z * ( axis.X * axis.Z * ( 1 - cos ) - axis.Y * sin ) ) );
         vec.Y = (float)( ( point.X * ( axis.X * axis.Y * ( 1 - cos ) - axis.Z * sin ) ) + ( point.Y * ( axis.Y * axis.Y * ( 1 - cos ) + cos ) ) + ( point.Z * ( axis.Y * axis.Z * ( 1 - cos ) + axis.X * sin ) ) );
         vec.Z = (float)( ( point.X * ( axis.X * axis.Z * ( 1 - cos ) + axis.Y * sin ) ) + ( point.Y * ( axis.Y * axis.Z * ( 1 - cos ) - axis.X * sin ) ) + ( point.Z * ( axis.Z * axis.Z * ( 1 - cos ) + cos ) ) );
         return vec;
      }

      public static Vector3 Scale( Vector3 vector, float value )
      {
         vector.Scale( value );
         return vector;
      }

      public static Vector3 Normalize( Vector3 vector )
      {
         vector.Normalize();
         return vector;
      }

      public static float Distance( Vector3 v1, Vector3 v2 )
      {
         float dx = v1.X - v2.X;
         float dy = v1.Y - v2.Y;
         float dz = v1.Z - v2.Z;
         float dist = (float)Math.Sqrt( dx * dx + dy * dy + dz * dz );
         return dist;
      }

      public static float DistanceSq( Vector3 v1, Vector3 v2 )
      {
         float dx = v1.X - v2.X;
         float dy = v1.Y - v2.Y;
         float dz = v1.Z - v2.Z;
         float distSq = dx * dx + dy * dy + dz * dz;
         return distSq;
      }

      public static Vector3 operator +( Vector3 left, Vector3 right )
      {
         return new Vector3( left.X + right.X, left.Y + right.Y, left.Z + right.Z );
      }

      public static Vector3 operator -( Vector3 left, Vector3 right )
      {
         return new Vector3( left.X - right.X, left.Y - right.Y, left.Z - right.Z );
      }


      public static Vector3 operator -( Vector3 vec )
      {
         return new Vector3( -vec.X, -vec.Y, -vec.Z );
      }


      public static Vector3 operator *( Vector3 left, Vector3 right )
      {
         return new Vector3( left.X * right.X, left.Y * right.Y, left.Z * right.Z );
      }

      public static Vector3 operator *( Vector3 vector, float scale )
      {
         return new Vector3( vector.X * scale, vector.Y * scale, vector.Z * scale );
      }

      public static Vector3 operator /( Vector3 left, Vector3 right )
      {
         return new Vector3( left.X / right.X, left.Y / right.Y, left.Z / right.Z );
      }

      public static Vector3 operator /( Vector3 vector, float scale )
      {
         return new Vector3( vector.X / scale, vector.Y / scale, vector.Z / scale );
      }

      public static bool operator ==( Vector3 left, Vector3 right )
      {
         return left.Equals( right );
      }

      public static bool operator !=( Vector3 left, Vector3 right )
      {
         return !left.Equals( right );
      }

      public bool Equals( Vector3 other )
      {
         return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
      }

      public override bool Equals( object obj )
      {
         if( obj is Vector3 vector )
            return Equals( vector );

         return base.Equals( obj );
      }

      public override int GetHashCode()
      {
         return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode();
      }

      public static readonly Vector3 Zero = new Vector3();
      public static readonly Vector3 UnitX = new Vector3( 1f, 0f, 0f );
      public static readonly Vector3 UnitY = new Vector3( 0f, 1f, 0f );
      public static readonly Vector3 UnitZ = new Vector3( 0f, 0f, 1f );

   }

}