using TMPro;
using UnityEngine;

public class HunterFeedBack : MonoBehaviour
{
   [SerializeField] private TextMeshPro _text;
   //[SerializeField] private Transform _hunter;


    public void SetState(string state)
    {
        _text.text = state;
    }
    private void LateUpdate()
    {
        //transform.position = _hunter.position + Vector3.up * Time.deltaTime;
        _text.transform.rotation = Camera.main.transform.rotation;
    }
}
