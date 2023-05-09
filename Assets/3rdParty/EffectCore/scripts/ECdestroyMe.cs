using System;
using UnityEngine;
using System.Collections;

public class ECdestroyMe : MonoBehaviour
{
	[SerializeField] private ParticleSystem _ParticleSystem;
	
    float timer;
    public float deathtimer = 10;

    // Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        timer += Time.deltaTime;

        if(timer >= deathtimer)
        {
            gameObject.SetActive(false);
            UniversalObjectPool.instance.ReturnToPool(this);
        }
	
	}

	private void OnEnable()
	{
		_ParticleSystem.Play(true);
	}
}
