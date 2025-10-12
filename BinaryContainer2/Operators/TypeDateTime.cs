using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using System;

namespace BinaryContainer2.Operators
{
	public class TypeDateTime : TypeOperatorBase
	{
		public TypeDateTime()
		{
			Raw = typeof(DateTime);
		}

		public override void Build()
		{
			IsBuilded = true;
			BuildCompleteSignal.Set();
		}

		public override void Write(DataContainer container, object? data, RefPool refPool)
		{
			container.Flags.Add(data == null);
			if (data == null) return;

			var value = (DateTime)data;
			var defaultValue = default(DateTime);

			container.Flags.Add(value == defaultValue);

			if (value != null && value != defaultValue)
			{
				// DateTime is stored as a 64-bit integer (long) representing ticks.
				container.Items.AddRange(BitConverter.GetBytes(value.Ticks));
				container.Items.Add((byte)value.Kind);
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			var isdefault = container.Flags.Read();
			if (isdefault == true) return default(DateTime);

			// Read 8 bytes for the Int64 (long) ticks
			var bytes = container.ReadItems(8);
			var kindByte = container.ReadItems(1);

			var ticks = BitConverter.ToInt64(bytes!, 0);
			var kind = (DateTimeKind)kindByte![0];

			return new DateTime(ticks, kind);
		}
	}
}
