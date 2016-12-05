# WavConvert
Transcode WAV files

Usage:
<pre>
WavConvert &lt;inFile&gt;                                   Describe a WAV file
WavConvert &lt;inFile&gt; [formatOptions] &lt;outFile&gt;         Transcode a WAV file
</pre>

Where `formatOptions` can be:
<pre>
-c&lt;n&gt;        Number of channels   (default: -c2)
-b&lt;n&gt;        Sample bit depth     (default: -b16)
-s&lt;n&gt;        Sample rate          (default: -s11025)
</pre>
