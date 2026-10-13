using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManejaMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Jugar()
    {
        Debug.Log("Entrar");
        SceneManager.LoadScene("Nivel1");
    }

    // Update is called once per frame
    public void Salir()
    {
        Debug.Log("Salir");
        Application.Quit();
    }
}
