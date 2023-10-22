using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using UnityEngine;

public class FireBaseManager : MonoBehaviour
{
    DatabaseReference reference;

    void Awake()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public IEnumerator GetInfo(int level, Action<regra> onCallback)
    {
        regra regra = new();
        var data = reference.Child("Regras").Child(level.ToString()).GetValueAsync();
        yield return new WaitUntil(() => data.IsCompleted);

        if (data.Exception != null)
        {
            Debug.LogError("Erro ao obter a regra: " + data.Exception.Message);
            onCallback.Invoke(regra);
        }
        else
        {
            DataSnapshot snap = data.Result;
            if (snap.Exists)
            {
                regra.descricao = snap.Child("Descrição").Value.ToString();
                List<string> possibilidades = new List<string>();
                if (snap.Child("Possibilidades").ChildrenCount > 0)
                {
                    Debug.Log(snap.Child("Possibilidades").ChildrenCount);
                    foreach (DataSnapshot childSnapshot in snap.Child("Possibilidades").Children)
                    {
                        string possibilidade = childSnapshot.GetValue(true).ToString();
                        possibilidades.Add(possibilidade);
                    }

                    if (possibilidades.Count >= 3)
                    {
                        List<int> indicesExclusivos = new List<int>();
                        for (int i = 0; i < possibilidades.Count; i++)
                        {
                            indicesExclusivos.Add(i);
                        }

                        for (int i = 0; i < 3; i++)
                        {
                            int randomIndex = UnityEngine.Random.Range(0, indicesExclusivos.Count);
                            int selectedIndex = indicesExclusivos[randomIndex];
                            indicesExclusivos.RemoveAt(randomIndex);

                            if (i == 0)
                            {
                                regra.opcao1 = possibilidades[selectedIndex];
                            }
                            else if (i == 1)
                            {
                                regra.opcao2 = possibilidades[selectedIndex];
                            }
                            else if (i == 2)
                            {
                                regra.opcao3 = possibilidades[selectedIndex];
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Não há pelo menos 3 possibilidades disponíveis.");
                    }
                }
                else
                {
                    regra.opcao1 = "";
                    regra.opcao2 = "";
                    regra.opcao3 = "";
                }
                Debug.Log(regra.descricao);
                Debug.Log(regra.opcao1);
                Debug.Log(regra.opcao2);
                Debug.Log(regra.opcao3);
                onCallback.Invoke(regra);
            }
            else
            {
                GameManager.FinalizarHandle?.Invoke();
            }
        }
    }

    public IEnumerator SendText(string texto, Dictionary<int, regra> dictRegras)
    {
        if (reference != null)
        {
            string chaveUnica = reference.Push().Key;

            DatabaseReference no = reference.Child("Historias").Child(chaveUnica);

            Dictionary<string, int> likeDeslike = new Dictionary<string, int>
    {
        { "Like", 0 },
        { "Deslike", 0 }
    };

            IDictionary<string, object> likeDeslikeObj = new Dictionary<string, object>();
            foreach (var kvp in likeDeslike)
            {
                likeDeslikeObj[kvp.Key] = kvp.Value;
            }

            no.UpdateChildrenAsync(likeDeslikeObj).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Erro ao enviar texto e valores para o Firebase: " + task.Exception.Message);
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("Texto e valores enviados com sucesso para o Firebase: " + texto);
                }
            });

            IDictionary<string, object> regrasObj = new Dictionary<string, object>();

            foreach (var kvp in dictRegras)
            {
                IDictionary<string, object> regraObj = new Dictionary<string, object>
                {
                    { "descricao", kvp.Value.descricao },
                    { "opcao1", kvp.Value.opcao1 },
                    { "opcao2", kvp.Value.opcao2 },
                    { "opcao3", kvp.Value.opcao3 }
                };

                regrasObj[kvp.Key.ToString()] = regraObj;
            }

            no.UpdateChildrenAsync(regrasObj).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Erro ao enviar texto e regras para o Firebase: " + task.Exception.Message);
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("Texto e regras enviados com sucesso para o Firebase: " + texto);
                }
            });

            no.Child("Texto").SetValueAsync(texto);

            yield return new WaitUntil(() => no.Child("Texto").GetValueAsync().IsCompleted);

            if (no.Child("Texto").GetValueAsync().Exception != null)
            {
                Debug.LogError("Erro ao enviar texto para o Firebase: " + no.Child("Texto").GetValueAsync().Exception.Message);
            }
            else
            {
                Debug.Log("Texto enviado com sucesso para o Firebase: " + texto);
            }
        }
        else
        {
            Debug.LogError("O Firebase não foi inicializado corretamente.");
        }
    }

    public async Task AdicionarUmLike(string chaveTexto)
    {
        if (reference != null)
        {
            DatabaseReference textoRef = reference.Child("Historias").Child(chaveTexto).Child("Like");

            textoRef.RunTransaction(mutableData => {
                if (mutableData.Value == null)
                {
                    mutableData.Value = 1;
                }
                else
                {
                    int valorAtual = Convert.ToInt32(mutableData.Value);
                    mutableData.Value = valorAtual + 1;
                }
                return TransactionResult.Success(mutableData);
            }).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Erro ao adicionar Like: " + task.Exception.Message);
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("Like adicionado com sucesso.");
                }
            });
        }
        else
        {
            Debug.LogError("O Firebase não foi inicializado corretamente.");
        }
    }

    public async Task AdicionarUmDeslike(string chaveTexto)
    {
        if (reference != null)
        {
            DatabaseReference textoRef = reference.Child("Historias").Child(chaveTexto).Child("Deslike");

            textoRef.RunTransaction(mutableData => {
                if (mutableData.Value == null)
                {
                    mutableData.Value = 1;
                }
                else
                {
                    int valorAtual = Convert.ToInt32(mutableData.Value);
                    mutableData.Value = valorAtual + 1;
                }
                return TransactionResult.Success(mutableData);
            }).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Erro ao adicionar Like: " + task.Exception.Message);
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("Like adicionado com sucesso.");
                }
            });
        }
        else
        {
            Debug.LogError("O Firebase não foi inicializado corretamente.");
        }
    }



    public IEnumerator GetObjetoAleatorioHistorias(System.Action<DataSnapshot> onComplete)
    {
        if (reference != null)
        {
            DatabaseReference historiasRef = reference.Child("Historias");

            List<string> chaves = new List<string>();

            var getChavesTask = historiasRef.GetValueAsync();
            yield return new WaitUntil(() => getChavesTask.IsCompleted);

            if (getChavesTask.Exception != null)
            {
                Debug.LogError("Erro ao obter chaves do Firebase: " + getChavesTask.Exception.Message);
                yield break;
            }

            DataSnapshot chavesSnapshot = getChavesTask.Result;

            foreach (var childSnapshot in chavesSnapshot.Children)
            {
                chaves.Add(childSnapshot.Key);
            }

            if (chaves.Count > 0)
            {
                string chaveAleatoria = chaves[UnityEngine.Random.Range(0, chaves.Count)];

                DatabaseReference objetoRef = historiasRef.Child(chaveAleatoria);

                var getObjetoTask = objetoRef.GetValueAsync();
                yield return new WaitUntil(() => getObjetoTask.IsCompleted);

                if (getObjetoTask.Exception != null)
                {
                    Debug.LogError("Erro ao obter objeto aleatório do Firebase: " + getObjetoTask.Exception.Message);
                    yield break;
                }

                DataSnapshot objetoSnapshot = getObjetoTask.Result;

                onComplete(objetoSnapshot);
            }
            else
            {
                Debug.LogWarning("Não há histórias disponíveis.");
            }
        }
        else
        {
            Debug.LogError("O Firebase não foi inicializado corretamente.");
        }
    }
}