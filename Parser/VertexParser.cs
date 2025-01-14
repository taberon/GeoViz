using System;
using System.Collections.Generic;
using System.Text;

namespace GeometryVisualizer
{
   public class VertexParser
   {
      public struct ParseErrorInfo
      {
         public int Position;
         public int Length;
         // type ..? (error, warning..?)

         public ParseErrorInfo( int position, int length )
         {
            this.Position = position;
            this.Length = length;
         }
      }

      static List<ParseErrorInfo> parseErrors = new List<ParseErrorInfo>();
      public static List<ParseErrorInfo> ParseErrors
      {
         get { return parseErrors; }
      }

      public static void ClearErrors()
      {
         parseErrors.Clear();
      }

      static void AddError( int index, int length )
      {
         parseErrors.Add( new ParseErrorInfo( index, length ) );
      }

      public static List<VertexSet> ParseVertexSets( string text )
      {
         // reset error state
         ClearErrors();

         StringBuilder currFloatString = new StringBuilder();
         List<float> currFloatList = new List<float>();

         VertexSet currVertexSet = new VertexSet() { Is2D = true };
         List<VertexSet> vertexSets = new List<VertexSet>();

         // TODO: save BraceInfo - ( char, index ) pair -- use for improved parse error highlights..?
         Stack<char> braceStack = new Stack<char>();

         bool decimalAdded = false;
         bool negativeSign = false;
         bool exponentNotation = false;

         StringBuilder currWordString = new StringBuilder();

         void EmitFloat()
         {
            // emit current float value
            if( currFloatString.Length > 0 && float.TryParse( currFloatString.ToString(), out float floatVal ) )
            {
               currFloatList.Add( floatVal );
            }

            // reset float parse state
            currFloatString.Clear();
            decimalAdded = false;
            negativeSign = false;
            exponentNotation = false;
         }

         void EmitVertex()
         {
            // check to emit current float value
            EmitFloat();

            // emit current vertex (list of floats) -- if at least 2 or more floats
            if( currFloatList.Count > 1 )
            {
               currVertexSet.AddVertex( currFloatList );
            }

            // reset vertex parse state
            currFloatList.Clear();
         }

         void EmitVertexSet()
         {
            // check to emit current vertex
            EmitVertex();

            // emit current vertex set (vertex loop)
            if( currVertexSet.Count > 0 )
            {
               currVertexSet.AutoSet2D();
               vertexSets.Add( currVertexSet );
               currVertexSet = new VertexSet() { Is2D = true };
            }

            // reset current vertex set parse state
            currVertexSet.IsPolyline = false;
            currVertexSet.IsClosed = false;
            // TODO: reset colors..?
         }

         char GetMatchingBrace( char braceChar )
         {
            switch( braceChar )
            {
               case '(': return ')';
               case ')': return '(';
               case '{': return '}';
               case '}': return '{';
               case '[': return ']';
               case ']': return '[';
               default: return ' ';
            }
         }

         char c;
         for( int i = 0; i < text.Length; ++i )
         {
            c = char.ToLower( text[i] );
            switch( c )
            {
               // check for float chars
               case '0': case '1':
               case '2': case '3':
               case '4': case '5':
               case '6': case '7':
               case '8': case '9':
               {
                  currFloatString.Append( c );
                  break;
               }
               // allow one decimal
               case '.':
               {
                  if( !decimalAdded )
                  {
                     currFloatString.Append( c );
                     decimalAdded = true;
                  }
                  else
                  {
                     EmitFloat();
                  }
                  break;
               }
               // allow one negative sign
               case '-':
               {
                  // allowed either at start of float or immediately after "e" or "E" char for exponent notation,
                  // in which case still only one negative sign that won't appear at start
                  if( !negativeSign && ( currFloatString.Length == 0 || char.ToLower( currFloatString[currFloatString.Length - 1] ) == 'e' ) )
                  {
                     currFloatString.Append( c );
                     negativeSign = true;
                  }
                  else
                  {
                     EmitFloat();
                  }
                  break;
               }
               // allow one 'e' or "E" for scientific/exponent notation
               case 'e':
               case 'E':
               {
                  // allowed after at least one other valid number and a decimal point
                  if( !exponentNotation && currFloatString.Length > 1 && decimalAdded )
                  {
                     currFloatString.Append( c );
                     exponentNotation = true;
                  }
                  else
                  {
                     EmitFloat();
                  }
                  break;
               }
               // white-space -- delimit float values
               case ' ':
               case '\t':
               {
                  // (don't need to handle these chars explicitly -- could just be handled by 'default' case...)
                  EmitFloat();
                  break;
               }
               // newline -- delimit vertices
               case '\n':
               case '\r':
               {
                  EmitVertex();
                  break;
               }
               // track parenthesis/brace groups
               case '(':
               case '{':
               case '[':
               {
                  // push opening brace on stack
                  braceStack.Push( c );

                  // ensure new vertex on opening brace
                  EmitVertex();

                  // check outer-most brace type to set vertex set property
                  if( braceStack.Count == 1 )
                  {
                     // use curly-brace to denote start of polygon (closed)
                     if( c == '{' )
                     {
                        currVertexSet.IsPolyline = true;
                        currVertexSet.IsClosed = true;
                     }
                     // use square-brace to denote start of polyline (open)
                     else if( c == '[' )
                     {
                        currVertexSet.IsPolyline = true;
                        currVertexSet.IsClosed = false;
                     }
                  }

                  break;
               }
               case ')':
               case '}':
               case ']':
               {
                  if( braceStack.Count == 0 )
                  {
                     // brace mismatch -- highlight error line in text..?
                     AddError( i, 1 );
                     break;
                  }

                  // validate matching closing brace
                  char openingBrace = GetMatchingBrace( c );
                  if( braceStack.Peek() == openingBrace )
                  {
                     braceStack.Pop();
                     EmitVertex();
                  }
                  else // parse error -- brace mismatch
                  {
                     // log parse error...
                     AddError( i, 1 );
                  }

                  if( braceStack.Count == 0 )
                  {
                     // top-level grouping reached, start new polygon
                     EmitVertexSet();
                  }
                  break;
               }

               // all other characters, delimit floats
               default:
               {
                  EmitFloat();
                  break;
               }
            }
         }

         // ensure current working vertex set is emitted
         EmitVertexSet();

         return vertexSets;
      }

      public static float[] ParseFloats( string textLine )
      {
         // reset error state
         ClearErrors();

         // get characters of string to edit in-line
         char[] chars = textLine.ToCharArray();
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
            return new float[] { }; // return empty array

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

   }
}