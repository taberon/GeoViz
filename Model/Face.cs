using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using MetalHelix.Geometry;

namespace GeometryVisualizer
{

   public class Face
   {
      List<int> indices;
      public List<int> Indices
      {
         get { return this.indices; }
      }

      public Face()
      {
         this.indices = new List<int>();
      }

      public Face( params int[] faceIndices )
      {
         this.indices = new List<int>( faceIndices );
      }

      public Face( Face source )
      {
         this.indices = new List<int>( source.indices );
      }
   }

}