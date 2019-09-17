using System.Collections.Generic;
using System.ComponentModel;

namespace RapportAgentPlugin.Utterances {
    public class UtteranceCategory {
        public string Category { get; set; } = "Category";

        public Dictionary<string, UtteranceSubCategory> SubCategories = new Dictionary<string, UtteranceSubCategory>();

        public UtteranceCategory(string category) {
            this.Category = category;
        }

        public UtteranceSubCategory GetSubCategory(string subCategory) {
            return SubCategories.ContainsKey(subCategory) ? SubCategories[subCategory] : null;
        }     
    }
}
