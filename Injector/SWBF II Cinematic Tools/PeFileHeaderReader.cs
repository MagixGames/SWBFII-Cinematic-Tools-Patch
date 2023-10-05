using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SWBF_II_Cinematic_Tools
{
	// Token: 0x02000003 RID: 3
	public class PeFileHeaderReader
	{
		// Token: 0x06000012 RID: 18 RVA: 0x0000269C File Offset: 0x0000089C
		public PeFileHeaderReader(string path)
		{
			this.Path = path;
			using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				BinaryReader binaryReader = new BinaryReader(fileStream);
				PeFileHeaderReader.IMAGE_DOS_HEADER image_DOS_HEADER = PeFileHeaderReader.FromBinaryReader<PeFileHeaderReader.IMAGE_DOS_HEADER>(binaryReader);
				binaryReader.BaseStream.Position = fileStream.Seek((long)((ulong)image_DOS_HEADER.e_lfanew), SeekOrigin.Begin) + 4L;
				this._fileHeader = PeFileHeaderReader.FromBinaryReader<PeFileHeaderReader.IMAGE_FILE_HEADER>(binaryReader);
			}
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000013 RID: 19 RVA: 0x00002714 File Offset: 0x00000914
		// (set) Token: 0x06000014 RID: 20 RVA: 0x0000271C File Offset: 0x0000091C
		public string Path { get; private set; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000015 RID: 21 RVA: 0x00002725 File Offset: 0x00000925
		public PeFileHeaderReader.IMAGE_FILE_HEADER FileHeader
		{
			get
			{
				return this._fileHeader;
			}
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00002730 File Offset: 0x00000930
		public static T FromBinaryReader<T>(BinaryReader reader) where T : struct
		{
			GCHandle gchandle = GCHandle.Alloc(reader.ReadBytes(Marshal.SizeOf(typeof(T))), GCHandleType.Pinned);
			T result = (T)((object)Marshal.PtrToStructure(gchandle.AddrOfPinnedObject(), typeof(T)));
			gchandle.Free();
			return result;
		}

		// Token: 0x06000017 RID: 23 RVA: 0x0000277C File Offset: 0x0000097C
		public string GetTimeDateStampAsHexString()
		{
			return this.FileHeader.TimeDateStamp.ToString("X");
		}

		// Token: 0x0400000B RID: 11
		private readonly PeFileHeaderReader.IMAGE_FILE_HEADER _fileHeader;

		// Token: 0x02000009 RID: 9
		public struct IMAGE_DOS_HEADER
		{
			// Token: 0x04000014 RID: 20
			public ushort e_magic;

			// Token: 0x04000015 RID: 21
			public ushort e_cblp;

			// Token: 0x04000016 RID: 22
			public ushort e_cp;

			// Token: 0x04000017 RID: 23
			public ushort e_crlc;

			// Token: 0x04000018 RID: 24
			public ushort e_cparhdr;

			// Token: 0x04000019 RID: 25
			public ushort e_minalloc;

			// Token: 0x0400001A RID: 26
			public ushort e_maxalloc;

			// Token: 0x0400001B RID: 27
			public ushort e_ss;

			// Token: 0x0400001C RID: 28
			public ushort e_sp;

			// Token: 0x0400001D RID: 29
			public ushort e_csum;

			// Token: 0x0400001E RID: 30
			public ushort e_ip;

			// Token: 0x0400001F RID: 31
			public ushort e_cs;

			// Token: 0x04000020 RID: 32
			public ushort e_lfarlc;

			// Token: 0x04000021 RID: 33
			public ushort e_ovno;

			// Token: 0x04000022 RID: 34
			public ushort e_res_0;

			// Token: 0x04000023 RID: 35
			public ushort e_res_1;

			// Token: 0x04000024 RID: 36
			public ushort e_res_2;

			// Token: 0x04000025 RID: 37
			public ushort e_res_3;

			// Token: 0x04000026 RID: 38
			public ushort e_oemid;

			// Token: 0x04000027 RID: 39
			public ushort e_oeminfo;

			// Token: 0x04000028 RID: 40
			public ushort e_res2_0;

			// Token: 0x04000029 RID: 41
			public ushort e_res2_1;

			// Token: 0x0400002A RID: 42
			public ushort e_res2_2;

			// Token: 0x0400002B RID: 43
			public ushort e_res2_3;

			// Token: 0x0400002C RID: 44
			public ushort e_res2_4;

			// Token: 0x0400002D RID: 45
			public ushort e_res2_5;

			// Token: 0x0400002E RID: 46
			public ushort e_res2_6;

			// Token: 0x0400002F RID: 47
			public ushort e_res2_7;

			// Token: 0x04000030 RID: 48
			public ushort e_res2_8;

			// Token: 0x04000031 RID: 49
			public ushort e_res2_9;

			// Token: 0x04000032 RID: 50
			public uint e_lfanew;
		}

		// Token: 0x0200000A RID: 10
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct IMAGE_FILE_HEADER
		{
			// Token: 0x04000033 RID: 51
			public ushort Machine;

			// Token: 0x04000034 RID: 52
			public ushort NumberOfSections;

			// Token: 0x04000035 RID: 53
			public uint TimeDateStamp;

			// Token: 0x04000036 RID: 54
			public uint PointerToSymbolTable;

			// Token: 0x04000037 RID: 55
			public uint NumberOfSymbols;

			// Token: 0x04000038 RID: 56
			public ushort SizeOfOptionalHeader;

			// Token: 0x04000039 RID: 57
			public ushort Characteristics;
		}
	}
}
