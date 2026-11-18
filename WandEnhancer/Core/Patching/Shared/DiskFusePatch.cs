using System;
using System.IO;

namespace WandEnhancer.Core.Patching.Shared
{
	internal static class DiskFusePatch
	{
		private const string FuseBackupSuffix = ".fusebak";

		public static bool Apply(string exePath, Action<string, ELogType> log)
		{
			long num = ElectronFuseWire.FindStateFileOffset(exePath);
			if (num < 0)
			{
				log?.Invoke("[ENHANCER] No Electron fuse block in " + Path.GetFileName(exePath) + "; nothing to flip.", ELogType.Warn);
				return false;
			}
			using (FileStream fileStream = new FileStream(exePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
			{
				fileStream.Position = num;
				int num2 = fileStream.ReadByte();
				if (num2 == 114)
				{
					return true;
				}
				string path = exePath + ".fusebak";
				if (!File.Exists(path))
				{
					File.WriteAllBytes(path, new byte[1] { (byte)num2 });
				}
				fileStream.Position = num;
				fileStream.WriteByte(114);
			}
			log?.Invoke("[ENHANCER] ASAR integrity fuse cleared on disk in " + Path.GetFileName(exePath) + ".", ELogType.Info);
			return true;
		}

		public static void Remove(string exePath, Action<string, ELogType> log)
		{
			string path = exePath + ".fusebak";
			if (!File.Exists(path))
			{
				return;
			}
			long num = ElectronFuseWire.FindStateFileOffset(exePath);
			if (num < 0)
			{
				log?.Invoke("[ENHANCER] Could not locate the fuse to restore in " + Path.GetFileName(exePath) + "; keeping .fusebak.", ELogType.Warn);
				return;
			}
			byte value = File.ReadAllBytes(path)[0];
			using (FileStream fileStream = new FileStream(exePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
			{
				fileStream.Position = num;
				fileStream.WriteByte(value);
			}
			log?.Invoke("[ENHANCER] ASAR integrity fuse restored on disk in " + Path.GetFileName(exePath) + ".", ELogType.Info);
			File.Delete(path);
		}
	}
}
