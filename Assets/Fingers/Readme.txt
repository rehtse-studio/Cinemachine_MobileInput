Fingers, by Jeff Johnson
Fingers (c) 2015-Present Digital Ruby, LLC
https://www.digitalruby.com/unity-plugins/

Version 3.0.3

See ChangeLog.txt for history.

Fingers is an advanced gesture recognizer system for Unity and any other platform where C# is supported (such as Xamarin). I am using this same code in a native drawing app for Android (You Doodle) and they work great.

If you've used UIGestureRecognizer on iOS, you should feel right at home using Fingers. In fact, Apple's documentation is very relevant to fingers: https://developer.apple.com/library/ios/documentation/UIKit/Reference/UIGestureRecognizer_Class/

Code Documentation
--------------------
Fingers - Gestures for Unity code documentation is available at https://unitydocs.digitalruby.com

Tutorial
--------------------
Please note that the tutorials may reference the "Updated" property of the DigitalRubyShared.GestureRecognizer class. This property has been deprecated in favor of the "StateUpdated" property, the "Updated" property will be removed in a future release.

Complete overview: https://youtu.be/97tJz0y52Fw
Image recognition, OCR and shapes: https://youtu.be/ljQkuqo1dV0. Older videos: https://youtu.be/7dvP_zhlWvU and https://youtu.be/6JgPYK38G9o.
Image recognition bulk import: https://youtu.be/ykBH84v22fc
Scroll view: https://youtu.be/MEmpdz--S3g
Joystick tutorial: https://youtu.be/_uGy6yAk83s
DPad tutorial: https://youtu.be/kra9zFDhM-8
First person controller tutorial: https://youtu.be/7T_IC3Cu1D8
Third person controller tutorial: https://youtu.be/2QCFq1rAXxE
Place object in AR/VR: https://youtu.be/pJ1Z1hdvne8

Please be sure to check out each demo scene as well (Fingers/Demo/DemoScene*).

I'm available for email support and questions to - support@digitalruby.com.

Instructions
--------------------
To get started, perform the following:
- In your scripts you can the simply refer to FingersScript.Instance whenever you need to add or remove gestures.
- You are welcome to drag the FingersScriptPrefab object into your scene as well if you want, but this is not necessary.
- Add "using DigitalRubyShared;" to the top of your scripts to include the gestures and framework.
- Create some gestures. There are many example scripts and scenes for you to refer to. The component menu allows adding gestures directly in the editor.
- Add colliders to elements that you want to receive touches. Make sure your camera has physics raycasters as well.

For new Unity Input System package:
- Define UNITY_INPUT_SYSTEM_V2 in scripting defines in player settings.
- If you are supporting multiple touch screens/devices, you can set the DeviceId property on your gesture recognizer to limit the gesture to a specific device, or set to 0 for all devices.
- Make sure you have imported the new input system package and enabled the new input backends (requires Unity restart).

Fingers script has these properties:
- Treat mouse pointer/wheel as finger (default is true, useful for testing in the player for some gestures). Disable this if you are using Unity Remote or are running on a touch screen like Surface Pro.
- Simulate mouse with touch - whether to send mouse events for touches. Default is false. You don't need this unless you have legacy code relying on mouse events.
- RequireControlKeyForMouseZoom - whether the control key is required to scale with mouse wheel.
- Pass through objects. Any object in this list will always allow the gesture to execute and will not block it.
- Show touches. Default is false. Set to true to debug touches. Requires using the prefab.
- Touch circles. For debug, if showing touches. Turn off before releasing your game or app. Requires using the prefab.
- Default DPI. In the event that Unity can't figure out the DPI of the device, use this default value.
- Clear gestures on level load. Default is true. This clears out all gestures when a new scene is loaded.

