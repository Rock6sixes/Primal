public struct SoundModifiers
{
	public float volume;

	public float pan;

	public float pitch;

	public static SoundModifiers Neutral => new SoundModifiers(1f, 0f, 1f);

	public SoundModifiers(float volume, float pan, float pitch)
	{
		this.volume = volume;
		this.pan = pan;
		this.pitch = pitch;
	}
}
