using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class GameManager : MonoBehaviour        //manages the game state, including ship placement, turns, and win/loss conditions
{
    [Header("Game Objects")]
    public GameObject[] PlayerShips;
    public GameObject[] EnemyShips;
    public GameObject[] PlayerTiles;
    public GameObject[] EnemyTiles;

    [Header("UI Elements")]
    public Text InstructionText;
    public Text PlayerRemainingText;
    public Text EnemyRemainingText;
    public GameObject rotateBtn;
    public GameObject doneBtn;
    public GameObject resetBtn;

    Ray ray;
    private RaycastHit hit;

    private List<int> placedShipsPlayer = new List<int>();
    private List<int> placedShipsEnemy = new List<int>();
    private Enemy enemy = new Enemy();
    private GameObject selectedShip;

    bool selected = false;
    bool setupCompleted;
    bool playerTurn = false;
    bool gameEnded = false;

    int enemyRemaining;
    int playerRemaining;

    private void Start()
    {
        PlayerRemainingText.gameObject.SetActive(false);    //deactivating UI Elements not needing at beginning
        EnemyRemainingText.gameObject.SetActive(false);
        resetBtn.gameObject.SetActive(false);
        enemyRemaining = PlayerShips.Length;        //set Number of Ships that are not destroyed
        playerRemaining = PlayerShips.Length;
        enemy.EnemySetup();         //Enemy chooses Tiles for Enemy Ships
        placedShipsEnemy = enemy.getEnemyShips();       //Enemy Ships are save to List
    }
    void Update()
    {
        if (!setupCompleted)        //Player chooses Tiles for Ships
        {
            Setup();
        }
        else if (!gameEnded)        //begining of Game
        {
            if (playerTurn)         //turn of Player
            {
                PlayerTurn();
            }
            else                    //turn of Enemy
            {
                EnemyTurn();
            }
            checkIfWon();           //checks if someone won
        }

    }


    void Setup()        //Player chooses Tiles for Ships
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);        
        if (Physics.Raycast(ray, out hit))          //detects what is hit by ray
        {
            if (!selected && Input.GetMouseButtonDown(0) && hit.collider.CompareTag("Ship"))        //if Ship is clicked on with Leftmousebutton it is selected
            {
                selected = true;
                selectedShip = hit.collider.gameObject;
                hit.transform.position = new Vector3(hit.transform.position.x, hit.transform.position.y + 0.5f, hit.transform.position.z);
            }

            if (selected && Input.GetMouseButtonDown(0) && hit.collider.CompareTag("Tile") && Array.Exists(PlayerTiles, t => t == hit.collider.gameObject))     //if PlayerTiles are clicked on the selected ship is moved to the accpoding Tile
            {
                selected = false;
                float shipLength = (float)selectedShip.GetComponent<Ships>().length;
                bool shipHorizontal = selectedShip.GetComponent<Ships>().horizontal;
                Ships shipPlace = selectedShip.GetComponent<Ships>();
                if (!shipHorizontal)
                {
                    selectedShip.transform.position = new Vector3(hit.transform.position.x - (shipLength - 1f) / 2f - (shipLength - 1f) * 0.125f, hit.transform.position.y + 0.75f, hit.transform.position.z); //shipLength-1 is Number of Gaps between Tiles, 0.125 is half the length of a Gap
                }
                else if (shipHorizontal)
                {
                    selectedShip.transform.position = new Vector3(hit.transform.position.x, hit.transform.position.y + 0.75f, hit.transform.position.z - (shipLength - 1f) / 2f - (shipLength - 1f) * 0.125f);
                }
                shipPlace.setShipPlace(GetTileNumber(hit.collider.gameObject.name));
                selectedShip = null;
            }
        }
    }

    void PlayerTurn()       //listens to Playerinput
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (Input.GetMouseButtonDown(0) && hit.collider.CompareTag("Tile") && Array.Exists(EnemyTiles, t => t == hit.collider.gameObject))
            {
                int hitTile = GetTileNumber(hit.collider.gameObject.name);
                checkIfHit(hitTile);
            }
        }
    }
    void EnemyTurn()        //manages Enemies turn
    {
        int hitTile = enemy.chooseTile();
        checkIfHit(hitTile);
    }

    void checkIfHit(int hitTile)        //checks whether ship is hit
    {
        if (playerTurn)
        {
            if (placedShipsEnemy.Contains(hitTile))
            {
                hit.collider.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;        //colors tile red if hit
                placedShipsEnemy.Remove(hitTile);                                                       //removes hit ship Tiles from Enemy Ship Tile List
                checkIfShipSunken();                                                                    //checks if whole Ship is hit
            }
            else
            {
                hit.collider.gameObject.GetComponent<MeshRenderer>().material.color = Color.gray;       //colors tile grey if no ship on tile
                playerTurn = false;                                                                     //end player turn
            }
        }
        else
        {
            GameObject tile = Array.Find(PlayerTiles, tile => tile.name == "Tile (" + hitTile + ")");   //Finds Tile which is choosen by Enemy
            if (placedShipsPlayer.Contains(hitTile))                                                    //check if hit Tile is in Player Ship Tile List
            {
                tile.GetComponent<MeshRenderer>().material.color = Color.red;                           //colors Tile red
                enemy.setHitTiles(hitTile);                                                             //tell Enemy Script which tile is hit
                placedShipsPlayer.Remove(hitTile);                                                      //removes hit ship Tiles from Player Ship Tile List
                checkIfShipSunken();                                                                    //check if whole Ship is hit
            }
            else
            {
                tile.GetComponent<MeshRenderer>().material.color = Color.gray;                          //colors Tile grey 
                playerTurn = true;                                                                      //ends Enemyturn
            }
        }
    }

    void checkIfWon()       //checks if somebody won
    {
        if (!placedShipsPlayer.Any())       //checks if Player Ship Tile List empty
        {
            InstructionText.text = "Game Over: Enemy won";      //changes Text to who won
            gameEnded = true;                                   //ends game
            resetBtn.gameObject.SetActive(true);                //activates replay button
        }
        else if (!placedShipsEnemy.Any())       //checks if Enemy Ship Tile List empty
        {
            InstructionText.text = "Game Over: Player won";
            gameEnded = true;
            resetBtn.gameObject.SetActive(true);
        }
    }

    void checkIfShipSunken()        //checks if whole ship is sunken
    {
        for (int i = 0; i < PlayerShips.Length; i++)        //iterates through all Ships
        {
            if (playerTurn)
            {
                Ships ship = EnemyShips[i].GetComponent<Ships>();
                int[] shipTiles = ship.getShipPlace();
                int temp = 0;
                if (ship.getIsDestroyed() == false)         //checks whether ship is already destroyed
                {
                    for (int j = 0; j < ship.length; j++)
                    {
                        if (!placedShipsEnemy.Contains(shipTiles[j]))       
                        {
                            temp++;                         //adds one for everytime a Tile with a Ship is hit
                        }
                    }
                    if (temp == ship.length)                //checks whether whole ship is hit
                    {
                        ship.setIsDestroyed(true);          //set Ship to destroyed
                        enemyRemaining--;
                        EnemyRemainingText.text = "Enemy Ships Remaining: " + enemyRemaining + "/" + PlayerShips.Length;  //sets remaining ships 
                    }
                }
            }
            else    //same as above for Player
            {
                Ships ship = PlayerShips[i].GetComponent<Ships>();
                int[] shipTiles = ship.getShipPlace();
                int temp = 0;
                if (ship.getIsDestroyed() == false)
                {
                    for (int j = 0; j < ship.length; j++)
                    {
                        if (!placedShipsPlayer.Contains(shipTiles[j]))
                        {
                            temp++;
                        }
                    }
                    if (temp == ship.length)
                    {
                        ship.setIsDestroyed(true);
                        playerRemaining--;
                        PlayerRemainingText.text = "Player Ships Remaining: " + playerRemaining + "/" + PlayerShips.Length;
                    }
                }
            }

        }

    }

    private int GetTileNumber(string subjectString)     //extracts Tilenumber from string
    {
        string resultString = Regex.Match(subjectString, @"\d+").Value;
        int result = Int32.Parse(resultString);
        return result;
    }

    public void RotateButton()      //Button to rotate ships
    {
        try
        {
            if (selectedShip.GetComponent<Ships>().horizontal)      //rotate 90 degrees right if horizontal
            {
                selectedShip.transform.Rotate(new Vector3(0, 90, 0));
                selectedShip.GetComponent<Ships>().horizontal = false;
            }
            else                                                    //rotate 90 degrees left if vertical
            {
                selectedShip.transform.Rotate(new Vector3(0, -90, 0));
                selectedShip.GetComponent<Ships>().horizontal = true;

            }
        }
        catch (NullReferenceException e)
        {

        }
    }
    public void DoneButton()        //activates game after Setup is complete
    {
        placedShipsPlayer.Clear();      //clears List
        for (int i = 0; i < PlayerShips.Length; i++)
        {
            Ships shipComponent = PlayerShips[i].GetComponent<Ships>();

            List<int> shipPositions = shipComponent.getShipPlace().ToList();

            if (shipPositions != null)      //Adds all Items from Array to list
            {
                placedShipsPlayer.AddRange(shipPositions);
            }

        }
        if (!placedShipsPlayer.Contains(0))     //no Number in List equals 0 to make sure every ship is placed on field
        {
            if (placedShipsPlayer.Count == placedShipsPlayer.Distinct().Count())        //makes sure no Tile is occupied twice
            {
                setupCompleted = true;      //toggles setup as complete
                playerTurn = true;          //activates Playerturn
                rotateBtn.gameObject.SetActive(false);      //deactivates not needed UI
                doneBtn.gameObject.SetActive(false);
                InstructionText.text = "Choose Tile to Attack";     //changes instruction text
                PlayerRemainingText.gameObject.SetActive(true);     //actuivates need UI
                EnemyRemainingText.gameObject.SetActive(true);
                EnemyRemainingText.text = "Enemy Ships Remaining: " + enemyRemaining + "/" + PlayerShips.Length;        
                PlayerRemainingText.text = "Player Ships Remaining: " + playerRemaining + "/" + PlayerShips.Length;
            }
            else 
            {
                InstructionText.text = "Cannot place ship here, tiles overlap!";
            }
        }
        else
        {
            InstructionText.text = "All Ships must be placed";
        }
    }

    public void Reset()     //resets scene to play again
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void Quit()     //closes game
    {
        Application.Quit();
    }
}
