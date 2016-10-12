using UnityEngine;
using System.Collections;

public class PlayAnimOnEnter : MonoBehaviour {


    public string animName;
    public Animator animatorThatHasAnim;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            animatorThatHasAnim.Play(animName);
    }

	
}
