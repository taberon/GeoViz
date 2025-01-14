using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using MetalHelix.Geometry;

namespace GeometryVisualizer
{
   public class PlotView3D : UserControl, IPlotView
   {
      PlotData plotData;
      public PlotData PlotData { get { return this.plotData; } }

      PlotData xformPlotData;

      PlotViewOptions plotOptions;
      public PlotViewOptions PlotOptions { get { return this.plotOptions; } }

      PlotRendererGDI plotRenderer;
      public PlotRenderer PlotRenderer
      {
         get { return this.plotRenderer; }
      }

      bool cameraChanged;

      OrthoCamera camera;
      public OrthoCamera Camera { get { return this.camera; } }

      /// <summary> Event fired whenever the plot scale or view center is changed. </summary>
      public event PlotViewChangedDelegate PlotViewChanged;

      public PlotView3D()
      {
         // create default plot data
         this.plotData = new PlotData();

         // set default plot options
         this.plotOptions = new PlotViewOptions();

         // create renderer
         this.plotRenderer = new PlotRendererGDI();
         this.plotRenderer.PlotData = this.plotData;

         // create 3d camera
         this.camera = new OrthoCamera( Vector3.UnitX, Vector3.UnitY );
         this.camera.Origin = Vector3.Zero;

         // set initial view bounds
         this.plotRenderer.ViewSize = this.ClientSize;

         this.BackColor = Color.LightGray;

         //this.DoubleBuffered = true;
         this.SetStyle( ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true );

         // create default plot data for 3d view
         LoadCubePoints();
      }

      void LoadCubePoints()
      {
         this.plotData.Points.Add( new Vector3( -1, -1, -1 ) );
         this.plotData.Points.Add( new Vector3( 1, -1, -1 ) );
         this.plotData.Points.Add( new Vector3( 1, 1, -1 ) );
         this.plotData.Points.Add( new Vector3( -1, 1, -1 ) );

         this.plotData.Points.Add( new Vector3( -1, -1, 1 ) );
         this.plotData.Points.Add( new Vector3( 1, -1, 1 ) );
         this.plotData.Points.Add( new Vector3( 1, 1, 1 ) );
         this.plotData.Points.Add( new Vector3( -1, 1, 1 ) );

         //VertexSet face1 = new VertexSet();
         //face1.Vertices.Add(
         //this.plotData.VertexSets.Add( new VertexSet() { 

         //this.plotData.Lines.AddRange( new int[] { 0, 1, 1, 2, 2, 3, 3, 0 } );
         //this.plotData.Lines.AddRange( new int[] { 4, 5, 5, 6, 6, 7, 7, 4 } );

         this.plotData.DefaultPlotSet.Faces.Add( new Face( 0, 1, 2, 3 ) );
         this.plotData.DefaultPlotSet.Faces.Add( new Face( 4, 5, 6, 7 ) );

         this.plotData.DefaultPlotSet.AutoSet2D();
      }

      /// <summary> Calculate scale and view center for for current plot data. </summary>
      void CalculateScaleForPoints( out PointF plotScale, out PointF plotCenter )
      {
         Size viewSize = this.ClientSize;
         RectangleF bounds = this.plotData.GetBounds2D();

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
         float xScale = viewSize.Width / bounds.Width;
         float yScale = viewSize.Height / bounds.Height;

         // check if aspect ratio should be retained
         if( this.plotOptions.MaintainAspect )
         {
            if( xScale < yScale )
               yScale = xScale;
            else
               xScale = yScale;
         }

         // set the current plot scale
         plotScale = new PointF( xScale, yScale );

         // reset view center to middle of bounds
         plotCenter = new PointF( bounds.Left + bounds.Width / 2f, bounds.Top + bounds.Height / 2f );
      }

      public void BestFitView()
      {
         CalculateScaleForPoints( out PointF plotScale, out PointF plotCenter );
         this.plotRenderer.PlotScale = plotScale;
         this.plotRenderer.PlotCenter = plotCenter;
         this.plotRenderer.PlotData = this.plotData;

         // reset camera view
         this.camera = new OrthoCamera( Vector3.UnitX, Vector3.UnitY );
         this.camera.Origin = Vector3.Zero;
         this.cameraChanged = false;

         // notify of plot view change
         OnPlotViewChanged();
      }

