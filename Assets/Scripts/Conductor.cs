using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.IO;

public class Conductor : MonoBehaviour
{

    //-------------------Music---------------------------------//
    //Song beats per minute
    //This is determined by the song you're trying to sync up to
    public float songBpm;

    //The number of seconds for each song beat
    public float secPerBeat;

    //Current song position, in seconds
    public float songPosition;

    //Current song position, in beats
    public float songPositionInBeats;

    public float startingSongPosition;

    //How many seconds have passed since the song started
    public float dspSongTime;

    //an AudioSource attached to this GameObject that will play the music.
    public AudioSource audioSource;
    public float startingTime;

    public bool gameIsPaused = false;

    private bool songStarted = false; 

    //-------------------Notes---------------------------------//

    //keep all the position-in-beats of notes in the song
    [System.Serializable]
    public class MusicNote{
        public float timestamp;
        public int gridPosition;
        public int zAxis;

        public MusicNote(float timestamp, int gridPosition, int zAxis)
        {
            this.timestamp = timestamp;
            this.gridPosition = gridPosition;
            this.zAxis = zAxis;
        }
    }
    public List<MusicNote> noteTrack = new List<MusicNote>();


    // Create noteTrack list using midi file
    public static MidiFile midiFile;
    public string fileLocation;
    private List<double> timeStamps = new List<double>();

    private string beatMapPath = "Assets/beatmap.csv";
    private string createPath = "Assets/temp.csv";

    //the index of the next note to be spawned
    public int nextIndex = 0;
    public int futureIndex = 0;

    private Vector3 position;

    public GameObject NoteObjectRed;
    public GameObject NoteObjectBlue;
    private GameObject NoteToUse;

    public bool isNoteRed = true;

    //-------------------Functions---------------------------------//

    // Start is called before the first frame update
    void Start()
    {

        // dont want to use midi as final implementation
        // need to find a way to extract the noteTrack list to allow editing
        // midi can be used to create baseline beat map

        // midiFile = MidiFile.Read(Application.streamingAssetsPath + "/" + fileLocation);
        // var midiNotes = midiFile.GetNotes();
        // var noteArray = new Melanchall.DryWetMidi.Interaction.Note[midiNotes.Count];
        // midiNotes.CopyTo(noteArray, 0);
        // foreach (var note in noteArray) {
        //     var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, midiFile.GetTempoMap());
        //     timeStamps.Add((double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f);
        // }

        // StringBuilder sb = new StringBuilder();
        // foreach (var stamp in timeStamps) {
        //     sb.AppendLine($"{stamp},0,0");
        // }
        // File.WriteAllText(createPath, sb.ToString());
        

        // Load beatmap from csv file
        var linesRead = File.ReadLines(beatMapPath);
        foreach (var line in linesRead) {
            string[] temp = line.Split(',');
            MusicNote newNote = new MusicNote((float)Convert.ToDouble(temp[0]), Convert.ToInt32(temp[1]), Convert.ToInt32(temp[2]));
            noteTrack.Add(newNote);
        }

        //Load the AudioSource attached to the Conductor GameObject
        audioSource = GetComponent<AudioSource>();

        //Calculate the number of seconds in each beat
        secPerBeat = 60f / songBpm;

        //Record the time when the music starts
        dspSongTime = (float)AudioSettings.dspTime;

        startingSongPosition = ((float)(AudioSettings.dspTime - dspSongTime) + startingTime);

        // //Start the music
        audioSource.time = startingTime; 
        
        // PlayScheduled 
    }

    void Init(GameObject NoteObjectRed, Vector3 positions, MusicNote noteToInit) {

        Note musicNote = ((GameObject) Instantiate(NoteObjectRed, positions, Quaternion.identity)).GetComponent<Note>();
        musicNote.Initialize(this, noteToInit.timestamp);
    }

    void switchInit(int notePosition) 
    {
        if (notePosition == 0) {
            Init(NoteToUse, new Vector3 (0.3f, 0.7f, nextIndex), noteTrack[nextIndex]);
        }
        else if (notePosition == 1) {
            Init(NoteToUse, new Vector3 (0.5f, 0.7f, nextIndex), noteTrack[nextIndex]);
        }
        else if (notePosition == 2) {
            Init(NoteToUse, new Vector3 (0.7f, 0.7f, nextIndex), noteTrack[nextIndex]);
        }
        else if (notePosition == 3) {
            Init(NoteToUse, new Vector3 (0.3f, 0.5f, nextIndex), noteTrack[nextIndex]);
        }
        else if (notePosition == 4) {
            Init(NoteToUse, new Vector3 (0.5f, 0.5f, nextIndex), noteTrack[nextIndex]);
        }
        else if (notePosition == 5) {
            Init(NoteToUse, new Vector3 (0.7f, 0.5f, nextIndex), noteTrack[nextIndex]);
        }
        else if (notePosition == 6) {
            Init(NoteToUse, new Vector3 (0.3f, 0.3f, nextIndex), noteTrack[nextIndex]);
        }
        else if (notePosition == 7) {
            Init(NoteToUse, new Vector3 (0.5f, 0.3f, nextIndex), noteTrack[nextIndex]);
        }
        else {
            Init(NoteToUse, new Vector3 (0.7f, 0.3f, nextIndex), noteTrack[nextIndex]);
        }
    }

    void PauseGame ()
    {
        if(gameIsPaused)
        {
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }
        else 
        {
            Time.timeScale = 1;
            AudioListener.pause = false;
        }
    }

    // // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !songStarted)
        {
            audioSource.Play();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameIsPaused = !gameIsPaused;
            PauseGame();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }

        //determine how many seconds since the song started
        songPosition = ((float)(AudioSettings.dspTime - dspSongTime) + startingTime);

        double sourceTime = (double)audioSource.timeSamples / audioSource.clip.frequency;
        if (nextIndex < noteTrack.Count && sourceTime >= noteTrack[nextIndex].timestamp - 1 && noteTrack[nextIndex].timestamp > startingSongPosition)
        {
            int notePosition = noteTrack[nextIndex].gridPosition;

            if (isNoteRed) {
                NoteToUse = NoteObjectRed;
                isNoteRed = false;
            }
            else {
                NoteToUse = NoteObjectBlue;
                isNoteRed = true;
            }
            switchInit(notePosition);
            nextIndex++;     
        }
        else if (nextIndex < noteTrack.Count && noteTrack[nextIndex].timestamp < startingSongPosition) {
            nextIndex++; 
        }
    }
}