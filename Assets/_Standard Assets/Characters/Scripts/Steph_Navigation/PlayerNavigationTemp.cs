using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerNavigationTemp : MonoBehaviour {

    public Camera Cam;
    public NavMeshAgent navMesh;
  

	void Start () {
	    if (Cam == null)
	    {
		    Cam = Camera.main;   
	    }
        navMesh = GetComponent < NavMeshAgent> ();
 
    }


    void Update() {
        if (Input.GetKeyDown(KeyCode.Mouse0)){
            Ray ray = Cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast ( ray, out hit)) {
                navMesh.SetDestination (hit.point);

            }

        }
	}

   
}
