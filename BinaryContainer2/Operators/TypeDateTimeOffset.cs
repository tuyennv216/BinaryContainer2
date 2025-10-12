using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using System;

namespace BinaryContainer2.Operators
{
	public class TypeDateTimeOffset : TypeOperatorBase
	{
		public TypeDateTimeOffset()
		{
			Raw = typeof(DateTimeOffset);
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

			var value = (DateTimeOffset)data;
			var defaultValue = default(DateTimeOffset);

			container.Flags.Add(value == defaultValue);

			if (value != null && value != defaultValue)
			{
				// DateTimeOffset is stored as two 64-bit integers (long):

				// A. Ticks: The number of 100-nanosecond intervals since 1/1/0001
				container.Items.AddRange(BitConverter.GetBytes(value.Ticks));

				// B. Offset Ticks: The offset from Coordinated Universal Time (UTC),
				// also represented as ticks, which is derived from a TimeSpan.
				container.Items.AddRange(BitConverter.GetBytes(value.Offset.Ticks));
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			var isdefault = container.Flags.Read();
			if (isdefault == true) return default(DateTimeOffset);

			// A. Read 8 bytes for the Int64 (long) Ticks
			var ticksBytes = container.ReadItems(8);
			var ticks = BitConverter.ToInt64(ticksBytes!, 0);

			// B. Read 8 bytes for the Int64 (long) Offset Ticks
			var offsetTicksBytes = container.ReadItems(8);
			var offsetTicks = BitConverter.ToInt64(offsetTicksBytes!, 0);

			// Reconstruct the TimeSpan from the offset ticks
			var offsetTimeSpan = new TimeSpan(offsetTicks);

			// Create the DateTimeOffset from Ticks and the TimeSpan offset
			return new DateTimeOffset(ticks, offsetTimeSpan);
		}
	}
}
