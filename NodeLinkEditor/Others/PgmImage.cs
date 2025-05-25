using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeLinkEditor.Others
{
    public class PgmImage
    {
        public static (int, int, byte[]) LoadPgmImage(string path)
        {
            var line = string.Empty;
            using (var reader = new StreamReader(path))
            {
                line = reader.ReadLine();
            }
            if (line == "P2")
            { return LoadP2(path); }
            else if (line == "P5")
            { return LoadP5(path); }
            else
            { throw new InvalidDataException("Not a P2 or P5 PGM file"); }
        }

        private static (int, int, byte[]) LoadP2(string path)
        {
            using var reader = new StreamReader(path);
            string? line;

            // ヘッダー読み込み
            line = reader.ReadLine();
            if (line != "P2")
            { throw new InvalidDataException("Not a P2 PGM file"); }

            // コメント・空行をスキップ
            do
            {
                line = reader.ReadLine();
            } while (line != null && line.StartsWith("#"));

            // サイズ
            string[] size = line!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var width = int.Parse(size[0]);
            var height = int.Parse(size[1]);

            // 最大値（例: 255）
            int maxValue = int.Parse(reader.ReadLine()!);
            if (maxValue > 255)
            { throw new InvalidDataException("Invalid PGM file format. Expected max value of 255."); }

            // ピクセル値読み込み
            var pixeld = new byte[height * width];
            int count = 0;
            while ((line = reader.ReadLine()) != null)
            {
                var values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var val in values)
                { pixeld[count++] = byte.Parse(val); }
            }

            if (count != width * height)
            { throw new InvalidDataException("Pixel data does not match expected size"); }

            return (width, height, pixeld);

        }
        private static (int, int, byte[]) LoadP5(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(stream, Encoding.ASCII);

            string magicNumber = ReadToken(reader);
            if (magicNumber != "P5")
            { throw new InvalidDataException("Invalid PGM file format. Expected P5."); }
            string token;
            do
            {
                token = ReadToken(reader);
            } while (token.StartsWith("#"));
            var width = int.Parse(token);
            var height = int.Parse(ReadToken(reader));
            int maxValue = int.Parse(ReadToken(reader));
            if (maxValue > 255)
            { throw new InvalidDataException("Invalid PGM file format. Expected max value of 255."); }
            var pixels = new byte[height * width];
            for (int i = 0; i < height * width; i++)
            {
                pixels[i] = reader.ReadByte();
            }
            return (width, height, pixels);
        }
        private static string ReadToken(BinaryReader reader)
        {
            byte b;
            do
            {
                b = reader.ReadByte();
            } while (b == ' ' || b == '\n' || b == '\r' || b == '\t');
            if (b == '#')
            {

                while (reader.ReadByte() != '\n') ;
                return ReadToken(reader);
            }
            var bytes = new List<byte>();
            do
            {
                bytes.Add(b);
                b = reader.PeekChar() >= 0 ? reader.ReadByte() : (byte)0;
            } while (b != ' ' && b != '\n' && b != '\r' && b != '\t');
            return Encoding.ASCII.GetString([.. bytes]);
        }
    }
}
