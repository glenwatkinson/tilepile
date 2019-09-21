using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    GameObject highlightCube;
    List<ConnectionLine> connectionLine = new List<ConnectionLine>();
    List<ParticleSystem> explosionPrefabs = new List<ParticleSystem>();

    // Start is called before the first frame update
    void Awake()
    {
        highlightCube = transform.Find("Tile Highlight").gameObject;
        explosionPrefabs.Add(this.transform.Find("TileExplosion").GetComponent<ParticleSystem>());
        explosionPrefabs.Add(GameObject.Instantiate(explosionPrefabs[0]).GetComponent<ParticleSystem>());
        explosionPrefabs[explosionPrefabs.Count-1].transform.SetParent(this.transform);
        connectionLine.Add(this.transform.Find("Connection Line").GetComponent<ConnectionLine>());
    }

    void Start()
    {
        ClearLines();
        ClearTileHighlights();
    }

    public void PlayExplosion(Vector3 position)
    {
        for (int a = 0; a < explosionPrefabs.Count; a++)
        {
            if (!explosionPrefabs[a].isPlaying)
            {
                explosionPrefabs[a].transform.position = position;
                explosionPrefabs[a].Stop();
                explosionPrefabs[a].Clear();
                explosionPrefabs[a].Play();
                return;
            }
        }
        explosionPrefabs.Add(GameObject.Instantiate(explosionPrefabs[0]).GetComponent<ParticleSystem>());
        explosionPrefabs[explosionPrefabs.Count-1].transform.SetParent(this.transform);
        explosionPrefabs[explosionPrefabs.Count-1].transform.position = position;
        explosionPrefabs[explosionPrefabs.Count-1].Stop();
        explosionPrefabs[explosionPrefabs.Count-1].Clear();
        explosionPrefabs[explosionPrefabs.Count-1].Play();
    }

    public void ClearTileHighlights()
    {
        highlightCube.SetActive(false);
    }

    public void PlaceTileHighlight(Vector3 position)
    {
        highlightCube.SetActive(true);
        highlightCube.transform.position = position;
    }

    public void PlaceLine(Vector3[] positions, bool animateFromCurrent = false)
    {
        for (int a = 0; a < connectionLine.Count; a++)
        {
            if (!connectionLine[a].isInUse || (connectionLine[a].isInUse && connectionLine[a].isDragLine && !animateFromCurrent))
            {
                connectionLine[a].PlaceLine(positions, animateFromCurrent);
                return;
            }
        }
        connectionLine.Add(GameObject.Instantiate(connectionLine[0]).GetComponent<ConnectionLine>());
        connectionLine[connectionLine.Count-1].transform.SetParent(this.transform);
        connectionLine[connectionLine.Count-1].PlaceLine(positions, animateFromCurrent);

    }

    public void ClearDragLine()
    {
        for (int a = 0; a < connectionLine.Count; a++)
        {
            if (connectionLine[a].isDragLine)
                connectionLine[a].ClearLine();
        }
    }

    public void ClearLines()
    {
        for (int a = 0; a < connectionLine.Count; a++)
        {
            connectionLine[a].ClearLine();
        }
    }
}