Requirements for happy gestures in Unity scripts
--------------------
- Script fields: Create your gesture objects in fields of your script, i.e. private readonly PanGestureRecognizer panGesture = new PanGestureRecognizer();
- Wrap any private gesture recognizers that need to be accessed by other scripts with a public get only property, i.e. public PanGestureRecognizer PanGesture { get { return panGesture; } }
- OnEnable: Setup your gestures and add gestures using FingersScript.Instance.AddGesture in OnEnable. Add masks using FingersScript.Instance.AddMask.
- OnDisable: Remove gestures using FingersScript.Instance.RemoveGesture, remove masks using FingersScript.Instance.RemoveMask. Wrap both inside an if (FingersScript.HasInstance) { /* remove gestures, remove masks */ }.

Input Lag
--------------------
Unity input will likely lag on all devices with default settings. To get maximum input performance, do the following:
- Turn on UseFixedUpdate on the fingers script.
- Set project settings -> time -> fixed update rate to a low value (like 0.005) or whatever rate your mouse or touch device polls at.

If you do modify these settings, please profile and make sure your performance is acceptable.

Creating and Removing Gestures
--------------------
Always create new gestures in OnEnable. Remove them in OnDisable. The demo scripts all show this pattern in action. OnDisable should check if FingersScript.HasInstance is true before removing gestures.

This ensures that there are no memory leaks or bad behavior with Unity game objects / null reference exceptions, etc.

Masking Gestures
--------------------
Gestures can be restricted to masks using collider2d trigger object that must be in a canvas. You can have as many masks as you like.

Use FingersScript.Instance.AddMask and FingersScript.Instance.RemoveMask to add and remove masks. Component gestures also have a Collider2D mask field.

DemoSceneJoystickMaskArea shows an example.

Event System
--------------------
The gestures can work with the Unity event system. Gestures over certain UI elements in a Canvas will be blocked, such as Button, Dropdown, etc. Text elements are always ignored and never block the gesture.

You can add physics raycasters to allow objects not on the Unity UI to also be part of the gesture pass through system. This requires an event system. Collider and Collider2D components will not block gestures unless the PlatformSpecificView property on the gesture is not null and does not match the game object with the collider.

Any object in the pass through list of FingersScript will always pass the gesture through and allow it to execute.

Options for allowing gestures on UI elements:
- You can set the PlatformSpecificView on your gesture that is the game object that you want to allow gestures on. If the gesture then executes over this game object, the gesture is always allowed. See DemoScriptPlatformSpecificView.cs.
- You can populate the PassThroughObjects property of FingersScript. Any game object in this list will always pass the gesture through.
- You can use the CaptureGestureHandler callback on the fingers script to run custom logic to determine whether the gesture can pass through a UI element. See DemoScript.cs, CaptureGestureHandler function.
- You can use the ComponentTypesToDenyPassThrough and ComponentTypesToIgnorePassThrough properties of FingersScript to customize pass through behavior by adding additional component types.

See the DemoScript.cs file for more details and examples.

Canvas Coordinates
-------------------
If you need to convert from screen space to canvas space, use this Unity method:

// convert from screen space to the coordinate system of a canvas object
GameObject canvasObject = canvas; // can be a 'canvas', 'image', 'button' or another canvas ui element.
Vector2 canvasPoint;
RectTransform contentRectTransform = canvasObject.GetComponent<RectTransform>();
RectTransformUtility.ScreenPointToLocalPointInRectangle(contentRectTransform, new Vector2(gesture.FocusX, gesture.FocusY), null, out canvasPoint);
// canvasPoint is in the coordinate system of canvasObject now.

Standard Gestures:
--------------------
Once you've added the script, you will need to add some gestures. You can do this through the component menu in the editor or your own custom script. Remember to add "using DigitalRubyShared;" to your scripts.

Each gesture has public properties that can configure things such as thresholds for movement, rotation, etc. The defaults should work well for most cases. Fingers works in inches by default since most devices use DPI.

Please review the Start method of DemoScript.cs to see how gestures are created and added to the finger script. Also watch the tutorial video (links at top of this file) if you get lost, it will be very helpful.

Other demo scenes show how to use additional helper scripts, such as DemoScenePanScaleRotate and DemoSceneDragDrop.

