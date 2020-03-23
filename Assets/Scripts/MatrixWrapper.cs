using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

[System.Serializable]
public class MatrixWrapper
{
    [SerializeField] private float[] matrix;
    [SerializeField] private int rowCount = 0;
    [SerializeField] private int columnCount = 0;
    private Matrix<float> innerMatrix;

    public MatrixWrapper(float[,] values)
    {
        innerMatrix = Matrix<float>.Build.DenseOfArray(values);
        rowCount = values.GetLength(0);
        columnCount = values.GetLength(1);
        matrix = new float[rowCount * columnCount];
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                matrix[(i * columnCount) + j] = innerMatrix[i, j];
            }
        }
    }

    public MatrixWrapper(Matrix<float> values)
    {
        innerMatrix = values;
        rowCount = values.RowCount;
        columnCount = values.ColumnCount;
        matrix = new float[rowCount * columnCount];
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                matrix[(i * columnCount) + j] = innerMatrix[i, j];
            }
        }
    }

    public MatrixWrapper(float[] mat, int rows, int cols)
    {
        rowCount = rows;
        columnCount = cols;
        matrix = mat;
        CreateInnerMatrix();
    }

    public MatrixWrapper(int rows, int cols)
    {
        rowCount = rows;
        columnCount = cols;
        innerMatrix = Matrix<float>.Build.Dense(rows, cols);
        matrix = new float[rows * cols];
    }

    public float this[int i, int j]
    {
        get
        {
            return matrix[(i * columnCount) + j];
        }
        set
        {
            matrix[(i * columnCount) + j] = value;
            innerMatrix[i, j] = value;
        }
    }

    public int RowCount
    {
        get
        {
            return rowCount;
        }
    }

    public int ColumnCount
    {
        get
        {
            return columnCount;
        }
    }

    public void CreateInnerMatrix()
    {
        innerMatrix = Matrix<float>.Build.Dense(rowCount, columnCount);
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                innerMatrix[i, j] = matrix[(i * columnCount) + j];
            }
        }
    }

    public MatrixWrapper PointwiseTanh()
    {
        return new MatrixWrapper(innerMatrix.PointwiseTanh()); 
    }

    public static MatrixWrapper operator *(MatrixWrapper a, MatrixWrapper b)
    {
        return new MatrixWrapper(a.innerMatrix * b.innerMatrix);
    }

    public static MatrixWrapper operator +(MatrixWrapper a, float num)
    {
        return new MatrixWrapper(a.innerMatrix + num);
    }

    public MatrixWrapper Clone()
    {
        return new MatrixWrapper(innerMatrix.Clone());
    }
}
