using Cavern;
using System.Collections.Generic;
using UnityEngine;

public struct BoxEntry {
    public float Timestamp;
    public Vector2 Position;
    public Color Tint, Edge;
}

public class CubeDispenser : MonoBehaviour {
    public GameObject Cube;
    public Material CubeMat;
    public float Range = 30;
    [Tooltip("Additional units to travel by uncut cubes before disappearing.")]
    public float Overrun = 10;
    public float PathWidth = .75f;
    public float PathHeight = .75f;
    public float Speed = 15;
    public Vector3 Size = new Vector3(.3f, .3f, .3f);

    public AudioClip Song;
    public List<BoxEntry> Boxes;

    AudioSource3D Source;
    float Alive = 0;
    int Box = 0;
    List<GameObject> Cubes = new List<GameObject>();
    Queue<GameObject> Destroyables = new Queue<GameObject>();

    void Start() {
        Alive = Range / Speed;
        while (Box < Boxes.Count && Boxes[Box].Timestamp < Alive)
            ++Box;
        Source = gameObject.AddComponent<AudioSource3D>();
        Source.clip = Song;
        Source.SpatialBlend = 0;
    }

    void Update() {
        Alive = Source.time + Range / Speed;
        while (Box < Boxes.Count && Boxes[Box].Timestamp <= Alive) {
            GameObject NewCube = Instantiate(Cube, transform);
            NewCube.name = Boxes[Box].Timestamp.ToString();
            float Width = Boxes[Box].Position.x, Height = Boxes[Box].Position.y;
            NewCube.transform.localPosition = new Vector3(Width * PathWidth, Height * PathHeight, 0);
            NewCube.transform.localScale = Size;
            Material NewMat = new Material(CubeMat);
            NewMat.SetColor("_Color", Boxes[Box].Tint);
            NewMat.SetColor("_Edge", Boxes[Box].Edge);
            NewCube.GetComponent<Renderer>().material = NewMat;
            Cubes.Add(NewCube);
            ++Box;
        }
        foreach (GameObject MovingCube in Cubes) {
            if (!MovingCube || MovingCube.transform.localPosition.z < -Overrun)
                Destroyables.Enqueue(MovingCube);
            else {
                Vector3 CurrentPos = MovingCube.transform.localPosition;
                float Distance = (float.Parse(MovingCube.name) - Alive) * Speed + Range;
                MovingCube.transform.localPosition = new Vector3(CurrentPos.x, CurrentPos.y, Distance);
            }
        }
        while (Destroyables.Count != 0) {
            GameObject Target = Destroyables.Dequeue();
            Cubes.Remove(Target);
            if (Target) {
                Destroy(Target.GetComponent<Renderer>().material);
                Destroy(Target);
            }
        }
    }
}