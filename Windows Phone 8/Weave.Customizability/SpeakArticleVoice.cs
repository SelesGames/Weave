using Windows.Phone.Speech.Synthesis;

namespace Weave.Customizability
{
    public class SpeakArticleVoice
    {
        public string DisplayName { get; set; }
        public VoiceInformation Voice { get; set; }

        public static SpeakArticleVoice Create(VoiceInformation voice)
        {
            var displayName = voice.DisplayName.Replace("Microsoft", "").Replace("Mobile", "").Trim();
            return new SpeakArticleVoice { DisplayName = displayName, Voice = voice };
        }
    }
}