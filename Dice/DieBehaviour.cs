using MysteryDice.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Dice
{
    public abstract class DieBehaviour : PhysicsProp
    {
        protected GameObject DiceModel;
        public List<IEffect> Effects = new List<IEffect>();
        public Dictionary<int, EffectType[]> RollToEffect = new Dictionary<int, EffectType[]>();

        public virtual void SetupDiceEffects()
        {
            Effects.AddRange(MysteryDice.EffectConfig.getEnabledEffects());
        }
        public virtual void SetupRollToEffectMapping()
        {
            RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(2, new EffectType[] { EffectType.Bad });
            RollToEffect.Add(3, new EffectType[] { EffectType.Bad });
            RollToEffect.Add(4, new EffectType[] { EffectType.Good });
            RollToEffect.Add(5, new EffectType[] { EffectType.Good });
            RollToEffect.Add(6, new EffectType[] { EffectType.Great });
        }

        public override void Start()
        {
            base.Start();
            DiceModel = gameObject.transform.Find("Model").gameObject;
            DiceModel.AddComponent<Spinner>();
            SetupDiceEffects();
            SetupRollToEffectMapping();
        }

        public override void PocketItem()
        {
            DiceModel.SetActive(false);
            base.PocketItem();

        }
        public override void EquipItem()
        {
            DiceModel.SetActive(true);
            base.EquipItem();
        }

        public override void OnHitGround()
        {
            base.OnHitGround();
            DiceModel.SetActive(true);
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown)
            {
                if (StartOfRound.Instance == null) return;
                if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) return;

                ulong dropperID = playerHeldBy.playerClientId;
                GameNetworkManager.Instance.localPlayerController.DiscardHeldObject(true, null, GetItemFloorPosition(DiceModel.transform.parent.position), false);
                SyncDropServerRPC(dropperID);
            }
        }

        public virtual IEnumerator UseTimer(ulong userID)
        {
            DiceModel.GetComponent<Spinner>().StartHyperSpinning(3f);

            yield return new WaitForSeconds(3f);

            Landmine.SpawnExplosion(gameObject.transform.position, true, 0, 0);
            DestroyObject();

            if (GameNetworkManager.Instance.localPlayerController.playerClientId == userID)
            {
                if (StartOfRound.Instance == null) yield break;
                if (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded) yield break;

                if (StartOfRound.Instance.currentLevel.PlanetName == "71 Gordion")
                {
                    HUDManager.Instance.DisplayTip($"Company penalty", "Do not try this again.");
                    (new Detonate()).Use();
                    yield break;
                }

                Roll();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public virtual void SyncDropServerRPC(ulong userID)
        {
            if (!IsHost)
                DropAndBlock(userID);

            SyncDropClientRPC(userID);
        }
        [ClientRpc]
        public virtual void SyncDropClientRPC(ulong userID)
        {
            DropAndBlock(userID);
        }

        public virtual void DropAndBlock(ulong userID)
        {
            grabbable = false;
            grabbableToEnemies = false;
            DiceModel.SetActive(true);
            StartCoroutine(UseTimer(userID));
        }
        public virtual void DestroyObject()
        {
            grabbable = false;
            grabbableToEnemies = false;
            deactivated = true;
            if (radarIcon != null)
            {
                GameObject.Destroy(radarIcon.gameObject);
            }
            MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                GameObject.Destroy(componentsInChildren[i]);
            }
            Collider[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<Collider>();
            for (int j = 0; j < componentsInChildren2.Length; j++)
            {
                GameObject.Destroy(componentsInChildren2[j]);
            }
        }
        public virtual void Roll()
        {
            int diceRoll = UnityEngine.Random.Range(1, 7);

            IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

            if (randomEffect == null) return;

            PlaySoundBasedOnEffect(randomEffect.Outcome);
            randomEffect.Use();

            if (randomEffect.ShowDefaultTooltip)
                ShowDefaultTooltip(randomEffect.Outcome, diceRoll);
            else
                HUDManager.Instance.DisplayTip($"Rolled {diceRoll}", randomEffect.Tooltip);
        }
        public IEffect GetRandomEffect(int diceRoll, List<IEffect> effects)
        {
            List<IEffect> rolledEffects = new List<IEffect>();

            foreach (IEffect effect in Effects)
                if (RollToEffect[diceRoll].Contains(effect.Outcome))
                    rolledEffects.Add(effect);

            if (rolledEffects.Count == 0) return null;
            int randomEffectID = UnityEngine.Random.Range(0, rolledEffects.Count);
            return rolledEffects[randomEffectID];
        }

        public void PlaySoundBasedOnEffect(EffectType effectType)
        {
            switch (effectType)
            {
                case EffectType.Awful:
                    AudioSource.PlayClipAtPoint(MysteryDice.AwfulEffectSFX, GameNetworkManager.Instance.localPlayerController.transform.position);
                    break;
                case EffectType.Bad:
                    AudioSource.PlayClipAtPoint(MysteryDice.BadEffectSFX, GameNetworkManager.Instance.localPlayerController.transform.position);
                    break;
                default:
                    AudioSource.PlayClipAtPoint(MysteryDice.GoodEffectSFX, GameNetworkManager.Instance.localPlayerController.transform.position);
                    break;
            }

        }
        public static void ShowDefaultTooltip(EffectType effectType, int diceRoll)
        {
            switch (effectType)
            {
                case EffectType.Awful:
                    HUDManager.Instance.DisplayTip($"Rolled {diceRoll}", ":)");
                    break;
                case EffectType.Bad:
                    HUDManager.Instance.DisplayTip($"Rolled {diceRoll}", "Uh oh");
                    break;
                case EffectType.Good:
                    HUDManager.Instance.DisplayTip($"Rolled {diceRoll}", "Enjoy.");
                    break;
                case EffectType.Great:
                    HUDManager.Instance.DisplayTip($"Rolled {diceRoll}", "Lucky.");
                    break;
                case EffectType.Mixed:
                    HUDManager.Instance.DisplayTip($"Rolled {diceRoll}", "Debatable");
                    break;
            }
        }
    }


}
