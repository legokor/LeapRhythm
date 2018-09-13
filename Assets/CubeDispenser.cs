using Cavern;
using System.Collections.Generic;
using UnityEngine;

public class CubeDispenser : MonoBehaviour {
    public GameObject Cube;
    public Material LeftSide;
    public Material RightSide;
    public float Range = 10;
    public float PathWidth = 2;
    public float PathHeight = 1;
    public float Speed = 3;
    public float NewCubeIn = 1;
    public Vector3 Size = new Vector3(.5f, .5f, .5f);

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
        gameObject.AddComponent<AudioSource3D>().clip = Song;
    }

    void Update() {
        while (Box < Boxes.Count && Boxes[Box].Timestamp < Alive) {
            GameObject NewCube = Instantiate(Cube);
            float Width = Boxes[Box].Position.x;
            NewCube.transform.localPosition = new Vector3(Width * PathWidth, Boxes[Box].Position.y, Range);
            NewCube.transform.localScale = Size;
            NewCube.GetComponent<Renderer>().material = Width < 0 ? LeftSide : RightSide;
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
            Destroy(Target);
        }
        Alive += Time.deltaTime;
    }
}