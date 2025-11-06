using UnityEngine;

public class PlaySequence : MonoBehaviour
{
    public Animator animator;
    public string idleState = "Idle";
    public string action1State = "Action1";
    public string action2State = "Action2";
    public float idleDuration = 3f;

    private float timer;
    private int step = 0;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        timer = idleDuration;
        step = 0;
        animator.Play(idleState);
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            step++;
            switch (step)
            {
                case 1:
                    animator.CrossFade(action1State, 0.25f);
                    timer = animator.GetCurrentAnimatorStateInfo(0).length;
                    break;
                case 2:
                    animator.CrossFade(action2State, 0.25f);
                    timer = animator.GetCurrentAnimatorStateInfo(0).length;
                    break;
                default:
                    step = 0;
                    animator.CrossFade(idleState, 0.25f);
                    timer = idleDuration;
                    break;
            }
        }
    }
}
