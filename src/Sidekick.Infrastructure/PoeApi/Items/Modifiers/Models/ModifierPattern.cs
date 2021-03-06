using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sidekick.Domain.Game.Modifiers.Models;

namespace Sidekick.Infrastructure.PoeApi.Items.Modifiers.Models
{
    public class ModifierPattern
    {
        public ModifierMetadata Metadata { get; set; }

        public Regex Pattern { get; set; }

        public Regex NegativePattern { get; set; }

        public Regex AdditionalProjectilePattern { get; set; }

        public List<ModifierOptionParse> Options { get; set; }
    }
}
