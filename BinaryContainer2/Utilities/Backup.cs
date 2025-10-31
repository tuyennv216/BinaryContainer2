using BinaryContainer2.Converter;
using BinaryContainer2.Utilities.Datamodel;
using System;
using System.Collections.Concurrent;

namespace BinaryContainer2.Utilities
{
	public class Backup
	{
		private readonly ConcurrentDictionary<int, HistoryItem> _history = new();
		private readonly int _backupSize;
		private int _currentIndex = -1;
		private int _itemCount = 0;
		private readonly object _lock = new object();

		public Backup(int backupSize)
		{
			if (backupSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(backupSize));

			_backupSize = backupSize;
		}

		public void Add<T>(T? data)
		{
			lock (_lock)
			{
				_currentIndex = (_currentIndex + 1) % _backupSize;

				var historyItem = CreateHistoryItem(data);
				_history.AddOrUpdate(_currentIndex, historyItem, (key, existing) => historyItem);

				_itemCount = Math.Min(_itemCount + 1, _backupSize);
			}
		}

		public T? Get<T>(int stepsBack = 0)
		{
			var value = Get(stepsBack);
			return (T?)value;
		}

		public object? Get(int stepsBack = 0)
		{
			lock (_lock)
			{
				if (stepsBack < 0 || stepsBack >= _backupSize || stepsBack >= _itemCount)
					return null;

				int targetIndex = (_currentIndex - stepsBack + _backupSize) % _backupSize;

				if (_history.TryGetValue(targetIndex, out HistoryItem historyItem))
				{
					if (historyItem.IsNull)
						return null;

					var memberType = Type.GetType(historyItem.Name);
					if (memberType == null)
						return null;

					return BinConverter.GetItem(memberType, historyItem.Data);
				}

				return null;
			}
		}

		private HistoryItem CreateHistoryItem(object? data)
		{
			if (data == null)
				return HistoryItem.NullItem;

			var bytes = BinConverter.GetBytes(data);
			return new HistoryItem
			{
				IsNull = false,
				Name = data.GetType().AssemblyQualifiedName,
				Data = bytes
			};
		}

		public int Count => _itemCount;
	}
}