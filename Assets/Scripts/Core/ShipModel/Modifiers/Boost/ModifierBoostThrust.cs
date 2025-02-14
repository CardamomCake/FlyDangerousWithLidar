using System.Security.Cryptography;
using Core.Player;
using UnityEngine;

namespace Core.ShipModel.Modifiers.Boost {
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(MeshRenderer))]
    public class ModifierBoostThrust : MonoBehaviour, IModifier {
        [SerializeField] private float shipForceAdd = 50000000;
        [SerializeField] private float shipSpeedAdd = 150;
        [SerializeField] private float shipThrustAdd = 50000;

        private AudioSource _boostSound;
        private MeshRenderer _meshRenderer;
        private bool _useDistortion;
        private static readonly int includeDistortion = Shader.PropertyToID("_includeDistortion");

        public bool UseDistortion {
            get => _useDistortion;
            set {
                _useDistortion = value;
                MeshRenderer.sharedMaterial.SetInt(includeDistortion, _useDistortion ? 1 : 0);
            }
        }

        private MeshRenderer MeshRenderer {
            get {
                if (_meshRenderer == null)
                    _meshRenderer = GetComponent<MeshRenderer>();
                return _meshRenderer;
            }
        }

        public void Awake() {
            _boostSound = GetComponent<AudioSource>();
        }

        public void ApplyModifierEffect(Rigidbody shipRigidBody, ref AppliedEffects effects) {
            //Gets executed every tick the ship spends inside a modifier.
            if (!_boostSound.isPlaying) _boostSound.Play();

            var parameters = shipRigidBody.gameObject.GetComponent<ShipPlayer>().ShipPhysics.FlightParameters;
            if (parameters.use_old_boost)
            {
                // old implementation
                effects.shipForce += transform.forward * shipForceAdd;
                effects.shipDeltaSpeedCap += shipSpeedAdd;
                // apply additional thrust if the ship is facing the correct direction
                if (Vector3.Dot(transform.forward, shipRigidBody.transform.forward) > 0)
                {
                    effects.shipDeltaThrust += shipThrustAdd;
                }
            }
            else
            {
                // new implementation
                // This whole mess of normalisation factors should probably be replaced with a ship parameter called speedAddFactor or something
                // for now it serves to ensure that the ships revector ability stays consistent for 
                var thrustAddFactor = ShipParameters.Defaults.maxThrust / parameters.maxThrust * ShipParameters.Defaults.thrustBoostMultiplier / parameters.thrustBoostMultiplier;
                var massNorm = parameters.mass / ShipParameters.Defaults.mass;

                effects.shipDeltaSpeedCap += shipSpeedAdd;
                if (Vector3.Dot(transform.forward, shipRigidBody.transform.forward) > 0) effects.shipDeltaThrust += shipThrustAdd * thrustAddFactor * massNorm;
            }
        }

        public void ApplyInitialEffect(Rigidbody shipRigidBody, ref AppliedEffects effects)
        {
            var parameters = shipRigidBody.gameObject.GetComponent<ShipPlayer>().ShipPhysics.FlightParameters;
            if (!parameters.use_old_boost)
            {
                float targetSpeed = 2000;  // this should be a shipParameter
                var bleed = 0.95f;
                var velPar = Mathf.Abs(Vector3.Dot(shipRigidBody.velocity, transform.forward)) * transform.forward;
                var velPerp = shipRigidBody.velocity - velPar;
                float normalisation = 28.125f * parameters.mass; // 28.125 is the time normalisation (dt sum_{n=1}^inf  (0.8^(2n)))^-1
                float speed_dampling = Mathf.Exp(-(velPar.magnitude / targetSpeed + Mathf.Pow(velPar.magnitude, 2) / (2 * 6000 * 6000)));
                effects.shipForce += normalisation * (targetSpeed * speed_dampling * transform.forward - bleed * velPerp);
            }
        }
    }
}
