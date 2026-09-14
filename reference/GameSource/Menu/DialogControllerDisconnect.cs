using Kittehface.Framework20;
using Rewired;
using UnityEngine;

namespace Menu;

public class DialogControllerDisconnect : Dialog
{
	public SimpleButton cancelButton;

	public int playerIndex;

	public int exitTimer;

	public DialogControllerDisconnect(string description, ProcessManager manager, int playerIndex, bool allowCancel = false)
		: base(description, manager)
	{
		this.playerIndex = playerIndex;
		exitTimer = -1;
		if (allowCancel)
		{
			cancelButton = new SimpleButton(this, pages[0], Translate("cancel"), "CANCEL", new Vector2(pos.x + (size.x - 110f) * 0.5f, pos.y + Mathf.Max(size.y * 0.04f, 7f)), new Vector2(110f, 30f));
			pages[0].subObjects.Add(cancelButton);
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (message != null && message == "CANCEL")
		{
			if (playerIndex != 0)
			{
				manager.rainWorld.DeactivatePlayer(playerIndex);
			}
			manager.StopSideProcess(this);
		}
	}

	public override void Update()
	{
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Invalid comparison between Unknown and I4
		base.Update();
		bool flag = false;
		Player val = null;
		Profile val2 = null;
		PlayerHandler playerHandler = manager.rainWorld.GetPlayerHandler(playerIndex);
		if (playerHandler != null)
		{
			val2 = playerHandler.profile;
			if (val2 != null)
			{
				val = UserInput.GetRewiredPlayer(val2, playerHandler.playerIndex);
			}
		}
		Options.ControlSetup controlSetup = manager.rainWorld.options.controls[playerIndex];
		if (val == null)
		{
			val = controlSetup.player;
		}
		if (val == null)
		{
			val = controlSetup.player;
		}
		if (((Controller)ReInput.controllers.Mouse).GetAnyButtonDown())
		{
			flag = true;
		}
		if (val != null && val.controllers.joystickCount >= ReInput.configuration.maxJoysticksPerPlayer)
		{
			flag = true;
		}
		if (val != null && val.GetAnyButton() && exitTimer < 0)
		{
			exitTimer = 5;
		}
		for (int i = 0; i < val.controllers.Joysticks.Count; i++)
		{
			if (controlSetup.gamePadGuid != null && ((Controller)val.controllers.Joysticks[i]).deviceInstanceGuid.ToString() == controlSetup.gamePadGuid)
			{
				flag = true;
				break;
			}
		}
		if (!flag && exitTimer < 0)
		{
			foreach (Joystick joystick in ReInput.controllers.Joysticks)
			{
				if (((Controller)joystick).GetButton(((int)manager.rainWorld.options.enterButton != 1) ? 1 : 0))
				{
					Profile joystickProfile = UserInput.GetJoystickProfile(joystick);
					if (joystickProfile != null && val2 != null && joystickProfile == val2)
					{
						UserInput.ReconnectJoystick(val2, joystick);
						manager.rainWorld.RequestPlayerSignIn(playerIndex, joystick);
						exitTimer = 5;
						break;
					}
					exitTimer = 5;
				}
			}
		}
		if (val == null || (val2 == null && val == null) || (playerHandler == null && val == null))
		{
			flag = true;
		}
		if (exitTimer > 0)
		{
			exitTimer--;
			if (exitTimer == 0)
			{
				flag = true;
			}
		}
		if (flag)
		{
			manager.StopSideProcess(this);
		}
	}
}
