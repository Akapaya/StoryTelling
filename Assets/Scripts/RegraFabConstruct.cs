using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RegraFabConstruct : MonoBehaviour
{
    [SerializeField] TMP_Text RegraText;
    [SerializeField] TMP_Text DescricaoText;
    [SerializeField] TMP_Text Opcao1Text;
    [SerializeField] TMP_Text Opcao2Text;
    [SerializeField] TMP_Text Opcao3Text;

    public void Construct(string regra, string descricao, string opcao1, string opcao2, string opcao3)
    {
        RegraText.text = regra;
        DescricaoText.text = descricao;
        Opcao1Text.text = opcao1;
        Opcao2Text.text = opcao2;
        Opcao3Text.text = opcao3;
    }
}