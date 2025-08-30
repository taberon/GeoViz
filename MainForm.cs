using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using MetalHelix.Geometry;

namespace GeometryVisualizer
{
   public class MainForm : Form
   {
      const string AppName = "Geometry Visualizer v0.68";
      const string AppDate = "25 August 2025";

      PlotView2D plotView;
      StatusStrip statusBar;
      ToolStripLabel statusBounds;
      ToolStripLabel statusSize;
      ToolStripLabel statusSelectedPoint;
      ToolStripLabel statusMouseCoords;

      PointEditForm pointEditor;

      MenuItem menuItem_BestFit;
      MenuItem menuItem_Axis;
      MenuItem menuItem_Grid;
      MenuItem menuItem_MaintainAspect;
      MenuItem menuItem_ViewTransparent;
      MenuItem menuItem_PointsEdit;
      MenuItem menuItem_PointsPaste;
      MenuItem menuItem_PointsClear;
      MenuItem menuItem_PointDelete;

      MenuItem menuItem_About;

      public MainForm()
      {
         // create plot view control
         this.plotView = new PlotView2D();
         this.plotView.Dock = DockStyle.Fill;
         this.plotView.Parent = this;

         // update title text
         this.Text = AppName;

         // create point editor dialog
         this.pointEditor = new PointEditForm();
         // set owner as mainform -- so it hides/shows with the parent
         this.pointEditor.Owner = this;
         // hookup point editor event
         this.pointEditor.PointsChanged += PointsChangedHandler;


         //this.Text = AppName;
         this.ClientSize = new Size( 500, 400 ); // will trigger OnResize to set ViewSize

         this.statusBar = new StatusStrip();
         this.statusBar.Parent = this;

         this.statusBounds = new ToolStripLabel();
         this.statusBounds.Alignment = ToolStripItemAlignment.Left;
         this.statusBar.Items.Add( this.statusBounds );
         this.statusBounds.Text = "Taber was here!";

         this.statusBar.Items.Add( new ToolStripSeparator() );

         this.statusSize = new ToolStripLabel();
         this.statusSize.Alignment = ToolStripItemAlignment.Left;
         this.statusBar.Items.Add( this.statusSize );
         this.statusSize.Text = "20.5x16.4";

         this.statusBar.Items.Add( new ToolStripSeparator() );

         this.statusSelectedPoint = new ToolStripLabel();
         this.statusSelectedPoint.Alignment = ToolStripItemAlignment.Left;
         this.statusBar.Items.Add( this.statusSelectedPoint );
         this.statusSelectedPoint.Text = "Selected: {X: 0, Y: 0}";

         this.statusBar.Items.Add( new ToolStripSeparator() );

         this.statusMouseCoords = new ToolStripLabel();
         this.statusMouseCoords.Alignment = ToolStripItemAlignment.Right;
         this.statusBar.Items.Add( this.statusMouseCoords );
         this.statusBounds.Text = "{X: 0, Y: 0}";


         // handle mouse move for plot view control to update status bar
         this.plotView.MouseMove += PlotView_MouseMove;

         UpdateStatus();

         InitializeMenu();
      }

      private void PlotView_MouseMove( object sender, MouseEventArgs e )
      {
         UpdateStatus();
      }

      protected override void OnLoad( EventArgs e )
      {
         // load default point data
         this.plotView.Points.Add( new Vector3( .3f, .1f, 0f ) );
         this.plotView.Points.Add( new Vector3( .8f, .3f, 0f ) );
         this.plotView.Points.Add( new Vector3( -.2f, .4f, 0f ) );
         this.plotView.Points.Add( new Vector3( -.4f, .2f, 0f ) );
         this.plotView.Points.Add( new Vector3( -.8f, -.4f, 0f ) );
         this.plotView.Plotter.DefaultPlotSet.AutoSet2D();
         //*/

         /*/ add random points
         int pointCount = 100;
         Random r = new Random( 0 );
         for( int i = 0; i < pointCount; ++i )
            this.plotView.Points.Add( new Vector3( r.Next( -10, 10 ), r.Next( -10, 10 ), r.Next(-1, 1)) );
         //*/

         // set default best-fit view for data
         this.plotView.BestFitView();

         // subscribe to plot view changed event
         this.plotView.PlotViewChanged += PlotView_PlotViewChanged;

         // update status text
         UpdateStatus();

         base.OnLoad( e );
      }

