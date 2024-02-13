local entity = {}

entity.name = "CNY2024Helper/EasingBlackhole"
entity.placements =
{
    name = "normal",
    data = {
        width = 16,
        height = 16,
        duration = 1.0,
        delay = 0.0,
        rotationSpeedA = 1.0,
        rotationSpeedB = 1.0,
        scaleA = 1.0,
        scaleB = 2.0,
        flag = ""
    }
}
entity.texture = "decals/ChineseNewYear2024/StarSapphire/GDDNblackhole/asmallblackholecanrotitself00"
entity.justification = { 0.5, 0.5 }

function entity.offset(room, entity)
    return { -entity.width / 2 + 20, -entity.height / 2 + 20 }
end

return entity