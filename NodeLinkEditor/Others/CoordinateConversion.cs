using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NodeLinkEditor.Others
{
    static public class CoordConv
    {
        static public double Resolution { get; set; } = 1.0;
        static public double OriginX { get; set; } = 0.0;
        static public double OriginY { get; set; } = 0.0;
        static public double Width { get; set; } = 1.0;
        static public double Height { get; set; } = 1.0;
        /// <summary>
        /// pixelのX値を座標のX値に変換する
        /// 座標の向きは同じ
        /// 座標X[m] = pixelX*resolution+originX
        /// </summary>
        /// <param name="pixelX"></param>
        /// <param name="resolution">m/pixel</param>
        /// <param name="originX">座標系の原点から見た左端の位置</param>
        /// <returns></returns>
        public static double PixelXToCoordX(double pixelX) => Resolution == 0 ? pixelX : pixelX * Resolution + OriginX;
        /// <summary>
        /// pixelのY値を座標のY値に変換する
        /// 座標の向きは逆
        /// 座標Y[m] = (画像幅-pixelY)*resolution+originY
        /// </summary>
        /// <param name="pixelY"></param>
        /// <param name="resolution">m/pixel</param>
        /// <param name="originY">座標系の原点から見た下端の位置</param>
        /// <returns></returns>
        public static double PixelYToCoordY(double pixelY) => Resolution == 0 ? pixelY : (Height - pixelY) * Resolution + OriginY;
        public static Point PixelToCoord(Point point) => new(PixelXToCoordX(point.X), PixelYToCoordY(point.Y));
        /// <summary>
        /// 座標のX値をpixelのX値に変換する
        /// pixelX = (座標X-originX)/resolution
        /// </summary>
        public static double CoordXToPixelX(double coordX) => Resolution == 0 ? coordX : (coordX - OriginX) / Resolution;
        /// <summary>
        /// 座標のY値をpixelのY値に変換する
        /// pixelY = -(座標Y-originY)/resolution+height[pixel]
        /// </summary>
        public static double CoordYToPixelY(double coordY) => Resolution == 0 ? coordY : -(coordY - OriginY) / Resolution + Height;
        public static Point CoordToPixel(Point point) => new(CoordXToPixelX(point.X), CoordYToPixelY(point.Y));


    }
}
