﻿// From MMREG.H

namespace PcmConvert
{
    public enum WavFormat : ushort
    {
        PCM =  0x0001,
        ADPCM = 0x0002,
        IEEE754 = 0x0003,
        IBM_CVSD = 0x0005,
        ALAW = 0x0006,
        MULAW = 0x0007,
        OKI_ADPCM = 0x0010,
        DVI_ADPCM = 0x0011,
        MEDIASPACE_ADPCM = 0x0012,
        SIERRA_ADPCM = 0x0013,
        G723_ADPCM = 0x0014,
        DIGISTD = 0x0015,
        DIGIFIX = 0x0016,
        DIALOGIC_OKI_ADPCM = 0x0017,
        MEDIAVISION_ADPCM = 0x0018,
        YAMAHA_ADPCM = 0x0020,
        SONARC = 0x0021,
        DSPGROUP_TRUESPEECH = 0x0022,
        ECHOSC1 = 0x0023,
        AUDIOFILE_AF36 = 0x0024,
        APTX = 0x0025,
        AUDIOFILE_AF10 = 0x0026,
        DOLBY_AC2 = 0x0030,
        GSM610 = 0x0031,
        MSNAUDIO = 0x0032,
        ANTEX_ADPCME = 0x0033,
        CONTROL_RES_VQLPC = 0x0034,
        DIGIREAL = 0x0035,
        DIGIADPCM = 0x0036,
        CONTROL_RES_CR10 = 0x0037,
        NMS_VBXADPCM = 0x0038,
        CS_IMAADPCM = 0x0039,
        ECHOSC3 = 0x003A,
        ROCKWELL_ADPCM = 0x003B,
        ROCKWELL_DIGITALK = 0x003C,
        XEBEC = 0x003D,
        G721_ADPCM = 0x0040,
        G728_CELP = 0x0041,
        MPEG = 0x0050,
        MPEGLAYER3 = 0x0055,
        CIRRUS = 0x0060,
        ESPCM = 0x0061,
        VOXWARE = 0x0062,
        CANOPUS_ATRAC = 0x0063,
        G726_ADPCM = 0x0064,
        G722_ADPCM = 0x0065,
        DSAT = 0x0066,
        DSAT_DISPLAY = 0x0067,
        SOFTSOUND = 0x0080,
        RHETOREX_ADPCM = 0x0100,
        CREATIVE_ADPCM = 0x0200,
        CREATIVE_FASTSPEECH8 = 0x0202,
        CREATIVE_FASTSPEECH10 = 0x0203,
        QUARTERDECK = 0x0220,
        FM_TOWNS_SND = 0x0300,
        BTV_DIGITAL = 0x0400,
        OLIGSM = 0x1000,
        OLIADPCM = 0x1001,
        OLICELP = 0x1002,
        OLISBC = 0x1003,
        OLIOPR = 0x1004,
        LH_CODEC = 0x1100,
        NORRIS = 0x1400, 
        Extensible = 0xfffe
    }
}