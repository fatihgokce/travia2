using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartContro : MonoBehaviour {

    // Use this for initialization
    public InputField userName;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void BtnStartClick()
    {
        SceneManager.LoadScene("SampleScene");
        PlayerPrefs.SetString("userName",userName.text);
    }
}
