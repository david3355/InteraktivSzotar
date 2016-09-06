using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech;
using System.Speech.Synthesis;

namespace IntelligensSzotar
{
    class TextToSpeech
    {
        public TextToSpeech()
        {
            ttsEngine = new SpeechSynthesizer();
        }

        private SpeechSynthesizer ttsEngine;

        public void Speak(string Text)
        {
            ttsEngine.SpeakAsync(Text);
        }

        public void SetVolume(int Volume)
        {
            ttsEngine.Volume = Volume;
        }

        public void SetSpeechVoice(VoiceInfo Voice)
        {
            //ttsEngine.SelectVoice(
        }
    }
}
