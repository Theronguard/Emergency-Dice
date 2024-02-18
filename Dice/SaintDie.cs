using KaimiraGames;
using MysteryDice.Effects;
using MysteryDice.Visual;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Dice
{
    public class SaintDie : DieBehaviour
    {
        public override void Start()
        {
            base.Start();
            DiceModel.transform.Find("halo").gameObject.AddComponent<HaloSpin>();
        }
        public override void SetupRollToEffectMapping()
        {
            RollToEffect.Add(1, new EffectType[] { });
            RollToEffect.Add(2, new EffectType[] { EffectType.Good });
            RollToEffect.Add(3, new EffectType[] { EffectType.Good });
            RollToEffect.Add(4, new EffectType[] { EffectType.Good });
            RollToEffect.Add(5, new EffectType[] { EffectType.Great });
            RollToEffect.Add(6, new EffectType[] { EffectType.Great });
        }

        public override void Roll()
        {
            int diceRoll = UnityEngine.Random.Range(1,7);

            IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

            PlaySoundBasedOnEffect(randomEffect.Outcome);
            randomEffect.Use();
            Networker.Instance.LogEffectsToOwnerServerRPC(PlayerUser.playerUsername, randomEffect.Name);

            if (diceRoll == 1)
                HUDManager.Instance.DisplayTip($"Rolled 1", "Nothing happened");
            else
                HUDManager.Instance.DisplayTip($"Rolled {diceRoll}", randomEffect.Tooltip);
        }
    }
}
