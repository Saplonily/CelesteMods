local trigger = {}

trigger.name = "BitsHelper/AlterEgoTrigger"
trigger.nodeLimits = 1
trigger.nodeLineRenderType = "line"
trigger.placements = {
    name = "normal",
    placementType = "point",
    data = {
        width = 16,
        height = 16,
        nodes = {
            { x = 16, y = 0 }
        },
    }
}

return trigger