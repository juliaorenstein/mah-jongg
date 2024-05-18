using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

public class TileGenerator : EditorWindow
{
    private Transform TilePool;
    private GameObject TilePF;
    private int tileID = 0;

    [MenuItem("Tools/Tile Generator")]
    public static void ShowWindow()
    {
        GetWindow<TileGenerator>("GameObject Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Tiles", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate"))
        {
            GenerateTiles();
        }
    }

    private void GenerateTiles()
    {
        TilePool = GameObject.Find("Tile Pool").transform;
        TilePF = Resources.Load<GameObject>("Prefabs/Tile");
        CreateNumberDragons();
        CreateFlowerWinds();
        CreateJokers();
    }

    void CreateNumberDragons()
    {
        Suit[] suits = (Suit[])Enum.GetValues(typeof(Suit));

        foreach (Suit suit in suits)
        {
            for (int num = 0; num < 10; num++)
            {
                for (int i = 0; i < 4; i++)
                {
                    GameObject tileGO = Instantiate(TilePF, TilePool);
                    TileComponent tileComp = tileGO.GetComponent<TileComponent>();
                    tileComp.tile = new(tileComp, tileID++, num, suit);
                }
            }
        }
    }

    void CreateFlowerWinds()
    {
        Direction[] directions = (Direction[])Enum.GetValues(typeof(Direction));

        foreach (Direction dir in directions)
        {
            for (int id = 0; id < 4; id++)
            {
                GameObject tileGO = Instantiate(TilePF, TilePool);
                TileComponent tileComp = tileGO.GetComponent<TileComponent>();
                tileComp.tile = new(tileComp, tileID++, null, null, dir);
            }
        }

        // AND THE LAST FOUR FLOWERS
        for (int id = 0; id < 4; id++)
        {
            GameObject tileGO = Instantiate(TilePF, TilePool);
            TileComponent tileComp = tileGO.GetComponent<TileComponent>();
            tileComp.tile = new(tileComp, tileID++, null, null, Direction.flower);
        }
    }

    void CreateJokers()
    {
        for (int id = 0; id < 8; id++)
        {
            GameObject tileGO = Instantiate(TilePF, TilePool);
            TileComponent tileComp = tileGO.GetComponent<TileComponent>();
            tileComp.tile = new(tileComp, tileID++);
        }
    }
}
