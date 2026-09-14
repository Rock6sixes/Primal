public class RectangularDynamicSoundLoop : DynamicSoundLoop
{
	public FloatRect rect;

	public Room room;

	public RectangularDynamicSoundLoop(UpdatableAndDeletable owner, FloatRect rect, Room room)
		: base(owner)
	{
		this.rect = rect;
		this.room = room;
	}

	public override void InitSound()
	{
		emitter = room.PlayRectSound(sound, rect, loop: true, v, p);
	}
}
