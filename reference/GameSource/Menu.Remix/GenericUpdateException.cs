using System;
using System.Runtime.Serialization;

namespace Menu.Remix;

[Serializable]
public class GenericUpdateException : ApplicationException
{
	public GenericUpdateException()
	{
	}

	public GenericUpdateException(string log)
		: base(log)
	{
	}

	public GenericUpdateException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public GenericUpdateException(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		throw new NotImplementedException();
	}
}
