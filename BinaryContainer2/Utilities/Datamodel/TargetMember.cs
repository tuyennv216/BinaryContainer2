using BinaryContainer2.Types;
using BinaryContainer2.Utilities.Exceptions;
using System;

namespace BinaryContainer2.Utilities.Datamodel
{
	public class TargetMember
	{
		private readonly TypeBinding _binding = new();

		public object Wrapper { get; }
		public string Name { get; }
		public Type Type { get; }

		public TargetMember(object wrapper, string memberName)
		{
			Wrapper = wrapper;
			Name = memberName;

			var wrapType = Wrapper.GetType();
			var fieldInfo = wrapType.GetField(memberName);
			var propertyInfo = wrapType.GetProperty(memberName);

			if (fieldInfo != null)
			{
				Type = fieldInfo.FieldType;
				_binding.GetValue = fieldInfo.GetValue;
				_binding.SetValue = fieldInfo.SetValue;
			}
			else if (propertyInfo != null)
			{
				Type = propertyInfo.PropertyType;
				_binding.GetValue = propertyInfo.GetValue;
				_binding.SetValue = propertyInfo.SetValue;
			}
			else
			{
				throw new MemberNotFoundException($"Member {memberName} not found!");
			}
		}

		public object GetValue()
		{
			var value = _binding.GetValue!(Wrapper);
			return value;
		}

		public void SetValue(object value)
		{
			_binding.SetValue!(Wrapper, value);
		}
	}
}
