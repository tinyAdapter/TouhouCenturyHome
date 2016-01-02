using UnityEngine;
using System.Collections;

public enum WalkEnum
{
    Bot = 0,
    BotLeft,
    BotRight,
    Left,
    Right,
    Up,
    UpLeft,
    UpRight,
}

public class ExampleController : MonoBehaviour {

    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void OnGUI()
    {
        for (int i = 0; i < 8; i++)
        {
            if (GUILayout.Button(((WalkEnum)i).ToString()))
            {
                anim.SetFloat("WalkEnum", i / 8.0f);
            }
        }
    }
}
