using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace GeometryVisualizer
{
   #region Point Edit Form

   public class PointEditForm : Form
   {
      #region Fields and Properties

      TextBox textArea;

      // TODO: perhaps use data grid eventually for formatted display of points
      //DataGridView dataGrid;

      Button buttonOK;
      Button buttonApply;
      Button buttonCancel;

      public event EventHandler PointsChanged;

      List<PointF> points = new List<PointF>();
      public List<PointF> Points
      {
         get
         {
            // return current point collection
            return this.points;
            // points are only updated from text when Apply or OK are clicked
         }
         set
         {
            // copy given point collection
            this.points.Clear();
            this.points.AddRange( value );
            // update text area with current points
            WritePoints();
         }
      }

      #endregion Fields and Properties

      #region Construction

      public PointEditForm()
      {
         this.Text = "Geometry Vizualizer: Points";
         this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
         this.SizeGripStyle = SizeGripStyle.Show;
         this.ShowInTaskbar = false;

         int buttonHeight = 24;
         int buttonWidth = 64;
         int buttonSpace = 8;

         // set minimum form size
         int borderWidth = SystemInformation.FrameBorderSize.Width;
         int maxFormWidth = buttonWidth * 3 + buttonSpace * 4 + borderWidth * 2;
         this.MinimumSize = new Size( maxFormWidth, maxFormWidth );

         this.textArea = new TextBox();
         this.textArea.Multiline = true;
         this.textArea.ScrollBars = ScrollBars.Vertical;
         this.textArea.Width = this.ClientSize.Width;
         this.textArea.Height = this.ClientSize.Height - buttonHeight;
         this.textArea.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
         this.Controls.Add( this.textArea );

         this.buttonApply = new Button();
         this.buttonApply.Size = new Size( buttonWidth, buttonHeight );
         this.buttonApply.Text = "Apply";
         this.buttonApply.Top = this.ClientRectangle.Bottom - buttonHeight;
         this.buttonApply.Left = buttonSpace;
         this.buttonApply.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
         this.buttonApply.Click += ButtonClickHandler;
         this.Controls.Add( this.buttonApply );

         this.buttonCancel = new Button();
         this.buttonCancel.Size = new Size( buttonWidth, buttonHeight );
         this.buttonCancel.Text = "Cancel";
         this.buttonCancel.Top = this.ClientRectangle.Bottom - buttonHeight;
         this.buttonCancel.Left = this.ClientRectangle.Right - buttonWidth - buttonSpace;
         this.buttonCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
         this.buttonCancel.Click += ButtonClickHandler;
         this.Controls.Add( this.buttonCancel );

         this.buttonOK = new Button();
         this.buttonOK.Size = new Size( buttonWidth, buttonHeight );
         this.buttonOK.Text = "OK";
         this.buttonOK.Top = this.ClientRectangle.Bottom - buttonHeight;
         this.buttonOK.Left = this.buttonCancel.Left - buttonWidth - buttonSpace;
         this.buttonOK.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
         this.buttonOK.Click += ButtonClickHandler;
         this.Controls.Add( this.buttonOK );
      }

      #endregion Construction

      #region Update Methods

      float[] ParseFloats_old( string text )
      {
         // get characters of string to edit in-line
         char[] chars = text.ToCharArray();
         int ic = 0; // current valid character position
         for( int i = 0; i < chars.Length; ++i )
         {
            char c = chars[i];
            // check for valid characters to leave
            if( char.IsDigit( c ) || c == ' ' || c == '-' || c == '.' || c == ',' )
               chars[ic++] = chars[i]; // shift valid character in string
         }

         // ensure valid characters remain
         if( ic == 0 )
            return new float[] {}; // return empty array

         // get valid character string
         string validChars = new string( chars, 0, ic );

         // split floats strings
         string[] floatStrings = validChars.Split( new char[] { ',', ' ' } );

         // create dynamic return collection for valid floats
         List<float> retFloats = new List<float>();
         for( int i = 0; i < floatStrings.Length; ++i )
         {
            // skip blank lines
            if( floatStrings[i].Length == 0 )
               continue;
            retFloats.Add( float.Parse( floatStrings[i] ) );
         }

         return retFloats.ToArray();
      }

      float[] ParseFloats( string text )
      {
         //System.Text.StringBuilder currString = new System.Text.StringBuilder();
         //Stack<char> braceStack = new Stack<char>();

         // get characters of string to edit in-line
         char[] chars = text.ToCharArray();
         int ic = 0; // current valid character position
         for( int i = 0; i < chars.Length; ++i )
         {
            char c = chars[i];
            // check for valid characters to leave
            if( char.IsDigit( c ) || c == ' ' || c == '\t' || c == '-' || c == '.' || c == ',' )
               chars[ic++] = chars[i]; // shift valid character in string
         }

         // ensure valid characters remain
         if( ic == 0 )
            return new float[] { }; // return empty array

         // get valid character string
         string validChars = new string( chars, 0, ic );

         // split floats strings
         string[] floatStrings = validChars.Split( new char[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries );

         // create dynamic return collection for valid floats
         List<float> retFloats = new List<float>();
         float currFloat;
         for( int i = 0; i < floatStrings.Length; ++i )
         {
            // skip blank lines
            if( string.IsNullOrWhiteSpace( floatStrings[i] ) )
               continue;

            if( float.TryParse( floatStrings[i], out currFloat ) )
               retFloats.Add( currFloat );
         }

         return retFloats.ToArray();
      }

      public void SetPointText( string pointsText )
      {
         string[] lines = pointsText.Split( '\r', '\n' );
         ParsePoints( lines );
      }

      private void ParsePoints( string[] lines )
      {
         // clear current point collection
         this.points.Clear();

         // attempt point parsing from text area
         for( int i = 0; i < lines.Length; ++i )
         {
            float[] pointValues = ParseFloats( lines[i] );
            if( pointValues == null || pointValues.Length == 0 )
               continue;

            if( pointValues.Length == 2 ) // 2d point was parsed
               this.points.Add( new PointF( pointValues[0], pointValues[1] ) );
            else if( pointValues.Length == 3 ) // should be 3d point, but for now it including an index number, so use the last two floats...
               this.points.Add( new PointF( pointValues[1], pointValues[2] ) );
         }

         // trigger points changed event
         this.PointsChanged?.Invoke( this, new EventArgs() );
      }

      private void WritePoints()
      {
         // clear text area
         this.textArea.Clear();

         // write current points to text area
         for( int i = 0; i < this.points.Count; ++i )
         {
            PointF p = this.points[i];
            this.textArea.AppendText( string.Format( "{0}, {1}\r\n", p.X, p.Y ) );
         }
      }

      #endregion Update Methods

      #region Event Handlers

      void ButtonClickHandler( object sender, EventArgs e )
      {
         if( sender == this.buttonOK || sender == this.buttonApply )
            ParsePoints( this.textArea.Lines );
         if( sender == this.buttonOK || sender == this.buttonCancel )
            this.Hide();
      }

      #endregion Event Handlers
   }

   #endregion Point Edit Form
}