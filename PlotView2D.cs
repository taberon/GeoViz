using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using MetalHelix.Geometry;

namespace GeometryVisualizer
{
   public delegate void PlotViewChangedDelegate();

   public class PlotView2D : UserControl
   {
      PlotterGDI plotter;
      public PlotterGDI Plotter
      {
         get { return this.plotter; }
      }

      public List<Vector3> Points
      {
         get { return this.plotter.Points; }
      }

      public List<int> Lines
      {
         get { return this.plotter.Lines; }
      }

      public List<VertexSet> VertexSets
      {
         get { return this.plotter.VertexSets; }
      }

      bool enableFreeNavigation;
      /// <summary> Gets or sets whether free mouse navigation (panning and zooming) is enabled. </summary>
      public bool EnableFreeNavigation { get { return this.enableFreeNavigation; } set { this.enableFreeNavigation = value; } }

      /// <summary> Event fired whenever the plot scale or view center is changed. </summary>
      public event PlotViewChangedDelegate PlotViewChanged;

      public PlotView2D()
      {
         // create geometry visualizer
         this.plotter = new PlotterGDI();
         // set initial view bounds
         this.plotter.ViewSize = this.ClientSize;

         //this.DoubleBuffered = true;
         this.SetStyle( ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true );
      }

      public void BestFitView()
      {
         this.plotter.AutoScaleForPoints();

         // notify of plot view change
         OnPlotViewChanged();
      }

      protected virtual void OnPlotViewChanged()
      {
         this.PlotViewChanged?.Invoke();
      }

      #region Drawing Overrides

      protected override void OnPaint( PaintEventArgs e )
      {
         base.OnPaint( e );

         //e.Graphics.DrawRectangle( Pens.Blue, 0, 0, this.Width - 1, this.Height - 1 );

         e.Graphics.Clear( this.BackColor );

         // draw geometry visualization
         this.plotter.Draw( e.Graphics );
      }

      protected override void OnResize( EventArgs e )
      {
         base.OnResize( e );

         this.plotter.ViewSize = this.ClientSize;

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

         Vector2 currLoc = this.plotter.PointToWorld( e.Location );

         this.plotter.PlotScale = Vector2.Scale( this.plotter.PlotScale, scaleInc );

         Vector2 adjLoc = this.plotter.PointToWorld( e.Location );

         Vector2 panOffset = currLoc - adjLoc;
         //Vector2 panOffset = adjLoc - currLoc;
         this.plotter.PlotCenter += panOffset;

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
            PointF worldPt = this.plotter.PointToWorld( e.Location );

            this.plotter.Points.Add( new Vector3( worldPt.X, worldPt.Y, 0f ) );

            //this.plotter.AutoScaleForPoints();

            this.Invalidate();
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

            moveOffset.X /= -this.plotter.PlotScale.X;
            moveOffset.Y /= this.plotter.PlotScale.Y;

            bool viewChanged = false;
            if( e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle )
            {
               this.plotter.PlotCenter += moveOffset;
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
         float pixelSize = 1f / Math.Min( this.plotter.PlotScale.X, this.plotter.PlotScale.Y );
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

         if( HitTestVertexSet( this.plotter.DefaultPlotSet, out nearestIndex ) )
         {
            this.plotter.SelectedSet = this.plotter.DefaultPlotSet;
         }
         else
         {
            for( int i = 0; i < this.plotter.VertexSets.Count; ++i )
            {
               if( HitTestVertexSet( this.plotter.VertexSets[i], out nearestIndex ) )
               {
                  this.plotter.SelectedSet = this.plotter.VertexSets[i];
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
               PointF worldPoint = this.plotter.PointToWorld( e.Location );
               // for fun, test round-trip conversion -- yeah, same -- looks good
               PointF screenPoint = this.plotter.PointToScreen( worldPoint );

               VertexSet prevSet = this.plotter.SelectedSet;
               // hit test points for nearest -- within a minimum tolerance
               int hitPointIndex = HitTestNearestPoint( worldPoint );

               // check if selected index changed
               if( hitPointIndex != this.plotter.SelectedIndex || prevSet != this.plotter.SelectedSet )
               {
                  // set selected point on plotter
                  this.plotter.SelectedIndex = hitPointIndex;

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

      public void DeleteSelectedPoint()
      {
         if( this.plotter.SelectedSet != null && this.plotter.SelectedIndex >= 0 && this.plotter.SelectedIndex < this.plotter.SelectedSet.Count )
         {
            // remove point from list
            this.plotter.SelectedSet.Vertices.RemoveAt( this.plotter.SelectedIndex );
            // set selected point as previous index
            AdvanceSelectedPoint( -1 );
         }
      }

      public void ClearAll()
      {
         // clear all vertex sets
         this.plotter.Points.Clear();
         this.plotter.Lines.Clear();
         this.plotter.VertexSets.Clear();
         this.plotter.SelectedIndex = -1;
         this.plotter.SelectedSet = null;
      }

      public void AdvanceSelectedPoint( int dir = 1 )
      {
         if( this.plotter.SelectedSet == null )
         {
            this.plotter.SelectedSet = this.plotter.DefaultPlotSet;
         }

         // get current selected point index
         int currIndex = this.plotter.SelectedIndex;
         // advance by specified direction/amount
         currIndex += dir;

         // bounds check
         if( currIndex >= this.plotter.SelectedSet.Count )
            currIndex = 0;
         else if( currIndex < 0 )
            currIndex = this.plotter.SelectedSet.Count - 1;

         // check for empty point collection
         if( this.plotter.SelectedSet.Count == 0 )
            currIndex = -1;

         // set selected point
         this.plotter.SelectedIndex = currIndex;

         // notify of plot view change
         OnPlotViewChanged();

         // request redraw of plot view
         this.Invalidate();
      }

      protected override void OnKeyUp( KeyEventArgs e )
      {
         switch( e.KeyCode )
         {
            case Keys.Back:
            {
               DeleteSelectedPoint();
               break;
            }
            case Keys.Left:
            {
               // select previous point in list
               AdvanceSelectedPoint( -1 );
               break;
            }
            case Keys.Right:
            {
               // select next point in list
               AdvanceSelectedPoint( 1 );
               break;
            }
         }

         base.OnKeyUp( e );
      }

      #endregion Input Event Overrides
   }
}