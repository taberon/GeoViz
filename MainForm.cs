using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using MetalHelix.Geometry;

namespace GeometryVisualizer
{
   public class MainForm : Form
   {
      const string AppName = "Geometry Visualizer v0.70";
      const string AppDate = "21 June 2024";

      //PlotView2D plotView;
      Control plotControl;
      IPlotView PlotView { get { return (IPlotView)this.plotControl; } }

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
      MenuItem menuItem_View2D;
      MenuItem menuItem_View3D;

      MenuItem menuItem_PointsEdit;
      MenuItem menuItem_PointsPaste;
      MenuItem menuItem_PointsClear;
      MenuItem menuItem_PointDelete;

      MenuItem menuItem_About;

      public MainForm()
      {
         // create and set default plot view control
         SetPlotView( new PlotView2D() );

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

         UpdateStatus();

         InitializeMenu();
      }

      void SetPlotView( IPlotView plotViewControl )
      {
         IPlotView prevPlotView = null;
         if( this.plotControl != null )
         {
            this.plotControl.Parent = null;
            prevPlotView = this.PlotView;
            this.plotControl.MouseMove -= PlotView_MouseMove;
         }

         this.plotControl = (Control)plotViewControl;
         this.plotControl.Dock = DockStyle.Fill;
         this.plotControl.Parent = this;

         if( prevPlotView != null )
         {
            // TODO: restore plot view data and options..?
            //this.PlotView.PlotData = prevPlotView.PlotData;
            //this.PlotView.PlotOptions = prevPlotView.PlotOptions;
         }

         // handle mouse move for plot view control to update status bar
         this.plotControl.MouseMove += PlotView_MouseMove;
      }

      private void PlotView_MouseMove( object sender, MouseEventArgs e )
      {
         UpdateStatus();
      }

      protected override void OnLoad( EventArgs e )
      {
         /*/ load default point data
         this.PlotView.PlotData.Points.Add( new Vector3( .3f, .1f, 0f ) );
         this.PlotView.PlotData.Points.Add( new Vector3( .8f, .3f, 0f ) );
         this.PlotView.PlotData.Points.Add( new Vector3( -.2f, .4f, 0f ) );
         this.PlotView.PlotData.Points.Add( new Vector3( -.4f, .2f, 0f ) );
         this.PlotView.PlotData.Points.Add( new Vector3( -.8f, -.4f, 0f ) );
         this.PlotView.PlotData.DefaultPlotSet.AutoSet2D();
         //*/

         /*/ add random points
         int pointCount = 100;
         Random r = new Random( 0 );
         for( int i = 0; i < pointCount; ++i )
            this.plotView.Points.Add( new Vector3( r.Next( -10, 10 ), r.Next( -10, 10 ), r.Next(-1, 1)) );
         //*/

         // set default best-fit view for data
         this.PlotView.BestFitView();

         // subscribe to plot view changed event
         this.PlotView.PlotViewChanged += PlotView_PlotViewChanged;

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
         this.menuItem_View2D = new MenuItem( "2D", MenuItemClick, Shortcut.Ctrl2 );
         viewMenu.MenuItems.Add( this.menuItem_View2D );
         this.menuItem_View2D.Checked = true;
         this.menuItem_View3D = new MenuItem( "3D", MenuItemClick, Shortcut.Ctrl3 );
         viewMenu.MenuItems.Add( this.menuItem_View3D );

         MenuItem helpMenu = new MenuItem( "Help" );
         this.Menu.MenuItems.Add( helpMenu );

         this.menuItem_About = helpMenu.MenuItems.Add( "About", MenuItemClick );
      }

