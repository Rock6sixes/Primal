using System;

public abstract class ConfigAcceptableBase
{
	public Type ValueType;

	public ConfigAcceptableBase(Type valueType)
	{
		ValueType = valueType;
	}

	public abstract object Clamp(object value);

	public abstract bool IsValid(object value);

	public abstract string ToDescriptionString();
}
