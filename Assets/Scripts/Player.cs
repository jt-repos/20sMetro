using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [Space] [SerializeField] private CameraSpring cameraSpring;

    private PlayerInputActions _inputActions;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

        playerCharacter.Initialize();
        playerCamera.Initialize(playerCharacter.GetCameraTarget());
        
        cameraSpring.Initialize();
    }

    private void OnDestroy()
    {
        _inputActions.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        // get camera input and update its rotation
        var input = _inputActions.Gameplay;
        var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };
        var deltaTime = Time.deltaTime;
        playerCamera.UpdateRotation(cameraInput);

        //get character input and update it
        var characterInput = new CharacterInput()   
        {
            Rotation = playerCamera.transform.rotation,
            Move = input.Move.ReadValue<Vector2>(),
            Jump = input.Jump.WasPressedThisFrame(),
            JumpSustain = input.Jump.IsPressed(),
            Crouch = input.Crouch.WasPressedThisFrame()
                ? CrouchInput.Toggle
                : CrouchInput.None
        };
        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody(deltaTime);
        
        #if UNITY_EDITOR
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Teleport(hit.point);
            }
        }
        #endif
    }

    private void LateUpdate()
    {
        var deltaTime = Time.deltaTime;
        var cameraTarget = playerCharacter.GetCameraTarget();
        
        playerCamera.UpdatePosition(cameraTarget);
        cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
    }

    public void Teleport(Vector3 position)
    {
        playerCharacter.SetPosition(position);
    }
}