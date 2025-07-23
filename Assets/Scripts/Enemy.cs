using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour      //Enemy class controls the Enemy AI, Setup of Ships and Gameplay
{
    GameManager gameManager;
    public List<int> enemyShips = new List<int>();      //list of all Tiles a ship is
    public List<int> chosenTiles = new List<int>();     //list of all Tiles that have been already chosen
    private List<int> hitTiles = new List<int>();       //list of all Tiles that have been hit
    public void EnemySetup()        //Method to setup for Gameplay, chooses tiles for ships
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();       //gets GameManager Script from GameObject
        for (int i = 0; i < gameManager.EnemyShips.Length; i++)                  //iterates through all Ships
        {
            Ships shipComponent = gameManager.EnemyShips[i].GetComponent<Ships>(); //gets Ship Script from Ship-GameObject
            int rnd = UnityEngine.Random.Range(0, 2);                                   //randomly chooses wheter ship is horizontal or vertical
            if (rnd == 0)                                                               
            {
                shipComponent.horizontal = true;
            }
            else
            {
                shipComponent.horizontal = false;
            }
            shipComponent.setShipPlace(UnityEngine.Random.Range(1, 101));               //randomly chooses a Number between 1-100 until no Tiles are already taken
            while (shipComponent.getShipPlace().Any(num => enemyShips.Contains(num)))
            {
                shipComponent.setShipPlace(UnityEngine.Random.Range(1, 100));
            }
            enemyShips.AddRange(shipComponent.getShipPlace().ToList());                 //adds all Tiles that a ship is on to a List
        }
    }

    public int chooseTile()                 //determines the next tile the enemy should attack
    {
        if (hitTiles.Count > 0)             //prioritizes tiles adjacent to previous hits
        {
            foreach (int hit in hitTiles)
            {
                int[] adjacent = new int[] { hit - 1, hit + 1, hit - 10, hit + 10 };

                foreach (int adj in adjacent)
                {
                    if (adj >= 1 && adj <= 100 && !chosenTiles.Contains(adj))
                    {
                        chosenTiles.Add(adj);
                        return adj;
                    }
                }
            }
        }

        int rnd = UnityEngine.Random.Range(1, 101); //chooses a random tile if no strategic moves are available
        while (chosenTiles.Contains(rnd))
        {
            rnd = UnityEngine.Random.Range(1, 101);
        }
        chosenTiles.Add(rnd);
        return rnd;
    }

    public List<int> getEnemyShips()    //returns a list of all Tiles a ship is on
    {
        return enemyShips;
    }

    public void setHitTiles(int hitTile)    //adds a hit Tile to a List
    {
        hitTiles.Add(hitTile);
    }
}
