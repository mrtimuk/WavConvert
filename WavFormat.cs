using System;

namespace WavConvert
{
    public class WavFormat
    {
        public bool Error;

        public WavFormatCode Format;
        public ushort BitDepth;
        public ushort Channels;

        public uint Samples;

        private uint _sampleRate;
        public uint SampleRate
        {
            get { return _sampleRate; }
            set
            {
                if (value == 0)
                {
                    throw new ArgumentOutOfRangeException("SampleRate", value, "SampleRate must be positive");
                }
                _sampleRate = value;
            }
        }

        public string ChannelString
        {
            get { 
                return Channels == 1 ? "Mono" : 
                    Channels == 2 ? "Stereo" : 
                    Channels == 3 ? "2.1" : 
                    Channels == 6 ? "5.1" : 
                    Channels.ToString(); 
            }
        }

        public ushort BlockAlign
        {
            get { return (ushort)(Channels*BitDepth/8); }
        }

        public uint ByteRate
        {
            get { return SampleRate*BlockAlign; }
        }

        public uint DataLength
        {
            get { return Samples*BlockAlign; }
        }

        public void SetDataLength(uint dataLengthBytes)
        {
           Samples = BlockAlign == 0 ? 0 : dataLengthBytes / BlockAlign;
        }
    }
}