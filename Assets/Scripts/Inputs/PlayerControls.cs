using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Inputs
{
    public class PlayerControls : IInputActionCollection, IDisposable
    {
        public InputActionAsset Asset { get; }

        public PlayerControls()
        {
            Asset = InputActionAsset.FromJson(@"{
    ""name"": ""Gameplay"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""3c2fd68b-4fae-45af-9809-99248a83f47f"",
            ""actions"": [
                {
                    ""name"": ""Movement"",
                    ""type"": ""Value"",
                    ""id"": ""4804fb33-dc29-4837-b566-4aac5901002f"",
                    ""expectedControlType"": ""Vector3"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Look"",
                    ""type"": ""Value"",
                    ""id"": ""1986078e-e768-4519-908a-2b40d3ea9a41"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Fire"",
                    ""type"": ""Button"",
                    ""id"": ""f22ab6b1-7d71-4a68-a097-399f2bf9514d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Sprint"",
                    ""type"": ""Button"",
                    ""id"": ""dcf43f88-2c62-443f-8919-0d6c4dffbe09"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""f6bac4cb-cd80-4029-98f5-fc86554178ac"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Crouch"",
                    ""type"": ""Button"",
                    ""id"": ""925a6225-4cdc-444b-b3ed-bf58c2e35769"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""978bfe49-cc26-4a3d-ab7b-7d7a29327403"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""WASD"",
                    ""id"": ""00ca640b-d935-4593-8157-c05846ea39b3"",
                    ""path"": ""Dpad"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""e2062cb9-1b15-46a2-838c-2f8d72a0bdd9"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""up"",
                    ""id"": ""8180e8bd-4097-4f4e-ab88-4523101a6ce9"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""320bffee-a40b-4347-ac70-c210eb8bc73a"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""1c5327b5-f71c-4f60-99c7-4e737386f1d1"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""d2581a9b-1d11-4566-b27d-b92aff5fabbc"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""2e46982e-44cc-431b-9f0b-c11910bf467a"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""fcfe95b8-67b9-4526-84b5-5d0bc98d6400"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""77bff152-3580-4b21-b6de-dcd0c7e41164"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""c1f7a91b-d0fd-4a62-997e-7fb9b69bf235"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8c8e490b-c610-4785-884f-f04217b23ca4"",
                    ""path"": ""<Pointer>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""143bb1cd-cc10-4eca-a2f0-a3664166fe91"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Gamepad"",
                    ""action"": ""Fire"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""05f6913d-c316-48b2-a6bb-e225f14c7960"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Fire"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b3661e52-1545-4f23-83b6-b7d28dc1baae"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8226165b-3ad5-4af0-9f68-34f5348029ae"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""70d9212b-417b-40a3-9dcf-299394cb5287"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ba62589a-b966-442b-b59c-7a0b2a0fe043"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4cad0cb3-5c8f-4917-a2de-867cbb47f6df"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Crouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""49b1f8ee-d789-490a-a05a-bb7b8b3e1512"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Crouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard&Mouse"",
            ""bindingGroup"": ""Keyboard&Mouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Gamepad"",
            ""bindingGroup"": ""Gamepad"",
            ""devices"": [
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
            // Player
            _player = Asset.FindActionMap("Player", true);
            _playerMovement = _player.FindAction("Movement", true);
            _playerSprint = _player.FindAction("Sprint", true);
            _playerCrouch = _player.FindAction("Crouch", true);
            _playerJump = _player.FindAction("Jump", true);
            _playerLook = _player.FindAction("Look", true);
            _playerFire = _player.FindAction("Fire", true);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(Asset);
        }

        public InputBinding? bindingMask
        {
            get => Asset.bindingMask;
            set => Asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => Asset.devices;
            set => Asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => Asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return Asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return Asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            Asset.Enable();
        }

        public void Disable()
        {
            Asset.Disable();
        }

        // Player
        private readonly InputActionMap _player;
        private IPlayerActionMovement _playerActionsMovementCallbackInterface;
        private IPlayerActionSprint _playerActionsSprintCallbackInterface;
        private IPlayerActionCrouch _playerActionsCrouchCallbackInterface;
        private IPlayerActionJump _playerActionsJumpCallbackInterface;
        private IPlayerActionLook _playerActionsLookCallbackInterface;
        private IPlayerActionFire _playerActionsFireCallbackInterface;
        private readonly InputAction _playerMovement;
        private readonly InputAction _playerSprint;
        private readonly InputAction _playerCrouch;
        private readonly InputAction _playerJump;
        private readonly InputAction _playerLook;
        private readonly InputAction _playerFire;

        public struct PlayerActions
        {
            private PlayerControls _wrapper;

            public PlayerActions(PlayerControls wrapper)
            {
                _wrapper = wrapper;
            }

            public InputAction @Movement => _wrapper._playerMovement;
            public InputAction @Sprint => _wrapper._playerSprint;
            public InputAction @Crouch => _wrapper._playerCrouch;
            public InputAction @Jump => _wrapper._playerJump;
            public InputAction @Look => _wrapper._playerLook;
            public InputAction @Fire => _wrapper._playerFire;

            public InputActionMap Get()
            {
                return _wrapper._player;
            }

            public void Enable()
            {
                Get().Enable();
            }

            public void Disable()
            {
                Get().Disable();
            }

            public bool Enabled => Get().enabled;

            public static implicit operator InputActionMap(PlayerActions set)
            {
                return set.Get();
            }

            public void SetMovementCallbacks(IPlayerActionMovement instance)
            {
                if (_wrapper._playerActionsMovementCallbackInterface != null)
                {
                    @Movement.started -= _wrapper._playerActionsMovementCallbackInterface.OnMovement;
                    @Movement.performed -= _wrapper._playerActionsMovementCallbackInterface.OnMovement;
                    @Movement.canceled -= _wrapper._playerActionsMovementCallbackInterface.OnMovement;
                }

                _wrapper._playerActionsMovementCallbackInterface = instance;
                if (instance == null) return;
                @Movement.started += instance.OnMovement;
                @Movement.performed += instance.OnMovement;
                @Movement.canceled += instance.OnMovement;
            }
            
            public void SetSprintCallbacks(IPlayerActionSprint instance)
            {
                if (_wrapper._playerActionsSprintCallbackInterface != null)
                {
                    @Sprint.started -= _wrapper._playerActionsSprintCallbackInterface.OnSprint;
                    @Sprint.performed -= _wrapper._playerActionsSprintCallbackInterface.OnSprint;
                    @Sprint.canceled -= _wrapper._playerActionsSprintCallbackInterface.OnSprint;
                }

                _wrapper._playerActionsSprintCallbackInterface = instance;
                if (instance == null) return;
                @Sprint.started += instance.OnSprint;
                @Sprint.performed += instance.OnSprint;
                @Sprint.canceled += instance.OnSprint;
            }
            
            public void SetCrouchCallbacks(IPlayerActionCrouch instance)
            {
                if (_wrapper._playerActionsCrouchCallbackInterface != null)
                {
                    @Crouch.started -= _wrapper._playerActionsCrouchCallbackInterface.OnCrouch;
                    @Crouch.performed -= _wrapper._playerActionsCrouchCallbackInterface.OnCrouch;
                    @Crouch.canceled -= _wrapper._playerActionsCrouchCallbackInterface.OnCrouch;
                }

                _wrapper._playerActionsCrouchCallbackInterface = instance;
                if (instance == null) return;
                @Crouch.started += instance.OnCrouch;
                @Crouch.performed += instance.OnCrouch;
                @Crouch.canceled += instance.OnCrouch;
            }
            
            public void SetJumpCallbacks(IPlayerActionJump instance)
            {
                if (_wrapper._playerActionsJumpCallbackInterface != null)
                {
                    @Jump.started -= _wrapper._playerActionsJumpCallbackInterface.OnJump;
                    @Jump.performed -= _wrapper._playerActionsJumpCallbackInterface.OnJump;
                    @Jump.canceled -= _wrapper._playerActionsJumpCallbackInterface.OnJump;
                }

                _wrapper._playerActionsJumpCallbackInterface = instance;
                if (instance == null) return;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
            }
        
            public void SetLookCallbacks(IPlayerActionLook instance)
            {
                if (_wrapper._playerActionsLookCallbackInterface != null)
                {
                    @Look.started -= _wrapper._playerActionsLookCallbackInterface.OnLook;
                    @Look.performed -= _wrapper._playerActionsLookCallbackInterface.OnLook;
                    @Look.canceled -= _wrapper._playerActionsLookCallbackInterface.OnLook;
                }

                _wrapper._playerActionsLookCallbackInterface = instance;
                if (instance == null) return;
                @Look.started += instance.OnLook;
                @Look.performed += instance.OnLook;
                @Look.canceled += instance.OnLook;
            }
        
            public void SetFireCallbacks(IPlayerActionFire instance)
            {
                if (_wrapper._playerActionsFireCallbackInterface != null)
                {
                    @Fire.started -= _wrapper._playerActionsFireCallbackInterface.OnFire;
                    @Fire.performed -= _wrapper._playerActionsFireCallbackInterface.OnFire;
                    @Fire.canceled -= _wrapper._playerActionsFireCallbackInterface.OnFire;
                }

                _wrapper._playerActionsFireCallbackInterface = instance;
                if (instance == null) return;
                @Fire.started += instance.OnFire;
                @Fire.performed += instance.OnFire;
                @Fire.canceled += instance.OnFire;
            }
        }

        public PlayerActions @Player => new PlayerActions(this);
        private int _keyboardMouseSchemeIndex = -1;

        public InputControlScheme KeyboardMouseScheme
        {
            get
            {
                if (_keyboardMouseSchemeIndex == -1)
                    _keyboardMouseSchemeIndex = Asset.FindControlSchemeIndex("Keyboard&Mouse");
                return Asset.controlSchemes[_keyboardMouseSchemeIndex];
            }
        }

        private int _gamepadSchemeIndex = -1;

        public InputControlScheme GamepadScheme
        {
            get
            {
                if (_gamepadSchemeIndex == -1) _gamepadSchemeIndex = Asset.FindControlSchemeIndex("Gamepad");
                return Asset.controlSchemes[_gamepadSchemeIndex];
            }
        }
    }
}