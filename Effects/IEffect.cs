using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace MysteryDice.Effects
{
    public enum EffectType
    {
        Awful,
        Bad,
        Mixed,
        Good,
        Great
    }

    public interface IEffect
    {
        EffectType Outcome { get; }
        bool ShowDefaultTooltip { get; }
        string Tooltip { get; }
        void Use();
    }
}
