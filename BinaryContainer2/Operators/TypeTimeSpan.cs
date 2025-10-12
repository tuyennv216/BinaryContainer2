using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using System;

namespace BinaryContainer2.Operators
{
	public class TypeTimeSpan : TypeOperatorBase
	{
		public TypeTimeSpan()
		{
			Raw = typeof(TimeSpan);
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

			var value = (TimeSpan)data;
			var defaultValue = default(TimeSpan); // Equivalent to TimeSpan.Zero

			// We will use 'value == defaultValue' which is cleaner and safer.
			container.Flags.Add(value == defaultValue);

			if (value != null && value != defaultValue)
			{
				// TimeSpan is stored as a 64-bit integer (long) representing ticks.
				container.Items.AddRange(BitConverter.GetBytes(value.Ticks));
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			var isdefault = container.Flags.Read();
			if (isdefault == true) return default(TimeSpan); // TimeSpan.Zero

			// Read 8 bytes for the Int64 (long) ticks
			var bytes = container.ReadItems(8);

			var ticks = BitConverter.ToInt64(bytes!, 0);
			return new TimeSpan(ticks);
		}
	}
}
