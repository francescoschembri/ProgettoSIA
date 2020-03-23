using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class NeuralNetwork
{
    public MatrixWrapper inputLayer;

    public List<MatrixWrapper> weights = new List<MatrixWrapper>();
    public List<float> biases = new List<float>();

    public float fitness;
    private int numberOfOutputs;
    private int numberOfNeuronsPerHiddenLayer;

    public SaveNet savedNet;


    public NeuralNetwork(SaveNet savedNet)
    {
        this.savedNet = savedNet;
    }

    public void Initialise(int numberOfInputs, int numberOfOutputs, int numberOfHiddenLayers,
                            int numberOfNeuronsPerHiddenLayer, List<MatrixWrapper> w = null, List<float> b = null, float fitness = 0f)
    {
        inputLayer = new MatrixWrapper(1, numberOfInputs);
        this.fitness = fitness;
        this.numberOfOutputs = numberOfOutputs;
        this.numberOfNeuronsPerHiddenLayer = numberOfNeuronsPerHiddenLayer;

        //biases
        if (b == null)
        {
            biases.Clear();
            for(int i = 0; i < numberOfHiddenLayers+2; i++)
                biases.Add(Random.Range(-1f, 1f));
        }
        else
        {
            biases = b;
        }

        //weights
        if (w == null)
        {
            weights.Clear();
            int prevNumberOfNeurons = numberOfInputs;

            for (int i = 0; i < numberOfHiddenLayers; i++)
            {
                MatrixWrapper weight = new MatrixWrapper(prevNumberOfNeurons, numberOfNeuronsPerHiddenLayer);
                weights.Add(weight);
                prevNumberOfNeurons = numberOfNeuronsPerHiddenLayer;
            }

            weights.Add(new MatrixWrapper(numberOfNeuronsPerHiddenLayer, numberOfOutputs));

            RandomiseWeights();
        }
        else
        {
            weights = w;
        }
    }

    public void RandomiseWeights()
    {
        for (int i = 0; i < weights.Count; i++)
        {
            for (int x = 0; x < weights[i].RowCount; x++)
            {
                for (int y = 0; y < weights[i].ColumnCount; y++)
                {
                    weights[i][x, y] = Random.Range(-1f, 1f);
                }
            }
        }
    }

    public (float, float) RunNetwork(float[] input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            inputLayer[0, i] = input[i];
        }

        MatrixWrapper lastLayer = (inputLayer.PointwiseTanh() * weights[0] + biases[0]);

        for (int i = 1; i < weights.Count; i++)
        {
            lastLayer = lastLayer.PointwiseTanh() * weights[i] + biases[i];
        }

        lastLayer = lastLayer.PointwiseTanh();

        return (Sigmoid(lastLayer[0, 0]), (float)Math.Tanh(lastLayer[0, 1]));
    }

    private float Sigmoid(float s)
    {
        return (1 / (1 + Mathf.Exp(-s)));
    }

    public void Save(bool forceSave = false)
    {
        if (fitness >= savedNet.fitness || forceSave)
        {
            savedNet.Save(inputLayer.ColumnCount, numberOfOutputs, numberOfNeuronsPerHiddenLayer, weights, biases, fitness);
        }
    }
}
