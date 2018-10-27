using Cavern;
using System.Collections.Generic;
using UnityEngine;

public class CubeDispenser : MonoBehaviour {
    public GameObject Cube;
    public Material CubeMat;
    public float Range = 30;
    public float PathWidth = .75f;
    public float PathHeight = .75f;
    public float Speed = 15;
    public Vector3 Size = new Vector3(.3f, .3f, .3f);

    public AudioClip Song;
    public List<BoxEntry> Boxes;

    float Alive = 0;
    int Box = 0;
    List<GameObject> Cubes = new List<GameObject>();
    Queue<GameObject> Destroyables = new Queue<GameObject>();

    void Start() {
        Alive = Range / Speed;
        while (Box < Boxes.Count && Boxes[Box].Timestamp < Alive)
            ++Box;
        AudioSource3D Source = gameObject.AddComponent<AudioSource3D>();
        Source.clip = Song;
        Source.SpatialBlend = 0;
    }

    void Update() {
        while (Box < Boxes.Count && Boxes[Box].Timestamp < Alive) {
            GameObject NewCube = Instantiate(Cube);
            NewCube.name = Boxes[Box].Timestamp.ToString();
            float Width = Boxes[Box].Position.x, Height = Boxes[Box].Position.y;
            NewCube.transform.localPosition = new Vector3(Width * PathWidth, Height * PathHeight, Range);
            NewCube.transform.localScale = Size;
            Material NewMat = new Material(CubeMat);
            NewMat.SetColor("_Color", Boxes[Box].Tint);
            NewMat.SetColor("_Edge", Boxes[Box].Edge);
            NewCube.GetComponent<Renderer>().material = NewMat;
            Cubes.Add(NewCube);
            ++Box;
        }
        float Direction = -Speed * Time.deltaTime;
        foreach (GameObject MovingCube in Cubes) {
            if (!MovingCube || MovingCube.transform.localPosition.z < 0)
                Destroyables.Enqueue(MovingCube);
            else
                MovingCube.transform.Translate(0, 0, Direction, Space.Self);
        }
        while (Destroyables.Count != 0) {
            GameObject Target = Destroyables.Dequeue();
            Cubes.Remove(Target);
            if (Target) {
                Destroy(Target.GetComponent<Renderer>().material);
                Destroy(Target);
            }
        }
        Alive += Time.deltaTime;
    }
}