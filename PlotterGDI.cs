using System;
using System.Drawing;
using System.Collections.Generic;

using MetalHelix.Geometry;

namespace GeometryVisualizer
{
   public class PlotterGDI
   {
      #region Fields and Properties

      PointF plotScale;
      /// <summary> Gets and sets the Horizontal and Vertical scale for the view. </summary>
      public PointF PlotScale { get { return this.plotScale; } set { this.plotScale = value; } }

      PointF plotCenter;
      /// <summary> Get and set the center position of the view. </summary>
      public PointF PlotCenter { get { return this.plotCenter; } set { this.plotCenter = value; } }

      Size viewSize;
      /// <summary> Get and set the associated window size for the visualization. </summary>
      public Size ViewSize { get { return this.viewSize; } set { this.viewSize = value; } }


      /// <summary> Gets and sets the size the plot size of the visualization. </summary>
      public SizeF PlotSize
      {
         get
         {
            return new SizeF( this.viewSize.Width / this.plotScale.X, this.viewSize.Height / this.plotScale.Y );
         }
         set
         {
            this.plotScale.X = this.viewSize.Width * value.Width;
            this.plotScale.Y = this.viewSize.Height * value.Height;
         }
      }

      /// <summary> Minimum horizontal coordinate system value. </summary>
      public float PlotMinX
      {
         get
         {
            return this.plotCenter.X - this.PlotSize.Width / 2f;
         }
      }

      /// <summary> Maximum horizontal coordinate system value. </summary>
      public float PlotMaxX
      {
         get
         {
            return this.plotCenter.X + this.PlotSize.Width / 2f;
         }
      }

      /// <summary> Minimum vertical coordinate system value. </summary>
      public float PlotMinY
      {
         get
         {
            return this.plotCenter.Y - this.PlotSize.Height / 2f;
         }
      }

      /// <summary> Maximum vertical coordinate system value. </summary>
      public float PlotMaxY
      {
         get
         {
            return this.plotCenter.Y + this.PlotSize.Height / 2f;
         }
      }


      float gridDivisionMajor = 10f;
      public float GridDivisionMajor
      {
         get { return this.gridDivisionMajor; }
         set { this.gridDivisionMajor = value; }
      }

      float gridDivisionMinor = 10f;
      public float GridDivisionMinor
      {
         get { return this.gridDivisionMinor; }
         set { this.gridDivisionMinor = value; }
      }

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

      bool drawAxis = true;
      /// <summary> Whether the origin axis is drawn. </summary>
      public bool DrawAxis { get { return this.drawAxis; } set { this.drawAxis = value; } }

      bool drawGrid = true;
      /// <summary> Whether a full grid scale will be drawn. </summary>
      public bool DrawGrid { get { return this.drawGrid; } set { this.drawGrid = value; } }

      bool drawCircularPoints = true; // DrawStyle { Rect, Circle, etc..? }
      /// <summary> Whether points are drawn circular or rectangular. </summary>
      public bool DrawCircularPoints { get { return this.drawCircularPoints; } set { this.drawCircularPoints = value; } }

      bool maintainAspect = true;
      /// <summary> Whether the set scale is adjusted to maintain a square aspect ratio. </summary>
      public bool MaintainAspect { get { return this.maintainAspect; } set { this.maintainAspect = value; } }

      #endregion Fields and Properties

      #region Construction

      public PlotterGDI()
      {
         this.plotScale = new PointF( 1f, 1f );
         this.plotCenter = new PointF( 0f, 0f );
         this.viewSize = new Size( 100, 100 );

         this.defaultSet = new VertexSet();
         this.defaultFace = new Face();
         this.defaultSet.Faces.Add( this.defaultFace );
         this.vertexSets = new List<VertexSet>();

         this.selectedSet = this.defaultSet;
         this.selectedIndex = -1;
      }

      #endregion Construction

      #region Drawing

      RectangleF CalculateVertexBounds( List<Vector3> vertices )
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

