public class GenericDynamicSoundLoop : DynamicSoundLoop
{
	public GenericLoopEmitter.Mixer mixer;

	public GenericDynamicSoundLoop(UpdatableAndDeletable owner, GenericLoopEmitter.Mixer mixer)
		: base(owner)
	{
		this.mixer = mixer;
	}

	public override void InitSound()
	{
		emitter = owner.room.PlayGenericSound(sound, v, p, mixer);
	}
}
