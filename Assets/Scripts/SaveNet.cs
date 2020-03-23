using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="new Neural Network", menuName = "Neural Network")]
public class SaveNet : ScriptableObject
{
    public int numberOfInputs;
    public int numberOfOutputs;
    public int numberOfHiddenLayers;
    public int numberOfNeuronsPerHiddenLayer;
    public List<MatrixWrapper> weights = new List<MatrixWrapper>();
    public List<float> biases = new List<float>();
    public float fitness;

    public void Save(int numberOfInputs, int numberOfOutputs, int numberOfNeuronsPerHiddenLayer, List<MatrixWrapper> weights, List<float> biases, float fitness = 0)
    {
        //Debug.Log("SaveNet: Start Saving");
        this.numberOfInputs = numberOfInputs;
        this.numberOfOutputs = numberOfOutputs;
        this.numberOfHiddenLayers = biases.Count-2;
        this.numberOfNeuronsPerHiddenLayer = numberOfNeuronsPerHiddenLayer;
        this.weights = weights;
        this.biases = biases;
        this.fitness = fitness;
        //Debug.Log(fitness);
        //Debug.Log("SaveNet: End Saving");
    }

    public NeuralNetwork ToNetwork()
    {
        //Debug.Log("SaveNet: Start Loading");
        NeuralNetwork net = new NeuralNetwork(this);
        for (int i = 0; i < weights.Count; i++)
        {
            weights[i].CreateInnerMatrix();
        }

        net.Initialise(numberOfInputs, numberOfOutputs, numberOfHiddenLayers, numberOfNeuronsPerHiddenLayer, weights, biases, fitness);
        //Debug.Log("SaveNet: End Loading");
        return net;
    }
}
