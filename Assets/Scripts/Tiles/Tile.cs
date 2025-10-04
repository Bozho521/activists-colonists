using UnityEngine;

public class Tile : MonoBehaviour
{
    public GameObject replacementPrefab;

    public void Replace()
    {
        if (replacementPrefab != null)
        {
            Instantiate(replacementPrefab, transform.position, transform.rotation);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log(gameObject.name + " has no replacement prefab set!");
        }
    }
}
