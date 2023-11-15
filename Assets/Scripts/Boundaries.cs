using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Boundary
{
    public float up, down, left, right;

    public Boundary(float up, float down, float left, float right)
    {
        this.up = up;
        this.down = down;
        this.left = left;
        this.right = right;
    }
}


public static class Boundaries
{
    static public Boundary agentPusherBoundarySoft = new Boundary(68.8f, 0f, -30f, 30f);
    static public Boundary humanPusherBoundarySoft = new Boundary(0, -68.8f, -30f, 30f);
    static public Boundary agentPusherBoundaryHard = new Boundary(73f, 0f, -36f, 36f);
    static public Boundary humanPusherBoundaryHard = new Boundary(0, -73f, -36f, 36f);
    static public Boundary puckBoundary = new Boundary(50f, -50f, -30f, 30f);

}
