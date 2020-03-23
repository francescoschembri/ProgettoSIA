using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private Vector3 startPosition, startRotation;

    [Header("Movement")]
    [Range(0f, 1f)]
    public float acceleration;
    [Range(-1f, 1f)]
    public float turning;
    public float turningAngle = 90;
    public float accelerationSpeed = 0.05f;
    public float accelerationPercentage = 0.02f;
    public float turningPercentage = 0.1f;

    [SerializeField] float timeSinceStart = 0f;
    [SerializeField] float minTimeBeforeResetAlive = 20;

    [Header("Neural Network Options")]
    public bool controlledByNeuralNetwork = true;
    public bool trainNetwork = true;
    public SaveNet networkSaved;
    //ONLY FOR TRAINING PURPOSE WE WANT TO LOAD THE NETWORK NOT TRAIN IT.
    private NeuralNetwork network;
    private GeneticNetworkTrainer evolutionManager;

    [Header("Fitness")]
    public bool enableFitnessCap = false;
    [SerializeField] float overallFitness;
    [SerializeField] float maxOverallFitness = 2000f;
    [SerializeField] float minOverallFitness = 10f;
    [SerializeField] float distanceMultipler = 10f;
    [SerializeField] float avgSpeedMultiplier = 0.2f;
    [SerializeField] float sensorMultiplier = 0.1f;

    private Vector3 lastPosition;
    [SerializeField] private float totalDistanceTravelled;
    [SerializeField] private float avgSpeed;

    private Vector3 input;

    [Header("Sensors")]
    public float sensorAttenuation = 10f;
    public Vector3[] directions;
    [SerializeField] private float[] sensors;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        sensors = new float[directions.Length];

        if (trainNetwork)
        {
            network = new NeuralNetwork(networkSaved);
            evolutionManager = FindObjectOfType<GeneticNetworkTrainer>();
            network.Initialise(directions.Length, 2, evolutionManager.numberOfHiddenLayers, evolutionManager.numberOfNeuronsPerHiddenLayer);
        }
        else
        {
            //Debug.Log("Car Controller Network: Start Loading");
            network = networkSaved.ToNetwork();
            //Debug.Log("Car Controller Network: End Loading");
        }
    
    }

    public void Reset(NeuralNetwork net)
    {
        network = net;
        Reset();
    }

    public void Reset()
    {
        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }

    private void Reset(float fitness)
    {
        if (evolutionManager != null && trainNetwork)
        {
            evolutionManager.Death(fitness);
        }
        else
        {
            Reset();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Reset(overallFitness);
    }

    private void FixedUpdate()
    {

        lastPosition = transform.position;

        if (controlledByNeuralNetwork)
        {
            InputSensors();
            (acceleration, turning) = network.RunNetwork(sensors);
        }
        //else move with a controller

        MoveCar(acceleration, turning);

        timeSinceStart += Time.deltaTime;

        CalculateFitness();

    }


    private void InputSensors()
    {
        Vector3[] localDirections = new Vector3[directions.Length];
        for(int i = 0; i < directions.Length; i++)
        {
            localDirections[i] = transform.TransformDirection(directions[i]);
        }

        Ray r = new Ray(transform.position, localDirections[0]);
        RaycastHit hit;

        for (int i = 0;i<localDirections.Length; i++)
        {
            r.direction = localDirections[i];
            if (Physics.Raycast(r, out hit))
            {
                sensors[i] = hit.distance / sensorAttenuation;
                Debug.DrawLine(r.origin, hit.point, Color.red);
            }
        }
    }

    private void CalculateFitness()
    {
        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
        avgSpeed = totalDistanceTravelled / timeSinceStart;

        float totalSensors = 0;
        foreach(float s in sensors)
        {
            totalSensors += s;
        }

        overallFitness = totalDistanceTravelled * distanceMultipler //needed to make the net learn to drive properly
                        + avgSpeed * avgSpeedMultiplier //needed to make the net learn to go faster
                        + totalSensors / sensors.Length * sensorMultiplier; //needed to make the net learn to stay in the middle of the road 

        if (timeSinceStart > minTimeBeforeResetAlive && overallFitness < minOverallFitness)
        {
            Reset(overallFitness);
        }

        if (overallFitness >= maxOverallFitness && enableFitnessCap)
        {
            //Debug.Log("CarController: Start Saving");
            network.fitness = overallFitness;
            network.Save();
            //Debug.Log("CarController: End Saving");
            Reset(overallFitness);
        }
    }


    public void MoveCar(float acceleration, float turning)
    {
        input = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, acceleration * accelerationSpeed), accelerationPercentage);
        input = transform.TransformDirection(input);
        Vector3 tmp = input + transform.position;

        transform.position += input;
        transform.eulerAngles += new Vector3(0, (turning * turningAngle) * turningPercentage, 0);
    }

    public void ModifyNetwork(NeuralNetwork newNetwork)
    {
        network = newNetwork;
    }
}
