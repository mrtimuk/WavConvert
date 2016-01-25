using System;
using System.IO;
using System.Linq;
using System.Text;

namespace WavConvert
{
    public class WavConvert
    {
        public static void Convert(
            Stream inStream, 
            Stream outStream, 
            ushort outChannels, 
            ushort outBitDepth, 
            WavFormat outFormat,
            uint outSampleRate)
        {
            using (var inReader = new BinaryReader(inStream))
            {
                // Read header
                ExpectAscii(inReader, "RIFF");
                var fileLength = inReader.ReadUInt32() + 8;
                ExpectAscii(inReader, "WAVE");
                ExpectAscii(inReader, "fmt ");
                var fmtLength = inReader.ReadUInt32();
                var inFormat = (WavFormat) inReader.ReadUInt16();
                var inChannels = inReader.ReadUInt16();
                var inSampleRate = inReader.ReadUInt32();
                var inByteRate = inReader.ReadUInt32();
                var inBlockAlign = inReader.ReadUInt16();
                var inBitDepth = inReader.ReadUInt16();
                var inCbSize = fmtLength > 16 ? inReader.ReadUInt16() : 0;
                // TODO: read past any extra fmt and any CB bytes //
                ExpectAscii(inReader, "data");
                var inSamples = inReader.ReadUInt32();

                if (inFormat != WavFormat.PCM && inFormat != WavFormat.IEEE754) 
                {
                    throw new FormatException(
                        "File format " + inFormat + " is not supported. Only PCM and IEEE FP");
                }

                Console.WriteLine("Input file:");
                Console.WriteLine("   Format:      {0}", inFormat.ToString());
                Console.WriteLine("   Channels:    {0}", inChannels == 1 ? "Mono" : "Stereo");
                Console.WriteLine("   Bit depth:   {0} bit", inBitDepth);
                Console.WriteLine("   Sample rate: {0} KHz", inSampleRate / 1000.0);
                Console.WriteLine("   Bit rate:    {0} Kbs", inByteRate * 8 / 1000);
                Console.WriteLine("");
                Console.WriteLine("   File length: {0} bytes", fileLength);
                Console.WriteLine("   Fmt length:  {0} bytes", fmtLength);
                Console.WriteLine("   CB Size:     {0} bytes", inCbSize);
                Console.WriteLine("   Block align: {0} bytes", inBlockAlign);
                Console.WriteLine("   Data:        {0} bytes", inSamples);
                Console.WriteLine("");

                var outBlockAlign = (short)(outChannels * outBitDepth / 8);
                uint outByteRate = (uint)(outSampleRate * outBlockAlign);
                var outSamples = inSamples;

                if (outStream != null)
                {
                    using (var outWriter = new BinaryWriter(outStream))
                    {
                        Console.WriteLine("Writing output file");

                        uint outFileLength = (uint)(44 + outSamples * outBlockAlign);

                        // Write header
                        WriteAscii(outWriter, "RIFF");
                        outWriter.Write(outFileLength - 8);
                        WriteAscii(outWriter, "WAVE");
                        WriteAscii(outWriter, "fmt ");
                        outWriter.Write(16); // no CB
                        outWriter.Write((ushort)outFormat);
                        outWriter.Write(outChannels);
                        outWriter.Write(outSampleRate);
                        outWriter.Write(outByteRate);
                        outWriter.Write(outBlockAlign);
                        outWriter.Write(outBitDepth);
                        WriteAscii(outWriter, "data");
                        outWriter.Write(outSamples * outBlockAlign);

                        MoveSamples(inReader, inBitDepth, inFormat, inSampleRate, inChannels,
                                    outWriter, outBitDepth, outFormat, outSampleRate, outChannels);
                    }
                }
            }
        }

