using TMPro;
using UnityEngine;

public class HunterFeedBack : MonoBehaviour
{
   [SerializeField] private TextMeshPro _text;
  


    public void SetState(string state)
    {
        _text.text = state;
    }
    private void LateUpdate()
    {
        
        _text.transform.rotation = Camera.main.transform.rotation;
    }
}
