using UnityEngine;

public class ObjectID : MonoBehaviour
{
    public int objectID = 0;
    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        UpdateObjectID();
    }

    public void UpdateObjectID()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat("_ObjectID", objectID);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}
