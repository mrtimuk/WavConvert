using System;
using System.IO;

namespace PcmConvert
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new[] { 
                    "e:\\Documents\\Visual Studio 2013\\Projects\\PcmConvert\\nervous.wav",
                    "e:\\Documents\\Visual Studio 2013\\Projects\\PcmConvert\\out.wav" 
                };
            }

            if (args.Length < 1)
            {
                Console.Write("\nUsage:\n  {0} <inputfile> [<outputfile>]\n", Environment.CommandLine);
                return;
            }

            var inStream = File.OpenRead(args[0]);

            Stream outStream = null;
            if (args.Length == 2) 
            {
                outStream = File.OpenWrite(args[1]);
            }

            ushort channels = 1;
            ushort bitDepth = 32;
            uint sampleRate = 11025; 

            var wc = new WavConvert();
            wc.Convert(inStream, outStream, channels, bitDepth, WavFormat.PCM, sampleRate);
        }
    }
}