// GENERATED AUTOMATICALLY FROM 'Assets/InputSys/PlayerControl.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerControl : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerControl()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControl"",
    ""maps"": [
        {
            ""name"": ""PlayerControlMaps"",
            ""id"": ""d7b185d5-41fc-4248-baea-ef7f2273a541"",
            ""actions"": [
                {
                    ""name"": ""MoveActions"",
                    ""type"": ""Value"",
                    ""id"": ""cf8d179a-5f5e-41b0-a8de-1e5b154281c5"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Shoot"",
                    ""type"": ""Button"",
                    ""id"": ""66226944-fa27-42d9-b8bf-49dba359e609"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""wasd"",
                    ""id"": ""e5d3fd7b-8be6-4f55-a9a0-89d996c68366"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveActions"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""00399037-62ed-414e-82fb-36b99da56f98"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveActions"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""c51d8adc-6d13-46f9-a19b-6ebfb2436c22"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveActions"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""9a991594-96ef-48b5-9f75-531be1621202"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveActions"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""b02a45a4-2c04-4a13-a7da-37ed6433ff81"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveActions"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""6ca281f7-9410-4407-a4ae-5660bdcfc0ba"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveActions"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ca5bc957-7e64-4846-9f4a-f966817fd3a3"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Shoot"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""df944ac3-7a5b-4a98-aff5-8f298a02d67b"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Shoot"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // PlayerControlMaps
        m_PlayerControlMaps = asset.FindActionMap("PlayerControlMaps", throwIfNotFound: true);
        m_PlayerControlMaps_MoveActions = m_PlayerControlMaps.FindAction("MoveActions", throwIfNotFound: true);
        m_PlayerControlMaps_Shoot = m_PlayerControlMaps.FindAction("Shoot", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // PlayerControlMaps
    private readonly InputActionMap m_PlayerControlMaps;
    private IPlayerControlMapsActions m_PlayerControlMapsActionsCallbackInterface;
    private readonly InputAction m_PlayerControlMaps_MoveActions;
    private readonly InputAction m_PlayerControlMaps_Shoot;
    public struct PlayerControlMapsActions
    {
        private @PlayerControl m_Wrapper;
        public PlayerControlMapsActions(@PlayerControl wrapper) { m_Wrapper = wrapper; }
        public InputAction @MoveActions => m_Wrapper.m_PlayerControlMaps_MoveActions;
        public InputAction @Shoot => m_Wrapper.m_PlayerControlMaps_Shoot;
        public InputActionMap Get() { return m_Wrapper.m_PlayerControlMaps; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerControlMapsActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerControlMapsActions instance)
        {
            if (m_Wrapper.m_PlayerControlMapsActionsCallbackInterface != null)
            {
                @MoveActions.started -= m_Wrapper.m_PlayerControlMapsActionsCallbackInterface.OnMoveActions;
                @MoveActions.performed -= m_Wrapper.m_PlayerControlMapsActionsCallbackInterface.OnMoveActions;
                @MoveActions.canceled -= m_Wrapper.m_PlayerControlMapsActionsCallbackInterface.OnMoveActions;
                @Shoot.started -= m_Wrapper.m_PlayerControlMapsActionsCallbackInterface.OnShoot;
                @Shoot.performed -= m_Wrapper.m_PlayerControlMapsActionsCallbackInterface.OnShoot;
                @Shoot.canceled -= m_Wrapper.m_PlayerControlMapsActionsCallbackInterface.OnShoot;
            }
            m_Wrapper.m_PlayerControlMapsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @MoveActions.started += instance.OnMoveActions;
                @MoveActions.performed += instance.OnMoveActions;
                @MoveActions.canceled += instance.OnMoveActions;
                @Shoot.started += instance.OnShoot;
                @Shoot.performed += instance.OnShoot;
                @Shoot.canceled += instance.OnShoot;
            }
        }
    }
    public PlayerControlMapsActions @PlayerControlMaps => new PlayerControlMapsActions(this);
    public interface IPlayerControlMapsActions
    {
        void OnMoveActions(InputAction.CallbackContext context);
        void OnShoot(InputAction.CallbackContext context);
    }
}
