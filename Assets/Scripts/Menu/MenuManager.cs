using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject mainPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("Botones del menu principal")]
    public Button playButton;
    public Button optionsButton;
    public Button creditsButton;
    public Button quitButton;

    [Header("Botones Volver (Opciones y Creditos)")]
    public List<Button> backButtons = new List<Button>();

    [Header("Opciones")]
    public Slider volumeSlider;
    [Tooltip("Toggle de Musica: marcado = la musica suena")]
    public Toggle muteToggle;
    [Tooltip("Toggle de Efectos (opcional): solo guarda la preferencia")]
    public Toggle effectsToggle;
    public AudioSource musicSource;

    [Header("Escena de juego")]
    public string gameSceneName = "Nivel1";

    const string VolumeKey = "volumen";
    const string MusicKey = "musica";
    const string EffectsKey = "efectos";

    void Start()
    {
        if (playButton) playButton.onClick.AddListener(Jugar);
        if (optionsButton) optionsButton.onClick.AddListener(MostrarOpciones);
        if (creditsButton) creditsButton.onClick.AddListener(MostrarCreditos);
        if (quitButton) quitButton.onClick.AddListener(Salir);

        foreach (Button boton in backButtons)
        {
            if (boton) boton.onClick.AddListener(MostrarMenu);
        }

        if (volumeSlider)
        {
            volumeSlider.value = PlayerPrefs.GetFloat(VolumeKey, volumeSlider.value);
            volumeSlider.onValueChanged.AddListener(CambiarVolumen);
            CambiarVolumen(volumeSlider.value);
        }

        if (muteToggle)
        {
            muteToggle.isOn = PlayerPrefs.GetInt(MusicKey, muteToggle.isOn ? 1 : 0) == 1;
            muteToggle.onValueChanged.AddListener(CambiarMusica);
            CambiarMusica(muteToggle.isOn);
        }

        if (effectsToggle)
        {
            effectsToggle.isOn = PlayerPrefs.GetInt(EffectsKey, effectsToggle.isOn ? 1 : 0) == 1;
            effectsToggle.onValueChanged.AddListener(CambiarEfectos);
        }

        MostrarMenu();
    }

    public void MostrarMenu()
    {
        MostrarPanel(mainPanel);
    }

    public void MostrarOpciones()
    {
        MostrarPanel(optionsPanel);
    }

    public void MostrarCreditos()
    {
        MostrarPanel(creditsPanel);
    }

    void MostrarPanel(GameObject panel)
    {
        if (mainPanel) mainPanel.SetActive(panel == mainPanel);
        if (optionsPanel) optionsPanel.SetActive(panel == optionsPanel);
        if (creditsPanel) creditsPanel.SetActive(panel == creditsPanel);
    }

    public void Jugar()
    {
        Debug.Log("Entrar");
        SceneManager.LoadScene(gameSceneName);
    }

    public void Salir()
    {
        Debug.Log("Salir");
        Application.Quit();
    }

    public void CambiarVolumen(float valor)
    {
        AudioListener.volume = valor;
        PlayerPrefs.SetFloat(VolumeKey, valor);
    }

    public void CambiarMusica(bool activa)
    {
        if (musicSource) musicSource.mute = !activa;
        PlayerPrefs.SetInt(MusicKey, activa ? 1 : 0);
    }

    public void CambiarEfectos(bool activos)
    {
        PlayerPrefs.SetInt(EffectsKey, activos ? 1 : 0);
    }
}
