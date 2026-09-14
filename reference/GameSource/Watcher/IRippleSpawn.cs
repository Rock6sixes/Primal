namespace Watcher;

public interface IRippleSpawn
{
	void SetTimeUntilFadeOut(int time);

	void ChangeRippleLayer(int rippleLayer, bool showEffect);
}
