using UnityEngine;

namespace Watcher;

public interface ICallForHelp
{
	bool NeedsHelp();

	void RecieveHelp();

	float HelpPriority();

	bool CanLetGo();

	bool CanAcceptHelp();

	void AggreToHelp();

	bool IsHelpComing();

	Vector2 HelpPoint();

	Vector2 HelpEntranceVec();
}
