using UnityEngine;
using System.Collections;

public class StartButton : MonoBehaviour {

    public void StartClicked() //load scene game khi player bat nut start
    {
            Application.LoadLevel("Game");
    }
}
