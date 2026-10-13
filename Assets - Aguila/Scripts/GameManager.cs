using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Lleva los puntos y los imprime en la consola. Ganas al recoger todos los diamantes.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int puntos;
    int gemasRestantes;
    bool ganado;

    void Awake() { Instance = this; }

    void Start()
    {
        gemasRestantes = GameObject.FindGameObjectsWithTag("gema").Length;
    }

    public void CambiarPuntos(int cantidad, bool esGema)
    {
        puntos += cantidad;
        if (esGema) gemasRestantes--;
        Debug.Log($"Puntos: {puntos} | Diamantes restantes: {gemasRestantes}");

        if (puntos < 0)
        {
            ganado = true;
            Debug.Log($"GAME OVER");
        }

        if (esGema && gemasRestantes <= 0)
        {
            ganado = true;
            Debug.Log($"¡Ganaste! Puntos finales: {puntos}. Pulsa R para reiniciar.");
        }
    }

    void Update()
    {
        if (ganado && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
