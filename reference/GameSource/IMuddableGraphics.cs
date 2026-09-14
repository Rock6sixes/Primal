public interface IMuddableGraphics
{
	bool MuddableSprite(RoomCamera.SpriteLeaser sLeaser, int sprite);

	void SetUpSpecialMudSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera.SpriteLeaser mudSleaser, MudOverlay mudOverlay);
}
