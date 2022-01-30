using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{

	bool hasTriggered = false;

	[SerializeField]
	string triggerText;


	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!hasTriggered)
		{
			GameManager.Instance.ShowTutorial(triggerText);
			hasTriggered = true;
		}
	}
}
