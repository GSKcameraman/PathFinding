# PathFinding

Made by Haoyang Liu

This is a project of making a A* alogrithm with  text map files.

## Map Processing
All the maps are auto-processed.
### Tiles
For the tiles, the prjects uses a set 2 x 2 tile for the map. The weight of the tile depends on how "empty" the tile is, i.e. the more the traversable tiles, small the weight of the tile
### Way Point System
The waypoints are also auto-processed. It uses the original tile system to detect corners and mark all corners as waypoints.
This algorithm works best with parallel maps, and with map have line goes diagonal, it would likely broke and mark every dot along the line.
And it takes long to mark, and even longer to connect the waypoints (O(n<sup>2</sup>)).

[Here is a link to the video demostration](https://youtu.be/T7ux7Xb4xzA)
