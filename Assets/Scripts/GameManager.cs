using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Database;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform panelRegrasTransform;
    [SerializeField] private Transform panelReviewRegrasTransform;
    [SerializeField] private Transform contentTransform;
    [SerializeField] private GameObject regraPrefab;
    [SerializeField] private GameObject regraReviewPrefab;
    [SerializeField] private GameObject textoPanel;
    [SerializeField] private TMP_Text texto;
    [SerializeField] private TMP_InputField textoEntrada;
    [SerializeField] private FireBaseManager firebaseManager;
    [SerializeField] private Button buttonInserteTexto;
    [SerializeField] private Button buttonEnviarTexto;
    [SerializeField] private GameObject painelFinal;

    public delegate void FinalizarEvent();
    public static FinalizarEvent FinalizarHandle;

    [SerializeField] Dictionary<int, regra> regras = new();
    [SerializeField] private int level = 1;

    [SerializeField] private string KeyText;
    [SerializeField] private int like;
    [SerializeField] private int deslike;
    [SerializeField] private string textoPego;
    [SerializeField] private TMP_Text showText;
    [SerializeField] private TMP_Text likeText;
    [SerializeField] private TMP_Text deslikeText;
    [SerializeField] private Button likeButton;
    [SerializeField] private Button deslikeButton;
    [SerializeField] private GameObject panelEnviar;
    [SerializeField] private GameObject panelReceber;
    [SerializeField] private Transform panelTextoRecebido;
    [SerializeField] private GameObject panelLikeDeslike;


    private void OnEnable()
    {
        FinalizarHandle += Finalizar;
    }

    private void OnDisable()
    {
        FinalizarHandle -= Finalizar;
    }

    void Start()
    {
        InstanciarProximaRegra();
    }

    private void InstanciarProximaRegra()
    {
        regra regra = new();
        StartCoroutine(firebaseManager.GetInfo(level, result => {
            regra = result;
            GameObject instanciado = Instantiate(regraPrefab, panelRegrasTransform.position, Quaternion.identity);
            instanciado.transform.SetParent(panelRegrasTransform);
            instanciado.GetComponent<RegraFabConstruct>().Construct("Regra " + level + "º", regra.descricao, regra.opcao1, regra.opcao2, regra.opcao3);
            instanciado.transform.localScale = Vector3.one / 2;
            regras.Add(level, regra);
            buttonInserteTexto.interactable = true;
        }));
    }

    private void Update()
    {
        panelRegrasTransform.SetParent(this.transform);
        panelRegrasTransform.SetParent(contentTransform);
        panelLikeDeslike.transform.SetParent(this.transform);
        panelLikeDeslike.transform.SetParent(panelTextoRecebido);
    }

    public void EnviarTexto()
    {
        buttonInserteTexto.interactable = false;
        texto.text += "\n" + textoEntrada.text;
        level++;
        InstanciarProximaRegra();
    }

    public void Finalizar()
    {
        textoEntrada.gameObject.SetActive(false);
        buttonEnviarTexto.gameObject.SetActive(true);
    }

    public void Reload()
    {
        SceneManager.LoadScene(0);
    }

    public void Close()
    {
        Application.Quit();
    }

    public void EnviarTextoParaBancoDeDados()
    {
        textoPanel.gameObject.SetActive(false);
        buttonEnviarTexto.gameObject.SetActive(false);
        panelRegrasTransform.gameObject.SetActive(false);
        painelFinal.SetActive(true);
        StartCoroutine(firebaseManager.SendText(texto.text,regras));
    }

    public void PegarTexto()
    {
        for (int i = panelReviewRegrasTransform.childCount - 1; i >= 0; i--)
        {
            Transform child = panelReviewRegrasTransform.GetChild(i);
            Destroy(child.gameObject);
        }
        StartCoroutine(firebaseManager.GetObjetoAleatorioHistorias(objetoSnapshot =>
        {
            KeyText = objetoSnapshot.Key.ToString();

            like = Convert.ToInt32(objetoSnapshot.Child("Like").Value);
            deslike = Convert.ToInt32(objetoSnapshot.Child("Deslike").Value);
            likeText.text = like.ToString();
            deslikeText.text = deslike.ToString();
            textoPego = objetoSnapshot.Child("Texto").Value.ToString();
            showText.text = textoPego;
            panelEnviar.SetActive(false);
            panelReceber.SetActive(true);
            likeButton.interactable = true;
            deslikeButton.interactable = true;

            for (int i = 1; i <= 20; i++)
            {
                DataSnapshot regraSnapshot = objetoSnapshot.Child(i.ToString());
                string descricao = regraSnapshot.Child("descricao").Value.ToString();
                string opcao1 = regraSnapshot.Child("opcao1").Value.ToString();
                string opcao2 = regraSnapshot.Child("opcao2").Value.ToString();
                string opcao3 = regraSnapshot.Child("opcao3").Value.ToString();

                GameObject instanciado = Instantiate(regraReviewPrefab, panelReviewRegrasTransform.position, Quaternion.identity);
                instanciado.transform.SetParent(panelReviewRegrasTransform);
                instanciado.GetComponent<RegraFabConstruct>().Construct("Regra " + i + "º", descricao, opcao1, opcao2, opcao3);
                instanciado.transform.localScale = Vector3.one / 2;
            }
        }));
    }

    public void Like()
    {
        firebaseManager.AdicionarUmLike(KeyText);
        likeButton.interactable = false;
        deslikeButton.interactable = false;
    }

    public void Deslike()
    {
        firebaseManager.AdicionarUmDeslike(KeyText);
        likeButton.interactable = false;
        deslikeButton.interactable = false;
    }
}

public struct regra
{
    public string descricao;
    public string opcao1;
    public string opcao2;
    public string opcao3;
}