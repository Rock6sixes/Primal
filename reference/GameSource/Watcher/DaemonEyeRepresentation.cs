using DevInterface;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class DaemonEyeRepresentation : PlacedObjectRepresentation
{
	public DaemonEye eye;

	public DaemonEyeRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		subNodes.Add(new Handle(owner, "Rad_Handle", this, new Vector2(0f, 100f)));
		((Handle)subNodes[subNodes.Count - 1]).pos = ((DaemonEyeData)pObj.data).handlePos;
		fSprites.Add(new FSprite("Futile_White"));
		fSprites[fSprites.Count - 1].shader = owner.room.game.rainWorld.Shaders["VectorCircle"];
		owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
		fSprites.Add(new FSprite("pixel"));
		fSprites[fSprites.Count - 1].anchorY = 0f;
		owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
		fSprites.Add(new FSprite("pixel"));
		fSprites[fSprites.Count - 1].anchorY = 0f;
		owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
		subNodes.Add(new DaemonEyePanel(owner, "Daemon_Eye_Control_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as DaemonEyePanel).pos = (pObj.data as DaemonEyeData).panelPos;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		fSprites[1].scale = (subNodes[0] as Handle).pos.magnitude / 8f;
		fSprites[1].alpha = 2f / (subNodes[0] as Handle).pos.magnitude;
		MoveSprite(2, absPos);
		fSprites[2].scaleY = (subNodes[0] as Handle).pos.magnitude;
		fSprites[2].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[0] as Handle).absPos);
		(pObj.data as DaemonEyeData).handlePos = (subNodes[0] as Handle).pos;
		MoveSprite(3, absPos);
		fSprites[3].scaleY = (subNodes[1] as DaemonEyePanel).pos.magnitude;
		fSprites[3].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[1] as DaemonEyePanel).absPos);
		(pObj.data as DaemonEyeData).handlePos = (subNodes[0] as Handle).pos;
		(pObj.data as DaemonEyeData).panelPos = (subNodes[1] as Panel).pos;
	}
}
