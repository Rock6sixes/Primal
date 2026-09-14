using UnityEngine;

public class GenericLoopEmitter : SoundEmitter
{
	public delegate SoundModifiers Mixer(VirtualMicrophone mic, Vector2 listenerPos);

	public Mixer mixer;

	public GenericLoopEmitter(float baseVolume, float basePitch, Mixer mixer)
		: base(baseVolume, basePitch)
	{
		this.mixer = mixer;
		requireActiveUpkeep = true;
	}
}
