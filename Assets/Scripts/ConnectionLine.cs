using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionLine : MonoBehaviour
{
    public bool isInUse = false;
    public bool isDragLine = false;
    GameObject sphere1;
    GameObject sphere2;
    LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Awake()
    {
        sphere1 = transform.Find("Connection Sphere 1").gameObject;
        sphere2 = transform.Find("Connection Sphere 2").gameObject;
        lineRenderer = this.GetComponent<LineRenderer>();
    }

    public void ClearLine()
    {
        isInUse = false;
        isDragLine = false;
        lineRenderer.enabled = false;
        sphere1.SetActive(false);
        sphere2.SetActive(false);
    }

    public void PlaceLine(Vector3[] positions, bool animateFromCurrent = false)
    {
        isInUse = true;
        for (int a = 0; a < positions.Length; a++)
        {
            positions[a] += Vector3.up * 0.25f;
        }
        lineRenderer.enabled = true;
        if (animateFromCurrent)
        {
            isDragLine = false;
            StartCoroutine(AnimateToPositions(positions));
        }
        else
        {
            isDragLine = true;
            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
        }
        sphere1.SetActive(true);
        sphere2.SetActive(true);
        sphere1.transform.position = positions[0];
        sphere2.transform.position = positions[positions.Length - 1];
    }

    IEnumerator AnimateToPositions(Vector3[] positions)
    {
        Vector3[] startPositions = new Vector3[positions.Length];
        startPositions[0] = positions[0];
        startPositions[startPositions.Length-1] = positions[positions.Length-1];
        for (int a = 1; a < startPositions.Length-1; a++)
        {
            Vector3 direction = (positions[positions.Length-1] - positions[0])/startPositions.Length;
            startPositions[a] = startPositions[a-1] + direction;
        }
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(startPositions);
        bool animating = true;
        while (animating)
        {
            animating = false;
            for (int a = 1; a < positions.Length-1; a++)
            {
                startPositions[a] = Vector3.MoveTowards(startPositions[a], positions[a], 0.15f);
                if (startPositions[a] != positions[a])
                    animating = true;
            }
            lineRenderer.SetPositions(startPositions);
            yield return null;
        }
        yield return new WaitForSeconds(1.0f);
        List<Vector3> foldUpPositions = new List<Vector3>();
        for (int a = 0; a < positions.Length; a++)
        {
            foldUpPositions.Add(positions[a]);
        }
        while (foldUpPositions.Count > 1)
        {
            if (foldUpPositions.Count > 2)
            {
                foldUpPositions[0] = Vector3.MoveTowards(foldUpPositions[0],foldUpPositions[1], 0.2f);
                if (foldUpPositions[0] == foldUpPositions[1])
                    foldUpPositions.RemoveAt(0);
                foldUpPositions[foldUpPositions.Count-1] = Vector3.MoveTowards(foldUpPositions[foldUpPositions.Count-1],foldUpPositions[foldUpPositions.Count-2], 0.2f);
                if (foldUpPositions[foldUpPositions.Count-1] == foldUpPositions[foldUpPositions.Count-2])
                    foldUpPositions.RemoveAt(foldUpPositions.Count-1);
            }
            else if (foldUpPositions.Count > 1)
            {
                Vector3 target = Vector3.Lerp(foldUpPositions[0],foldUpPositions[1],0.5f);
                foldUpPositions[0] = Vector3.MoveTowards(foldUpPositions[0],target, 0.2f);
                foldUpPositions[1] = Vector3.MoveTowards(foldUpPositions[1],target, 0.2f);
                if (foldUpPositions[0] == target) 
                    foldUpPositions.RemoveAt(0);
                else if (foldUpPositions[1] == target) 
                    foldUpPositions.RemoveAt(1);
            }
            sphere1.transform.position = foldUpPositions[0];
            sphere2.transform.position = foldUpPositions[foldUpPositions.Count - 1];
            lineRenderer.positionCount = foldUpPositions.Count;
            lineRenderer.SetPositions(foldUpPositions.ToArray());
            yield return null;
        }
        GameManager.instance.effects.PlayExplosion(foldUpPositions[0]);
        ClearLine();
    }
}
