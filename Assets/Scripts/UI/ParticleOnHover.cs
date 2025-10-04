using UnityEngine;

public class ParticleOnHover : MonoBehaviour
{
    private ParticleSystem voteParticle;

    void Start()
    {
        voteParticle = GetComponentInChildren<ParticleSystem>();
    }

    void Update()
    {
        
    }

    void OnMouseEnter()
    {
        Debug.Log("Mouse over Gameobject.");
        voteParticle.Play();
    }

    void OnMouseExit()
    {
        Debug.Log("Mouse now exiting.");
        voteParticle.Stop();
    }
}
