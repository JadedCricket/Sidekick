using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sidekick.Business.Apis.Poe.Models;
using Sidekick.Core.Initialization;

namespace Sidekick.Business.Apis.Poe.Trade.Data.Stats
{
    public class StatDataService : IStatDataService, IOnInit
    {
        private readonly IPoeTradeClient poeApiClient;

        public StatDataService(IPoeTradeClient poeApiClient)
        {
            this.poeApiClient = poeApiClient;
        }

        private List<StatData> ExplicitPatterns { get; set; }

        private List<StatData> ImplicitPatterns { get; set; }

        private List<StatData> EnchantPatterns { get; set; }

        private List<StatData> CraftedPatterns { get; set; }

        private List<StatData> VeiledPatterns { get; set; }

        private Regex NewLinePattern { get; set; }

        public async Task OnInit()
        {
            var categories = await poeApiClient.Fetch<StatDataCategory>();

            ExplicitPatterns = new List<StatData>();
            ImplicitPatterns = new List<StatData>();
            EnchantPatterns = new List<StatData>();
            CraftedPatterns = new List<StatData>();
            VeiledPatterns = new List<StatData>();

            NewLinePattern = new Regex("(?:\\\\)*[\\r\\n]+");

            var hashPattern = new Regex("\\\\#");
            var parenthesesPattern = new Regex("((?:\\\\\\ )*\\\\\\([^\\(\\)]*\\\\\\))");

            foreach (var category in categories)
            {
                var first = category.Entries.FirstOrDefault();
                if (first == null)
                {
                    continue;
                }

                // The notes in parentheses are never translated by the game.
                // We should be fine hardcoding them this way.
                string suffix, pattern;
                List<StatData> patterns;
                switch (first.Id.Split('.').First())
                {
                    default: continue;
                    case "delve":
                    case "monster":
                    case "explicit": suffix = "\\ *\\n+"; patterns = ExplicitPatterns; break;
                    case "implicit": suffix = "\\ *\\(implicit\\)"; patterns = ImplicitPatterns; break;
                    case "enchant": suffix = "\\ *\\(enchant\\)"; patterns = EnchantPatterns; break;
                    case "crafted": suffix = "\\ *\\(crafted\\)"; patterns = CraftedPatterns; break;
                    case "veiled": suffix = "\\ *\\(veiled\\)"; patterns = VeiledPatterns; break;
                }

                foreach (var entry in category.Entries)
                {
                    entry.Category = category.Label;

                    pattern = Regex.Escape(entry.Text);
                    pattern = parenthesesPattern.Replace(pattern, "(?:$1)?");
                    pattern = hashPattern.Replace(pattern, "([-+\\d,\\.]+)");
                    pattern = NewLinePattern.Replace(pattern, "\\n");

                    entry.Pattern = new Regex($"\\n+{pattern}{suffix}");
                    patterns.Add(entry);
                }
            }
        }

        public Mods ParseMods(string text)
        {
            text = NewLinePattern.Replace(text, "\n");

            var mods = new Mods();

            // Make sure the text ends with an empty line for our regexes to work correctly
            if (!text.EndsWith("\n"))
            {
                text += "\n";
            }

            FillMods(mods.Explicit, ExplicitPatterns, text);
            FillMods(mods.Implicit, ImplicitPatterns, text);
            FillMods(mods.Enchant, EnchantPatterns, text);
            FillMods(mods.Crafted, CraftedPatterns, text);
            // FillMods(mods.Veiled, VeiledPatterns, text);

            return mods;
        }

        private void FillMods(List<Mod> mods, List<StatData> patterns, string text)
        {
            var results = patterns
                .Where(x => x.Pattern.IsMatch(text))
                .ToList();

            foreach (var x in results)
            {
                var result = x.Pattern.Match(text);
                var magnitudes = new List<Magnitude>();

                if (result.Groups.Count > 1)
                {
                    for (var index = 1; index < result.Groups.Count; index++)
                    {
                        double? value = null;
                        if (double.TryParse(result.Groups[index].Value, out var parsedValue))
                        {
                            value = parsedValue;
                        }
                        magnitudes.Add(new Magnitude()
                        {
                            Hash = x.Id,
                            Max = value,
                            Min = value,
                        });
                    }
                }
                else
                {
                    magnitudes.Add(new Magnitude()
                    {
                        Hash = x.Id
                    });
                }

                mods.Add(new Mod()
                {
                    Magnitudes = magnitudes
                });
            }
        }

        public StatData GetById(string id)
        {
            if (ImplicitPatterns.Any(x => x.Id == id))
            {
                return ImplicitPatterns.First(x => x.Id == id);
            }
            if (ExplicitPatterns.Any(x => x.Id == id))
            {
                return ExplicitPatterns.First(x => x.Id == id);
            }
            if (CraftedPatterns.Any(x => x.Id == id))
            {
                return CraftedPatterns.First(x => x.Id == id);
            }
            if (EnchantPatterns.Any(x => x.Id == id))
            {
                return EnchantPatterns.First(x => x.Id == id);
            }
            return null;
        }
    }
}
