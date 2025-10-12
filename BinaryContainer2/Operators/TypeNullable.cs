using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using BinaryContainer2.Types;
using System;

namespace BinaryContainer2.Operators
{
	public class TypeNullable : TypeOperatorBase
	{
		public TypeNullable(Type type)
		{
			Raw = type;
		}

		public override void Build()
		{
			if (IsBuilded) return;
			IsBuilded = true;
			Follows = new();

			Follows!.Add(TypeOperators.Instance.GetOperator(Raw!.GenericTypeArguments[0]));
			BuildCompleteSignal.Set();
		}

		public override void Write(DataContainer container, object? data, RefPool refPool)
		{
			BuildCompleteSignal.Wait();

			Follows![0].Write(container, data, refPool);
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			BuildCompleteSignal.Wait();

			var insideValue = Follows![0].Read(container, refPool);
			if (insideValue == null) return null;

			var value = Activator.CreateInstance(Raw, insideValue);

			return value;
		}
	}
}
