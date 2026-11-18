using System;
using System.IO;
using System.Text;
using AsarSharp.Utils;

namespace WandEnhancer.Core.Patching.Shared
{
	internal static class ElectronFuseWire
	{
		public const byte StateRemoved = 114;

		public const int SentinelLength = 32;

		private const int WireHeaderLength = 2;

		private const int AsarIntegrityIndex = 4;

		public const int StateFromSentinel = 38;

		public const int MatchLength = 39;

		private const byte SupportedWireVersion = 1;

		private const int MinFuseCount = 5;

		private const int ChunkSize = 1048576;

		private static readonly byte[] Sentinel = Encoding.ASCII.GetBytes("dL7pKGdnNz796PbbjQWNKmHXBZaB9tsX");

		public static long FindStateRva(string exePath)
		{
			using (FileStream stream = OpenShared(exePath))
			{
				long num = FindStateOffset(stream);
				return (num < 0) ? (-1) : PeImage.FileOffsetToRva(stream, num);
			}
		}

		public static long FindStateFileOffset(string exePath)
		{
			using (FileStream stream = OpenShared(exePath))
			{
				return FindStateOffset(stream);
			}
		}

		public static bool BlockLooksValid(byte[] block, int offset)
		{
			if (MatchesSentinel(block, offset) && block[offset + 32] == 1)
			{
				return block[offset + 32 + 1] >= 5;
			}
			return false;
		}

		private static FileStream OpenShared(string exePath)
		{
			return new FileStream(exePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 1048576, FileOptions.SequentialScan);
		}

		private static long FindStateOffset(Stream stream)
		{
			byte[] array = new byte[1048615];
			long num = 0L;
			int num2 = 0;
			while (true)
			{
				num2 += stream.ReadFull(array, num2, array.Length - num2);
				if (num2 < 39)
				{
					return -1L;
				}
				int num3 = num2 - 39;
				for (int i = 0; i <= num3; i++)
				{
					if (array[i] == Sentinel[0] && BlockLooksValid(array, i))
					{
						return num + i + 38;
					}
				}
				if (num2 < array.Length)
				{
					break;
				}
				Buffer.BlockCopy(array, num3, array, 0, 39);
				num += num3;
				num2 = 39;
			}
			return -1L;
		}

		private static bool MatchesSentinel(byte[] buffer, int offset)
		{
			for (int i = 0; i < 32; i++)
			{
				if (buffer[offset + i] != Sentinel[i])
				{
					return false;
				}
			}
			return true;
		}
	}
}
