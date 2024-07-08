using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationSpeedSetter : MonoBehaviour
{
    private Animator animatorController;
    private Vector3 lastPosition;

    private void Awake()
    {
        animatorController = GetComponent<Animator>();
        
    }
    
    void Update()
    {
        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        animatorController.SetFloat("speed", speed);
        lastPosition = transform.position;
    }
}
