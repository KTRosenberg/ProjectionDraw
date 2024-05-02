# ProjectionDraw

Problem: It's challenging to draw curves in-mid air.
Solution: use objects as surfaces and points-of-reference for drawing-upon, or for transforming drawings, to make it easier to draw in 3D.

How it works: The user’s perspective determines the shape of line strokes he/she draws. Lines are
projected onto the world as they are drawn or moved. The user can spawn objects to draw-on,
and can transform (reposition, rotate) those objects to influence lines drawn on those objects

User-drawn lines are projected onto geometry in the background. A drawing appears
normal from an initial perspective, but when the user moves, it becomes clear that the drawing
has been projected along—for example—a cave wall. A user may draw along irregular surfaces
or create reference geometry (e.g. sphere, cube) onto which he/she can sketch lines. For
example, one might create a sphere, point at its equator, hold the draw button in-place, and
rotate the sphere on the y-axis to trace a circular path around the equator. Deleting the sphere
would leave a circular path. Alternatively, one could point at the center of the sphere, hold
draw, and walk around the sphere (comparable to a sculptor’s way of thinking).
Implementation: Unity engine project using HTC Vive.

### Demo Videos:

- [Overview: (drawing on walls and geometries, characters)](https://drive.google.com/file/d/1SlZv3DmJ8fT-6a9Dn864fkZs4U7Lxvtw/view?usp=sharing)
- [Drawing on a sphere](https://drive.google.com/file/d/1AL8CE8p8hNFDrqwuQxmi4FqI0o-yFUqV/view?usp=sharing)
- [Drawing on uneven walls](https://drive.google.com/file/d/19X0tO6dSCVjicT8KBEZ1wLQcZEm1ois7/view?usp=sharing)

