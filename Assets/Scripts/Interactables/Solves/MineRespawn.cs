using UnityEngine;

public class MineRespawn : MonoBehaviour
{
    public string triggerTag = "MineRespawnTrigger";
    public bool isMineDissolved = false;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public Animator animator;
    private Collider mainCollider;
    private Rigidbody mineRigidbody;
    public GameObject extraColliderObject;

    public bool allowInputRespawn = true;
    private GrabbableMine grabbableMine;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        mainCollider = GetComponent<Collider>();
        mineRigidbody = GetComponent<Rigidbody>();
        grabbableMine = GetComponent<GrabbableMine>();

        if (animator == null)
            Debug.LogWarning("Animator not found on the mine object.");

        if (extraColliderObject != null)
            extraColliderObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggerTag) && !isMineDissolved)
        {
            StartDissolve();
            if (grabbableMine != null)
            {
                grabbableMine.Release();
                grabbableMine.playerHand = null;
                grabbableMine.isGrabbed = false;
            }
        }

        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Mine triggered by player.");
            var handler = other.GetComponent<MineRespawnHandler>();
            handler.ResetDeleteRefTime();
            handler.mineRespawn = this;
        }
    }

    public void StartDissolve()
    {
        isMineDissolved = true;

        if (mineRigidbody != null)
        {
            mineRigidbody.isKinematic = true;
            mineRigidbody.velocity = Vector3.zero;
            mineRigidbody.angularVelocity = Vector3.zero;
        }

        if (mainCollider != null)
            mainCollider.enabled = false;

        if (animator != null)
            animator.SetTrigger("Dissolve");

        if (extraColliderObject != null)
            extraColliderObject.SetActive(false);

        Invoke(nameof(RespawnMine), 1.0f);
    }

    private void RespawnMine()
    {
        if (mineRigidbody != null)
        {
            mineRigidbody.isKinematic = true;
            mineRigidbody.position = initialPosition;
            mineRigidbody.rotation = initialRotation;
            mineRigidbody.isKinematic = false;
        }
        else
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }

        if (animator != null)
            animator.SetTrigger("Spawn");

        if (mainCollider != null)
            mainCollider.enabled = true;

        Invoke(nameof(EnableExtraCollider), 1.0f);

        isMineDissolved = false;
    }

    private void EnableExtraCollider()
    {
        if (extraColliderObject != null)
            extraColliderObject.SetActive(true);
    }
}
