// NOTE - Remove superfluous usings
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour, IDamageable, IEnemyMoveable, ITriggerCheckable
{
    [field: SerializeField] public float MaxHealth { get; set; } = 100f;
    [field: SerializeField] public float CurrentHealth { get; set; }

    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider easeHealthSlider;

    [SerializeField] private TMP_Text enemyText;

    [SerializeField] private float lerpSpeed = 0.05f; 

    // NOTE - Prefer name: Rigidbody (PascalCase for properties)
    public Rigidbody rb { get; set; }
    
    // NOTE - Does not need to be public
    public Animator animator;
    private Generator3D generator;
    private bool isDead = false;
    private float immunityTimer = 0;

    // NOTE - Remove unused code

    //public bool isFacingRight { get; set; } = false;

#region state machine variables
    // NOTE - Why should an enemy expose its state machine? A state machine
    // should respond to triggers, not be affected by others 
    // NOTE - Wrong indentation, and use PascalCase for properties
      public EnemyStateMachine stateMachine { get; set; }
      public EnemyIdleState idleState { get; set; }
      public EnemyChaseState chaseState { get; set; }
      public EnemyAttackState attackState { get; set; }

    // NOTE - Should be get; private set;
    public bool isAggroed { get; set; }

    // NOTE - Should be get; private set;
    public bool isInStrikingDistance { get; set; }

#endregion
    
#region scriptable object variables

    // NOTE - Missing access specifiers
    // NOTE - Also, keep to a convention with the attributes on a new line or
    // behind a field
    [SerializeField]
    EnemyIdleSOBase enemyIdleBase;

    [SerializeField]
    EnemyChaseSOBase enemyChaseBase;

    [SerializeField]
    EnemyAttackSOBase enemyAttackBase;

    // NOTE - Bad naming convetion, should be EnemyBehaviour...
    public EnemyIdleSOBase enemyIdleBaseInstance { get; set; }
    public EnemyChaseSOBase enemyChaseBaseInstance { get; set; }
    public EnemyAttackSOBase enemyAttackBaseInstance { get; set; }

#endregion

    // NOTE - Missing access specifier
    void OnEnable()
    {
        // NOTE - In my opinion, too spaced apart
        
        stateMachine = new EnemyStateMachine();

        idleState = new EnemyIdleState(this, stateMachine);

        chaseState = new EnemyChaseState(this, stateMachine);

        attackState = new EnemyAttackState(this, stateMachine);

        // NOTE - Is this needed? In my opinion, separate out static variables
        // in a scriptable object from runtime data and put the runtime data in
        // another scriptable object. Then, use that runtime data scriptable
        // object for each one of these enemies
        enemyIdleBaseInstance = Instantiate(enemyIdleBase);
        enemyChaseBaseInstance = Instantiate(enemyChaseBase);
        enemyAttackBaseInstance = Instantiate(enemyAttackBase);

        // NOTE - Can be simplified down, just equal all of them together
        CurrentHealth = MaxHealth;

        healthSlider.maxValue = CurrentHealth;
        easeHealthSlider.maxValue = CurrentHealth;

        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        enemyIdleBaseInstance.Init(gameObject, this);
        enemyChaseBaseInstance.Init(gameObject, this);
        enemyAttackBaseInstance.Init(gameObject, this);

        stateMachine.Init(idleState);   
        
        generator = GameObject.FindGameObjectWithTag("TradeButton").GetComponent<Generator3D>();

        immunityTimer = 0;
        isDead = false;
    }

    private void Update()
    {
        // NOTE - In my opinion, don't shorten current
        stateMachine.currEnemyState.FrameUpdate();
        UpdateAnimator();
        HealthBarSlider();

        if (immunityTimer > 0 && !isDead)
        {
            immunityTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        stateMachine.currEnemyState.PhysicsUpdate();
    }

    private void UpdateAnimator()
    {
        // NOTE - Use braces for a one line if statement
        if (isDead)
        return;

        // NOTE - Just put the condition in the boolean you pass in
        if (Mathf.Abs(rb.velocity.x) > 0.01f || Mathf.Abs(rb.velocity.z) > 0.01f)
        {
            animator.SetBool("isWalking", true);
        }

        else
        {
            animator.SetBool("isWalking", false);
        }
    }


#region health functions
    public void Damage(float damage)
    {
        // NOTE - Use braces for a one line if statement
        if (isDead)
        return;

        if (immunityTimer <= 0)
        {
            CurrentHealth -= damage;

            CurrentHealth = Mathf.Max(CurrentHealth, 0f);

            animator.SetTrigger("isHit");

            if (CurrentHealth <= 0)
            {
                animator.SetTrigger("isDead");
                isDead = true;
                rb.isKinematic = true;
                // NOTE - Cache this CapsuleCollider
                GetComponent<CapsuleCollider>().enabled = false;
            }
            immunityTimer = 0.5f;
        }
    }

    public void Despawn()
    {
        //change later to use object pooling
        rb.isKinematic = false;
        // NOTE - Cache this CapsuleCollider
        GetComponent<CapsuleCollider>().enabled = true;
        generator.RemoveEnemyFromRoom(gameObject.transform.parent.gameObject);
    }

#endregion

#region movement functions

    // NOTE - I feel like an enemy should not be affected this much. Don't
    // expose the rigidbody's velocity, but rather have triggers to affect it
    // like 'stun'
    public void MoveEnemy(Vector3 velocity)
    {
        rb.velocity = velocity;
    }

#endregion

#region animation triggers
    // NOTE - Unused function
    private void AnimationTriggerEvent(AnimationTriggerType triggerType)
    {
        stateMachine.currEnemyState.AnimationTriggerEvent(triggerType);
    }

    // NOTE - This should be at the top, or at least above the function above
    public enum AnimationTriggerType
    {
        EnemyDamaged,
        PlayFootstepSound
    }
#endregion

#region trigger checks

    // NOTE - Incorrect indentation
     public void SetAggroStatus(bool isAggroed)
    {
        this.isAggroed = isAggroed;
    }

    public void SetStrikingDistanceBool(bool isInStrikingDistance)
    {
        // NOTE - Incorrect indentation
       this.isInStrikingDistance = isInStrikingDistance;
    }

#endregion

    // NOTE - This should really be a static utils function instead of being in the enemy
    public List<GameObject> FindChildObjectsWithTag(GameObject parent, string tag)
    {
        List<GameObject> children = new();
 
        // NOTE - Inconsistent spacing between keywords and spaces
        foreach(Transform transform in parent.transform) 
        {
            if(transform.CompareTag(tag)) 
            {
                children.Add(transform.gameObject);
            }
        }
        
        return children;
    }

#region Enemy Health Bar

    // NOTE - Should be private.
    public void HealthBarSlider()
    {
        // NOTE - Inconsistent spacing between keywords and brackets
        if(healthSlider.value != CurrentHealth)
        {
            healthSlider.value = CurrentHealth;
        }

        enemyText.text = healthSlider.value.ToString() + " / " + MaxHealth;

        if(healthSlider.value != easeHealthSlider.value)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, CurrentHealth, lerpSpeed);
        }

        // NOTE - Remove unused code 
        //Debug.Log("Enemy Health: " + healthSlider.value);
    }

#endregion
}
