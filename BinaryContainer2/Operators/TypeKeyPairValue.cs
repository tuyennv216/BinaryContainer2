using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using BinaryContainer2.Types;
using System;
using System.Collections;
using System.Reflection;

namespace BinaryContainer2.Operators
{
	public class TypeKeyPairValue : TypeOperatorBase
	{
		public TypeKeyPairValue(Type type)
		{
			Raw = type;
		}

		public override void Build()
		{
			if (IsBuilded) return;
			IsBuilded = true;
			Follows = new();

			Follows.Add(TypeOperators.Instance.GetOperator(Raw!.GenericTypeArguments[0]));
			Follows.Add(TypeOperators.Instance.GetOperator(Raw!.GenericTypeArguments[1]));

			BuildCompleteSignal.Set();
		}

		public override void Write(DataContainer container, object? data, RefPool refPool)
		{
			BuildCompleteSignal.Wait();

			container.Flags.Add(data == null);
			if (data != null)
			{
				PropertyInfo? keyProp = Raw!.GetProperty("Key");
				PropertyInfo? valueProp = Raw!.GetProperty("Value");

				var key = keyProp.GetValue(data);
				var value = valueProp.GetValue(data);

				// Ghi Key và Value
				Follows![0].Write(container, key, refPool);
				Follows![1].Write(container, value, refPool);
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			BuildCompleteSignal.Wait();

			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			var key = Follows![0].Read(container, refPool);
			var value = Follows![1].Read(container, refPool);

			var kpv = Activator.CreateInstance(Raw, key, value);
			return kpv;
		}
	}
}
