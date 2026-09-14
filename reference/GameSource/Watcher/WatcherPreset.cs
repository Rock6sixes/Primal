namespace Watcher;

public class WatcherPreset<T>
{
	public Configurable<T> config;

	public T standardValue;

	public T reducedValue;

	public T minimalValue;

	public WatcherPreset(Configurable<T> config, T standardValue, T reducedValue, T minimalValue)
	{
		this.config = config;
		this.standardValue = standardValue;
		this.reducedValue = reducedValue;
		this.minimalValue = minimalValue;
	}
}
