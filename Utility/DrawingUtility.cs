using System;
using System.Drawing;
using System.Collections.Generic;
using MetalHelix.Geometry;

namespace GeometryVisualizer
{

   public static class DrawingUtility
   {

      public static RectangleF CalculateVertexBounds( List<Vector3> vertices )
      {
         Vector3 min = new Vector3( float.MaxValue, float.MaxValue, float.MaxValue );
         Vector3 max = new Vector3( float.MinValue, float.MinValue, float.MinValue );

         foreach( Vector3 vertex in vertices )
         {
            if( vertex.X < min.X )
               min.X = vertex.X;
            if( vertex.X > max.X )
               max.X = vertex.X;

            if( vertex.Y < min.Y )
               min.Y = vertex.Y;
            if( vertex.Y > max.Y )
               max.Y = vertex.Y;

            if( vertex.Z < min.Z )
               min.Z = vertex.Z;
            if( vertex.Z > max.Z )
               max.Z = vertex.Z;
         }

         // TODO: use a 3d bounding-box...
         RectangleF bounds = RectangleF.FromLTRB( min.X, min.Y, max.X, max.Y );
         return bounds;
      }


      public static bool ClipLineToViewRect( Rectangle rect, Vector2 pt1, Vector2 pt2 )
      {
         return false;
      }

      public static bool ClipRayToViewRect( RectangleF rect, Vector2 loc, Vector2 dir )
      {
         float slope = dir.Y / dir.X;

         // check for y-intercept with left and right view rectangle edges

         float distLeft = loc.X - rect.Left;
         float yLeft = slope * distLeft;
         
         float distRight = loc.X - rect.Right;
         float yRight = slope * distLeft;



         return false;

      }

   }

   public class Ray
   {
      public PointF Start { get; set; }
      public PointF Direction { get; set; }

      public Ray( PointF start, PointF direction )
      {
         Start = start;
         Direction = direction;
      }
   }

   public class RayClipper
   {
      public static bool ClipRayToRectangle( Ray ray, RectangleF rect, out PointF clippedStart, out PointF clippedEnd )
      {
         clippedStart = ray.Start;
         clippedEnd = new PointF( ray.Start.X + ray.Direction.X, ray.Start.Y + ray.Direction.Y );

         float t0 = 0.0f;
         float t1 = 1.0f;

         if( ClipTest( -ray.Direction.X, ray.Start.X - rect.Left, ref t0, ref t1 ) &&
             ClipTest( ray.Direction.X, rect.Right - ray.Start.X, ref t0, ref t1 ) &&
             ClipTest( -ray.Direction.Y, ray.Start.Y - rect.Top, ref t0, ref t1 ) &&
             ClipTest( ray.Direction.Y, rect.Bottom - ray.Start.Y, ref t0, ref t1 ) )
         {
            if( t1 < 1.0f )
            {
               clippedEnd = new PointF( ray.Start.X + t1 * ray.Direction.X, ray.Start.Y + t1 * ray.Direction.Y );
            }
            if( t0 > 0.0f )
            {
               clippedStart = new PointF( ray.Start.X + t0 * ray.Direction.X, ray.Start.Y + t0 * ray.Direction.Y );
            }
            return true;
         }

         return false;
      }

      private static bool ClipTest( float p, float q, ref float t0, ref float t1 )
      {
         if( p == 0.0f )
         {
            if( q < 0.0f ) return false;
         }
         else
         {
            float r = q / p;
            if( p < 0.0f )
            {
               if( r > t1 ) return false;
               if( r > t0 ) t0 = r;
            }
            else
            {
               if( r < t0 ) return false;
               if( r < t1 ) t1 = r;
            }
         }
         return true;
      }
   }


}