      void MenuItemClick( object sender, EventArgs e )
      {
         bool redraw = false;
         bool bestFit = false;

         MenuItem clickItem = (MenuItem)sender;

         if( clickItem == this.menuItem_BestFit )
         {
            bestFit = true;
            redraw = true;
         }
         else if( clickItem == this.menuItem_Axis )
         {
            clickItem.Checked = !clickItem.Checked;
            this.PlotView.PlotOptions.DrawAxis = clickItem.Checked;
            redraw = true;
         }
         else if( clickItem == this.menuItem_Grid )
         {
            clickItem.Checked = !clickItem.Checked;
            this.PlotView.PlotOptions.DrawGrid = clickItem.Checked;
            redraw = true;
         }
         else if( clickItem == this.menuItem_MaintainAspect )
         {
            clickItem.Checked = !clickItem.Checked;
            this.PlotView.PlotOptions.MaintainAspect = clickItem.Checked;
            redraw = true;
         }
         else if( clickItem == this.menuItem_View2D && !clickItem.Checked )
         {
            this.menuItem_View2D.Checked = true;
            this.menuItem_View3D.Checked = false;
            SetPlotView( new PlotView2D() );
            bestFit = true;
            redraw = true;
         }
         else if( clickItem == this.menuItem_View3D && !clickItem.Checked )
         {
            this.menuItem_View2D.Checked = false;
            this.menuItem_View3D.Checked = true;
            SetPlotView( new PlotView3D() );
            bestFit = true;
            redraw = true;
         }
         else if( clickItem == this.menuItem_PointsEdit )
         {
            // update points in editor window
            //if( this.plotView.Points.Count > 0 )
            //this.pointEditor.Points = this.plotView.Points;
            List<VertexSet> newSets = new List<VertexSet>();
            newSets.Add( this.PlotView.PlotData.DefaultPlotSet );
            newSets.AddRange( this.PlotView.PlotData.VertexSets );
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
               this.plotControl.Invalidate();
            }
         }
         else if( clickItem == this.menuItem_PointsClear )
         {
            // update points in editor window
            this.PlotView.PlotData.ClearAll();

            // notify of plot view change
            //this.PlotView.OnPlotViewChanged();

            // request redraw of plot view
            //this.Invalidate();
            this.plotControl.Invalidate();

            redraw = true;
         }
         else if( clickItem == this.menuItem_PointDelete )
         {
            // request to deleted selected point
            this.PlotView.PlotData.DeleteSelectedPoint();
            this.plotControl.Invalidate();
            UpdateStatus();
         }
         else if( clickItem == this.menuItem_About )
         {
            MessageBox.Show( string.Join( Environment.NewLine, AppName, AppDate, "Created by Taber", "with a computer" ), AppName );
         }

         if( bestFit )
            this.PlotView.BestFitView();

         if( redraw )
            this.plotControl.Invalidate();
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

         this.PlotView.PlotData.Points.Clear();
         this.PlotView.PlotData.Lines.Clear();
         this.PlotView.PlotData.DefaultPlotFace.Indices.Clear();
         this.PlotView.PlotData.DefaultPlotSet.Faces.Clear();
         this.PlotView.PlotData.VertexSets.Clear();
         this.PlotView.PlotData.VertexSets.AddRange( this.pointEditor.VertexSets );

         // update scale for new points
         this.PlotView.BestFitView();

         // request redraw of plot view
         this.plotControl.Invalidate();

         // Note: change to plotView will trigger PlotViewChange, and so automatically refersh status info

         // update status text
         //UpdateStatus();
      }

      void UpdateStatus()
      {
         PlotRenderer plotter = this.PlotView.PlotRenderer;

         // update the status Bounds
         this.statusBounds.Text = this.PlotView.GetPlotStatusBounds();

         // update status Size
         this.statusSize.Text = this.PlotView.GetPlotStatusSize();

         // set selected point -- if any
         string selectedPointString = "[-]: ---";
         if( this.PlotView.PlotData.SelectedIndex != -1 && this.PlotView.PlotData.SelectedSet != null )
         {
            int selectedIndex = this.PlotView.PlotData.SelectedIndex;
            VertexSet selectedSet = this.PlotView.PlotData.SelectedSet;
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