using System.Collections;
using UnityEngine;

public class WinTrigger : MonoBehaviour
{

	bool hasTriggered = false;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!hasTriggered)
		{
			StartCoroutine(WinSequence());
		}
	}

	IEnumerator WinSequence()
    {
		yield return new WaitForSeconds(1f);
		GameManager.Instance.Win();
		hasTriggered = true;
	}
}
