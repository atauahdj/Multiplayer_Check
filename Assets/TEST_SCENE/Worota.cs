using Mirror;
using TMPro;
using UnityEngine;

public class Worota : NetworkBehaviour
{
    Score s;
    [SyncVar(hook = nameof(OnScoreChanged))]
    public int score;
    void Start()
    {
        s = FindObjectOfType<Score>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return; 
        if (other.gameObject.tag == "Ball")
        {
            other.gameObject.transform.position = Vector3.zero;
            other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            s.score++;
            score = s.score;
        }
    }
    void OnScoreChanged(int oldValue, int newValue)
    {
        if (oldValue == newValue) return;
        foreach (GameObject text in GameObject.FindGameObjectsWithTag("Score"))
        {
            text.GetComponent<TMP_Text>().text = $"Score: {newValue}";
        }
        Debug.Log($"UI обновлен. Новый счет: {newValue}");
    }
}
