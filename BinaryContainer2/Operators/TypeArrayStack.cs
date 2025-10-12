using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using BinaryContainer2.Types;
using System;
using System.Collections;

namespace BinaryContainer2.Operators
{
	/// <summary>
	/// Gồm List, Stack, Queue, HashSet, LinkedList
	/// </summary>
	public class TypeArrayStack : TypeOperatorBase
	{
		public TypeArrayStack(Type type)
		{
			Raw = type;
		}

		public override void Build()
		{
			if (IsBuilded) return;
			IsBuilded = true;

			Follows = new();
			var underType = Raw!.GenericTypeArguments[0];
			Follows!.Add(TypeOperators.Instance.GetOperator(underType));

			BuildCompleteSignal.Set();
		}

		public override void Write(DataContainer container, object? data, RefPool refPool)
		{
			BuildCompleteSignal.Wait();
			container.Flags.Add(data == null);
			if (data == null) return;

			var isany = ((IEnumerable)data).GetEnumerator().MoveNext();
			container.Flags.Add(isany);

			if (isany)
			{
				if (refPool.Write(container, data)) return;

				refPool.AddObject(data);

				container.AddTempBytes(4);
				var length = 0;
				foreach (var item in (IEnumerable)data)
				{
					Follows![0].Write(container, item, refPool);
					length++;
				}
				container.SetTempBytes(BitConverter.GetBytes(length));
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			BuildCompleteSignal.Wait();
			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			var isany = container.Flags.Read();
			if (isany == false) return Activator.CreateInstance(Raw);

			var refObject = refPool.Read(container);
			if (refObject != null) return refObject;

			var lengthBytes = container.ReadItems(4);
			var length = BitConverter.ToInt32(lengthBytes, 0);

			var array = Array.CreateInstance(Follows![0].Raw, length!);
			refPool.AddObject(array);

			for (var i = 0; i < length; i++)
			{
				var item = Follows![0].Read(container, refPool);
				array.SetValue(item, i);
			}

			Array.Reverse(array);

			var likeArray = Activator.CreateInstance(Raw, array);

			return likeArray;
		}
	}
}