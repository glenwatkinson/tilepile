using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Opening,
    Gameplay,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public GameState currentGameState;
    public UIManager uIManager;
    public Camera gameCamera;
    public TileBoard tileBoard;
    public EffectsManager effects;
    public float timeRemaining = 100;
    public float maxTime = 100;
    public int score = 0;

    TilePiece firstHitTile = null;

    public static GameManager instance;

    void Awake()
    {
        instance = this;
        uIManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        if (gameCamera == null)
            gameCamera = Camera.main;
        tileBoard = this.GetComponentInChildren<TileBoard>();
        effects = this.GetComponentInChildren<EffectsManager>();
        Application.targetFrameRate = 60;
        if ((float)Screen.width/(float)Screen.height > 1.5f)
            gameCamera.transform.position = Vector3.up * 10.5f;
        else
            gameCamera.transform.position = Vector3.up * 12.0f;
    }

    void Start()
    {
        SetGameState(GameState.Opening, false);
    }

    public void SetGameState(GameState newGameState, bool transition = true)
    {
        if (uIManager.isTransitioning && transition)
            return;
        switch (newGameState)
        {
            case GameState.Gameplay:    
                tileBoard.ClearGrid();
                tileBoard.GenerateGrid();
                timeRemaining = 100;
                maxTime = 100;
                score = 0;
                break;
            case GameState.Opening:
                tileBoard.ClearGrid();
                effects.ClearTileHighlights();
                effects.ClearLines();
                break;
        }
        uIManager.SetUIScreen(newGameState, transition);
        currentGameState = newGameState;
    }

    void Update()
    {
        switch (currentGameState)
        {
            case GameState.Gameplay:
                UpdateTime();
                UpdateInput();
                break;
        }
    }

    private void UpdateTime()
    {
        timeRemaining -= Time.deltaTime;
        if (timeRemaining < 0)
        {
            SetGameState(GameState.GameOver);
        }
    }

    private void UpdateInput()
    {
        // The input supports both dragging from one tile to another and clicking on each separately.
        // A click only registers if the player releases their finger/mouse button while still over the tile they pressed it down on 
        // otherwise it counts as a drag and will attempt to join the first piece with whatever they're pointing at now.
        // dragging into nothing clears the original selection.
        if (Input.GetMouseButtonDown(0))
        {
            AttemptToClick();
        }
        if (Input.GetMouseButton(0))
        {
            if (firstHitTile != null)
            {
                MoveDragLine();
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (firstHitTile != null)
            {
                effects.ClearDragLine();
                AttemptToClick();
            }
        }
    }

    private void AttemptToClick()
    {
        RaycastHit hit;
        var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            TilePiece hitPiece = hit.collider.gameObject.GetComponent<TilePiece>();
            if (hitPiece != null)
            {
                SelectTilePiece(hitPiece);
            }
            else
            {
                firstHitTile = null;
                effects.ClearTileHighlights();
            }
        }
        else
        {
            firstHitTile = null;
            effects.ClearTileHighlights();
        }
    }

    private void SelectTilePiece(TilePiece hitPiece)
    {
        if (firstHitTile == null)
        {
            firstHitTile = hitPiece;
            effects.PlaceTileHighlight(hitPiece.transform.position);
        }
        else
        {
            if (firstHitTile == hitPiece)
                return;
            Vector3[] pathway;
            if (tileBoard.CanPiecesConnect(firstHitTile, hitPiece, out pathway))
            {
                tileBoard.RemoveTilePiece(firstHitTile);
                tileBoard.RemoveTilePiece(hitPiece);
                score += 2;
                timeRemaining += 2;
                effects.ClearTileHighlights();
                effects.PlaceLine(pathway, true);
                effects.PlayExplosion(pathway[0]);
                effects.PlayExplosion(pathway[pathway.Length-1]);
                if (tileBoard.tilesOnBoard == 0)
                    SetGameState(GameState.GameOver);
            }
            else
            {
                effects.ClearTileHighlights();
            }
            firstHitTile = null;
        }
    }

    private void MoveDragLine()
    {
        Vector3 screenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y - 0.5f, gameCamera.transform.position.y);
        Vector3 dragSpot = gameCamera.ScreenToWorldPoint(screenPoint);
        Vector3[] dragPath = new Vector3[2];
        dragPath[0] = firstHitTile.transform.position;
        dragPath[1] = dragSpot;
        effects.PlaceLine(dragPath);
    }
}
