using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class plant : MonoBehaviour
{
    private class State
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 headed;
        public float width;
        public int depth;
        public State Clone()
        {
            return (State)MemberwiseClone();
        }
    }

    private Color c1 = new Color(1,1,1);
    private Color c2 = new Color(0,0,0);

    private Stack<State> states = new Stack<State>();

    private GameObject plantModel;

    public string axiom = "F";
    
    private string output;

    [Range(0, 10)]
    public float initialLength = 1.0f;

    [Range(0, 5)]
    public float initialWidth = 2.5f;

    [Range(0, 2)]
    public float widthFactor = 0.1f;

    [Range(0, 2)]
    public float lengthFact = 1.0f;

    public bool waveOn = false;

    [Range(0, 5)]
    public float waveStrength = 0.05f;

    public bool texture = true;

    [Range(0, 360)]
    public int mainAngle = 30;

    [Range(0, 180)]
    public int wobbleStrength = 5;

    public Dictionary<char, List<string>> rules = new Dictionary<char, List<string>>();

    [Range(0, 10)]
    public int iterations = 3;

    public Material material;

    private State currentState;

    private List<GameObject> allPlants = new List<GameObject>();

    private void Awake()
    {
        rules.Add('P', new List<string>(new string[] { "F[+P][-P][&P][^P]" }));

        rules.Add('L', new List<string>(new string[] { "[+S][-S][&S][^S]I" }));

        rules.Add('K', new List<string>(new string[] { "[+R]//[+R]//[+R]//[+R]" }));

        rules.Add('S', new List<string>(new string[] { "I", "" }));
        rules.Add('I', new List<string>(new string[] { "FG" }));
        rules.Add('G', new List<string>(new string[] { "[-FG][^FG][+F[+FG][-F[&FG]FG]]", "FG", "F[+FG][&FG]FG", "[-FG][+FG]", "[-FI][+FI]F^FG", "[-FI^FG]+F[-FG^FG]F-FG", "F[^FG][&FG]" }));


        rules.Add('R', new List<string>(new string[] { "F[R]-F[-R]F[+R][-R]R" }));
    }

    // Use this for initialization
    void Start()
    {
        
    }


    void WaveMovement(GameObject plants)
    {
        foreach (Transform branch in plants.transform)
        {
            //if plant is out of draw distance, s
            if (!branch.GetComponent<LineRenderer>().isVisible) return;

            if (branch.name == "branch")
            {
                var dataHolder = branch.GetComponent<DataHolder>();

                Vector3[] oldPositions = dataHolder.data;

                LineRenderer lineRenderer = branch.GetComponent<LineRenderer>();

                int count = lineRenderer.positionCount;

                var newPositions = new Vector3[count];

                var t = Time.time;

                for (int i = 0; i < count; i++)
                {
                    float sin = Mathf.Sin(t + oldPositions[i].y) * 0.06f * oldPositions[i].y;
                    newPositions[i] = new Vector3(waveStrength * sin + oldPositions[i].x, oldPositions[i].y, oldPositions[i].z);
                }
                lineRenderer.SetPositions(newPositions);
            }
            
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (waveOn)
        {
            foreach (GameObject go in allPlants)
            {
                if(go.name == "Plant")
                {
                    WaveMovement(go);
                }
            }
        }        
    }

    public void KillOld()
    {
        Destroy(plantModel);
    }

    public void BuildNew(float x, float y, float z)
    {
        currentState = new State() { position = new Vector3(x, y, z), rotation = Quaternion.identity, headed = new Vector3(0, initialLength, 0), width = initialWidth, depth = 0 };
                
        plantModel = new GameObject("Plant");
        plantModel.transform.parent = this.transform;
        plantModel.transform.position = currentState.position;

        output = axiom;
        // Apply rules iterations times
        for (int i = 0; i < iterations; i++)
        {
            print(output + "\n");
            output = Replace(output);
        }

        GeneratePlantFromString();
        allPlants.Add(plantModel);
    }    

    private string Replace(string input)
    {
        StringBuilder sb = new StringBuilder();
        foreach (char c in input)
        {
            // If there are rules for character c, replace c with random one
            if (rules.ContainsKey(c))
            {
                int randomRule = Random.Range(0, rules[c].Count);
                sb.Append(rules[c][randomRule]);
            }
            // Else don't replace
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    private GameObject branch;
    private LineRenderer lr;

    private void drawLine(Vector3 vec1, Vector3 vec2, float fromWidth)
    {
        //Create a gameObject for the branch
        branch = new GameObject("branch");

        //Make the branch child of the main gameobject
        branch.transform.parent = plantModel.transform;
        


        var origVectors = branch.AddComponent<DataHolder>();

        origVectors.data = new Vector3[] { vec1, vec2 };

        //Add a line renderer to the branch gameobject
        lr = branch.AddComponent<LineRenderer>() as LineRenderer;

        //Change the material of the LineRenderer
        lr.material = material;

        //Thin the branches through each iteration
        lr.startWidth = fromWidth;
        lr.endWidth = fromWidth * widthFactor;

        //lr.numCapVertices = 1;

        //Draw the line.
        lr.SetPosition(0, vec1);
        lr.SetPosition(1, vec2);
    }
    
    private void DrawLines(State[] vecc)
    {
        float fromWidth = vecc[0].width;
        Vector3[] vec = new Vector3[vecc.Length];
        float tmpTotalLength = 0.0f;
        for(int i = 0; i < vecc.Length; i++)
        {
            vec[i] = vecc[i].position;
            if(i < vecc.Length-1)
            {
                tmpTotalLength += vecc[i].headed.y;
            }
        }
        if(vec.Length > 1)
        {

            branch = new GameObject("branch");
            branch.transform.parent = plantModel.transform;
            branch.transform.position = vec[0];

            var origVectors = branch.AddComponent<DataHolder>();
            origVectors.data = vec;

            lr = branch.AddComponent<LineRenderer>() as LineRenderer;

            if(texture)lr.material = material;
            if(!texture)lr.material = new Material(Shader.Find("Sprites/Diffuse"));
            
            /*
            float[] tmp = new float[vec.Length-1];
            float randTmp = 4;
            float tmpSum = 0f;
            for(int i = 0; i < vec.Length-1; i++)
            {
                tmpSum += randTmp;
                tmp[i] = randTmp;
                randTmp *= lengthFact;
                print(tmpSum);
            }
            */

            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0.0f, fromWidth);

            float tmpCurrLength = 0.0f;
            foreach(State st in vecc)
            {
                curve.AddKey(tmpCurrLength / tmpTotalLength, st.width);
                tmpCurrLength += st.headed.y;
            }

            lr.textureMode = LineTextureMode.Tile;

            //lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
                        

            lr.widthCurve = curve;
            lr.widthMultiplier = 1f;
            

            lr.receiveShadows = true;
            lr.generateLightingData = true;

            //lr.Simplify(1);
            
            lr.positionCount = vec.Length;
            lr.SetPositions(vec);


            
            if (!texture)
            {
                float alpha = 1.0f;
                Gradient gradient = new Gradient();

                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(c2, 0.0f), new GradientColorKey(c1, 1.0f)},
                    new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
                    );
                lr.colorGradient = gradient;

            }
            
        }
    }

    private void MoveForward()
    {
        /*
        Vector3 tmp = new Vector3();
        tmp = currentState.position;
        */
        currentState.position += currentState.rotation * currentState.headed;
        currentState.depth++;
        currentState.headed.y *= lengthFact;

        //drawLine(tmp, currentState.position, currentState.width);
        currentState.width *= widthFactor;
    }
    
    private Quaternion Rotate(int axis, int angle)
    {
        switch (axis)
        {
            case 1:
                return Quaternion.Euler(angle, 0, 0);
            case 2:
                return Quaternion.Euler(0, angle, 0);
            case 3:
                return Quaternion.Euler(0, 0, angle);
            default:
                return Quaternion.Euler(0, 0, 0);
        }
    }
    
    private List<State> statesOfBranch = new List<State>();

    private void GeneratePlantFromString()
    {

        states.Push(currentState);
        
        statesOfBranch.Add(currentState.Clone());

        int trisCount = 0;
        
        for(int i = 0; i < output.Length; i++)
        {
            char c = output[i];
            switch (c)
            {
                case 'F':
                    MoveForward();
                    trisCount += 2;
                    
                    statesOfBranch.Add(currentState.Clone());

                    break;
                case '[':

                    states.Push(currentState.Clone());
                    break;
                case ']':
                    currentState = states.Pop();
                    ///////
                    DrawLines(statesOfBranch.ToArray());
                    statesOfBranch.Clear();
                    statesOfBranch.Add(currentState.Clone());
                    ///////

                    break;

                case '+':
                    currentState.rotation *= Rotate(3, mainAngle);
                    break;
                case '-':
                    currentState.rotation *= Rotate(3, -mainAngle);
                    break;
                case '^':
                    currentState.rotation *= Rotate(1, mainAngle);
                    break;
                case '&':
                    currentState.rotation *= Rotate(1, -mainAngle);
                    break;
                case '/':
                    currentState.rotation *= Rotate(2, mainAngle);
                    break;
                case '\\':
                    currentState.rotation *= Rotate(2, -mainAngle);
                    break;

                case 'W':
                    currentState.rotation *= Rotate(Random.Range(1, 4), wobbleStrength);
                    break;
                default:
                    break;
                
            }
        }
        DrawLines(statesOfBranch.ToArray());
        statesOfBranch.Clear();


        print("triscount : " + trisCount);

        int branches = 0;
        foreach (GameObject go in FindObjectsOfType(typeof(GameObject)) as GameObject[])
        {
            if (go.name == "branch")
            {
                branches++;
            }
        }
        print("branches : " + branches);
        

    }
}

public class DataHolder : MonoBehaviour
{
    public Vector3[] data;
}