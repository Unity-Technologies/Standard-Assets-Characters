using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClayShootingController : MonoBehaviour {
	
		/// <summary>
		/// Gets required components
		/// </summary>
 public Rigidbody clayBullet;
 //public int direction;
 public Transform spawnLocation;
 public int speed; 
 private float counter = 5;
	void Update () {
	
	 counter -= Time.deltaTime; 

		if(counter < 0){
			Shoot ();
			counter = 5;
		}
		
	}

	void Shoot(){

		Rigidbody shootBullet = Instantiate(clayBullet, spawnLocation.position, spawnLocation.rotation);
		shootBullet.velocity =  (spawnLocation.forward * speed);
		Debug.Log ("is shooting them bullets");
	}


}
