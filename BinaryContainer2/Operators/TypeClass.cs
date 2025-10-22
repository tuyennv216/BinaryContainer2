using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using BinaryContainer2.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BinaryContainer2.Operators
{
	public class TypeClass : TypeOperatorBase
	{
		public List<TypeBinding>? Bindings { get; private set; }

		public TypeClass(Type type)
		{
			Raw = type;
		}

		public override void Build()
		{
			if (IsBuilded) return;
			IsBuilded = true;
			Follows = new();
			Bindings = new();

			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

			var fields = Raw!.GetFields(flags);
			var props = Raw!.GetProperties(flags);

			Array.Sort(fields, (u, v) =>  u.Name.CompareTo(v.Name));
			Array.Sort(props, (u, v) =>  u.Name.CompareTo(v.Name));

			foreach (var field in fields)
			{
				if (!field.IsDefined(typeof(SkipContainerAttribute), true))
				{
					var op = TypeOperators.Instance.GetOperator(field.FieldType);

					Follows!.Add(op);
					Bindings!.Add(new TypeBinding
					{
						GetValue = field.GetValue,
						SetValue = field.SetValue,
					});
				}
			}

			foreach (var prop in props)
			{
				if (!prop.IsDefined(typeof(SkipContainerAttribute), true) && prop.CanRead && prop.CanWrite)
				{
					var op = TypeOperators.Instance.GetOperator(prop.PropertyType);

					Follows!.Add(op);
					Bindings!.Add(new TypeBinding
					{
						GetValue = prop.GetValue,
						SetValue = prop.SetValue,
					});
				}
			}

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
				}

				var dataType = data.GetType();
				var sameType = Raw == dataType;
				container.Flags.Add(sameType);

				if (sameType)
				{
					if (container.Settings.Is(Settings.Using_RefPool, true))
					{
						refPool.AddObject(data);
					}

					for (var i = 0; i < Follows!.Count; i++)
					{
						var value = Bindings![i].GetValue!(data);
						Follows[i].Write(container, value, refPool);
					}
				}
				else
				{
					var typeStr = dataType.AssemblyQualifiedName;
					var strOp = TypeOperators.Instance.GetOperator<string>();
					strOp.Write(container, typeStr, refPool);

					if (container.Settings.Is(Settings.Using_RefPool, true))
					{
						refPool.AddObject(data);
					}

					var op = (TypeClass)TypeOperators.Instance.GetOperator(dataType);
					for (var i = 0; i < op.Follows!.Count; i++)
					{
						var value = op.Bindings![i].GetValue!(data);
						op.Follows![i].Write(container, value, refPool);
					}
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

			var sameType = container.Flags.Read();
			if (sameType == true)
			{
				var customClass = Activator.CreateInstance(Raw);

				if (container.Settings.Is(Settings.Using_RefPool, true))
				{
					refPool.AddObject(customClass);
				}

				for (var i = 0; i < Follows!.Count; i++)
				{
					var value = Follows[i].Read(container, refPool);
					Bindings![i].SetValue!(customClass, value!);
				}
				return customClass;
			}
			else
			{
				var strOp = TypeOperators.Instance.GetOperator<string>();
				var dataTypeStr = strOp.Read(container, refPool);
				var dataType = Type.GetType((string)dataTypeStr!);

				var customClass = Activator.CreateInstance(dataType);

				if (container.Settings.Is(Settings.Using_RefPool, true))
				{
					refPool.AddObject(customClass);
				}

				var op = (TypeClass)TypeOperators.Instance.GetOperator(dataType);

				for (var i = 0; i < op.Follows!.Count; i++)
				{
					var value = op.Follows![i].Read(container, refPool);
					op.Bindings![i].SetValue!(customClass, value!);
				}

				return customClass;
			}
		}
	}
}
