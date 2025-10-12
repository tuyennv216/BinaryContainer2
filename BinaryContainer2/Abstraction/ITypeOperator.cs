using BinaryContainer2.Container;
using BinaryContainer2.Others;
using System;
using System.Collections.Generic;

namespace BinaryContainer2.Abstraction
{
	public interface ITypeOperator
	{
		public Type? Raw { get; }
		public List<ITypeOperator>? Follows { get; }
		public bool IsBuilded { get; }

		public void Build();

		public void Write(DataContainer container, object? data, RefPool refPool);
		public object? Read(DataContainer container, RefPool refPool);
	}
}
