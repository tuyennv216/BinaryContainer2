using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using BinaryContainer2.Types;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BinaryContainer2.Operators
{
	public class TypeShortedList : TypeOperatorBase
	{
		public TypeShortedList(Type type)
		{
			Raw = type;
		}

		public override void Build()
		{
			if (IsBuilded) return;
			IsBuilded = true;
			Follows = new();

			var keyType = Raw!.GenericTypeArguments[0];
			Follows!.Add(TypeOperators.Instance.GetOperator(keyType));

			var valueType = Raw!.GenericTypeArguments[1];
			Follows!.Add(TypeOperators.Instance.GetOperator(valueType));

			BuildCompleteSignal.Set();
		}

		public override void Write(DataContainer container, object? data, RefPool refPool)
		{
			BuildCompleteSignal.Wait();

			container.Flags.Add(data == null);
			if (data != null)
			{
				if (container.Settings.Is(Settings.Using_RefPool, true))
				{
					if (refPool.Write(container, data)) return;
					refPool.AddObject(data);
				}

				var dictionary = (IDictionary)data;

				container.Flags.Add(dictionary.Count == 0);
				if (dictionary.Count == 0) return;

				container.AddNumber(dictionary.Count);

				var enumerator = dictionary.GetEnumerator();
				for (var i = 0; i < dictionary.Count; i++)
				{
					enumerator.MoveNext();
					var entry = (DictionaryEntry)enumerator.Current; // Lấy ra cặp Key-Value

					// Ghi Key và Value từ DictionaryEntry
					Follows![0].Write(container, entry.Key, refPool);
					Follows![1].Write(container, entry.Value, refPool);
				}
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			BuildCompleteSignal.Wait();

			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			if (container.Settings.Is(Settings.Using_RefPool, true))
			{
				var refObject = refPool.Read(container);
				if (refObject != null) return refObject;
			}

			var sortedListType = typeof(SortedList<,>).MakeGenericType(Follows![0].Raw, Follows![1].Raw);
			var sortedList = (IDictionary)Activator.CreateInstance(sortedListType);

			if (container.Settings.Is(Settings.Using_RefPool, true))
			{
				refPool.AddObject(sortedList);
			}

			var isempty = container.Flags.Read();
			if (isempty == true) return sortedList;

			var length = container.ReadNumber();

			for (var i = 0; i < length; i++)
			{
				var key = Follows![0].Read(container, refPool);
				var value = Follows[1].Read(container, refPool);

				// Add sẽ tự động sắp xếp Key trong SortedList
				sortedList.Add(key, value);
			}

			return sortedList;
		}
	}
}
