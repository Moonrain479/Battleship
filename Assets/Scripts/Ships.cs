using UnityEngine;

public class Ships : MonoBehaviour      //represents a single ship, including its length, orientation, position on the tile grid and whether it is destroyed
{
    public int length;
    public bool horizontal;
    private int[] shipPlace;
    private bool isDestroyed = false;

    private void Start()
    {
        shipPlace = new int[length];
    }

    public void setShipPlace(int Tile)      //sets the Tile where the ship should be
    {
        if (!horizontal)
        {
            for (int i = 0; i < length; i++)
            {
                if (((Tile-1) % 10)+1 - i > 0)      //checks whether the Ship is out of field
                {
                    shipPlace[i] = Tile - i;        //if in field, calculates next Tile where ships is on and save it in Array
                }                   
                else                                //if not in field, moves ship until its in field
                {
                    Tile = Tile + (length - i);
                    transform.position = new Vector3(this.transform.position.x + (length - i) + (length - i) * 0.125f + (length - 1f) * 0.125f, this.transform.position.y, this.transform.position.z );
                    setShipPlace(Tile);
                }
            }
        }
        else
        {
            for (int i = 0; i < length; i++)        
            {
                if ((Tile - i * 10) > 0)            //checks whether the Ship is out of field
                {
                    shipPlace[i] = Tile - i * 10;   //if in field, calculates next Tile where ships is on and save it in Array
                }
                else                                //if not in field, moves ship until its in field
                {
                    Tile = Tile + (length - i) * 10;
                    transform.position = new Vector3(this.transform.position.x,this.transform.position.y, this.transform.position.z + (length-i) + (length - i) * 0.125f + (length - 1f) * 0.125f);
                    setShipPlace(Tile);
                }
            }
        }
    }
    public int[] getShipPlace()         //returns an Array of all Tiles the ship is on
    {
        return shipPlace;
    }

    public bool getIsDestroyed()        //returns whether ship is destroyed
    {
        return isDestroyed;
    }
    public void setIsDestroyed(bool destroyed)      //sets ship whether ship is destroyed
    {
        isDestroyed = destroyed;
    }
}
