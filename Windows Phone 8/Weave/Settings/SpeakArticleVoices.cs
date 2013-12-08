using SelesGames.Phone;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Weave.Customizability;
using Windows.Phone.Speech.Synthesis;

namespace Weave.Settings
{
    public class SpeakArticleVoices : List<SpeakArticleVoice>
    {
        readonly SpeakArticleVoice defaultVoice;

        public SpeakArticleVoice SelectedVoice { get; set; }

        public SpeakArticleVoices()
        {
            if (DesignerProperties.IsInDesignTool)
            {
                this.AddRange(new[] 
                { 
                    new SpeakArticleVoice { DisplayName = "Mock Voice 1" },
                    new SpeakArticleVoice { DisplayName = "Mock Voice 2" },
                    new SpeakArticleVoice { DisplayName = "Mock Voice 3" },
                });
            }
            else
            {
                var voices = InstalledVoices.All
                    .GetCultureFilteredVoices(nameHint: "Mark")
                    .Select(SpeakArticleVoice.Create);

                this.AddRange(voices);
            }

            SelectedVoice = defaultVoice = this.FirstOrDefault();
        }

        public SpeakArticleVoice GetByDisplayName(string displayName)
        {
            var selected = this.FirstOrDefault(o => o.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase));
            return selected ?? defaultVoice;
        }
    }
}