        private static void MoveSamples(
            BinaryReader inReader,  ushort inBits,  WavFormat inFormat, uint inRate,  ushort inChannels,
            BinaryWriter outWriter, ushort outBits, WavFormat outFormat, uint outRate, ushort outChannels)
        {
            while (inReader.BaseStream.Position != inReader.BaseStream.Length)
            {
                double[] inSamples = null;
                if (inFormat == WavFormat.PCM || inFormat == WavFormat.Extensible)
                {
                    switch (inBits)
                    {
                        case 8: inSamples = ReadFrameI8(inReader, inChannels); break;
                        case 16: inSamples = ReadFrameI16(inReader, inChannels); break;
                        case 24: inSamples = ReadFrameI24(inReader, inChannels); break;
                        case 32: inSamples = ReadFrameI32(inReader, inChannels); break;
                        default: throw new FormatException("Input PCM bit depth " + inBits + " is not supported");
                    }
                }
                else if (inFormat == WavFormat.IEEE754)
                {
                    switch (inBits)
                    {
                        case 32: inSamples = ReadFrameF32(inReader, inChannels); break;
                        case 64: inSamples = ReadFrameF64(inReader, inChannels); break;
                        default: throw new FormatException("Input IEEE bit depth " + inBits + " is not supported");
                    }
                }

                var outSamples = ApplyMatrix(inSamples, outChannels);

                // Resample

                if (outFormat == WavFormat.PCM)
                {
                    switch (outBits)
                    {
                        case 8: WriteFrameI8(outWriter, outSamples); break;
                        case 16: WriteFrameI16(outWriter, outSamples); break;
                        case 32: WriteFrameI32(outWriter, outSamples); break;
                        default: throw new FormatException("Output bit depth " + outBits + " is not supported");
                    }
                }
                else if (outFormat == WavFormat.IEEE754)
                {
                    switch (outBits)
                    {
                        case 32: WriteFrameF32(outWriter, outSamples); break;
                        case 64: WriteFrameF64(outWriter, outSamples); break;
                        default: throw new FormatException("Output IEEE bit depth " + inBits + " is not supported");
                    }
                }
            }
        }

        // 8-bit unsigned integer samples
        static double[] ReadFrameI8(BinaryReader inStream, ushort channels)
        {
            return Enumerable.Range(0, channels).Select(i =>
            {
                var b = inStream.ReadByte();
                return (b - 127.0) / 128.0;
            }).ToArray();
        }

        // 16-bit signed integer samples
        static double[] ReadFrameI16(BinaryReader inStream, ushort channels)
        {
            return Enumerable.Range(0, channels).Select(i =>
            {
                var s = inStream.ReadInt16();
                return s / 32768.0;
            }).ToArray();
        }

        // 24-bit signed integer samples
        static double[] ReadFrameI24(BinaryReader inStream, ushort channels)
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
        static double[] ReadFrameI32(BinaryReader inStream, ushort channels)
        {
            return Enumerable.Range(0, channels).Select(i =>
            {
                var s = inStream.ReadInt32();
                return s / 2147483648.0;
            }).ToArray();
        }

        // 32-bit float samples
        static double[] ReadFrameF32(BinaryReader inStream, ushort channels)
        {
            return Enumerable.Range(0, channels)
                             .Select(i => (double)inStream.ReadSingle())
                             .ToArray();
        }

        // 64-bit float samples
        static double[] ReadFrameF64(BinaryReader inStream, ushort channels)
        {
            return Enumerable.Range(0, channels)
                             .Select(i => inStream.ReadDouble())
                             .ToArray();
        }

        static void WriteFrameI8(BinaryWriter outWriter, double[] samples)
        {
            for (var c = 0; c < samples.Length; c++)
            {
                var b = (byte)(samples[c] * 128.0 + 127.0);
                outWriter.Write(b);
            }
        }

        static void WriteFrameI16(BinaryWriter outWriter, double[] samples)
        {
            for (var c = 0; c < samples.Length; c++)
            {
                var s = (short)(samples[c] * 32768.0);
                outWriter.Write(s);
            }
        }

        static void WriteFrameI32(BinaryWriter outWriter, double[] samples)
        {
            for (var c = 0; c < samples.Length; c++)
            {
                var s = (long)(samples[c] * 2147483648.0);
                outWriter.Write(s);
            }
        }

        static void WriteFrameF32(BinaryWriter outWriter, double[] samples)
        {
            for (var c = 0; c < samples.Length; c++)
            {
                outWriter.Write((float)samples[c]);
            }
        }

        static void WriteFrameF64(BinaryWriter outWriter, double[] samples)
        {
            for (var c = 0; c < samples.Length; c++)
            {
                outWriter.Write(samples[c]);
            }
        }

        static double[] ApplyMatrix(double[] sample, ushort outChannels)
        {
            if (outChannels == 1)
            {
                return new[] { sample.Average() };
            }
            else
            {
                return new[] { sample.First(), sample.Last() };
            }
        }

        static void ExpectAscii(BinaryReader stream, string str)
        {
            var expectedBytes = Encoding.ASCII.GetBytes(str);
            var actualBytes = new byte[str.Length];
            stream.Read(actualBytes, 0, actualBytes.Length);

            if (!expectedBytes.SequenceEqual(actualBytes))
            {
                throw new FormatException("Expected '" + str + "'");
            }
        }

        static void WriteAscii(BinaryWriter stream, string str)
        {
            var bytes = Encoding.ASCII.GetBytes(str);
            stream.Write(bytes);
        }
    }
}