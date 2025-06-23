using UnityEngine;

public class BlockShape
{
    public int[,] shape;
    public int width { get { return shape.GetLength(1); } }
    public int height { get { return shape.GetLength(0); } }
    public int count;

    public void CalculateCount()
    {
        count = 0;
        for (int i = 0; i < shape.GetLength(0); i++)
        {
            for (int j = 0; j < shape.GetLength(1); j++)
            {
                if (shape[i,j] == 1) count++;
            }
        }
    }
}
