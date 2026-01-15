using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

namespace Game.Scripts.LiveObjects
{
    public class Crate : MonoBehaviour
    {
        [SerializeField] private float _punchDelay;
        [SerializeField] private GameObject _wholeCrate, _brokenCrate;
        [SerializeField] private Rigidbody[] _pieces;
        [SerializeField] private BoxCollider _crateCollider;
        [SerializeField] private InteractableZone _interactableZone;
        private bool _isReadyToBreak = false;
        // Input System Variable
        private PlayerInputAction _crateInput;

        private List<Rigidbody> _brakeOff = new List<Rigidbody>();

        // Actions
        private void Awake()
        {
            _crateInput = new PlayerInputAction();
        }

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;
            // Enable the Input System
            _crateInput.Crate.Enable();
        }

        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {
            if (_isReadyToBreak == false && _brakeOff.Count > 0)
            {
                _wholeCrate.SetActive(false);
                _brokenCrate.SetActive(true);
                _isReadyToBreak = true;
            }

            if (_isReadyToBreak && zone.GetZoneID() == 6) //Crate zone            
            {
                _crateInput.Crate.Distruction.performed += ctx =>
                {
                    if (ctx.interaction is HoldInteraction)
                    {
                            if (_brakeOff.Count > 0)
                            {
                                int toBreak = Mathf.Min(5, _brakeOff.Count);
                                for (int i = 0; i < toBreak; i++)
                                {
                                    BreakPart();
                                }
                                StartCoroutine(PunchDelay());
                            }
                            else
                            {
                                _isReadyToBreak = false;
                                _crateCollider.enabled = false;
                                _interactableZone.CompleteTask(6);
                                Debug.Log("Completely Busted");
                            }
                    }
                    else
                    {
                        if (_brakeOff.Count > 0)
                        {
                            BreakPart();
                            StartCoroutine(PunchDelay());
                        }
                        else if (_brakeOff.Count == 0)
                        {
                            _isReadyToBreak = false;
                            _crateCollider.enabled = false;
                            _interactableZone.CompleteTask(6);
                            Debug.Log("Completely Busted");
                        }
                    }
                };
            }

            //Old Code

            /*if (_isReadyToBreak == false && _brakeOff.Count >0)
            {
                _wholeCrate.SetActive(false);
                _brokenCrate.SetActive(true);
                _isReadyToBreak = true;
            }

            if (_isReadyToBreak && zone.GetZoneID() == 6) //Crate zone            
            {
                if (_brakeOff.Count > 0)
                {
                    BreakPart();
                    StartCoroutine(PunchDelay());
                }
                else if(_brakeOff.Count == 0)
                {
                    _isReadyToBreak = false;
                    _crateCollider.enabled = false;
                    _interactableZone.CompleteTask(6);
                    Debug.Log("Completely Busted");
                }
            }*/
        }

        private void Start()
        {
            _brakeOff.AddRange(_pieces);
        }

        public void BreakPart(float forceMultiplier = 1f)
        {
            int rng = Random.Range(0, _brakeOff.Count);
            _brakeOff[rng].constraints = RigidbodyConstraints.None;
            Vector3 baseForce = new Vector3(1f, 1f, 1f);
            _brakeOff[rng].AddForce(baseForce * forceMultiplier, ForceMode.Impulse); //changed from Force to Impulse
            _brakeOff.Remove(_brakeOff[rng]);            
        }

        IEnumerator PunchDelay()
        {
            float delayTimer = 0;
            while (delayTimer < _punchDelay)
            {
                yield return new WaitForEndOfFrame();
                delayTimer += Time.deltaTime;
            }

            _interactableZone.ResetAction(6);
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= InteractableZone_onZoneInteractionComplete;
            // Disable the Input System
            _crateInput.Crate.Disable();
        }
    }
}
