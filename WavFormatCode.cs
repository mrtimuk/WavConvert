﻿// From MMREG.H

namespace WavConvert
{
    public enum WavFormatCode : ushort
    {
        Pcm =  0x0001,
        Adpcm = 0x0002,
        Ieee754 = 0x0003,
        IbmCvsd = 0x0005,
        Alaw = 0x0006,
        Mulaw = 0x0007,
        OkiAdpcm = 0x0010,
        DviAdpcm = 0x0011,
        MediaspaceAdpcm = 0x0012,
        SierraAdpcm = 0x0013,
        G723Adpcm = 0x0014,
        Digistd = 0x0015,
        Digifix = 0x0016,
        DialogicOkiAdpcm = 0x0017,
        MediavisionAdpcm = 0x0018,
        YamahaAdpcm = 0x0020,
        Sonarc = 0x0021,
        DspgroupTruespeech = 0x0022,
        Echosc1 = 0x0023,
        AudiofileAf36 = 0x0024,
        Aptx = 0x0025,
        AudiofileAf10 = 0x0026,
        DolbyAc2 = 0x0030,
        Gsm610 = 0x0031,
        Msnaudio = 0x0032,
        AntexAdpcme = 0x0033,
        ControlResVqlpc = 0x0034,
        Digireal = 0x0035,
        Digiadpcm = 0x0036,
        ControlResCr10 = 0x0037,
        NmsVbxadpcm = 0x0038,
        CsImaadpcm = 0x0039,
        Echosc3 = 0x003A,
        RockwellAdpcm = 0x003B,
        RockwellDigitalk = 0x003C,
        Xebec = 0x003D,
        G721Adpcm = 0x0040,
        G728Celp = 0x0041,
        Mpeg = 0x0050,
        MpegLayer3 = 0x0055,
        Cirrus = 0x0060,
        Espcm = 0x0061,
        Voxware = 0x0062,
        CanopusAtrac = 0x0063,
        G726Adpcm = 0x0064,
        G722Adpcm = 0x0065,
        Dsat = 0x0066,
        DsatDisplay = 0x0067,
        Softsound = 0x0080,
        RhetorexAdpcm = 0x0100,
        CreativeAdpcm = 0x0200,
        CreativeFastspeech8 = 0x0202,
        CreativeFastspeech10 = 0x0203,
        Quarterdeck = 0x0220,
        FmTownsSnd = 0x0300,
        BtvDigital = 0x0400,
        Oligsm = 0x1000,
        Oliadpcm = 0x1001,
        Olicelp = 0x1002,
        Olisbc = 0x1003,
        Oliopr = 0x1004,
        LhCodec = 0x1100,
        Norris = 0x1400, 
        Extensible = 0xfffe
    }
}