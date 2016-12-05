using System;
using System.IO;
using System.Linq;

namespace WavConvert
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Default arguments
            ushort channels = 2;
            ushort sampleRate = 11025;
            ushort bitDepth = 16;
            string inFile = null;
            string outFile = null;

            // Parse command line
            foreach (var arg in args)
            {
                if (arg.StartsWith("-c"))
                {
                    channels = ushort.Parse(arg.Substring(2));
                }
                else if (arg.StartsWith("-b"))
                {
                    bitDepth = ushort.Parse(arg.Substring(2));
                }
                else if (arg.StartsWith("-s"))
                {
                    sampleRate = ushort.Parse(arg.Substring(2));
                }
                else if (inFile == null)
                {
                    inFile = arg;
                }
                else if (outFile == null)
                {
                    outFile = arg;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Unexpected argument: {0}", arg);
                    Usage();
                    Environment.Exit(1);
                }
            }

            // Input
            if (inFile == null)
            {
                Console.WriteLine();
                Console.WriteLine("No input file specified!");
                Usage();
                Environment.Exit(1);
            }
            var inStream = File.OpenRead(inFile);

            // Output
            Stream outStream = null;
            if (outFile != null) 
            {
                outStream = File.OpenWrite(outFile);
            }

            // Do the convesion
            if (!WavConvert.Convert(
                inStream,
                outStream,
                new WavFormat
                {
                    Format = WavFormatCode.Pcm,
                    Channels = channels,
                    BitDepth = bitDepth,
                    SampleRate = sampleRate
                }))
            {
                Environment.Exit(1);
            }
        }

        private static void Usage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  {0} <inputfile> [[outputOptions] <outputfile>]", 
                Environment.CommandLine.Split('\\').Last());
            Console.WriteLine();
            Console.WriteLine("Output options:");
            Console.WriteLine("  -b<bitDepth>        Number of bits per sample (8, 16, or 32). Default: -b16");
            Console.WriteLine("  -s<sampleRate>      Sample rate in HZ eg: 8000, 11025, 22050. Default: -s11025");
            Console.WriteLine("  -c<channels>        1 = mono, 2 = stereo. Default: -c2");
            Console.WriteLine();
        }
    }
}