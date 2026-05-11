using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 25f;
    public float lifeTime = 5f;
    public bool isFlying = false; 

    private bool hasHit = false;
    private Collider myCollider;

    void Awake()
    {
        myCollider = GetComponent<Collider>();
        if (myCollider != null) myCollider.enabled = false; 
    }

    void Start()
    {
        Destroy(gameObject, lifeTime + 2f); 
    }

    public void Launch(Vector3 targetPos)
    {
        isFlying = true;
        transform.SetParent(null); // 脱离手部骨骼

        transform.LookAt(targetPos);
        transform.Rotate(Vector3.right, 90f);
        if (myCollider != null) myCollider.enabled = true; 
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (!isFlying || hasHit) return;

        // 沿前方飞行
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isFlying || other.CompareTag("Enemy")) return;

        if (hasHit || !isFlying) return;

        if (other.CompareTag("Player"))
        {
            hasHit = true;
            GetComponent<Collider>().enabled = false;
            Destroy(gameObject);
        }
        else if (other.CompareTag("Untagged") || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }
}