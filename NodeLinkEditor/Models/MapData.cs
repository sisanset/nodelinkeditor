using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NodeLinkEditor.Models
{
    public class MapData
    {
        public string YamlFilePath { get; set; } = string.Empty;
        public ImageSource MapImage { get; set; } = new BitmapImage();
        public int Width { get; set; }
        public int Height { get; set; }
        public double Resolution { get; set; }
        public double[] Origin { get; set; } = new double[3];
        //public double OccupiedThresh { get; set; }
        //public double FreeThresh { get; set; }
        //public int Negate { get; set; }
    }

}