Image and Shape Gestures:
--------------------
Custom shapes are possible with ShapeGestureRecognizer. This uses a fuzzy image recognition algorithm, and will require you to train it to match variants of your shape. Shapes are defined as a grid of pixels up to 64 pixels in size. Performance is excellent as rows are compared by a simple ulong bitmask comparison.

Fingers/Prefab/Scenes/FingersImageAutomationScene is a great way to rapidly create the code for your shapes. Run the scene. As you draw each gesture, the code gets put into the text box. Click the X in the bottom right to remove the last line if you made a mistake. Up to about 50 lines can go in the text box before Unity starts throwing errors, so copy the code out every so often and clear out the text box.

DemoSceneImage is a great start scene for your game. You can enter your image details in the script in the inspector and receive callbacks for when points are being drawn, along with matched images. Just add shapes in the inspector and paste in the scripts you get while running FingersImageAutomationScene as you train your image gesture.

To learn more about creating custom shape gestures and how to test them and refine them, please watch the tutorial video at https://youtu.be/ljQkuqo1dV0

One Finger Gestures:
--------------------
Scaling and Rotation one finger gestures are available. Please see DemoSceneOneFinger for more details.

Joystick:
--------------------
FingersJoystickScript is a great way to create a joystick. Please see DemoSceneJoystick and DemoScriptJoystick for examples. The joystick features distance limiting and power (moves further as joystick moves away from center), along with a follow speed to have the joystick follow the touch if it moves beyond the joystick bounds.

The joystick now has a prefab! It must be placed under a Canvas. The joystick rect transform must have a pivot of 0.5,0.5 and this is enforced in OnEnable in the joystick script.

DPad:
--------------------
FingersDPadScript allows use of a DPad. You can swap out the images and change the colliders. Please watch the DPad tutorial and scene to see how this works in full.

The DPad now has a prefab! It must be placed under a Canvas.

Controllers
--------------------
First person and third person controller prefabs are now part of the asset. Please watch the tutorial videos to learn how they work.

First person controller tutorial: https://youtu.be/7T_IC3Cu1D8
Third person controller tutorial: https://youtu.be/2QCFq1rAXxE

VR / AR / Virtual touches
--------------------
Please see DemosceneVirtualTouches and DemoScriptVirtualTouches for examples on how to do AR / VR or other virtual touches.

Demos:
--------------------
I've made many demo scenes. Please check them out as they are great for seeing everything Fingers - Gestures for Unity can do.

Multiple Gestures with One Touch
--------------------
See DemoSceneSwipe / DemoScriptSwipe.cs for an example of how to make a gesture that can restart without lifting the finger or mouse.

OnMouse* events and scripts
--------------------
Fingers Gestures does not support OnMouse* script events on platforms without a mouse. This is due to the massive amounts of bugs and complexity of having a mouse pointer simulated on these platforms. If you have OnMouse* methods, you will need to replace with actual gestures.

Misc:
--------------------
*Note* I don't use anonymous / inline delegates in the demo script as these seem to crash on iOS.

Troubleshooting / FAQ:
--------------------
Q: My gestures aren't working.
A: Did you add a physics and/or physics2d ray caster to your camera? What about pass through objects, have you set those up properly? Did you add collider or collider2d to your game object?

Q: Simultaneous gestures are not working.
A: Ensure you call the allow simultaneous method on one of the gestures. Also consider trying ClearTrackedTouchesOnEndOrFail = true.

Q: My gesture is always failing.
A: Most likely you need to set the platform specific view on the gesture. You can set this to the game object of the canvas, or another game object with the UI element or game object collider you want the gesture to execute on.
A: In Player Settings > Other Settings > Turn on Prebake Collision Meshes. Turn off Strip Engine Code.

Q: Help / I'm lost!?!?
A: I'm available to answer your questions or feedback at support@digitalruby.com

Thank you.

- Jeff Johnson, creator of Fingers - Gestures for Unity
http://www.digitalruby.com/Unity-Plugins

