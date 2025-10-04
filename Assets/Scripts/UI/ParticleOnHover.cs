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
        voteParticle.Play();
    }

    void OnMouseExit()
    {
        voteParticle.Stop();
    }
}
