using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using System;
using Random = UnityEngine.Random;

public class GeneticNetworkTrainer : MonoBehaviour
{
    [Header("Debug Info")]
    public int currentGeneration;
    public int currentGenome;
    public Text currentGenerationLabel;
    public Text currentGenomeLabel;

    [Header("Neural Network Parameters")]
    [SerializeField] int numberOfInputs;
    [SerializeField] int numberOfOutputs;
    public int numberOfHiddenLayers = 1;
    public int numberOfNeuronsPerHiddenLayer = 10;

    [Header("Subject to train")]
    public CarController car;

    [Header("Population Info")]
    public int populationSize = 100;
    private NeuralNetwork[] population;
    public int subjectsToResetAtEachGeneration = 10;

    [Header("Selection")]
    public int subpopulationSize = 30; //we use the tournament selection criterium with alfa = 30
    public int numParents = 25;

    [Header("Crossover")]
    [Range(0.1f, 0.9f)]
    public float swapChance = 0.5f;
    [Range(0f, 1f)]
    public float biasesInterpolationChance = 0.25f;

    [Header("Mutation")]
    [Range(0f, 1f)]
    public float mutationChance = 0.3f;
    public float minNoiseToAdd = -0.5f;
    public float maxNoiseToAdd = 0.5f;

    public void Start()
    {
        CurrentGeneration = 0;
        CurrentGenome = 0;
        population = new NeuralNetwork[populationSize];
        numberOfInputs = car.directions.Length;
        numberOfOutputs = 2;
        RandomizePopulationFromIndex(0);
    }

    public int CurrentGeneration
    {
        get
        {
            return currentGeneration;
        }
        set
        {
            currentGeneration = value;
            currentGenerationLabel.text = currentGeneration.ToString();
        }
    }

    public int CurrentGenome
    {
        get
        {
            return currentGenome;
        }
        set
        {
            currentGenome = value;
            currentGenomeLabel.text = currentGenome.ToString();
        }
    }

    public void RandomizePopulationFromIndex(int index)
    {
        for (int i = index; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(car.networkSaved);
            net.Initialise(numberOfInputs, numberOfOutputs, numberOfHiddenLayers, numberOfNeuronsPerHiddenLayer);
            population[i] = net;
        }
    }

    NeuralNetwork[] Selection()
    {
        Array.Sort(population, new FitnessComparer());
        NeuralNetwork[] tmp = new NeuralNetwork[numParents];
        for (int i = 0; i < numParents; i++)
        {
            int maxIndex = 0;
            for (int j = 0; j < subpopulationSize; j++)
            {
                int newIndex = Random.Range(0, populationSize);
                if (newIndex > maxIndex)
                {
                    maxIndex = newIndex;
                }
            }
            tmp[i] = population[maxIndex];
        }
        return tmp;
    }

    void CrossoverAndMutation(NeuralNetwork[] parents)
    {
        int i = 0;
        for (; i < parents.Length; i++)
        {
            population[i] = parents[i];
            population[i].fitness = parents[i].fitness;
        }

        for (; i < populationSize - subjectsToResetAtEachGeneration; i++)
        {
            NeuralNetwork child1 = new NeuralNetwork(car.networkSaved);
            NeuralNetwork child2 = new NeuralNetwork(car.networkSaved);
            NeuralNetwork p1 = parents[Random.Range(0, parents.Length)];
            NeuralNetwork p2 = parents[Random.Range(0, parents.Length)];

            List<MatrixWrapper> weights1 = new List<MatrixWrapper>();
            List<MatrixWrapper> weights2 = new List<MatrixWrapper>();

            for (int j = 0; j < p1.weights.Count; j++)
            {
                int rows = p1.weights[j].RowCount;
                int cols = p1.weights[j].ColumnCount;
                MatrixWrapper m1 = p1.weights[j].Clone();
                MatrixWrapper m2 = p2.weights[j].Clone();

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        if (Random.Range(0f, 1f) <= swapChance)
                        {
                            m1[r, c] = p2.weights[j][r, c];
                            m2[r, c] = p1.weights[j][r, c];
                        }
                        if (Random.Range(0f, 1f) <= mutationChance)
                        {
                            m1[r, c] += Random.Range(minNoiseToAdd, maxNoiseToAdd);
                            //Mathf.Clamp(m1[r, c], -1, 1);
                        }
                        if (Random.Range(0f, 1f) <= mutationChance)
                        { 
                            m2[r, c] += Random.Range(minNoiseToAdd, maxNoiseToAdd);
                            //Mathf.Clamp(m2[r, c], -1, 1);
                        }
                    }
                }
                weights1.Add(m1);
                weights2.Add(m2);

            }

            child1.Initialise(numberOfInputs, numberOfOutputs, numberOfHiddenLayers, numberOfNeuronsPerHiddenLayer, weights1);
            child2.Initialise(numberOfInputs, numberOfOutputs, numberOfHiddenLayers, numberOfNeuronsPerHiddenLayer, weights2);

            for (int j = 0; j < child1.biases.Count; j++)
            {
                if (Random.Range(0f, 1f) <= biasesInterpolationChance)
                {
                    float b1, b2;
                    float interpolator = Random.Range(0f, 1f);
                    b1 = child1.biases[j] * interpolator + child2.biases[j] * (1 - interpolator);
                    b2 = child2.biases[j] * interpolator + child1.biases[j] * (1 - interpolator);
                    child1.biases[j] = b1;
                    child2.biases[j] = b2;
                }
            }

            population[i] = child1;
            i++;
            if (i < populationSize - 1 - subjectsToResetAtEachGeneration) //needed if we have an odd number of subjects in the population
            {
                population[i] = child2;
            }
        }

        RandomizePopulationFromIndex(populationSize - 1 - subjectsToResetAtEachGeneration);

    }

    public void Death(float fitness)
    {
        population[CurrentGenome].fitness = fitness;
        CurrentGenome = (CurrentGenome + 1) % populationSize;
        if (CurrentGenome == 0)
        {
            CurrentGeneration++;
            CrossoverAndMutation(Selection());
        }
        car.Reset(population[CurrentGenome]);
    }

    public class FitnessComparer : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            NeuralNetwork n1 = (NeuralNetwork)x;
            NeuralNetwork n2 = (NeuralNetwork)y;
            return n1.fitness.CompareTo(n2.fitness);
        }
    }
}
