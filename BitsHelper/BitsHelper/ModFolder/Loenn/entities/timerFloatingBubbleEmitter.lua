local entity = {}

entity.name = "BitsHelper/TimerFloatingBubbleEmitter"
entity.texture = "objects/BitsHelper/bubbleEmitter/idle00"
entity.placements = {
    name = "normal",
    data = {
        interval = 1.0,
        attach = false,
        initialTimer = 1.0
    }
}

return entity