using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{

	bool hasTriggered = false;

	[SerializeField]
	string triggerText;

	spiderman context;

	private void Start()
	{
		context = FindObjectOfType<spiderman>();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!hasTriggered)
		{
			context.ShowTutorial(triggerText);
			hasTriggered = true;
		}
	}
}