      RectangleF GetAllVertexBounds()
      {
         RectangleF allBounds = CalculateVertexBounds( this.defaultSet.Vertices );

         foreach( VertexSet set in this.vertexSets )
         {
            RectangleF setBounds = CalculateVertexBounds( set.Vertices );

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

      /// <summary> Automatically set visualization scale for current points. </summary>
      public void AutoScaleForPoints()
      {
         RectangleF bounds = GetAllVertexBounds();

         // ensure bounds are valid -- not empty and not having width or height of infinity
         if( ( bounds.Width == 0f && bounds.Height == 0f )
            || float.IsInfinity( bounds.Width ) || float.IsInfinity( bounds.Height ) )
         {
            bounds = new RectangleF( -1f, -1f, 11f, 11f ); // set default bounds
         }
         else // inflate bounds to ensure visible contents
         {
            bounds.Inflate( bounds.Width * .1f, bounds.Height * .1f );
         }

         // calculate scale from contained points
         float xScale = this.viewSize.Width / bounds.Width;
         float yScale = this.viewSize.Height / bounds.Height;

         // check if aspect ratio should be retained
         if( this.maintainAspect )
         {
            if( xScale < yScale )
               yScale = xScale;
            else
               xScale = yScale;
         }

         // set the current plot scale
         this.plotScale = new PointF( xScale, yScale );

         // reset view center to middle of bounds
         this.plotCenter = new PointF( bounds.Left + bounds.Width / 2f, bounds.Top + bounds.Height / 2f );
      }

      void SetupTransform( Graphics grfx )
      {
         // offset device relative to the screen center
         grfx.TranslateTransform( this.viewSize.Width / 2f, this.viewSize.Height / 2f );

         // adjust by world unit scale (while inverting the y-axis)
         grfx.ScaleTransform( this.plotScale.X, -this.plotScale.Y );

         // translate point by current view center
         grfx.TranslateTransform( -this.plotCenter.X, -this.plotCenter.Y );
      }

      public void Draw( Graphics grfx )
      {
         SetupTransform( grfx );

         // TODO: DrawGrid method...

         // calculate current pixel size
         float pixelSizeX = 1f / this.plotScale.X;
         float pixelSizeY = 1f / this.plotScale.Y;
         float pixelSizeAvg = ( pixelSizeX + pixelSizeY ) / 2f;

         // create pen objects
         Pen penAxisX = new Pen( Color.Maroon, pixelSizeX * 2f );
         Pen penAxisY = new Pen( Color.ForestGreen, pixelSizeY * 2f );
         Pen penGrid = new Pen( Color.Gray, 0f );
         penGrid.Width = 0f;
         Pen penGrid2 = new Pen( Color.Silver, 0f );
         penGrid2.Width = 0f;

         // find maximum plot dimension
         float maxPlotDim = Math.Max( this.PlotSize.Width, this.PlotSize.Height );
         //maxPlotDim = (float)Math.Round( maxPlotDim );

         // calculate order of magnitude (number of zeros) for dynamic grid spacing
         float logSize = (float)Math.Log10( maxPlotDim );
         int digitPlaces = (int)logSize;
         float gridStepMajor = (float)Math.Pow( this.gridDivisionMajor, digitPlaces );
         float gridStepMinor = gridStepMajor / this.gridDivisionMinor;

         float scaleStepBeginX, scaleStepBeginY;
         float scaleStepEndX, scaleStepEndY;
         if( !this.drawGrid )
         {
            scaleStepBeginX = -gridStepMinor;
            scaleStepEndX = gridStepMinor;
            scaleStepBeginY = -gridStepMinor;
            scaleStepEndY = gridStepMinor;
         }
         else // extend scale step indicators to display full grid
         {
            scaleStepBeginX = this.PlotMinX;
            scaleStepEndX = this.PlotMaxX;
            scaleStepBeginY = this.PlotMinY;
            scaleStepEndY = this.PlotMaxY;
         }

         int maxGridLineCount = (int)( this.gridDivisionMajor * this.gridDivisionMinor );
         int numStepsX = (int)( this.PlotSize.Width / gridStepMinor ) + 2;
         int numStepsY = (int)( this.PlotSize.Height / gridStepMinor ) + 2;

         // calculate starting locations for scale step drawing,
         // such that they are evenly balanced around the origin
         float stepXMin = (int)( this.PlotMinX / gridStepMinor ) * gridStepMinor;
         float stepYMin = (int)( this.PlotMinY / gridStepMinor ) * gridStepMinor;

         // draw scale step indicators
         if( this.drawAxis || this.drawGrid )
         {
            int c = 0;
            for( float x = stepXMin; c < numStepsX; x += gridStepMinor, ++c )
            {
               if( x % gridStepMajor == 0 )
                  grfx.DrawLine( penGrid, x, scaleStepBeginY, x, scaleStepEndY );
               else
                  grfx.DrawLine( penGrid2, x, scaleStepBeginY, x, scaleStepEndY );
            }

            c = 0;
            for( float y = stepYMin; c < numStepsY; y += gridStepMinor, ++c )
            {
               if( y % gridStepMajor == 0 )
                  grfx.DrawLine( penGrid, scaleStepBeginX, y, scaleStepEndX, y );
               else
                  grfx.DrawLine( penGrid2, scaleStepBeginX, y, scaleStepEndX, y );
            }
         }


         // draw origin axis lines
         if( this.drawAxis )
         {
            grfx.DrawLine( penAxisX, this.PlotMinX, 0f, this.PlotMaxX, 0f );
            grfx.DrawLine( penAxisY, 0f, this.PlotMinY, 0f, this.PlotMaxY );
         }

         // release pen resources
         penAxisX.Dispose();
         penAxisY.Dispose();
         penGrid.Dispose();


         // draw vertex sets

         Pen linePen = new Pen( Color.RoyalBlue, pixelSizeAvg * 2f );

         // draw default vertex set faces/lines
         DrawSetFaceLines( grfx, linePen, this.defaultSet );

         // draw vertex set faces/lines
         for( int i = 0; i < this.vertexSets.Count; ++i )
         {
            DrawSetFaceLines( grfx, linePen, this.vertexSets[i] );
         }

         linePen.Dispose();

         // each point will be drawn with a radius of these many pixels :)
         float pointRad = 4;

         // calculate radius to use for point circles
         float pointRadiusX = pixelSizeX * pointRad;
         float pointRadiusY = pixelSizeY * pointRad;

         // create brush for filling point circles
         Brush defaultBrush = new SolidBrush( Color.Black );
         Brush selectedBrush = new SolidBrush( Color.DeepSkyBlue );
         Pen selectedPen = new Pen( Color.DeepSkyBlue, 0f );

         // draw default vertex set points
         DrawSetPoints( grfx, this.defaultSet, pointRadiusX, pointRadiusY, defaultBrush, selectedBrush, selectedPen );

         // draw vertex set points
         for( int i = 0; i < this.vertexSets.Count; ++i )
         {
            DrawSetPoints( grfx, this.vertexSets[i], pointRadiusX, pointRadiusY, defaultBrush, selectedBrush, selectedPen );
         }

         // release brush resources
         defaultBrush.Dispose();
         selectedBrush.Dispose();
      }

      void DrawSetFaceLines( Graphics grfx, Pen linePen, VertexSet vertexSet )
      {
         Face currFace;
         Vector3 start, end;

         linePen.Color = vertexSet.LineColor.IsEmpty ? Color.RoyalBlue : vertexSet.LineColor;

         // check if custom faces defined for vertices
         if( vertexSet.FaceCount > 0 )
         {
            // draw faces for set
            for( int f = 0; f < vertexSet.Faces.Count; ++f )
            {
               currFace = vertexSet.Faces[f];
               for( int i = 0; i < currFace.Indices.Count - 1; ++i )
               {
                  start = vertexSet.Vertices[currFace.Indices[i]];
                  end = vertexSet.Vertices[currFace.Indices[i + 1]];
                  grfx.DrawLine( linePen, start.ToPointF(), end.ToPointF() );
               }
               // draw last closing line
               if( currFace.Indices.Count > 2 )
               {
                  start = vertexSet.Vertices[currFace.Indices[currFace.Indices.Count - 1]];
                  end = vertexSet.Vertices[currFace.Indices[0]];
                  grfx.DrawLine( linePen, start.ToPointF(), end.ToPointF() );
               }
            }
         }
         else if( vertexSet.IsPolyline && vertexSet.Vertices.Count > 1 )
         {
            for( int i = 0; i < vertexSet.Vertices.Count - 1; ++i )
            {
               start = vertexSet.Vertices[i];
               end = vertexSet.Vertices[i + 1];
               grfx.DrawLine( linePen, start.ToPointF(), end.ToPointF() );
            }
            // draw last closing line
            if( vertexSet.IsClosed && vertexSet.Vertices.Count > 2 )
            {
               start = vertexSet.Vertices[vertexSet.Vertices.Count - 1];
               end = vertexSet.Vertices[0];
               grfx.DrawLine( linePen, start.ToPointF(), end.ToPointF() );
            }
         }
      }

      private void DrawSetPoints( Graphics grfx, VertexSet vertexSet, float pointRadiusX, float pointRadiusY, Brush defaultBrush, Brush selectedBrush, Pen selectedPen )
      {
         bool isSelectedSet = this.selectedSet == vertexSet;

         Brush activeBrush = null;
         PointF pt;

         // draw points
         for( int i = 0; i < vertexSet.Vertices.Count; ++i )
         {
            activeBrush = isSelectedSet && i == this.selectedIndex ? selectedBrush : defaultBrush;

            pt = vertexSet.Vertices[i].ToPointF();

            if( !this.drawCircularPoints )
               grfx.FillRectangle( activeBrush, pt.X - pointRadiusX, pt.Y - pointRadiusY, pointRadiusX * 2, pointRadiusY * 2 );
            else
               grfx.FillEllipse( activeBrush, pt.X - pointRadiusX, pt.Y - pointRadiusY, pointRadiusX * 2, pointRadiusY * 2 );

            if( isSelectedSet && i == this.selectedIndex )
            {
               grfx.DrawEllipse( selectedPen, pt.X - pointRadiusX * 2, pt.Y - pointRadiusY * 2, pointRadiusX * 4, pointRadiusY * 4 );
            }
         }
      }

      #endregion Drawing

      #region Point Conversion

      public PointF PointToWorld( Point screenPoint )
      {
         // offset point relative to the screen center
         float worldPointX = screenPoint.X - this.viewSize.Width / 2f;
         float worldPointY = screenPoint.Y - this.viewSize.Height / 2f;

         // adjust point by world unit scale
         worldPointX = worldPointX / this.plotScale.X;
         worldPointY = worldPointY / -this.plotScale.Y; // invert y-axis

         // translate point by current view center
         worldPointX = worldPointX + this.plotCenter.X;
         worldPointY = worldPointY + this.plotCenter.Y;

         return new PointF( worldPointX, worldPointY );
      }

      public PointF PointToScreen( PointF worldPoint )
      {
         // translate point by current view center
         float screenPointX = worldPoint.X - this.plotCenter.X;
         float screenPointY = worldPoint.Y - this.plotCenter.Y;

         // adjust point by world unit scale
         screenPointX = screenPointX * this.plotScale.X;
         screenPointY = screenPointY * -this.plotScale.Y;

         // offset point relative to the screen center
         screenPointX = screenPointX + this.viewSize.Width / 2f;
         screenPointY = screenPointY + this.viewSize.Height / 2f;

         return new Point( (int)screenPointX, (int)screenPointY );
      }

      #endregion Point Conversion
   }

   static class Vector3Extensions
   {
      public static PointF ToPointF( this Vector3 vector )
      {
         return new PointF( vector.X, vector.Y );
      }
   }
}