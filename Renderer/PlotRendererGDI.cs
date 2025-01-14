using System;
using System.Drawing;
using System.Collections.Generic;

using MetalHelix.Geometry;

namespace GeometryVisualizer
{
   public abstract class PlotRenderer
   {

      public abstract void Draw( Graphics grfx );

      public abstract PointF PointToWorld( Point screenPoint );

      public abstract PointF PointToScreen( PointF worldPoint );


   }

   public class PlotRendererGDI : PlotRenderer
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

      PlotData plotData;
      public PlotData PlotData
      {
         get { return this.plotData; }
         set { this.plotData = value; }
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

      public PlotRendererGDI( PlotData sourcePlotData = null )
      {
         this.plotScale = new PointF( 1f, 1f );
         this.plotCenter = new PointF( 0f, 0f );
         this.viewSize = new Size( 100, 100 );

         this.plotData = sourcePlotData ?? new PlotData();
      }

      #endregion Construction

      #region Drawing

      void SetupTransform( Graphics grfx )
      {
         // offset device relative to the screen center
         grfx.TranslateTransform( this.viewSize.Width / 2f, this.viewSize.Height / 2f );

         // adjust by world unit scale (while inverting the y-axis)
         grfx.ScaleTransform( this.plotScale.X, -this.plotScale.Y );

         // translate point by current view center
         grfx.TranslateTransform( -this.plotCenter.X, -this.plotCenter.Y );
      }

      public override void Draw( Graphics grfx )
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
         DrawSetFaceLines( grfx, linePen, this.plotData.DefaultPlotSet );

         // draw vertex set faces/lines
         for( int i = 0; i < this.plotData.VertexSets.Count; ++i )
         {
            DrawSetFaceLines( grfx, linePen, this.plotData.VertexSets[i] );
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
         DrawSetPoints( grfx, this.plotData.DefaultPlotSet, pointRadiusX, pointRadiusY, defaultBrush, selectedBrush, selectedPen );

         // draw vertex set points
         for( int i = 0; i < this.plotData.VertexSets.Count; ++i )
         {
            DrawSetPoints( grfx, this.plotData.VertexSets[i], pointRadiusX, pointRadiusY, defaultBrush, selectedBrush, selectedPen );
         }

         // release brush resources
         defaultBrush.Dispose();
         selectedBrush.Dispose();
         selectedPen.Dispose();
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
         bool isSelectedSet = this.plotData.SelectedSet == vertexSet;

         Brush activeBrush = null;
         PointF pt;

         // draw points
         for( int i = 0; i < vertexSet.Vertices.Count; ++i )
         {
            activeBrush = isSelectedSet && i == this.plotData.SelectedIndex ? selectedBrush : defaultBrush;

            pt = vertexSet.Vertices[i].ToPointF();

            if( !this.drawCircularPoints )
               grfx.FillRectangle( activeBrush, pt.X - pointRadiusX, pt.Y - pointRadiusY, pointRadiusX * 2, pointRadiusY * 2 );
            else
               grfx.FillEllipse( activeBrush, pt.X - pointRadiusX, pt.Y - pointRadiusY, pointRadiusX * 2, pointRadiusY * 2 );

            if( isSelectedSet && i == this.plotData.SelectedIndex )
            {
               grfx.DrawEllipse( selectedPen, pt.X - pointRadiusX * 2, pt.Y - pointRadiusY * 2, pointRadiusX * 4, pointRadiusY * 4 );
            }
         }
      }

      #endregion Drawing

      #region Point Conversion

      public override PointF PointToWorld( Point screenPoint )
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

      public override PointF PointToScreen( PointF worldPoint )
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