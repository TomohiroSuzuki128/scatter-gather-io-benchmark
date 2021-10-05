using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Win32.SafeHandles;

namespace ScatterGatherIOBenchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<WriteVsWriteGather>();
        }
    }

    public class WriteVsWriteGather
    {
        const int FileSize = 100_000_000;
        string _filePath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
        byte[] _buffer = new byte[16000];

        [Benchmark]
        public void Write()
        {
            byte[] userBuffer = _buffer;
            using SafeFileHandle fileHandle = File.OpenHandle(_filePath, FileMode.Create, FileAccess.Write, FileShare.Read, FileOptions.DeleteOnClose);

            long bytesWritten = 0;
            for (int i = 0; i < FileSize / userBuffer.Length; i++)
            {
                RandomAccess.Write(fileHandle, userBuffer, bytesWritten);
                bytesWritten += userBuffer.Length;
            }
        }

        [Benchmark]
        public void WriteGather()
        {
            byte[] userBuffer = _buffer;
            IReadOnlyList<ReadOnlyMemory<byte>> buffers = new ReadOnlyMemory<byte>[] { _buffer, _buffer, _buffer, _buffer };
            using SafeFileHandle fileHandle = File.OpenHandle(_filePath, FileMode.Create, FileAccess.Write, FileShare.Read, FileOptions.DeleteOnClose);

            long bytesWritten = 0;
            for (int i = 0; i < FileSize / (userBuffer.Length * 4); i++)
            {
                RandomAccess.Write(fileHandle, buffers, bytesWritten);
                bytesWritten += userBuffer.Length * 4;
            }
        }
    }
}