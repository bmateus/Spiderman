using UnityEngine;

public class WinTrigger : MonoBehaviour
{

	bool hasTriggered = false;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!hasTriggered)
		{
			GameManager.Instance.Win();
			hasTriggered = true;
		}
	}
}
