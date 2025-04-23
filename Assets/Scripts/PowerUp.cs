using UnityEngine;

public enum PowerUpType
{
    SlowMotion,
    ScoreMultiplier,
    FruitFrenzy
}

public class PowerUp : MonoBehaviour
{
    public PowerUpType powerUpType;
    public float duration = 5f;
    public GameObject whole;
    public GameObject sliced;
    public ParticleSystem activationEffect;
    
    private Rigidbody powerUpRigidbody;
    private Collider powerUpCollider;
    
    private void Awake()
    {
        powerUpRigidbody = GetComponent<Rigidbody>();
        powerUpCollider = GetComponent<Collider>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Blade blade = other.GetComponent<Blade>();
            Activate(blade.direction, blade.transform.position, blade.sliceForce);
        }
    }
    
    private void Activate(Vector3 direction, Vector3 position, float force)
    {
        // Disable collider to prevent multiple activations
        powerUpCollider.enabled = false;
        
        // Visual feedback for slicing the power-up
        if (whole != null) {
            whole.SetActive(false);
        }
        
        if (sliced != null) {
            sliced.SetActive(true);
            
            // Rotate based on the slice angle
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            sliced.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            
            // Add forces to sliced pieces
            Rigidbody[] slices = sliced.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody slice in slices)
            {
                slice.velocity = powerUpRigidbody.velocity;
                slice.AddForceAtPosition(direction * force, position, ForceMode.Impulse);
            }
        }
        
        // Play activation effect if we have one
        if (activationEffect != null) {
            activationEffect.Play();
        }
        
        // Apply power-up effect based on type
        switch (powerUpType)
        {
            case PowerUpType.SlowMotion:
                GameManager.Instance.ActivateSlowMotion(duration);
                break;
                
            case PowerUpType.ScoreMultiplier:
                GameManager.Instance.ActivateScoreMultiplier(duration);
                break;
                
            case PowerUpType.FruitFrenzy:
                GameManager.Instance.ActivateFruitFrenzy(duration);
                break;
        }
    }
} 