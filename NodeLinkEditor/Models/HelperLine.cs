namespace NodeLinkEditor.Models
{
    public class HelperLine
    {
        public Guid ID { get; set; } = Guid.CreateVersion7();
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double EndX { get; set; }
        public double EndY { get; set; }
        public bool HasEqualCoordinates(HelperLine other)
        {
            if (StartX != other.StartX) { return false; }
            if (StartY != other.StartY) { return false; }
            if (EndX != other.EndX) { return false; }
            if (EndY != other.EndY) { return false; }
            return true;
        }
    }
}
