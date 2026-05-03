local entity = {}

entity.name = "BitsHelper/AlterEgoBlockField"
entity.fillColor = {0.2, 0.4, 0.6, 0.4}
entity.borderColor = {1.0, 0.4, 1.0, 1.0}
entity.placements = {
    {
        name = "in",
        data = {
            width = 8,
            height = 8,
            blockIn = true,
            blockOut = false
        }
    },
    {
        name = "out",
        data = {
            width = 8,
            height = 8,
            blockIn = false,
            blockOut = true
        }
    }
}

return entity