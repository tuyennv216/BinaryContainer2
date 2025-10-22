using BinaryContainer2.Abstraction;
using BinaryContainer2.Others;
using System;
using System.Collections.Generic;
using System.IO;

namespace BinaryContainer2.Container
{
	public class DataContainer : IContainer<byte>
	{
		public Settings Settings { get; set; }
		public BinContainer Flags { get; set; }
		public List<byte> Items { get; set; }
		public List<byte> Arrays { get; set; }

		public int Items_Itor { get; set; }
		public int Arrays_Itor { get; set; }

		public Stack<int> TempBytesPosition { get; set; }

		// Flag Total + Items Count + Arrays Count
		public int TotalElements => Flags.Total + Items.Count + Arrays.Count;

		public int TotalExportBytes
		{
			get
			{
				if (TotalElements == 0) return 5;
				// Length (int size) + Settings size + Flags size + Items size + Arrays size
				return 4 + 1 + Flags.TotalExportBytes + Items.Count + Arrays.Count;
			}
		}

		public DataContainer(bool useRefPool = true)
		{
			Flags = new(); 
			Items = new();
			Arrays = new();

			Items_Itor = 0;
			Arrays_Itor = 0;
			TempBytesPosition = new();

			Settings = Settings.Set(Settings.Using_RefPool, useRefPool);
		}

		public void SetSetting(Settings setting, bool value)
		{
			Settings = Settings.Set(setting, value);
		}

		public void UpdateSettings()
		{
			Settings = Settings.Set(Settings.Has_Flags, Flags.Total > 0);
			Settings = Settings.Set(Settings.Has_Items, Items.Count > 0);
			Settings = Settings.Set(Settings.Has_Arrays, Arrays.Count > 0);
		}

		public void AddTempBytes(int count)
		{
			TempBytesPosition.Push(Items.Count);
			Items.AddRange(new byte[count]);
		}

		public void SetTempBytes(byte[] bytes)
		{
			var position = TempBytesPosition.Pop();
			for (var i = 0; i < bytes.Length; i++)
			{
				Items[position + i] = bytes[i];
			}
		}

		public void AddNumber(int number)
		{
			var byteNumber = byte.MinValue <= number && number <= byte.MaxValue;
			Flags.Add(byteNumber);
			if (byteNumber)
			{
				Arrays.Add((byte)number);
			}
			else
			{
				Arrays.AddRange(BitConverter.GetBytes(number));
			}
		}

		public int ReadNumber()
		{
			var byteNumber = Flags.Read();
			if (byteNumber == true)
			{
				var bytes = ReadArrays(1);
				return (int)bytes![0];
			} else
			{
				var bytes = ReadArrays(4);
				return BitConverter.ToInt32(bytes, 0);
			}
		}

		public byte[]? ReadItems(int length)
		{
			if (Items_Itor + length > Items.Count) return null;

			var result = new byte[length];
			for (var i = 0; i < length; i++)
				result[i] = Items[Items_Itor + i];

			Items_Itor += length;
			return result;
		}

		public byte[]? ReadArrays(int length)
		{
			if (Arrays_Itor + length > Arrays.Count) return null;

			var result = new byte[length];
			for (var i = 0; i < length; i++)
				result[i] = Arrays[Arrays_Itor + i];

			Arrays_Itor += length;
			return result;
		}

		public byte[] Export()
		{
			var length = this.TotalExportBytes;
			var lengthData = BitConverter.GetBytes(length);
			
			UpdateSettings();

			using (var memoryStream = new MemoryStream(length))
			using (var binaryWriter = new BinaryWriter(memoryStream))
			{
				binaryWriter.Write(lengthData);
				binaryWriter.Write((byte)Settings);
				if (Settings.Is(Settings.Has_Flags, true))
				{
					binaryWriter.Write(Flags.Export());
				}

				if (Settings.Is(Settings.Has_Items, true))
				{
					binaryWriter.Write(Items.Count);
					binaryWriter.Write(Items.ToArray());
				}

				if (Settings.Is(Settings.Has_Arrays, true))
				{
					binaryWriter.Write(Arrays.Count);
					binaryWriter.Write(Arrays.ToArray());
				}

				return memoryStream.ToArray();
			}
		}

		public int Import(byte[] buffer, int start = 0)
		{
			var length = BitConverter.ToInt32(buffer, start);

			Settings = (Settings)buffer[start + 4];

			if (length == 5)
				return start + length;

			var dataPivot = start + 5;

			if (Settings.Is(Settings.Has_Flags, true))
			{
				dataPivot = Flags.Import(buffer, dataPivot);
			}

			if (Settings.Is(Settings.Has_Items, true))
			{
				var itemsLength = BitConverter.ToInt32(buffer, dataPivot);
				var items = new byte[itemsLength];
				Buffer.BlockCopy(buffer, dataPivot + 4, items, 0, itemsLength);
				Items = new List<byte>(items);

				dataPivot += 4 + itemsLength;
			}

			if (Settings.Is(Settings.Has_Arrays, true))
			{
				var arrsLength = BitConverter.ToInt32(buffer, dataPivot);
				var arrs = new byte[arrsLength];
				Buffer.BlockCopy(buffer, dataPivot + 4, arrs, 0, arrsLength);
				Arrays = new List<byte>(arrs);

				//dataPivot += 4 + arrsLength;
			}

			return start + length;
		}
	}
}
