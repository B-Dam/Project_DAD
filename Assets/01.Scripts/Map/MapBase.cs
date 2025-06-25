using UnityEngine;

public abstract class MapBase : MonoBehaviour
{
	public MapData MapData;

	private void Awake()
	{
		
    }

    protected abstract void OnLoadMap();  // ���� ȣ���� �� �۵��ϴ� ����

    protected abstract void OnReleaseMap();  // ���� ���� �� �۵��ϴ� ����


}
