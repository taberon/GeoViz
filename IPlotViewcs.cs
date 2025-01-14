using System;
using System.Collections.Generic;

namespace GeometryVisualizer
{

   public delegate void PlotViewChangedDelegate();

   public class PlotViewOptions
   {
      public bool DrawAxis { get; set; }
      public bool DrawGrid { get; set; }
      public bool MaintainAspect { get; set; }

      public PlotViewOptions()
      {
         this.DrawAxis = true;
         this.DrawGrid = true;
         this.MaintainAspect = true;
      }
   }

   public interface IPlotView
   {
      PlotData PlotData { get; }

      PlotViewOptions PlotOptions { get; }

      PlotRenderer PlotRenderer { get; }

      /// <summary> Event fired whenever the plot scale or view center is changed. </summary>
      event PlotViewChangedDelegate PlotViewChanged;

      /// <summary>  </summary>
      void BestFitView();

      string GetPlotStatusBounds();

      string GetPlotStatusSize();

   }

}