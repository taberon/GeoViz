using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using MetalHelix.Geometry;

namespace GeometryVisualizer
{

   public struct BoundingBox : IEquatable<BoundingBox>
   {
      public Vector3 Min;
      public Vector3 Max;

      public float Left { get { return this.Min.X; } }
      public float Right { get { return this.Max.X; } }

      public float Front { get { return this.Min.Y; } }
      public float Back { get { return this.Max.Y; } }

      public float Bottom { get { return this.Min.Z; } }
      public float Top { get { return this.Max.Z; } }

      public float Width { get { return this.Max.X - this.Min.X; } }
      public float Depth { get { return this.Max.Y - this.Min.Y; } }
      public float Height { get { return this.Max.Z - this.Min.Z; } }

      public bool IsEmpty { get { return Equals( Empty ); } }
      public bool IsVoid { get { return Equals( Void ); } }

      public BoundingBox( Vector3 min, Vector3 max )
      {
         this.Min = min;
         this.Max = max;
      }

      public BoundingBox( float minX, float maxX, float minY, float maxY, float minZ, float maxZ )
      {
         this.Min = new Vector3( minX, minY, minZ );
         this.Max = new Vector3( maxX, maxY, maxZ );
      }

      public void Add( Vector3 point )
      {
         if( point.X < this.Min.X )
            this.Min.X = point.X;
         else if( point.X > this.Max.X )
            this.Max.X = point.X;

         if( point.Y < this.Min.Y )
            this.Min.Y = point.Y;
         else if( point.Y > this.Max.Y )
            this.Max.Y = point.Y;

         if( point.Z < this.Min.Z )
            this.Min.Z = point.Z;
         else if( point.Z > this.Max.Z )
            this.Max.Z = point.Z;
      }

      // NOTE: can only use this method if internal state is not "void"...
      void Add_Fast( Vector3 point )
      {
         if( point.X < this.Min.X )
            this.Min.X = point.X;
         else if( point.X > this.Max.X )
            this.Max.X = point.X;

         if( point.Y < this.Min.Y )
            this.Min.Y = point.Y;
         else if( point.Y > this.Max.Y )
            this.Max.Y = point.Y;

         if( point.Z < this.Min.Z )
            this.Min.Z = point.Z;
         else if( point.Z > this.Max.Z )
            this.Max.Z = point.Z;
      }

      public void Add( BoundingBox bounds )
      {
         Add( bounds.Min );
         Add( bounds.Max );
      }

      public bool Contains( Vector3 point )
      {
         if( point.X < this.Min.X || point.X > this.Max.X ||
             point.Y < this.Min.Y || point.Y > this.Max.Y ||
             point.Z < this.Min.Z || point.Z > this.Max.Z )
            return false;

         return true;
      }

      public bool Contains( BoundingBox bounds )
      {
         return Contains( bounds.Min ) && Contains( bounds.Max );
      }

      public override string ToString()
      {
         return $"Min: {this.Min} - Max: {this.Max} - Width: {this.Width} - Depth: {this.Depth} - Height: {this.Height}";
      }

      public bool Equals( BoundingBox other )
      {
         return this.Min == other.Min && this.Max == other.Max;
      }

      public override bool Equals( object obj )
      {
         if( obj is BoundingBox bounds )
            return Equals( bounds );

         return base.Equals( obj );
      }

      public override int GetHashCode()
      {
         return this.Min.GetHashCode() ^ this.Max.GetHashCode();
      }

      public static readonly BoundingBox Empty = new BoundingBox();
      public static readonly BoundingBox Void = new BoundingBox( float.MaxValue, float.MaxValue, float.MaxValue, float.MinValue, float.MinValue, float.MinValue );

      public static BoundingBox FromVertices( IEnumerable<Vector3> vertices )
      {
         if( vertices == null )
            throw new ArgumentNullException( nameof( vertices ) );

         BoundingBox bounds = BoundingBox.Void;

         if( vertices.Any() )
         {
            // add first vertex
            bounds.Add( vertices.First() );

            foreach( Vector3 vertex in vertices.Skip( 1 ) )
            {
               bounds.Add_Fast( vertex );
            }
         }

         return bounds;
      }

   }

}