using Microsoft.VisualBasic.FileIO;
using RapportActionProposer.RCPluginDefinition;
using RapportAgentPlugin.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace RapportAgentPlugin.Utterances {

    public class UtteranceSpecification {
        public string Category { get; }
        public string SubCategory { get; }
        public string Text { get; }
        public int InitialDelay { get; }
        public int TimeOutMs { get; }
        public ushort Priority { get; }

        public UtteranceSpecification(string category, string subCategory, string text, ushort priority, int initialDelay = 0, int timeOut = 30000) {
            this.Category = category;
            this.SubCategory = subCategory;
            this.Text = text;
            this.Priority = priority;
            this.InitialDelay = initialDelay;
            this.TimeOutMs = timeOut;
        }
    }

    class CSVUtterancesReader {
        public static char Separator { get; } = ',';

        public string[] GetAvailableUtterances(string path) {
            return Directory.GetFiles(path, "*.csv");
        }

        public List<UtteranceSpecification> ReadCSV(IRCPlugin plugin, string fileName) {           
            List<UtteranceSpecification> result = new List<UtteranceSpecification>();
            using (TextFieldParser parser = new TextFieldParser(Path.ChangeExtension(fileName, ".csv"))) {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                
                //skip one line
                parser.ReadFields();

                while (!parser.EndOfData) {
                    //Process row
                    string[] data = parser.ReadFields();
                    try {
                        result.Add(new UtteranceSpecification(data[0], data[1], data[2], ushort.Parse(data[3]), int.Parse(data[4]), int.Parse(data[5])));
                    }catch(Exception e) {
                        plugin.LogFatal("Error Utterance file at line " + parser.LineNumber + ": " + e.Message);
                    }
                }
            }
            return result;
        }

        public void Save(IRCPlugin plugin, List<Category> categories, string path) {
            using (var w = new StreamWriter(path)) {
                w.WriteLine("CATEGORY,SUBCATEGORY,TEXT,PRIORITY,INITIALDELAY,TIMEOUT");
                w.Flush();

                foreach(var cat in categories) {
                    foreach(var subCat in cat.SubCategories) {
                        foreach(var u in subCat.Utterances) {
                            var line = string.Format("{0},{1},{2},{3},{4},{5}", cat.Name, subCat.Name, "\"" + u.Text + "\"", u.Priority, u.InitialDelay, u.TimeOutMs);

                            w.WriteLine(line);
                            w.Flush();
                        }
                    }
                }
            }
        }
    }
}
