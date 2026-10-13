using UnityEngine;

public class RecogerObjeto : MonoBehaviour
{

    PlayerVidas playerVidas;
    public void Awake(){
        playerVidas = GameObject.Find("PlayerObject").GetComponent<PlayerVidas>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("moneda"))
        {
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("corazon"))
        {
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("obstaculo") || collision.CompareTag("Obstaculo"))
        {
            Debug.Log("Game over");
            playerVidas.SufreDano();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("obstaculo") || collision.gameObject.CompareTag("Obstaculo"))
        {
            Debug.Log("Game over");
            playerVidas.SufreDano();
        }
    }
}
