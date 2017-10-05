# GZipZipper
console C# zipp application based on GZip
<br /><b>version 0.0.1.0</b> <br /> <br />
Multithreading zipping algorithm: <br />
  * Separated thread consequentially read the file by sized blocks and push them to source data queue. <br />
  * According to CPU cores count creates threads, which take data blocks from the queue of source data, archived them with gzip, enumerate them and push to archive data queue. <br />
  * Because of fragmented zip thread impossible to unzip in an unusual way, information about blocks length writes in addition. <br />
<br /> *Idea:* <br />
   If we compress bad compressed data, it adds a flag to indicate if we would or not to decompress data. <br /> 
   Add a separate thread to check next data block in the queue and if it found to push it to the outgoing thread. <br />
<br /> Note: <br />
  - Paralleling decompression wasnt inmplemented, because this operation uses very rare in working with backups. If its needed, imlements trivial base on algorithm of parallel compressing. <br />
<br /><b>version 0.0.2.0</b> <br /> <br />
  * Created separated classes for compressor's working threads. <br />
  * Sleeps in threads replaced with auto reset events. <br /> 
  * Added exception handles in work thread classes. <br />
  * Many variable in compressor class were make static for more convinient using.	<br />
<br /> *Idea:* <br />
  To add in thread compressor class counter, to check if we have enough memory for performing. <br />
