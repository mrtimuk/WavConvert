using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WavConvert
{
    public class WavConvert
    {
        public static WavFormat ReadWavFormat(BinaryReader inReader, out ushort inCbSize)
        {
            var inFormat = new WavFormat();

            ExpectAscii(inReader, "fmt ");
            var fmtLength = inReader.ReadUInt32();
            inFormat.Format = (WavFormatCode)inReader.ReadUInt16();
            inFormat.Channels = inReader.ReadUInt16();
            inFormat.SampleRate = inReader.ReadUInt32();
            var inByteRate = inReader.ReadUInt32();
            var inBlockAlign = inReader.ReadUInt16();
            inFormat.BitDepth = inReader.ReadUInt16();
            inCbSize = (ushort)(fmtLength > 16 ? inReader.ReadUInt16() : 0);
            inReader.BaseStream.Seek(inCbSize, SeekOrigin.Current); // Skip CB

            if (inFormat.BlockAlign != inBlockAlign)
            {
                inFormat.Error = true;

                Console.Error.WriteLine("Error: Inconsistent block alignment: {0} channels of {1} bits is not {2} bytes",
                        inFormat.Channels,
                        inFormat.BitDepth,
                        inBlockAlign);
            }

            if (inFormat.ByteRate != inByteRate)
            {
                inFormat.Error = true;
                Console.Error.WriteLine("Error: Inconsistent sample rate: {0} channels of {1} bits at {2} KHz is not {3} KBps",
                        inFormat.Channels,    
                        inFormat.BitDepth,
                        inFormat.SampleRate/1000,
                        inByteRate/1000);
            }

            return inFormat;
        }

        public static bool Convert(
            Stream inStream, 
            Stream outStream, 
            WavFormat outFormat)
        {
            using (var inReader = new BinaryReader(inStream))
            {
                var inFormat = new WavFormat();

                // Read header
                ExpectAscii(inReader, "RIFF");
                var fileLength = inReader.ReadUInt32() + 8;
                ExpectAscii(inReader, "WAVE");

                ushort inCbSize;
                inFormat = ReadWavFormat(inReader, out inCbSize);
                
                ExpectAscii(inReader, "data");
                inFormat.SetDataLength(inReader.ReadUInt32());

                Console.WriteLine();
                Console.WriteLine("Input file:");
                Console.WriteLine("   Format:      {0}", inFormat.Format);
                Console.WriteLine("   Channels:    {0}", inFormat.ChannelString);
                Console.WriteLine("   Bit depth:   {0} bit", inFormat.BitDepth);
                Console.WriteLine("   Sample rate: {0} KHz", inFormat.SampleRate / 1000.0);
                Console.WriteLine("   Bit rate:    {0} Kbs", inFormat.ByteRate * 8 / 1000);
                Console.WriteLine();
                Console.WriteLine("   File length: {0} bytes", fileLength);
                Console.WriteLine("   CB Size:     {0} bytes", inCbSize);
                Console.WriteLine("   Block align: {0} bytes", inFormat.BlockAlign);
                Console.WriteLine("   Data:        {0} samples", inFormat.Samples);
                Console.WriteLine();

                if (inFormat.Error)
                {
                    return false;
                }

                switch (inFormat.Format)
                {
                    case WavFormatCode.Pcm:
                    case WavFormatCode.Ieee754:
                    case WavFormatCode.Extensible:
                        break;

                    default:
                        Console.Error.WriteLine(
                            "File format {0} is not supported. Only PCM, Extensible, and IEEE 574", inFormat.Format);
                        return false;
                }

                outFormat.Samples = inFormat.Samples;

                if (outStream != null)
                {
                    using (var outWriter = new BinaryWriter(outStream))
                    {
                        Console.WriteLine("Writing output file");

                        var outFileLength = 44 + outFormat.DataLength;

                        // Write header
                        WriteAscii(outWriter, "RIFF");
                        outWriter.Write(outFileLength - 8);
                        WriteAscii(outWriter, "WAVE");
                        WriteAscii(outWriter, "fmt ");
                        outWriter.Write(16); // no CB
                        outWriter.Write((ushort) outFormat.Format);
                        outWriter.Write(outFormat.Channels);
                        outWriter.Write(outFormat.SampleRate);
                        outWriter.Write(outFormat.ByteRate);
                        outWriter.Write(outFormat.BlockAlign);
                        outWriter.Write(outFormat.BitDepth);
                        WriteAscii(outWriter, "data");
                        outWriter.Write(outFormat.DataLength);

                        MoveSamples(inReader, inFormat, outWriter, outFormat);
                    }
                }
            }
            return true;
        }

        private static void MoveSamples(
            BinaryReader inReader,  WavFormat inFormat,
            BinaryWriter outWriter, WavFormat outFormat)
        {
            // For each frame that we read, we write as many frames as required for this slice of time
            // (possibly zero if the output sample rate is lower than the input)

            var lastInFrameTime = 0.0;
            var lastOutFrameTime = 0.0;
            var lastInFrame = new double[] {};

            if (inReader.BaseStream.Position != inReader.BaseStream.Length)
            {
                lastInFrame = ReadFrame(inReader, inFormat);
                lastInFrame = ApplyMatrix(lastInFrame, outFormat.Channels);
                WriteFrame(outWriter, outFormat, lastInFrame);
            }

            var inDeltaTime = 1.0 / inFormat.SampleRate;
            var outDeltaTime = 1.0 / outFormat.SampleRate;
            while (inReader.BaseStream.Position != inReader.BaseStream.Length)
            {
                var thisInFrame = ReadFrame(inReader, inFormat);
                var thisInFrameTime = lastInFrameTime + inDeltaTime;

                thisInFrame = ApplyMatrix(thisInFrame, outFormat.Channels);

                // Resample: output is interpolated from the last two frames from the source
                for (var thisOutFrameTime = lastOutFrameTime + outDeltaTime;
                     thisOutFrameTime < thisInFrameTime;
                     thisOutFrameTime += outDeltaTime)
                {
                    var ratio = (thisOutFrameTime - lastInFrameTime)/outDeltaTime;

                    var frame = lastInFrame.Zip(thisInFrame, (s1, s2) => ratio*s1 + (1.0 - ratio)*s2)
                                           .ToArray();

                    WriteFrame(outWriter, outFormat, frame);
                    lastOutFrameTime = thisOutFrameTime;
                }

                lastInFrameTime = thisInFrameTime;
                lastInFrame = thisInFrame;
            }
        }

        private static double[] ReadFrame(BinaryReader inReader, WavFormat inFormat)
        {
            double[] frame;
            switch (inFormat.Format)
            {
                case WavFormatCode.Pcm:
                case WavFormatCode.Extensible:
                    switch (inFormat.BitDepth)
                    {
                        case 8: frame = ReadFrameI8(inReader, inFormat.Channels); break;
                        case 16: frame = ReadFrameI16(inReader, inFormat.Channels); break;
                        case 24: frame = ReadFrameI24(inReader, inFormat.Channels); break;
                        case 32: frame = ReadFrameI32(inReader, inFormat.Channels); break;
                        default: throw new FormatException(
                            "Input PCM bit depth " + inFormat.BitDepth + " is not supported");
                    }
                    break;

                case WavFormatCode.Ieee754:
                    switch (inFormat.BitDepth)
                    {
                        case 32: frame = ReadFrameF32(inReader, inFormat.Channels); break;
                        case 64: frame = ReadFrameF64(inReader, inFormat.Channels); break;
                        default: throw new FormatException(
                            "Input IEEE bit depth " + inFormat.BitDepth + " is not supported");
                    }
                    break;

                default:
                    throw new FormatException("Input file format " + inFormat.Format + " is not supported");
            }
            return frame;
        }

        private static void WriteFrame(BinaryWriter outWriter, WavFormat outFormat, double[] frame)
        {
            switch (outFormat.Format)
            {
                case WavFormatCode.Pcm:
                    switch (outFormat.BitDepth)
                    {
                        case 8: WriteFrameI8(outWriter, frame); break;
                        case 16: WriteFrameI16(outWriter, frame); break;
                        case 32: WriteFrameI32(outWriter, frame); break;
                        default: throw new FormatException(string.Format(
                            "Output bit depth {0} is not supported", outFormat.BitDepth));
                    }
                    break;

                case WavFormatCode.Ieee754:
                    switch (outFormat.BitDepth)
                    {
                        case 32: WriteFrameF32(outWriter, frame); break;
                        case 64: WriteFrameF64(outWriter, frame); break;
                        default: throw new FormatException(string.Format(
                            "Output IEEE bit depth {0} is not supported", outFormat.BitDepth));
                    }
                    break;

                default:
                    throw new FormatException(string.Format(
                        "Output file format {0} is not supported", outFormat.Format));
            }
        }

        // 8-bit unsigned integer samples
        private static double[] ReadFrameI8(BinaryReader inStream, ushort channels)
        {
            return Enumerable.Range(0, channels).Select(i =>
            {
                var b = inStream.ReadByte();
                return (b - 127.0) / 128.0;
            }).ToArray();
        }

        // 16-bit signed integer samples
        private static double[] ReadFrameI16(BinaryReader inStream, ushort channels)
        {
            return Enumerable.Range(0, channels).Select(i =>
            {
                var s = inStream.ReadInt16();
                return s / 32768.0;
            }).ToArray();
        }

        // 24-bit signed integer samples
        private static double[] ReadFrameI24(BinaryReader inStream, ushort channels)
        {
            return Enumerable.Range(0, channels).Select(i =>
            {
                var l = inStream.ReadByte() +
                    inStream.ReadByte() << 8 +
                    inStream.ReadByte() << 16;
                if (l > 8388608) l -= 16777216;
                return l / 8388608.0;
            }).ToArray();
        }

        // 32-bit signed integer samples
        private static double[] ReadFrameI32(BinaryReader inStream, ushort channels)
        {
            return Enumerable.Range(0, channels).Select(i =>
            {
                var s = inStream.ReadInt32();
                return s / 2147483648.0;
            }).ToArray();
        }

        // 32-bit float samples
        private static double[] ReadFrameF32(BinaryReader inStream, ushort channels)
        {
            return Enumerable.Range(0, channels)
                             .Select(i => (double)inStream.ReadSingle())
                             .ToArray();
        }

        // 64-bit float samples
        private static double[] ReadFrameF64(BinaryReader inStream, ushort channels)
        {
            return Enumerable.Range(0, channels)
                             .Select(i => inStream.ReadDouble())
                             .ToArray();
        }

        private static void WriteFrameI8(BinaryWriter outWriter, IEnumerable<double> samples)
        {
            foreach (var sample in samples)
            {
                var b = (byte)(sample * 128.0 + 127.0);
                outWriter.Write(b);
            }
        }

        private static void WriteFrameI16(BinaryWriter outWriter, IEnumerable<double> samples)
        {
            foreach (var s in samples.Select(sample => (short)(sample * 32768.0)))
            {
                outWriter.Write(s);
            }
        }

        private static void WriteFrameI32(BinaryWriter outWriter, IEnumerable<double> samples)
        {
            foreach (var s in samples.Select(sample => (long)(sample * 2147483648.0)))
            {
                outWriter.Write(s);
            }
        }

        private static void WriteFrameF32(BinaryWriter outWriter, IEnumerable<double> samples)
        {
            foreach (var sample in samples)
            {
                outWriter.Write((float)sample);
            }
        }

        private static void WriteFrameF64(BinaryWriter outWriter, IEnumerable<double> samples)
        {
            foreach (var sample in samples)
            {
                outWriter.Write(sample);
            }
        }

        private static double[] ApplyMatrix(double[] sample, ushort outChannels)
        {
            return outChannels == 1 ? 
                new[] { sample.Average() } : 
                new[] { sample.First(), sample.Last() };
        }

        private static void ExpectAscii(BinaryReader stream, string str)
        {
            var expectedBytes = Encoding.ASCII.GetBytes(str);
            var actualBytes = new byte[str.Length];
            stream.Read(actualBytes, 0, actualBytes.Length);

            if (!expectedBytes.SequenceEqual(actualBytes))
            {
                throw new FormatException("Expected '" + str + "'");
            }
        }

        private static void WriteAscii(BinaryWriter stream, string str)
        {
            var bytes = Encoding.ASCII.GetBytes(str);
            stream.Write(bytes);
        }
    }
}