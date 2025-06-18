//using UnityEngine;
//using UnityEngine.UI;

//public class UI : MonoBehaviour
//{
//    [SerializeField] private GameObject characterUI;

//    public UI_ItemToolTip itemToolTip;

//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        itemToolTip = GetComponentInChildren<UI_ItemToolTip>();
        
//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }

//    public void SwitchTo(GameObject _menu)
//    { 
//        for (int i = 0; i < transform.childCount; i++)
//            transform.GetChild(i).gameObject.SetActive(false);
//        if (_menu != null)
//            _menu.SetActive(true);

//    }
//}
