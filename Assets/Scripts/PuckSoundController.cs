using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuckSoundController : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private List<AudioClip> clipList;
    private PuckController puckController;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        puckController = GameObject.Find("Puck").GetComponent<PuckController>();
        
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
        //audioSource.Play();
        var puckVelocity = puckController.GetCurrentVelocity();
        if(puckVelocity.magnitude > 20f)
        {
            //var x = other.;
            var particleSystem = GetComponent<ParticleSystem>();
            var burst = new ParticleSystem.Burst[] {new ParticleSystem.Burst(0.0f, puckVelocity.magnitude)};
            particleSystem.emission.SetBursts(burst);
            transform.eulerAngles = new Vector3(90f, Mathf.Rad2Deg * Mathf.Atan2(puckVelocity.y, puckVelocity.x), 0f);
            particleSystem.Play();
        }
    }
}
