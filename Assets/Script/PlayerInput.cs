using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] Transform lightPos;
    [SerializeField] Transform shadowPos;

    [SerializeField] Track shadowTrack;
    [SerializeField] Track lightTrack;

    private void Judge()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            this.transform.position = lightPos.position;
            this.gameObject.GetComponent<SpriteRenderer>().color = Color.black;
            shadowTrack.Judge();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            this.transform.position = shadowPos.position;
            this.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            lightTrack.Judge();
        }
    }

    void Update()
    {
        Judge();
    }
}
