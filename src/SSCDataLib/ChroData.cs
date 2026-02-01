namespace SSC.DataLib
{
    public struct Point3F
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Point3F(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }
    }

    public class ChroData
    {
        // Identifiers
        public string? Id { get; set; }
        public string? Name { get; set; }

        // Sample / instrument info
        public string? SampleName { get; set; }
        public string? Instrument { get; set; }
        public string? Method { get; set; }
        public DateTime? AcquisitionTime { get; set; }

        // Core chromatogram arrays (parallel arrays)
        // Retention times (same length as Intensities). Unit described by TimeUnit.
        public double[] RetentionTimes { get; set; } = Array.Empty<double>();
        // Detector intensities (same length as RetentionTimes). Unit described by IntensityUnit.
        public double[] Intensities { get; set; } = Array.Empty<double>();
        // Optional scan numbers mapping to each point
        public int[] ScanNumbers { get; set; } = Array.Empty<int>();

        // 3D point collection (convenience collection representing X/Y/Z points)
        public List<Point3F> Points3D { get; set; } = new List<Point3F>();

        // Units and flags
        public string TimeUnit { get; set; } = "min";
        public string IntensityUnit { get; set; } = "a.u.";
        public bool IsCentroided { get; set; }

        // Additional metadata
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        // Derived properties
        public int PointCount => Math.Max(RetentionTimes?.Length ?? 0, Intensities?.Length ?? 0);

        public double StartTime => (RetentionTimes.Length > 0) ? RetentionTimes[0] : double.NaN;
        public double EndTime => (RetentionTimes.Length > 0) ? RetentionTimes[RetentionTimes.Length - 1] : double.NaN;

        public double MaxIntensity => (Intensities.Length > 0) ? Intensities.Max() : double.NaN;
        public double MinIntensity => (Intensities.Length > 0) ? Intensities.Min() : double.NaN;
        public double TotalIntensity => (Intensities.Length > 0) ? Intensities.Sum() : 0.0;

        // Simple helper to get the index of the maximum intensity
        public int IndexOfMaxIntensity()
        {
            if (Intensities == null || Intensities.Length == 0) return -1;
            var max = MaxIntensity;
            for (int i = 0; i < Intensities.Length; i++) if (Intensities[i] == max) return i;
            return -1;
        }

        // Constructor
        public ChroData() { }

        // Convenience constructor to create from arrays (defensive copy)
        public ChroData(string? id, string? name, double[]? retentionTimes, double[]? intensities)
        {
            Id = id;
            Name = name;
            RetentionTimes = retentionTimes != null ? (double[])retentionTimes.Clone() : Array.Empty<double>();
            Intensities = intensities != null ? (double[])intensities.Clone() : Array.Empty<double>();
            ScanNumbers = Array.Empty<int>();
            Points3D = new List<Point3F>();
        }
    }
}
