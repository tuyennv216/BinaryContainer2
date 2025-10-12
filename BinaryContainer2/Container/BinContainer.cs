using BinaryContainer2.Abstraction;
using System;
using System.Collections.Generic;

namespace BinaryContainer2.Container
{
	/// <summary>
	/// Lưu và đọc dữ liệu binary
	/// </summary>
	public class BinContainer : IContainer<byte>
	{
		public int TotalExportBytes
		{
			get
			{
				if (Data.Count == 0)
				{
					return 4; // TotalExportBytes int size
				}

				// TotalExportBytes int size + Offset byte size + Data byte array
				return 4 + 1 + Data.Count;
			}
		}

		// Serialization data
		public int Offset;
		public List<byte> Data;
		// End Serialization data

		/// <summary>
		/// Số lượng byte dữ liệu Data
		/// </summary>
		public int Total;
		/// <summary>
		/// Byte cuối cùng đang làm việc
		/// </summary>
		public int Template;

		// Read
		public int ReadItor;
		public int ReadOffset;
		public int ReadTotal;
		// End Read

		/// <summary>
		/// Khởi tạo
		/// </summary>
		public BinContainer()
		{
			Data = new List<byte>();
		}

		/// <summary>
		/// Thêm một bit dữ liệu
		/// </summary>
		public void Add(bool data)
		{
			if (Offset >= 8 || Total == 0)
			{
				Data.Add(0);
				Offset = 0;
				Template = 0;
			}

			if (data)
			{
				Template |= 1 << Offset;
				Data[Data.Count - 1] = (byte)Template;
			}

			Offset++;
			Total++;
		}

		/// <summary>
		/// Thêm một mảng bit dữ liệu
		/// </summary>
		public void AddArray(bool[] data)
		{
			foreach (bool item in data)
				Add(item);
		}

		/// <summary>
		/// Đọc một bit dữ liệu
		/// </summary>
		public bool? Read()
		{
			if (ReadTotal >= Total) return null;
			if (ReadOffset >= 8)
			{
				ReadItor++;
				ReadOffset = 0;
			}

			var data = (Data[ReadItor] & (1 << ReadOffset)) != 0;
			ReadOffset++;
			ReadTotal++;
			return data;
		}

		/// <summary>
		/// Đọc một bit dữ liệu, không tăng vị trí đọc
		/// </summary>
		/// <returns></returns>
		public bool? Scan()
		{
			if (ReadTotal >= Total) return null;
			var data = (Data[ReadItor] & (1 << ReadOffset)) != 0;
			return data;
		}

		/// <summary>
		/// Đọc một mảng bit dữ liệu
		/// </summary>
		public bool?[] ReadArray(int length)
		{
			var data = new bool?[length];
			for (int i = 0; i < length; i++)
				data[i] = Read();

			return data;
		}

		public bool? ReadAt(int index)
		{
			if (index < 0 || index >= Total) return null;
			var readItor = index / 8;
			var readOffset = index % 8;

			var data = (Data[readItor] & (1 << readOffset)) != 0;
			return data;
		}

		public void WriteAt(int index, bool value)
		{
			if (index < 0 || index > Total) return;

			var readItor = index / 8;
			var readOffset = index % 8;

			if (value)
				Data[readItor] |= (byte)(1 << readOffset);
			else
				Data[readItor] &= (byte)~(1 << readOffset);
		}

		/// <summary>
		/// Reset thông tin đọc
		/// </summary>
		public void ReadReset()
		{
			ReadTotal = 0;
			ReadItor = 0;
			ReadOffset = 0;
		}

		/// <summary>
		/// Làm rỗng container
		/// </summary>
		public void Clear()
		{
			Total = 0;
			Offset = 0;
			Template = 0;
			Data.Clear();
			ReadReset();
		}

		/// <summary>
		/// Xuất dữ liệu từ container
		/// </summary>
		public byte[] Export()
		{
			byte[] lengthData = BitConverter.GetBytes(this.TotalExportBytes);
			byte offset = ((byte)this.Offset);
			if (this.Data.Count == 0)
				return [.. lengthData];
			else
				return [.. lengthData, offset, .. Data];
		}

		/// <summary>
		/// Nhập dữ liệu vào container
		/// </summary>
		/// <param name="buffer">Mảng dữ liệu</param>
		/// <param name="start">Vị trí bắt đầu</param>
		public int Import(byte[] buffer, int start = 0)
		{
			Clear();

			var totalBytes = BitConverter.ToInt32(buffer, start);

			if (totalBytes == 4)
			{
				return start + 4;
			}

			var offset_Index = start + 4;
			var data_Index = offset_Index + 1;

			Offset = buffer[offset_Index];
			var dataSize = totalBytes - 4 - 1;
			if (dataSize > 0)
			{
				var data = new byte[dataSize];
				Buffer.BlockCopy(buffer, data_Index, data, 0, dataSize);

				Data.AddRange(data);
				Template = Data[dataSize - 1];
				Total = ((dataSize - 1) * 8) + Offset;
			}

			return start + totalBytes;
		}
	}
}
