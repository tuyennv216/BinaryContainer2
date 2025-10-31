using BinaryContainer2.Converter;
using BinaryContainer2.Utilities.Datamodel;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace BinaryContainer2.Utilities
{
	public class UndoRedo
	{
		private readonly TargetMember _member;
		private readonly ConcurrentDictionary<int, HistoryItem> _history = new();
		private readonly SementIndexes _indexes;
		private readonly object _lock = new object();

		public UndoRedo(object wrapper, string memberName, int maxStep)
		{
			if (wrapper == null) throw new ArgumentNullException(nameof(wrapper));
			if (maxStep <= 0) maxStep = 1;

			_member = new TargetMember(wrapper, memberName);

			_indexes = new SementIndexes(maxStep);
		}

		public void Commit()
		{
			lock (_lock)
			{
				_indexes.NewIndex();
				_history[_indexes.WorkingIndex] = CreateHistoryItem();
			}
		}

		public void Undo()
		{
			lock (_lock)
			{
				if (_indexes.Use == 0) return;

				var (type, bytes) = GetMemberInfo();
				var latest = _history[_indexes.WorkingIndex];

				if (latest.Data == bytes || latest.Data?.SequenceEqual(bytes) == true)
				{
					_indexes.MovePrevious();
				}

				Restore();
			}
		}

		public void Redo()
		{
			lock (_lock)
			{
				if (_indexes.Use == 0) return;

				_indexes.MoveNext();
				Restore();
			}
		}

		private (Type, byte[]?) GetMemberInfo()
		{
			var value = _member.GetValue();
			if (value == null) return (typeof(Object), null);
			return (value.GetType(), BinConverter.GetBytes(value));
		}

		private HistoryItem CreateHistoryItem()
		{
			var (type, bytes) = GetMemberInfo();

			if (bytes == null)
				return HistoryItem.NullItem;

			return new HistoryItem
			{
				IsNull = false,
				Name = type.AssemblyQualifiedName,
				Data = bytes,
			};
		}

		private void Restore()
		{
			var historyItem = _history[_indexes.WorkingIndex];
			if (historyItem.IsNull)
			{
				_member.SetValue(null);
			}
			else
			{
				var memberType = Type.GetType(historyItem.Name);
				var value = BinConverter.GetItem(memberType, historyItem.Data);
				_member.SetValue(value);
			}
		}
	}
}
