<!-- # Cinemachine Camera Controller Setup

The `CinemachineCameraController` works like the original `CameraController` but drives a `CinemachineCamera`. Follow these steps to switch a scene to the Cinemachine version.

## 1. Add the CinemachineCamera component
1. Select your main camera.
2. Use **Add Component** and choose **CinemachineCamera**.
3. Remove or disable the old `CameraController` component.

## 2. Create a CinemachineCameraRig
1. Create an empty GameObject named `CameraRig` and add the `CinemachineCameraRig` component.
2. Add two child GameObjects and attach `CinemachineVirtualCamera` components.
   - Configure one for the third‑person view.
   - Configure the other for the first‑person view.
3. Tweak the virtual cameras until the desired perspectives are reached.

## 3. Connect the controller
1. Add `CinemachineCameraController` to the main camera.
2. Drag the `CameraRig` object to its **cameraRig** field.
3. Adjust camera profiles or input actions as needed.

## 4. Accessing the controller
Other scripts should reference the `ICameraControl` interface when interacting with the controller.

```csharp
ICameraControl cam = FindObjectOfType<ICameraControl>();
cam?.DisableCameraControl();
```

This pattern works with both `CameraController` and `CinemachineCameraController`. -->