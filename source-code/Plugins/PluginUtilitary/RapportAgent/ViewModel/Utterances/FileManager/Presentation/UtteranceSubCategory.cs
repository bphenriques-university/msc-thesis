using RapportAgentPlugin.ViewModel;
using System;
using System.Collections.Generic;

namespace RapportAgentPlugin.Utterances {
    public class UtteranceSubCategory {
        public string SubCategory { get; } = "SubCategory";

        public List<UtteranceInfo> Utterances { get; set; } = new List<UtteranceInfo>();

        public UtteranceSubCategory(string subCategory) {
            this.SubCategory = subCategory;
        }

        private int currentIndex = 0;
        public UtteranceInfo GetRandomUtterance() {
            if (Utterances.Count == 0)
                return null;

            currentIndex = currentIndex + 1 % int.MaxValue;
            int selectedUtteranceIndex = currentIndex % Utterances.Count;

            //reshuffle everytime it goes around
            if (selectedUtteranceIndex == 0) {
                Shuffle();
            }

            return Utterances[selectedUtteranceIndex];
        }

        public void Shuffle() {
            Random r = new Random(Environment.TickCount);
            Utterances.Sort((x, y) => r.Next(-1, 1));
        }
    }
}
