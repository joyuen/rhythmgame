using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    private bool flag = false;

    // Keep a reference of the conductor.
	public Conductor conductor;
    public float beat;

    private static double diagonalMoveSpeed;
    private static double cardinalMoveSpeed;

    private static double scaleSpeed;
    
    private Vector3 target;
    private Vector2 targetScale;
    private float currentPositionX;
    private float currentPositionY;
    private float currentPositionZ;
    private int gridPosition;

    private SpriteRenderer spriteRenderer;

    public AudioClip hitNote;

    public void Initialize(Conductor conductor, float beat)
	{
		this.conductor = conductor;
        this.beat = beat;

        spriteRenderer = GetComponent<SpriteRenderer>();
	}

    void OnMouseOver() {      
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X))
        {
            AudioSource.PlayClipAtPoint(hitNote, new Vector3(0, 0, 0), 0.5f);
            Destroy(gameObject);
        }
    }

    void Start() {
        // use difficulty to determine numbers, change just cardinal movespeed. Easy = 0.8x, Hard = 1.25x
        cardinalMoveSpeed = 1.00f;
        diagonalMoveSpeed = Math.Sqrt(Math.Pow(cardinalMoveSpeed,2)*2);
        scaleSpeed = cardinalMoveSpeed * 0.2f;

        spriteRenderer.color = new Color(1f, 1f, 1f, 0.35f);

        targetScale = new Vector2(0.14f, 0.14f);

        currentPositionX = transform.position.x;
        currentPositionY = transform.position.y;
        currentPositionZ = transform.position.z;

        if (currentPositionX == 0.3f) {
            if (currentPositionY == 0.3f) {
                // 6
                target = new Vector3(-0.5f, -0.5f, currentPositionZ);   
                gridPosition = 6;           
            }
            else if (currentPositionY == 0.5f) {
                // 3
                target = new Vector3(-0.5f, 0.5f, currentPositionZ);
                gridPosition = 3;  
            }
            else {
                // 0
                target = new Vector3(-0.5f, 1.5f, currentPositionZ); 
                gridPosition = 0; 
            }
        }
        else if (currentPositionX == 0.5f) {
            if (currentPositionY == 0.3f) {
                // 7
                target = new Vector3(0.5f, -0.5f, currentPositionZ); 
                gridPosition = 7; 
            }
            else if (currentPositionY == 0.5f) {
                // 4
                target = new Vector3(0.5f, 0.5f, currentPositionZ);
                gridPosition = 4;
            }
            else {
                // 1
                target = new Vector3(0.5f, 1.5f, currentPositionZ);
                gridPosition = 1;
            }
        }
        else {
            if (currentPositionY == 0.3f) {
                // 8
                target = new Vector3(1.5f, -0.5f, currentPositionZ); 
                gridPosition = 8;
            }
            else if (currentPositionY == 0.5f) {
                // 5
                target = new Vector3(1.5f, 0.5f, currentPositionZ); 
                gridPosition = 5;
            }
            else {
                // 2
                target = new Vector3(1.5f, 1.5f, currentPositionZ);
                gridPosition = 2;
            }
        }
    }

    void Update() {

        double diagonalMoveStep = diagonalMoveSpeed * Time.deltaTime;
        double cardinalMoveStep = cardinalMoveSpeed * Time.deltaTime;
        double scaleStep = scaleSpeed * Time.deltaTime;
        
        if (gridPosition == 1 || gridPosition == 3 || gridPosition == 5 || gridPosition == 7) {
            transform.position = Vector3.MoveTowards(transform.position, target, (float)cardinalMoveStep);
        }
        else {
            transform.position = Vector3.MoveTowards(transform.position, target, (float)diagonalMoveStep);
        }
        transform.localScale = Vector2.MoveTowards (transform.localScale, targetScale, (float)scaleStep);

        Color oldCol = spriteRenderer.color;
        spriteRenderer.color = new Color(oldCol.r, oldCol.g, oldCol.b, oldCol.a + conductor.secPerBeat * Time.deltaTime);

        // cleanup missed notes
        if (conductor.songPositionInBeats >= beat+1) {
            // Destroy(gameObject);
        }

        Vector2 v2 = transform.localScale;
        Vector3 v3 = transform.position;

        // if (v2 == targetScale && flag == false) {
        //     // conductor.musicSource.Stop();
        //     Debug.Log(transform.localScale);
        //     Debug.Log(conductor.songPosition);
        //     flag = true;
        // }

        // if (v3 == target && flag == false && v2 == targetScale) {
        //     // Debug.Log(conductor.songPosition);
        //     flag = true;
        //     AudioSource.PlayClipAtPoint(hitNote, new Vector3(0, 0, 0), 0.5f);
        //     // Destroy(gameObject);
        // }
    }
}
