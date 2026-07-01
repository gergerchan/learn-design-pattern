using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvaderFleet : MonoBehaviour
{
    [SerializeField] GameObject invaderPrefab;
    [SerializeField] Sprite[] rowSprites;
    [SerializeField] int rows = 3;
    [SerializeField] int cols = 8;
    [SerializeField] float spacingX = 1.2f;
    [SerializeField] float spacingY = 0.9f;
    [SerializeField] float stepSize = 0.7f;
    [SerializeField] float dropSize = 0.6f;
    [SerializeField] float stepInterval = 1f;
    [SerializeField] int stepsPerDrop = 5;

    List<GameObject> activeInvaders = new List<GameObject>();
    int aliveCount;
    int stepCount;
    int moveDirection = -1;
    bool isActive;
    float playerRowY;

    void Awake()
    {
        playerRowY = Object.FindFirstObjectByType<ShipController>().transform.position.y;
    }

    public void SpawnFleet()
    {
        aliveCount = rows * cols;
        stepCount = 0;
        moveDirection = -1;
        isActive = true;

        transform.position = Vector3.zero;

        float startX = -(cols - 1) * spacingX * 0.5f;
        float startY = Camera.main.orthographicSize * 0.55f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 localPos = new Vector3(startX + c * spacingX, startY - r * spacingY, 0f);
                GameObject obj = Instantiate(invaderPrefab, transform);
                obj.transform.localPosition = localPos;

                if (rowSprites != null && r < rowSprites.Length)
                    obj.GetComponent<SpriteRenderer>().sprite = rowSprites[r];

                obj.GetComponent<Invader>().Initialize(this);
                activeInvaders.Add(obj);
            }
        }

        StartCoroutine(MoveRoutine());
    }

    public void ClearFleet()
    {
        StopAllCoroutines();
        isActive = false;

        foreach (GameObject obj in activeInvaders)
        {
            if (obj != null) Destroy(obj);
        }

        activeInvaders.Clear();
        aliveCount = 0;
    }

    IEnumerator MoveRoutine()
    {
        while (isActive)
        {
            yield return new WaitForSeconds(stepInterval);
            ExecuteStep();
        }
    }

    void ExecuteStep()
    {
        transform.position += new Vector3(stepSize * moveDirection, 0f, 0f);
        stepCount++;

        if (stepCount >= stepsPerDrop)
        {
            transform.position += new Vector3(0f, -dropSize, 0f);
            moveDirection *= -1;
            stepCount = 0;
            EvaluateFleetPosition();
        }
    }

    void EvaluateFleetPosition()
    {
        float lowestY = float.MaxValue;
        foreach (GameObject obj in activeInvaders)
        {
            if (obj == null) continue;
            if (obj.transform.position.y < lowestY)
                lowestY = obj.transform.position.y;
        }

        if (lowestY <= playerRowY + 0.5f)
        {
            isActive = false;
            if (GameControl.Instance != null)
                GameControl.Instance.OnFleetReachedPlayer();
        }
    }

    public void OnInvaderDestroyed(GameObject invader)
    {
        activeInvaders.Remove(invader);
        aliveCount--;

        if (aliveCount <= 0)
        {
            isActive = false;
            if (GameControl.Instance != null)
                GameControl.Instance.OnAllInvadersDestroyed();
        }
    }
}
