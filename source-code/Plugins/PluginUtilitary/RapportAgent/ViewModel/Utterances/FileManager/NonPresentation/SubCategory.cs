using RapportAgentPlugin.Utterances;
using System.Collections.ObjectModel;

namespace RapportAgentPlugin.ViewModel {
    public class SubCategory {
        public UtteranceSubCategory Original { get; }
        public string Name { get; set; } = "SubCategory";
        public ObservableCollection<UtteranceInfo> Utterances { get; } = new ObservableCollection<UtteranceInfo>();

        public SubCategory() {
            Utterances.Add(new UtteranceInfo());
        }

        public SubCategory(UtteranceSubCategory subCategory) {
            this.Name = subCategory.SubCategory;
            this.Original = subCategory;

            foreach(var u in subCategory.Utterances) {
                Utterances.Add(u);
            }
        }
    }
}