      protected virtual void OnPlotViewChanged()
      {
         if( this.cameraChanged )
         {
            this.cameraChanged = false;

            // copy plot data
            this.xformPlotData = new PlotData( this.plotData );

            // transform/project plot vertices
            TransformVertexSet( this.xformPlotData.DefaultPlotSet );
            for( int i = 0; i < this.xformPlotData.VertexSets.Count; ++i )
            {
               TransformVertexSet( this.xformPlotData.VertexSets[i] );
            }

            this.plotRenderer.PlotData = this.xformPlotData;

            // TODO: should this go here..? maybe, but currently all calls to this method also call Invalidate()...
            //this.Invalidate();
         }

         this.PlotViewChanged?.Invoke();
      }

      void TransformVertexSet( VertexSet vertexSet )
      {
         for( int i = 0; i < vertexSet.Vertices.Count; ++i )
         {
            vertexSet.Vertices[i] = this.camera.TransformPoint( vertexSet.Vertices[i] );
         }
      }

      public string GetPlotStatusBounds()
      {
         return $"X: [{this.plotRenderer.PlotMinX:G3}, {this.plotRenderer.PlotMaxX:G3}] - Y: [{this.plotRenderer.PlotMinY:G3}, {this.plotRenderer.PlotMaxY:G3}]";
      }

      public string GetPlotStatusSize()
      {
         return $"{this.plotRenderer.PlotSize.Width:G3}x{this.plotRenderer.PlotSize.Height:G3}";
      }

      #region Drawing Overrides

      protected override void OnPaint( PaintEventArgs e )
      {
         base.OnPaint( e );

         //e.Graphics.DrawRectangle( Pens.Blue, 0, 0, this.Width - 1, this.Height - 1 );

         e.Graphics.Clear( this.BackColor );

         // draw origin axes lines
         DrawOriginLines( e.Graphics );

         //Vector3.IsParallel( ...
         bool cameraAxisOriented = false;
         const float dotTol = 0.000001f;
         if( Math.Abs( this.camera.AxisZ.Dot( Vector3.UnitX ) ) + dotTol >= 1f
            || Math.Abs( this.camera.AxisZ.Dot( Vector3.UnitY ) ) + dotTol >= 1f
            || Math.Abs( this.camera.AxisZ.Dot( Vector3.UnitZ ) ) + dotTol >= 1f )
         {
            cameraAxisOriented = true;
         }
         else
         {
            // draw transformed axis grid lines...
            //this.plotRenderer.PlotScale
         }

         // set current plot options
         this.plotRenderer.DrawAxis = this.plotOptions.DrawAxis && cameraAxisOriented;
         this.plotRenderer.DrawGrid = this.plotOptions.DrawGrid; // && cameraAxisOriented;
         this.plotRenderer.MaintainAspect = this.plotOptions.MaintainAspect;


         System.Drawing.Drawing2D.GraphicsState state = e.Graphics.Save();

         // draw geometry visualization
         this.plotRenderer.Draw( e.Graphics );

         e.Graphics.Restore( state );

         // draw orientation axes display
         DrawOrientationAxis( e.Graphics );
      }

      void DrawOriginLines( Graphics grfx )
      {
         
         Vector3 worldX = this.camera.TransformNormal( Vector3.UnitX );
         Vector3 worldY = this.camera.TransformNormal( -Vector3.UnitY ); // invert y-axis to match scree-space orientation
         Vector3 worldZ = this.camera.TransformNormal( Vector3.UnitZ );

         Pen linePen = new Pen( Color.Blue, 0f );

         int radX = this.ClientSize.Width / 2;
         int radY = this.ClientSize.Height / 2;
         int maxLen = Math.Max( this.ClientSize.Width, this.ClientSize.Height );

         Vector2 origin = new Vector2( radX, radY );

         Vector2 lineX = origin + new Vector2( worldX.X, worldX.Y ) * maxLen;
         Vector2 lineY = origin + new Vector2( worldY.X, worldY.Y ) * maxLen;
         Vector2 lineZ = origin + new Vector2( worldZ.X, worldZ.Y ) * maxLen;

         RectangleF viewRect = new RectangleF( 0f, 0f, this.ClientSize.Width, this.ClientSize.Height );

         Ray xRay = new Ray( origin, new PointF( worldX.X, worldX.Y ) );
         if( RayClipper.ClipRayToRectangle( xRay, viewRect, out PointF xAxis1, out PointF xAxis2 ) )
            grfx.DrawLine( linePen, xAxis1, xAxis2 );

         //grfx.DrawLine( linePen, origin, lineZ );
         //grfx.DrawLine( linePen, origin, -lineZ );
         linePen.Color = Color.Green;
         //grfx.DrawLine( linePen, origin, lineY );
         //grfx.DrawLine( linePen, origin, -lineY );
         linePen.Color = Color.Red;
         //grfx.DrawLine( linePen, origin, lineX );
         //grfx.DrawLine( linePen, origin, -lineX );

         linePen.Dispose();
      }