      #region Main Menu

      private void InitializeMenu()
      {
         this.Menu = new MainMenu();

         MenuItem fileMenu = new MenuItem( "File" );
         this.Menu.MenuItems.Add( fileMenu );

         MenuItem editMenu = new MenuItem( "Edit" );
         this.Menu.MenuItems.Add( editMenu );

         this.menuItem_PointsEdit = new MenuItem( "Edit Points", MenuItemClick, Shortcut.CtrlE );
         editMenu.MenuItems.Add( this.menuItem_PointsEdit );

         this.menuItem_PointsPaste = new MenuItem( "Paste Points", MenuItemClick, Shortcut.CtrlV );
         editMenu.MenuItems.Add( this.menuItem_PointsPaste );

         this.menuItem_PointsClear = new MenuItem( "Clear Points", MenuItemClick, Shortcut.CtrlW );
         editMenu.MenuItems.Add( this.menuItem_PointsClear );

         this.menuItem_PointDelete = new MenuItem( "Delete Point", MenuItemClick, Shortcut.Del );
         editMenu.MenuItems.Add( this.menuItem_PointDelete );

         MenuItem viewMenu = new MenuItem( "View" );
         this.Menu.MenuItems.Add( viewMenu );

         this.menuItem_BestFit = new MenuItem( "Best Fit", MenuItemClick, Shortcut.F7 );
         viewMenu.MenuItems.Add( this.menuItem_BestFit );
         viewMenu.MenuItems.Add( "-" );

         this.menuItem_Axis = viewMenu.MenuItems.Add( "Axis", MenuItemClick );
         this.menuItem_Axis.Checked = true; // set default true

         this.menuItem_Grid = viewMenu.MenuItems.Add( "Grid", MenuItemClick );
         this.menuItem_Grid.Checked = true; // set default true

         this.menuItem_MaintainAspect = viewMenu.MenuItems.Add( "Maintain Aspect", MenuItemClick );
         this.menuItem_MaintainAspect.Checked = true; // set default true


         viewMenu.MenuItems.Add( "-" );
         this.menuItem_ViewTransparent = new MenuItem( "Transparent (Overlay)", MenuItemClick, Shortcut.CtrlT );
         viewMenu.MenuItems.Add( this.menuItem_ViewTransparent );

         MenuItem helpMenu = new MenuItem( "Help" );
         this.Menu.MenuItems.Add( helpMenu );

         this.menuItem_About = helpMenu.MenuItems.Add( "About", MenuItemClick );
      }

      void MenuItemClick( object sender, EventArgs e )
      {
         bool redraw = false;

         MenuItem clickItem = (MenuItem)sender;

         if( clickItem == this.menuItem_BestFit )
         {
            this.plotView.BestFitView();
            redraw = true;
         }
         else if( clickItem == this.menuItem_Axis )
         {
            clickItem.Checked = !clickItem.Checked;
            this.plotView.Plotter.DrawAxis = clickItem.Checked;
            redraw = true;
         }
         else if( clickItem == this.menuItem_Grid )
         {
            clickItem.Checked = !clickItem.Checked;
            this.plotView.Plotter.DrawGrid = clickItem.Checked;
            redraw = true;
         }
         else if( clickItem == this.menuItem_MaintainAspect )
         {
            clickItem.Checked = !clickItem.Checked;
            this.plotView.Plotter.MaintainAspect = clickItem.Checked;
            redraw = true;
         }
         else if( clickItem == this.menuItem_ViewTransparent )
         {
            bool enabled = !this.menuItem_ViewTransparent.Checked;
            this.menuItem_ViewTransparent.Checked = enabled;
            this.Opacity = enabled ? 0.50f : 1f;
         }
         else if( clickItem == this.menuItem_PointsEdit )
         {
            // update points in editor window
            //if( this.plotView.Points.Count > 0 )
            //this.pointEditor.Points = this.plotView.Points;
            List<VertexSet> newSets = new List<VertexSet>();
            newSets.Add( this.plotView.Plotter.DefaultPlotSet );
            newSets.AddRange( this.plotView.VertexSets );
            this.pointEditor.VertexSets = newSets;
            this.pointEditor.Show();
         }
         else if( clickItem == this.menuItem_PointsPaste )
         {
            // update points in editor window
            if( Clipboard.ContainsText() )
            {
               string clipText = Clipboard.GetText();
               this.pointEditor.SetPointText( clipText );
               this.plotView.Invalidate();
            }
         }
         else if( clickItem == this.menuItem_PointsClear )
         {
            // update points in editor window
            this.plotView.ClearAll();
            redraw = true;
         }
         else if( clickItem == this.menuItem_PointDelete )
         {
            // request to deleted selected point
            this.plotView.DeleteSelectedPoint();
         }
         else if( clickItem == this.menuItem_About )
         {
            MessageBox.Show( string.Join( Environment.NewLine, AppName, AppDate, "Created by Taber", "with a computer" ), AppName );
         }

         if( redraw )
            this.plotView.Invalidate();
      }

