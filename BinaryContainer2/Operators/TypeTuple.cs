using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using BinaryContainer2.Types;
using System;
using System.Reflection;

namespace BinaryContainer2.Operators
{
	public class TypeTuple : TypeOperatorBase
	{
		public TypeTuple(Type type)
		{
			Raw = type;
		}

		public override void Build()
		{
			if (IsBuilded) return;
			IsBuilded = true;
			Follows = new();

			for (var i = 0; i < Raw!.GenericTypeArguments.Length; i++)
			{
				Follows!.Add(TypeOperators.Instance.GetOperator(Raw!.GenericTypeArguments[i]));
			}

			BuildCompleteSignal.Set();
		}

		public override void Write(DataContainer container, object? data, RefPool refPool)
		{
			BuildCompleteSignal.Wait();

			container.Flags.Add(data == null);
			if (data != null)
			{
				for (var i = 0; i < Follows!.Count; i++)
				{
					string propertyName = $"Item{i + 1}";
					PropertyInfo? item = Raw!.GetProperty(propertyName);
					object? value = item.GetValue(data);
					Follows[i].Write(container, value, refPool);
				}
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			BuildCompleteSignal.Wait();

			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			var argsLength = Raw!.GenericTypeArguments.Length;
			object?[] constructorArgs = new object[argsLength];
			for (var i = 0; i < argsLength; i++)
			{
				constructorArgs[i] = Follows![i].Read(container, refPool);
			}

			var value = Activator.CreateInstance(Raw, constructorArgs);

			return value;
		}
	}
}
