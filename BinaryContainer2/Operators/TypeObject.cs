using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using BinaryContainer2.Types;
using System;

namespace BinaryContainer2.Operators
{
	public class TypeObject : TypeOperatorBase
	{
		public TypeObject()
		{
			Raw = typeof(Object);
		}

		public override void Build()
		{
			IsBuilded = true;
			BuildCompleteSignal.Set();
		}

		public override void Write(DataContainer container, object? data, RefPool refPool)
		{
			container.Flags.Add(data == null);
			if (data != null)
			{
				var dataType = data.GetType();
				var op = TypeOperators.Instance.GetOperator(dataType);
				switch (data)
				{
					case SByte:
						container.Items.Add(TypeUtils.TypeBuildinSign.SByte);
						op.Write(container, data, refPool);
						return;

					case Byte:
						container.Items.Add(TypeUtils.TypeBuildinSign.Byte);
						op.Write(container, data, refPool);
						return;

					case Int16:
						container.Items.Add(TypeUtils.TypeBuildinSign.Int16);
						op.Write(container, data, refPool);
						return;

					case UInt16:
						container.Items.Add(TypeUtils.TypeBuildinSign.UInt16);
						op.Write(container, data, refPool);
						return;

					case Int32:
						container.Items.Add(TypeUtils.TypeBuildinSign.Int32);
						op.Write(container, data, refPool);
						return;

					case UInt32:
						container.Items.Add(TypeUtils.TypeBuildinSign.UInt32);
						op.Write(container, data, refPool);
						return;

					case Int64:
						container.Items.Add(TypeUtils.TypeBuildinSign.Int64);
						op.Write(container, data, refPool);
						return;

					case UInt64:
						container.Items.Add(TypeUtils.TypeBuildinSign.UInt64);
						op.Write(container, data, refPool);
						return;

					case Single:
						container.Items.Add(TypeUtils.TypeBuildinSign.Single);
						op.Write(container, data, refPool);
						return;

					case Double:
						container.Items.Add(TypeUtils.TypeBuildinSign.Double);
						op.Write(container, data, refPool);
						return;

					case Decimal:
						container.Items.Add(TypeUtils.TypeBuildinSign.Decimal);
						op.Write(container, data, refPool);
						return;

					case char:
						container.Items.Add(TypeUtils.TypeBuildinSign.Char);
						op.Write(container, data, refPool);
						return;

					case string:
						container.Items.Add(TypeUtils.TypeBuildinSign.String);
						op.Write(container, data, refPool);
						return;

					case bool:
						container.Items.Add(TypeUtils.TypeBuildinSign.Boolean);
						op.Write(container, data, refPool);
						return;

					case DateTime:
						container.Items.Add(TypeUtils.TypeBuildinSign.DateTime);
						op.Write(container, data, refPool);
						return;

					case DateTimeOffset:
						container.Items.Add(TypeUtils.TypeBuildinSign.DateTimeOffset);
						op.Write(container, data, refPool);
						return;

					case TimeSpan:
						container.Items.Add(TypeUtils.TypeBuildinSign.TimeSpan);
						op.Write(container, data, refPool);
						return;

					case Guid:
						container.Items.Add(TypeUtils.TypeBuildinSign.Guid);
						op.Write(container, data, refPool);
						return;
				}

				switch (dataType.BaseType?.FullName)
				{
					case TypeNames.Enum:
						{
							container.Items.Add(((byte)TypeCategory.Enum));
							var strOp = TypeOperators.Instance.GetOperator<string>();
							strOp.Write(container, dataType.AssemblyQualifiedName, refPool);
							op.Write(container, data, refPool);
							return;
						}
				}

				if (dataType == typeof(object))
				{
					container.Items.Add(TypeUtils.TypeBuildinSign.Object);
					return;
				}

				// Kiểm tra nếu type Class
				container.Items.Add(TypeUtils.TypeBuildinSign.Classes);
				container.Flags.Add(dataType.IsClass);
				if (dataType.IsClass)
				{
					var typeTarget = dataType.AssemblyQualifiedName;
					var strOp = TypeOperators.Instance.GetOperator<string>();
					strOp.Write(container, typeTarget, refPool);

					var classOp = TypeOperators.Instance.GetOperator(dataType);
					classOp.Write(container, data, refPool);
				}
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			var sign = container.ReadItems(1)![0];
			switch (sign)
			{
				case TypeUtils.TypeBuildinSign.SByte:
					{
						var op = TypeOperators.Instance.GetOperator<sbyte>();
						var value = op.Read(container, refPool);
						return value;
					}

				case TypeUtils.TypeBuildinSign.Byte:
					{
						var op = TypeOperators.Instance.GetOperator<byte>();
						var value = op.Read(container, refPool);
						return value;
					}

				case TypeUtils.TypeBuildinSign.Int16:
					{
						var op = TypeOperators.Instance.GetOperator<Int16>();
						var value = op.Read(container, refPool);
						return value;
					}

				case TypeUtils.TypeBuildinSign.UInt16:
					{
						var op = TypeOperators.Instance.GetOperator<UInt16>();
						var value = op.Read(container, refPool);
						return value;
					}

				case TypeUtils.TypeBuildinSign.Int32:
					{
						var op = TypeOperators.Instance.GetOperator<Int32>();
						var value = op.Read(container, refPool);
						return value;
					}

				case TypeUtils.TypeBuildinSign.UInt32:
					{
						var op = TypeOperators.Instance.GetOperator<UInt32>();
						var value = op.Read(container, refPool);
						return value;
					}

				case TypeUtils.TypeBuildinSign.Int64:
					{
						var op = TypeOperators.Instance.GetOperator<Int64>();
						var value = op.Read(container, refPool);
						return value;
					}

				case TypeUtils.TypeBuildinSign.UInt64:
					{
						var op = TypeOperators.Instance.GetOperator<UInt64>();
						var value = op.Read(container, refPool);
						return value;
					}

				case TypeUtils.TypeBuildinSign.Single:
					{
						var op = TypeOperators.Instance.GetOperator<Single>();
						var value = op.Read(container, refPool);
						return value;
					}

				case TypeUtils.TypeBuildinSign.Double:
					{
						var op = TypeOperators.Instance.GetOperator<Double>();
						var value = op.Read(container, refPool);
						return value;
					}

				case TypeUtils.TypeBuildinSign.Decimal:
					{
						var op = TypeOperators.Instance.GetOperator<Decimal>();
						var value = op.Read(container, refPool);
						return value;
					}

				case TypeUtils.TypeBuildinSign.Char:
					{
						var op = TypeOperators.Instance.GetOperator<char>();
						var value = op.Read(container, refPool);
						return value;
					}

				case TypeUtils.TypeBuildinSign.String:
					{
						var op = TypeOperators.Instance.GetOperator<string>();
						var value = op.Read(container, refPool);
						return value;
					}

				case TypeUtils.TypeBuildinSign.Boolean:
					{
						var op = TypeOperators.Instance.GetOperator<bool>();
						var value = op.Read(container, refPool);
						return value;
					}

				case TypeUtils.TypeBuildinSign.DateTime:
					{
						var op = TypeOperators.Instance.GetOperator<DateTime>();
						var value = op.Read(container, refPool);
						return value;
					}

				case TypeUtils.TypeBuildinSign.DateTimeOffset:
					{
						var op = TypeOperators.Instance.GetOperator<DateTimeOffset>();
						var value = op.Read(container, refPool);
						return value;
					}

				case TypeUtils.TypeBuildinSign.TimeSpan:
					{
						var op = TypeOperators.Instance.GetOperator<TimeSpan>();
						var value = op.Read(container, refPool);
						return value;
					}

				case TypeUtils.TypeBuildinSign.Guid:
					{
						var op = TypeOperators.Instance.GetOperator<Guid>();
						var value = op.Read(container, refPool);
						return value;
					}

				case TypeUtils.TypeBuildinSign.Object:
					{
						var value = Activator.CreateInstance(typeof(Object));
						return value;
					}

				case ((byte)TypeCategory.Enum):
					{
						var strOp = TypeOperators.Instance.GetOperator<string>();
						var targetTypeStr = strOp.Read(container, refPool);
						var targetType = Type.GetType((string)targetTypeStr!);
						var enumOp = TypeOperators.Instance.GetOperator(targetType);
						var value = enumOp.Read(container, refPool);
						return value;
					}

				default:
					{
						var isClass = container.Flags.Read();
						if (isClass == true)
						{
							var strOp = TypeOperators.Instance.GetOperator<string>();
							var targetTypeStr = strOp.Read(container, refPool);
							var targetType = Type.GetType((string)targetTypeStr!);
							var classOp = TypeOperators.Instance.GetOperator(targetType);
							var value = classOp.Read(container, refPool);
							return value;
						}
					}
					break;
			}

			return null;
		}
	}
}