      void DrawOrientationAxis( Graphics grfx )
      {
         // draw world axes in bottom corner
         Vector3 worldX = this.camera.TransformNormal( Vector3.UnitX );
         Vector3 worldY = this.camera.TransformNormal( -Vector3.UnitY ); // invert y-axis to match scree-space orientation
         Vector3 worldZ = this.camera.TransformNormal( Vector3.UnitZ );

         float lineLen = 32f;

         Pen linePen = new Pen( Color.Blue, 0f );

         Vector2 origin = new Vector2( lineLen * 1.5f, this.ClientSize.Height - lineLen * 1.5f );
         Vector2 lineX = origin + new Vector2( worldX.X, worldX.Y ) * lineLen;
         Vector2 lineY = origin + new Vector2( worldY.X, worldY.Y ) * lineLen;
         Vector2 lineZ = origin + new Vector2( worldZ.X, worldZ.Y ) * lineLen;

         grfx.DrawLine( linePen, origin, lineZ );
         linePen.Color = Color.Green;
         grfx.DrawLine( linePen, origin, lineY );
         linePen.Color = Color.Red;
         grfx.DrawLine( linePen, origin, lineX );

         linePen.Dispose();
      }

      protected override void OnResize( EventArgs e )
      {
         base.OnResize( e );

         this.plotRenderer.ViewSize = this.ClientSize;

         this.Invalidate();

         // notify of plot view change
         OnPlotViewChanged();
      }

      #endregion Drawing Overrides

      #region Input Event Overrides

      protected override void OnMouseWheel( MouseEventArgs e )
      {
         base.OnMouseWheel( e );

         float scaleInc = 1.1f;

         if( e.Delta < 0f )
            scaleInc = 1f / scaleInc;

         Vector2 currLoc = this.plotRenderer.PointToWorld( e.Location );

         this.plotRenderer.PlotScale = Vector2.Scale( this.plotRenderer.PlotScale, scaleInc );

         Vector2 adjLoc = this.plotRenderer.PointToWorld( e.Location );

         Vector2 panOffset = currLoc - adjLoc;
         //Vector2 panOffset = adjLoc - currLoc;
         this.plotRenderer.PlotCenter += panOffset;

         this.Invalidate();

         // notify of plot view change
         OnPlotViewChanged();
      }

      Point startPt;
      Point lastPt;
      bool cursorMoved;

      protected override void OnMouseDown( MouseEventArgs e )
      {
         base.OnMouseDown( e );

         if( e.Clicks == 2 && e.Button == MouseButtons.Middle )
         {
            this.BestFitView();
            this.Invalidate();
         }

         if( e.Button == MouseButtons.Right || ( e.Button == MouseButtons.Left && ( ModifierKeys & Keys.Shift ) > 0 ) )
         {
            PointF worldPt = this.plotRenderer.PointToWorld( e.Location );

            // TODO: augment renderer point to world method with camera projection...

            this.plotData.Points.Add( new Vector3( worldPt.X, worldPt.Y, 0f ) );

            //this.plotRenderer.AutoScaleForPoints();

            // set camera changed to update transformed plot data
            this.cameraChanged = true;
            this.Invalidate();
            OnPlotViewChanged();
         }

         this.startPt = e.Location;
         this.lastPt = e.Location;
      }

      protected override void OnMouseMove( MouseEventArgs e )
      {
         base.OnMouseMove( e );

         if( e.Button == MouseButtons.None )
            return;

         Vector2 overallOffset = new Vector2( e.X - this.startPt.X, e.Y - this.startPt.Y );
         if( overallOffset.Length > 1f )
            this.cursorMoved = true;

         Vector2 moveOffset = new Vector2( e.X - this.lastPt.X, e.Y - this.lastPt.Y );
         if( !moveOffset.IsEmpty() )
         {
            this.lastPt = e.Location;

            bool viewChanged = false;
            if( ( ModifierKeys & Keys.Alt ) != 0 && e.Button == MouseButtons.Left )
            {
               float rotAngle = (float)( Math.PI / this.ClientSize.Width * 2 );
               this.camera.Rotate( moveOffset.X * rotAngle, moveOffset.Y * rotAngle );
               this.cameraChanged = true;
               viewChanged = true;
            }
            else if( e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle )
            {
               moveOffset.X /= -this.plotRenderer.PlotScale.X;
               moveOffset.Y /= this.plotRenderer.PlotScale.Y;

               this.plotRenderer.PlotCenter += moveOffset;
               viewChanged = true;
            }

            if( viewChanged )
            {
               this.Invalidate();

               // notify of plot view change
               OnPlotViewChanged();
            }
         }
      }

