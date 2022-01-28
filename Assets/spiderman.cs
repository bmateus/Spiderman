using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class spiderman : MonoBehaviour
{
    [SerializeField]
    CharacterController2D controller;

    [SerializeField]
    SpriteRenderer spriteRenderer;

    float move;
    bool jump;
    bool crouch;
    float moveSpeed = 40f;


    bool isAttached;
    Vector3 webTarget;

    [SerializeField]
    LineRenderer web;


    Vector3 spawnPos;

    float distance = 0;
    [SerializeField]
    TMP_Text distanceLabel;

    [SerializeField]
    LayerMask collisionLayer;


    [SerializeField]
    Animator panelAnimator;

    [SerializeField]
    TMP_Text panelText;


    [SerializeField]
    Animator spideyAnimator;

	private void Start()
	{
        spawnPos = transform.position;
	}


	private void Update()
	{
		move = Input.GetAxisRaw("Horizontal") * moveSpeed;

        spideyAnimator.SetFloat("move", Mathf.Abs(move));

        if (Input.GetButtonDown("Jump"))
		{

            jump = true;
            web.enabled = false;
		}

        if (Input.GetButtonDown("Crouch"))
        {
            crouch = true;
            spriteRenderer.color = Color.green;
        }
        else if (Input.GetButtonUp("Crouch"))
		{
            crouch = false;
            spriteRenderer.color = Color.white;
		}

        if ( Input.GetMouseButtonDown(0) )
		{
            //shoot a web
            webTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
            //fire a ray at the spot
            RaycastHit2D hit = Physics2D.Raycast(transform.position, (webTarget - transform.position).normalized, 1000, collisionLayer);
            if ( hit.collider != null )
			{
                webTarget = hit.point;
                web.enabled = true;
            }
            else
			{
                web.enabled = false;
			}                
		}

        distance = Mathf.Max(0, transform.position.x - spawnPos.x);
        distanceLabel.text = $"DISTANCE: {distance:0.0}m";
    }

	void FixedUpdate()
    {
        controller.Move(move * Time.fixedDeltaTime, crouch, jump);
        jump = false;

        //update line renderer
        if (web.enabled)
		{
            web.SetPosition(0, transform.position);
            web.SetPosition(1, webTarget);

            var dist = (transform.position - webTarget).magnitude;

            if ( dist > 0.1f)
			{
                //zoom towards the web target
                var dir = (webTarget - transform.position).normalized;
                var force = 100f;
                GetComponent<Rigidbody2D>().AddForce(dir * force);
            }
            else
			{
                //web.enabled = false;
			}

        }

        if (transform.position.y < -3)
            transform.position = spawnPos;


    }

    public void ShowTutorial(string text)
	{
        panelText.text = text;
        panelAnimator.Play("PanelOpen");
    }

}
