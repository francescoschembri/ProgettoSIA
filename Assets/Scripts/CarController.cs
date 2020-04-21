using UnityEngine;

public class CarController : MonoBehaviour
{
    private Vector3 startPosition, startRotation;

    [Header("Movement")]
    [Range(0f, 1f)]
    public float displacementPercentage;
    [Range(-1f, 1f)]
    public float turningPercentage;
    public float turningAngle = 9;
    public float maxDisplacement = 0.15f;

    [SerializeField] float timeSinceStart = 0f;
    [SerializeField] float minTimeBeforeResetAlive = 10;

    [Header("Neural Network Options")]
    public bool controlledByNeuralNetwork = true;
    public bool trainNetwork = true;
    public SaveNet networkSaved;
    private NeuralNetwork network;
    private GeneticNetworkTrainer evolutionManager;

    [Header("Fitness")]
    [SerializeField] float overallFitness;
    [SerializeField] float maxOverallFitness = 2000f;
    [SerializeField] float minOverallFitness = 25f;

    [Header("Fitness Multiplier")]
    [SerializeField] float distanceMultipler = 10f;
    [SerializeField] float avgSpeedMultiplier = 0.3f;

    [Header("Other variables")]
    private Vector3 lastPosition;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxStopTime = 1f;
    [SerializeField] private float stopTime = 0f;
    [SerializeField] private float totalDistanceTravelled;
    [SerializeField] private bool safe;

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
            network = networkSaved.ToNetwork();
        }
    
    }

    public void Reset(NeuralNetwork net)
    {
        network = net;
        Reset();
    }

    public void Reset()
    {
        safe = false;
        timeSinceStart = 0;
        displacementPercentage = 0f;
        turningPercentage = 0f;
        totalDistanceTravelled = 0f;
        stopTime = 0f;
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

    private void Update()
    {
        if (lastPosition == transform.position)
        {
            stopTime += Time.deltaTime;
        }
        else
        {
            stopTime = 0f;
        }

        if (stopTime > maxStopTime)
        {
            Reset(1);
        }
        else
        {
            timeSinceStart += Time.deltaTime;
            lastPosition = transform.position;

            if (controlledByNeuralNetwork)
            {
                InputSensors();
                (displacementPercentage, turningPercentage) = network.RunNetwork(sensors);
            }
            //else move with a controller
            MoveCar(displacementPercentage, turningPercentage);


            if (trainNetwork)
            {
                CalculateFitness();
            }
        }
    }

    private void InputSensors()
    {
        Ray r = new Ray(transform.position, transform.TransformDirection(directions[0]));
        RaycastHit hit;

        for (int i = 0; i<directions.Length; i++)
        {
            r.direction = transform.TransformDirection(directions[i]);
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
        float avgSpeed = totalDistanceTravelled / timeSinceStart;

        overallFitness = totalDistanceTravelled * distanceMultipler //needed to make the net learn to drive properly
                        + avgSpeed * avgSpeedMultiplier; //needed to make the net learn to go faster

        if (timeSinceStart > minTimeBeforeResetAlive && (overallFitness < minOverallFitness || (!safe && Vector3.Distance(transform.position, startPosition) < minDistance)))
        {
            Debug.Log("BadNet");
            Reset(1);
        }
        if (timeSinceStart > minTimeBeforeResetAlive)
        {
            safe = true;
        }
        if (overallFitness >= maxOverallFitness)
        {
            Debug.Log("TopNet");
            network.fitness = overallFitness;
            network.Save();
            Reset(overallFitness);
        }
    }

    public void MoveCar(float acceleration, float turning)
    {
        transform.position += transform.TransformDirection(new Vector3(0, 0, acceleration * maxDisplacement));
        transform.eulerAngles += new Vector3(0, turning * turningAngle, 0);
    }
}
