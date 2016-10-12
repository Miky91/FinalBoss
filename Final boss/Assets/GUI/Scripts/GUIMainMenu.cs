using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GUIMainMenu : MonoBehaviour {

	//public GameObject PauseUI;
	
	//private bool paused = false;
	
	void Start()
	{
		//PauseUI.SetActive(false);
		
	}
	
	

	
	
	public void NewGame()
	{
        SceneManager.LoadScene("Sala_comienzo");
	}  
	
	public void Continue()
	{
        SceneManager.LoadScene("Sala_trofeos");

	}
	
	public void Options()
	{
        SceneManager.LoadScene("MainMenu");

	}

	
	public void About()
	{
        SceneManager.LoadScene("MainMenu");

	}


	public void Quit()
	{
		Application.Quit();
	}
}
