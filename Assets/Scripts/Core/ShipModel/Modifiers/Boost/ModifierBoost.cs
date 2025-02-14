using Core.ShipModel.Modifiers.Water;
using NaughtyAttributes;
using UnityEngine;

namespace Core.ShipModel.Modifiers.Boost {
    // Controller of boost modifier object, used to set the various params and serialise etc
    [ExecuteAlways]
    public class ModifierBoost : MonoBehaviour {
        [SerializeField] private ModifierBoostAttractor modifierBoostAttractor;
        [SerializeField] private ModifierBoostThrust modifierBoostThrust;
        [SerializeField] private ModifierBoostStream modifierBoostStream;

        [Range(1000, 50000)] [OnValueChanged("BoostLengthChanged")] [SerializeField]
        private float boostStreamLengthMeters;

        public float BoostStreamLengthMeters {
            get => boostStreamLengthMeters;
            set {
                boostStreamLengthMeters = value;
                BoostLengthChanged();
            }
        }

        private void Awake() {
            boostStreamLengthMeters = modifierBoostStream.TrailLengthMeters;
        }

        private void OnEnable() {
            modifierBoostThrust.UseDistortion = FindObjectOfType<ModifierWater>() == null;
        }

        private void BoostLengthChanged() {
            modifierBoostStream.TrailLengthMeters = boostStreamLengthMeters;
        }
    }
}