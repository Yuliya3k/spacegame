# CinemachineCameraController

This component mirrors the existing `CameraController` but drives a `CinemachineCamera`.  It exposes the same camera profile fields and input actions so existing options continue to work.  The controller also implements `ICameraControl` which allows `InputFreezeManager` to disable or enable camera input at runtime.

## Switching Views
Attach a `CinemachineCameraRig` and assign it to the controller's **cameraRig** field.  Calling `ToggleView`, `SetFirstPerson` or `SetThirdPerson` will forward the request to the rig and switch between the configured first‑ and third‑person virtual cameras.

## Accessing the Controller
Other scripts should depend on the `ICameraControl` interface when interacting with a camera controller.  For example:

```csharp
ICameraControl cam = FindObjectOfType<ICameraControl>();
cam?.DisableCameraControl();
```

This works for both `CameraController` and `CinemachineCameraController`.