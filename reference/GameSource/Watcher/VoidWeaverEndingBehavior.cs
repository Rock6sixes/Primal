using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class VoidWeaverEndingBehavior : VoidWeaver.Behavior
{
	public const float triggerDistance = 500f;

	public bool spotted;

	public int spottedTime;

	public bool everOutsideRange;

	public Vector2 camFrom;

	public Vector2 camTo;

	public FadeOut fadeOut;

	public override bool NoticedPlayer => true;

	public VoidWeaverEndingBehavior(VoidWeaver owner)
		: base(owner)
	{
	}

	public override void Update()
	{
		base.Update();
		RoomCamera[] cameras;
		if (!spotted && base.FocusPlayer != null)
		{
			if (!Custom.DistLess(base.FocusPlayer.mainBodyChunk.pos, owner.pos, 500f))
			{
				everOutsideRange = true;
			}
			else if (everOutsideRange)
			{
				spotted = true;
				cameras = owner.room.game.cameras;
				foreach (RoomCamera roomCamera in cameras)
				{
					camFrom = roomCamera.pos;
					camTo = owner.pos - roomCamera.sSize * new Vector2(0.5f, 0.8f);
					if (roomCamera.room == owner.room)
					{
						roomCamera.EnterCutsceneMode(base.FocusPlayer.abstractCreature, RoomCamera.CameraCutsceneType.Standard);
					}
				}
				foreach (Player item in owner.room.PlayersInRoom)
				{
					if (item != null)
					{
						item.controller = new Player.NullController();
					}
				}
			}
		}
		if (!spotted)
		{
			return;
		}
		spottedTime++;
		cameras = owner.room.game.cameras;
		foreach (RoomCamera roomCamera2 in cameras)
		{
			if (roomCamera2.room == owner.room)
			{
				roomCamera2.pos = Vector2.Lerp(camFrom, camTo, Mathf.SmoothStep(0f, 1f, (float)spottedTime / 80f));
			}
		}
		if (spottedTime == 160)
		{
			fadeOut = new FadeOut(owner.room, Color.black, 400f, fadeIn: false);
			owner.room.AddObject(fadeOut);
		}
		if (fadeOut != null && fadeOut.IsDoneFading())
		{
			ProcessManager manager = owner.room.game.manager;
			manager.statsAfterCredits = false;
			manager.rainWorld.progression.miscProgressionData.watcherEndingID = 3;
			RainWorldGame.BeatGameMode(owner.room.game, standardVoidSea: false);
			manager.nextSlideshow = WatcherEnums.SlideShowID.EndingVoidWeaver;
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlideShow);
			fadeOut = null;
		}
	}
}
