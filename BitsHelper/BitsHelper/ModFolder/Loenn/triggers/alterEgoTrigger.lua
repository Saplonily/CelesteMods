local trigger = {}

trigger.name = "BitsHelper/AlterEgoTrigger"
trigger.nodeLimits = {1, 1}
trigger.placements = {
    name = "normal",
    data = {
        width = 16,
        height = 16,
        visualMode = 3
    }
}

trigger.fieldInformation = {
    visualMode = {
        options = {
            ["SameAsPlayer"] = 0,
            ["Backpack"] = 1,
            ["NoBackpack"] = 2,
            ["MadelineAsBadeline"] = 3,
        },
        editable = false,
        fieldType = "integer"
    }
}

return trigger