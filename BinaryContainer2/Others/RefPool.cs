using BinaryContainer2.Container;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace BinaryContainer2.Others
{
	public class RefPool
	{
		private int _index = -1;
		private readonly ConcurrentDictionary<Object, int> _mapIndex = new();
		private readonly ConcurrentDictionary<int, Object> _mapObject = new();

		public int FindIndex(Object obj)
		{
			if (_mapIndex.TryGetValue(obj, out var index)) return index;
			return -1;
		}

		public bool AddObject(Object obj)
		{
			// Chỉ cấp phát index nếu đối tượng là mới.
			if (!_mapIndex.ContainsKey(obj))
			{
				// Lấy index mới an toàn. Interlocked.Increment trả về giá trị sau khi tăng.
				int indexToAssign = Interlocked.Increment(ref _index);

				// Cố gắng thêm đối tượng vào _mapIndex với index đã tính. 
				// Nếu một luồng khác đã thêm đối tượng này sau khi ta kiểm tra ContainsKey, 
				// TryAdd sẽ thất bại và trả về false.
				if (_mapIndex.TryAdd(obj, indexToAssign))
				{
					// Nếu thành công, thêm vào _mapObject
					_mapObject.TryAdd(indexToAssign, obj);
					return true;
				}
				return false;
			}

			return false;
		}

		public Object? GetObject(int index)
		{
			if (_mapObject.TryGetValue(index, out object obj))
			{
				return obj;
			}
			else
			{
				return null;
			}
		}

		public bool Write(DataContainer container, object data)
		{
			var foundIndex = FindIndex(data);
			container.Flags.Add(foundIndex != -1);

			if (foundIndex != -1)
			{
				container.AddNumber(foundIndex);
				return true;
			}

			return false;
		}

		public object? Read(DataContainer container)
		{
			var foundRef = container.Flags.Read();
			if (foundRef == true)
			{
				var index = container.ReadNumber();
				var refObject = GetObject(index);
				return refObject;
			}

			return null;
		}
	}
}