      int HitTestNearestPoint( PointF worldPoint )
      {
         float pixelSize = 1f / Math.Min( this.plotRenderer.PlotScale.X, this.plotRenderer.PlotScale.Y );
         float minHitDist = 16f * pixelSize;
         float minHitDistSq = minHitDist * minHitDist;

         int nearestIndex = -1;
         float nearestDist = float.MaxValue;

         bool HitTestVertexSet( VertexSet vertSet, out int hitIndex )
         {
            hitIndex = -1;

            for( int i = 0; i < vertSet.Vertices.Count; ++i )
            {
               Vector3 vert3D = vertSet.Vertices[i];
               Vector2 vert2D = new Vector2( vert3D.X, vert3D.Y );

               float currDist = Vector2.DistanceSq( worldPoint, vert2D );
               if( currDist < nearestDist && currDist <= minHitDistSq )
               {
                  hitIndex = i;
                  nearestDist = currDist;
               }
            }

            return hitIndex != -1;
         }

         if( HitTestVertexSet( this.plotData.DefaultPlotSet, out nearestIndex ) )
         {
            this.plotData.SelectedSet = this.plotData.DefaultPlotSet;
         }
         else
         {
            for( int i = 0; i < this.plotData.VertexSets.Count; ++i )
            {
               if( HitTestVertexSet( this.plotData.VertexSets[i], out nearestIndex ) )
               {
                  this.plotData.SelectedSet = this.plotData.VertexSets[i];
                  break;
               }
            }
         }

         return nearestIndex;
      }

      protected override void OnMouseUp( MouseEventArgs e )
      {
         base.OnMouseUp( e );

         if( e.Button == MouseButtons.Left )
         {
            // check for hit-test of points
            if( !this.cursorMoved )
            {
               // get world coordinate of current mouse position
               PointF worldPoint = this.plotRenderer.PointToWorld( e.Location );
               // for fun, test round-trip conversion -- yeah, same -- looks good
               PointF screenPoint = this.plotRenderer.PointToScreen( worldPoint );

               VertexSet prevSet = this.plotData.SelectedSet;
               // hit test points for nearest -- within a minimum tolerance
               int hitPointIndex = HitTestNearestPoint( worldPoint );

               // check if selected index changed
               if( hitPointIndex != this.plotData.SelectedIndex || prevSet != this.plotData.SelectedSet )
               {
                  // set selected point on plotter
                  this.plotData.SelectedIndex = hitPointIndex;

                  // notify of plot view change
                  OnPlotViewChanged();

                  // redraw view for selected point
                  this.Invalidate();
               }
            }
            // reset cursor moved state
            this.cursorMoved = false;
         }
      }

      protected override void OnKeyDown( KeyEventArgs e )
      {
         if( e.Alt )
         {
            float rotAngle = (float)Math.PI / 24f;

            // rotate camera with arrow keys
            switch( e.KeyCode )
            {
               case Keys.Up:
               {
                  this.camera.Rotate( 0f, rotAngle );
                  this.cameraChanged = true;
                  this.Invalidate();
                  OnPlotViewChanged();
                  break;
               }
               case Keys.Down:
               {
                  this.camera.Rotate( 0f, -rotAngle );
                  this.cameraChanged = true;
                  this.Invalidate();
                  OnPlotViewChanged();
                  break;
               }
               case Keys.Left:
               {
                  this.camera.Rotate( -rotAngle, 0f );
                  this.cameraChanged = true;
                  this.Invalidate();
                  OnPlotViewChanged();
                  break;
               }
               case Keys.Right:
               {
                  this.camera.Rotate( rotAngle, 0f );
                  this.cameraChanged = true;
                  this.Invalidate();
                  OnPlotViewChanged();
                  break;
               }
            }
         }

         base.OnKeyDown( e );
      }

      protected override void OnKeyUp( KeyEventArgs e )
      {
         switch( e.KeyCode )
         {
            case Keys.Back:
            {
               this.plotData.DeleteSelectedPoint();
               this.Invalidate();
               OnPlotViewChanged();
               break;
            }
            case Keys.Left:
            {
               // select previous point in list
               this.plotData.AdvanceSelectedPoint( -1 );
               this.Invalidate();
               OnPlotViewChanged();
               break;
            }
            case Keys.Right:
            {
               // select next point in list
               this.plotData.AdvanceSelectedPoint( 1 );
               this.Invalidate();
               OnPlotViewChanged();
               break;
            }
         }

         base.OnKeyUp( e );
      }

      #endregion Input Event Overrides
   }
}