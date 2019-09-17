using RapportAgentPlugin.Utterances;
using System.Collections.ObjectModel;

namespace RapportAgentPlugin.ViewModel {
    public class Category {
        public UtteranceCategory Original { get; }
        public string Name { get; set; } = "Category";
        public ObservableCollection<SubCategory> SubCategories { get; } = new ObservableCollection<SubCategory>();

        public Category() {
            SubCategories.Add(new SubCategory());
        }

        public Category(UtteranceCategory category) {
            this.Original = category;
            Name = category.Category;
            foreach(var sub in category.SubCategories.Values) {
                SubCategories.Add(new SubCategory(sub));
            }
        }
    }
}
