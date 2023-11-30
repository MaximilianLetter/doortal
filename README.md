# Doortal

Augmented Reality (AR) application that detects door openings in order to place AR Portals.

The two proposed adaptations of the main detection pipeline are separated on two branches of this repository.
The User-Input-Adaptation is put at master branch as default choice.

# Project Description

This app uses real world doors as context for augmented reality. Target platform are Android smartphones with augmented reality supported. Achieved with Unity3D, AR Foundation and OpenCV.

Door detection coded as a C++ plugin, made available for Android using the Android NDK and used in Unity. Based on feature points and optical flow, doorways are detected and used as anchors for spawning portals. The portals are adjusted at the actual height and width of the door frame.

Visual effects inside portals are displayed as Post-Processing shaders that affect only the portal area. In addition, particles and objects are placed in the world that move with the user.

- Underwater: Shader based distortion of the image with Simplex noise. Fish are spawned and moved using boid simulation. Bubbles rise up from the ground as particle effect.
- Ice: The image is overlaid with a texture to emulate the freezing of the camera. Using simplex noise, the texture is altered to vary in distortion and transparency. Snow falls down around the user and collides with the real world floor.
- Fire: Similar to underwater, a shader distorts the image in a heatwave like pattern, ember is emitted around the user. A smoke particle effect moves in random intervals.
- Pixel: The shader pixelates the image by increasing saturation of colors and drastically reducing the resolution of the image in the portal area.
