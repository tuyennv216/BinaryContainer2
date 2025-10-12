using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using System;

namespace BinaryContainer2.Operators
{
	public class TypeChar : TypeOperatorBase
	{
		public TypeChar()
		{
			Raw = typeof(Char);
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
				container.Items.AddRange(BitConverter.GetBytes((Char)data));
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			var bytes = container.ReadItems(2); // Char là 2 byte

			var value = BitConverter.ToChar(bytes!, 0);
			return value;
		}
	}
}
