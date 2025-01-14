using System;

namespace MetalHelix.Geometry
{
   /// <summary>
   /// Represents a local 3D coordinate system defined by a local origin and orientation.
   /// </summary>
   public class OrthoCamera
   {
      Vector3 origin;
      public Vector3 Origin
      {
         get { return this.origin; }
         set { this.origin = value; }
      }

      Vector3 axisX;
      public Vector3 AxisX
      {
         get { return this.axisX; }
      }

      Vector3 axisY;
      public Vector3 AxisY
      {
         get { return this.axisY; }
      }

      Vector3 axisZ;
      public Vector3 AxisZ
      {
         get { return this.axisZ; }
      }

      public OrthoCamera( Vector3 viewAxisX, Vector3 viewAxisY )
      {
         this.axisX = Vector3.Normalize( viewAxisX );
         this.axisY = Vector3.Normalize( viewAxisY );
         this.axisZ = Vector3.Normalize( viewAxisX.Cross( viewAxisY ) );
      }

      public OrthoCamera( Vector3 viewAxisX, Vector3 viewAxisY, Vector3 viewAxisZ )
      {
         this.axisX = Vector3.Normalize( viewAxisX );
         this.axisY = Vector3.Normalize( viewAxisY );
         this.axisZ = Vector3.Normalize( viewAxisZ );
      }

      public Vector3 TransformPoint( Vector3 point )
      {
         point = point - this.origin;
         float x = this.axisX.Dot( point );
         float y = this.axisY.Dot( point );
         float z = this.axisZ.Dot( point );
         return new Vector3( x, y, z );
      }

      public Vector3 TransformNormal( Vector3 point )
      {
         float x = this.axisX.Dot( point );
         float y = this.axisY.Dot( point );
         float z = this.axisZ.Dot( point );
         return new Vector3( x, y, z );
      }

      public void Rotate( float horizontalAngle, float verticalAngle )
      {
         if( horizontalAngle != 0f )
         {
            Vector3 horzAxis = this.axisY;
            this.axisX.Rotate( horzAxis, horizontalAngle );
            this.axisZ.Rotate( horzAxis, horizontalAngle );
         }

         if( verticalAngle != 0f )
         {
            Vector3 vertAxis = this.axisX;
            this.axisY.Rotate( vertAxis, verticalAngle );
            this.axisZ.Rotate( vertAxis, verticalAngle );
         }
      }
   }

}