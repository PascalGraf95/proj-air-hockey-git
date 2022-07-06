using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuckSoundController : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private List<AudioClip> clipList;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" || other.tag == "Agent")
        {
            audioSource.clip = clipList[0];
            audioSource.pitch = Random.Range(0.95f, 1.05f);
        }
        else if(other.tag == "Boundary")
        {
            audioSource.clip = clipList[1];
            audioSource.pitch = Random.Range(0.95f, 1.05f);
        }
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
