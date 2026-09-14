using RWCustom;
using UnityEngine;

namespace DevInterface;

public class LobeTreeRepresentation : ResizeableObjectRepresentation
{
	public Handle rootHandle;

	public TriangleMesh trunk;

	public LobeTree.LobeTreeData Data => pObj.data as LobeTree.LobeTreeData;

	public LobeTreeRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name, showRing: true)
	{
		rootHandle = new Handle(owner, "Root_Handle", this, Data.rootOffset);
		subNodes.Add(rootHandle);
		trunk = TriangleMesh.MakeLongMesh(8, pointyTip: false, customColor: false);
		owner.placedObjectsContainer.AddChild(trunk);
		fSprites.Add(trunk);
	}

	public override void Refresh()
	{
		base.Refresh();
		Data.rootOffset = rootHandle.pos;
		LobeTree.GetTrunkBezier(owner.room, pObj, out var a, out var cA, out var b, out var cB);
		for (int i = 0; i < trunk.vertices.Length; i += 2)
		{
			float f = (float)i / (float)(trunk.vertices.Length - 2);
			Vector2 vector = Custom.Bezier(a, cA, b, cB, f);
			Vector2 vector2 = Custom.PerpendicularVector(Custom.BezierDerivative(a, cA, b, cB, f));
			trunk.MoveVertice(i, vector + vector2 - owner.game.cameras[0].pos);
			trunk.MoveVertice(i + 1, vector - vector2 - owner.game.cameras[0].pos);
		}
	}
}
