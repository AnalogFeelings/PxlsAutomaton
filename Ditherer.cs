using AForge.Imaging.ColorReduction;
using System.Drawing;

namespace PxlsAutomaton
{
    public class Ditherer
    {
        public Dictionary<Color, int> ColorPalette = new()
        {
            { Color.FromArgb(0, 0, 0), 0 },
            { Color.FromArgb(34, 34, 34), 1 },
            { Color.FromArgb(85, 85, 85) , 2 },
            { Color.FromArgb(136, 136, 136), 3 },
            { Color.FromArgb(205, 205, 205), 4 },
            { Color.FromArgb(255, 255, 255), 5 },
            { Color.FromArgb(245, 205, 181), 6 },
            { Color.FromArgb(255, 183, 131), 7 },
            { Color.FromArgb(182, 109, 61), 8 },
            { Color.FromArgb(119, 67, 31), 9 },
            { Color.FromArgb(252, 117, 16), 10 },
            { Color.FromArgb(252, 168, 14), 11 },
            { Color.FromArgb(253, 232, 23), 12 },
            { Color.FromArgb(255, 244, 145), 13 },
            { Color.FromArgb(190, 255, 64), 14 },
            { Color.FromArgb(112, 221, 19), 15 },
            { Color.FromArgb(49, 161, 23), 16 },
            { Color.FromArgb(11, 95, 53), 17 },
            { Color.FromArgb(39, 126, 108), 18 },
            { Color.FromArgb(50, 182, 159), 19 },
            { Color.FromArgb(136, 255, 243), 20 },
            { Color.FromArgb(36, 181, 254), 21 },
            { Color.FromArgb(18, 92, 199), 22 },
            { Color.FromArgb(38, 41, 96), 23 },
            { Color.FromArgb(139, 47, 168), 24 },
            { Color.FromArgb(210, 76, 233), 25 },
            { Color.FromArgb(255, 89, 239), 26 },
            { Color.FromArgb(255, 169, 217), 27 },
            { Color.FromArgb(255, 100, 116), 28 },
            { Color.FromArgb(240, 37, 35), 29 },
            { Color.FromArgb(177, 18, 6), 30 },
            { Color.FromArgb(116, 12, 0), 31 },
        };

        public Bitmap ExecuteFloydSteinberg(ref Bitmap Bitmap)
        {
            FloydSteinbergColorDithering FloydSteinberg = new FloydSteinbergColorDithering();
            FloydSteinberg.ColorTable = ColorPalette.Keys.ToArray();

            return FloydSteinberg.Apply(Bitmap);
        }
    }
}
