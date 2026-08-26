local entity = {}

entity.name = "BitsHelper/StaticBubble"
entity.texture = "objects/BitsHelper/bubble/idle00"
entity.justification = { 0.5, 0.5 }
entity.placements = {
    name = "normal",
    data = {
        oneUse = false,
        respawnTime = 2.5
    }
}

return entity