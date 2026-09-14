namespace DevInterface;

public class DevUISignalType : ExtEnum<DevUISignalType>
{
	public static DevUISignalType ButtonClick = new DevUISignalType("ButtonClick", register: true);

	public static DevUISignalType Create = new DevUISignalType("Create", register: true);

	public DevUISignalType(string value, bool register = false)
		: base(value, register)
	{
	}
}
