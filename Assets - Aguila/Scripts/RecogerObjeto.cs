using UnityEngine;

// Va en el águila. Los diamantes y las cerezas son triggers con las tags "gema" y "cereza".
public class RecogerObjeto : MonoBehaviour
{
    public int puntosGema = 10;     // Cada diamante suma
    public int puntosCereza = 10;   // Cada cereza resta

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("gema"))
        {
            GameManager.Instance?.CambiarPuntos(puntosGema, true);
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("cereza"))
        {
            GameManager.Instance?.CambiarPuntos(-puntosCereza, false);
            Destroy(collision.gameObject);
        }
    }
}
