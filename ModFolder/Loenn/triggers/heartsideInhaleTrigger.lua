local trigger = {}

trigger.name = "CNY2024Helper/HeartsideInhaleTrigger"
trigger.nodeLimits = {1, 1}
trigger.placements = {
    name = "normal",
    data = {
        strength = 400.0,
        minimumdis = 10,
        k = 10,
        flag = ""
    }
}

return trigger
