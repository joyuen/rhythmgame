using System;
using System.Diagnostics;
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

    public double startingSongPosition;

    //an AudioSource attached to this GameObject that will play the music.
    public AudioSource audioSource;

    //Starting time for music (can be used to play song from further point in time)
    public float startingTime;

    private double noteDelay;

    //------------------- Dynamic Music Variables ---------------------------------//

    //Current song position, in seconds
    public double songPosition;

    //Current song position, in beats
    public float songPositionInBeats;

    //How many seconds have passed since the song started
    public double dspSongTime;

    //Has the song started
    private bool songStarted = false;

    //------------------- Pause Variables ---------------------------------//

    public bool gameIsPaused = false;   

    //------------------- Notes Variables ---------------------------------//

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
    private double travelTime = 0.80057407142857142857142857142857;
    public Stopwatch timer;
    public TimeSpan timeElapsed { get; private set; }
    public double elapsedTime;


    // Create noteTrack list using midi file
    public static MidiFile midiFile;
    public string fileLocation;
    private List<double> timeStamps = new List<double>();

    private string beatMapPath = "Assets/The Sport Electro.csv";
    private string createPath = "Assets/temp.csv";

    // private string songData = "Assets/The Sport Electro.txt";

    //the index of the next note to be spawned
    public int nextIndex = 0;

    private Vector3 position;

    public GameObject NoteObjectRed;
    public GameObject NoteObjectBlue;
    private GameObject NoteToUse;

    public bool isNoteRed = true;

    //-------------------Functions---------------------------------//

    // void GenerateBeatmap()
    // {
    //     // dont want to use midi as final implementation
    //     // need to find a way to extract the noteTrack list to allow editing
    //     // midi can be used to create baseline beat map

    //     midiFile = MidiFile.Read(Application.streamingAssetsPath + "/" + fileLocation);
    //     var midiNotes = midiFile.GetNotes();
    //     var noteArray = new Melanchall.DryWetMidi.Interaction.Note[midiNotes.Count];
    //     midiNotes.CopyTo(noteArray, 0);
    //     foreach (var note in noteArray) {
    //         var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, midiFile.GetTempoMap());
    //         timeStamps.Add((double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f);
    //     }

    //     StringBuilder sb = new StringBuilder();
    //     foreach (var stamp in timeStamps) {
    //         sb.AppendLine($"{stamp},0,0");
    //     }
    //     File.WriteAllText(createPath, sb.ToString());
    // }

    // Start is called before the first frame update
    void Start()
    {

        // GenerateBeatmap()

        // Load beatmap from csv file
        var linesRead = File.ReadLines(beatMapPath);
        foreach (var line in linesRead) {
            string[] temp = line.Split(',');
            MusicNote newNote = new MusicNote((float)Convert.ToDouble(temp[0]), Convert.ToInt32(temp[1]), 0);
            noteTrack.Add(newNote);
        }

        //Load the AudioSource attached to the Conductor GameObject
        audioSource = GetComponent<AudioSource>();

        //Calculate the number of seconds in each beat
        secPerBeat = 60f / songBpm;
    }

    void Init(GameObject NoteObjectRed, Vector3 positions, MusicNote noteToInit) {

        Note musicNote = ((GameObject) Instantiate(NoteObjectRed, positions, Quaternion.identity)).GetComponent<Note>();
        musicNote.Initialize(this, noteToInit.timestamp);
    }

    void switchInit(int notePosition, int index) 
    {
        if (notePosition == 0) {
            Init(NoteToUse, new Vector3 (0.3f, 0.7f, index), noteTrack[index]);
        }
        else if (notePosition == 1) {
            Init(NoteToUse, new Vector3 (0.5f, 0.7f, index), noteTrack[index]);
        }
        else if (notePosition == 2) {
            Init(NoteToUse, new Vector3 (0.7f, 0.7f, index), noteTrack[index]);
        }
        else if (notePosition == 3) {
            Init(NoteToUse, new Vector3 (0.3f, 0.5f, index), noteTrack[index]);
        }
        else if (notePosition == 4) {
            Init(NoteToUse, new Vector3 (0.5f, 0.5f, index), noteTrack[index]);
        }
        else if (notePosition == 5) {
            Init(NoteToUse, new Vector3 (0.7f, 0.5f, index), noteTrack[index]);
        }
        else if (notePosition == 6) {
            Init(NoteToUse, new Vector3 (0.3f, 0.3f, index), noteTrack[index]);
        }
        else if (notePosition == 7) {
            Init(NoteToUse, new Vector3 (0.5f, 0.3f, index), noteTrack[index]);
        }
        else {
            Init(NoteToUse, new Vector3 (0.7f, 0.3f, index), noteTrack[index]);
        }
    }

    void StartMusic()
    {
        audioSource.time = startingTime; 

        //Record the time when the audio starts
        dspSongTime = AudioSettings.dspTime;

        startingSongPosition = ((AudioSettings.dspTime - dspSongTime) + startingTime);
        songPosition = startingSongPosition;

        timer = new Stopwatch();
        timer.Start();

        //Start the song
        audioSource.Play();
        // audioSource.PlayScheduled(startingSongPosition + 5);
        songStarted = true;
    }

    void PauseGame ()
    {
        if(gameIsPaused)
        {
            Time.timeScale = 0f;
            AudioListener.pause = true;
            timer.Stop();
        }
        else 
        {
            Time.timeScale = 1;
            AudioListener.pause = false;
            timer.Start();
        }
    }

    // // Update is called once per frame
    void Update()
    {
        if (!songStarted) 
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartMusic();
            }
            else
            {
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameIsPaused = !gameIsPaused;
            PauseGame();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
            elapsedTime = 0;
            timer.Restart();
        }

        //determine how many seconds since the song started
        // songPosition = (AudioSettings.dspTime - dspSongTime);  //what is this used for if I'm using a timer to dictate notes. Problem with syncing? or drift? lag? 

        elapsedTime = timer.Elapsed.TotalSeconds;

        // noteTrack[nextIndex].timestamp > startingSongPosition - used for testing notes further into song
        if (nextIndex < noteTrack.Count && elapsedTime >= noteTrack[nextIndex].timestamp - travelTime && noteTrack[nextIndex].timestamp > startingSongPosition)
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

            switchInit(notePosition, nextIndex);

            // UnityEngine.Debug.Log("songPosition  " + songPosition + "; timeElapsed " + elapsedTime);
            // UnityEngine.Debug.Log("init  " + noteTrack[nextIndex].timestamp); 

            nextIndex++;     
        }
        else if (nextIndex < noteTrack.Count && noteTrack[nextIndex].timestamp < startingSongPosition) {
            nextIndex++; 
        }
    }
}
