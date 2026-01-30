using UnityEngine;

public class connectmaskmovement : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private bool keepInitialOffset = true;

    private Vector3 positionOffset;

    private void Awake()
    {
        if (followTarget != null)
        {
            positionOffset = transform.position - followTarget.position;
        }
    }

    private void LateUpdate()
    {
        if (followTarget == null)
        {
            return;
        }

        if (keepInitialOffset)
        {
            transform.position = followTarget.position + positionOffset;
        }
        else
        {
            transform.position = followTarget.position;
        }
    }
}
