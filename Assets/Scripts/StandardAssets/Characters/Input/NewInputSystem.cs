using System;
using System.Collections;
using System.Collections.Generic;
using StandardAssets.Characters.Input;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Experimental.Input.Controls;
using UnityEngine.Experimental.Input;
using Random = UnityEngine.Random;

public class NewInputSystem : MonoBehaviour, IInput
{

	Vector2 m_MoveInput;
	private Vector2 moveVector2;

	
	public Camera mainCamera;
	
	public GameObject projectile; 
	
	public InputAction fireAction;

	public InputAction moveAction;

	public void OnEnable()
	{
		fireAction.Enable();
		moveAction.Enable();
	}

	public void OnDisable()
	{
		fireAction.Disable();
		moveAction.Disable();
	
	}

	public void Awake()
	{
		fireAction.performed += context => Fire(context);
		moveAction.performed += context => Move(context);
	}

	void Move(InputAction.CallbackContext context)
	{
		
		String axis = (context.control.ToString());
		char last = axis[axis.Length - 1];
		moveVector2 = new Vector2(0,0);
		switch (last)
		{
			case 'i':
				moveVector2.y = 1f;
				
				break;
			case 'k':
				moveVector2.y = -1f;
				break;
			case 'j' :
				moveVector2.x = -1f;
				break;
			case 'l':
				moveVector2.x = 1f;
				break;
			
		}
		
	
	}


	void Fire(InputAction.CallbackContext context)
	{
		var transform = this.transform;
		var newProjectile = Instantiate(projectile);
		newProjectile.transform.position = transform.position + transform.forward * 0.6f;
		newProjectile.transform.rotation = transform.rotation;
		var size = 1;
		newProjectile.transform.localScale *= size;
		newProjectile.GetComponent<Rigidbody>().mass = Mathf.Pow(size, 3);
		newProjectile.GetComponent<Rigidbody>().AddForce(transform.forward * 20f, ForceMode.Impulse);
		newProjectile.GetComponent<MeshRenderer>().material.color =
			new Color(Random.value, Random.value, Random.value, 1.0f);
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		m_MoveInput.Set(moveVector2.x,moveVector2.y);
		Debug.Log(m_MoveInput.ToString());

	}
	
	public bool isMoveInput 
	{ 
		get { return moveInput.sqrMagnitude > 0; }
	}
	
	public Vector2 moveInput
	{
		get { return m_MoveInput; }
	}

	
	//public bool isMoveInput { get; private set; }
	public Vector2 lookInput { get; private set; }
	public Action jump { get; set; }
}
