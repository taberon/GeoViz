using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using MetalHelix.Geometry;

namespace GeometryVisualizer
{


   public class PlotData
   {
      VertexSet defaultSet;
      /// <summary> Get the default plot set. </summary>
      public VertexSet DefaultPlotSet
      {
         get { return this.defaultSet; }
      }

      Face defaultFace;
      /// <summary> Get the default plot face -- index list. </summary>
      public Face DefaultPlotFace
      {
         get { return this.defaultFace; }
      }

      /// <summary> Collection of points to be drawn. </summary>
      public List<Vector3> Points
      {
         get { return this.defaultSet.Vertices; }
      }

      /// <summary> Collection of line indices to be drawn. </summary>
      public List<int> Lines // int[][] -- use plain array get/set for short-hand def of "mini" (degenerate) faces..?
      {
         get { return this.defaultFace.Indices; }
      }

      List<VertexSet> vertexSets;
      /// <summary> Collection of custom VertexSet instances to be drawn. </summary>
      public List<VertexSet> VertexSets
      {
         get { return this.vertexSets; }
      }

      VertexSet selectedSet;
      /// <summary> Gets or sets the currently selected VertexSet. </summary>
      public VertexSet SelectedSet
      {
         get { return this.selectedSet; }
         set { this.selectedSet = value; }
      }

      int selectedIndex;
      /// <summary> Gets or sets a single selected point, to be drawn in a different highlight color. </summary>
      public int SelectedIndex
      {
         get
         {
            // validate the selected index with actual point count
            if( this.selectedSet != null && this.selectedIndex >= this.selectedSet.Count )
               this.selectedIndex = -1;

            return this.selectedIndex;
         }
         set { this.selectedIndex = value; }
      }


      public PlotData()
      {
         this.defaultSet = new VertexSet();
         this.defaultFace = new Face();
         this.defaultSet.Faces.Add( this.defaultFace );

         this.vertexSets = new List<VertexSet>();

         this.selectedSet = this.defaultSet;
         this.selectedIndex = -1;
      }

      public PlotData( PlotData source )
      {
         this.defaultSet = new VertexSet( source.defaultSet );
         this.defaultFace = this.defaultSet.Faces.FirstOrDefault(); //new Face( source.defaultFace );
         //this.defaultSet.Faces.Add( this.defaultFace );

         this.vertexSets = new List<VertexSet>();
         for( int i = 0; i < source.vertexSets.Count; ++i )
         {
            this.vertexSets.Add( new VertexSet( source.vertexSets[i] ) );
         }

         this.selectedSet = source.selectedSet;
         this.selectedIndex = source.selectedIndex;
      }

      public RectangleF GetBounds2D()
      {
         RectangleF allBounds = DrawingUtility.CalculateVertexBounds( this.defaultSet.Vertices );

         foreach( VertexSet set in this.vertexSets )
         {
            RectangleF setBounds = DrawingUtility.CalculateVertexBounds( set.Vertices );

            float minX = Math.Min( allBounds.Left, setBounds.Left );
            float maxX = Math.Max( allBounds.Right, setBounds.Right );
            float minY = Math.Min( allBounds.Top, setBounds.Top );
            float maxY = Math.Max( allBounds.Bottom, setBounds.Bottom );

            allBounds.X = minX;
            allBounds.Width = maxX - minX;
            allBounds.Y = minY;
            allBounds.Height = maxY - minY;
         }

         return allBounds;
      }

      public BoundingBox GetBounds3D()
      {
         BoundingBox allBounds = BoundingBox.FromVertices( this.defaultSet.Vertices );

         foreach( VertexSet set in this.vertexSets )
         {
            BoundingBox setBounds = BoundingBox.FromVertices( set.Vertices );
            allBounds.Add( setBounds );
         }

         return allBounds;
      }


      public void DeleteSelectedPoint()
      {
         if( this.SelectedSet != null && this.SelectedIndex >= 0 && this.SelectedIndex < this.SelectedSet.Count )
         {
            // remove point from list
            this.SelectedSet.Vertices.RemoveAt( this.SelectedIndex );
            // set selected point as previous index
            AdvanceSelectedPoint( -1 );
         }
      }

      public void ClearAll()
      {
         // clear all vertex sets
         this.Points.Clear();
         this.Lines.Clear();
         this.VertexSets.Clear();
         this.SelectedIndex = -1;
         this.SelectedSet = null;
      }

      public void AdvanceSelectedPoint( int dir = 1 )
      {
         if( this.SelectedSet == null )
         {
            this.SelectedSet = this.DefaultPlotSet;
         }

         // get current selected point index
         int currIndex = this.SelectedIndex;
         // advance by specified direction/amount
         currIndex += dir;

         // bounds check
         if( currIndex >= this.SelectedSet.Count )
            currIndex = 0;
         else if( currIndex < 0 )
            currIndex = this.SelectedSet.Count - 1;

         // check for empty point collection
         if( this.SelectedSet.Count == 0 )
            currIndex = -1;

         // set selected point
         this.SelectedIndex = currIndex;

         // notify of plot view change
         //OnPlotViewChanged();

         // request redraw of plot view
         //this.Invalidate();
      }


   }

}
