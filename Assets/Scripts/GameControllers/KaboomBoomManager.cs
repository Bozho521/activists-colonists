using System;
using GameControllers;
using UnityEngine;
using Tiles;

public class KaboomBoomManager : MonoBehaviour
{
    [SerializeField] private HexGridManager hexGridManager;
    private AudioSource audioSource;
    [SerializeField] private GameObject smokeEffect;
    [SerializeField] private GameObject explosionEffect;
    private bool isArmed = false;
    private bool used = false;

    private void Start()
    {
        
    }

    private void OnMouseDown()
    {
        if (used) return;

        if (!hexGridManager)
        {
            Debug.LogWarning("HexGridManager not assigned!");
            return;
        }

        if (!isArmed)
        {
            transform.localPosition= Input.mousePosition; 
            Debug.Log("Kaboom armed! Click a tile to explode.");
            isArmed = true;
        }
    }

    private void Update()
    {
        if (!isArmed || used) return;

        if (Input.GetMouseButtonDown(0))
            TryExplodeTile();
    }
    
    
    

    private void TryExplodeTile()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        Tile tile = hit.collider.GetComponentInParent<Tile>();
        if (tile == null) return;

        hexGridManager.TriggerKaboom(tile);
        
        smokeEffect.SetActive(true);
        explosionEffect.SetActive(true);
        
        used = true;
        isArmed = false;
    }
}