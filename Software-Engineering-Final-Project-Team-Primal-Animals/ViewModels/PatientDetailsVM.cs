using System;
using System.Collections.Generic;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.ViewModels
{
    public class PatientDetailsVM
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }

        // Latest values
        public int PeakPressureIndex { get; set; }
        public string ContactArea { get; set; }

        // Threshold
        public int HighPressureThreshold { get; set; }

        // Heatmap matrix
        public string HeatmapJson { get; set; }
        public int MatrixSize { get; set; }

        // Frames for timeline slider
        public List<FrameInfo> Frames { get; set; } = new();

        // Comments list (latest first)
        public List<CommentVM> Comments { get; set; } = new();

        // Chart time series (peak & contact)
        public List<TimeSeriesPoint> PeakPressureSeries { get; set; } = new();
        public List<TimeSeriesPoint> ContactAreaSeries { get; set; } = new();
    }
}
