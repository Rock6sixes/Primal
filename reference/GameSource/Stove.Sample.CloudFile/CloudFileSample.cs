using UnityEngine;

namespace Stove.Sample.CloudFile;

public class CloudFileSample : MonoBehaviour
{
	[SerializeField]
	public string applicationKey;

	[SerializeField]
	public string applicationSecret;

	[SerializeField]
	public MemberForCloudFile memberInfoAccessor;

	[SerializeField]
	public string GameNo;

	[SerializeField]
	public string Category;

	[SerializeField]
	public string[] FilePaths;

	[SerializeField]
	public bool OverWrite;

	[SerializeField]
	public int pageSize = 10;

	[SerializeField]
	public string lastCreatedDate;

	[SerializeField]
	public string sortOrder;

	[SerializeField]
	public string partialFields;

	[SerializeField]
	public string fileid;

	[SerializeField]
	public bool shared;

	[SerializeField]
	public string fileFullPath;

	[SerializeField]
	public string fileCheckSum;
}
