using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Threading;
using System.Globalization;



public delegate void SpeechRecognizedEvent(string RecognisedText);
public delegate void AudioLevelChangedEvent(int AudioLevel);
public delegate void Message(String Text);

public class SpeechRecogEngineMS
{
    public SpeechRecogEngineMS(
        List<string> Words,
        string RecogniserCulture,
        SpeechRecognizedEvent SpeechRecognizedFunction,
        AudioLevelChangedEvent AudioLevelChangedFunction,
        Message MessageFunction)
    {
        speechRecognizedFunction = SpeechRecognizedFunction;
        audioLevelChangedFunction = AudioLevelChangedFunction;
        msgFunction = MessageFunction;
        srEngine = null;
        wordsToRecognize = Words;
        InitEngine(RecogniserCulture);
    }

    private SpeechRecognitionEngine srEngine;
    private List<string> wordsToRecognize;
    private SpeechRecognizedEvent speechRecognizedFunction;
    private AudioLevelChangedEvent audioLevelChangedFunction;
    private Message msgFunction;
    private bool on;

    public bool RecognizingON
    {
        get { return on; }
    }

    public static string[] InstalledRecogniserNames
    {
        get
        {
            ReadOnlyCollection<RecognizerInfo> ir = SpeechRecognitionEngine.InstalledRecognizers();
            string[] recognisers = new string[ir.Count];
            for (int i = 0; i < ir.Count; i++)
            {
                recognisers[i] = ir[i].Culture.Name;
            }
            return recognisers;
        }
    }

    public void InitEngine(string RecogniserCultureName)
    {
        try
        {
            Dispose();
            srEngine = createSpeechEngine(RecogniserCultureName);
            if (srEngine != null)
            {
                srEngine.AudioLevelUpdated += engine_AudioLevelUpdated;
                srEngine.SpeechRecognized += engine_SpeechRecognized;
                loadWordListToEngine(wordsToRecognize);
                SetDefaultMicrophone();
            }
        }
        catch (Exception ex)
        {
            msgFunction("Speech recognition initialization failed due to: " + ex.Message);
        }
    }

    public bool StartRecognising()
    {
        if (!on && srEngine != null && srEngine.Grammars.Count > 0)
        {
            srEngine.RecognizeAsync(RecognizeMode.Multiple);
            on = !on;
            return true;
        }
        return false;
    }

    public void StopRecognising()
    {
        if (on && srEngine != null)
        {
            srEngine.RecognizeAsyncStop();
            on = !on;
        }
    }

    public void Dispose()
    {
        if (on) StopRecognising();
        if (srEngine != null)
        {
            srEngine.Dispose();
            ClearWordList();
            srEngine = null;
        }
    }

    public void ClearWordList()
    {
        if (wordsToRecognize != null && wordsToRecognize.Count > 0) wordsToRecognize.Clear();
    }

    public void SetDefaultMicrophone()
    {
        if (srEngine != null) srEngine.SetInputToDefaultAudioDevice();
    }

    public void AddWordToExistingList(string Word)
    {
        wordsToRecognize.Add(Word);
        loadWordListToEngine(wordsToRecognize);
    }

    public void AddWordsToExistingList(List<string> Words)
    {
        wordsToRecognize.AddRange(Words);
        loadWordListToEngine(wordsToRecognize);
    }

    public void LoadNewWordList(List<string> Words)
    {
        ClearWordList();
        loadWordListToEngine(Words);
    }

    private SpeechRecognitionEngine createSpeechEngine(string preferredCulture)
    {
        ReadOnlyCollection<RecognizerInfo> installedRecs = SpeechRecognitionEngine.InstalledRecognizers();
        foreach (RecognizerInfo config in installedRecs)
        {
            if (config.Culture.Name == preferredCulture)
            {
                srEngine = new SpeechRecognitionEngine(config);
                break;
            }
        }

        if (srEngine == null)
        {
            if (installedRecs.Count > 0)
            {
                msgFunction("The desired culture is not installed on this machine, the speech-engine will continue using "
                    + installedRecs[0].Culture.Name + " as the default culture.");
                srEngine = new SpeechRecognitionEngine(installedRecs[0]);
            }
            else msgFunction("There is no installed speech recognizer on your computer!");
        }
        else if (Thread.CurrentThread.CurrentUICulture.Name != preferredCulture)
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(preferredCulture);

        return srEngine;
    }


    private void loadWordListToEngine(List<string> Words)
    {
        if (srEngine == null) return;
        try
        {
            Choices texts = new Choices(Words.ToArray());
            Grammar wordsList = new Grammar(new GrammarBuilder(texts));
            srEngine.LoadGrammar(wordsList);
        }
        catch (Exception e) { msgFunction("Adding words to engine failed: " + e.Message); }
    }

    private void engine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
    {
        speechRecognizedFunction(e.Result.Text);
    }

    private void engine_AudioLevelUpdated(object sender, AudioLevelUpdatedEventArgs e)
    {
        audioLevelChangedFunction(e.AudioLevel);
    }
}

