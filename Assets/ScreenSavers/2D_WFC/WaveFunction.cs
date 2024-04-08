using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class WaveFunction : MonoBehaviour
{
    public int dimensions; //size of grid
    public Tile[] tileObjects; //possible tiles
    public List<Cell> gridComponents; //all cells that need to be filled
    public Cell cellObj; //a placeholder cell

    int iterations = 0;

    void Awake()
    {
        //on awake, we create the list and fill it by initializing the grid
        gridComponents = new List<Cell>();
        InitializeGrid();
    }

    void InitializeGrid()
    {
        //create a new cell at each given grid coordinate
        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                Cell newCell = Instantiate(cellObj, new Vector2(x, y), Quaternion.identity);
                newCell.CreateCell(false, tileObjects); //at start, no cells are collapsed, and all tiles are valid for each cell!
                gridComponents.Add(newCell);
            }
        }

        StartCoroutine(CheckEntropy());
    }


    IEnumerator CheckEntropy() //here we want to find the cell with the least available tile options
    {
        List<Cell> tempGrid = new List<Cell>(gridComponents);

        tempGrid.RemoveAll(c => c.collapsed); //remove the collapsed functions

        tempGrid.Sort((a, b) => { return a.tileOptions.Length - b.tileOptions.Length; }); //sort by tileOptions.Length

        int arrLength = tempGrid[0].tileOptions.Length;
        int stopIndex = 0;

        //starting at the beginning of the sorted list, check each cell until we find one where the tileOptions.Length is greater than the current
        //then set the stopIndex == to the index of the last cell with the least amount of possible tileOptions
        for (int i = 1; i < tempGrid.Count; i++)
        {
            if (tempGrid[i].tileOptions.Length > arrLength)
            {
                stopIndex = i;
                break;
            }
        }

        //if there's more than one cell that fits the least length criteria, cut off the rest of the list so we get just the 
        //the relevant cells for now
        if (stopIndex > 0)
        {
            tempGrid.RemoveRange(stopIndex, tempGrid.Count - stopIndex);
        }

        yield return new WaitForSeconds(0.01f);

        //pass all our potential next cell targets to the collapse cell function!
        CollapseCell(tempGrid);
    }

    void CollapseCell(List<Cell> tempGrid)
    {
        //pick a random candidate from our valid list (from CheckEntropy()) for collapsing
        int randIndex = UnityEngine.Random.Range(0, tempGrid.Count);
        Cell cellToCollapse = tempGrid[randIndex];
        //set the collapsed bool to true, then pick a random valid tile for collapsing
        cellToCollapse.collapsed = true;
        Tile selectedTile = cellToCollapse.tileOptions[UnityEngine.Random.Range(0, cellToCollapse.tileOptions.Length)];
        cellToCollapse.tileOptions = new Tile[] { selectedTile };

        Tile foundTile = cellToCollapse.tileOptions[0];
        Instantiate(foundTile, cellToCollapse.transform.position, Quaternion.identity);

        //in here we're gonna update the list with the validated cell and update any
        //tile options that have changed
        UpdateGeneration();
    }

    void UpdateGeneration()
    {
        List<Cell> newGenerationCell = new List<Cell>(gridComponents); //placeholder list 

        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                int index = x + y * dimensions; //find an index in the grid (dim x dim)
                if (gridComponents[index].collapsed)
                {
                    Debug.Log("called");
                    newGenerationCell[index] = gridComponents[index]; //if the cell is collapsed, just add it to the list as-is
                }
                else
                {
                    List<Tile> options = new List<Tile>(); //list of tileOptions for our current selected index
                    foreach (Tile t in tileObjects)
                    {
                        options.Add(t); //start by just adding all of them before filtering out invalid results
                    }

                    //next we cycle through all 4 neighbors and remove options if necessary
                    //update above
                    if (y > 0)
                    {
                        Cell up = gridComponents[x + (y - 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in up.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].upNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions); //this uses our generated validOptions list to validate the options list
                    }

                    //update right
                    if (x < dimensions - 1)
                    {
                        Cell right = gridComponents[x + 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in right.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].leftNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //look down
                    if (y < dimensions - 1)
                    {
                        Cell down = gridComponents[x + (y + 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in down.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].downNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //look left
                    if (x > 0)
                    {
                        Cell left = gridComponents[x - 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in left.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].rightNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //finally, with all invalid options removed, create an array to send to the cell
                    Tile[] newTileList = new Tile[options.Count];

                    for (int i = 0; i < options.Count; i++)
                    {
                        newTileList[i] = options[i];
                    }

                    newGenerationCell[index].RecreateCell(newTileList); //assign the new valid options array
                }
            }
        }

        gridComponents = newGenerationCell; //finally, set our permanent list == to the updated placeholder one
        iterations++;

        if(iterations < dimensions * dimensions) //every time we run this, if we see there's still empty cells, re-run entropy
        {
            StartCoroutine(CheckEntropy());
        }

    }


    //optionList here is the previous valid targets before entropy for tiles at a given cell
    //validOptions we'll have to calculate in a Generation() above and pass into here
    void CheckValidity(List<Tile> optionList, List<Tile> validOption)
    {
        //count down so we remove from the bottom of the list first
        for (int x = optionList.Count - 1; x >= 0; x--)
        {
            var element = optionList[x];
            if (!validOption.Contains(element)) 
            {
                optionList.RemoveAt(x); //if the list of valid options from our neighbors doesn't include X, remove it!
            }
        }
    }
}