      #endregion Main Menu

      #region Event Handlers

      void PlotView_PlotViewChanged()
      {
         // update status text
         UpdateStatus();
      }

      void PointsChangedHandler( object sender, EventArgs e )
      {
         // update vizualizer with modified points
         //this.plotView.Points.Clear();
         //this.plotView.Points.AddRange( this.pointEditor.Points );
         //this.plotView.Plotter.Lines.Clear();
         //this.plotView.Plotter.Lines.AddRange( this.pointEditor.Lines );

         this.plotView.Plotter.Points.Clear();
         this.plotView.Plotter.Lines.Clear();
         this.plotView.VertexSets.Clear();
         this.plotView.VertexSets.AddRange( this.pointEditor.VertexSets );

         // update scale for new points
         this.plotView.BestFitView();

         // request redraw of plot view
         this.plotView.Invalidate();

         // Note: change to plotView will trigger PlotViewChange, and so automatically refersh status info

         // update status text
         //UpdateStatus();
      }

      void UpdateStatus()
      {
         PlotterGDI plotter = this.plotView.Plotter;

         // update the status Bounds
         this.statusBounds.Text = string.Format( "X: [{0:G3}, {1:G3}] - Y: [{2:G3}, {3:G3}]", plotter.PlotMinX, plotter.PlotMaxX, plotter.PlotMinY, plotter.PlotMaxY );

         // update status Size
         this.statusSize.Text = string.Format( "{0:G3}x{1:G3}", plotter.PlotSize.Width, plotter.PlotSize.Height );

         // set selected point -- if any
         string selectedPointString = "[-]: ---";
         if( this.plotView.Plotter.SelectedIndex != -1 && this.plotView.Plotter.SelectedSet != null )
         {
            int selectedIndex = this.plotView.Plotter.SelectedIndex;
            VertexSet selectedSet = this.plotView.Plotter.SelectedSet;
            if( selectedIndex < selectedSet.Vertices.Count )
            {
               Vector3 selectedVert = selectedSet.Vertices[selectedIndex];
               if( selectedSet.Is2D )
                  selectedPointString = $"[{selectedIndex}]: ( {selectedVert.X:G3}, {selectedVert.Y:G3} )";
               else
                  selectedPointString = $"[{selectedIndex}]: ( {selectedVert.X:G3}, {selectedVert.Y:G3}, {selectedVert.Z:G3} )";
            }
         }
         this.statusSelectedPoint.Text = selectedPointString;

         // get plot coords for current mouse pos
         Point mouseScreenPos = PointToClient( MousePosition );
         PointF plotPos = plotter.PointToWorld( mouseScreenPos );

         // update mouse info
         this.statusMouseCoords.Text = string.Format( "X: {0:G3}, Y: {1:G3}", plotPos.X, plotPos.Y );
      }

      #endregion Event Handlers

      [STAThread]
      public static void Main()
      {
         Application.EnableVisualStyles();
         Application.Run( new MainForm() );
      }
   }
}