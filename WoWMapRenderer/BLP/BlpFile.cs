using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SereniaBLPLib
{
    // Some Helper Struct to store Color-Data
    public struct ARGBColor8
    {
        public byte Alpha;
        public byte Blue;
        public byte Green;
        public byte Red;

        public ARGBColor8(int r, int g, int b)
        {
            Red = (byte) r;
            Green = (byte) g;
            Blue = (byte) b;
            Alpha = 255;
        }

        public ARGBColor8(byte r, byte g, byte b)
        {
            Red = r;
            Green = g;
            Blue = b;
            Alpha = 255;
        }

        public ARGBColor8(int a, int r, int g, int b)
        {
            Red = (byte) r;
            Green = (byte) g;
            Blue = (byte) b;
            Alpha = (byte) a;
        }

        public ARGBColor8(byte a, byte r, byte g, byte b)
        {
            Red = r;
            Green = g;
            Blue = b;
            Alpha = a;
        }

        /// <summary>
        ///     Converts the given Pixel-Array into the BGRA-Format
        ///     This will also work vice versa
        /// </summary>
        /// <param name="pixel"></param>
        public static void ConvertToBgra(ref byte[] pixel)
        {
            byte tmp = 0;
            for (var i = 0; i < pixel.Length; i += 4)
            {
                tmp = pixel[i]; // store red
                pixel[i] = pixel[i + 2]; // Write blue into red
                pixel[i + 2] = tmp; // write stored red into blue
            }
        }
    }

    public class BlpFile : IDisposable
    {
        private byte alphaDepth; // 0 = no alpha, 1 = 1 Bit, 4 = Bit (only DXT3), 8 = 8 Bit Alpha

        private byte alphaEncoding;
            // 0: DXT1 alpha (0 or 1 Bit alpha), 1 = DXT2/3 alpha (4 Bit), 7: DXT4/5 (interpolated alpha)

        private byte encoding; // 1 = Uncompressed, 2 = DirectX Compressed
        private byte hasMipmaps; // If true (1), then there are Mipmaps
        private uint[] mipmapOffsets = new uint[16]; // Offset for every Mipmap level. If 0 = no more mitmap level
        private uint[] mippmapSize = new uint[16]; // Size for every level
        private ARGBColor8[] paletteBGRA = new ARGBColor8[256]; // The color-palette for non-compressed pictures
        private Stream str; // Reference of the stream
        private uint type; // compression: 0 = JPEG Compression, 1 = Uncompressed or DirectX Compression

        public BlpFile(Stream stream)
        {
            str = stream;
            var buffer = new byte[4];
            // Well, have to fix this... looks weird o.O
            str.Read(buffer, 0, 4);

            // Checking for correct Magic-Code
            if ((new ASCIIEncoding()).GetString(buffer) != "BLP2")
                throw new Exception("Invalid BLP Format");

            // Reading type
            str.Read(buffer, 0, 4);
            type = BitConverter.ToUInt32(buffer, 0);
            if (type != 1)
                throw new Exception("Invalid BLP-Type! Should be 1 but " + type + " was found");

            // Reading encoding, alphaBitDepth, alphaEncoding and hasMipmaps
            str.Read(buffer, 0, 4);
            encoding = buffer[0];
            alphaDepth = buffer[1];
            alphaEncoding = buffer[2];
            hasMipmaps = buffer[3];

            // Reading width
            str.Read(buffer, 0, 4);
            Width = BitConverter.ToInt32(buffer, 0);

            // Reading height
            str.Read(buffer, 0, 4);
            Height = BitConverter.ToInt32(buffer, 0);

            // Reading MipmapOffset Array
            for (var i = 0; i < 16; i++)
            {
                stream.Read(buffer, 0, 4);
                mipmapOffsets[i] = BitConverter.ToUInt32(buffer, 0);
            }

            // Reading MipmapSize Array
            for (var i = 0; i < 16; i++)
            {
                str.Read(buffer, 0, 4);
                mippmapSize[i] = BitConverter.ToUInt32(buffer, 0);
            }

            // When encoding is 1, there is no image compression and we have to read a color palette
            if (encoding == 1)
            {
                // Reading palette
                for (var i = 0; i < 256; i++)
                {
                    var color = new byte[4];
                    str.Read(color, 0, 4);
                    paletteBGRA[i].Blue = color[0];
                    paletteBGRA[i].Green = color[1];
                    paletteBGRA[i].Red = color[2];
                    paletteBGRA[i].Alpha = color[3];
                }
            }
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        /// <summary>
        ///     Returns the amount of Mipmaps in this BLP-File
        /// </summary>
        public int MipMapCount
        {
            get
            {
                var i = 0;
                while (mipmapOffsets[i] != 0) i++;
                return i;
            }
        }

        /// <summary>
        ///     Runs close()
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        ///     Extracts the palettized Image-Data from the given Mipmap and returns a byte-Array in the 32Bit RGBA-Format
        /// </summary>
        /// <param name="mipmap">The desired Mipmap-Level. If the given level is invalid, the smallest available level is choosen</param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="data"></param>
        /// <returns>Pixel-data</returns>
        private byte[] GetPictureUncompressedByteArray(int w, int h, byte[] data)
        {
            var length = w*h;
            var pic = new byte[length*4];
            for (var i = 0; i < length; i++)
            {
                pic[i*4] = paletteBGRA[data[i]].Red;
                pic[i*4 + 1] = paletteBGRA[data[i]].Green;
                pic[i*4 + 2] = paletteBGRA[data[i]].Blue;
                pic[i*4 + 3] = GetAlpha(data, i, length);
            }
            return pic;
        }

        private byte GetAlpha(byte[] data, int index, int alphaStart)
        {
            switch (alphaDepth)
            {
                default:
                    return 0xFF;
                case 1:
                {
                    var b = data[alphaStart + (index/8)];
                    return (byte) ((b & (0x01 << (index%8))) == 0 ? 0x00 : 0xff);
                }
                case 4:
                {
                    var b = data[alphaStart + (index/2)];
                    return (byte) (index%2 == 0 ? (b & 0x0F) << 4 : b & 0xF0);
                }
                case 8:
                    return data[alphaStart + index];
            }
        }

        /// <summary>
        ///     Returns the raw Mipmap-Image Data. This data can either be compressed or uncompressed, depending on the Header-Data
        /// </summary>
        /// <param name="mipmapLevel"></param>
        /// <returns></returns>
        private byte[] GetPictureData(int mipmapLevel)
        {
            if (str != null)
            {
                var data = new byte[mippmapSize[mipmapLevel]];
                str.Position = (int) mipmapOffsets[mipmapLevel];
                str.Read(data, 0, data.Length);
                return data;
            }
            return null;
        }

        /// <summary>
        ///     Returns the uncompressed image as a bytarray in the 32pppRGBA-Format
        /// </summary>
        private byte[] GetImageBytes(int w, int h, byte[] data)
        {
            byte[] pic;
            switch (encoding)
            {
                case 1:
                    pic = GetPictureUncompressedByteArray(w, h, data);
                    break;
                case 2:
                    var flag = (alphaDepth > 1)
                        ? ((alphaEncoding == 7)
                            ? (int) DXTDecompression.DXTFlags.DXT5
                            : (int) DXTDecompression.DXTFlags.DXT3)
                        : (int) DXTDecompression.DXTFlags.DXT1;
                    pic = DXTDecompression.DecompressImage(w, h, data, flag);
                    break;
                case 3:
                    pic = data;
                    break;
                default:
                    pic = new byte[0];
                    break;
            }

            return pic;
        }

        /// <summary>
        ///     Converts the BLP to a System.Drawing.Bitmap
        /// </summary>
        /// <param name="mipmapLevel">
        ///     The desired Mipmap-Level. If the given level is invalid, the smallest available level is
        ///     choosen
        /// </param>
        /// <returns>The Bitmap</returns>
        public Bitmap GetBitmap(int mipmapLevel)
        {
            if (mipmapLevel >= MipMapCount) mipmapLevel = MipMapCount - 1;
            if (mipmapLevel < 0) mipmapLevel = 0;

            var scale = (int) Math.Pow(2, mipmapLevel);
            var w = Width/scale;
            if (w < 1) w = 1;
            var h = Height/scale;
            if (h < 1) h = 1;
            var bmp = new Bitmap(w, h);

            var data = GetPictureData(mipmapLevel);
            var pic = GetImageBytes(w, h, data); // This bytearray stores the Pixel-Data

            // Faster bitmap Data copy
            var bmpdata = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            // when we want to copy the pixeldata directly into the bitmap, we have to convert them into BGRA befor doing so
            ARGBColor8.ConvertToBgra(ref pic);
            Marshal.Copy(pic, 0, bmpdata.Scan0, pic.Length); // copy! :D
            bmp.UnlockBits(bmpdata);

            return bmp;
        }

        public byte[] GetByteBuffer(int mipmapLevel)
        {
            if (mipmapLevel >= MipMapCount) mipmapLevel = MipMapCount - 1;
            if (mipmapLevel < 0) mipmapLevel = 0;

            var scale = (int)Math.Pow(2, mipmapLevel);
            var w = Width / scale;
            if (w < 1) w = 1;
            var h = Height / scale;
            if (h < 1) h = 1;

            return GetImageBytes(w, h, GetPictureData(mipmapLevel));
            // ARGBColor8.ConvertToBgra(ref picture);
        }

        /// <summary>
        ///     Closes the Memorystream
        /// </summary>
        public void Close()
        {
            if (str != null)
            {
                str.Close();
                str = null;
            }
        }
    }
}