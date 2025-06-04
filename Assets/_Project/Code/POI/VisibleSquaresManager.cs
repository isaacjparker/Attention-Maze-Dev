using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibleSquaresManager : MonoBehaviour
{
    public static VisibleSquaresManager Instance { get; private set; }

    private HashSet<SquareData> visibleSquares = new HashSet<SquareData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void AddSquare(SquareData square)
    { 
        visibleSquares.Add(square);
    }

    public void RemoveSquare(SquareData square)
    { 
        visibleSquares.Remove(square);
    }

    public IEnumerable<SquareData> GetVisibleSquares()
    { 
        return visibleSquares;
    }

}
