using UnityEngine;

public class SceneSwitch : MonoBehaviour {

    public int scn = 0;    
	
	void FixedUpdate () {
        if (Input.GetKeyUp(KeyCode.M)) {
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scn);
        }
	}
}
