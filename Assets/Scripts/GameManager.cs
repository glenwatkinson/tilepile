﻿using System.Collections;
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
    }

    void Start()
    {
        SetGameState(GameState.Opening);
    }

    public void SetGameState(GameState newGameState)
    {
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
        uIManager.SetUIScreen(newGameState);
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

        if (Input.GetMouseButtonDown(0))
        {
            TryToSelectTile();
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
                TryToSelectTile();
            }
        }
    }

    private void TryToSelectTile()
    {
        RaycastHit hit;
        var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            TilePiece hitPiece = hit.collider.gameObject.GetComponent<TilePiece>();
            if (hitPiece != null)
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
                    }
                    else
                    {
                        effects.ClearTileHighlights();
                    }
                    firstHitTile = null;
                }
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
