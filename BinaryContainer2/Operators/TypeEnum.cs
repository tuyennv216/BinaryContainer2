using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using BinaryContainer2.Types;
using System;

namespace BinaryContainer2.Operators
{
	public class TypeEnum : TypeOperatorBase
	{
		public TypeEnum(Type type)
		{
			Raw = type;
		}

		public override void Build()
		{
			if (IsBuilded) return;
			IsBuilded = true;
			Follows = new();

			var underType = Enum.GetUnderlyingType(Raw!);
			Follows!.Add(TypeOperators.Instance.GetOperator(underType));

			BuildCompleteSignal.Set();
		}

		public override void Write(DataContainer container, object? data, RefPool refPool)
		{
			BuildCompleteSignal.Wait();

			container.Flags.Add(data == null);
			if (data != null)
			{
				var underValue = Convert.ChangeType(data, Follows![0].Raw);
				Follows[0].Write(container, underValue, refPool);
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			BuildCompleteSignal.Wait();

			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			var underValue = Follows![0].Read(container, refPool);
			var enumValue = Enum.ToObject(Raw, underValue);
			return enumValue;
		}
	}
}
