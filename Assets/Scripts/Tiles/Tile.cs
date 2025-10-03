using UnityEngine;

public class Tile : MonoBehaviour
{
    void OnDestroy()
    {
        Debug.Log(gameObject.name + " was destroyed!");
    }
}
