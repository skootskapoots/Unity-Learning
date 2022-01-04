using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Inputs
{
    public class PlayerControls : IInputActionCollection, IDisposable
    {
        public InputActionAsset Asset { get; }
        public readonly string InputJsonObject = File.ReadAllText(
            Application.dataPath + "/Scripts/Inputs/Gameplay.inputactions");

        public PlayerControls()
        {
            Asset = InputActionAsset.FromJson(InputJsonObject);
            // Player
            _player = Asset.FindActionMap("Player", true);
            _playerMovement = _player.FindAction("Movement", true);
            _playerSprint = _player.FindAction("Sprint", true);
            _playerCrouch = _player.FindAction("Crouch", true);
            _playerJump = _player.FindAction("Jump", true);
            _playerLook = _player.FindAction("Look", true);
            _playerFire = _player.FindAction("Fire", true);
            _playerSwim = _player.FindAction("Swim", true);
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
        private IPlayerActionSwim _playerActionsSwimCallbackInterface;
        private readonly InputAction _playerMovement;
        private readonly InputAction _playerSprint;
        private readonly InputAction _playerCrouch;
        private readonly InputAction _playerJump;
        private readonly InputAction _playerLook;
        private readonly InputAction _playerFire;
        private readonly InputAction _playerSwim;

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
            public InputAction @Swim => _wrapper._playerSwim;

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
            
            public void SetSwimCallbacks(IPlayerActionSwim instance)
            {
                if (_wrapper._playerActionsSwimCallbackInterface != null)
                {
                    @Swim.started -= _wrapper._playerActionsSwimCallbackInterface.OnSwim;
                    @Swim.performed -= _wrapper._playerActionsSwimCallbackInterface.OnSwim;
                    @Swim.canceled -= _wrapper._playerActionsSwimCallbackInterface.OnSwim;
                }

                _wrapper._playerActionsSwimCallbackInterface = instance;
                if (instance == null) return;
                @Swim.started += instance.OnSwim;
                @Swim.performed += instance.OnSwim;
                @Swim.canceled += instance.OnSwim;
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