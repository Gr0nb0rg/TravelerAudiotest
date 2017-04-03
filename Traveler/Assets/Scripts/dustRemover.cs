using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dustRemover : MonoBehaviour {

	void Start ()
	{
	    Invoke("Remove", 1.1f);
	}

    void Remove()
    {
        Destroy(gameObject);
    }
}
