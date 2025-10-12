using BinaryContainer2.Container;
using BinaryContainer2.Others;
using System;
using System.Collections.Generic;
using System.Threading;

namespace BinaryContainer2.Abstraction
{
	public abstract class TypeOperatorBase : ITypeOperator
	{
		public Type? Raw { get; protected set; }
		public List<ITypeOperator>? Follows { get; protected set; }
		public bool IsBuilded { get; protected set; }
		protected readonly ManualResetEventSlim BuildCompleteSignal = new ManualResetEventSlim(false);

		public abstract void Build();

		public abstract object? Read(DataContainer container, RefPool refPool);

		public abstract void Write(DataContainer container, object? data, RefPool refPool);
	}
}
