using UnityEngine;

namespace DevInterface;

public class HideVoidSpawnRepresentation : ResizeableObjectRepresentation, IDevUISignals
{
	public Button button;

	public PlacedObject.HideVoidSpawnData Data => pObj.data as PlacedObject.HideVoidSpawnData;

	public HideVoidSpawnRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj)
		: base(owner, IDstring, parentNode, pObj, "HideVoidSpawn", showRing: true)
	{
		Panel panel = new Panel(owner, IDstring, this, Vector2.zero, new Vector2(100f, 30f), "HideSpawnType");
		subNodes.Add(panel);
		button = new Button(owner, IDstring + "_button", panel, new Vector2(5f, 5f), 80f, Data.type.value);
		panel.subNodes.Add(button);
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (sender == button)
		{
			string[] array = ExtEnumBase.GetExtEnumType(typeof(VoidSpawn.SpawnType)).entries.ToArray();
			int num = array.IndexOf(Data.type.value);
			num++;
			if (num >= array.Length)
			{
				num = 0;
			}
			Data.type = new VoidSpawn.SpawnType(array[num]);
			button.Text = Data.type.value;
		}
	}
}
