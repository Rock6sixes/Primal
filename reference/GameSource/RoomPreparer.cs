using System.Globalization;
using System.IO;
using System.Threading;

public class RoomPreparer
{
	public int status;

	public bool finished;

	public ShortcutMapper scMapper;

	public AImapper aiMapper;

	public AIdataPreprocessor aiDataPreprocessor;

	public string aiHeatMapsData;

	public bool loadAiHeatMaps;

	public bool shortcutsOnly;

	public Thread thread;

	public bool threadFinished;

	public bool requestShortcutsReady;

	public bool requestReadyForAI;

	public Room room { get; set; }

	public bool done { get; set; }

	public bool falseBake { get; set; }

	public bool failed { get; set; }

	public RoomPreparer(Room room, bool loadAiHeatMaps, bool falseBake, bool shortcutsOnly)
	{
		this.room = room;
		this.loadAiHeatMaps = loadAiHeatMaps;
		this.falseBake = falseBake;
		this.shortcutsOnly = shortcutsOnly;
		string text = WorldLoader.FindRoomFile(WorldLoader.RoomNameManipulator(room.abstractRoom.FileName, room.game), includeRootDirectory: false, ".txt");
		if (text == null)
		{
			done = true;
			failed = true;
			return;
		}
		string[] array = File.ReadAllLines(text);
		room.LoadFromDataString(array);
		if (ModManager.PrecycleModule && room.defaultWaterLevel == -1)
		{
			room.defaultWaterLevel = -5000;
		}
		if (loadAiHeatMaps)
		{
			aiHeatMapsData = array[10];
		}
		scMapper = new ShortcutMapper(room);
	}

	public void Update()
	{
		bool flag = false;
		bool flag2 = false;
		lock (this)
		{
			if (!threadFinished && thread == null)
			{
				thread = new Thread(UpdateThread);
				thread.Start();
			}
			if (requestShortcutsReady)
			{
				flag = true;
				requestShortcutsReady = false;
				status++;
			}
			if (requestReadyForAI)
			{
				flag2 = true;
				requestReadyForAI = false;
				status++;
			}
			if (threadFinished)
			{
				if (!done)
				{
					done = true;
				}
				if (thread != null)
				{
					thread = null;
				}
			}
		}
		if (flag)
		{
			room.ShortCutsReady();
		}
		if (flag2)
		{
			room.ReadyForAI();
		}
	}

	public void UpdateThread()
	{
		CultureInfo.CurrentCulture = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");
		while (true)
		{
			int num;
			bool flag;
			bool flag2;
			lock (this)
			{
				num = status;
				flag = requestShortcutsReady;
				flag2 = requestReadyForAI;
			}
			switch (num)
			{
			case 0:
				if (flag)
				{
					break;
				}
				scMapper.Update();
				if (!scMapper.done)
				{
					break;
				}
				scMapper = null;
				if (shortcutsOnly)
				{
					lock (this)
					{
						requestShortcutsReady = true;
						threadFinished = true;
						return;
					}
				}
				aiMapper = new AImapper(room);
				lock (this)
				{
					requestShortcutsReady = true;
				}
				break;
			case 1:
				aiMapper.Update();
				if (aiMapper.done)
				{
					lock (this)
					{
						status++;
					}
					room.aimap = aiMapper.ReturnAIMap();
					aiMapper = null;
				}
				break;
			case 2:
				if (loadAiHeatMaps)
				{
					room.aimap.creatureSpecificAImaps = RoomPreprocessor.DecompressStringToAImaps(aiHeatMapsData, room.aimap);
					lock (this)
					{
						status = 4;
					}
				}
				else
				{
					aiDataPreprocessor = new AIdataPreprocessor(room.aimap, falseBake);
					lock (this)
					{
						status++;
					}
				}
				break;
			case 3:
				aiDataPreprocessor.Update();
				if (aiDataPreprocessor.done)
				{
					lock (this)
					{
						status++;
					}
				}
				break;
			case 4:
				lock (this)
				{
					if (!flag2)
					{
						requestReadyForAI = true;
					}
				}
				break;
			case 5:
				lock (this)
				{
					threadFinished = true;
					return;
				}
			}
		}
	}

	public AbstractRoomNode[] ReturnRoomConnectivity()
	{
		if (aiDataPreprocessor != null)
		{
			return aiDataPreprocessor.Connectivity();
		}
		return null;
	}
}
