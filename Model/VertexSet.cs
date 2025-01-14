using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using MetalHelix.Geometry;

namespace GeometryVisualizer
{

   public class VertexSet
   {
      List<Vector3> vertices;
      public List<Vector3> Vertices { get { return this.vertices; } }

      public int Count { get { return this.vertices.Count; } }

      List<Face> faces;
      public List<Face> Faces { get { return this.faces; } }

      public int FaceCount { get { return this.faces.Count; } }

      List<Segment> lines;
      public List<Segment> Lines { get { return this.lines; } }

      public int LineCount { get { return this.lines.Count; } }

      /// <summary> Gets or sets whether this set of vertices should be drawn connected as a polyline. </summary>
      public bool IsPolyline { get; set; }

      /// <summary> Gets or sets whether this connected set of polygon vertices should be draw closed
      /// -- connecting the last vertex to the first. (Only used if IsPolygon is also enabled.) </summary>
      public bool IsClosed { get; set; }

      /// <summary> Gets whether the vertices are used as 2D coordinates -- ignoring the Z component. </summary>
      public bool Is2D { get; set; }

      // TODO: define bools as flags..?

      public Color PointColor { get; set; }

      public Color LineColor { get; set; }

      public Color FillColor { get; set; }

      public VertexSet()
      {
         this.vertices = new List<Vector3>();
         this.faces = new List<Face>();
         this.lines = new List<Segment>();
      }

      public VertexSet( VertexSet source )
      {
         this.vertices = new List<Vector3>( source.vertices );
         this.faces = new List<Face>( source.faces );
         this.lines = new List<Segment>( source.lines );

         this.IsPolyline = source.IsPolyline;
         this.IsClosed = source.IsClosed;
         this.Is2D = source.Is2D;
         this.PointColor  = source.PointColor;
         this.LineColor = source.LineColor;
         this.FillColor = source.FillColor;
      }

      /// <summary> Add a single vertex specifying an X, Y, and Z value. </summary>
      public void AddVertex( float x, float y, float z )
      {
         Vector3 vert = new Vector3( x, y, z );
         this.vertices.Add( vert );
      }

      /// <summary> Add a single vertex using values from a list of float values. </summary>
      /// <param name="floatList"></param>
      public void AddVertex( IList<float> floatList )
      {
         Vector3 vert = new Vector3();
         for( int i = 0; i < 3 && i < floatList.Count; ++i )
         {
            switch( i )
            {
               case 0:
                  vert.X = floatList[i];
                  break;
               case 1:
                  vert.Y = floatList[i];
                  break;
               case 2:
                  vert.Z = floatList[i];
                  this.Is2D = false;
                  break;
            }
         }
         this.vertices.Add( vert );
      }

      public void AutoSet2D()
      {
         bool hasZCoords = false;
         for( int i = 0; i < this.vertices.Count; ++i )
         {
            if( this.vertices[i].Z != 0f )
            {
               hasZCoords = true;
               break;
            }
         }
         this.Is2D = !hasZCoords;
      }

      /// <summary> Add multiple vertices by taking every two or three floats from a continuous list.  </summary>
      /// <param name="floatList"> List of float values. </param>
      /// <param name="pairHint"> Indicates whether vertices should be 2D or 3D sets of floats. </param>
      /// <returns> Number of vertices added. </returns>
      public int AddVertices( List<float> floatList, int pairHint = 2 )
      {
         int verticesAdded = 0;

         if( floatList.Count % pairHint != 0 )
         {
            // break-up continuous array of floats based on specified pairing number

            float[] floatArray = floatList.ToArray();
            for( int i = 0; i < floatArray.Length; i += pairHint )
            {
               ArraySegment<float> currVertFloats = new ArraySegment<float>( floatArray, i, pairHint );
               AddVertex( currVertFloats );
            }
         }

         return verticesAdded;
      }

      public string GenerateText()
      {
         StringBuilder text = new StringBuilder();

         if( this.IsPolyline )
         {
            string groupChar = this.IsClosed ? "{" : "[";
            // begin vertex set with opening curly brace
            text.AppendLine( groupChar );
         }

         Vector3 vert;
         for( int i = 0; i < this.vertices.Count; ++i )
         {
            vert = this.vertices[i];
            if( this.Is2D )
            {
               text.AppendLine( $"   {vert.X}, {vert.Y}" );
            }
            else
            {
               text.AppendLine( $"   {vert.X}, {vert.Y}, {vert.Z}" );
            }
         }

         // check if custom faces are defined
         // ...

         // check if custom lines/segments are defined
         // ...

         if( this.IsPolyline )
         {
            string groupChar = this.IsClosed ? "}" : "]";
            // end vertex set with closing curly brace
            text.AppendLine( groupChar );
         }

         return text.ToString();
      }
   }

}