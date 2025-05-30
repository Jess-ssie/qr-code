using System.Drawing;
using Helpers;

namespace QRCoder
{
    public class QRCodeOptions
    {
        public int Version { get; set; } = 1;
        public ErrorCorrectionLevel ErrorCorrectionLevel { get; set; } = ErrorCorrectionLevel.M;
        public EncodingMode EncodingMode { get; set; } = EncodingMode.Byte;
        public int Scale { get; set; } = 4;
        public int QuietZone { get; set; } = 4;
        public Color ForegroundColor { get; set; } = Color.Black;
        public Color BackgroundColor { get; set; } = Color.White;
    }
}