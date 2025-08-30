using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GeometryVisualizer
{
   #region Point Edit Form

   public class PointEditForm : Form
   {
      #region Fields and Properties

      RichTextBox textArea;

      Button buttonOK;
      Button buttonApply;
      Button buttonCancel;

      public event EventHandler PointsChanged;

      List<VertexSet> vertexSets = new List<VertexSet>();
      public List<VertexSet> VertexSets
      {
         get { return this.vertexSets; }
         set
         {
            // copy given point collection
            this.vertexSets.Clear();
            this.vertexSets.AddRange( value );
            // update text area with current points
            WritePoints();
         }
      }

      //List<PointF> points = new List<PointF>();
      //public List<PointF> Points
      //{
      //   get
      //   {
      //      // return current point collection
      //      return this.points;
      //      // points are only updated from text when Apply or OK are clicked
      //   }
      //   set
      //   {
      //      // copy given point collection
      //      this.points.Clear();
      //      this.points.AddRange( value );
      //      // update text area with current points
      //      WritePoints();
      //   }
      //}

      //List<int> lines = new List<int>();
      //public List<int> Lines
      //{
      //   get
      //   {
      //      return this.lines;
      //   }
      //   //set
      //   //{
      //   //   // copy given point collection
      //   //   this.lines.Clear();
      //   //   this.lines.AddRange( value );
      //   //   // TODO: how to update text formatting for include line/polygon scopes...
      //   //}
      //}

      float defaultZoom = 1.5f;

      #endregion Fields and Properties

      #region Construction

      public PointEditForm()
      {
         this.Text = "Geometry Visualizer: Points";
         //this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
         this.FormBorderStyle = FormBorderStyle.Sizable;
         this.SizeGripStyle = SizeGripStyle.Show;
         this.ShowInTaskbar = false;
         this.KeyPreview = true;

         int buttonHeight = 22;
         int buttonHeightTop = 24;
         int buttonWidth = 68;

         int buttonSpace = 8;

         // set minimum form size
         int borderWidth = SystemInformation.FrameBorderSize.Width;
         int maxFormWidth = buttonWidth * 3 + buttonSpace * 8 + borderWidth * 2;
         this.MinimumSize = new Size( maxFormWidth, maxFormWidth );

         //this.textArea = new TextBox();
         this.textArea = new RichTextBox();

         float currFontSize = this.Font.Size;
         //this.textArea.Font = new Font( this.Font.FontFamily, 10f );
         //this.textArea.Font = new Font( FontFamily.GenericMonospace, 10f );
         //this.textArea.Font = new Font( "Consolas", 10f );

         this.textArea.ZoomFactor = this.defaultZoom;
         this.textArea.AutoWordSelection = true;
         this.textArea.Multiline = true;
         this.textArea.AcceptsTab = true;
         //this.textArea.ScrollBars = ScrollBars.Vertical;
         this.textArea.ScrollBars = RichTextBoxScrollBars.Vertical;
         this.textArea.Location = new Point( buttonSpace / 2, buttonSpace / 2 );
         this.textArea.Width = this.ClientSize.Width - buttonSpace;
         this.textArea.Height = this.ClientSize.Height - buttonHeight - buttonSpace;
         this.textArea.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
         this.Controls.Add( this.textArea );

         this.buttonApply = new Button();
         this.buttonApply.Size = new Size( buttonWidth, buttonHeight );
         this.buttonApply.Text = "Apply";
         this.buttonApply.Top = this.ClientRectangle.Bottom - buttonHeightTop;
         this.buttonApply.Left = buttonSpace * 2;
         this.buttonApply.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
         this.buttonApply.Click += ButtonClickHandler;
         this.Controls.Add( this.buttonApply );

         this.buttonCancel = new Button();
         this.buttonCancel.Size = new Size( buttonWidth, buttonHeight );
         this.buttonCancel.Text = "Cancel";
         this.buttonCancel.Top = this.ClientRectangle.Bottom - buttonHeightTop;
         this.buttonCancel.Left = this.ClientRectangle.Right - buttonWidth - buttonSpace * 2;
         this.buttonCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
         this.buttonCancel.Click += ButtonClickHandler;
         this.Controls.Add( this.buttonCancel );

         this.buttonOK = new Button();
         this.buttonOK.Size = new Size( buttonWidth, buttonHeight );
         this.buttonOK.Text = "OK";
         this.buttonOK.Top = this.ClientRectangle.Bottom - buttonHeightTop;
         this.buttonOK.Left = this.buttonCancel.Left - buttonWidth - buttonSpace;
         this.buttonOK.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
         this.buttonOK.Click += ButtonClickHandler;
         this.Controls.Add( this.buttonOK );
      }

      #endregion Construction

      #region Overrides

      protected override void OnShown( EventArgs e )
      {
         // zoom factor is reset after hiding..?
         //this.textArea.ZoomFactor = 1.8f;
         // ...but this is only called the very first Show()...

         // dock to parent form and size to match
         if( this.Owner != null )
         {
            DockParentWindow( this.Owner );
            this.StartPosition = FormStartPosition.Manual;
         }

         base.OnShown( e );
      }

      void DockParentWindow( Form parentWin )
      {
         Point pos = this.Owner.Location;
         Size size = this.Owner.Size;

         // check if room on current screen to dock to left or right
         Screen currScreen = Screen.FromControl( parentWin );
         if( pos.X - this.Width > currScreen.Bounds.Left )
         {
            // dock to left
            this.Location = new Point( pos.X - this.Width, pos.Y );
         }
         else if( pos.X + size.Width + this.Width < currScreen.Bounds.Right )
         {
            // dock to right
            this.Location = new Point( pos.X + size.Width, pos.Y );
         }
         else
         {
            // dock in center of main form
            int sizeDiff = ( size.Width - this.Width );
            this.Location = new Point( pos.X + sizeDiff / 2, pos.Y );
         }

         this.Height = size.Height;
         //this.Width = size.Width / 3;
      }

      protected override void OnVisibleChanged( EventArgs e )
      {
         if( this.Visible && this.Owner != null )
         {
            DockParentWindow( this.Owner );
         }

         base.OnVisibleChanged( e );
      }

      protected override void OnClosing( CancelEventArgs e )
      {
         // don't allow closing of form -- only hide
         e.Cancel = true;

         // hide form to be re-show again later
         this.Hide();

         base.OnClosing( e );
      }

      protected override void OnKeyPress( KeyPressEventArgs e )
      {
         switch( e.KeyChar )
         {
            case '\t':
               if( this.textArea.SelectionLength > 0 )
                  e.Handled = true;
               break;
         }

         base.OnKeyPress( e );
      }

      protected override void OnKeyDown( KeyEventArgs e )
      {
         float zoomScaleInc = 1.1f;

         int currIndex = this.textArea.SelectionStart;
         int selectionLen = this.textArea.SelectionLength;
         int lineNum = this.textArea.GetLineFromCharIndex( currIndex );
         int textLen = this.textArea.TextLength;
         int lineCount = this.textArea.Lines.Length - 1;

         switch( e.KeyCode )
         {
            // check for arrow keys presses at beginning/end of text area
            // to ignore key input and prevent system error sounds...
            case Keys.Left:
               if( currIndex == 0 && selectionLen == 0 )
               {
                  e.Handled = true;
               }
               break;
            case Keys.Up:
               if( lineNum == 0 && selectionLen == 0 )
               {
                  e.Handled = true;
               }
               break;
            case Keys.Right:
               if( currIndex == textLen && selectionLen == 0 )
               {
                  e.Handled = true;
               }
               break;
            case Keys.Down:
               if( lineNum == lineCount && selectionLen == 0 )
               {
                  e.Handled = true;
               }
               break;

            case Keys.Escape:
               this.Hide();
               break;
            case Keys.Oemplus:
               if( e.Control )
               {
                  this.textArea.ZoomFactor *= zoomScaleInc;
               }
               break;
            case Keys.OemMinus:
               if( e.Control )
               {
                  this.textArea.ZoomFactor /= zoomScaleInc;
               }
               break;
            case Keys.D0:
               if( e.Control )
               {
                  this.textArea.ZoomFactor = 1f;
               }
               break;

            case Keys.Enter:
               if( e.Control )
               {
                  // apply point changes
                  SetPointText( this.textArea.Text );
                  e.Handled = true;
               }
               break;

            case Keys.Tab:
            {
               if( this.textArea.SelectionLength > 0 )
               {
                  // retain current selection
                  int selectionStart = this.textArea.SelectionStart;
                  int selectionCount = this.textArea.SelectionLength;

                  //this.textArea.GetFirstCharIndexOfCurrentLine();
                  int startLine = this.textArea.GetLineFromCharIndex( this.textArea.SelectionStart );
                  int startIndex = this.textArea.GetFirstCharIndexFromLine( startLine );

                  // get selected text to be replaced
                  string selectedText = this.textArea.SelectedText;
                  
                  // TODO: define method to handle indentation adjustment for selected text/lines

                  // adjust indentation for selected lines
                  int indentAdjust = e.Shift ? -3 : 3;

                  // TODO: handle case of negative indent, where crash occurs if continuing to press Shift+Tab...

                  string[] lines = this.textArea.SelectedText.Split( '\n' );
                  // TODO: handle trailing newline better -- perhaps check if line only contains \n, then skip..?
                  //string[] lines = this.textArea.SelectedText.Split( new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries );
                  for( int i = 0; i < lines.Length; ++i )
                  {
                     int spaceCount = 0;
                     for( int j = 0; j < lines[i].Length; ++j )
                     {
                        if( lines[i][j] == ' ' )
                           ++spaceCount;
                        else
                           break;
                     }
                     spaceCount += indentAdjust;
                     if( spaceCount >= 0 )
                        lines[i] = new string( ' ', spaceCount ) + lines[i].TrimStart();
                  }

                  // set selected lines
                  selectedText = string.Join( "\n", lines );
                  this.textArea.SelectedText = selectedText;

                  // restore text selection
                  int adjSelectionCount = selectionCount + ( lines.Length * indentAdjust );
                  this.textArea.SelectionStart = selectionStart;
                  this.textArea.SelectionLength = adjSelectionCount;
               }
               // handle event so tab character isn't inserted
               e.Handled = true;
               break;
            }

            case Keys.B:
               if( e.Control )
               {
                  // test rich text formatting...
                  FontStyle currStyle = this.textArea.SelectionFont.Style;
                  Font currFont = this.textArea.SelectionFont;

                  if( currFont.Bold )
                     currStyle &= ~FontStyle.Bold;
                  else
                     currStyle |= FontStyle.Bold;

                  this.textArea.SelectionFont = new Font( currFont, currStyle );
               }
               break;
            case Keys.R:
               if( e.Control && e.Shift )
               {
                  Color currColor = this.textArea.SelectionColor;
                  if( currColor == Color.Red )
                     currColor = Color.Black;
                  else
                     currColor = Color.Red;
                  this.textArea.SelectionColor = currColor;
               }
               break;
         }

         base.OnKeyDown( e );
      }

      #endregion Overrides

      #region Update Methods

      void AddTextHighlight( int startIndex, int count, Color textColor, FontStyle textStyle )
      {
         // save current cursor pos
         int curPos = this.textArea.SelectionStart;

         // set text selection
         this.textArea.SelectionStart = startIndex;
         this.textArea.SelectionLength = count;

         // set text color
         this.textArea.SelectionColor = textColor;

         //this.textArea.SelectionBackColor

         // set text style
         //if( textStyle != FontStyle.Regular )
         {
            Font currFont = this.textArea.SelectionFont ?? this.textArea.Font;
            this.textArea.SelectionFont = new Font( currFont, textStyle );
         }

         // clear text selection
         this.textArea.SelectionStart = curPos;
         this.textArea.SelectionLength = 0;
      }

      private void ParsePointGroups( string multilineText )
      {
         // clear current vertex set collection
         this.vertexSets.Clear();

         List<VertexSet> vertexSets = VertexParser.ParseVertexSets( multilineText );

         // check for parse errors
         if( VertexParser.ParseErrors.Count > 0 )
         {
            // add error highlights to text
            for( int i = 0; i < VertexParser.ParseErrors.Count; ++i )
            {
               VertexParser.ParseErrorInfo errInfo = VertexParser.ParseErrors[i];
               AddTextHighlight( errInfo.Index, errInfo.Length, Color.Red, FontStyle.Bold );
            }
            VertexParser.ClearErrors();
         }
         else
         {
            // reset text area formatting
            AddTextHighlight( 0, this.textArea.TextLength, Color.Black, FontStyle.Regular );
         }

         // load polygon loops from parsed data
         foreach( VertexSet vertexSet in vertexSets )
         {
            // skip if empty set (should that happen..?)
            if( vertexSet.Count == 0 )
               continue;

            this.vertexSets.Add( vertexSet );

            /*/
            for( int i = 0; i < vertexSet.Count; ++i )
            {
               this.points.Add( new PointF( vertexSet[i].X, vertexSet[i].Y ) );
            }

            if( vertexSet.IsPolyline )
            {
               int baseIndex = this.points.Count - vertexSet.Count;
               // add line indices for vertex set
               for( int i = 0; i < vertexSet.Count - 1; ++i )
               {
                  this.lines.Add( baseIndex + i );
                  this.lines.Add( baseIndex + i + 1 );
               }

               // add closing line
               if( vertexSet.IsClosed )
               {
                  this.lines.Add( baseIndex + vertexSet.Count - 1 );
                  this.lines.Add( baseIndex + 0 );
               }
            }//*/
         }

         // trigger points changed event
         this.PointsChanged?.Invoke( this, new EventArgs() );
      }

      public void SetPointText( string pointsText )
      {
         /*/ old method -- break into lines, search for one 2D vertex on each line
         string[] lines = pointsText.Split( '\r', '\n' );
         ParsePointLines( lines );
         //*/

         // new parser, search for any continuous float values, separated by newlines or braces,
         // and allowing a polygon loops to be grouped by braces as well
         ParsePointGroups( pointsText );
      }

      private void ParsePointLines( string[] lines )
      {
         // clear current vertex set collection
         this.vertexSets.Clear();

         VertexSet vertexSet = new VertexSet();
         this.vertexSets.Add( vertexSet );

         //// clear current point collection
         //this.points.Clear();

         // attempt point parsing from text area
         for( int i = 0; i < lines.Length; ++i )
         {
            float[] pointValues = VertexParser.ParseFloats( lines[i] );
            if( pointValues == null || pointValues.Length == 0 )
               continue;

            if( pointValues.Length == 2 ) // 2d point was parsed
            {
               vertexSet.AddVertex( pointValues[0], pointValues[1], 0f );
               //this.points.Add( new PointF( pointValues[0], pointValues[1] ) );
            }
            else if( pointValues.Length == 3 ) // should be 3d point, but for now it including an index number, so use the last two floats...
            {
               vertexSet.AddVertex( pointValues[1], pointValues[2], 0f );
               //this.points.Add( new PointF( pointValues[1], pointValues[2] ) );
            }
         }

         // trigger points changed event
         this.PointsChanged?.Invoke( this, new EventArgs() );
      }

      private void WritePoints()
      {
         // save original zoom factor, which gets reset after Clear method is called (some bug in the control or something)
         float prevZoom = this.textArea.ZoomFactor;

         // clear text area
         this.textArea.Clear();

         // write current vertex sets to text area
         foreach( VertexSet vertSet in this.vertexSets )
         {
            this.textArea.AppendText( vertSet.GenerateText() );
         }

         //// write current points to text area
         //for( int i = 0; i < this.points.Count; ++i )
         //{
         //   PointF p = this.points[i];
         //   this.textArea.AppendText( string.Format( "{0}, {1}\r\n", p.X, p.Y ) );
         //}

         // first reset back to default zoom factor, the restore previous setting to work-around bug
         this.textArea.ZoomFactor = 1f;
         this.textArea.ZoomFactor = prevZoom;
      }

      #endregion Update Methods

      #region Event Handlers

      void ButtonClickHandler( object sender, EventArgs e )
      {
         if( sender == this.buttonOK || sender == this.buttonApply )
            SetPointText( this.textArea.Text );

         if( sender == this.buttonOK || sender == this.buttonCancel )
            this.Hide();
      }

      #endregion Event Handlers
   }

   #endregion Point Edit Form
}