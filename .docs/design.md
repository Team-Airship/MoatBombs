# MoatBomb Design Plan

## Overview
The MoatBomb mod introduces a specialized variant of the standard bomb designed specifically for digging canals and reshaping waterways. 

## Features
- **Canal Digging:** Functions similarly to a normal bomb by destroying blocks in a radius.
- **Water Placement:** It checks the environment's sea level. If the destroyed blocks are below the world's sea level, it automatically replaces them with water (`water-still-7`) instead of leaving empty air, instantly creating flooded canals.

## Technical Details
- The mod registers a custom block (`BlockMoatBomb`) and a custom block entity (`BlockEntityMoatBomb`).
- It hooks into the existing Vintage Story explosion logic. Immediately after the standard explosion resolves, a short delay callback (approx. 50ms) scans the affected radius and fills qualifying air blocks with water.
- The standard bomb assets (shape, blocktype, and item) have been duplicated and adapted for the MoatBomb.

## How to Craft
The MoatBomb requires standard bomb materials combined with water. It yields 8 MoatBombs.
Place the following in the crafting grid:
- **Top Row:** Water Block, Iron Oxide Powder (x8), Water Block
- **Bottom Row:** Flax Twine, Blasting Powder (x4), Linen

## Configuration & Stats
If you wish to change the stats of the MoatBomb (like blast radius or damage), you can edit the blocktype configuration JSON file located at:
`assets/moatbomb/blocktypes/moatbomb.json`

Specifically, you can modify the properties inside the `attributes` block:
```json
    "attributes": {
       "blastRadiusByType": {
              "*-t1": 5,
              "*-t2": 7,
              "*-t3": 8
           },
       "injureRadiusByType": {
              "*-t1": 7,
              "*-t2": 9,
              "*-t3": 10
           },
        "blastType": 1,
        "igniteItemByType": {
            "*-t3": "empty"
        }
    },
```

### Ignition Mechanics
By default, standard firestarters and torches can ignite the bomb. However, you can restrict or change what item is required to ignite it using the `igniteItem` or `igniteItemByType` attribute in the JSON configuration. 
- Using `"igniteItem": "empty"` will allow the player to ignite the bomb with an empty hand.
- Using a wildcard like `"igniteItem": "game:stick"` will restrict ignition specifically to items matching that code.
- Omit the attribute entirely to use the game's default firestarter rules.

## Author
Sergey
