using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerVidas : MonoBehaviour
{
    TMP_Text textoVidas;
    private int vidas;
    public Image[] corazones;
    // Start is called before the first frame update
    void Awake()
    {
        vidas = corazones.Length;
        ModificaUI();
        textoVidas = GameObject.Find("TextoGameOver").GetComponent<TMP_Text>();
        textoVidas.enabled = false;
    }

    // Update is called once per frame


    public void SufreDano()
    {
        vidas--;
        ModificaUI();
        if (vidas <= 0)
        {
            Debug.Log("Game Over");
            textoVidas.enabled = true;
            Time.timeScale = 0f;
            return;
        }
    }

    private void ModificaUI()
    {
        for (int i = 0; i < corazones.Length; i++)
        {
            
                corazones[i].enabled = i < vidas;

        }
    }
}
